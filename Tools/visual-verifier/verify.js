const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

(async () => {
  const base = process.env.BASE_URL || 'http://localhost:5184';
  const outDir = path.resolve(__dirname, 'screenshots');
  if (!fs.existsSync(outDir)) fs.mkdirSync(outDir, { recursive: true });

  // Pages to capture (discover from root nav, fallback to these)
  const fallbackRoutes = [
    '/',
    '/Home/Dashboard',
    '/Bookings',
    '/PhotoShoots',
    '/Galleries',
    '/FileManager',
    '/Photos',
    '/Invoices'
  ];

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1280, height: 800 } });
  const page = await context.newPage();

  // First navigate root and discover side-nav links to ensure we hit the real routes
  const discovered = new Set();
  try {
    await page.goto(base, { waitUntil: 'networkidle', timeout: 30000 });
    const links = await page.$$eval('a.side-nav-link', els => els.map(a => a.getAttribute('href')).filter(Boolean));
    links.forEach(h => {
      try {
        const url = new URL(h, base).pathname;
        discovered.add(url);
      } catch (e) {
        // ignore malformed hrefs
      }
    });
  } catch (err) {
    console.warn('Failed to discover nav links on root:', err.message);
  }

  const routes = Array.from(discovered).concat(fallbackRoutes.filter(r => !discovered.has(r)));

  for (const route of routes) {
    const url = new URL(route, base).toString();
    const safeName = route === '/' ? 'root' : route.replace(/[^a-z0-9]/gi, '_').replace(/^_+|_+$/g, '');
    const filePath = path.join(outDir, `${safeName}.png`);

    try {
      console.log('Navigating to', url);
      const resp = await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });
      if (!resp || !resp.ok()) {
        const status = resp ? resp.status() : 'no response';
        console.warn(`Non-OK response for ${url}: ${status}`);
        try {
          const body = resp ? await resp.text() : '';
          const htmlPath = path.join(outDir, `${safeName}.html`);
          fs.writeFileSync(htmlPath, body || `Status: ${status}`);
          console.log('Saved response body to', htmlPath);
        } catch (err) {
          console.warn('Failed to save response body for', url, err.message);
        }
      }

      // Wait a short time for client-side widgets to initialize
      await page.waitForTimeout(750);

      // Remove any debugging overlays that might interfere (optional)
      await page.evaluate(() => {
        const els = document.querySelectorAll('.devtools, .debug-overlay');
        els.forEach(e => e.remove());
      }).catch(() => {});

      await page.screenshot({ path: filePath, fullPage: true });
      console.log('Saved', filePath);
    } catch (err) {
      console.error('Failed to capture', url, err.message);
    }
  }

  // Attempt to login (useful for capturing authenticated pages)
  const adminEmail = process.env.ADMIN_EMAIL || 'admin@myphoto.biz';
  const adminPassword = process.env.ADMIN_PASSWORD || 'Admin@123456';
  try {
    const loginUrl = new URL('/Identity/Account/Login', base).toString();
    console.log('Attempting login at', loginUrl);
    await page.goto(loginUrl, { waitUntil: 'networkidle', timeout: 30000 });
    // try multiple common selectors for the Identity UI
    const emailSel = 'input[type="email"], input[name="Input.Email"], #Input_Email';
    const passwordSel = 'input[type="password"], input[name="Input.Password"], #Input_Password';
    const submitSel = 'button[type="submit"], input[type="submit"]';

    const emailEl = await page.$(emailSel);
    const passEl = await page.$(passwordSel);
    const submitEl = await page.$(submitSel);

    if (emailEl && passEl && submitEl) {
      await page.fill(emailSel, adminEmail);
      await page.fill(passwordSel, adminPassword);
      await Promise.all([
        page.click(submitSel),
        page.waitForNavigation({ waitUntil: 'networkidle', timeout: 30000 }).catch(() => {})
      ]);
      console.log('Login attempt finished, now capturing authenticated routes');

      for (const route of routes) {
        const url = new URL(route, base).toString();
        const safeName = route === '/' ? 'root' : route.replace(/[^a-z0-9]/gi, '_').replace(/^_+|_+$/g, '');
        const filePath = path.join(outDir, `${safeName}_auth.png`);
        try {
          const resp = await page.goto(url, { waitUntil: 'networkidle', timeout: 30000 });
          if (!resp || !resp.ok()) console.warn(`Non-OK response for ${url} after auth: ${resp ? resp.status() : 'no response'}`);
          await page.waitForTimeout(750);
          await page.screenshot({ path: filePath, fullPage: true });
          console.log('Saved', filePath);
        } catch (err) {
          console.error('Failed to capture after auth', url, err.message);
        }
      }
    } else {
      console.warn('Login form not found — skipping authenticated captures');
    }
  } catch (err) {
    console.warn('Login attempt failed:', err.message);
  }

  await browser.close();
  console.log('Done. Screenshots in', outDir);
})();

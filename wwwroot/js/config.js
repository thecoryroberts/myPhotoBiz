/**
 * Template Name: INSPINIA - Multipurpose Admin & Dashboard Template
 * By (Author): WebAppLayers
 * Module/App (File Name): Config
 * Version: 4.2.0
 */

(function () {
    const html = document.documentElement;
    const storageKey = "__INSPINIA_CONFIG__";
    // Use localStorage for persistence across browser sessions
    const savedConfig = localStorage.getItem(storageKey);

    // Default config
    const defaultConfig = {
        skin: "material",
        theme: "system",
        layout: {
            position: "fixed",
        },
        topbar: {
            color: "light",
        },
        menu: {
            color: "dark",
        },
        sidenav: {
            size: "default",
            user: true,
        },
    };

    function getSystemTheme() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? "dark" : "light";
    }

    // Build config from HTML attributes
    const htmlConfig = {
        skin: html.getAttribute("data-skin") || defaultConfig.skin,
        theme: html.getAttribute("data-bs-theme") === 'system'
            ? getSystemTheme()
            : html.getAttribute("data-bs-theme") || (defaultConfig.theme === 'system' ? getSystemTheme() : defaultConfig.theme),
        layout: {
            position: html.getAttribute("data-layout-position") || defaultConfig.layout.position,
        },
        topbar: {
            color: html.getAttribute("data-topbar-color") || defaultConfig.topbar.color,
        },
        menu: {
            color: html.getAttribute("data-menu-color") || defaultConfig.menu.color,
        },
        sidenav: {
            size: html.getAttribute("data-sidenav-size") || defaultConfig.sidenav.size,
            user: html.hasAttribute("data-sidenav-user") || defaultConfig.sidenav.user,
        },
    };

    // Save merged config as defaults globally
    window.defaultConfig = structuredClone(htmlConfig);

    // Load saved config if exists, but let server-rendered skin win.
    // This avoids stale localStorage forcing "classic" when layout sets.
    let config;
    let parsedSavedConfig = null;
    if (savedConfig) {
        try {
            parsedSavedConfig = JSON.parse(savedConfig);
        } catch (e) {
            console.warn("Invalid saved config, using defaults:", e);
            localStorage.removeItem(storageKey);
        }
    }
    config = parsedSavedConfig || htmlConfig;
    if (parsedSavedConfig) {
        config = {
            ...htmlConfig,
            ...parsedSavedConfig,
            layout: { ...htmlConfig.layout, ...parsedSavedConfig.layout },
            topbar: { ...htmlConfig.topbar, ...parsedSavedConfig.topbar },
            menu: { ...htmlConfig.menu, ...parsedSavedConfig.menu },
            sidenav: { ...htmlConfig.sidenav, ...parsedSavedConfig.sidenav },
        };
    }

    if (savedConfig) {
        if (
            html.hasAttribute("data-skin") &&
            config.skin === defaultConfig.skin &&
            htmlConfig.skin !== defaultConfig.skin
        ) {
            config.skin = htmlConfig.skin;
        }
    }    window.config = config;




    // Apply layout attributes immediately
    html.setAttribute("data-skin", config.skin);
    html.setAttribute("data-bs-theme", config.theme === 'system' ? getSystemTheme() : config.theme);
    html.setAttribute("data-menu-color", config.menu.color);
    html.setAttribute("data-topbar-color", config.topbar.color);
    html.setAttribute("data-layout-position", config.layout.position);

    if (config.sidenav.size) {
        let size = config.sidenav.size;

        if (window.innerWidth <= 767) {
            size = "offcanvas";
        } else if (window.innerWidth <= 1140 && !["offcanvas"].includes(size)) {
            size = "condensed";
        }

        html.setAttribute("data-sidenav-size", size);

        if (config.sidenav.user === true) {
            html.setAttribute("data-sidenav-user", "true");
        } else {
            html.removeAttribute("data-sidenav-user");
        }
    }
})();

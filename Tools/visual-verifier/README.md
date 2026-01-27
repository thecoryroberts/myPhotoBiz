Usage

1. Ensure the app is running locally (default http://localhost:5184):

```bash
cd /media/thecoryroberts/External/Solutions/myPhotoBiz/myPhotoBiz
dotnet run
```

2. Open a new terminal and install dependencies for the visual verifier:

```bash
cd tools/visual-verifier
npm install
```

3. Run the verifier to capture screenshots:

```bash
npm run verify
```

4. Screenshots will be saved to `tools/visual-verifier/screenshots/`.

Customization

- To change the base URL, set `BASE_URL`, e.g. `BASE_URL=http://localhost:5184 npm run verify`.
- Edit `verify.js` to add/remove routes or adjust timeouts.

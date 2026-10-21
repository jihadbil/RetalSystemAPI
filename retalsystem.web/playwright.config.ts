import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './tests/browser',
  fullyParallel: false,
  workers: 1,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: { baseURL: 'http://127.0.0.1:4173', trace: 'retain-on-failure',
    launchOptions: { executablePath: process.env.PLAYWRIGHT_CHROMIUM_EXECUTABLE } },
  webServer: { command: 'npm run dev -- --host 127.0.0.1 --port 4173 --strictPort',
    env: { VITE_API_BASE_URL: 'http://127.0.0.1:4173/api' },
    url: 'http://127.0.0.1:4173', reuseExistingServer: !process.env.CI },
});

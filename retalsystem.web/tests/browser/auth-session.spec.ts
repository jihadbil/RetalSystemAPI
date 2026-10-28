import { expect, test, type Page } from '@playwright/test';

const user = { id: 'admin', userName: 'masoud', fullName: 'Masoud', roles: ['Admin'], permissions: [] as string[], tenantId: 'tenant', branchId: 'branch' };

async function cachedSession(page: Page, persistent = true, permissions: string[] = []) {
  await page.addInitScript(({ user, persistent, permissions }) => {
    const storage = persistent ? localStorage : sessionStorage;
    storage.setItem('retal_token', 'cached-session');
    storage.setItem('retal_user', JSON.stringify({ ...user, permissions }));
    storage.setItem('retal_tenant_id', user.tenantId);
    storage.setItem('retal_branch_id', user.branchId);
  }, { user, persistent, permissions });
}

async function dashboard(page: Page) {
  await page.route('**/api/dashboard/summary*', (route) => route.fulfill({ json: { success: true, data: {
    kpis: {}, counts: {}, dailySalesTrend: [], topSellingProducts: [], lowStockAlerts: [], recentInvoices: [],
  } } }));
  await page.route('**/api/lookups/branches*', (route) => route.fulfill({ json: { success: true, data: { items: [] } } }));
}

test('expired saved session returns to login automatically', async ({ page }) => {
  await cachedSession(page);
  await page.route('**/api/auth/session', (route) => route.fulfill({ status: 401, json: { success: false } }));
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'تسجيل الدخول للنظام' })).toBeVisible();
  expect(await page.evaluate(() => [localStorage.getItem('retal_token'), localStorage.getItem('retal_user')])).toEqual([null, null]);
  await expect(page.getByRole('heading', { name: 'لا توجد شاشات مصرح بها' })).not.toBeVisible();
});

for (const persistent of [true, false]) {
  test(`restores permissions from server for ${persistent ? 'remembered' : 'temporary'} session`, async ({ page }) => {
    await cachedSession(page, persistent);
    await dashboard(page);
    await page.route('**/api/auth/session', (route) => route.fulfill({ json: { success: true, data: { ...user, permissions: ['Permissions.Dashboard.View'] } } }));
    await page.goto('/');
    await expect(page.getByRole('heading', { name: 'لوحة العمليات والتحليلات الحية' })).toBeVisible();
    expect(await page.evaluate((persistent) => JSON.parse((persistent ? localStorage : sessionStorage).getItem('retal_user')!).permissions, persistent)).toEqual(['Permissions.Dashboard.View']);
  });
}

test('account without screen permissions can leave immediately even if logout is unavailable', async ({ page }) => {
  await cachedSession(page);
  await page.route('**/api/auth/session', (route) => route.fulfill({ json: { success: true, data: user } }));
  await page.route('**/api/auth/logout', (route) => route.fulfill({ status: 503, json: { success: false } }));
  await page.goto('/');
  await expect(page.getByText('الحساب الحالي: masoud')).toBeVisible();
  await page.getByRole('button', { name: 'تسجيل الخروج', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'تسجيل الدخول للنظام' })).toBeVisible();
});

test('fresh login opens the authorized screen after an expired saved session', async ({ page }) => {
  await cachedSession(page);
  await dashboard(page);
  await page.route('**/api/auth/session', (route) => route.request().headers().authorization === 'Bearer new-session'
    ? route.fulfill({ json: { success: true, data: { ...user, permissions: ['Permissions.Dashboard.View'] } } })
    : route.fulfill({ status: 401, json: { success: false } }));
  await page.route('**/api/auth/login', (route) => route.fulfill({ json: { success: true, data: {
    token: 'new-session', userId: user.id, userName: user.userName, tenantId: user.tenantId, branchId: user.branchId,
    roles: user.roles, permissions: ['Permissions.Dashboard.View'],
  } } }));
  await page.goto('/');
  await page.getByLabel('اسم المستخدم', { exact: true }).fill('masoud');
  await page.getByLabel('كلمة المرور', { exact: true }).fill('isolated-test-password');
  await page.getByRole('button', { name: 'تسجيل الدخول', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'لوحة العمليات والتحليلات الحية' })).toBeVisible();
  expect(await page.evaluate(() => sessionStorage.getItem('retal_token'))).toBe('new-session');
});

test('network failure allows retry without granting cached permissions', async ({ page }) => {
  await cachedSession(page, true, ['Permissions.Dashboard.View']);
  await dashboard(page);
  let unavailable = true;
  await page.route('**/api/auth/session', (route) => unavailable
    ? route.fulfill({ status: 503, json: { message: 'الخادم غير متاح' } })
    : route.fulfill({ json: { success: true, data: { ...user, permissions: ['Permissions.Dashboard.View'] } } }));
  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'تعذر التحقق من الجلسة' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'لوحة العمليات والتحليلات الحية' })).not.toBeVisible();
  unavailable = false;
  await page.getByRole('button', { name: 'إعادة المحاولة', exact: true }).click();
  await expect(page.getByRole('heading', { name: 'لوحة العمليات والتحليلات الحية' })).toBeVisible();
});

test('late session response cannot restore an account after logout', async ({ page }) => {
  await cachedSession(page);
  let release: () => void = () => {};
  const pending = new Promise<void>((resolve) => { release = resolve; });
  await page.route('**/api/auth/session', async (route) => {
    await pending;
    await route.fulfill({ json: { success: true, data: { ...user, permissions: ['Permissions.Dashboard.View'] } } }).catch(() => {});
  });
  await page.route('**/api/auth/logout', (route) => route.fulfill({ json: { success: true } }));
  await page.goto('/');
  await page.getByRole('button', { name: 'العودة لتسجيل الدخول' }).click();
  release();
  await expect(page.getByRole('heading', { name: 'تسجيل الدخول للنظام' })).toBeVisible();
  expect(await page.evaluate(() => localStorage.getItem('retal_token'))).toBeNull();
});

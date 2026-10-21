import { expect, test, type Page, type Route } from '@playwright/test';

const pageResult = (items: unknown[], total = items.length, pageNumber = 1) => ({ items, totalCount: total, pageNumber, pageSize: 50, totalPages: Math.ceil(total / 50), hasNextPage: pageNumber * 50 < total, hasPreviousPage: pageNumber > 1 });
const ok = (route: Route, data: unknown) => route.fulfill({ json: { success: true, data } });
async function session(page: Page) {
  await page.addInitScript(() => {
    localStorage.setItem('retal_token', 'isolated-browser-test');
    localStorage.setItem('retal_user', JSON.stringify({ id: 'cashier', userName: 'cashier', fullName: 'Cashier', roles: ['SuperAdmin'], permissions: [], tenantId: 'tenant', branchId: 'branch-a' }));
    if (!localStorage.getItem('retal_branch_id')) localStorage.setItem('retal_branch_id', 'branch-a');
    localStorage.setItem('retal_tenant_id', 'tenant');
    window.print = () => {};
  });
}

test.beforeEach(async ({ page }) => { await session(page); });

test('nested dialogs trap and restore focus, close only the top dialog and retain scroll locking', async ({ page }) => {
  await page.goto('/tests/browser/harness.html?mode=dialogs');
  await page.evaluate(() => { document.body.style.overflow = 'scroll'; });
  await page.getByRole('button', { name: 'Open parent' }).click();
  await page.getByRole('button', { name: 'Open child' }).click();
  const child = page.getByRole('dialog', { name: 'Child dialog' });
  await expect(child).toBeVisible();
  for (let i = 0; i < 6; i++) {
    await page.keyboard.press('Tab');
    expect(await child.evaluate((element) => element.contains(document.activeElement))).toBe(true);
  }
  await page.keyboard.press('Escape');
  await expect(child).not.toBeVisible();
  await expect(page.getByRole('dialog', { name: 'Parent dialog' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Open child' })).toBeFocused();
  expect(await page.evaluate(() => document.body.style.overflow)).toBe('hidden');
  await page.keyboard.press('Escape');
  await expect(page.getByRole('button', { name: 'Open parent' })).toBeFocused();
  expect(await page.evaluate(() => document.body.style.overflow)).toBe('scroll');
});

test('dashboard ignores a slow response from the previous branch', async ({ page }) => {
  let old: Route | undefined;
  const data = (sales: number) => ({ kpis: { todaySales: sales }, counts: {}, dailySalesTrend: [], topSellingProducts: [], lowStockAlerts: [], recentInvoices: [] });
  await page.route('**/api/dashboard/summary*', async (route) => {
    if (new URL(route.request().url()).searchParams.get('branchId') === 'branch-a') old = route;
    else await ok(route, data(555));
  });
  await page.goto('/tests/browser/harness.html?mode=dashboard');
  await expect.poll(() => Boolean(old)).toBe(true);
  await page.getByRole('button', { name: 'Branch B', exact: true }).click();
  await expect(page.getByText('555.00', { exact: false })).toBeVisible();
  const response = page.waitForResponse('**/api/dashboard/summary?branchId=branch-a');
  await ok(old!, data(111)); await response;
  await expect(page.getByText('555.00', { exact: false })).toBeVisible();
  await expect(page.getByText('111.00', { exact: false })).toHaveCount(0);
});

for (const kind of ['sales', 'purchase']) test(`${kind} details ignore the previous invoice response`, async ({ page }) => {
  let old: Route | undefined;
  const invoice = (name: string) => ({ id: name, invoiceNumber: name, invoiceDate: '2026-10-21', status: 2,
    subTotal: 10, taxAmount: 0, discountAmount: 0, totalAmount: 10, paidAmount: 10, remainingAmount: 0, items: [] });
  await page.route(`**/api/${kind === 'sales' ? 'sales' : 'purchase'}-invoices/*`, async (route) => {
    if (route.request().url().endsWith('invoice-a')) old = route;
    else await ok(route, invoice('LATEST-B'));
  });
  await page.goto(`/tests/browser/harness.html?mode=${kind}-details`);
  await expect.poll(() => Boolean(old)).toBe(true);
  // Harness controls sit outside the native modal; dispatching here simulates changing the selected record.
  await page.getByRole('button', { name: 'Invoice B', exact: true }).dispatchEvent('click');
  await expect(page.getByRole('dialog')).toHaveAccessibleName(/LATEST-B/);
  await ok(old!, invoice('STALE-A'));
  await expect(page.getByRole('dialog')).toHaveAccessibleName(/LATEST-B/);
});

test('lookup can search and select an entry beyond the first page', async ({ page }) => {
  const searched: string[] = [];
  await page.route('**/api/lookups/suppliers*', async (route) => {
    const url = new URL(route.request().url());
    const search = url.searchParams.get('search') || '';
    searched.push(search);
    const item = { id: search || url.searchParams.get('id') ? 'supplier-501' : 'supplier-1', name: search || url.searchParams.get('id') ? 'Supplier 501' : 'Supplier 1', isActive: true };
    await ok(route, pageResult([item], search ? 1 : 501));
  });
  await page.goto('/tests/browser/harness.html?mode=lookup');
  await page.getByRole('searchbox', { name: 'بحث Supplier' }).fill('501');
  await page.getByLabel('Supplier', { exact: true }).selectOption('supplier-501');
  await expect(page.getByLabel('Supplier', { exact: true })).toHaveValue('supplier-501');
  expect(searched).toContain('501');
});

test('stock ignores a slow response for a previously selected warehouse', async ({ page }) => {
  let old: Route | undefined;
  const warehouses = ['warehouse-a', 'warehouse-b'].map((id) => ({ id, name: id, branchId: 'branch-a', type: 1, isActive: true }));
  await page.route('**/api/lookups/warehouses*', async (route) => {
    const id = new URL(route.request().url()).searchParams.get('id');
    await ok(route, pageResult(id ? warehouses.filter((w) => w.id === id) : warehouses));
  });
  await page.route('**/api/stock/storge/*/paged*', async (route) => {
    if (route.request().url().includes('warehouse-a/')) old = route;
    else await ok(route, pageResult([{ productId: 'b', productName: 'LATEST-STOCK-B', quantity: 11 }]));
  });
  await page.goto('/tests/browser/harness.html?mode=stock');
  await expect.poll(() => Boolean(old)).toBe(true);
  await page.getByLabel('المستودع', { exact: true }).selectOption('warehouse-b');
  await expect(page.getByText('LATEST-STOCK-B')).toBeVisible();
  await ok(old!, pageResult([{ productId: 'a', productName: 'STALE-STOCK-A', quantity: 999 }]));
  await expect(page.getByText('LATEST-STOCK-B')).toBeVisible();
  await expect(page.getByText('STALE-STOCK-A')).toHaveCount(0);
});

test('expense creation works with only workflow metadata and no financial view data', async ({ page }) => {
  await page.route('**/api/lookups/*', async (route) => {
    const safe = route.request().url().includes('/safes');
    await ok(route, pageResult([{ id: safe ? 'safe' : 'category', name: safe ? 'Account' : 'Expense category', isActive: true, branchId: safe ? 'branch-a' : undefined }]));
  });
  let posted: Record<string, unknown> | undefined;
  await page.route('**/api/expenses', async (route) => { posted = route.request().postDataJSON(); await ok(route, { id: 'expense' }); });
  await page.goto('/tests/browser/harness.html?mode=expense');
  await page.getByLabel('بند / تصنيف المصروف *', { exact: true }).selectOption('category');
  await page.getByLabel('الخصم من خزينة / بنك *', { exact: true }).selectOption('safe');
  await page.getByLabel('قيمة المصروف (د.ل) *').fill('12');
  await page.getByRole('textbox', { name: 'البيان / تفاصيل وشرح المصروف *' }).fill('Office supplies');
  await page.getByRole('button', { name: /حفظ/ }).click();
  await expect.poll(() => posted?.safeId).toBe('safe');
  expect(posted?.expenseCategoryId).toBe('category');
});

test('lost POS response survives reload and recovers the original operation before a distinct new sale', async ({ page }) => {
  const operations = new Map<string, string>();
  const requests: string[] = [];
  let loseFirstResponse = true;
  await page.route('**/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/pos-device/context') return ok(route, { terminal: { id: 'terminal', branchId: 'branch-a', name: 'Terminal' }, currentSession: { id: 'session' } });
    if (path === '/api/lookups/warehouses') return ok(route, pageResult([{ id: 'warehouse', name: 'Warehouse', branchId: 'branch-a', type: 1, isActive: true }]));
    if (path === '/api/lookups/pos-payment-accounts') return ok(route, pageResult([]));
    if (path === '/api/pos-sales') {
      const number: string = route.request().postDataJSON().invoiceNumber;
      requests.push(number);
      if (!operations.has(number)) operations.set(number, `sale-${operations.size + 1}`);
      if (loseFirstResponse) { loseFirstResponse = false; return route.abort('failed'); }
      return ok(route, { id: operations.get(number), invoiceNumber: number });
    }
    return ok(route, []);
  });
  await page.goto('/tests/browser/harness.html?mode=pos');
  await expect(page.getByTestId('ready')).toHaveText('session');
  await page.getByRole('button', { name: 'Select warehouse' }).click();
  await page.getByRole('button', { name: 'Add item' }).click();
  await page.getByRole('button', { name: 'Exact cash' }).click();
  await page.getByRole('button', { name: 'Checkout', exact: true }).click();
  await expect.poll(() => requests.length).toBe(1);
  const original = requests[0];
  await page.reload();
  await expect(page.getByTestId('pending')).toHaveText(original);
  await page.getByRole('button', { name: 'Checkout', exact: true }).click();
  expect(requests).toEqual([original]);
  await page.getByRole('button', { name: 'Recover' }).click();
  await expect(page.getByTestId('receipt')).toHaveText(original);
  await expect(page.getByTestId('pending')).toHaveText('none');
  expect(operations.size).toBe(1);
  expect(requests).toEqual([original, original]);
  await page.getByRole('button', { name: 'Select warehouse' }).click();
  await page.getByRole('button', { name: 'Add item' }).click();
  await page.getByRole('button', { name: 'Exact cash' }).click();
  await page.getByRole('button', { name: 'Checkout', exact: true }).click();
  await expect.poll(() => operations.size).toBe(2);
  expect(requests[2]).not.toBe(original);
});

test('terminal creation selects a drawer outside the current account page', async ({ page }) => {
  await page.route('**/api/lookups/*', async (route) => {
    const drawer = route.request().url().includes('/cash-drawers');
    await ok(route, pageResult([{ id: drawer ? 'drawer-501' : 'branch-a', name: drawer ? 'Drawer 501' : 'Branch A', branchId: 'branch-a', isActive: true }]));
  });
  let posted: Record<string, unknown> | undefined;
  await page.route('**/api/pos-terminals', async (route) => { posted = route.request().postDataJSON(); await ok(route, { id: 'terminal' }); });
  await page.goto('/tests/browser/harness.html?mode=terminal');
  await page.getByLabel('رمز نقطة البيع').fill('POS-501');
  await page.getByLabel('اسم نقطة البيع').fill('Counter');
  await page.getByLabel('درج النقود', { exact: true }).selectOption('drawer-501');
  await page.getByRole('button', { name: 'حفظ نقطة البيع' }).click();
  await expect.poll(() => posted?.defaultCashDrawerId).toBe('drawer-501');
  expect(posted?.branchId).toBe('branch-a');
});

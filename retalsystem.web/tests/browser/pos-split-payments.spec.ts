import { test, expect, type Page, type Route } from '@playwright/test';

const ok = (route: Route, data: unknown) => route.fulfill({ json: { success: true, data } });
const pageResult = (items: unknown[]) => ({ items, totalCount: items.length, pageNumber: 1, pageSize: 50, totalPages: 1, hasNextPage: false, hasPreviousPage: false });
async function login(page: Page) {
  await page.addInitScript(() => {
    localStorage.setItem('retal_token', 'isolated-test');
    localStorage.setItem('retal_user', JSON.stringify({ id: 'cashier', userName: 'cashier', roles: ['SuperAdmin'], permissions: [], tenantId: 'tenant', branchId: 'branch-a' }));
    localStorage.setItem('retal_tenant_id', 'tenant'); localStorage.setItem('retal_branch_id', 'branch-a'); window.print = () => {};
  });
}
test.beforeEach(async ({ page }) => {
  await login(page);
  await page.route('**/api/auth/session', route => ok(route, { id: 'cashier', userName: 'cashier', roles: ['SuperAdmin'], permissions: [], tenantId: 'tenant', branchId: 'branch-a' }));
});

test('cashier saves all payment amounts in a dialog, checks out normally, and closes without automatic settlement', async ({ page }) => {
  let posted: Record<string, unknown> | undefined;
  let closed = 0, settlements = 0;
  await page.route('**/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/auth/session') return route.fallback();
    if (path === '/api/pos-device/context') return ok(route, { terminal: { id: 'terminal', branchId: 'branch-a', name: 'Terminal' }, currentSession: closed ? undefined : { id: 'session' } });
    if (path === '/api/lookups/warehouses') return ok(route, pageResult([{ id: 'warehouse', name: 'Warehouse', branchId: 'branch-a', type: 1, isActive: true }]));
    if (path === '/api/pos-sales') { posted = route.request().postDataJSON(); return ok(route, { id: 'saved-sale', invoiceNumber: posted!.invoiceNumber }); }
    if (path === '/api/pos-device/sessions/close') { closed++; return ok(route, { id: 'session', status: 3 }); }
    if (path.endsWith('/settle-payments')) settlements++;
    return ok(route, pageResult([]));
  });
  await page.goto('/tests/browser/harness.html?mode=pos-split');
  await expect(page.getByTestId('ready')).toHaveText('session');
  await page.getByRole('button', { name: 'Add item', exact: true }).click();
  await page.getByRole('button', { name: 'Select warehouse', exact: true }).click();
  await page.getByRole('button', { name: 'دفع متعدد', exact: true }).click();
  const dialog = page.getByRole('dialog', { name: 'الدفع بأكثر من وسيلة' });
  await expect(dialog.getByRole('spinbutton')).toHaveCount(4);
  await dialog.getByLabel('مبلغ نقداً', { exact: true }).fill('4');
  await dialog.getByLabel('مبلغ بطاقة مصرفية', { exact: true }).fill('6');
  await dialog.getByLabel('النقد المستلم', { exact: false }).fill('5');
  await dialog.getByRole('button', { name: 'حفظ التوزيع' }).click();
  await expect(dialog).not.toBeVisible();
  expect(posted).toBeUndefined();
  await page.getByRole('button', { name: 'دفع متعدد', exact: true }).click();
  await dialog.getByLabel('مبلغ نقداً', { exact: true }).fill('9');
  await dialog.getByRole('button', { name: 'إلغاء', exact: true }).click();
  await expect(page.getByLabel('توزيع الدفع المعتمد')).toContainText('4.0000');
  await page.getByRole('button', { name: 'إتمام البيع والطباعة (F10)', exact: true }).click();
  await expect.poll(() => Boolean(posted)).toBe(true);
  expect(posted!.cashSessionId).toBe('session'); expect(posted!.paidAmount).toBe(10);
  expect(posted!.payments).toEqual([expect.objectContaining({ paymentMethod: 1, amount: 4, tenderedAmount: 5 }), expect.objectContaining({ paymentMethod: 3, amount: 6 })]);
  await expect(page.getByTestId('cart-count')).toHaveText('0');
  await page.getByRole('button', { name: 'Close shift', exact: true }).click();
  await expect.poll(() => closed).toBe(1); expect(settlements).toBe(0);
});

test('settlement is an explicit separate action after closure with an account for each method', async ({ page }) => {
  let settled = false;
  let posted: { accounts: { paymentMethod: number; safeId: string }[] } | undefined;
  const session = () => ({ id: 'session', branchId: 'branch-a', terminalName: 'Terminal', cashDrawerName: 'Drawer', openedAt: '2026-10-25T12:00:00Z', status: 3, isSettled: settled,
    totals: [{ paymentMethod: 1, amount: 4 }, { paymentMethod: 3, amount: 6 }],
    movements: [{ id: 'cash', invoiceNumber: 'POS-1', paymentMethod: 1, direction: 1, amount: 4, isSettled: settled }, { id: 'card', invoiceNumber: 'POS-1', paymentMethod: 3, direction: 1, amount: 6, isSettled: settled }] });
  await page.route('**/api/**', async route => {
    const path = new URL(route.request().url()).pathname;
    if (path === '/api/auth/session') return route.fallback();
    if (path === '/api/cash-sessions/payments') return ok(route, [session()]);
    if (path.endsWith('/settle-payments')) { posted = route.request().postDataJSON(); settled = true; return ok(route, session()); }
    if (path === '/api/lookups/safes') return ok(route, pageResult([{ id: 'box', name: 'Cash box' }, { id: 'bank', name: 'Bank account' }]));
    return ok(route, []);
  });
  await page.goto('/tests/browser/harness.html?mode=session-payments');
  await page.getByRole('button', { name: 'عرض الحركات والتسوية' }).click();
  expect(posted).toBeUndefined();
  const dialog = page.getByRole('dialog');
  await dialog.getByLabel('حساب تسوية نقداً', { exact: true }).selectOption('box');
  await dialog.getByLabel('حساب تسوية بطاقة مصرفية', { exact: true }).selectOption('bank');
  await dialog.getByRole('button', { name: 'تسوية اختيارية إلى الحسابات' }).click();
  await expect(dialog).toContainText('تم تحويل حركات هذه الوردية إلى حساباتها.');
  expect(posted!.accounts).toEqual([{ paymentMethod: 1, safeId: 'box' }, { paymentMethod: 3, safeId: 'bank' }]);
});

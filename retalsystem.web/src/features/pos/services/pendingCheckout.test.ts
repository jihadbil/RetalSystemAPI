import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ApiError } from '../../../shared/services/apiClient';
import { loadPendingCheckout, savePendingCheckout, settlePendingCheckout, type PendingCheckout } from './pendingCheckout';

const rows = new Map<string, string>();
const storage = { getItem: (key: string) => rows.get(key) ?? null,
  setItem: (key: string, value: string) => { rows.set(key, value); }, removeItem: (key: string) => { rows.delete(key); } };
const pending = (scope = 'tenant:branch:user', number = 'POS-original'): PendingCheckout => ({
  version: 1, scope, request: { invoiceNumber: number, warehouseId: 'warehouse', items: [{ productId: 'p', quantity: 1 }] },
  receipt: { invoiceNumber: number, date: '2026-10-21', cashierName: 'Cashier',
    items: [{ name: 'Product', quantity: 1, price: 10, total: 10 }], subtotal: 10, discount: 0, tax: 0, total: 10, paid: 10, change: 0, paymentMethod: 'cash' },
});

beforeEach(() => { rows.clear(); vi.restoreAllMocks(); vi.stubGlobal('localStorage', storage); });

describe('pending POS operation lifecycle', () => {
  it('retains the exact request across lost response and reload, then allows a distinct identical sale', async () => {
    const original = pending(); savePendingCheckout(original);
    const operations = new Map<string, string>();
    const send = vi.fn(async (payload) => {
      const number = payload.invoiceNumber as string;
      if (!operations.has(number)) operations.set(number, `invoice-${operations.size}`);
      if (send.mock.calls.length === 1) throw new ApiError('response lost after commit', 0);
      return { id: operations.get(number)!, invoiceNumber: number };
    });
    await expect(settlePendingCheckout(original, send, true)).rejects.toThrow();
    const afterReload = loadPendingCheckout(original.scope)!;
    expect(afterReload.request).toEqual(original.request);
    const receipt = await settlePendingCheckout(afterReload, send, false);
    expect(receipt.invoiceNumber).toBe(original.request.invoiceNumber);
    expect(operations.size).toBe(1); expect(loadPendingCheckout(original.scope)).toBeNull();
    const next = pending(original.scope, 'POS-new'); savePendingCheckout(next);
    await settlePendingCheckout(next, send, true);
    expect(operations.size).toBe(2);
  });
  it('isolates pending operations by tenant, branch and cashier and refuses replacement', () => {
    const original = pending(); savePendingCheckout(original);
    expect(loadPendingCheckout('other:branch:user')).toBeNull();
    expect(loadPendingCheckout('tenant:other:user')).toBeNull();
    expect(loadPendingCheckout('tenant:branch:other')).toBeNull();
    expect(() => savePendingCheckout(pending(original.scope, 'POS-new'))).toThrow();
    expect(loadPendingCheckout(original.scope)?.request.invoiceNumber).toBe('POS-original');
  });
  it('permits a fresh sale after a definite initial validation rejection', async () => {
    const original = pending(); savePendingCheckout(original);
    await expect(settlePendingCheckout(original, async () => { throw new ApiError('invalid stock', 400); }, true)).rejects.toThrow();
    expect(loadPendingCheckout(original.scope)).toBeNull();
  });
  it('keeps an uncertain operation even if a later retry is rejected', async () => {
    const original = pending(); savePendingCheckout(original);
    await expect(settlePendingCheckout(original, async () => { throw new ApiError('session closed', 400); }, false)).rejects.toThrow();
    expect(loadPendingCheckout(original.scope)?.request.invoiceNumber).toBe('POS-original');
  });
  it('stops before submission when persistence is unavailable', () => {
    vi.spyOn(storage, 'setItem').mockImplementation(() => { throw new Error('quota'); });
    expect(() => savePendingCheckout(pending())).toThrow('quota');
    expect(rows.size).toBe(0);
  });
  it('requires the server invoice id before clearing an uncertain operation', async () => {
    const original = pending(); savePendingCheckout(original);
    await expect(settlePendingCheckout(original, async () => ({ id: '', invoiceNumber: 'POS-original' }), false)).rejects.toThrow();
    expect(loadPendingCheckout(original.scope)).not.toBeNull();
  });
});

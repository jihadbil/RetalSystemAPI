import React from 'react';
import { renderToStaticMarkup } from 'react-dom/server';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
const transport = vi.hoisted(() => ({ get: vi.fn(), post: vi.fn(), put: vi.fn() }));
vi.mock('./shared/services/apiClient', async (original) => ({ ...await original<typeof import('./shared/services/apiClient')>(), apiClient: transport }));
import { CheckoutAttempt } from './features/pos/services/CheckoutAttempt';
import { posService } from './features/pos/services/posService';
import { buildCustomerPayload, customerService } from './features/customers/services/customerService';
import { buildSupplierPayload, supplierService } from './features/suppliers/services/supplierService';
import { accountingService } from './features/accounting/services/accountingService';
import { TransactionDirection } from './features/accounting/types/accounting.types';
import { Pagination } from './shared/components/ui/Pagination';

describe('V2 engineering repairs', () => {
  beforeEach(() => vi.clearAllMocks());
  afterEach(() => vi.unstubAllGlobals());
  it('retains one key after a lost response and an identical retry', async () => {
    const attempt = new CheckoutAttempt();
    const payload = { warehouseId: 'warehouse', items: [{ productId: 'product', quantity: 1 }] };
    transport.post.mockRejectedValueOnce(new Error('Response lost')).mockResolvedValueOnce({ id: 'invoice' });
    const key = attempt.begin(payload)!;
    await expect(posService.checkout({ ...payload, invoiceNumber: key })).rejects.toThrow('Response lost');
    attempt.finish(false);
    const retry = attempt.begin({ ...payload })!;
    expect(retry).toBe(key);
    await posService.checkout({ ...payload, invoiceNumber: retry });
    expect(transport.post.mock.calls.map((call) => call[1].invoiceNumber)).toEqual([key, key]);
  });
  it('locks immediately against a double checkout and releases for a retry', () => {
    const attempt = new CheckoutAttempt();
    const first = attempt.begin({ quantity: 1 });
    expect(attempt.inFlight).toBe(true);
    expect(attempt.begin({ quantity: 1 })).toBeNull();
    attempt.finish(false);
    expect(attempt.inFlight).toBe(false);
    expect(attempt.begin({ quantity: 1 })).toBe(first);
  });
  it('uses a new key for an intentional new sale after success or clearing', () => {
    const attempt = new CheckoutAttempt();
    const first = attempt.begin({ quantity: 1 }); attempt.finish(true);
    const second = attempt.begin({ quantity: 1 }); expect(second).not.toBe(first);
    attempt.finish(false); attempt.reset();
    expect(attempt.begin({ quantity: 1 })).not.toBe(second);
  });
  it('uses a new key when the warehouse or quantity changes', () => {
    const attempt = new CheckoutAttempt();
    const first = attempt.begin({ warehouseId: 'a', quantity: 1 }); attempt.finish(false);
    expect(attempt.begin({ warehouseId: 'b', quantity: 1 })).not.toBe(first);
  });
  it('surfaces quota failure to the draft caller', () => {
    const quota = new DOMException('Full', 'QuotaExceededError');
    vi.stubGlobal('localStorage', { setItem: () => { throw quota; } });
    expect(() => posService.saveDrafts('tenant:branch:user', [])).toThrow(quota);
  });
  it('retains customer and supplier legal metadata and notes in payloads', () => {
    expect(buildCustomerPayload({ name: 'Customer', phone: '', isActive: true, creditLimit: 0, discountRate: 3.125,
      taxNumber: ' TAX ', commercialRegister: ' REG ', notes: ' Notes ' })).toMatchObject({
      taxNumber: 'TAX', commercialRegister: 'REG', discountRate: 3.125, notes: 'Notes',
    });
    expect(buildSupplierPayload({ name: 'Supplier', phone: '', isActive: true,
      companyName: ' Company ', email: ' mail@example.test ', taxNumber: ' TAX ', notes: ' Notes ' })).toMatchObject({
      companyName: 'Company', email: 'mail@example.test', taxNumber: 'TAX', notes: 'Notes',
    });
  });
  it('filters debt independently of activity and preserves global totals', async () => {
    const summary = { totalDebt: 300, totalCredit: 0, withDebt: 30 };
    transport.get.mockResolvedValue({ items: [], totalCount: 30, summary });
    expect((await customerService.getPaged(2, 10, 'text', undefined, true)).summary).toEqual(summary);
    expect(transport.get).toHaveBeenLastCalledWith('customers/paged', { pageNumber: 2, pageSize: 10, search: 'text', hasDebt: 'true' });
    await supplierService.getPaged(2, 10, undefined, true, true);
    expect(transport.get).toHaveBeenLastCalledWith('suppliers/paged', { pageNumber: 2, pageSize: 10, isActive: 'true', hasDebt: 'true' });
  });
  it('preserves manual transaction and transfer dates and references', async () => {
    await accountingService.createTransaction({ safeId: 'safe', amount: 10, direction: TransactionDirection.Inbound,
      transactionDate: '2026-01-04', referenceNumber: 'REF', description: 'Manual' });
    expect(transport.post).toHaveBeenLastCalledWith('safes/transactions', expect.objectContaining({ transactionDate: '2026-01-04', referenceNumber: 'REF' }));
    await accountingService.transferFunds({ sourceSafeId: 'a', destinationSafeId: 'b', amount: 1, transferDate: '2026-01-05', referenceNumber: 'TRANSFER' });
    expect(transport.post).toHaveBeenLastCalledWith('safes/transfer', expect.objectContaining({ transferDate: '2026-01-05', referenceNumber: 'TRANSFER' }));
  });
  it('passes requested accounting pages to the API', async () => {
    transport.get.mockResolvedValue({ items: [], summary: 1000 });
    await accountingService.getExpenses(3, 25);
    expect(transport.get).toHaveBeenLastCalledWith('expenses', { pageNumber: 3, pageSize: 25 });
    await accountingService.getSafeTransactions('safe', 4, 25);
    expect(transport.get).toHaveBeenLastCalledWith('safes/transactions', { safeId: 'safe', pageNumber: 4, pageSize: 25 });
  });
  it('gets safe details and renders the totalItems paginator contract', async () => {
    transport.get.mockResolvedValue({ id: 'bank', bankAccountNumber: 'Account' });
    expect((await accountingService.getSafeById('bank')).bankAccountNumber).toBe('Account');
    expect(transport.get).toHaveBeenLastCalledWith('safes/bank');
    const html = renderToStaticMarkup(<Pagination currentPage={1} totalPages={13} pageSize={10} totalItems={123} onPageChange={() => {}} />);
    expect(html).toContain('>123</span>');
  });
});

import { beforeEach, describe, expect, it, vi } from 'vitest';

const transport = vi.hoisted(() => ({ get: vi.fn(), post: vi.fn(), put: vi.fn(), patch: vi.fn() }));
vi.mock('./shared/services/apiClient', async (importOriginal) => ({ ...await importOriginal<typeof import('./shared/services/apiClient')>(), apiClient: transport }));

import { customerService } from './features/customers/services/customerService';
import { supplierService } from './features/suppliers/services/supplierService';
import { organizationService } from './features/organization/services/organizationService';
import { buildCustomerPayload } from './features/customers/services/customerService';
import { buildSupplierPayload } from './features/suppliers/services/supplierService';
import { posService } from './features/pos/services/posService';
import { purchaseService } from './features/purchase/services/purchaseService';
import { stockService } from './features/stock/services/stockService';
import { canOpenView } from './shared/auth/viewAccess';
import { salesReturnLines, purchaseReturnLines } from './shared/utils/returnLines';
import { InvoiceStatus } from './features/sales/types/sales.types';
import { PurchaseInvoiceStatus } from './features/purchase/types/purchase.types';

describe('feature audit regression contracts', () => {
  beforeEach(() => vi.clearAllMocks());

  it('sends a customer phone in the backend Phones collection', async () => {
    transport.post.mockResolvedValue({ id: 'customer' });
    await customerService.create({ name: 'Customer', phone: '0912345678', creditLimit: 0, discountRate: 0, isActive: true });
    expect(transport.post.mock.calls[0][1]).toMatchObject({ phones: [{ phoneNumber: '0912345678' }] });
  });

  it('sends a supplier phone in the backend Phones collection', async () => {
    transport.post.mockResolvedValue({ id: 'supplier' });
    await supplierService.create({ name: 'Supplier', phone: '0912345678', isActive: true });
    expect(transport.post.mock.calls[0][1]).toMatchObject({ phones: [{ phoneNumber: '0912345678' }] });
  });

  it('maps customer PrimaryPhone from real summary responses', async () => {
    transport.get.mockResolvedValue({ items: [{ id: 'customer', name: 'Customer', primaryPhone: '0912345678', phoneCount: 1 }] });
    const result = await customerService.getPaged();
    expect(result.items[0].phone).toBe('0912345678');
  });

  it('maps edited tenant UI fields back to the backend names', async () => {
    transport.put.mockResolvedValue({});
    await organizationService.updateTenantProfile({
      id: 'tenant', name: 'Tenant', companyName: 'New commercial name',
      phone: '0922222222', email: 'new@example.test', currencySymbol: 'NEW',
      defaultTaxRate: 0, receiptHeader: 'New header', receiptFooter: 'New footer',
      ...({ commercialName: 'Old commercial name', phoneNumber: '0911111111', contactEmail: 'old@example.test',
        defaultCurrencySymbol: 'OLD', invoiceHeaderNote: 'Old header', invoiceFooterNote: 'Old footer' }),
    });
    expect(transport.put.mock.calls[0][1]).toMatchObject({
      commercialName: 'New commercial name', phoneNumber: '0922222222', contactEmail: 'new@example.test',
      defaultCurrencySymbol: 'NEW', invoiceHeaderNote: 'New header', invoiceFooterNote: 'New footer',
    });
  });

  it('preserves customer details and secondary phones while changing the primary phone', () => {
    expect(buildCustomerPayload({ name: 'Customer', phone: 'new', isActive: true, creditLimit: 10, discountRate: 0,
      code: 'C-42', type: 3, address: 'Address', email: 'mail@example.test',
      phones: [{ phoneNumber: 'secondary', isDefault: false }, { phoneNumber: 'primary', isDefault: true }],
    })).toMatchObject({ code: 'C-42', type: 3, address: 'Address', email: 'mail@example.test', phones: [{ phoneNumber: 'secondary' }, { phoneNumber: 'new', isDefault: true }] });
  });

  it('sends an empty phone collection when the only customer phone is cleared', () => {
    expect(buildCustomerPayload({ name: 'Customer', phone: '', isActive: true, creditLimit: 0, discountRate: 0,
      phones: [{ phoneNumber: 'old', isDefault: true }] }).phones).toEqual([]);
  });

  it('preserves additional supplier contacts during primary phone editing', () => {
    expect(buildSupplierPayload({ name: 'Supplier', phone: 'new', isActive: false,
      phones: [{ phoneNumber: 'old', name: 'Main' }, { phoneNumber: 'secondary', name: 'Accounts' }],
    })).toMatchObject({ isActive: false, phones: [{ phoneNumber: 'new', name: 'Main' }, { phoneNumber: 'secondary', name: 'Accounts' }] });
  });

  it('uses the POS permission boundary and retains the selected card account', async () => {
    transport.post.mockResolvedValue({ id: 'sale', invoiceNumber: 'POS-1' });
    await posService.checkout({ warehouseId: 'storage', paymentMethod: 3, paymentSafeId: 'bank-2' });
    expect(transport.post).toHaveBeenCalledWith('pos-sales', expect.objectContaining({ paymentSafeId: 'bank-2' }));
  });

  it('sends the received and completed status values to posting actions', async () => {
    await purchaseService.receiveOrder('order');
    await stockService.completeTransfer('transfer');
    expect(transport.patch.mock.calls).toEqual([['purchase-orders/order/status', 4], ['stock-transfers/transfer/status', 3]]);
  });

  it.each([
    ['sales', 'SalesReturns'], ['purchases', 'PurchaseOrders'], ['stock', 'StockTransfers'],
    ['catalog', 'Categories'], ['accounting', 'Expenses'], ['users', 'Roles'],
  ] as const)('opens %s for its authorized subfeature', (view, permission) => {
    expect(canOpenView(view, (key) => key === `Permissions.${permission}.View`)).toBe(true);
    expect(canOpenView(view, () => false)).toBe(false);
  });

  it('keeps different sales barcodes separate and groups repeated lots at original net prices', () => {
    const lines = salesReturnLines({ id: 'sale', invoiceNumber: 'SALE', invoiceDate: '', status: InvoiceStatus.Paid, paymentMethod: 1,
      totalAmount: 36, paidAmount: 36, remainingAmount: 0, items: [
        { productId: 'p', productName: 'Product', productBarCodeId: 'a', quantity: 1, unitPrice: 10, discountAmount: 0 },
        { productId: 'p', productName: 'Product', productBarCodeId: 'b', quantity: 1, unitPrice: 20, discountAmount: 0 },
        { productId: 'p', productName: 'Product', productBarCodeId: 'a', quantity: 1, unitPrice: 10, discountAmount: 0 },
      ] });
    expect(lines).toEqual([
      expect.objectContaining({ productBarCodeId: 'a', quantity: 2, unitPrice: 9 }),
      expect.objectContaining({ productBarCodeId: 'b', quantity: 1, unitPrice: 18 }),
    ]);
  });

  it('returns purchase breakdown barcodes and allocates the original discount to them', () => {
    const lines = purchaseReturnLines({ id: 'purchase', invoiceNumber: 'PUR', invoiceDate: '', branchId: 'branch', warehouseId: 'storage',
      status: PurchaseInvoiceStatus.Paid, subTotal: 40, discountAmount: 4, taxAmount: 0, totalAmount: 36, paidAmount: 36, remainingAmount: 0,
      items: [{ productId: 'p', productName: 'Product', quantity: 3, unitCost: 10, unitPrice: 10, discountAmount: 0, lineTotal: 40,
        breakdowns: [{ productBarCodeId: 'a', quantity: 2, packageQuantity: 2, unitsPerPackage: 1, unitPrice: 10 },
          { productBarCodeId: 'b', quantity: 1, packageQuantity: 1, unitsPerPackage: 1, unitPrice: 20 }] }],
    });
    expect(lines).toEqual([
      expect.objectContaining({ productBarCodeId: 'a', quantity: 2, unitPrice: 9 }),
      expect.objectContaining({ productBarCodeId: 'b', quantity: 1, unitPrice: 18 }),
    ]);
  });
});

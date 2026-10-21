import { describe, expect, it } from 'vitest';
import type { CreatePurchaseInvoiceItemRequest } from '../types/purchase.types';
import { allocatePurchaseLine, purchaseLineTotal } from './purchasePriceAllocation';

const item = (changes: Partial<CreatePurchaseInvoiceItemRequest> = {}): CreatePurchaseInvoiceItemRequest => ({
  productId: 'p', productName: 'Product', packageUnitName: 'Box', packageQuantity: 1,
  unitsPerPackage: 3, invoicePackagePrice: 10, quantity: 3, unitPrice: 10 / 3,
  salePrice: 5, discountAmount: 0, lineTotal: 10, profitPerPiece: 0, profitMarginPercentage: 0, ...changes,
});

describe('purchase package price allocation', () => {
  it.each([3, 7, 12, 999])('preserves the exact package value for %i base units', (quantity) => {
    const input = item({ quantity, unitsPerPackage: quantity });
    const lines = allocatePurchaseLine(input);
    expect(lines.reduce((sum, line) => sum + line.quantity, 0)).toBe(quantity);
    expect(lines.reduce((sum, line) => sum + Math.round(line.unitPrice * 10000) * line.quantity, 0)).toBe(100000);
    expect(lines.every((line) => Number(line.unitPrice.toFixed(4)) === line.unitPrice)).toBe(true);
    expect(purchaseLineTotal(input)).toBe(10);
  });
  it('preserves barcode quantities and the exact discount across the allocated lines', () => {
    const input = item({ quantity: 9, packageQuantity: 3, discountAmount: 0.0001, breakdowns: [
      { productBarCodeId: 'a', quantity: 3, unitsPerPackage: 3, packageQuantity: 1, unitPrice: 10 / 3 },
      { productBarCodeId: 'b', quantity: 6, unitsPerPackage: 3, packageQuantity: 2, unitPrice: 10 / 3 },
    ] });
    const lines = allocatePurchaseLine(input);
    expect(lines.filter((l) => l.productBarCodeId === 'a').reduce((sum, l) => sum + l.quantity, 0)).toBe(3);
    expect(lines.filter((l) => l.productBarCodeId === 'b').reduce((sum, l) => sum + l.quantity, 0)).toBe(6);
    expect(lines.reduce((sum, l) => sum + Math.round(l.discountAmount * 10000), 0)).toBe(1);
    expect(purchaseLineTotal(input)).toBe(29.9999);
  });
  it('supports fractional packages containing whole base units and free goods', () => {
    expect(purchaseLineTotal(item({ packageQuantity: 0.5, unitsPerPackage: 6 }))).toBe(5);
    expect(purchaseLineTotal(item({ packageQuantity: 1.001, unitsPerPackage: 1000, quantity: 1001 }))).toBe(10.01);
    expect(purchaseLineTotal(item({ invoicePackagePrice: 0 }))).toBe(0);
  });
  it.each([
    { quantity: 1.5 }, { quantity: 2147483648 }, { unitsPerPackage: 2 },
    { invoicePackagePrice: 1.00001 }, { invoicePackagePrice: -1 }, { discountAmount: 10.0001 },
    { breakdowns: [{ productBarCodeId: 'a', quantity: 2, packageQuantity: 1, unitsPerPackage: 2, unitPrice: 5 }] },
  ])('rejects invalid amounts or quantities before posting: %j', (changes) => {
    expect(() => allocatePurchaseLine(item(changes))).toThrow();
  });
});

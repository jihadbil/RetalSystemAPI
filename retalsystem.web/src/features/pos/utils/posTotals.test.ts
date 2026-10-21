import { describe, expect, it } from 'vitest';
import { calculatePosTotals } from './posTotals';

describe('POS totals agree with decimal API amounts', () => {
  it.each([
    { items: [{ quantity: 1, unitPrice: 0.1, discountAmount: 0 }, { quantity: 1, unitPrice: 0.2, discountAmount: 0 }], total: 0.3 },
    { items: [{ quantity: 3, unitPrice: 0.1, discountAmount: 0 }], total: 0.3 },
    { items: [{ quantity: 3, unitPrice: 1.2345, discountAmount: 0 }], total: 3.7035 },
  ])('accepts exact payment for $total without an artificial remainder', ({ items, total }) => {
    const amounts = calculatePosTotals(items, 0, 0, total);
    expect(amounts.subtotal).toBe(total);
    expect(amounts.grandTotal).toBe(total);
    expect(amounts.remainingAmount).toBe(0);
    expect(amounts.changeAmount).toBe(0);
  });

  it('preserves a real shortfall of one smallest monetary unit', () => {
    const amounts = calculatePosTotals([{ quantity: 3, unitPrice: 0.1, discountAmount: 0 }], 0, 0, 0.2999);
    expect(amounts.remainingAmount).toBe(0.0001);
    expect(amounts.changeAmount).toBe(0);
  });

  it('calculates change after line and invoice discounts', () => {
    const amounts = calculatePosTotals([{ quantity: 3, unitPrice: 0.1, discountAmount: 0.01 }], 0.02, 0, 0.3);
    expect(amounts.subtotal).toBe(0.29);
    expect(amounts.grandTotal).toBe(0.27);
    expect(amounts.changeAmount).toBe(0.03);
  });

  it('keeps the existing two-decimal tax rule and four-decimal total', () => {
    const amounts = calculatePosTotals([{ quantity: 1, unitPrice: 1.2345, discountAmount: 0 }], 0, 10, 2);
    expect(amounts.taxAmount).toBe(0.12);
    expect(amounts.grandTotal).toBe(1.3545);
    expect(amounts.changeAmount).toBe(0.6455);
  });
});

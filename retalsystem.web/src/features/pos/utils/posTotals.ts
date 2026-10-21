import type { CartItem } from '../types/pos.types';

// Monetary values use four decimal places in the API and database.
const money = (value: number) => Number(value.toFixed(4));

export const calculatePosTotals = (
  items: Pick<CartItem, 'quantity' | 'unitPrice' | 'discountAmount'>[],
  discount: number,
  taxRate: number,
  paidAmount: number,
) => {
  const subtotal = money(items.reduce((sum, item) => sum + Math.max(0, item.quantity * item.unitPrice - item.discountAmount), 0));
  const discountAmount = money(Math.min(subtotal, Math.max(0, discount)));
  const taxAmount = Math.round((subtotal - discountAmount) * (taxRate / 100) * 100) / 100;
  const grandTotal = money(Math.max(0, subtotal - discountAmount + taxAmount));
  const changeAmount = money(Math.max(0, paidAmount - grandTotal));
  const remainingAmount = money(Math.max(0, grandTotal - paidAmount));
  return { subtotal, discountAmount, taxAmount, grandTotal, changeAmount, remainingAmount };
};

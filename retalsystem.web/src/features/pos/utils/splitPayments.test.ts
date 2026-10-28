import { describe, expect, it } from 'vitest';
import { PaymentMethod, type PosPayment } from '../types/pos.types';
import { sumPayments, validateSplitPayments } from './splitPayments';

const split = (cash = 40, card = 60): PosPayment[] => [
  { clientMovementId: 'cash', paymentMethod: PaymentMethod.Cash, amount: cash, tenderedAmount: cash + 10 },
  { clientMovementId: 'card', paymentMethod: PaymentMethod.Card, amount: card },
];
describe('split payment allocation', () => {
  it('sums allocated amounts without including cash change', () => {
    expect(sumPayments(split())).toBe(100);
    expect(validateSplitPayments(split(), 100, false)).toBeNull();
    expect(sumPayments(split(0.1, 0.2))).toBe(0.3);
  });
  it('rejects overpayment, underpayment without a customer, and duplicate methods', () => {
    expect(validateSplitPayments(split(50, 60), 100, false)).toBeTruthy();
    expect(validateSplitPayments(split(10, 60), 100, false)).toBeTruthy();
    expect(validateSplitPayments(split(10, 60), 100, true)).toBeNull();
    const values = split(); values[1].paymentMethod = PaymentMethod.Cash;
    expect(validateSplitPayments(values, 100, false)).toBeTruthy();
  });
  it('rejects non-receipt methods, invalid precision and non-cash tendered money', () => {
    for (const method of [PaymentMethod.Credit, PaymentMethod.Multiple]) {
      const values = split(); values[1].paymentMethod = method;
      expect(validateSplitPayments(values, 100, false)).toBeTruthy();
    }
    expect(validateSplitPayments(split(0.00001, 99.99999), 100, false)).toBeTruthy();
    const values = split(); values[1].tenderedAmount = 60;
    expect(validateSplitPayments(values, 100, false)).toBeTruthy();
  });
});

import { PaymentMethod, PosTenderMethods, type PosPayment } from '../types/pos.types';

const units = (amount: number) => Math.round(amount * 10000);
export const sumPayments = (payments: readonly Pick<PosPayment, 'amount'>[]) =>
  payments.reduce((total, payment) => total + units(payment.amount), 0) / 10000;

export function validateSplitPayments(payments: readonly PosPayment[], total: number, hasCustomer: boolean): string | null {
  if (payments.length === 0) return 'أدخل مبلغ وسيلة دفع واحدة على الأقل';
  if (payments.length > 4 || new Set(payments.map(p => p.paymentMethod)).size !== payments.length ||
      new Set(payments.map(p => p.clientMovementId)).size !== payments.length) return 'توزيع وسائل الدفع يحتوي على تكرار';
  for (const payment of payments) {
    if (!PosTenderMethods.some(method => method === payment.paymentMethod) || !Number.isFinite(payment.amount) ||
        payment.amount <= 0 || !Number.isSafeInteger(units(payment.amount)) || Math.abs(payment.amount - units(payment.amount) / 10000) > 1e-9)
      return 'أدخل مبالغ موجبة بحد أقصى أربع منازل عشرية';
    if (payment.tenderedAmount !== undefined && (!Number.isFinite(payment.tenderedAmount) || payment.paymentMethod !== PaymentMethod.Cash ||
        payment.tenderedAmount < payment.amount || !Number.isSafeInteger(units(payment.tenderedAmount)) ||
        Math.abs(payment.tenderedAmount - units(payment.tenderedAmount) / 10000) > 1e-9)) return 'راجع النقد المستلم والفكة';
  }
  const paid = units(sumPayments(payments)), due = units(total);
  if (!Number.isSafeInteger(paid) || !Number.isSafeInteger(due)) return 'المبلغ يتجاوز النطاق المسموح';
  if (paid > due) return 'مجموع وسائل الدفع يتجاوز إجمالي الفاتورة';
  if (paid < due && !hasCustomer) return 'وزع كامل قيمة الفاتورة أو حدد عميلاً للمبلغ الآجل';
  return null;
}

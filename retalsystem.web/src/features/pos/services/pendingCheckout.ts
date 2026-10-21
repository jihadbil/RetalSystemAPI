import { ApiError } from '../../../shared/services/apiClient';
import type { ReceiptData } from '../types/pos.types';
import type { CheckoutPayload } from './posService';

export interface PendingCheckout {
  version: 1;
  scope: string;
  request: CheckoutPayload & { invoiceNumber: string };
  receipt: ReceiptData;
}
const key = (scope: string) => `retal_pos_pending_checkout:${scope}`;

export function loadPendingCheckout(scope: string): PendingCheckout | null {
  const raw = localStorage.getItem(key(scope));
  if (!raw) return null;
  const value: PendingCheckout = JSON.parse(raw);
  if (value.version !== 1 || value.scope !== scope || !value.request?.invoiceNumber || !value.request.warehouseId || !value.receipt?.items?.length)
    throw new Error('تعذر قراءة محاولة البيع المعلقة؛ يجب مراجعة نتيجتها قبل بدء بيع جديد');
  return value;
}

export function savePendingCheckout(pending: PendingCheckout): void {
  const existing = loadPendingCheckout(pending.scope);
  if (existing && existing.request.invoiceNumber !== pending.request.invoiceNumber)
    throw new Error('توجد محاولة بيع معلقة؛ استعد نتيجتها أولاً');
  // Failure to persist stops the operation before any request reaches the server.
  localStorage.setItem(key(pending.scope), JSON.stringify(pending));
}

export async function settlePendingCheckout(
  pending: PendingCheckout,
  send: (payload: CheckoutPayload) => Promise<{ invoiceNumber: string; id: string }>,
  isNew: boolean,
): Promise<ReceiptData> {
  try {
    const result = await send(pending.request);
    if (!result.id) throw new Error('لم يؤكد الخادم معرّف الفاتورة؛ احتُفظ بمحاولة البيع للتحقق');
    if (loadPendingCheckout(pending.scope)?.request.invoiceNumber === pending.request.invoiceNumber)
      localStorage.removeItem(key(pending.scope));
    return { ...pending.receipt, invoiceNumber: result.invoiceNumber,
      qrCodeValue: `${result.invoiceNumber}|${pending.receipt.total.toFixed(2)}|${pending.receipt.date}` };
  } catch (error) {
    // Only the very first, definitively rejected request can be discarded.
    // A failed retry cannot prove that an earlier request did not commit.
    if (isNew && error instanceof ApiError && error.statusCode >= 400 && error.statusCode < 500 && error.statusCode !== 409)
      localStorage.removeItem(key(pending.scope));
    throw error;
  }
}

/** Serializes check-and-save and recovery across tabs of the same cashier session. */
export function withCheckoutLock<T>(scope: string, action: () => Promise<T>): Promise<T> {
  if (!navigator.locks) return Promise.reject(new Error('استخدم متصفحاً حديثاً واتصال HTTPS لحفظ محاولات البيع بأمان'));
  return navigator.locks.request(key(scope), action);
}

import type { SalesInvoiceDto } from '../../features/sales/types/sales.types';
import type { PurchaseInvoiceDto } from '../../features/purchase/types/purchase.types';

export interface ReturnLine {
  key: string;
  productId: string;
  productBarCodeId?: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

function groupLines(lines: Omit<ReturnLine, 'key'>[]): ReturnLine[] {
  const groups = new Map<string, ReturnLine>();
  for (const line of lines) {
    const key = `${line.productId}:${line.productBarCodeId || ''}`;
    const previous = groups.get(key);
    if (previous) {
      previous.unitPrice = (previous.unitPrice * previous.quantity + line.unitPrice * line.quantity) / (previous.quantity + line.quantity);
      previous.quantity += line.quantity;
    } else groups.set(key, { ...line, key });
  }
  return [...groups.values()].map((line) => ({ ...line, unitPrice: Number(line.unitPrice.toFixed(4)) }));
}

// Prices are estimates for display; the server values returns from the original document.
export function salesReturnLines(invoice: SalesInvoiceDto): ReturnLine[] {
  let weights = (invoice.items || []).map((item) => Math.max(0, item.quantity * item.unitPrice - item.discountAmount));
  if (weights.every((weight) => weight === 0)) weights = (invoice.items || []).map((item) => item.quantity);
  const sum = weights.reduce((a, b) => a + b, 0);
  return groupLines((invoice.items || []).map((item, index) => ({
    productId: item.productId, productBarCodeId: item.productBarCodeId,
    productName: `${item.productName}${item.barcodeTitle ? ` (${item.barcodeTitle})` : ''}`,
    quantity: item.quantity,
    unitPrice: item.quantity > 0 && sum > 0 ? weights[index] / sum * invoice.totalAmount / item.quantity : 0,
  })));
}

export function purchaseReturnLines(invoice: PurchaseInvoiceDto): ReturnLine[] {
  const lots = invoice.items.flatMap((item) => {
    const price = item.unitPrice ?? item.unitCost ?? 0;
    const amount = item.lineTotal ?? item.quantity * price - item.discountAmount;
    if (!item.breakdowns?.length) return [{ productId: item.productId, productName: item.productName,
      productBarCodeId: item.productBarCodeId, quantity: item.quantity, amount }];
    let weights = item.breakdowns.map((lot) => lot.quantity * (lot.unitPrice > 0 ? lot.unitPrice : price));
    if (weights.every((weight) => weight === 0)) weights = item.breakdowns.map((lot) => lot.quantity);
    const sum = weights.reduce((a, b) => a + b, 0);
    return item.breakdowns.map((lot, index) => ({ productId: item.productId, productName: item.productName,
      productBarCodeId: lot.productBarCodeId, quantity: lot.quantity, amount: sum > 0 ? weights[index] / sum * amount : 0 }));
  });
  const sum = lots.reduce((total, lot) => total + lot.amount, 0);
  const quantity = lots.reduce((total, lot) => total + lot.quantity, 0);
  return groupLines(lots.map((lot) => ({ ...lot, unitPrice: lot.quantity > 0
    ? (sum > 0 ? lot.amount / sum : quantity > 0 ? lot.quantity / quantity : 0) * invoice.totalAmount / lot.quantity : 0 })));
}

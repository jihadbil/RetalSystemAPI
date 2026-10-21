import type { CreatePurchaseInvoiceItemRequest, CreatePurchaseInvoiceRequest } from '../types/purchase.types';

function moneyUnits(value: number): bigint {
  if (!Number.isFinite(value) || value < 0 || value > Number.MAX_SAFE_INTEGER / 10000 || Math.abs(value - Number(value.toFixed(4))) > 1e-8)
    throw new Error('المبالغ يجب أن تكون غير سالبة وبأربع منازل عشرية كحد أقصى');
  return BigInt(value.toFixed(4).replace('.', ''));
}
const amount = (value: bigint) => Number(value) / 10000;

/** Allocates the indivisible 0.0001 remainder to whole units; never changes the package total. */
export function allocatePurchaseLine(item: CreatePurchaseInvoiceItemRequest): CreatePurchaseInvoiceRequest['items'] {
  const q = item.quantity;
  if (!Number.isInteger(q) || q <= 0 || q > 2147483647 || !Number.isInteger(item.unitsPerPackage) || item.unitsPerPackage <= 0)
    throw new Error('كمية العبوات يجب أن تمثل عدداً صحيحاً موجباً من أصغر وحدة');
  const packagePrice = moneyUnits(item.invoicePackagePrice);
  const packages = moneyUnits(item.packageQuantity);
  if (packages * BigInt(item.unitsPerPackage) !== BigInt(q) * 10000n)
    throw new Error('كمية العبوات يجب أن تمثل عدداً صحيحاً موجباً من أصغر وحدة');
  const gross = (packagePrice * packages + 5000n) / 10000n;
  const discount = moneyUnits(item.discountAmount);
  if (discount > gross) throw new Error('خصم البند يتجاوز قيمته');
  if (gross > BigInt(Number.MAX_SAFE_INTEGER)) throw new Error('قيمة البند تتجاوز الدقة الحسابية المدعومة');
  const sources = item.breakdowns?.length
    ? item.breakdowns.map((b) => ({ barcodeId: b.productBarCodeId, quantity: b.quantity }))
    : [{ barcodeId: item.productBarCodeId, quantity: q }];
  if (sources.some((s) => !Number.isInteger(s.quantity) || s.quantity <= 0) || sources.reduce((sum, s) => sum + s.quantity, 0) !== q)
    throw new Error('مجموع كميات النكهات يجب أن يساوي كمية البند');
  const low = gross / BigInt(q);
  let extra = Number(gross % BigInt(q));
  const lines: CreatePurchaseInvoiceRequest['items'] = [];
  let allocatedGross = 0n;
  let allocatedDiscount = 0n;
  for (const source of sources) {
    const highQty = Math.min(source.quantity, extra);
    extra -= highQty;
    for (const [quantity, price] of [[highQty, low + 1n], [source.quantity - highQty, low]] as const) {
      if (!quantity) continue;
      allocatedGross += BigInt(quantity) * price;
      const cumulativeDiscount = gross === 0n ? 0n : discount * allocatedGross / gross;
      const lineDiscount = cumulativeDiscount - allocatedDiscount;
      allocatedDiscount = cumulativeDiscount;
      lines.push({
        productId: item.productId, productBarCodeId: source.barcodeId,
        quantity, unitPrice: amount(price), discountAmount: amount(lineDiscount),
        breakdowns: source.barcodeId ? [{ productBarCodeId: source.barcodeId, packageQuantity: quantity, unitsPerPackage: 1, quantity, unitPrice: amount(price) }] : undefined,
      });
    }
  }
  return lines;
}

export function purchaseLineTotal(item: CreatePurchaseInvoiceItemRequest): number {
  return amount(allocatePurchaseLine(item).reduce((sum, line) => sum + moneyUnits(line.unitPrice) * BigInt(line.quantity) - moneyUnits(line.discountAmount), 0n));
}

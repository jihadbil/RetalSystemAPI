// أنواع ونماذج المبيعات وفواتير البيع والمرتجعات

export enum InvoiceStatus {
  Draft = 0,
  Issued = 1,
  Paid = 2,
  PartiallyPaid = 3,
  Cancelled = 4,
  Voided = 5,
}

export const InvoiceStatusLabels: Record<InvoiceStatus, { text: string; color: string; bg: string }> = {
  [InvoiceStatus.Draft]: { text: 'مسودة', color: 'text-slate-700', bg: 'bg-slate-100' },
  [InvoiceStatus.Issued]: { text: 'مصدرة', color: 'text-sky-700', bg: 'bg-sky-50' },
  [InvoiceStatus.Paid]: { text: 'مدفوعة بالكامل', color: 'text-emerald-700', bg: 'bg-emerald-50' },
  [InvoiceStatus.PartiallyPaid]: { text: 'مدفوعة جزئياً', color: 'text-amber-700', bg: 'bg-amber-50' },
  [InvoiceStatus.Cancelled]: { text: 'ملغية', color: 'text-rose-700', bg: 'bg-rose-50' },
  [InvoiceStatus.Voided]: { text: 'باطلة / مسترجعة', color: 'text-purple-700', bg: 'bg-purple-50' },
};

export interface SalesInvoiceItemDto {
  id?: string;
  productId: string;
  productName: string;
  code?: string;
  unitName?: string;
  quantity: number;
  unitPrice: number;
  unitCost?: number;
  discountAmount: number;
  taxAmount: number;
  totalPrice: number;
}

export interface SalesInvoiceDto {
  id: string;
  invoiceNumber: string;
  branchId: string;
  branchName?: string;
  warehouseId: string;
  warehouseName?: string;
  customerId?: string;
  customerName?: string;
  invoiceDate: string;
  dueDate?: string;
  status: InvoiceStatus;
  paymentMethod: number;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  notes?: string;
  items: SalesInvoiceItemDto[];
}

export interface SalesReturnItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  reason?: string;
}

export interface SalesReturnDto {
  id: string;
  returnNumber: string;
  originalInvoiceId?: string;
  originalInvoiceNumber?: string;
  customerId?: string;
  customerName?: string;
  returnDate: string;
  totalAmount: number;
  refundMethod: number;
  notes?: string;
  items: SalesReturnItemDto[];
}

// أنواع ونماذج المبيعات وفواتير البيع والمرتجعات

export enum InvoiceStatus {
  Draft = 0,
  Issued = 1,
  Paid = 2,
  PartiallyPaid = 3,
  Cancelled = 4,
  Voided = 5,
}

export enum SalesPaymentMethod {
  Cash = 1,
  BankTransfer = 2,
  CreditCard = 3,
  Credit = 4,
  Cheque = 5,
}

export const InvoiceStatusLabels: Record<InvoiceStatus, { text: string; color: string; bg: string }> = {
  [InvoiceStatus.Draft]: { text: 'مسودة', color: 'text-slate-700', bg: 'bg-slate-100' },
  [InvoiceStatus.Issued]: { text: 'مصدرة', color: 'text-sky-700', bg: 'bg-sky-50' },
  [InvoiceStatus.Paid]: { text: 'مدفوعة ومكتملة', color: 'text-emerald-700', bg: 'bg-emerald-50' },
  [InvoiceStatus.PartiallyPaid]: { text: 'مدفوعة جزئياً', color: 'text-amber-700', bg: 'bg-amber-50' },
  [InvoiceStatus.Cancelled]: { text: 'ملغية', color: 'text-rose-700', bg: 'bg-rose-50' },
  [InvoiceStatus.Voided]: { text: 'باطلة / مسترجعة', color: 'text-purple-700', bg: 'bg-purple-50' },
};

export interface CreateSalesInvoiceItemRequest {
  productId: string;
  productName: string;
  productCode?: string;
  productBarCodeId?: string;
  barCode?: string;
  quantity: number;
  unitPrice: number;
  unitCost?: number;
  discountAmount: number;
  lineTotal: number;
}

export interface CreateSalesInvoiceRequest {
  invoiceNumber: string;
  invoiceDate: string;
  branchId: string;
  warehouseId: string;
  customerId?: string;
  status: InvoiceStatus;
  paymentMethod: number;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  paymentSafeId?: string;
  notes?: string;
  items: {
    productId: string;
    productBarCodeId?: string;
    quantity: number;
    unitPrice: number;
    discountAmount: number;
  }[];
}

export interface SalesInvoiceItemDto {
  id?: string;
  productId: string;
  productName: string;
  code?: string;
  unitName?: string;
  barcodeValue?: string;
  barcodeTitle?: string;
  quantity: number;
  unitPrice: number;
  unitCost?: number;
  discountAmount: number;
  taxAmount?: number;
  lineTotal?: number;
  totalPrice?: number;
}

export interface SalesInvoiceDto {
  id: string;
  invoiceNumber: string;
  branchId?: string;
  branchName?: string;
  warehouseId?: string;
  warehouseName?: string;
  customerId?: string;
  customerName?: string;
  invoiceDate: string;
  dueDate?: string;
  status: InvoiceStatus;
  statusName?: string;
  paymentMethod: number;
  paymentMethodName?: string;
  subTotal?: number;
  discountAmount?: number;
  taxAmount?: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentSafeId?: string;
  notes?: string;
  itemCount?: number;
  items?: SalesInvoiceItemDto[];
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

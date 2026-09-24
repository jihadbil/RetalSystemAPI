// أنواع ونماذج فواتير وأوامر ومرتجعات الشراء

export enum PurchaseInvoiceStatus {
  Draft = 0,
  Pending = 1,
  Received = 2,
  Paid = 3,
  Cancelled = 4,
}

export const PurchaseStatusLabels: Record<PurchaseInvoiceStatus, { text: string; color: string; bg: string }> = {
  [PurchaseInvoiceStatus.Draft]: { text: 'مسودة', color: 'text-slate-700', bg: 'bg-slate-100' },
  [PurchaseInvoiceStatus.Pending]: { text: 'قيد الاستلام', color: 'text-amber-700', bg: 'bg-amber-50' },
  [PurchaseInvoiceStatus.Received]: { text: 'مستلمة', color: 'text-emerald-700', bg: 'bg-emerald-50' },
  [PurchaseInvoiceStatus.Paid]: { text: 'مسددة بالكامل', color: 'text-sky-700', bg: 'bg-sky-50' },
  [PurchaseInvoiceStatus.Cancelled]: { text: 'ملغية', color: 'text-rose-700', bg: 'bg-rose-50' },
};

export interface PurchaseInvoiceItemDto {
  id?: string;
  productId: string;
  productName: string;
  code?: string;
  unitName?: string;
  quantity: number;
  unitCost: number;
  discountAmount: number;
  taxAmount: number;
  totalCost: number;
}

export interface PurchaseInvoiceDto {
  id: string;
  invoiceNumber: string;
  supplierInvoiceNumber?: string;
  branchId: string;
  branchName?: string;
  warehouseId: string;
  warehouseName?: string;
  supplierId?: string;
  supplierName?: string;
  invoiceDate: string;
  dueDate?: string;
  status: PurchaseInvoiceStatus;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  notes?: string;
  items: PurchaseInvoiceItemDto[];
}

export interface PurchaseOrderDto {
  id: string;
  orderNumber: string;
  supplierId: string;
  supplierName?: string;
  branchId: string;
  branchName?: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  status: string;
  totalAmount: number;
  notes?: string;
  items: {
    productId: string;
    productName: string;
    quantity: number;
    unitCost: number;
    totalCost: number;
  }[];
}

export interface PurchaseReturnDto {
  id: string;
  returnNumber: string;
  purchaseInvoiceId?: string;
  supplierId: string;
  supplierName?: string;
  warehouseId: string;
  returnDate: string;
  totalAmount: number;
  notes?: string;
  items: {
    productId: string;
    productName: string;
    quantity: number;
    unitCost: number;
    totalCost: number;
    reason?: string;
  }[];
}

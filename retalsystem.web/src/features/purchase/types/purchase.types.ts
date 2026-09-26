// أنواع ونماذج فواتير وأوامر ومرتجعات الشراء

export enum PurchaseInvoiceStatus {
  Draft = 0,
  Pending = 1,
  Paid = 2,
  PartiallyPaid = 3,
  Cancelled = 4,
  Voided = 5,
}

export enum PurchasePaymentMethod {
  Cash = 1,
  BankTransfer = 2,
  CreditCard = 3,
  Credit = 4,
  Cheque = 5,
}

export const PurchaseStatusLabels: Record<PurchaseInvoiceStatus, { text: string; color: string; bg: string }> = {
  [PurchaseInvoiceStatus.Draft]: { text: 'مسودة', color: 'text-slate-700', bg: 'bg-slate-100' },
  [PurchaseInvoiceStatus.Pending]: { text: 'قيد الإدخال', color: 'text-amber-700', bg: 'bg-amber-50' },
  [PurchaseInvoiceStatus.Paid]: { text: 'مغلقة ومرحلة', color: 'text-emerald-700', bg: 'bg-emerald-50' },
  [PurchaseInvoiceStatus.PartiallyPaid]: { text: 'مسددة جزئياً', color: 'text-sky-700', bg: 'bg-sky-50' },
  [PurchaseInvoiceStatus.Cancelled]: { text: 'ملغاة', color: 'text-rose-700', bg: 'bg-rose-50' },
  [PurchaseInvoiceStatus.Voided]: { text: 'مبطلة', color: 'text-slate-500', bg: 'bg-slate-50' },
};

export interface PackagingUnitOption {
  name: string;
  conversionFactor: number;
  displayText: string;
}

export interface FlavorBreakdownItem {
  productBarCodeId: string;
  barCode: string;
  title: string;
  packageQuantity: number;
  unitsPerPackage: number;
  totalPieces: number;
  unitPrice: number;
  totalCost: number;
}

export interface CreatePurchaseInvoiceItemRequest {
  productId: string;
  productName: string;
  productBarCodeId?: string;
  barCode?: string;
  packageUnitName: string;
  packageQuantity: number;
  unitsPerPackage: number;
  invoicePackagePrice: number;
  quantity: number;
  unitPrice: number;
  salePrice: number;
  discountAmount: number;
  lineTotal: number;
  profitPerPiece: number;
  profitMarginPercentage: number;
  breakdowns?: {
    productBarCodeId: string;
    packageQuantity: number;
    unitsPerPackage: number;
    quantity: number;
    unitPrice: number;
  }[];
  flavorsSummaryText?: string;
  packagingSummary?: string;
}

export interface CreatePurchaseInvoiceRequest {
  invoiceNumber: string;
  invoiceDate: string;
  supplierId: string;
  branchId: string;
  warehouseId: string;
  status: PurchaseInvoiceStatus;
  paymentMethod: number;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  paymentSafeId?: string;
  purchaseOrderId?: string;
  notes?: string;
  items: {
    productId: string;
    productBarCodeId?: string;
    quantity: number;
    unitPrice: number;
    discountAmount: number;
    breakdowns?: {
      productBarCodeId: string;
      packageQuantity: number;
      unitsPerPackage: number;
      quantity: number;
      unitPrice: number;
    }[];
  }[];
}

export interface PurchaseInvoiceItemDto {
  id?: string;
  productId: string;
  productName: string;
  code?: string;
  unitName?: string;
  quantity: number;
  unitCost: number;
  discountAmount: number;
  taxAmount?: number;
  totalCost?: number;
  lineTotal?: number;
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
  paymentMethod?: number;
  paymentSafeId?: string;
  subTotal: number;
  discountAmount: number;
  taxAmount: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  notes?: string;
  items: PurchaseInvoiceItemDto[];
}

export interface CreatePurchaseOrderItemRequest {
  productBarCodeId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreatePurchaseOrderRequest {
  orderNumber: string;
  branchId: string;
  supplierId?: string;
  warehouseId?: string;
  orderDate: string;
  expectedDate?: string;
  items: CreatePurchaseOrderItemRequest[];
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

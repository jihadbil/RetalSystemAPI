// أنواع ونماذج نقطة البيع السحابية السريعة (POS)

export enum PaymentMethod {
  Cash = 1,
  BankTransfer = 2,
  Card = 3,
  Credit = 4,
  Cheque = 5,
  Multiple = 6
}

export const PaymentMethodLabels: Record<PaymentMethod, string> = {
  [PaymentMethod.Cash]: 'نقداً',
  [PaymentMethod.Card]: 'بطاقة مصرفية',
  [PaymentMethod.BankTransfer]: 'تحويل بنكي',
  [PaymentMethod.Credit]: 'آجل (على الحساب)',
  [PaymentMethod.Cheque]: 'صك',
  [PaymentMethod.Multiple]: 'دفع مجزأ / مركب',
};

export interface CartItem {
  productId: string;
  productName: string;
  code?: string;
  productBarCodeId?: string;
  barCode?: string;
  imageUrl?: string;
  unitName?: string;
  unitPrice: number;
  quantity: number;
  discountAmount: number;
  lineTotal: number;
}

export interface PosDraft {
  id: string;
  savedAt: string;
  branchId?: string;
  warehouseId?: string;
  customerId?: string;
  customerName?: string;
  items: CartItem[];
  discount: number;
  tax: number;
  paid: number;
  paymentMethod: PaymentMethod;
  notes: string;
  total: number;
  label: string;
}

export interface QuickProductItem {
  productId: string;
  name: string;
  code: string;
  price: number;
  imageUrl?: string;
  categoryName?: string;
  barCode?: string;
}

export interface ReceiptData {
  invoiceNumber: string;
  date: string;
  cashierName: string;
  customerName?: string;
  branchName?: string;
  tenantName?: string;
  taxNumber?: string;
  items: {
    name: string;
    quantity: number;
    price: number;
    total: number;
  }[];
  subtotal: number;
  discount: number;
  tax: number;
  total: number;
  paid: number;
  change: number;
  paymentMethod: string;
  qrCodeValue?: string;
}

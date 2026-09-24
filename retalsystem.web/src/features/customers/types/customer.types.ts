// أنواع ونماذج العملاء وسندات القبض وكشوفات الحساب

export interface CustomerDto {
  id: string;
  name: string;
  phone: string;
  email?: string;
  address?: string;
  taxNumber?: string;
  commercialRegister?: string;
  creditLimit: number;
  currentBalance: number; // مدين إذا كان موجباً
  discountRate: number;
  isActive: boolean;
  notes?: string;
}

export interface CustomerPaymentDto {
  id: string;
  paymentNumber: string;
  customerId: string;
  customerName?: string;
  safeId: string;
  safeName?: string;
  amount: number;
  paymentDate: string;
  paymentMethod: number;
  notes?: string;
}

export interface CustomerStatementTransaction {
  transactionDate: string;
  documentType: number;
  documentTypeName: string;
  documentId?: string;
  description?: string;
  debitAmount: number;
  creditAmount: number;
  balanceAfter: number;
}

export interface CustomerStatementDto {
  customerId: string;
  customerName: string;
  fromDate?: string;
  toDate?: string;
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  currentBalance: number;
  transactions: CustomerStatementTransaction[];
}

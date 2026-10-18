// أنواع ونماذج الموردين وسندات الصرف وكشوفات الحساب

export interface SupplierDto {
  phones?: { phoneNumber: string; name?: string }[];
  id: string;
  name: string;
  companyName?: string;
  phone: string;
  email?: string;
  address?: string;
  taxNumber?: string;
  currentBalance: number; // دائن للمورد إذا كان موجباً
  isActive: boolean;
  notes?: string;
}

export interface SupplierPaymentDto {
  id: string;
  paymentNumber: string;
  supplierId: string;
  supplierName?: string;
  safeId: string;
  safeName?: string;
  amount: number;
  paymentDate: string;
  paymentMethod: number;
  notes?: string;
}

export interface SupplierStatementTransaction {
  transactionDate: string;
  documentType: number;
  documentTypeName: string;
  documentId?: string;
  description?: string;
  debitAmount: number;
  creditAmount: number;
  balanceAfter: number;
}

export interface SupplierStatementDto {
  supplierId: string;
  supplierName: string;
  fromDate?: string;
  toDate?: string;
  openingBalance: number;
  totalDebit: number;
  totalCredit: number;
  currentBalance: number;
  transactions: SupplierStatementTransaction[];
}

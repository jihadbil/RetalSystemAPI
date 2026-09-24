// أنواع ونماذج الخزائن والحسابات المصرفية والمصروفات

export enum SafeType {
  Bank = 1,       // حساب بنكي
  CashBox = 2,    // خزينة نقدية
  CashDrawer = 3, // درج كاشير
}

export const SafeTypeLabels: Record<SafeType, string> = {
  [SafeType.Bank]: 'حساب بنكي',
  [SafeType.CashBox]: 'خزينة رئيسية',
  [SafeType.CashDrawer]: 'درج كاشير',
};

export interface SafeDto {
  id: string;
  name: string;
  type: SafeType;
  typeName?: string;
  branchId: string;
  branchName?: string;
  openingBalance: number;
  currentBalance: number;
  bankAccountNumber?: string;
  bankName?: string;
  isActive: boolean;
  notes?: string;
}

export enum TransactionDirection {
  Inbound = 1,  // إيداع / وارد
  Outbound = 2, // سحب / صادر
}

export interface SafeTransactionDto {
  id: string;
  safeId: string;
  safeName?: string;
  direction: TransactionDirection;
  amount: number;
  balanceAfter: number;
  transactionDate: string;
  transactionNumber: string;
  description?: string;
  documentType: number;
  documentTypeName: string;
  documentId?: string;
}

export interface SafeTransferDto {
  id: string;
  transferNumber: string;
  sourceSafeId: string;
  sourceSafeName?: string;
  destinationSafeId: string;
  destinationSafeName?: string;
  amount: number;
  transferDate: string;
  notes?: string;
}

export interface ExpenseCategoryDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

export interface ExpenseDto {
  id: string;
  expenseNumber: string;
  expenseCategoryName: string;
  safeId?: string;
  safeName?: string;
  amount: number;
  expenseDate: string;
  paymentMethodName: string;
  beneficiaryName?: string;
  description: string;
  branchName: string;
  isCancelled: boolean;
}

export interface PosTerminalDto {
  id: string;
  code: string;
  name: string;
  branchId: string;
  branchName: string;
  defaultCashDrawerId: string;
  defaultCashDrawerName: string;
  isActive: boolean;
  lastSeenAt?: string;
}

export interface PairingCodeDto {
  tenantId: string;
  terminalId: string;
  pairingCode: string;
  expiresAt: string;
}

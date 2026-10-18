import { apiClient } from '../../../shared/services/apiClient';
import type {
  SafeDto,
  SafeTransactionDto,
  ExpenseCategoryDto,
  ExpenseDto,
  PosTerminalDto,
  PairingCodeDto,
} from '../types/accounting.types';
import { SafeType, TransactionDirection } from '../types/accounting.types';
import type { PagedResult } from '../../../shared/types/api.types';

export interface CreateSafeRequest {
  name: string;
  type: SafeType;
  branchId: string;
  openingBalance: number;
  notes?: string;
  accountNumber?: string;
  bankAccountNumber?: string;
  bankName?: string;
  iban?: string;
  isActive?: boolean;
}

export interface UpdateSafeRequest {
  id?: string;
  name?: string;
  notes?: string;
  isActive?: boolean;
  type?: SafeType;
  branchId?: string;
  accountNumber?: string;
  bankAccountNumber?: string;
  bankName?: string;
  iban?: string;
}

export interface TransferFundsRequest {
  fromSafeId?: string;
  toSafeId?: string;
  sourceSafeId?: string;
  destinationSafeId?: string;
  amount: number;
  description?: string;
  notes?: string;
  transferDate?: string;
  referenceNumber?: string;
}

export interface CreateSafeTransactionRequest {
  safeId: string;
  amount: number;
  direction: TransactionDirection;
  documentType?: string;
  description?: string;
  transactionDate?: string;
  notes?: string;
  referenceNumber?: string;
}

export interface CreateExpenseRequest {
  expenseCategoryId?: string;
  categoryId?: string;
  safeId?: string;
  branchId?: string;
  amount: number;
  expenseDate?: string;
  paymentMethod?: number;
  description?: string;
  beneficiaryName?: string;
  notes?: string;
  taxInvoiceNumber?: string;
}

export const accountingService = {
  getPosTerminals(): Promise<PosTerminalDto[]> {
    return apiClient.get<PosTerminalDto[]>('pos-terminals');
  },

  createPosTerminal(data: { code: string; name: string; branchId: string; defaultCashDrawerId: string }): Promise<PosTerminalDto> {
    return apiClient.post<PosTerminalDto>('pos-terminals', data);
  },

  generatePairingCode(id: string): Promise<PairingCodeDto> {
    return apiClient.post<PairingCodeDto>(`pos-terminals/${id}/pairing-code`, {});
  },

  // --- الخزائن ---
  async getSafes(branchId?: string, type?: SafeType, isActive?: boolean): Promise<SafeDto[]> {
    try {
      const params: Record<string, string | number> = {};
      if (branchId) params.branchId = branchId;
      if (type !== undefined) params.type = type;
      if (isActive !== undefined) params.isActive = String(isActive);

      return await apiClient.get<SafeDto[]>('safes', params);
    } catch (err) {
      console.error('[accountingService.getSafes] Failed to fetch safes:', err);
      throw err;
    }
  },

  async createSafe(data: CreateSafeRequest): Promise<SafeDto> {
    try {
      return await apiClient.post<SafeDto>('safes', data);
    } catch (err) {
      console.error('[accountingService.createSafe] Failed to create safe:', err);
      throw err;
    }
  },

  async updateSafe(id: string, data: UpdateSafeRequest): Promise<SafeDto> {
    try {
      return await apiClient.put<SafeDto>(`safes/${id}`, data);
    } catch (err) {
      console.error(`[accountingService.updateSafe] Failed to update safe ${id}:`, err);
      throw err;
    }
  },

  async deleteSafe(id: string): Promise<void> {
    try {
      await apiClient.delete(`safes/${id}`);
    } catch (err) {
      console.error(`[accountingService.deleteSafe] Failed to delete safe ${id}:`, err);
      throw err;
    }
  },

  async toggleSafeActive(id: string, currentActive: boolean): Promise<void> {
    try {
      void currentActive;
      await apiClient.patch(`safes/${id}/toggle-active`);
    } catch (err) {
      console.error(`[accountingService.toggleSafeActive] Failed to toggle safe ${id}:`, err);
      throw err;
    }
  },

  // --- التحويل المالي وحركات الخزينة ---
  async transferFunds(request: TransferFundsRequest): Promise<void> {
    try {
      const payload = {
        fromSafeId: request.fromSafeId || request.sourceSafeId,
        toSafeId: request.toSafeId || request.destinationSafeId,
        amount: request.amount,
        description: request.description || request.notes || 'تحويل مالي بين الخزائن',
        notes: request.notes,
      };
      await apiClient.post('safes/transfer', payload);
    } catch (err) {
      console.error('[accountingService.transferFunds] Failed to transfer funds:', err);
      throw err;
    }
  },

  async createTransaction(request: CreateSafeTransactionRequest): Promise<void> {
    try {
      await apiClient.post('safes/transactions', request);
    } catch (err) {
      console.error('[accountingService.createTransaction] Failed to create transaction:', err);
      throw err;
    }
  },

  async getSafeTransactions(
    safeId: string,
    pageNumber = 1,
    pageSize = 20
  ): Promise<PagedResult<SafeTransactionDto>> {
    try {
      return await apiClient.get<PagedResult<SafeTransactionDto>>('safes/transactions', {
        safeId,
        pageNumber,
        pageSize,
      });
    } catch (err) {
      console.error(`[accountingService.getSafeTransactions] Failed for safe ${safeId}:`, err);
      throw err;
    }
  },

  // --- تصنيفات المصروفات ---
  async getExpenseCategories(): Promise<ExpenseCategoryDto[]> {
    try {
      return await apiClient.get<ExpenseCategoryDto[]>('expense-categories');
    } catch (err) {
      console.error('[accountingService.getExpenseCategories] Failed to fetch expense categories:', err);
      throw err;
    }
  },

  async createExpenseCategory(data: { name: string; description?: string }): Promise<ExpenseCategoryDto> {
    try {
      return await apiClient.post<ExpenseCategoryDto>('expense-categories', data);
    } catch (err) {
      console.error('[accountingService.createExpenseCategory] Failed to create category:', err);
      throw err;
    }
  },

  async deleteExpenseCategory(id: string): Promise<void> {
    try {
      await apiClient.delete(`expense-categories/${id}`);
    } catch (err) {
      console.error(`[accountingService.deleteExpenseCategory] Failed to delete category ${id}:`, err);
      throw err;
    }
  },

  // --- سندات المصروفات ---
  async getExpenses(
    pageNumber = 1,
    pageSize = 10,
    categoryId?: string,
    safeId?: string,
    search?: string,
    branchId?: string
  ): Promise<PagedResult<ExpenseDto>> {
    try {
      const params: Record<string, string | number> = { pageNumber, pageSize };
      if (categoryId) params.expenseCategoryId = categoryId;
      if (safeId) params.safeId = safeId;
      if (search) params.searchTerm = search;
      if (branchId) params.branchId = branchId;

      return await apiClient.get<PagedResult<ExpenseDto>>('expenses', params);
    } catch (err) {
      console.error('[accountingService.getExpenses] Failed to fetch expenses:', err);
      throw err;
    }
  },

  async createExpense(data: CreateExpenseRequest): Promise<ExpenseDto> {
    try {
      const branchId = data.branchId || localStorage.getItem('retal_branch_id') || undefined;
      const payload = {
        expenseCategoryId: data.expenseCategoryId || data.categoryId,
        branchId,
        safeId: data.safeId || undefined,
        amount: data.amount,
        expenseDate: data.expenseDate || new Date().toISOString(),
        paymentMethod: data.paymentMethod || 1, // Cash
        description: data.description || data.notes || 'سند مصروف تشغيلي',
        beneficiaryName: data.beneficiaryName,
        notes: data.notes,
      };
      return await apiClient.post<ExpenseDto>('expenses', payload);
    } catch (err) {
      console.error('[accountingService.createExpense] Failed to create expense:', err);
      throw err;
    }
  },

  async deleteExpense(id: string): Promise<void> {
    try {
      await apiClient.delete(`expenses/${id}`);
    } catch (err) {
      console.error(`[accountingService.deleteExpense] Failed to delete expense ${id}:`, err);
      throw err;
    }
  },
};

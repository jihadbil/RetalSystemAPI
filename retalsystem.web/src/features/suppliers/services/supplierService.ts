import { apiClient } from '../../../shared/services/apiClient';
import type { SupplierDto, SupplierStatementDto } from '../types/supplier.types';
import type { PagedResult } from '../../../shared/types/api.types';

export interface CreateSupplierRequest {
  name: string;
  companyName?: string;
  phone: string;
  email?: string;
  address?: string;
  taxNumber?: string;
  notes?: string;
  isActive: boolean;
}

export interface UpdateSupplierRequest extends CreateSupplierRequest {
  id: string;
}

export interface CreateSupplierPaymentRequest {
  amount: number;
  safeId: string;
  branchId?: string;
  paymentDate: string;
  paymentMethod: number; // 1=Cash, 2=Bank, 3=Card
  notes?: string;
}

export interface CreateSupplierAdjustmentRequest {
  amount: number;
  type: number; // 1=Debit, 2=Credit
  notes?: string;
}

export const supplierService = {
  async getAll(search?: string): Promise<SupplierDto[]> {
    try {
      const params: Record<string, string> = {};
      if (search) params.search = search;

      return await apiClient.get<SupplierDto[]>('suppliers', params);
    } catch (err) {
      console.error('[supplierService.getAll] Failed to fetch suppliers:', err);
      throw err;
    }
  },

  async getPaged(
    pageNumber = 1,
    pageSize = 10,
    search?: string
  ): Promise<PagedResult<SupplierDto>> {
    try {
      const params: Record<string, string | number> = { pageNumber, pageSize };
      if (search) params.search = search;

      return await apiClient.get<PagedResult<SupplierDto>>('suppliers/paged', params);
    } catch (err) {
      console.error('[supplierService.getPaged] Failed to fetch paged suppliers:', err);
      throw err;
    }
  },

  async getById(id: string): Promise<SupplierDto> {
    try {
      return await apiClient.get<SupplierDto>(`suppliers/${id}`);
    } catch (err) {
      console.error(`[supplierService.getById] Failed to fetch supplier ${id}:`, err);
      throw err;
    }
  },

  async create(data: CreateSupplierRequest): Promise<SupplierDto> {
    try {
      return await apiClient.post<SupplierDto>('suppliers', data);
    } catch (err) {
      console.error('[supplierService.create] Failed to create supplier:', err);
      throw err;
    }
  },

  async update(id: string, data: UpdateSupplierRequest): Promise<SupplierDto> {
    try {
      return await apiClient.put<SupplierDto>(`suppliers/${id}`, data);
    } catch (err) {
      console.error(`[supplierService.update] Failed to update supplier ${id}:`, err);
      throw err;
    }
  },

  async delete(id: string): Promise<void> {
    try {
      await apiClient.delete(`suppliers/${id}`);
    } catch (err) {
      console.error(`[supplierService.delete] Failed to delete supplier ${id}:`, err);
      throw err;
    }
  },

  async toggleActive(id: string): Promise<SupplierDto> {
    try {
      return await apiClient.patch<SupplierDto>(`suppliers/${id}/toggle-active`);
    } catch (err) {
      console.error(`[supplierService.toggleActive] Failed to toggle supplier ${id}:`, err);
      throw err;
    }
  },

  async addPayment(supplierId: string, request: CreateSupplierPaymentRequest): Promise<void> {
    try {
      const branchId = request.branchId || localStorage.getItem('retal_branch_id') || undefined;
      await apiClient.post('suppliers/payments', {
        supplierId,
        amount: request.amount,
        safeId: request.safeId || undefined,
        branchId,
        paymentDate: request.paymentDate || new Date().toISOString(),
        paymentMethod: request.paymentMethod || 1,
        notes: request.notes,
      });
    } catch (err) {
      console.error(`[supplierService.addPayment] Failed for supplier ${supplierId}:`, err);
      throw err;
    }
  },

  async getStatement(
    supplierId: string,
    fromDate?: string,
    toDate?: string
  ): Promise<SupplierStatementDto> {
    try {
      const params: Record<string, string> = {};
      if (fromDate) params.fromDate = fromDate;
      if (toDate) params.toDate = toDate;

      return await apiClient.get<SupplierStatementDto>(`suppliers/${supplierId}/statement`, params);
    } catch (err) {
      console.error(`[supplierService.getStatement] Failed for supplier ${supplierId}:`, err);
      throw err;
    }
  },
};

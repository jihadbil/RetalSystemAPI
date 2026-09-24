import { apiClient } from '../../../shared/services/apiClient';
import type { CustomerDto, CustomerStatementDto } from '../types/customer.types';
import type { PagedResult } from '../../../shared/types/api.types';

export interface CreateCustomerRequest {
  name: string;
  phone: string;
  email?: string;
  address?: string;
  taxNumber?: string;
  commercialRegister?: string;
  creditLimit: number;
  discountRate: number;
  notes?: string;
  isActive: boolean;
}

export interface UpdateCustomerRequest extends CreateCustomerRequest {
  id: string;
}

export interface CreateCustomerPaymentRequest {
  amount: number;
  safeId: string;
  branchId?: string;
  paymentDate: string;
  paymentMethod: number; // 1=Cash, 2=Bank, 3=Card
  notes?: string;
}

export interface CreateCustomerAdjustmentRequest {
  amount: number;
  type: number; // 1=Debit, 2=Credit
  notes?: string;
}

export const customerService = {
  async getAll(search?: string, isActive?: boolean): Promise<CustomerDto[]> {
    try {
      const params: Record<string, string> = {};
      if (search) params.search = search;
      if (isActive !== undefined) params.isActive = String(isActive);

      return await apiClient.get<CustomerDto[]>('customers', params);
    } catch (err) {
      console.error('[customerService.getAll] Failed to fetch customers:', err);
      throw err;
    }
  },

  async getPaged(
    pageNumber = 1,
    pageSize = 10,
    search?: string,
    isActive?: boolean
  ): Promise<PagedResult<CustomerDto>> {
    try {
      const params: Record<string, string | number> = { pageNumber, pageSize };
      if (search) params.search = search;
      if (isActive !== undefined) params.isActive = String(isActive);

      return await apiClient.get<PagedResult<CustomerDto>>('customers/paged', params);
    } catch (err) {
      console.error('[customerService.getPaged] Failed to fetch paged customers:', err);
      throw err;
    }
  },

  async getById(id: string): Promise<CustomerDto> {
    try {
      return await apiClient.get<CustomerDto>(`customers/${id}`);
    } catch (err) {
      console.error(`[customerService.getById] Failed to fetch customer ${id}:`, err);
      throw err;
    }
  },

  async create(data: CreateCustomerRequest): Promise<CustomerDto> {
    try {
      return await apiClient.post<CustomerDto>('customers', data);
    } catch (err) {
      console.error('[customerService.create] Failed to create customer:', err);
      throw err;
    }
  },

  async update(id: string, data: UpdateCustomerRequest): Promise<CustomerDto> {
    try {
      return await apiClient.put<CustomerDto>(`customers/${id}`, data);
    } catch (err) {
      console.error(`[customerService.update] Failed to update customer ${id}:`, err);
      throw err;
    }
  },

  async delete(id: string): Promise<void> {
    try {
      await apiClient.delete(`customers/${id}`);
    } catch (err) {
      console.error(`[customerService.delete] Failed to delete customer ${id}:`, err);
      throw err;
    }
  },

  async toggleActive(id: string): Promise<CustomerDto> {
    try {
      return await apiClient.patch<CustomerDto>(`customers/${id}/toggle-active`);
    } catch (err) {
      console.error(`[customerService.toggleActive] Failed to toggle customer ${id}:`, err);
      throw err;
    }
  },

  async addPayment(customerId: string, request: CreateCustomerPaymentRequest): Promise<void> {
    try {
      const branchId = request.branchId || localStorage.getItem('retal_branch_id') || undefined;
      await apiClient.post('customers/payments', {
        customerId,
        amount: request.amount,
        safeId: request.safeId || undefined,
        branchId,
        paymentDate: request.paymentDate || new Date().toISOString(),
        paymentMethod: request.paymentMethod || 1,
        notes: request.notes,
      });
    } catch (err) {
      console.error(`[customerService.addPayment] Failed for customer ${customerId}:`, err);
      throw err;
    }
  },

  async getStatement(
    customerId: string,
    fromDate?: string,
    toDate?: string
  ): Promise<CustomerStatementDto> {
    try {
      const params: Record<string, string> = {};
      if (fromDate) params.fromDate = fromDate;
      if (toDate) params.toDate = toDate;

      return await apiClient.get<CustomerStatementDto>(`customers/${customerId}/statement`, params);
    } catch (err) {
      console.error(`[customerService.getStatement] Failed for customer ${customerId}:`, err);
      throw err;
    }
  },
};

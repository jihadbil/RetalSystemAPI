import { apiClient } from '../../../shared/services/apiClient';
import type { CustomerDto, CustomerStatementDto } from '../types/customer.types';
import type { PagedResult } from '../../../shared/types/api.types';

export interface CreateCustomerRequest {
  code?: string;
  type?: number;
  phones?: CustomerDto['phones'];
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

interface CustomerApiDto extends CustomerDto { primaryPhone?: string }

export const normalizeCustomer = (data: CustomerApiDto): CustomerDto => ({
  ...data,
  phone: data.primaryPhone || data.phones?.find((p) => p.isDefault)?.phoneNumber || data.phones?.[0]?.phoneNumber || data.phone || '',
});

export function buildCustomerPayload(data: CreateCustomerRequest) {
  const phones = (data.phones || []).map((phone) => ({ ...phone }));
  const primary = Math.max(0, phones.findIndex((phone) => phone.isDefault));
  if (data.phone.trim()) {
    if (phones.length) phones[primary].phoneNumber = data.phone.trim();
    else phones.push({ phoneNumber: data.phone.trim(), isDefault: true });
  } else if (phones.length) phones.splice(primary, 1);
  return {
    name: data.name.trim(), code: data.code || null, type: data.type ?? 1,
    email: data.email?.trim() || null, address: data.address?.trim() || null,
    creditLimit: data.creditLimit, isActive: data.isActive, phones,
  };
}

export const customerService = {
  async getAll(search?: string, isActive?: boolean): Promise<CustomerDto[]> {
    try {
      const params: Record<string, string> = {};
      if (search) params.search = search;
      if (isActive !== undefined) params.isActive = String(isActive);

      return (await apiClient.get<CustomerApiDto[]>('customers', params)).map(normalizeCustomer);
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

      const result = await apiClient.get<PagedResult<CustomerApiDto>>('customers/paged', params);
      return { ...result, items: result.items.map(normalizeCustomer) };
    } catch (err) {
      console.error('[customerService.getPaged] Failed to fetch paged customers:', err);
      throw err;
    }
  },

  async getById(id: string): Promise<CustomerDto> {
    try {
      return normalizeCustomer(await apiClient.get<CustomerApiDto>(`customers/${id}`));
    } catch (err) {
      console.error(`[customerService.getById] Failed to fetch customer ${id}:`, err);
      throw err;
    }
  },

  async create(data: CreateCustomerRequest): Promise<CustomerDto> {
    try {
      return normalizeCustomer(await apiClient.post<CustomerApiDto>('customers', buildCustomerPayload(data)));
    } catch (err) {
      console.error('[customerService.create] Failed to create customer:', err);
      throw err;
    }
  },

  async update(id: string, data: UpdateCustomerRequest): Promise<CustomerDto> {
    try {
      return normalizeCustomer(await apiClient.put<CustomerApiDto>(`customers/${id}`, { id, ...buildCustomerPayload(data) }));
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

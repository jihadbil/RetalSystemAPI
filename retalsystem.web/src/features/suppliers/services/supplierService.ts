import { apiClient } from '../../../shared/services/apiClient';
import type { SupplierDto, SupplierStatementDto } from '../types/supplier.types';
import type { PagedResult } from '../../../shared/types/api.types';

export interface CreateSupplierRequest {
  phones?: SupplierDto['phones'];
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

interface SupplierApiDto extends SupplierDto { primaryPhone?: string }

export const normalizeSupplier = (data: SupplierApiDto): SupplierDto => ({
  ...data, phone: data.primaryPhone || data.phones?.[0]?.phoneNumber || data.phone || '',
});

export function buildSupplierPayload(data: CreateSupplierRequest) {
  const phones = (data.phones || []).map((phone) => ({ ...phone }));
  if (data.phone.trim()) {
    if (phones.length) phones[0].phoneNumber = data.phone.trim();
    else phones.push({ phoneNumber: data.phone.trim() });
  } else if (phones.length) phones.splice(0, 1);
  return { name: data.name.trim(), address: data.address?.trim() || null, isActive: data.isActive, phones };
}

export const supplierService = {
  async getAll(search?: string): Promise<SupplierDto[]> {
    try {
      const params: Record<string, string> = {};
      if (search) params.search = search;

      return (await apiClient.get<SupplierApiDto[]>('suppliers', params)).map(normalizeSupplier);
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

      const result = await apiClient.get<PagedResult<SupplierApiDto>>('suppliers/paged', params);
      return { ...result, items: result.items.map(normalizeSupplier) };
    } catch (err) {
      console.error('[supplierService.getPaged] Failed to fetch paged suppliers:', err);
      throw err;
    }
  },

  async getById(id: string): Promise<SupplierDto> {
    try {
      return normalizeSupplier(await apiClient.get<SupplierApiDto>(`suppliers/${id}`));
    } catch (err) {
      console.error(`[supplierService.getById] Failed to fetch supplier ${id}:`, err);
      throw err;
    }
  },

  async create(data: CreateSupplierRequest): Promise<SupplierDto> {
    try {
      return normalizeSupplier(await apiClient.post<SupplierApiDto>('suppliers', buildSupplierPayload(data)));
    } catch (err) {
      console.error('[supplierService.create] Failed to create supplier:', err);
      throw err;
    }
  },

  async update(id: string, data: UpdateSupplierRequest): Promise<SupplierDto> {
    try {
      return normalizeSupplier(await apiClient.put<SupplierApiDto>(`suppliers/${id}`, { id, ...buildSupplierPayload(data) }));
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

import { apiClient, ApiError } from './apiClient';
import type { PagedResult } from '../types/api.types';
import type { ProductDto } from '../../features/catalog/types/catalog.types';

export type LookupResource = 'branches' | 'customers' | 'suppliers' | 'safes' | 'expense-categories' | 'categories' | 'units' | 'products' | 'warehouses' | 'sales-return-invoices' | 'purchase-return-invoices' | 'category-directory' | 'unit-directory' | 'pos-terminals' | 'pos-payment-accounts' | 'cash-drawers';
export interface LookupOption { id: string; name: string; isActive?: boolean; branchId?: string; type?: number }
export interface LookupParams { search?: string; id?: string; barcode?: string; branchId?: string; pageNumber?: number; pageSize?: number }

export interface ReturnSourceInvoice extends LookupOption {
  invoiceNumber: string; warehouseId: string; status: number; totalAmount: number;
  customerId?: string; customerName?: string; supplierId?: string; supplierName?: string;
  items: { productId: string; productName: string; productBarCodeId?: string; quantity: number; unitPrice: number;
    discountAmount: number; lineTotal?: number; barcodeTitle?: string; unitCost?: number;
    breakdowns?: { productBarCodeId: string; quantity: number; unitPrice: number }[] }[];
}

export const lookupService = {
  page<T extends LookupOption = LookupOption>(resource: LookupResource, query: LookupParams = {}): Promise<PagedResult<T>> {
    return apiClient.get<PagedResult<T>>(`lookups/${resource}`, { ...query, pageSize: query.pageSize ?? 50 });
  },
  async list(resource: LookupResource, branchId?: string): Promise<LookupOption[]> {
    return (await this.page(resource, { branchId })).items;
  },
  async product(id: string): Promise<ProductDto> {
    const result = await this.page<ProductDto>('products', { id, pageSize: 1 });
    if (!result.items[0]) throw new ApiError('الصنف غير موجود', 404, 'NOT_FOUND');
    return result.items[0];
  },
  async barcode(barcode: string): Promise<ProductDto> {
    const result = await this.page<ProductDto>('products', { barcode, pageSize: 1 });
    if (!result.items[0]) throw new ApiError('الباركود غير موجود', 404, 'NOT_FOUND');
    return result.items[0];
  },
};

import { apiClient } from '../../../shared/services/apiClient';
import type {
  PurchaseInvoiceDto,
  PurchaseOrderDto,
  PurchaseReturnDto,
} from '../types/purchase.types';
import type { PagedResult } from '../../../shared/types/api.types';

export const purchaseService = {
  // فواتير المشتريات
  async getInvoices(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    supplierId?: string;
    branchId?: string;
    warehouseId?: string;
    status?: number;
  }): Promise<PagedResult<PurchaseInvoiceDto>> {
    try {
      return await apiClient.get<PagedResult<PurchaseInvoiceDto>>('purchase-invoices', params);
    } catch (err) {
      console.error('[purchaseService.getInvoices] Failed to fetch purchase invoices:', err);
      throw err;
    }
  },

  async createInvoice(invoice: unknown): Promise<PurchaseInvoiceDto> {
    try {
      return await apiClient.post<PurchaseInvoiceDto>('purchase-invoices', invoice);
    } catch (err) {
      console.error('[purchaseService.createInvoice] Failed to create purchase invoice:', err);
      throw err;
    }
  },

  // أوامر الشراء
  async getOrders(params?: {
    branchId?: string;
    warehouseId?: string;
    status?: number;
  }): Promise<PurchaseOrderDto[]> {
    try {
      return await apiClient.get<PurchaseOrderDto[]>('purchase-orders', params);
    } catch (err) {
      console.error('[purchaseService.getOrders] Failed to fetch purchase orders:', err);
      throw err;
    }
  },

  // مرتجعات الشراء
  async getReturns(params?: {
    branchId?: string;
    warehouseId?: string;
    supplierId?: string;
  }): Promise<PurchaseReturnDto[]> {
    try {
      return await apiClient.get<PurchaseReturnDto[]>('purchase-returns', params);
    } catch (err) {
      console.error('[purchaseService.getReturns] Failed to fetch purchase returns:', err);
      throw err;
    }
  },

  async createReturn(ret: unknown): Promise<PurchaseReturnDto> {
    try {
      return await apiClient.post<PurchaseReturnDto>('purchase-returns', ret);
    } catch (err) {
      console.error('[purchaseService.createReturn] Failed to create purchase return:', err);
      throw err;
    }
  },
};

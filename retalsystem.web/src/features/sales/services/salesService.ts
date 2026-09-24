import { apiClient } from '../../../shared/services/apiClient';
import type { SalesInvoiceDto, SalesReturnDto } from '../types/sales.types';
import type { PagedResult } from '../../../shared/types/api.types';

export const salesService = {
  // فواتير المبيعات
  async getInvoices(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    status?: number;
    branchId?: string;
    warehouseId?: string;
    customerId?: string;
  }): Promise<PagedResult<SalesInvoiceDto>> {
    try {
      return await apiClient.get<PagedResult<SalesInvoiceDto>>('sales-invoices/paged', params);
    } catch (err) {
      console.error('[salesService.getInvoices] Failed to fetch invoices:', err);
      throw err;
    }
  },

  async getInvoiceById(id: string): Promise<SalesInvoiceDto> {
    try {
      return await apiClient.get<SalesInvoiceDto>(`sales-invoices/${id}`);
    } catch (err) {
      console.error(`[salesService.getInvoiceById] Failed to fetch invoice ${id}:`, err);
      throw err;
    }
  },

  async createInvoice(invoice: unknown): Promise<SalesInvoiceDto> {
    try {
      return await apiClient.post<SalesInvoiceDto>('sales-invoices', invoice);
    } catch (err) {
      console.error('[salesService.createInvoice] Failed to create invoice:', err);
      throw err;
    }
  },

  // مرتجعات المبيعات
  async getReturns(params?: {
    branchId?: string;
    warehouseId?: string;
    customerId?: string;
    search?: string;
  }): Promise<SalesReturnDto[]> {
    try {
      return await apiClient.get<SalesReturnDto[]>('sales-returns', params);
    } catch (err) {
      console.error('[salesService.getReturns] Failed to fetch returns:', err);
      throw err;
    }
  },

  async createReturn(ret: unknown): Promise<SalesReturnDto> {
    try {
      return await apiClient.post<SalesReturnDto>('sales-returns', ret);
    } catch (err) {
      console.error('[salesService.createReturn] Failed to create return:', err);
      throw err;
    }
  },
};

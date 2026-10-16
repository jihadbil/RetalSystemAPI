import { apiClient } from '../../../shared/services/apiClient';
import type { PagedResult } from '../../../shared/types/api.types';
import type {
  WarehouseDto,
  StockLevelDto,
  StockAdjustmentDto,
  StockTransferDto,
  StockQueryParams,
} from '../types/stock.types';

interface WarehouseListResponse {
  items?: WarehouseDto[];
}

interface StockLevelApiItem {
  id?: string;
  productId?: string;
  productBarcodeId?: string;
  productName?: string;
  barcodeTitle?: string;
  barcodeValue?: string;
  productCode?: string;
  categoryName?: string;
  warehouseId?: string;
  warehouseName?: string;
  quantity?: number;
  minStockLevel?: number;
  costPrice?: number;
  salePrice?: number;
  isBelowMinLevel?: boolean;
  imageUrl?: string;
}

interface StockAdjustmentApiItem extends Partial<StockAdjustmentDto> {
  id: string;
  adjustmentNumber: string;
  adjustmentDate: string;
}

interface StockTransferApiItem extends Partial<StockTransferDto> {
  id: string;
  transferNumber: string;
  transferDate: string;
  fromWarehouseId?: string;
  fromWarehouseName?: string;
  toWarehouseId?: string;
  toWarehouseName?: string;
}

export const stockService = {
  // المستودعات
  async getWarehouses(): Promise<WarehouseDto[]> {
    try {
      const res = await apiClient.get<WarehouseDto[] | WarehouseListResponse>('warehouses');
      const list = Array.isArray(res) ? res : res.items || [];
      return list.map((w) => ({
        id: w.id,
        name: w.name,
        branchId: w.branchId,
        branchName: w.branchName,
        type: w.type,
        typeName: w.typeName,
        isActive: w.isActive !== false,
      }));
    } catch (err) {
      console.error('[stockService.getWarehouses] Failed to fetch warehouses:', err);
      throw err;
    }
  },

  async createWarehouse(wh: Partial<WarehouseDto>): Promise<WarehouseDto> {
    try {
      const payload = {
        name: wh.name,
        branchId: wh.branchId,
        type: Number(wh.type) || 1,
        isActive: wh.isActive !== false,
      };
      return await apiClient.post<WarehouseDto>('warehouses', payload);
    } catch (err) {
      console.error('[stockService.createWarehouse] Failed to create warehouse:', err);
      throw err;
    }
  },

  // أرصدة المخزون بالترقيم الصفحي من السيرفر
  async getStockLevels(
    warehouseId?: string,
    params?: StockQueryParams,
    warehouseType?: number
  ): Promise<PagedResult<StockLevelDto>> {
    if (!warehouseId || warehouseId === 'all') {
      return {
        items: [],
        totalCount: 0,
        pageNumber: 1,
        pageSize: params?.pageSize || 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      };
    }

    try {
      const queryParams: Record<string, string | number | boolean | undefined> = {
        pageNumber: params?.pageNumber || 1,
        pageSize: params?.pageSize || 10,
      };

      if (params?.search && params.search.trim()) {
        queryParams.searchTerm = params.search.trim();
      }

      if (params?.exactBarcode) {
        queryParams.exactBarcode = true;
      }

      const isStorage = warehouseType === 1;
      const endpoint = isStorage
        ? `stock/storge/${warehouseId}/paged`
        : `stock/showroom/${warehouseId}/paged`;

      const res = await apiClient.get<PagedResult<StockLevelApiItem>>(endpoint, queryParams);

      const items: StockLevelDto[] = (res.items || []).map((item) => {
        const qty = Number(item.quantity) || 0;
        const minLevel = Number(item.minStockLevel) || 0;
        return {
          productId: item.productId || item.productBarcodeId || item.id || '',
          productName: item.productName || item.barcodeTitle || 'صنف بدون اسم',
          code: item.barcodeValue || item.productCode || (item.productId ? item.productId.slice(0, 8) : ''),
          categoryName: isStorage ? 'مخزن رئيسي' : (item.categoryName || 'صالة العرض'),
          warehouseId: item.warehouseId || warehouseId,
          warehouseName: item.warehouseName || '',
          currentQuantity: qty,
          minStockLevel: minLevel,
          costPrice: Number(item.costPrice) || 0,
          salePrice: Number(item.salePrice) || 0,
          status: qty <= 0 ? 'نفد' : item.isBelowMinLevel || qty <= minLevel ? 'منخفض' : 'متوفر',
          barcode: item.barcodeValue,
          imageUrl: item.imageUrl,
        };
      });

      return {
        items,
        totalCount: res.totalCount ?? items.length,
        pageNumber: res.pageNumber ?? (params?.pageNumber || 1),
        pageSize: res.pageSize ?? (params?.pageSize || 10),
        totalPages: res.totalPages ?? Math.max(1, Math.ceil((res.totalCount || items.length) / (params?.pageSize || 10))),
        hasPreviousPage: Boolean(res.hasPreviousPage),
        hasNextPage: Boolean(res.hasNextPage),
      };
    } catch (err) {
      console.error('[stockService.getStockLevels] Failed to fetch stock levels:', err);
      throw err;
    }
  },

  // التسويات الجردية بالترقيم الصفحي
  async getAdjustments(params?: {
    pageNumber?: number;
    pageSize?: number;
    warehouseId?: string;
    search?: string;
  }): Promise<PagedResult<StockAdjustmentDto>> {
    try {
      const cleanParams: Record<string, string | number | boolean | undefined | null> = {
        pageNumber: params?.pageNumber || 1,
        pageSize: params?.pageSize || 10,
      };
      if (params?.warehouseId && params.warehouseId !== 'all') {
        cleanParams.warehouseId = params.warehouseId;
      }
      if (params?.search && params.search.trim()) {
        cleanParams.search = params.search.trim();
      }

      const res = await apiClient.get<PagedResult<StockAdjustmentApiItem>>('stock-adjustments/paged', cleanParams);
      const items: StockAdjustmentDto[] = (res.items || []).map((adj) => ({
        id: adj.id,
        adjustmentNumber: adj.adjustmentNumber,
        adjustmentDate: adj.adjustmentDate,
        warehouseId: adj.warehouseId,
        warehouseName: adj.warehouseName,
        reason: adj.reason,
        reasonName: adj.reasonName || (adj.reason !== undefined ? String(adj.reason) : 'تسوية'),
        itemCount: adj.itemCount ?? (adj.items ? adj.items.length : 0),
        totalDifferenceCost: adj.totalDifferenceCost ?? 0,
        items: adj.items || [],
      }));

      return {
        items,
        totalCount: res.totalCount ?? items.length,
        pageNumber: res.pageNumber ?? Number(cleanParams.pageNumber),
        pageSize: res.pageSize ?? Number(cleanParams.pageSize),
        totalPages: res.totalPages ?? Math.max(1, Math.ceil((res.totalCount || items.length) / Number(cleanParams.pageSize))),
        hasPreviousPage: Boolean(res.hasPreviousPage),
        hasNextPage: Boolean(res.hasNextPage),
      };
    } catch (err) {
      console.error('[stockService.getAdjustments] Failed to fetch stock adjustments:', err);
      throw err;
    }
  },

  async createAdjustment(adj: unknown): Promise<StockAdjustmentDto> {
    try {
      return await apiClient.post<StockAdjustmentDto>('stock-adjustments', adj);
    } catch (err) {
      console.error('[stockService.createAdjustment] Failed to create stock adjustment:', err);
      throw err;
    }
  },

  // التحويلات المخزنية بالترقيم الصفحي
  async getTransfers(params?: {
    pageNumber?: number;
    pageSize?: number;
    fromWarehouseId?: string;
    toWarehouseId?: string;
    search?: string;
  }): Promise<PagedResult<StockTransferDto>> {
    try {
      const cleanParams: Record<string, string | number | boolean | undefined | null> = {
        pageNumber: params?.pageNumber || 1,
        pageSize: params?.pageSize || 10,
      };
      if (params?.fromWarehouseId && params.fromWarehouseId !== 'all') {
        cleanParams.fromWarehouseId = params.fromWarehouseId;
      }
      if (params?.toWarehouseId && params.toWarehouseId !== 'all') {
        cleanParams.toWarehouseId = params.toWarehouseId;
      }
      if (params?.search && params.search.trim()) {
        cleanParams.search = params.search.trim();
      }

      const res = await apiClient.get<PagedResult<StockTransferApiItem>>('stock-transfers/paged', cleanParams);
      const items: StockTransferDto[] = (res.items || []).map((trf) => ({
        id: trf.id,
        transferNumber: trf.transferNumber,
        transferDate: trf.transferDate,
        sourceWarehouseId: trf.fromWarehouseId || trf.sourceWarehouseId,
        sourceWarehouseName: trf.fromWarehouseName || trf.sourceWarehouseName,
        fromWarehouseName: trf.fromWarehouseName || trf.sourceWarehouseName,
        destinationWarehouseId: trf.toWarehouseId || trf.destinationWarehouseId,
        destinationWarehouseName: trf.toWarehouseName || trf.destinationWarehouseName,
        toWarehouseName: trf.toWarehouseName || trf.destinationWarehouseName,
        status: trf.status ?? 0,
        statusName: trf.statusName,
        notes: trf.notes,
        itemCount: trf.itemCount ?? (trf.items ? trf.items.length : 0),
        items: trf.items || [],
      }));

      return {
        items,
        totalCount: res.totalCount ?? items.length,
        pageNumber: res.pageNumber ?? Number(cleanParams.pageNumber),
        pageSize: res.pageSize ?? Number(cleanParams.pageSize),
        totalPages: res.totalPages ?? Math.max(1, Math.ceil((res.totalCount || items.length) / Number(cleanParams.pageSize))),
        hasPreviousPage: Boolean(res.hasPreviousPage),
        hasNextPage: Boolean(res.hasNextPage),
      };
    } catch (err) {
      console.error('[stockService.getTransfers] Failed to fetch stock transfers:', err);
      throw err;
    }
  },

  async createTransfer(trf: unknown): Promise<StockTransferDto> {
    try {
      return await apiClient.post<StockTransferDto>('stock-transfers', trf);
    } catch (err) {
      console.error('[stockService.createTransfer] Failed to create stock transfer:', err);
      throw err;
    }
  },
};

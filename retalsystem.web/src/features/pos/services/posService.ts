import { apiClient, ApiError } from '../../../shared/services/apiClient';
import type { CartItem, PosDraft, QuickProductItem } from '../types/pos.types';
import type { ProductDto } from '../../catalog/types/catalog.types';
import type { PagedResult } from '../../../shared/types/api.types';
import { stockService } from '../../stock/services/stockService';

const DRAFTS_STORAGE_KEY = 'retal_pos_held_drafts';

export interface CashSessionInfo {
  id: string;
  cashDrawerId: string;
  cashDrawerName: string;
  openingFloat: number;
  expectedClosingBalance: number;
  countedClosingBalance?: number;
  difference?: number;
  status: number;
}

export interface PosDeviceContext {
  terminal: { id: string; name: string; code: string; branchId: string; defaultCashDrawerName: string };
  currentSession?: CashSessionInfo;
}

interface CheckoutPayload {
  invoiceNumber?: string;
  warehouseId?: string;
  [key: string]: unknown;
}

interface CheckoutResponse {
  invoiceNumber?: string;
  id?: string;
  invoiceId?: string;
}

export const posService = {
  claimDevice(tenantId: string, pairingCode: string) {
    return apiClient.post('pos-device/claim', { tenantId, pairingCode });
  },

  getDeviceContext(): Promise<PosDeviceContext> {
    return apiClient.get<PosDeviceContext>('pos-device/context');
  },

  openCashSession(openingFloat: number): Promise<CashSessionInfo> {
    return apiClient.post<CashSessionInfo>('pos-device/sessions/open', { openingFloat });
  },

  closeCashSession(countedClosingBalance: number, notes?: string): Promise<CashSessionInfo> {
    return apiClient.post<CashSessionInfo>('pos-device/sessions/close', { countedClosingBalance, notes });
  },

  // حفظ قائمة المسودات محلياً
  saveDrafts(drafts: PosDraft[]): void {
    try {
      localStorage.setItem(DRAFTS_STORAGE_KEY, JSON.stringify(drafts));
    } catch {
      // إهمال أخطاء التخزين
    }
  },

  // استرجاع المسودات المعلقة المحفوظة
  loadDrafts(): PosDraft[] {
    try {
      const stored = localStorage.getItem(DRAFTS_STORAGE_KEY);
      if (!stored) return [];
      return JSON.parse(stored);
    } catch {
      return [];
    }
  },

  // البحث السريع عن الصنف بواسطة الباركود
  async findProductByBarcode(barcode: string): Promise<CartItem | null> {
    try {
      const product = await apiClient.get<ProductDto>(
        `catalog/products/by-barcode/${encodeURIComponent(barcode)}`
      );

      if (!product || !product.id) return null;

      return {
        productId: product.id,
        productName: product.name,
        code: product.code,
        barCode: barcode,
        unitName: product.defaultUnitName || 'قطعة',
        unitPrice: product.salePrice,
        quantity: 1,
        discountAmount: 0,
        lineTotal: product.salePrice,
      };
    } catch (err: unknown) {
      if (err instanceof ApiError && err.statusCode === 404) {
        return null;
      }
      console.error('[posService.findProductByBarcode] API failed:', err);
      throw err;
    }
  },

  // جلب قائمة الأصناف السريعة (Quick Items)
  async getQuickItems(): Promise<QuickProductItem[]> {
    try {
      const paged = await apiClient.get<PagedResult<ProductDto>>('catalog/products/paged', {
        pageNumber: 1,
        pageSize: 30,
      });

      const items = paged?.items || [];
      return items.map((p) => ({
        productId: p.id,
        name: p.name,
        code: p.code,
        price: p.salePrice,
        categoryName: p.categoryName || 'عام',
        barCode: p.barCodes?.[0]?.barCode || p.code,
      }));
    } catch (err) {
      console.error('[posService.getQuickItems] API failed:', err);
      throw err;
    }
  },

  // حفظ واعتماد الفاتورة نهائياً
  async checkout(invoiceData: CheckoutPayload): Promise<{ invoiceNumber: string; id: string }> {
    try {
      const payload = { ...invoiceData };
      if (!payload.invoiceNumber) {
        payload.invoiceNumber = `POS-${Date.now()}`;
      }
      if (!payload.warehouseId) {
        const warehouses = await stockService.getWarehouses().catch(() => []);
        if (warehouses && warehouses.length > 0) {
          payload.warehouseId = warehouses[0].id;
        }
      }

      const res = await apiClient.post<CheckoutResponse>('sales-invoices', payload);
      return {
        invoiceNumber: res?.invoiceNumber || payload.invoiceNumber,
        id: res?.id || res?.invoiceId || '',
      };
    } catch (err) {
      console.error('[posService.checkout] Checkout failed:', err);
      throw err;
    }
  },
};

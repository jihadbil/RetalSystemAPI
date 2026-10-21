import { apiClient, ApiError } from '../../../shared/services/apiClient';
import type { CartItem, PosDraft, QuickProductItem } from '../types/pos.types';
import { lookupService } from '../../../shared/services/lookupService';
import type { ProductDto } from '../../catalog/types/catalog.types';

const DRAFTS_STORAGE_PREFIX = 'retal_pos_held_drafts';

const getDraftsStorageKey = (scope: string) => `${DRAFTS_STORAGE_PREFIX}:${scope}`;

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

export interface PosPaymentAccount { id: string; name: string; branchId: string }

export interface PosDeviceContext {
  terminal: { id: string; name: string; code: string; branchId: string; defaultCashDrawerName: string };
  currentSession?: CashSessionInfo;
}

export interface CheckoutPayload {
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
  getPaymentAccounts(branchId: string): Promise<PosPaymentAccount[]> {
    return lookupService.list('pos-payment-accounts', branchId).then((options) => options.map((option) => ({ id: option.id, name: option.name, branchId: option.branchId || '' })));
  },
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
  saveDrafts(scope: string, drafts: PosDraft[]): void {
    try {
      localStorage.setItem(getDraftsStorageKey(scope), JSON.stringify(drafts));
    } catch {
      // إهمال أخطاء التخزين
    }
  },

  // استرجاع المسودات المعلقة المحفوظة
  loadDrafts(scope: string): PosDraft[] {
    try {
      const stored = localStorage.getItem(getDraftsStorageKey(scope));
      if (!stored) return [];
      return JSON.parse(stored);
    } catch {
      return [];
    }
  },

  // البحث السريع عن الصنف بواسطة الباركود
  async findProductByBarcode(barcode: string): Promise<CartItem | null> {
    try {
      const product = await lookupService.barcode(barcode);

      if (!product || !product.id) return null;

      const matchedBarcode = product.barCodes?.find(
        (entry) => entry.barCode.toLowerCase() === barcode.toLowerCase()
      );
      return {
        lineId: matchedBarcode?.id || matchedBarcode?.barCode || product.id,
        productId: product.id,
        productBarCodeId: matchedBarcode?.id,
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
  async getQuickItems(search?: string): Promise<QuickProductItem[]> {
    try {
      const paged = await lookupService.page<ProductDto>('products', {
        search,
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
        payload.invoiceNumber = `POS-${crypto.randomUUID().replace(/-/g, '')}`;
      }
      if (!payload.warehouseId) {
        throw new ApiError('يجب تحديد المستودع الذي ستُخصم منه مبيعات نقطة البيع', 400, 'WAREHOUSE_REQUIRED');
      }

      const res = await apiClient.post<CheckoutResponse>('pos-sales', payload);
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

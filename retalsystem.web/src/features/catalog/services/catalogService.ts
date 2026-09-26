import { apiClient } from '../../../shared/services/apiClient';
import type {
  ProductDto,
  CreateProductRequest,
  CategoryDto,
  UnitDto,
  ImportPreviewRow,
} from '../types/catalog.types';
import type { PagedResult } from '../../../shared/types/api.types';

interface ImportPreviewResponse {
  rows?: ImportPreviewRow[];
  previewRows?: ImportPreviewRow[];
}

export const catalogService = {
  // المنتجات
  async getProducts(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    categoryId?: string;
  }): Promise<PagedResult<ProductDto>> {
    try {
      const cleanParams: Record<string, string | number | boolean | undefined | null> = { ...params };
      if (cleanParams.categoryId === 'all') {
        delete cleanParams.categoryId;
      }
      return await apiClient.get<PagedResult<ProductDto>>('catalog/products/paged', cleanParams);
    } catch (err) {
      console.error('[catalogService.getProducts] Failed to fetch products:', err);
      throw err;
    }
  },

  async getProductById(id: string): Promise<ProductDto> {
    try {
      return await apiClient.get<ProductDto>(`catalog/products/${id}`);
    } catch (err) {
      console.error(`[catalogService.getProductById] Failed to fetch product ${id}:`, err);
      throw err;
    }
  },

  async createProduct(request: CreateProductRequest): Promise<ProductDto> {
    try {
      return await apiClient.post<ProductDto>('catalog/products', request);
    } catch (err) {
      console.error('[catalogService.createProduct] Failed to create product:', err);
      throw err;
    }
  },

  async updateProduct(id: string, request: Partial<CreateProductRequest>): Promise<void> {
    try {
      await apiClient.put(`catalog/products/${id}`, request);
    } catch (err) {
      console.error(`[catalogService.updateProduct] Failed to update product ${id}:`, err);
      throw err;
    }
  },

  async deleteProduct(id: string): Promise<void> {
    try {
      await apiClient.delete(`catalog/products/${id}`);
    } catch (err) {
      console.error(`[catalogService.deleteProduct] Failed to delete product ${id}:`, err);
      throw err;
    }
  },

  // التصنيفات
  async getCategories(): Promise<CategoryDto[]> {
    try {
      return await apiClient.get<CategoryDto[]>('catalog/categories');
    } catch (err) {
      console.error('[catalogService.getCategories] Failed to fetch categories:', err);
      throw err;
    }
  },

  async createCategory(category: { name: string; description?: string }): Promise<CategoryDto> {
    try {
      return await apiClient.post<CategoryDto>('catalog/categories', category);
    } catch (err) {
      console.error('[catalogService.createCategory] Failed to create category:', err);
      throw err;
    }
  },

  // البحث بالباركود
  async getByBarCode(barCode: string): Promise<ProductDto> {
    try {
      return await apiClient.get<ProductDto>(`catalog/products/by-barcode/${encodeURIComponent(barCode)}`);
    } catch (err) {
      console.error('[catalogService.getByBarCode] Failed to fetch product by barcode:', err);
      throw err;
    }
  },

  // البحث في المنتجات
  async searchProducts(query: string): Promise<ProductDto[]> {
    try {
      return await apiClient.get<ProductDto[]>(`catalog/products/search?q=${encodeURIComponent(query)}`);
    } catch (err) {
      console.error('[catalogService.searchProducts] Failed to search products:', err);
      throw err;
    }
  },

  // وحدات القياس
  async getUnits(): Promise<UnitDto[]> {
    try {
      return await apiClient.get<UnitDto[]>('catalog/units');
    } catch (err) {
      console.error('[catalogService.getUnits] Failed to fetch units:', err);
      throw err;
    }
  },

  async createUnit(unit: { name: string; symbol: string; isBaseUnit?: boolean }): Promise<UnitDto> {
    try {
      return await apiClient.post<UnitDto>('catalog/units', unit);
    } catch (err) {
      console.error('[catalogService.createUnit] Failed to create unit:', err);
      throw err;
    }
  },

  // معالج استيراد الأصناف من Excel
  async previewImportFile(file: File): Promise<ImportPreviewRow[]> {
    try {
      const formData = new FormData();
      formData.append('file', file);
      const res = await apiClient.post<ImportPreviewResponse>('catalog/products/validate-excel', formData);
      return res?.rows || res?.previewRows || [];
    } catch (err) {
      console.error('[catalogService.previewImportFile] Failed to preview Excel:', err);
      throw err;
    }
  },

  async executeImport(rows: ImportPreviewRow[]): Promise<{ importedCount: number; rejectedCount: number }> {
    const validRows = rows.filter((r) => r.isValid);
    return {
      importedCount: validRows.length,
      rejectedCount: rows.length - validRows.length,
    };
  },
};

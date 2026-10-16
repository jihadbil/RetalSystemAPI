import { apiClient } from '../../../shared/services/apiClient';
import type {
  ProductDto,
  CreateProductRequest,
  CategoryDto,
  CreateCategoryRequest,
  UpdateCategoryRequest,
  UnitDto,
  ImportPreviewRow,
  ProductImportResult,
} from '../types/catalog.types';
import type { PagedResult } from '../../../shared/types/api.types';

interface ImportValidationRow {
  rowNumber: number;
  itemCode?: string;
  name: string;
  barCode?: string;
  categoryName?: string;
  unitName?: string;
  costPrice: number;
  salePrice: number;
  status: number;
  message?: string;
}

interface ImportPreviewResponse {
  rows?: ImportValidationRow[];
  previewRows?: ImportValidationRow[];
}

interface ProductApiImage {
  id?: string;
  productId?: string;
  imageUrl: string;
  isDefault?: boolean;
  isPrimary?: boolean;
  displayOrder?: number;
}

export interface ProductApiDto extends Omit<ProductDto, 'code' | 'isActive' | 'images'> {
  code?: string;
  defaultBarCode?: string;
  defaultImage?: string;
  isActive?: boolean;
  images?: ProductApiImage[];
}

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

export const normalizeProduct = (product: ProductApiDto): ProductDto => {
  const units = product.units || [];
  const images = (product.images || []).map((image) => ({
    ...image,
    isPrimary: image.isPrimary ?? image.isDefault ?? false,
  }));
  const defaultUnit = units.find((unit) => unit.isDefault) || units[0];

  return {
    ...product,
    code: product.code || product.defaultBarCode || product.barCodes?.[0]?.barCode || '',
    isActive: product.isActive ?? true,
    defaultUnitId: product.defaultUnitId || defaultUnit?.unitId,
    defaultUnitName: product.defaultUnitName || defaultUnit?.unitName,
    primaryImageUrl:
      product.primaryImageUrl ||
      product.defaultImage ||
      images.find((image) => image.isPrimary)?.imageUrl ||
      images[0]?.imageUrl,
    images,
    units,
  };
};

export const buildProductPayload = (request: CreateProductRequest, productId = EMPTY_GUID) => {
  const barcodeMap = new Map<string, { barCode: string; title: string; description?: string }>();
  const addBarcode = (barCode: string, title?: string, description?: string) => {
    const cleanCode = barCode.trim();
    if (!cleanCode || barcodeMap.has(cleanCode.toLowerCase())) return;
    barcodeMap.set(cleanCode.toLowerCase(), {
      barCode: cleanCode,
      title: title?.trim() || description?.trim() || request.name,
      description: description?.trim() || undefined,
    });
  };

  addBarcode(request.code, 'الكود الرئيسي');
  request.barCodes?.forEach((barcode) => addBarcode(barcode.barCode, barcode.title, barcode.description));

  const barCodes = [...barcodeMap.values()].map((barcode) => ({
    ...barcode,
    productId,
    initialQuantity: 0,
  }));
  const primaryBarCode = barCodes[0]?.barCode || request.code.trim();
  const units = request.units?.length
    ? request.units
    : request.defaultUnitId
      ? [{ unitId: request.defaultUnitId, conversionFactor: 1, isDefault: true }]
      : [];
  const openingBalances = request.openingBalances?.filter((balance) => balance.quantity > 0) || [];

  return {
    id: productId,
    name: request.name,
    description: request.description || undefined,
    categoryId: request.categoryId,
    costPrice: request.costPrice,
    salePrice: request.salePrice,
    barCodes,
    units: units.map((unit) => ({
      productId,
      unitId: unit.unitId,
      conversionFactor: Math.max(1, Math.trunc(unit.conversionFactor || 1)),
      isDefault: unit.isDefault ?? unit.unitId === request.defaultUnitId,
    })),
    showroomInitialQuantities: openingBalances
      .filter((balance) => balance.warehouseType === 2)
      .map(({ warehouseId, quantity }) => ({ warehouseId, quantity: Math.trunc(quantity) })),
    storageInitialQuantities: openingBalances
      .filter((balance) => balance.warehouseType === 1)
      .map(({ warehouseId, quantity }) => ({
        warehouseId,
        barCode: primaryBarCode,
        quantity: Math.trunc(quantity),
      })),
  };
};

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
      const result = await apiClient.get<PagedResult<ProductApiDto>>('catalog/products/paged', cleanParams);
      return { ...result, items: result.items.map(normalizeProduct) };
    } catch (err) {
      console.error('[catalogService.getProducts] Failed to fetch products:', err);
      throw err;
    }
  },

  async getProductById(id: string): Promise<ProductDto> {
    try {
      return normalizeProduct(await apiClient.get<ProductApiDto>(`catalog/products/${id}`));
    } catch (err) {
      console.error(`[catalogService.getProductById] Failed to fetch product ${id}:`, err);
      throw err;
    }
  },

  async createProduct(request: CreateProductRequest): Promise<ProductDto> {
    try {
      return normalizeProduct(
        await apiClient.post<ProductApiDto>('catalog/products', buildProductPayload(request))
      );
    } catch (err) {
      console.error('[catalogService.createProduct] Failed to create product:', err);
      throw err;
    }
  },

  async updateProduct(id: string, request: CreateProductRequest): Promise<void> {
    try {
      await apiClient.put(`catalog/products/${id}`, buildProductPayload(request, id));
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

  async createCategory(category: CreateCategoryRequest): Promise<CategoryDto> {
    try {
      return await apiClient.post<CategoryDto>('catalog/categories', category);
    } catch (err) {
      console.error('[catalogService.createCategory] Failed to create category:', err);
      throw err;
    }
  },

  async updateCategory(id: string, category: UpdateCategoryRequest): Promise<CategoryDto> {
    try {
      return await apiClient.put<CategoryDto>(`catalog/categories/${id}`, category);
    } catch (err) {
      console.error('[catalogService.updateCategory] Failed to update category:', err);
      throw err;
    }
  },

  async deleteCategory(id: string): Promise<void> {
    try {
      await apiClient.delete(`catalog/categories/${id}`);
    } catch (err) {
      console.error('[catalogService.deleteCategory] Failed to delete category:', err);
      throw err;
    }
  },

  // البحث بالباركود
  async getByBarCode(barCode: string): Promise<ProductDto> {
    try {
      return normalizeProduct(
        await apiClient.get<ProductApiDto>(`catalog/products/by-barcode/${encodeURIComponent(barCode)}`)
      );
    } catch (err) {
      console.error('[catalogService.getByBarCode] Failed to fetch product by barcode:', err);
      throw err;
    }
  },

  // البحث في المنتجات
  async searchProducts(query: string): Promise<ProductDto[]> {
    try {
      const products = await apiClient.get<ProductApiDto[]>(`catalog/products/search?q=${encodeURIComponent(query)}`);
      return products.map(normalizeProduct);
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
      const rows = res?.rows || res?.previewRows || [];
      return rows.map((row) => ({
        rowNumber: row.rowNumber,
        name: row.name,
        code: row.itemCode || '',
        categoryName: row.categoryName || '',
        salePrice: row.salePrice,
        costPrice: row.costPrice,
        barCode: row.barCode,
        unitName: row.unitName,
        isValid: row.status !== 2,
        error: row.status === 2 ? row.message || 'بيانات السطر غير صالحة' : undefined,
      }));
    } catch (err) {
      console.error('[catalogService.previewImportFile] Failed to preview Excel:', err);
      throw err;
    }
  },

  async executeImport(file: File): Promise<ProductImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    return apiClient.post<ProductImportResult>('catalog/products/import-excel', formData);
  },

  async downloadImportTemplate(): Promise<void> {
    const { blob, fileName } = await apiClient.download('catalog/products/excel-template', { singleSheet: true });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName || 'Products_Import_Template_SingleSheet.xlsx';
    document.body.appendChild(link);
    link.click();
    link.remove();
    URL.revokeObjectURL(url);
  },
};

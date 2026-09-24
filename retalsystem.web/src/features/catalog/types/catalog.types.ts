// أنواع ونماذج كتالوج المنتجات والتصنيفات والوحدات

export interface ProductBarCodeDto {
  id?: string;
  productId?: string;
  barCode: string;
  description?: string;
  isPrimary?: boolean;
}

export interface ProductUnitDto {
  id?: string;
  productId?: string;
  unitId: string;
  unitName?: string;
  conversionFactor: number;
  salePrice: number;
  purchasePrice: number;
  isDefault?: boolean;
}

export interface ProductImageDto {
  id?: string;
  productId?: string;
  imageUrl: string;
  isPrimary: boolean;
  displayOrder?: number;
}

export interface ProductDto {
  id: string;
  name: string;
  code: string;
  description?: string;
  categoryId: string;
  categoryName?: string;
  defaultUnitId?: string;
  defaultUnitName?: string;
  costPrice: number;
  salePrice: number;
  minStockLevel: number;
  isActive: boolean;
  primaryImageUrl?: string;
  currentStock?: number;
  units?: ProductUnitDto[];
  barCodes?: ProductBarCodeDto[];
  images?: ProductImageDto[];
}

export interface CreateProductRequest {
  name: string;
  code: string;
  description?: string;
  categoryId: string;
  defaultUnitId?: string;
  costPrice: number;
  salePrice: number;
  minStockLevel: number;
  isActive: boolean;
  barCodes?: { barCode: string; description?: string }[];
  units?: { unitId: string; conversionFactor: number; salePrice: number; purchasePrice: number }[];
  openingBalances?: { warehouseId: string; quantity: number }[];
}

export interface CategoryDto {
  id: string;
  name: string;
  description?: string;
  parentCategoryId?: string;
  parentCategoryName?: string;
  isActive: boolean;
  productsCount?: number;
}

export interface UnitDto {
  id: string;
  name: string;
  symbol: string;
  isBaseUnit: boolean;
}

export interface ImportPreviewRow {
  rowNumber: number;
  name: string;
  code: string;
  categoryName: string;
  salePrice: number;
  costPrice: number;
  barCode?: string;
  unitName?: string;
  isValid: boolean;
  error?: string;
}

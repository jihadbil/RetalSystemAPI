import { describe, expect, it } from 'vitest';
import { buildProductPayload, normalizeProduct, type ProductApiDto } from './catalogService';

describe('catalog API contract mapping', () => {
  it('builds the backend product contract without duplicate barcodes', () => {
    const payload = buildProductPayload({
      name: 'قهوة',
      code: 'SKU-1',
      categoryId: 'category-1',
      defaultUnitId: 'unit-1',
      costPrice: 4,
      salePrice: 6,
      barCodes: [
        { barCode: 'SKU-1', description: 'مكرر' },
        { barCode: '628100', title: 'عبوة' },
      ],
      openingBalances: [
        { warehouseId: 'show-1', warehouseType: 2, quantity: 5.9 },
        { warehouseId: 'store-1', warehouseType: 1, quantity: 8.4 },
      ],
    });

    expect(payload.barCodes).toHaveLength(2);
    expect(payload.units).toEqual([expect.objectContaining({ unitId: 'unit-1', isDefault: true })]);
    expect(payload.showroomInitialQuantities).toEqual([{ warehouseId: 'show-1', quantity: 5 }]);
    expect(payload.storageInitialQuantities).toEqual([
      { warehouseId: 'store-1', barCode: 'SKU-1', quantity: 8 },
    ]);
  });

  it('normalizes summary and detail responses for the UI', () => {
    const product = normalizeProduct({
      id: 'product-1',
      name: 'قهوة',
      description: '',
      categoryId: 'category-1',
      categoryName: 'مشروبات',
      costPrice: 4,
      salePrice: 6,
      defaultBarCode: '628100',
      barCodes: [{ id: 'barcode-1', barCode: '628100' }],
      units: [{ unitId: 'unit-1', unitName: 'قطعة', conversionFactor: 1, isDefault: true }],
      images: [{ imageUrl: '/image.jpg', isDefault: true }],
    } as ProductApiDto);

    expect(product.code).toBe('628100');
    expect(product.defaultUnitId).toBe('unit-1');
    expect(product.primaryImageUrl).toBe('/image.jpg');
    expect(product.isActive).toBe(true);
  });
});

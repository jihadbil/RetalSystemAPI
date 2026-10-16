// أنواع ونماذج المخزون والتسويات والتحويلات والمستودعات

export interface WarehouseDto {
  id: string;
  name: string;
  branchId: string;
  branchName?: string;
  type?: number;
  typeName?: string;
  storekeeperName?: string;
  phone?: string;
  address?: string;
  isActive: boolean;
}

export interface StockQueryParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  exactBarcode?: boolean;
}

export interface StockLevelDto {
  productId: string;
  productName: string;
  code: string;
  categoryName?: string;
  warehouseId: string;
  warehouseName: string;
  currentQuantity: number;
  minStockLevel: number;
  costPrice: number;
  salePrice: number;
  status: 'متوفر' | 'منخفض' | 'نفد';
  barcode?: string;
  imageUrl?: string;
}

export enum AdjustmentType {
  Shortage = 1, // عجز
  Surplus = 2,  // فائض
  Damage = 3,   // تالف
}

export const AdjustmentTypeLabels: Record<AdjustmentType, string> = {
  [AdjustmentType.Shortage]: 'عجز مخزني',
  [AdjustmentType.Surplus]: 'فائض مخزني',
  [AdjustmentType.Damage]: 'بضاعة تالفة',
};

export interface StockAdjustmentItemDto {
  productId: string;
  productName: string;
  code?: string;
  currentQuantity: number;
  actualQuantity: number;
  differenceQuantity: number;
  unitCost: number;
  totalCostDifference: number;
  notes?: string;
}

export interface StockAdjustmentDto {
  id: string;
  adjustmentNumber: string;
  warehouseId?: string;
  warehouseName?: string;
  adjustmentDate: string;
  type?: AdjustmentType;
  reason?: string | number;
  reasonName?: string;
  totalDifferenceCost?: number;
  itemCount?: number;
  items?: StockAdjustmentItemDto[];
}

export enum TransferStatus {
  Pending = 0,
  Dispatched = 1,
  Received = 2,
  Cancelled = 3,
}

export const TransferStatusLabels: Record<TransferStatus, { text: string; color: string; bg: string }> = {
  [TransferStatus.Pending]: { text: 'قيد التجهيز', color: 'text-amber-700', bg: 'bg-amber-50' },
  [TransferStatus.Dispatched]: { text: 'مرسل / في الطريق', color: 'text-sky-700', bg: 'bg-sky-50' },
  [TransferStatus.Received]: { text: 'مستلم ومكتمل', color: 'text-emerald-700', bg: 'bg-emerald-50' },
  [TransferStatus.Cancelled]: { text: 'ملغي', color: 'text-rose-700', bg: 'bg-rose-50' },
};

export interface StockTransferDto {
  id: string;
  transferNumber: string;
  sourceWarehouseId?: string;
  sourceWarehouseName?: string;
  fromWarehouseName?: string;
  destinationWarehouseId?: string;
  destinationWarehouseName?: string;
  toWarehouseName?: string;
  transferDate: string;
  status: TransferStatus;
  statusName?: string;
  notes?: string;
  itemCount?: number;
  items?: {
    productId: string;
    productName: string;
    quantity: number;
    unitName?: string;
  }[];
}

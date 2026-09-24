import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import type { WarehouseDto } from '../types/stock.types';
import { AdjustmentType } from '../types/stock.types';
import { catalogService } from '../../catalog/services/catalogService';
import type { ProductDto } from '../../catalog/types/catalog.types';

import { useDebounce } from '../../../shared/hooks/useDebounce';

interface StockAdjustmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  warehouses: WarehouseDto[];
  onSave: (data: unknown) => Promise<void>;
}

export const StockAdjustmentModal: React.FC<StockAdjustmentModalProps> = ({
  isOpen,
  onClose,
  warehouses,
  onSave,
}) => {
  const [warehouseId, setWarehouseId] = useState('');
  const [type, setType] = useState<AdjustmentType>(AdjustmentType.Shortage);
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [productSearch, setProductSearch] = useState('');
  const debouncedProductSearch = useDebounce(productSearch, 300);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [currentQty, setCurrentQty] = useState<number>(0);
  const [actualQty, setActualQty] = useState<number>(0);
  const [reason, setReason] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (isOpen) {
      if (warehouses.length > 0 && !warehouseId) {
        setWarehouseId(warehouses[0].id);
      }
      catalogService
        .getProducts({ pageNumber: 1, pageSize: 30, search: debouncedProductSearch.trim() || undefined })
        .then((res) => {
          const prods = res.items || [];
          setProducts(prods);
          if (prods.length > 0 && !prods.some((p) => p.id === selectedProductId)) {
            setSelectedProductId(prods[0].id);
          }
        })
        .catch((err) => console.error('Failed to load products:', err));
    }
  }, [isOpen, warehouses, warehouseId, debouncedProductSearch, selectedProductId]);

  const selectedProduct = products.find((p) => p.id === selectedProductId);
  const diff = actualQty - currentQty;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProductId) return;

    setIsLoading(true);
    try {
      const adjustmentNumber = `ADJ-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(
        100 + Math.random() * 900
      )}`;
      await onSave({
        adjustmentNumber,
        adjustmentDate: new Date().toISOString(),
        warehouseId: warehouseId || warehouses[0]?.id,
        reason: Number(type),
        notes: reason.trim() || undefined,
        items: [
          {
            productId: selectedProductId,
            systemQuantity: currentQty,
            actualQuantity: actualQty,
            unitCost: selectedProduct?.costPrice || 0,
            reason: Number(type),
          },
        ],
      });
      onClose();
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إنشاء تسوية جردية جديدة"
      subtitle="تسجيل الفروقات المخزنية الناتجة عن الجرد الدوري (عجز، فائض، أو تالف)"
      size="md"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose} disabled={isLoading}>
            إلغاء
          </Button>
          <Button variant="primary" size="md" onClick={handleSubmit} isLoading={isLoading}>
            اعتماد التسوية
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-3 py-1">
        <Select
          label="المستودع *"
          value={warehouseId}
          onChange={(e) => setWarehouseId(e.target.value)}
        >
          {warehouses.map((w) => (
            <option key={w.id} value={w.id}>
              {w.name}
            </option>
          ))}
        </Select>

        <Select
          label="نوع التسوية *"
          value={type}
          onChange={(e) => setType(Number(e.target.value) as AdjustmentType)}
        >
          <option value={AdjustmentType.Shortage}>عجز مخزني (نقص في الكمية)</option>
          <option value={AdjustmentType.Surplus}>فائض مخزني (زيادة في الكمية)</option>
          <option value={AdjustmentType.Damage}>بضاعة تالفة / منتهية الصلاحية</option>
        </Select>

        <Input
          label="بحث في الأصناف (بالاسم أو الكود)"
          placeholder="اكتب اسم الصنف أو كوده للبحث السريع..."
          value={productSearch}
          onChange={(e) => setProductSearch(e.target.value)}
        />

        <Select
          label="الصنف الخاضع للتسوية *"
          value={selectedProductId}
          onChange={(e) => setSelectedProductId(e.target.value)}
        >
          {products.length === 0 ? (
            <option value="">لا توجد أصناف مطابقة للبحث</option>
          ) : (
            products.map((p) => (
              <option key={p.id} value={p.id}>
                {p.name} ({p.code})
              </option>
            ))
          )}
        </Select>

        <div className="grid grid-cols-2 gap-3">
          <Input
            label="الكمية الدفترية الحالية"
            type="number"
            value={currentQty}
            onChange={(e) => setCurrentQty(Number(e.target.value))}
            isLtr
          />

          <Input
            label="الكمية الفعلية بالجرد"
            type="number"
            value={actualQty}
            onChange={(e) => setActualQty(Number(e.target.value))}
            isLtr
          />
        </div>

        <div className="p-2.5 rounded-lg bg-slate-50 border border-slate-200 flex items-center justify-between text-xs">
          <span className="font-semibold text-slate-700">فرق التسوية:</span>
          <span
            className={`font-bold num-ltr ${
              diff < 0 ? 'text-rose-600' : diff > 0 ? 'text-emerald-600' : 'text-slate-800'
            }`}
          >
            {diff > 0 ? `+${diff}` : diff} قطعة
          </span>
        </div>

        <div>
          <label className="text-xs font-semibold text-slate-700 block mb-1">سبب ومبرر التسوية</label>
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="مثال: تلف أثناء النقل أو فرق جرد شهري..."
            className="w-full text-xs p-2.5 rounded-lg border border-slate-200 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
            rows={2}
          />
        </div>
      </form>
    </Modal>
  );
};

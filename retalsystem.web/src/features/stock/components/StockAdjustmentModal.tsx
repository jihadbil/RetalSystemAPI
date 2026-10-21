import { createDocumentNumber } from '../../../shared/utils/documentNumber';
import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import type { WarehouseDto } from '../types/stock.types';
import { AdjustmentType } from '../types/stock.types';
import { useProductBarcodes } from '../../catalog/hooks/useProductBarcodes';
import { stockService } from '../services/stockService';


interface StockAdjustmentModalProps {
  isOpen: boolean;
  onClose: () => void;
  warehouses: WarehouseDto[];
  onSave: (data: unknown) => Promise<void>;
}

export const StockAdjustmentModal: React.FC<StockAdjustmentModalProps> = ({
  isOpen,
  onClose,
  warehouses: initialWarehouses,
  onSave,
}) => {
  const [warehouseId, setWarehouseId] = useState('');
  const [type, setType] = useState<AdjustmentType>(AdjustmentType.Shortage);
  const [warehouses, setWarehouses] = useState(initialWarehouses);
  useEffect(() => { setWarehouses(initialWarehouses); }, [initialWarehouses]);



  const [selectedProductId, setSelectedProductId] = useState('');
  const [currentQty, setCurrentQty] = useState<number>(0);
  const [actualQty, setActualQty] = useState<number>(0);
  const [reason, setReason] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [adjustmentNumber, setDocumentNumber] = useState(() => createDocumentNumber('ADJ'));
  useEffect(() => { if (isOpen) setDocumentNumber(createDocumentNumber('ADJ')); }, [isOpen]);
  const { product, barcodeId, setBarcodeId, error: barcodeError } = useProductBarcodes(selectedProductId, isOpen);
  const needsBarcode = warehouses.find((w) => w.id === warehouseId)?.type === 1;
  const [stockError, setStockError] = useState('');
  const [submitError, setSubmitError] = useState('');
  const [loadingQuantity, setLoadingQuantity] = useState(false);
  useEffect(() => {
    let cancelled = false;
    setCurrentQty(0);
    setStockError('');
    setLoadingQuantity(false);
    if (isOpen && warehouseId && selectedProductId && (!needsBarcode || barcodeId)) {
      setLoadingQuantity(true);
      void stockService.getQuantity(warehouseId, selectedProductId, needsBarcode ? barcodeId : undefined)
        .then((quantity) => { if (!cancelled) setCurrentQty(quantity); })
        .catch(() => { if (!cancelled) setStockError('تعذر تحميل رصيد المخزون الحالي'); })
        .finally(() => { if (!cancelled) setLoadingQuantity(false); });
    }
    return () => { cancelled = true; };
  }, [isOpen, warehouseId, selectedProductId, needsBarcode, barcodeId]);

  useEffect(() => {
    if (isOpen) setWarehouseId((previous) => initialWarehouses.some((warehouse) => warehouse.id === previous) ? previous : initialWarehouses[0]?.id || '');
  }, [isOpen, initialWarehouses]);

  const selectedProduct = product;
  const diff = actualQty - currentQty;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProductId || !product) { setSubmitError('انتظر تحميل بيانات الصنف'); return; }
    if (loadingQuantity || stockError || !warehouseId || !Number.isInteger(actualQty) || actualQty < 0 || needsBarcode && !barcodeId) {
      setSubmitError('انتظر تحميل الرصيد، وحدد الباركود وأدخل كمية جرد صحيحة غير سالبة');
      return;
    }

    setIsLoading(true);
    setSubmitError('');
    try {
      await onSave({
        adjustmentNumber,
        adjustmentDate: new Date().toISOString(),
        warehouseId: warehouseId || warehouses[0]?.id,
        reason: type === AdjustmentType.Damage ? 2 : 1,
        notes: reason.trim() || undefined,
        items: [
          {
            productId: selectedProductId,
            productBarCodeId: needsBarcode ? barcodeId : undefined,
            systemQuantity: currentQty,
            actualQuantity: actualQty,
            unitCost: product?.averagePrice || selectedProduct?.costPrice || 0,
            reason: type === AdjustmentType.Damage ? 2 : 1,
          },
        ],
      });
      onClose();
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'تعذر حفظ التسوية');
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
          <Button variant="primary" size="md" onClick={handleSubmit} isLoading={isLoading} disabled={loadingQuantity || !!stockError || !!barcodeError}>
            اعتماد التسوية
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-3 py-1">
        {(submitError || stockError || barcodeError) && <p className="text-xs text-rose-600">{submitError || stockError || barcodeError}</p>}
        <Select
          lookup={{ resource: 'warehouses' }} onLookupSelect={(option) => setWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="المستودع *"
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



        <Select
          lookup={{ resource: 'products' }} label="الصنف الخاضع للتسوية *"
          value={selectedProductId}
          onChange={(e) => setSelectedProductId(e.target.value)}
        >
          <option value="">اختر الصنف</option>
        </Select>

        {needsBarcode && <Select label="الباركود / البديل *" value={barcodeId} onChange={(e) => setBarcodeId(e.target.value)}>
          <option value="">اختر الباركود</option>
          {product?.barCodes?.map((barcode) => <option key={barcode.id} value={barcode.id}>{barcode.title || barcode.barCode} — {barcode.barCode}</option>)}
        </Select>}
        <div className="grid grid-cols-2 gap-3">
          <Input
            label="الكمية الدفترية الحالية"
            type="number"
            value={currentQty}
            readOnly
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

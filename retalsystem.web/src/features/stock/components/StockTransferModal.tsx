import { createDocumentNumber } from '../../../shared/utils/documentNumber';
import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import type { WarehouseDto } from '../types/stock.types';
import { useProductBarcodes } from '../../catalog/hooks/useProductBarcodes';
import { stockService } from '../services/stockService';

interface StockTransferModalProps {
  isOpen: boolean;
  onClose: () => void;
  warehouses: WarehouseDto[];
  onSave: (data: unknown) => Promise<void>;
}

export const StockTransferModal: React.FC<StockTransferModalProps> = ({
  isOpen,
  onClose,
  warehouses,
  onSave,
}) => {
  const [sourceWarehouseId, setSourceWarehouseId] = useState('');
  const [destWarehouseId, setDestWarehouseId] = useState('');

  const [selectedProductId, setSelectedProductId] = useState('');
  const [quantity, setQuantity] = useState<number>(1);
  const [notes, setNotes] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [transferNumber, setDocumentNumber] = useState(() => createDocumentNumber('TRF'));
  useEffect(() => { if (isOpen) setDocumentNumber(createDocumentNumber('TRF')); }, [isOpen]);
  const { product, barcodeId, setBarcodeId, error: barcodeError } = useProductBarcodes(selectedProductId, isOpen);
  const [error, setError] = useState('');
  const [destWarehouses, setDestWarehouses] = useState<WarehouseDto[]>([]);
  const needsBarcode = [...warehouses, ...destWarehouses].some((w) => (w.id === sourceWarehouseId || w.id === destWarehouseId) && w.type === 1);

  useEffect(() => {
    if (!isOpen) return;
    let cancelled = false;
    void stockService.getWarehouses().then((all) => {
      if (cancelled) return;
      const active = all.filter((warehouse) => warehouse.isActive);
      setDestWarehouses(active);
    }).catch(() => { if (!cancelled) setError('تعذر تحميل المستودعات الوجهة'); });
    return () => { cancelled = true; };
  }, [isOpen]);

  useEffect(() => {
    const choices = destWarehouses.filter((warehouse) => warehouse.id !== sourceWarehouseId);
    setDestWarehouseId((previous) => choices.some((warehouse) => warehouse.id === previous) ? previous : choices[0]?.id || '');
  }, [destWarehouses, sourceWarehouseId]);

  useEffect(() => {
    if (isOpen) {
      if (warehouses.length > 0) {
        setSourceWarehouseId(warehouses[0].id);
        if (warehouses.length > 1) {
          setDestWarehouseId(warehouses[1].id);
        }
      }

    }
  }, [isOpen, warehouses]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProductId || !sourceWarehouseId || !destWarehouseId || quantity <= 0 || isLoading) return;
    if (sourceWarehouseId === destWarehouseId) return;
    if (!Number.isInteger(quantity) || (needsBarcode && !barcodeId)) {
      setError('اختر باركود الصنف وأدخل كمية صحيحة موجبة');
      return;
    }

    setIsLoading(true);
    try {
      await onSave({
        transferNumber,
        transferDate: new Date().toISOString(),
        fromWarehouseId: sourceWarehouseId,
        toWarehouseId: destWarehouseId,
        notes: notes.trim() || undefined,
        items: [{ productId: selectedProductId, productBarCodeId: barcodeId || undefined, quantity: Number(quantity) }],
      });
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'تعذر حفظ التحويل');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إنشاء تحويل مخزني جديد"
      subtitle="نقل بضائع وأصناف بين المستودعات وصالات العرض"
      size="md"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose} disabled={isLoading}>
            إلغاء
          </Button>
          <Button variant="primary" size="md" onClick={handleSubmit} isLoading={isLoading}>
            إصدار أمر التحويل
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-3 py-1">
        {(error || barcodeError) && <p className="text-xs text-rose-600">{error || barcodeError}</p>}
        <div className="grid grid-cols-2 gap-3">
          <Select
            lookup={{ resource: 'warehouses' }} onLookupSelect={(option) => setDestWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="المستودع المصدر (من) *"
            value={sourceWarehouseId}
            onChange={(e) => setSourceWarehouseId(e.target.value)}
          >
            {warehouses.map((w) => (
              <option key={w.id} value={w.id}>
                {w.name}
              </option>
            ))}
          </Select>

          <Select
            lookup={{ resource: 'warehouses' }} onLookupSelect={(option) => setDestWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="المستودع المستلم (إلى) *"
            value={destWarehouseId}
            onChange={(e) => setDestWarehouseId(e.target.value)}
          >
            {destWarehouses
              .filter((w) => w.id !== sourceWarehouseId)
              .map((w) => (
                <option key={w.id} value={w.id}>
                  {w.name}
                </option>
              ))}
          </Select>
        </div>

        <Select
          lookup={{ resource: 'products' }} label="الصنف المراد تحويله *"
          value={selectedProductId}
          onChange={(e) => setSelectedProductId(e.target.value)}
        >
<option value="">اختر الصنف</option>
        </Select>

        {needsBarcode && <Select label="الباركود / البديل *" value={barcodeId} onChange={(e) => setBarcodeId(e.target.value)}>
          <option value="">اختر الباركود</option>
          {product?.barCodes?.map((barcode) => <option key={barcode.id} value={barcode.id}>{barcode.title || barcode.barCode} — {barcode.barCode}</option>)}
        </Select>}

        <Input
          label="الكمية المحولة *"
          type="number"
          min="1"
          value={quantity}
          onChange={(e) => setQuantity(Math.max(1, Number(e.target.value)))}
          isLtr
          required
        />

        <div>
          <label className="text-xs font-semibold text-slate-700 block mb-1">ملاحظات التحويل</label>
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="مثال: تزويد صالة العرض بالنواقص..."
            className="w-full text-xs p-2.5 rounded-lg border border-slate-200 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
            rows={2}
          />
        </div>
      </form>
    </Modal>
  );
};

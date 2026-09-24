import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import type { WarehouseDto } from '../types/stock.types';
import { catalogService } from '../../catalog/services/catalogService';
import type { ProductDto } from '../../catalog/types/catalog.types';

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
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [quantity, setQuantity] = useState<number>(1);
  const [notes, setNotes] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (isOpen) {
      if (warehouses.length > 0) {
        setSourceWarehouseId(warehouses[0].id);
        if (warehouses.length > 1) {
          setDestWarehouseId(warehouses[1].id);
        }
      }
      catalogService
        .getProducts({ pageNumber: 1, pageSize: 100 })
        .then((res) => {
          const prods = res.items || [];
          setProducts(prods);
          if (prods.length > 0) {
            setSelectedProductId(prods[0].id);
          }
        })
        .catch((err) => console.error('Failed to load products for transfer:', err));
    }
  }, [isOpen, warehouses]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedProductId || quantity <= 0) return;
    if (sourceWarehouseId === destWarehouseId) return;

    setIsLoading(true);
    try {
      const transferNumber = `TRF-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.floor(
        100 + Math.random() * 900
      )}`;
      await onSave({
        transferNumber,
        transferDate: new Date().toISOString(),
        fromWarehouseId: sourceWarehouseId,
        toWarehouseId: destWarehouseId,
        notes: notes.trim() || undefined,
        items: [{ productId: selectedProductId, quantity: Number(quantity) }],
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
        <div className="grid grid-cols-2 gap-3">
          <Select
            label="المستودع المصدر (من) *"
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
            label="المستودع المستلم (إلى) *"
            value={destWarehouseId}
            onChange={(e) => setDestWarehouseId(e.target.value)}
          >
            {warehouses
              .filter((w) => w.id !== sourceWarehouseId)
              .map((w) => (
                <option key={w.id} value={w.id}>
                  {w.name}
                </option>
              ))}
          </Select>
        </div>

        <Select
          label="الصنف المراد تحويله *"
          value={selectedProductId}
          onChange={(e) => setSelectedProductId(e.target.value)}
        >
          {products.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name} ({p.code})
            </option>
          ))}
        </Select>

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

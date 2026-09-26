import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { catalogService } from '../../catalog/services/catalogService';
import type { CategoryDto, UnitDto, ProductDto } from '../../catalog/types/catalog.types';
import { IconPlus } from '@tabler/icons-react';

interface QuickProductModalProps {
  isOpen: boolean;
  onClose: () => void;
  prefillBarcode?: string;
  categories: CategoryDto[];
  units: UnitDto[];
  onProductCreated: (product: ProductDto) => void;
}

export const QuickProductModal: React.FC<QuickProductModalProps> = ({
  isOpen,
  onClose,
  prefillBarcode = '',
  categories,
  units,
  onProductCreated,
}) => {
  const [name, setName] = useState('');
  const [code, setCode] = useState('');
  const [barCode, setBarCode] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [defaultUnitId, setDefaultUnitId] = useState('');
  const [costPrice, setCostPrice] = useState<number>(0);
  const [salePrice, setSalePrice] = useState<number>(0);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setName('');
      setBarCode(prefillBarcode || '');
      setCode(prefillBarcode ? `PRD-${prefillBarcode}` : `PRD-${Date.now().toString().slice(-6)}`);
      setCostPrice(0);
      setSalePrice(0);
      setError(null);
      if (categories.length > 0) setCategoryId(categories[0].id);
      if (units.length > 0) setDefaultUnitId(units[0].id);
    }
  }, [isOpen, prefillBarcode, categories, units]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setError('يرجى إدخال اسم الصنف');
      return;
    }
    if (!categoryId) {
      setError('يرجى اختيار التصنيف');
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const created = await catalogService.createProduct({
        name: name.trim(),
        code: code.trim() || `PRD-${Date.now().toString().slice(-6)}`,
        categoryId,
        defaultUnitId: defaultUnitId || undefined,
        costPrice: Number(costPrice) || 0,
        salePrice: Number(salePrice) || 0,
        minStockLevel: 5,
        isActive: true,
        barCodes: barCode.trim() ? [{ barCode: barCode.trim(), description: 'الباركود الرئيسي' }] : undefined,
      });

      onProductCreated(created);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل إنشاء الصنف الجديد';
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="تعريف صنف جديد سريعاً"
      subtitle="إدخال صنف جديد إلى كتالوج المنتجات وربطه فوراً بفاتورة المشتريات"
      size="md"
      footer={
        <div className="flex justify-end gap-2 w-full">
          <Button variant="secondary" size="md" onClick={onClose} disabled={isLoading}>
            إلغاء
          </Button>
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconPlus className="w-4 h-4" />}
            onClick={handleSubmit}
            isLoading={isLoading}
          >
            حفظ وإدراج في الفاتورة
          </Button>
        </div>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 py-1">
        {error && (
          <div className="p-3 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs font-semibold">
            {error}
          </div>
        )}

        <Input
          label="اسم الصنف *"
          placeholder="مثال: شوكولاتة نوتيلا 750 جم"
          value={name}
          onChange={(e) => setName(e.target.value)}
          autoFocus
          required
        />

        <div className="grid grid-cols-2 gap-3">
          <Input
            label="الباركود الدولي"
            placeholder="امسح أو اكتب الباركود..."
            value={barCode}
            onChange={(e) => setBarCode(e.target.value)}
            isLtr
          />

          <Input
            label="كود الصنف الداخلي"
            placeholder="PRD-001"
            value={code}
            onChange={(e) => setCode(e.target.value)}
            isLtr
          />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <Select
            label="التصنيف *"
            value={categoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            required
          >
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </Select>

          <Select
            label="وحدة القياس الافتراضية"
            value={defaultUnitId}
            onChange={(e) => setDefaultUnitId(e.target.value)}
          >
            {units.map((u) => (
              <option key={u.id} value={u.id}>
                {u.name} ({u.symbol})
              </option>
            ))}
          </Select>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <Input
            label="سعر الشراء المبدئي (د.ل)"
            type="number"
            step="0.01"
            min="0"
            value={costPrice}
            onChange={(e) => setCostPrice(Number(e.target.value))}
            isLtr
          />

          <Input
            label="سعر البيع المستهدف (د.ل)"
            type="number"
            step="0.01"
            min="0"
            value={salePrice}
            onChange={(e) => setSalePrice(Number(e.target.value))}
            isLtr
          />
        </div>
      </form>
    </Modal>
  );
};

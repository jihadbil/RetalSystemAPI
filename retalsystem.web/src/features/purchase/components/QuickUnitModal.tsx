import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { catalogService } from '../../catalog/services/catalogService';
import type { UnitDto } from '../../catalog/types/catalog.types';
import { IconScale } from '@tabler/icons-react';

interface QuickUnitModalProps {
  isOpen: boolean;
  onClose: () => void;
  onUnitCreated: (unit: UnitDto) => void;
}

export const QuickUnitModal: React.FC<QuickUnitModalProps> = ({
  isOpen,
  onClose,
  onUnitCreated,
}) => {
  const [name, setName] = useState('');
  const [symbol, setSymbol] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setError('يرجى كتابة اسم وحدة القياس (مثل: صندوق، دستة، باكت)');
      return;
    }

    setIsLoading(true);
    setError(null);
    try {
      const created = await catalogService.createUnit({
        name: name.trim(),
        symbol: symbol.trim() || name.trim().slice(0, 3),
        isBaseUnit: false,
      });
      onUnitCreated(created);
      setName('');
      setSymbol('');
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل إضافة وحدة القياس';
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إضافة وحدة قياس جديدة"
      subtitle="تعريف وحدة تعبئة جديدة للاستخدام في فواتير الشراء والتسعير"
      size="sm"
      footer={
        <div className="flex justify-end gap-2 w-full">
          <Button variant="secondary" size="md" onClick={onClose} disabled={isLoading}>
            إلغاء
          </Button>
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconScale className="w-4 h-4" />}
            onClick={handleSubmit}
            isLoading={isLoading}
          >
            حفظ الوحدة
          </Button>
        </div>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-3 py-1">
        {error && (
          <div className="p-2.5 rounded-lg bg-rose-50 border border-rose-200 text-rose-700 text-xs font-semibold">
            {error}
          </div>
        )}
        <Input
          label="اسم الوحدة *"
          placeholder="مثال: كرتونة، باكت، صندوق 24"
          value={name}
          onChange={(e) => setName(e.target.value)}
          autoFocus
          required
        />
        <Input
          label="الرمز أو الاختصار"
          placeholder="مثال: كرتون، صند، باكت"
          value={symbol}
          onChange={(e) => setSymbol(e.target.value)}
        />
      </form>
    </Modal>
  );
};

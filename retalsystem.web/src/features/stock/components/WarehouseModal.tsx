import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { organizationService } from '../../organization/services/organizationService';
import { useAuth } from '../../../shared/context/AuthContext';
import type { BranchDto } from '../../organization/types/organization.types';
import type { WarehouseDto } from '../types/stock.types';

interface WarehouseModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: Partial<WarehouseDto>) => Promise<void>;
}

export const WarehouseModal: React.FC<WarehouseModalProps> = ({ isOpen, onClose, onSave }) => {
  const { currentBranchId } = useAuth();
  const [name, setName] = useState('');
  const [branchId, setBranchId] = useState<string>('');
  const [type, setType] = useState<number>(1); // 1 = Storage, 2 = Showroom
  const [branches, setBranches] = useState<BranchDto[]>([]);
  const [isLoadingBranches, setIsLoadingBranches] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setName('');
      setType(1);
      setError(null);
      setIsLoadingBranches(true);
      organizationService
        .getBranches()
        .then((data) => {
          setBranches(data);
          if (currentBranchId && data.some((b) => b.id === currentBranchId)) {
            setBranchId(currentBranchId);
          } else if (data.length > 0) {
            setBranchId(data[0].id);
          }
        })
        .catch((err) => {
          console.error('Failed to load branches:', err);
          setError('فشل تحميل قائمة الفروع');
        })
        .finally(() => setIsLoadingBranches(false));
    }
  }, [isOpen, currentBranchId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setError('اسم المستودع مطلوب');
      return;
    }
    if (!branchId) {
      setError('يرجى تحديد الفرع التابع له المستودع');
      return;
    }

    setIsSubmitting(true);
    try {
      await onSave({
        name: name.trim(),
        branchId,
        type,
        isActive: true,
      });
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل حفظ المستودع';
      setError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إضافة مستودع جديد"
      subtitle="تسجيل مستودع تخزيني أو صالة عرض تابعة لفرع المنشأة"
      size="sm"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose} disabled={isSubmitting}>
            إلغاء
          </Button>
          <Button
            variant="primary"
            size="md"
            onClick={handleSubmit}
            isLoading={isSubmitting}
            disabled={isLoadingBranches || !branchId}
          >
            حفظ المستودع
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 py-1">
        {error && (
          <div className="p-3 bg-rose-50 text-rose-700 text-xs rounded-lg border border-rose-200">
            {error}
          </div>
        )}

        <Input
          label="اسم المستودع أو الصالة *"
          placeholder="مثال: مخزن المواد الغذائية الرئيسي"
          value={name}
          onChange={(e) => {
            setName(e.target.value);
            setError(null);
          }}
          required
          autoFocus
        />

        <Select
          label="الفرع التابع له *"
          value={branchId}
          onChange={(e) => setBranchId(e.target.value)}
          disabled={isLoadingBranches || branches.length === 0}
          options={branches.map((b) => ({
            value: b.id,
            label: b.name + (b.address ? ` (${b.address})` : ''),
          }))}
          helperText={isLoadingBranches ? 'جاري تحميل الفروع...' : undefined}
        />

        <Select
          label="نوع المستودع *"
          value={type}
          onChange={(e) => setType(Number(e.target.value))}
          options={[
            { value: 1, label: 'مستودع تخزيني (Storage)' },
            { value: 2, label: 'صالة عرض ومبيعات (Showroom)' },
          ]}
        />
      </form>
    </Modal>
  );
};

import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Button } from '../../../shared/components/ui/Button';
import { SafeDto, SafeType } from '../types/accounting.types';
import { accountingService, CreateSafeRequest } from '../services/accountingService';
import { useToast } from '../../../shared/context/ToastContext';
import { IconVault, IconBuildingBank, IconCash } from '@tabler/icons-react';
import { useAuth } from '../../../shared/context/AuthContext';

interface SafeFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  safeToEdit?: SafeDto | null;
  onSaved: () => void;
}

export const SafeFormModal: React.FC<SafeFormModalProps> = ({
  isOpen,
  onClose,
  safeToEdit,
  onSaved,
}) => {
  const { showSuccess, showError } = useToast();
  const { currentBranchId } = useAuth();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState<CreateSafeRequest>({
    name: '',
    type: SafeType.CashBox,
    branchId: currentBranchId || '',
    openingBalance: 0,
    bankAccountNumber: '',
    bankName: '',
    notes: '',
    isActive: true,
  });

  const [error, setError] = useState('');

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setFormData(safeToEdit ? {
          name: safeToEdit.name,
          type: safeToEdit.type,
          branchId: safeToEdit.branchId || currentBranchId || '',
          openingBalance: safeToEdit.openingBalance || 0,
          bankAccountNumber: safeToEdit.bankAccountNumber || '',
          bankName: safeToEdit.bankName || '',
          notes: safeToEdit.notes || '',
          isActive: safeToEdit.isActive,
        } : {
          name: '', type: SafeType.CashBox, branchId: currentBranchId || '', openingBalance: 0,
          bankAccountNumber: '', bankName: '', notes: '', isActive: true,
        });
      setError('');
    }, 0);
    return () => window.clearTimeout(timer);
  }, [safeToEdit, isOpen, currentBranchId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formData.name.trim()) {
      setError('اسم الخزينة أو الحساب البنكي مطلوب');
      return;
    }
    if (!formData.branchId) {
      setError('يجب اختيار فرع نشط قبل إنشاء الخزينة');
      return;
    }

    try {
      setIsSubmitting(true);
      if (safeToEdit) {
        await accountingService.updateSafe(safeToEdit.id, {
          id: safeToEdit.id,
          ...formData,
        });
        showSuccess('تم تحديث بيانات الحساب/الخزينة بنجاح');
      } else {
        await accountingService.createSafe(formData);
        showSuccess('تم إنشاء الخزينة/الحساب الجديد بنجاح');
      }
      onSaved();
      onClose();
    } catch (err: unknown) {
      showError(err instanceof Error ? err.message : 'فشل حفظ الخزينة');
    } finally {
      setIsSubmitting(false);
    }
  };

  const isBank = formData.type === SafeType.Bank;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={safeToEdit ? 'تعديل بيانات الخزينة / الحساب' : 'إضافة خزينة أو حساب بنكي'}
      subtitle="إدارة الصناديق النقدية والحسابات المصرفية وأدراج الكاشير"
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <Input
          label="اسم الخزينة أو الحساب *"
          placeholder="مثال: الخزينة النقدية الرئيسية / حساب الراجحي"
          value={formData.name}
          onChange={(e) => {
            setFormData({ ...formData, name: e.target.value });
            setError('');
          }}
          error={error}
          leftIcon={isBank ? <IconBuildingBank className="w-4 h-4" /> : <IconVault className="w-4 h-4" />}
          autoFocus
        />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            label="نوع الحساب *"
            value={String(formData.type)}
            onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value, 10) as SafeType })}
            options={[
              { value: String(SafeType.CashBox), label: 'خزينة رئيسية (صندوق نقدي)' },
              { value: String(SafeType.Bank), label: 'حساب مصرفي / بنك' },
              { value: String(SafeType.CashDrawer), label: 'درج نقطة بيع (كاشير)' },
            ]}
          />

          {!safeToEdit && (
            <Input
              label="الرصيد الافتتاحي (د.ل)"
              type="number"
              step="0.01"
              value={formData.openingBalance}
              onChange={(e) => setFormData({ ...formData, openingBalance: parseFloat(e.target.value) || 0 })}
              className="num-ltr text-right"
              leftIcon={<IconCash className="w-4 h-4" />}
            />
          )}
        </div>

        {isBank && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 p-3 bg-slate-50 border border-slate-200/80 rounded-xl">
            <Input
              label="اسم البنك"
              placeholder="مثال: مصرف الراجحي"
              value={formData.bankName}
              onChange={(e) => setFormData({ ...formData, bankName: e.target.value })}
            />

            <Input
              label="رقم الآيبان (IBAN) أو الحساب"
              placeholder="SA..."
              value={formData.bankAccountNumber}
              onChange={(e) => setFormData({ ...formData, bankAccountNumber: e.target.value })}
              className="num-ltr text-right font-mono"
            />
          </div>
        )}

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-slate-700">ملاحظات</label>
          <input
            type="text"
            className="w-full h-10 px-3 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            placeholder="ملاحظات حول موقع الخزينة أو الاستخدام..."
            value={formData.notes}
            onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
          />
        </div>

        <div className="flex items-center gap-2 pt-2 border-t border-slate-100">
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={formData.isActive}
              onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              className="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
            />
            <span className="text-sm font-semibold text-slate-800">الحساب نشط وجاهز للعمليات المالية</span>
          </label>
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            {safeToEdit ? 'حفظ التعديلات' : 'إضافة الحساب'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};

import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Button } from '../../../shared/components/ui/Button';
import { BranchDto } from '../types/organization.types';
import { organizationService, CreateBranchRequest } from '../services/organizationService';
import { useToast } from '../../../shared/context/ToastContext';
import { IconBuildingStore, IconBarcode, IconPhone, IconMapPin } from '@tabler/icons-react';

interface BranchFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  branchToEdit?: BranchDto | null;
  onSaved: () => void;
}

export const BranchFormModal: React.FC<BranchFormModalProps> = ({
  isOpen,
  onClose,
  branchToEdit,
  onSaved,
}) => {
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState<CreateBranchRequest>({
    name: '',
    code: '',
    phone: '',
    address: '',
    isActive: true,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (branchToEdit) {
      setFormData({
        name: branchToEdit.name,
        code: branchToEdit.code,
        phone: branchToEdit.phone || '',
        address: branchToEdit.address || '',
        isActive: branchToEdit.isActive,
      });
    } else {
      setFormData({
        name: '',
        code: `BR-${Math.floor(10 + Math.random() * 90)}`,
        phone: '',
        address: '',
        isActive: true,
      });
    }
    setErrors({});
  }, [branchToEdit, isOpen]);

  const validate = () => {
    const errs: Record<string, string> = {};
    if (!formData.name.trim()) errs.name = 'اسم الفرع مطلوب';
    if (!formData.code.trim()) errs.code = 'رمز أو كود الفرع مطلوب';
    if (formData.phone?.trim() && !/^(091|092|093|094|095)[0-9]{7}$/.test(formData.phone.trim())) {
      errs.phone = 'رقم الهاتف الليبي يجب أن يبدأ بـ (091, 092, 093, 094, 095) ويتكون من 10 أرقام';
    }
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      setIsSubmitting(true);
      if (branchToEdit) {
        await organizationService.updateBranch(branchToEdit.id, {
          id: branchToEdit.id,
          ...formData,
        });
        showSuccess('تم تحديث بيانات الفرع بنجاح');
      } else {
        await organizationService.createBranch(formData);
        showSuccess('تمت إضافة الفرع الجديد بنجاح');
      }
      onSaved();
      onClose();
    } catch (err: unknown) {
      showError(err instanceof Error ? err.message : 'فشل حفظ الفرع');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={branchToEdit ? 'تعديل بيانات الفرع' : 'إضافة فرع جديد'}
      subtitle="إدارة فروع المؤسسة ومنافذ البيع ونقاط التوزيع"
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <Input
          label="اسم الفرع أو المنفذ *"
          placeholder="مثال: فرع الملز - الرياض"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          error={errors.name}
          leftIcon={<IconBuildingStore className="w-4 h-4" />}
          autoFocus
        />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="رمز الفرع (الكود) *"
            placeholder="BR-01"
            value={formData.code}
            onChange={(e) => setFormData({ ...formData, code: e.target.value })}
            error={errors.code}
            leftIcon={<IconBarcode className="w-4 h-4" />}
            className="num-ltr text-right font-mono"
          />

          <Input
            label="رقم الهاتف"
            placeholder="091xxxxxxx"
            value={formData.phone}
            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
            error={errors.phone}
            leftIcon={<IconPhone className="w-4 h-4" />}
            className="num-ltr text-right"
          />
        </div>

        <Input
          label="العنوان والموقع الجغرافي"
          placeholder="مثال: الرياض - طريق صلاح الدين الأيوبي"
          value={formData.address}
          onChange={(e) => setFormData({ ...formData, address: e.target.value })}
          leftIcon={<IconMapPin className="w-4 h-4" />}
        />

        <div className="flex items-center gap-2 pt-2 border-t border-slate-100">
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={formData.isActive}
              onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
              className="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
            />
            <span className="text-sm font-semibold text-slate-800">الفرع نشط ومتاح للمبيعات وإصدار الفواتير</span>
          </label>
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            {branchToEdit ? 'حفظ التعديلات' : 'إضافة الفرع'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};

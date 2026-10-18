import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Button } from '../../../shared/components/ui/Button';
import { SupplierDto } from '../types/supplier.types';
import { CreateSupplierRequest, supplierService } from '../services/supplierService';
import { useToast } from '../../../shared/context/ToastContext';
import {
  IconTruckDelivery,
  IconBuildingStore,
  IconPhone,
  IconMail,
  IconMapPin,
  IconReceiptTax,
} from '@tabler/icons-react';

interface SupplierFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  supplierToEdit?: SupplierDto | null;
  onSaved: () => void;
}

export const SupplierFormModal: React.FC<SupplierFormModalProps> = ({
  isOpen,
  onClose,
  supplierToEdit,
  onSaved,
}) => {
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState<CreateSupplierRequest>({
    name: '',
    companyName: '',
    phone: '',
    email: '',
    address: '',
    taxNumber: '',
    notes: '',
    isActive: true,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (supplierToEdit) {
      setFormData({
        name: supplierToEdit.name,
        phones: supplierToEdit.phones,
        companyName: supplierToEdit.companyName || '',
        phone: supplierToEdit.phone || '',
        email: supplierToEdit.email || '',
        address: supplierToEdit.address || '',
        taxNumber: supplierToEdit.taxNumber || '',
        notes: supplierToEdit.notes || '',
        isActive: supplierToEdit.isActive,
      });
    } else {
      setFormData({
        name: '',
        companyName: '',
        phone: '',
        email: '',
        address: '',
        taxNumber: '',
        notes: '',
        isActive: true,
      });
    }
    setErrors({});
  }, [supplierToEdit, isOpen]);

  const validate = () => {
    const errs: Record<string, string> = {};
    if (!formData.name.trim()) errs.name = 'اسم المورد أو المسؤول مطلوب';
    if (!formData.phone.trim()) errs.phone = 'رقم الهاتف مطلوب';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      setIsSubmitting(true);
      if (supplierToEdit) {
        await supplierService.update(supplierToEdit.id, {
          id: supplierToEdit.id,
          ...formData,
        });
        showSuccess('تم تحديث بيانات المورد بنجاح');
      } else {
        await supplierService.create(formData);
        showSuccess('تمت إضافة المورد الجديد بنجاح');
      }
      onSaved();
      onClose();
    } catch (err: unknown) {
      showError(err instanceof Error ? err.message : 'فشل حفظ بيانات المورد');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={supplierToEdit ? 'تعديل بيانات المورد' : 'إضافة مورد جديد'}
      subtitle="سجل بيانات جهات التوريد والشركات الموردة"
      size="lg"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="اسم المورد أو جهة الاتصال *"
            placeholder="مثال: عبدالله بن سالم"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            error={errors.name}
            leftIcon={<IconTruckDelivery className="w-4 h-4" />}
            autoFocus
          />

          <Input
            label="اسم الشركة أو المؤسسة"
            placeholder="مثال: شركة التوزيع المتحدة"
            value={formData.companyName}
            onChange={(e) => setFormData({ ...formData, companyName: e.target.value })}
            leftIcon={<IconBuildingStore className="w-4 h-4" />}
          />

          <Input
            label="رقم الجوال *"
            placeholder="05xxxxxxxx"
            value={formData.phone}
            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
            error={errors.phone}
            leftIcon={<IconPhone className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="البريد الإلكتروني"
            type="email"
            placeholder="supplier@company.com"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            leftIcon={<IconMail className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="الرقم الضريبي للمنشأة الموردة"
            placeholder="3xxxxxxxxxxxxxx"
            value={formData.taxNumber}
            onChange={(e) => setFormData({ ...formData, taxNumber: e.target.value })}
            leftIcon={<IconReceiptTax className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="العنوان والمستودع الرئيسي"
            placeholder="مثال: جدة - المنطقة الصناعية"
            value={formData.address}
            onChange={(e) => setFormData({ ...formData, address: e.target.value })}
            leftIcon={<IconMapPin className="w-4 h-4" />}
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-slate-700">ملاحظات وشروط الدفع</label>
          <textarea
            className="w-full px-3 py-2 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 resize-none h-20 transition-all"
            placeholder="شروط التوريد، فترات السماح، والاتفاقيات..."
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
            <span className="text-sm font-semibold text-slate-800">حساب المورد نشط ومتاح لأوامر الشراء</span>
          </label>
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            {supplierToEdit ? 'حفظ التعديلات' : 'إضافة المورد'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};

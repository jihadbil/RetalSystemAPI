import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Button } from '../../../shared/components/ui/Button';
import { CustomerDto } from '../types/customer.types';
import { CreateCustomerRequest, customerService } from '../services/customerService';
import { useToast } from '../../../shared/context/ToastContext';
import { IconUser, IconPhone, IconMail, IconMapPin, IconReceiptTax, IconBuildingBank } from '@tabler/icons-react';

interface CustomerFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  customerToEdit?: CustomerDto | null;
  onSaved: () => void;
}

export const CustomerFormModal: React.FC<CustomerFormModalProps> = ({
  isOpen,
  onClose,
  customerToEdit,
  onSaved,
}) => {
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState<CreateCustomerRequest>({
    name: '',
    phone: '',
    email: '',
    address: '',
    taxNumber: '',
    commercialRegister: '',
    creditLimit: 0,
    discountRate: 0,
    notes: '',
    isActive: true,
  });

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (customerToEdit) {
      setFormData({
        name: customerToEdit.name,
        code: customerToEdit.code,
        type: customerToEdit.type,
        phones: customerToEdit.phones,
        phone: customerToEdit.phone || '',
        email: customerToEdit.email || '',
        address: customerToEdit.address || '',
        taxNumber: customerToEdit.taxNumber || '',
        commercialRegister: customerToEdit.commercialRegister || '',
        creditLimit: customerToEdit.creditLimit || 0,
        discountRate: customerToEdit.discountRate || 0,
        notes: customerToEdit.notes || '',
        isActive: customerToEdit.isActive,
      });
    } else {
      setFormData({
        name: '',
        phone: '',
        email: '',
        address: '',
        taxNumber: '',
        commercialRegister: '',
        creditLimit: 0,
        discountRate: 0,
        notes: '',
        isActive: true,
      });
    }
    setErrors({});
  }, [customerToEdit, isOpen]);

  const validate = () => {
    const errs: Record<string, string> = {};
    if (!formData.name.trim()) errs.name = 'اسم العميل مطلوب';
    if (!formData.phone.trim()) errs.phone = 'رقم الهاتف مطلوب';
    if (formData.creditLimit < 0) errs.creditLimit = 'سقف الائتمان لا يمكن أن يكون سالباً';
    if (formData.discountRate < 0 || formData.discountRate > 100)
      errs.discountRate = 'نسبة الخصم يجب أن تكون بين 0 و 100%';
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      setIsSubmitting(true);
      if (customerToEdit) {
        await customerService.update(customerToEdit.id, {
          id: customerToEdit.id,
          ...formData,
        });
        showSuccess('تم تحديث بيانات العميل بنجاح');
      } else {
        await customerService.create(formData);
        showSuccess('تمت إضافة العميل الجديد بنجاح');
      }
      onSaved();
      onClose();
    } catch (err: unknown) {
      showError(err instanceof Error ? err.message : 'حدث خطأ أثناء حفظ العميل');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={customerToEdit ? 'تعديل بيانات العميل' : 'إضافة عميل جديد'}
      subtitle="إدارة بيانات العميل والحدود الائتمانية والبيانات الضريبية"
      size="lg"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="اسم العميل أو المؤسسة *"
            placeholder="مثال: شركة الأفق للتجارة"
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            error={errors.name}
            leftIcon={<IconUser className="w-4 h-4" />}
            autoFocus
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
            placeholder="client@example.com"
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            leftIcon={<IconMail className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="العنوان والمدينة"
            placeholder="مثال: الرياض - حي الصحافة"
            value={formData.address}
            onChange={(e) => setFormData({ ...formData, address: e.target.value })}
            leftIcon={<IconMapPin className="w-4 h-4" />}
          />

          <Input
            label="الرقم الضريبي"
            placeholder="3xxxxxxxxxxxxxx"
            value={formData.taxNumber}
            onChange={(e) => setFormData({ ...formData, taxNumber: e.target.value })}
            leftIcon={<IconReceiptTax className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="رقم السجل التجاري"
            placeholder="1010xxxxxx"
            value={formData.commercialRegister}
            onChange={(e) => setFormData({ ...formData, commercialRegister: e.target.value })}
            leftIcon={<IconBuildingBank className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="سقف الائتمان (د.ل)"
            type="number"
            placeholder="0.00"
            value={formData.creditLimit}
            onChange={(e) => setFormData({ ...formData, creditLimit: parseFloat(e.target.value) || 0 })}
            error={errors.creditLimit}
            className="num-ltr text-right"
          />

          <Input
            label="نسبة الخصم المرجعية (%)"
            type="number"
            placeholder="0"
            value={formData.discountRate}
            onChange={(e) => setFormData({ ...formData, discountRate: parseFloat(e.target.value) || 0 })}
            error={errors.discountRate}
            className="num-ltr text-right"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-slate-700">ملاحظات إضافية</label>
          <textarea
            className="w-full px-3 py-2 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 resize-none h-20 transition-all"
            placeholder="أي تفاصيل خاصة بشروط التعامل أو السداد..."
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
            <span className="text-sm font-semibold text-slate-800">حساب العميل نشط ومتاح للتعامل</span>
          </label>
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            {customerToEdit ? 'حفظ التعديلات' : 'إضافة العميل'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};

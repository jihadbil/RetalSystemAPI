import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Button } from '../../../shared/components/ui/Button';
import { UserItemDto } from '../types/user.types';
import { userService } from '../services/userService';
import { organizationService } from '../../organization/services/organizationService';
import { BranchDto } from '../../organization/types/organization.types';
import { useToast } from '../../../shared/context/ToastContext';
import { IconUser, IconKey, IconPhone, IconMail } from '@tabler/icons-react';

interface UserFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  userToEdit?: UserItemDto | null;
  onSaved: () => void;
}

export const UserFormModal: React.FC<UserFormModalProps> = ({
  isOpen,
  onClose,
  userToEdit,
  onSaved,
}) => {
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [branches, setBranches] = useState<BranchDto[]>([]);
  const [userName, setUserName] = useState('');
  const [fullName, setFullName] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState('Cashier');
  const [branchId, setBranchId] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [isActive, setIsActive] = useState(true);

  const [errors, setErrors] = useState<Record<string, string>>({});

  useEffect(() => {
    if (isOpen) {
      organizationService
        .getBranches()
        .then((data) => {
          const activeBranches = (data || []).filter((b) => b.isActive);
          setBranches(activeBranches);
          if (!userToEdit && activeBranches.length > 0) {
            setBranchId((prev) => prev || activeBranches[0].id);
          }
        })
        .catch((err) => {
          console.error('Failed to fetch branches:', err);
        });
    }
  }, [isOpen, userToEdit]);

  useEffect(() => {
    if (userToEdit) {
      setUserName(userToEdit.userName);
      setFullName(userToEdit.fullName);
      setPassword('');
      setRole(userToEdit.role);
      setBranchId(userToEdit.branchId || '');
      setEmail(userToEdit.email || '');
      setPhone(userToEdit.phone || '');
      setIsActive(userToEdit.isActive);
    } else {
      setUserName('');
      setFullName('');
      setPassword('');
      setRole('Cashier');
      setBranchId('');
      setEmail('');
      setPhone('');
      setIsActive(true);
    }
    setErrors({});
  }, [userToEdit, isOpen]);

  const validate = () => {
    const errs: Record<string, string> = {};
    if (!userName.trim()) errs.userName = 'اسم المستخدم (تسجيل الدخول) مطلوب';
    if (!fullName.trim()) errs.fullName = 'الاسم الكامل مطلوب';
    if (!userToEdit && (!password || password.length < 4)) {
      errs.password = 'كلمة المرور يجب أن لا تقل عن 4 خانات';
    }
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    try {
      setIsSubmitting(true);
      if (userToEdit) {
        await userService.update(userToEdit.id, {
          fullName,
          role,
          branchId,
          email: email.trim() || undefined,
          phone: phone.trim() || undefined,
          isActive,
        });
        showSuccess('تم تحديث بيانات المستخدم بنجاح');
      } else {
        await userService.create({
          userName: userName.trim(),
          fullName: fullName.trim(),
          password,
          role,
          branchId,
          email: email.trim() || undefined,
          phone: phone.trim() || undefined,
          isActive,
        });
        showSuccess('تم إنشاء حساب المستخدم بنجاح');
      }
      onSaved();
      onClose();
    } catch (err: unknown) {
      showError(err instanceof Error ? err.message : 'فشل حفظ بيانات المستخدم');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={userToEdit ? 'تعديل بيانات المستخدم' : 'إضافة مستخدم جديد'}
      subtitle="تحديد بيانات الدخول، الدور الوظيفي، والفرع المخصص"
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="اسم المستخدم (Login) *"
            placeholder="مثال: cashier2"
            value={userName}
            onChange={(e) => setUserName(e.target.value)}
            disabled={!!userToEdit}
            error={errors.userName}
            leftIcon={<IconUser className="w-4 h-4" />}
            autoFocus={!userToEdit}
            className="num-ltr text-right font-mono"
          />

          <Input
            label="الاسم الكامل للموظف *"
            placeholder="مثال: فهد سالم الدوسري"
            value={fullName}
            onChange={(e) => setFullName(e.target.value)}
            error={errors.fullName}
          />

          {!userToEdit && (
            <Input
              label="كلمة المرور المؤقتة *"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              error={errors.password}
              leftIcon={<IconKey className="w-4 h-4" />}
              className="num-ltr text-right"
            />
          )}

          <Select
            label="الدور الوظيفي والصلاحيات *"
            value={role}
            onChange={(e) => setRole(e.target.value)}
            options={[
              { value: 'Admin', label: 'مدير نظام (Admin)' },
              { value: 'Cashier', label: 'كاشير مبيعات (Cashier)' },
              { value: 'Accountant', label: 'محاسب مالي (Accountant)' },
              { value: 'StockManager', label: 'أمين مستودع (Stock Manager)' },
            ]}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            label="الفرع المخصص للعمل فيه *"
            value={branchId}
            onChange={(e) => setBranchId(e.target.value)}
            options={
              branches.length > 0
                ? branches.map((b) => ({
                    value: b.id,
                    label: b.name,
                  }))
                : [{ value: '', label: 'لا توجد فروع متاحة' }]
            }
          />

          <Input
            label="رقم الجوال"
            placeholder="05xxxxxxxx"
            value={phone}
            onChange={(e) => setPhone(e.target.value)}
            leftIcon={<IconPhone className="w-4 h-4" />}
            className="num-ltr text-right"
          />
        </div>

        <Input
          label="البريد الإلكتروني"
          type="email"
          placeholder="user@retalsystem.sa"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          leftIcon={<IconMail className="w-4 h-4" />}
          className="num-ltr text-right"
        />

        <div className="flex items-center gap-2 pt-2 border-t border-slate-100">
          <label className="flex items-center gap-2 cursor-pointer select-none">
            <input
              type="checkbox"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              className="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500"
            />
            <span className="text-sm font-semibold text-slate-800">الحساب نشط ومسموح له بتسجيل الدخول</span>
          </label>
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            {userToEdit ? 'حفظ التعديلات' : 'إنشاء المستخدم'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};

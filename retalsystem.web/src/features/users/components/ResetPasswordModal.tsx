import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Button } from '../../../shared/components/ui/Button';
import { UserItemDto } from '../types/user.types';
import { userService } from '../services/userService';
import { useToast } from '../../../shared/context/ToastContext';
import { IconKey } from '@tabler/icons-react';

interface ResetPasswordModalProps {
  isOpen: boolean;
  onClose: () => void;
  user: UserItemDto | null;
}

export const ResetPasswordModal: React.FC<ResetPasswordModalProps> = ({
  isOpen,
  onClose,
  user,
}) => {
  const { showSuccess, showError } = useToast();
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState('');

  if (!user) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (password.length < 10 || !/[a-z]/.test(password) || !/[A-Z]/.test(password) || !/\d/.test(password) || !/[^A-Za-z0-9]/.test(password)) {
      setError('10 خانات على الأقل وتشمل حرفاً كبيراً وصغيراً ورقماً ورمزاً');
      return;
    }
    if (password !== confirmPassword) {
      setError('كلمات المرور غير متطابقة');
      return;
    }

    try {
      setIsSubmitting(true);
      await userService.resetPassword(user.id, password);
      showSuccess(`تم تعيين كلمة مرور جديدة للمستخدم "${user.userName}" بنجاح`);
      setPassword('');
      setConfirmPassword('');
      setError('');
      onClose();
    } catch {
      showError('فشل إعادة تعيين كلمة المرور');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إعادة تعيين كلمة المرور"
      subtitle={`تعيين كلمة سر جديدة للمستخدم: @${user.userName}`}
      size="sm"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <Input
          label="كلمة المرور الجديدة *"
          type="password"
          placeholder="••••••••"
          value={password}
          onChange={(e) => {
            setPassword(e.target.value);
            setError('');
          }}
          leftIcon={<IconKey className="w-4 h-4" />}
          autoFocus
          className="num-ltr text-right"
        />

        <Input
          label="تأكيد كلمة المرور الجديدة *"
          type="password"
          placeholder="••••••••"
          value={confirmPassword}
          onChange={(e) => {
            setConfirmPassword(e.target.value);
            setError('');
          }}
          error={error}
          leftIcon={<IconKey className="w-4 h-4" />}
          className="num-ltr text-right"
        />

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            تحديث كلمة المرور
          </Button>
        </div>
      </form>
    </Modal>
  );
};

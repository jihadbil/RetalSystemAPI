import React, { useState, useEffect, useCallback } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { PermissionDefinition } from '../types/user.types';
import { userService } from '../services/userService';
import { useToast } from '../../../shared/context/ToastContext';
import { IconShieldCheck, IconCheck } from '@tabler/icons-react';

interface PermissionsMatrixModalProps {
  isOpen: boolean;
  onClose: () => void;
  roleName: string | null;
  roleId: string | null;
  roleLabel: string;
}

export const PermissionsMatrixModal: React.FC<PermissionsMatrixModalProps> = ({
  isOpen,
  onClose,
  roleName,
  roleId,
  roleLabel,
}) => {
  const { showSuccess, showError } = useToast();
  const [permissions, setPermissions] = useState<PermissionDefinition[]>([]);
  const [selectedKeys, setSelectedKeys] = useState<string[]>([]);
  const [isSaving, setIsSaving] = useState(false);
  const [loadedRoleId, setLoadedRoleId] = useState<string | null>(null);
  const protectedRole = roleName === 'Admin' || roleName === 'SuperAdmin';

  const load = useCallback(async () => {
    if (!roleId) return;
    setLoadedRoleId(null);
    try {
      const [allPerms, assigned] = await Promise.all([
        userService.getAllPermissions(),
        userService.getRolePermissions(roleId),
      ]);
      setPermissions(allPerms);
      setSelectedKeys(assigned);
      setLoadedRoleId(roleId);
    } catch {
      showError('فشل تحميل الصلاحيات');
    }
  }, [roleId, showError]);

  useEffect(() => {
    if (isOpen && roleName) {
      void load();
    }
  }, [isOpen, roleName, load]);

  if (!roleName) return null;

  const toggleKey = (key: string) => {
    if (protectedRole || loadedRoleId !== roleId) return;
    setSelectedKeys((prev) =>
      prev.includes(key) ? prev.filter((k) => k !== key) : [...prev, key]
    );
  };

  const handleSave = async () => {
    if (protectedRole || !roleId || loadedRoleId !== roleId) return;
    try {
      setIsSaving(true);
      if (!roleId) return;
      await userService.updateRolePermissions(roleId, selectedKeys);
      showSuccess(`تم تحديث صلاحيات الدور "${roleLabel}" بنجاح`);
      onClose();
    } catch {
      showError('فشل حفظ الصلاحيات');
    } finally {
      setIsSaving(false);
    }
  };

  // تجميع الصلاحيات حسب التصنيف
  const categories = Array.from(new Set(permissions.map((p) => p.category)));

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`صلاحيات الدور: ${roleLabel}`}
      subtitle="تحديد الصلاحيات وإمكانية الوصول للشاشات والعمليات الحساسة"
      size="lg"
    >
      <div className="flex flex-col gap-4">
        {protectedRole && (
          <div className="p-3 rounded-xl bg-amber-50 text-amber-900 border border-amber-200 text-xs font-semibold flex items-center gap-2">
            <IconShieldCheck className="w-5 h-5 text-amber-600 shrink-0" />
            صلاحيات هذا الدور محمية ويحددها النظام.
          </div>
        )}

        <div className="flex flex-col gap-4 max-h-96 overflow-y-auto pr-1">
          {categories.map((cat) => (
            <div key={cat} className="p-3 bg-slate-50/80 rounded-xl border border-slate-200/80 flex flex-col gap-2">
              <h4 className="text-xs font-bold text-slate-800 border-b border-slate-200 pb-1">
                {cat}
              </h4>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                {permissions
                  .filter((p) => p.category === cat)
                  .map((p) => {
                    const isChecked = selectedKeys.includes(p.key);
                    return (
                      <label
                        key={p.key}
                        className={`flex items-start gap-2.5 p-2 rounded-lg border text-xs cursor-pointer transition-all ${
                          isChecked
                            ? 'bg-white border-emerald-300 shadow-2xs'
                            : 'bg-white/60 border-slate-200/70 opacity-70 hover:opacity-100'
                        }`}
                      >
                        <input
                          type="checkbox"
                          checked={isChecked}
                          disabled={protectedRole || loadedRoleId !== roleId}
                          onChange={() => toggleKey(p.key)}
                          className="mt-0.5 w-4 h-4 rounded text-emerald-600 border-slate-300 focus:ring-emerald-500"
                        />
                        <div className="flex flex-col min-w-0">
                          <span className="font-bold text-slate-900">{p.name}</span>
                          <span className="text-[11px] text-slate-500 mt-0.5">{p.description}</span>
                        </div>
                      </label>
                    );
                  })}
              </div>
            </div>
          ))}
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSaving}>
            إلغاء
          </Button>
          <Button
            variant="primary"
            onClick={handleSave}
            isLoading={isSaving}
            disabled={protectedRole || !roleId || loadedRoleId !== roleId}
          >
            <IconCheck className="w-4 h-4" />
            حفظ مصفوفة الصلاحيات
          </Button>
        </div>
      </div>
    </Modal>
  );
};

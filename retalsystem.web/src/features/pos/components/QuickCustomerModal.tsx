import { lookupService, type LookupOption } from '../../../shared/services/lookupService';
import { useDebounce } from '../../../shared/hooks/useDebounce';
import React, { useState, useEffect, useCallback, useRef } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { IconUser, IconPhone, IconPlus, IconCheck, IconSearch } from '@tabler/icons-react';
import { useToast } from '../../../shared/context/ToastContext';
import { customerService } from '../../customers/services/customerService';
import { useAuth } from '../../../shared/context/AuthContext';
import { Permissions } from '../../../shared/auth/permissions';

interface QuickCustomerModalProps {
  isOpen: boolean;
  onClose: () => void;
  currentCustomerName?: string;
  onSelectCustomer: (customer: { id?: string; name: string; phone?: string }) => void;
}

export const QuickCustomerModal: React.FC<QuickCustomerModalProps> = ({
  isOpen,
  onClose,
  currentCustomerName,
  onSelectCustomer,
}) => {
  const { hasPermission } = useAuth();
  const canCreateCustomer = hasPermission(Permissions.Customers.Create);
  const { showSuccess, showError } = useToast();
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 250);
  const requestVersion = useRef(0);
  const invalidateRequest = useCallback(() => { requestVersion.current++; }, []);
  const [customers, setCustomers] = useState<LookupOption[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [isAddingNew, setIsAddingNew] = useState(false);
  const [newName, setNewName] = useState('');
  const [newPhone, setNewPhone] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const loadCustomers = useCallback(async (q?: string) => {
    const version = ++requestVersion.current;
    setCustomers([]);
    setIsLoading(true);
    setLoadError(null);
    try {
      const data = (await lookupService.page('customers', { search: q })).items;
      if (version !== requestVersion.current) return;
      setCustomers(data);
    } catch (err: unknown) {
      if (version !== requestVersion.current) return;
      const msg = err instanceof Error ? err.message : 'فشل تحميل قائمة العملاء';
      setLoadError(msg);
    } finally {
      if (version === requestVersion.current) setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    if (isOpen) {
      loadCustomers(debouncedSearch);
    }
    return invalidateRequest;
  }, [isOpen, debouncedSearch, loadCustomers, invalidateRequest]);

  const handleSelect = (cust: { id?: string; name: string; phone?: string }) => {
    onSelectCustomer(cust);
    onClose();
  };

  const handleCreateNew = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!canCreateCustomer || !newName.trim()) return;

    setIsSubmitting(true);
    try {
      const created = await customerService.create({
        name: newName.trim(),
        phone: newPhone.trim(),
        creditLimit: 0,
        discountRate: 0,
        isActive: true,
      });

      onSelectCustomer({ id: created.id, name: created.name, phone: created.phone });
      showSuccess('تمت إضافة العميل', `تم تسجيل ${created.name} واختياره للفاتورة`);
      setNewName('');
      setNewPhone('');
      setIsAddingNew(false);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل إضافة العميل على الخادم';
      showError('خطأ أثناء حفظ العميل', msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="اختيار أو إضافة عميل (F3)"
      subtitle="حدد العميل المسجل للفاتورة أو أنشئ عميلاً سريعاً برقم الهاتف"
      size="md"
    >
      <div className="flex flex-col gap-4">
        {!isAddingNew ? (
          <>
            {/* حقل البحث وزر إضافة عميل جديد */}
            <div className="flex items-center gap-2">
              <div className="flex-1">
                <Input
                  placeholder="ابحث بالاسم أو رقم الهاتف..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  leftIcon={<IconSearch className="w-4 h-4" />}
                  autoFocus
                />
              </div>

              {canCreateCustomer && <Button
                variant="secondary"
                size="md"
                leftIcon={<IconPlus className="w-4 h-4 text-emerald-600" />}
                onClick={() => setIsAddingNew(true)}
              >
                عميل جديد
              </Button>}
            </div>

            {loadError && (
              <div className="p-3 rounded-lg bg-rose-50 text-rose-700 text-xs flex items-center justify-between">
                <span>{loadError}</span>
                <button
                  type="button"
                  onClick={() => loadCustomers(search)}
                  className="font-bold underline cursor-pointer"
                >
                  إعادة المحاولة
                </button>
              </div>
            )}

            {/* قائمة العملاء */}
            <div className="flex flex-col divide-y divide-slate-100 max-h-64 overflow-y-auto border border-slate-200 rounded-xl">
              {/* خيار عميل نقدي دائماً متاح في الأعلى */}
              <button
                type="button"
                onClick={() => handleSelect({ name: 'عميل نقدي' })}
                className="flex items-center justify-between p-3 text-right hover:bg-emerald-50/50 transition-colors cursor-pointer bg-slate-50/50"
              >
                <div className="flex items-center gap-2.5">
                  <div className="w-8 h-8 rounded-lg bg-emerald-100 text-emerald-700 flex items-center justify-center">
                    <IconUser className="w-4 h-4" />
                  </div>
                  <div>
                    <p className="text-sm font-bold text-slate-800">عميل نقدي عام</p>
                    <p className="text-[11px] text-slate-400">بيع فوري بدون تسجيل بيانات</p>
                  </div>
                </div>
              </button>

              {customers.map((cust) => {
                const isSelected = currentCustomerName === cust.name;
                return (
                  <button
                    key={cust.id}
                    type="button"
                    onClick={() => handleSelect({ id: cust.id, name: cust.name, phone: undefined })}
                    className={`flex items-center justify-between p-3 text-right hover:bg-emerald-50/50 transition-colors cursor-pointer ${
                      isSelected ? 'bg-emerald-50 text-emerald-900 font-bold' : 'text-slate-800'
                    }`}
                  >
                    <div className="flex items-center gap-2.5">
                      <div className="w-8 h-8 rounded-lg bg-slate-100 flex items-center justify-center text-slate-500">
                        <IconUser className="w-4 h-4" />
                      </div>
                      <div>
                        <p className="text-sm leading-tight">{cust.name}</p>
                      </div>
                    </div>

                    {isSelected && <IconCheck className="w-4 h-4 text-emerald-600" />}
                  </button>
                );
              })}

              {!isLoading && customers.length === 0 && !loadError && (
                <div className="p-6 text-center text-xs text-slate-400">
                  {canCreateCustomer
                    ? 'لا يوجد عملاء مسجلون بهذا الاسم. يمكنك الضغط على "عميل جديد" لإضافته.'
                    : 'لا يوجد عملاء مسجلون بهذا الاسم.'}
                </div>
              )}
            </div>
          </>
        ) : (
          /* نموذج إضافة عميل جديد فوري */
          <form onSubmit={handleCreateNew} className="flex flex-col gap-3 p-1">
            <Input
              label="اسم العميل أو المنشأة"
              placeholder="مثال: مطعم الريف"
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              leftIcon={<IconUser className="w-4 h-4" />}
              autoFocus
              required
            />

            <Input
              label="رقم الهاتف"
              placeholder="09XXXXXXXX"
              value={newPhone}
              onChange={(e) => setNewPhone(e.target.value)}
              leftIcon={<IconPhone className="w-4 h-4" />}
              isLtr
            />

            <div className="flex items-center justify-end gap-2 mt-3 pt-3 border-t border-slate-100">
              <Button variant="secondary" size="md" onClick={() => setIsAddingNew(false)} disabled={isSubmitting}>
                رجوع للقائمة
              </Button>
              <Button variant="primary" size="md" type="submit" isLoading={isSubmitting}>
                حفظ واختيار العميل
              </Button>
            </div>
          </form>
        )}
      </div>
    </Modal>
  );
};

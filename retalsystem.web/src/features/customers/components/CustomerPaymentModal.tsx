import { lookupService, type LookupOption } from '../../../shared/services/lookupService';
import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Button } from '../../../shared/components/ui/Button';
import { CustomerDto } from '../types/customer.types';
import { customerService } from '../services/customerService';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import { soundService } from '../../../shared/services/soundService';
import { IconCash, IconCalendar } from '@tabler/icons-react';

interface CustomerPaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
  customer: CustomerDto | null;
  onPaymentSuccess: () => void;
}

export const CustomerPaymentModal: React.FC<CustomerPaymentModalProps> = ({
  isOpen,
  onClose,
  customer,
  onPaymentSuccess,
}) => {
  const { currentBranchId } = useAuth();
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [safes, setSafes] = useState<LookupOption[]>([]);
  const [amount, setAmount] = useState<number | ''>('');
  const [safeId, setSafeId] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('1'); // 1: نقدي, 2: بنكي, 3: مدى
  const [paymentDate, setPaymentDate] = useState(new Date().toISOString().slice(0, 10));
  const [notes, setNotes] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;
    if (isOpen) {
      lookupService
        .list('safes')
        .then((data) => {
          if (cancelled) return;
          const activeSafes = (data || []).filter((s) => s.isActive);
          setSafes(activeSafes);
          if (activeSafes.length > 0) {
            setSafeId(activeSafes[0].id);
          }
        })
        .catch((err) => {
          if (cancelled) return;
          console.error('Failed to fetch safes:', err);
        });
    }
    return () => { cancelled = true; };
  }, [isOpen]);

  if (!customer) return null;

  const handlePayFull = () => {
    if (customer.currentBalance > 0) {
      setAmount(customer.currentBalance);
      setError('');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!amount || amount <= 0) {
      setError('يرجى إدخال مبلغ صحيح أكبر من الصفر');
      return;
    }

    try {
      setIsSubmitting(true);
      await customerService.addPayment(customer.id, {
        amount: Number(amount),
        safeId,
        branchId: currentBranchId || undefined,
        paymentDate,
        paymentMethod: parseInt(paymentMethod, 10),
        notes: notes.trim() || undefined,
      });

      soundService.playSuccess();
      showSuccess(`تم تسجيل سند قبض بمبلغ ${Number(amount).toLocaleString('ar-LY')} د.ل بنجاح`);
      onPaymentSuccess();
      onClose();
    } catch (err: unknown) {
      soundService.playError();
      showError(err instanceof Error ? err.message : 'فشل تسجيل سند القبض');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="سند قبض من عميل (تحصيل دفعة)"
      subtitle={`تسجيل دفعة نقدية أو تحويل بنكي لحساب: ${customer.name}`}
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        {/* ملخص رصيد العميل الحالي */}
        <div className="p-3.5 bg-emerald-50/60 border border-emerald-200/80 rounded-xl flex items-center justify-between">
          <div>
            <span className="text-xs text-emerald-800 font-medium">الرصيد المستحق الحالي</span>
            <div className="text-lg font-black text-emerald-900 num-ltr text-right">
              {customer.currentBalance.toLocaleString('ar-LY', { minimumFractionDigits: 2 })}{' '}
              <span className="text-xs font-normal">د.ل</span>
            </div>
          </div>
          {customer.currentBalance > 0 && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={handlePayFull}
              className="text-xs text-emerald-700 border-emerald-300 hover:bg-emerald-100"
            >
              سداد الرصيد بالكامل
            </Button>
          )}
        </div>

        <Input
          label="المبلغ المحصل (د.ل) *"
          type="number"
          step="0.01"
          placeholder="0.00"
          value={amount}
          onChange={(e) => {
            setAmount(e.target.value === '' ? '' : parseFloat(e.target.value));
            setError('');
          }}
          error={error}
          leftIcon={<IconCash className="w-4 h-4 text-emerald-600" />}
          autoFocus
          className="num-ltr text-right text-lg font-bold"
        />

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            lookup={{ resource: 'safes', branchId: currentBranchId || undefined }} label="الخزينة / الحساب المودع فيه *"
            value={safeId}
            onChange={(e) => setSafeId(e.target.value)}
            options={
              safes.length > 0
                ? safes.map((s) => ({
                    value: s.id,
                    label: `${s.name}`,
                  }))
                : [{ value: '', label: 'لا توجد خزائن متاحة' }]
            }
          />

          <Select
            label="طريقة التحصيل *"
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(e.target.value)}
            options={[
              { value: '1', label: 'نقداً (كاش)' },
              { value: '2', label: 'تحويل مصرفي' },
              { value: '3', label: 'بطاقة مصرفية / شبكة' },
              { value: '5', label: 'صك / شيك مصرفي' },
            ]}
          />
        </div>

        <Input
          label="تاريخ السند *"
          type="date"
          value={paymentDate}
          onChange={(e) => setPaymentDate(e.target.value)}
          leftIcon={<IconCalendar className="w-4 h-4" />}
          className="num-ltr text-right"
        />

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-slate-700">البيان / ملاحظات السند</label>
          <input
            type="text"
            className="w-full h-10 px-3 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            placeholder="مثال: دفعة تحت الحساب - حوالة رقم 99827"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
          />
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            حفظ سند القبض (F10)
          </Button>
        </div>
      </form>
    </Modal>
  );
};

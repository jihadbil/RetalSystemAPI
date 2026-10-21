import { lookupService, type LookupOption } from '../../../shared/services/lookupService';
import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Button } from '../../../shared/components/ui/Button';
import { SupplierDto } from '../types/supplier.types';
import { supplierService } from '../services/supplierService';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import { soundService } from '../../../shared/services/soundService';
import { IconCash, IconCalendar } from '@tabler/icons-react';

interface SupplierPaymentModalProps {
  isOpen: boolean;
  onClose: () => void;
  supplier: SupplierDto | null;
  onPaymentSuccess: () => void;
}

export const SupplierPaymentModal: React.FC<SupplierPaymentModalProps> = ({
  isOpen,
  onClose,
  supplier,
  onPaymentSuccess,
}) => {
  const { currentBranchId } = useAuth();
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [safes, setSafes] = useState<LookupOption[]>([]);
  const [amount, setAmount] = useState<number | ''>('');
  const [safeId, setSafeId] = useState('');
  const [paymentMethod, setPaymentMethod] = useState('1'); // 1: نقدي, 2: بنكي, 3: مدى/شيك
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

  if (!supplier) return null;

  const handlePayFull = () => {
    if (supplier.currentBalance > 0) {
      setAmount(supplier.currentBalance);
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
      await supplierService.addPayment(supplier.id, {
        amount: Number(amount),
        safeId,
        branchId: currentBranchId || undefined,
        paymentDate,
        paymentMethod: parseInt(paymentMethod, 10),
        notes: notes.trim() || undefined,
      });

      soundService.playSuccess();
      showSuccess(`تم تسجيل سند صرف لمورد بمبلغ ${Number(amount).toLocaleString('ar-LY')} د.ل بنجاح`);
      onPaymentSuccess();
      onClose();
    } catch (err: unknown) {
      soundService.playError();
      showError(err instanceof Error ? err.message : 'فشل تسجيل سند الصرف');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="سند صرف لمورد (سداد دفعة)"
      subtitle={`صرف دفعة نقدية أو تحويل بنكي للمورد: ${supplier.name}`}
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        {/* ملخص المستحقات الحالية للمورد */}
        <div className="p-3.5 bg-amber-50/60 border border-amber-200/80 rounded-xl flex items-center justify-between">
          <div>
            <span className="text-xs text-amber-800 font-medium">إجمالي المستحق للمورد (دائن)</span>
            <div className="text-lg font-black text-amber-900 num-ltr text-right">
              {supplier.currentBalance.toLocaleString('ar-LY', { minimumFractionDigits: 2 })}{' '}
              <span className="text-xs font-normal">د.ل</span>
            </div>
          </div>
          {supplier.currentBalance > 0 && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={handlePayFull}
              className="text-xs text-amber-800 border-amber-300 hover:bg-amber-100"
            >
              سداد المستحق كاملاً
            </Button>
          )}
        </div>

        <Input
          label="المبلغ المصروف (د.ل) *"
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
            lookup={{ resource: 'safes', branchId: currentBranchId || undefined }} label="الخصم من خزينة / حساب *"
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
            label="طريقة الصرف *"
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(e.target.value)}
            options={[
              { value: '1', label: 'نقداً من الصندوق' },
              { value: '2', label: 'تحويل مصرفي' },
              { value: '3', label: 'بطاقة مصرفية' },
              { value: '5', label: 'صك / شيك مصرفي' },
            ]}
          />
        </div>

        <Input
          label="تاريخ الصرف *"
          type="date"
          value={paymentDate}
          onChange={(e) => setPaymentDate(e.target.value)}
          leftIcon={<IconCalendar className="w-4 h-4" />}
          className="num-ltr text-right"
        />

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-slate-700">البيان / ملاحظات الصرف</label>
          <input
            type="text"
            className="w-full h-10 px-3 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            placeholder="مثال: دفعة من فاتورة توريد رقم 44 - حوالة بنكية"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
          />
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            حفظ سند الصرف
          </Button>
        </div>
      </form>
    </Modal>
  );
};

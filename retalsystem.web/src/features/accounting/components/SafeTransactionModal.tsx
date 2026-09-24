import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Button } from '../../../shared/components/ui/Button';
import { SafeDto, TransactionDirection } from '../types/accounting.types';
import { accountingService } from '../services/accountingService';
import { useToast } from '../../../shared/context/ToastContext';
import { soundService } from '../../../shared/services/soundService';
import { IconCash, IconArrowDownLeft, IconArrowUpRight, IconCalendar } from '@tabler/icons-react';

interface SafeTransactionModalProps {
  isOpen: boolean;
  onClose: () => void;
  safe: SafeDto | null;
  onTransactionSuccess: () => void;
}

export const SafeTransactionModal: React.FC<SafeTransactionModalProps> = ({
  isOpen,
  onClose,
  safe,
  onTransactionSuccess,
}) => {
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [direction, setDirection] = useState<TransactionDirection>(TransactionDirection.Inbound);
  const [amount, setAmount] = useState<number | ''>('');
  const [transactionDate, setTransactionDate] = useState(new Date().toISOString().slice(0, 10));
  const [description, setDescription] = useState('');
  const [referenceNumber, setReferenceNumber] = useState('');
  const [error, setError] = useState('');

  if (!safe) return null;

  const isInbound = direction === TransactionDirection.Inbound;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!amount || amount <= 0) {
      setError('يرجى إدخال مبلغ صحيح أكبر من الصفر');
      return;
    }
    if (!description.trim()) {
      setError('يرجى إدخال سبب / بيان الحركة');
      return;
    }
    if (!isInbound && amount > safe.currentBalance) {
      setError(`المبلغ يتجاوز رصيد الخزينة الحالي (${safe.currentBalance.toLocaleString('ar-SA')} ر.س)`);
      return;
    }

    try {
      setIsSubmitting(true);
      await accountingService.createTransaction({
        safeId: safe.id,
        direction,
        amount: Number(amount),
        transactionDate,
        description: description.trim(),
        referenceNumber: referenceNumber.trim() || undefined,
      });

      soundService.playSuccess();
      showSuccess(
        `تم تسجيل عملية ${isInbound ? 'الإيداع' : 'السحب'} بمبلغ ${Number(amount).toLocaleString('ar-SA')} ر.س بنجاح`
      );
      onTransactionSuccess();
      onClose();
    } catch (err: unknown) {
      soundService.playError();
      showError(err instanceof Error ? err.message : 'فشل إتمام الحركة');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={isInbound ? 'إيداع نقدي مباشر' : 'سحب نقدي مباشر'}
      subtitle={`الحساب المتأثر: ${safe.name} (الرصيد: ${safe.currentBalance.toLocaleString('ar-SA')} ر.س)`}
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        {/* اختيار اتجاه الحركة */}
        <div className="grid grid-cols-2 gap-3 p-1 bg-slate-100 rounded-xl">
          <button
            type="button"
            onClick={() => {
              setDirection(TransactionDirection.Inbound);
              setError('');
            }}
            className={`flex items-center justify-center gap-2 py-2 rounded-lg text-xs font-black transition-all ${
              isInbound
                ? 'bg-emerald-600 text-white shadow-xs'
                : 'text-slate-600 hover:text-slate-900'
            }`}
          >
            <IconArrowDownLeft className="w-4 h-4" />
            إيداع مباشر (وارد)
          </button>

          <button
            type="button"
            onClick={() => {
              setDirection(TransactionDirection.Outbound);
              setError('');
            }}
            className={`flex items-center justify-center gap-2 py-2 rounded-lg text-xs font-black transition-all ${
              !isInbound
                ? 'bg-rose-600 text-white shadow-xs'
                : 'text-slate-600 hover:text-slate-900'
            }`}
          >
            <IconArrowUpRight className="w-4 h-4" />
            سحب مباشر (صادر)
          </button>
        </div>

        <Input
          label="المبلغ (ر.س) *"
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
          <Input
            label="تاريخ الحركة *"
            type="date"
            value={transactionDate}
            onChange={(e) => setTransactionDate(e.target.value)}
            leftIcon={<IconCalendar className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="رقم الإيصال / المرجع"
            placeholder="مثال: REF-9921"
            value={referenceNumber}
            onChange={(e) => setReferenceNumber(e.target.value)}
            className="num-ltr text-right font-mono"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-slate-700">بيان الحركة والسبب *</label>
          <input
            type="text"
            className="w-full h-10 px-3 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            placeholder="مثال: توريد إيرادات متفرقة / استرجاع عهدة نقدية"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button
            variant={isInbound ? 'primary' : 'danger'}
            type="submit"
            isLoading={isSubmitting}
          >
            {isInbound ? 'تأكيد الإيداع' : 'تأكيد السحب'}
          </Button>
        </div>
      </form>
    </Modal>
  );
};

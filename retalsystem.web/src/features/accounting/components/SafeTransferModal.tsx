import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Button } from '../../../shared/components/ui/Button';
import { SafeDto } from '../types/accounting.types';
import { accountingService } from '../services/accountingService';
import { useToast } from '../../../shared/context/ToastContext';
import { soundService } from '../../../shared/services/soundService';
import { IconArrowsExchange, IconCash, IconCalendar } from '@tabler/icons-react';

interface SafeTransferModalProps {
  isOpen: boolean;
  onClose: () => void;
  safes: SafeDto[];
  onTransferSuccess: () => void;
}

export const SafeTransferModal: React.FC<SafeTransferModalProps> = ({
  isOpen,
  onClose,
  safes,
  onTransferSuccess,
}) => {
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const activeSafes = safes.filter((s) => s.isActive);
  const [sourceSafeId, setSourceSafeId] = useState(activeSafes[0]?.id || '');
  const [destinationSafeId, setDestinationSafeId] = useState(activeSafes[1]?.id || '');
  const [amount, setAmount] = useState<number | ''>('');
  const [transferDate, setTransferDate] = useState(new Date().toISOString().slice(0, 10));
  const [notes, setNotes] = useState('');
  const [error, setError] = useState('');

  const selectedSourceId = sourceSafeId;
  const selectedDestinationId = destinationSafeId;
  const sourceSafe = activeSafes.find((s) => s.id === selectedSourceId);
  const destSafe = activeSafes.find((s) => s.id === selectedDestinationId);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedSourceId || !selectedDestinationId) {
      setError('يرجى اختيار الخزينة المصدر والخزينة الوجهة');
      return;
    }
    if (selectedSourceId === selectedDestinationId) {
      setError('لا يمكن التحويل من وإلى نفس الحساب');
      return;
    }
    if (!amount || amount <= 0) {
      setError('يرجى إدخال مبلغ صحيح أكبر من الصفر');
      return;
    }
    if (sourceSafe && amount > sourceSafe.currentBalance) {
      setError(`المبلغ يتجاوز رصيد الخزينة المصدر (${sourceSafe.currentBalance.toLocaleString('ar-LY')} د.ل)`);
      return;
    }

    try {
      setIsSubmitting(true);
      await accountingService.transferFunds({
        fromSafeId: selectedSourceId,
        toSafeId: selectedDestinationId,
        amount: Number(amount),
        transferDate,
        description: notes.trim() || 'تحويل مالي بين الخزائن',
        notes: notes.trim() || undefined,
      });

      soundService.playSuccess();
      showSuccess(`تم تحويل مبلغ ${Number(amount).toLocaleString('ar-LY')} د.ل بنجاح`);
      onTransferSuccess();
      onClose();
    } catch (err: unknown) {
      soundService.playError();
      showError(err instanceof Error ? err.message : 'فشل إتمام عملية التحويل');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="تحويل مالي بين الخزائن والحسابات"
      subtitle="نقل السيولة النقدية أو التغذية بين الصناديق والحسابات المصرفية"
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="flex flex-col gap-1">
            <Select
              lookup={{ resource: 'safes' }} label="التحويل من (المصدر) *"
              value={selectedSourceId}
              onChange={(e) => {
                setSourceSafeId(e.target.value);
                setError('');
              }}
              options={activeSafes.map((s) => ({
                value: s.id,
                label: `${s.name} (${s.currentBalance.toLocaleString('ar-LY')} د.ل)`,
              }))}
            />
            {sourceSafe && (
              <span className="text-[11px] text-slate-500 font-medium">
                الرصيد المتاح: <b className="num-ltr">{sourceSafe.currentBalance.toLocaleString('ar-LY')}</b> د.ل
              </span>
            )}
          </div>

          <div className="flex flex-col gap-1">
            <Select
              lookup={{ resource: 'safes' }} label="التحويل إلى (الوجهة) *"
              value={selectedDestinationId}
              onChange={(e) => {
                setDestinationSafeId(e.target.value);
                setError('');
              }}
              options={activeSafes
                .filter((s) => s.id !== selectedSourceId)
                .map((s) => ({
                  value: s.id,
                  label: `${s.name} (${s.currentBalance.toLocaleString('ar-LY')} د.ل)`,
                }))}
            />
            {destSafe && (
              <span className="text-[11px] text-slate-500 font-medium">
                الرصيد الحالي: <b className="num-ltr">{destSafe.currentBalance.toLocaleString('ar-LY')}</b> د.ل
              </span>
            )}
          </div>
        </div>

        <Input
          label="مبلغ التحويل (د.ل) *"
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

        <Input
          label="تاريخ التحويل *"
          type="date"
          value={transferDate}
          onChange={(e) => setTransferDate(e.target.value)}
          leftIcon={<IconCalendar className="w-4 h-4" />}
          className="num-ltr text-right"
        />

        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-bold text-slate-700">البيان / سبب التحويل</label>
          <input
            type="text"
            className="w-full h-10 px-3 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            placeholder="مثال: تغذية درج الكاشير للوردية المسائية"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
          />
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            <IconArrowsExchange className="w-4 h-4" />
            تنفيذ التحويل
          </Button>
        </div>
      </form>
    </Modal>
  );
};

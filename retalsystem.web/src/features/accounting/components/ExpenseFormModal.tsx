import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Button } from '../../../shared/components/ui/Button';
import { SafeDto, ExpenseCategoryDto } from '../types/accounting.types';
import { accountingService } from '../services/accountingService';
import { useToast } from '../../../shared/context/ToastContext';
import { soundService } from '../../../shared/services/soundService';
import { IconCash, IconCalendar, IconReceiptTax } from '@tabler/icons-react';

import { useAuth } from '../../../shared/context/AuthContext';

interface ExpenseFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  categories: ExpenseCategoryDto[];
  safes: SafeDto[];
  onSaved: () => void;
}

export const ExpenseFormModal: React.FC<ExpenseFormModalProps> = ({
  isOpen,
  onClose,
  categories,
  safes,
  onSaved,
}) => {
  const { currentBranchId } = useAuth();
  const { showSuccess, showError } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const activeSafes = safes.filter((s) => s.isActive);
  const [categoryId, setCategoryId] = useState(categories[0]?.id || '');
  const [safeId, setSafeId] = useState(activeSafes[0]?.id || '');
  const [amount, setAmount] = useState<number | ''>('');
  const [paymentMethod, setPaymentMethod] = useState('1'); // 1: Cash, 2: Bank, 3: Card
  const [expenseDate, setExpenseDate] = useState(new Date().toISOString().slice(0, 10));
  const [taxInvoiceNumber, setTaxInvoiceNumber] = useState('');
  const [description, setDescription] = useState('');
  const [beneficiaryName, setBeneficiaryName] = useState('');
  const [error, setError] = useState('');

  const selectedCategoryId = categoryId;
  const selectedSafeId = safeId;
  const selectedSafe = activeSafes.find((s) => s.id === selectedSafeId);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedCategoryId) {
      setError('يرجى اختيار تصنيف المصروف');
      return;
    }
    if (!selectedSafeId) {
      setError('يرجى اختيار الخزينة أو الحساب المسدد منه');
      return;
    }
    if (!amount || amount <= 0) {
      setError('يرجى إدخال مبلغ صحيح أكبر من الصفر');
      return;
    }
    if (!description.trim()) {
      setError('يرجى إدخال بيان وشرح المصروف');
      return;
    }

    try {
      setIsSubmitting(true);
      await accountingService.createExpense({
        expenseCategoryId: selectedCategoryId,
        safeId: selectedSafeId,
        branchId: currentBranchId || undefined,
        amount: Number(amount),
        expenseDate,
        paymentMethod: parseInt(paymentMethod, 10),
        description: description.trim(),
        beneficiaryName: beneficiaryName.trim() || undefined,
        taxInvoiceNumber: taxInvoiceNumber.trim() || undefined,
      });

      soundService.playSuccess();
      showSuccess(`تم تسجيل سند المصروف بمبلغ ${Number(amount).toLocaleString('ar-LY')} د.ل بنجاح`);
      onSaved();
      onClose();
    } catch (err: unknown) {
      soundService.playError();
      showError(err instanceof Error ? err.message : 'فشل حفظ سند المصروف');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="سند مصروف جديد"
      subtitle="تسجيل وقيد المصروفات التشغيلية والإدارية والعمومية"
      size="md"
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Select
            lookup={{ resource: 'expense-categories' }} label="بند / تصنيف المصروف *"
            value={selectedCategoryId}
            onChange={(e) => setCategoryId(e.target.value)}
            options={categories.map((c) => ({
              value: c.id,
              label: c.name,
            }))}
          />

          <div className="flex flex-col gap-1">
            <Select
              lookup={{ resource: 'safes', branchId: currentBranchId || undefined }} label="الخصم من خزينة / بنك *"
              value={selectedSafeId}
              onChange={(e) => {
                setSafeId(e.target.value);
                setError('');
              }}
              options={activeSafes.map((s) => ({
                value: s.id,
                label: `${s.name} (${s.currentBalance.toLocaleString('ar-SA')} د.ل)`,
              }))}
            />
            {selectedSafe && (
              <span className="text-[11px] text-slate-500 font-medium">
                الرصيد: <b className="num-ltr">{selectedSafe.currentBalance.toLocaleString('ar-LY')}</b> د.ل
              </span>
            )}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="قيمة المصروف (د.ل) *"
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

          <Select
            label="طريقة السداد *"
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(e.target.value)}
            options={[
              { value: '1', label: 'نقداً من الصندوق' },
              { value: '2', label: 'تحويل مصرفي' },
              { value: '3', label: 'بطاقة مصرفية' },
            ]}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input
            label="تاريخ المصروف *"
            type="date"
            value={expenseDate}
            onChange={(e) => setExpenseDate(e.target.value)}
            leftIcon={<IconCalendar className="w-4 h-4" />}
            className="num-ltr text-right"
          />

          <Input
            label="اسم المستفيد / الجهة (اختياري)"
            placeholder="مثال: شركة الكهرباء أو الموظف"
            value={beneficiaryName}
            onChange={(e) => setBeneficiaryName(e.target.value)}
          />
        </div>

        <Input
          label="رقم الفاتورة الضريبية (اختياري)"
          placeholder="رقم فاتورة المورد أو المرجع"
          value={taxInvoiceNumber}
          onChange={(e) => setTaxInvoiceNumber(e.target.value)}
          leftIcon={<IconReceiptTax className="w-4 h-4" />}
        />

        <div className="flex flex-col gap-1.5">
          <label htmlFor="expense-description" className="text-xs font-bold text-slate-700">البيان / تفاصيل وشرح المصروف *</label>
          <input
            id="expense-description" type="text"
            className="w-full h-10 px-3 rounded-xl border border-slate-200 bg-white text-slate-900 text-sm focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500"
            placeholder="مثال: فاتورة صيانة دورية للمكيفات بالفرع الرئيسي"
            value={description}
            onChange={(e) => {
              setDescription(e.target.value);
              setError('');
            }}
            required
          />
        </div>

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-slate-100">
          <Button variant="secondary" onClick={onClose} disabled={isSubmitting} type="button">
            إلغاء
          </Button>
          <Button variant="primary" type="submit" isLoading={isSubmitting}>
            حفظ سند المصروف
          </Button>
        </div>
      </form>
    </Modal>
  );
};

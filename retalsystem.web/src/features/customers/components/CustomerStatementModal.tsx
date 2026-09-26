import React, { useState, useEffect, useCallback } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { CustomerDto, CustomerStatementDto } from '../types/customer.types';
import { customerService } from '../services/customerService';
import { IconPrinter, IconFilter, IconArrowUpRight, IconArrowDownLeft } from '@tabler/icons-react';
import { LoadingOverlay } from '../../../shared/components/ui/LoadingOverlay';

interface CustomerStatementModalProps {
  isOpen: boolean;
  onClose: () => void;
  customer: CustomerDto | null;
}

export const CustomerStatementModal: React.FC<CustomerStatementModalProps> = ({
  isOpen,
  onClose,
  customer,
}) => {
  const [statement, setStatement] = useState<CustomerStatementDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [fromDate, setFromDate] = useState('2026-01-01');
  const [toDate, setToDate] = useState(new Date().toISOString().slice(0, 10));

  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  const loadStatement = useCallback(async () => {
    if (!customer) return;
    try {
      setIsLoading(true);
      setErrorMsg(null);
      const data = await customerService.getStatement(customer.id, fromDate, toDate);
      setStatement(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل كشف حساب العميل من الخادم';
      setErrorMsg(msg);
    } finally {
      setIsLoading(false);
    }
  }, [customer, fromDate, toDate]);

  useEffect(() => {
    if (!isOpen || !customer) return;
    const timer = window.setTimeout(() => { void loadStatement(); }, 0);
    return () => window.clearTimeout(timer);
  }, [isOpen, customer, loadStatement]);

  const handlePrint = () => {
    window.print();
  };

  if (!customer) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`كشف حساب العميل: ${customer.name}`}
      subtitle="سجل تفصيلي لجميع فواتير المبيعات، المرتجعات، وسندات القبض"
      size="xl"
    >
      <div className="flex flex-col gap-4">
        {/* شريط الفلترة بالتواريخ والإجراءات */}
        <div className="p-3 bg-slate-50 border border-slate-200/80 rounded-xl flex flex-wrap items-center justify-between gap-3">
          <div className="flex items-center gap-3">
            <div className="flex items-center gap-1.5 text-xs text-slate-600 font-medium">
              <span>من:</span>
              <input
                type="date"
                value={fromDate}
                onChange={(e) => setFromDate(e.target.value)}
                className="px-2 py-1 text-xs rounded-lg border border-slate-300 bg-white num-ltr"
              />
            </div>
            <div className="flex items-center gap-1.5 text-xs text-slate-600 font-medium">
              <span>إلى:</span>
              <input
                type="date"
                value={toDate}
                onChange={(e) => setToDate(e.target.value)}
                className="px-2 py-1 text-xs rounded-lg border border-slate-300 bg-white num-ltr"
              />
            </div>
            <Button variant="secondary" size="sm" onClick={loadStatement} disabled={isLoading}>
              <IconFilter className="w-3.5 h-3.5" />
              تطبيق
            </Button>
          </div>

          <Button variant="outline" size="sm" onClick={handlePrint}>
            <IconPrinter className="w-4 h-4" />
            طباعة كشف الحساب
          </Button>
        </div>

        {errorMsg && (
          <div className="p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-xs flex items-center justify-between">
            <span>{errorMsg}</span>
            <Button variant="secondary" size="sm" onClick={loadStatement}>
              إعادة المحاولة
            </Button>
          </div>
        )}

        {/* بطاقات المؤشرات المالية لكشف الحساب */}
        {statement && (
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <div className="p-3 rounded-xl bg-white border border-slate-200 shadow-2xs">
              <span className="text-[11px] font-bold text-slate-500">الرصيد الافتتاحي</span>
              <div className="text-base font-black text-slate-800 num-ltr text-right mt-1">
                {statement.openingBalance.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
                <span className="text-[10px] font-normal">د.ل</span>
              </div>
            </div>

            <div className="p-3 rounded-xl bg-rose-50/50 border border-rose-200/60 shadow-2xs">
              <div className="flex items-center justify-between">
                <span className="text-[11px] font-bold text-rose-700">إجمالي مدين (مبيعات)</span>
                <IconArrowUpRight className="w-3.5 h-3.5 text-rose-600" />
              </div>
              <div className="text-base font-black text-rose-900 num-ltr text-right mt-1">
                {statement.totalDebit.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
                <span className="text-[10px] font-normal">د.ل</span>
              </div>
            </div>

            <div className="p-3 rounded-xl bg-emerald-50/50 border border-emerald-200/60 shadow-2xs">
              <div className="flex items-center justify-between">
                <span className="text-[11px] font-bold text-emerald-700">إجمالي دائن (سدادات)</span>
                <IconArrowDownLeft className="w-3.5 h-3.5 text-emerald-600" />
              </div>
              <div className="text-base font-black text-emerald-900 num-ltr text-right mt-1">
                {statement.totalCredit.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
                <span className="text-[10px] font-normal">د.ل</span>
              </div>
            </div>

            <div className="p-3 rounded-xl bg-slate-900 text-white shadow-2xs">
              <span className="text-[11px] font-medium text-slate-300">الرصيد الختامي المتبقي</span>
              <div className="text-base font-black text-emerald-400 num-ltr text-right mt-1">
                {statement.currentBalance.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
                <span className="text-[10px] font-normal text-slate-300">د.ل</span>
              </div>
            </div>
          </div>
        )}

        {/* جدول حركات كشف الحساب */}
        <div className="relative border border-slate-200 rounded-xl overflow-hidden bg-white shadow-2xs">
          <LoadingOverlay isVisible={isLoading} message="جارٍ تحديث كشف الحساب..." />
          <div className="overflow-x-auto max-h-80">
            <table className="w-full text-right text-xs">
              <thead className="bg-slate-50 border-b border-slate-200 text-slate-600 font-bold sticky top-0 z-10">
                <tr>
                  <th className="py-2.5 px-3">التاريخ</th>
                  <th className="py-2.5 px-3">المستند</th>
                  <th className="py-2.5 px-3">الرقم المرجعي</th>
                  <th className="py-2.5 px-3">البيان</th>
                  <th className="py-2.5 px-3 text-left">مدين (+)</th>
                  <th className="py-2.5 px-3 text-left">دائن (-)</th>
                  <th className="py-2.5 px-3 text-left">الرصيد التراكمي</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 font-medium">
                {statement && statement.transactions.length > 0 ? (
                  statement.transactions.map((tr, idx) => (
                    <tr key={idx} className="hover:bg-slate-50/80 transition-colors">
                      <td className="py-2.5 px-3 text-slate-500 num-ltr text-right">{tr.transactionDate}</td>
                      <td className="py-2.5 px-3 font-semibold text-slate-800">{tr.documentTypeName}</td>
                      <td className="py-2.5 px-3 font-mono text-slate-600 num-ltr text-right">
                        {tr.documentId || '-'}
                      </td>
                      <td className="py-2.5 px-3 text-slate-600">{tr.description}</td>
                      <td className="py-2.5 px-3 text-left num-ltr font-bold text-rose-600">
                        {tr.debitAmount > 0
                          ? tr.debitAmount.toLocaleString('ar-SA', { minimumFractionDigits: 2 })
                          : '-'}
                      </td>
                      <td className="py-2.5 px-3 text-left num-ltr font-bold text-emerald-600">
                        {tr.creditAmount > 0
                          ? tr.creditAmount.toLocaleString('ar-SA', { minimumFractionDigits: 2 })
                          : '-'}
                      </td>
                      <td className="py-2.5 px-3 text-left num-ltr font-black text-slate-900">
                        {tr.balanceAfter.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="py-8 text-center text-slate-400">
                      لا توجد حركات مالية مسجلة لهذا العميل خلال الفترة المحددة
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        <div className="flex justify-end pt-2">
          <Button variant="secondary" onClick={onClose}>
            إغلاق
          </Button>
        </div>
      </div>
    </Modal>
  );
};

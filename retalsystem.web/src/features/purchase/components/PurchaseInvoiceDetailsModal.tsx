import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { purchaseService } from '../services/purchaseService';
import type { PurchaseInvoiceDto } from '../types/purchase.types';
import { PurchaseInvoiceStatus } from '../types/purchase.types';
import { IconPrinter, IconEdit } from '@tabler/icons-react';

interface PurchaseInvoiceDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  invoiceId?: string | null;
  onEditInvoice?: (invoice: PurchaseInvoiceDto) => void;
}

export const PurchaseInvoiceDetailsModal: React.FC<PurchaseInvoiceDetailsModalProps> = ({
  isOpen,
  onClose,
  invoiceId,
  onEditInvoice,
}) => {
  const [invoice, setInvoice] = useState<PurchaseInvoiceDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && invoiceId) {
      setIsLoading(true);
      setError(null);
      purchaseService
        .getInvoiceById(invoiceId)
        .then((data) => setInvoice(data))
        .catch((err) => {
          console.error('Failed to load purchase invoice details:', err);
          setError('تعذر تحميل تفاصيل الفاتورة من الخادم');
        })
        .finally(() => setIsLoading(false));
    } else {
      setInvoice(null);
    }
  }, [isOpen, invoiceId]);

  if (!isOpen) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={invoice ? `تفاصيل فاتورة مشتريات #${invoice.invoiceNumber}` : 'تفاصيل الفاتورة'}
      subtitle="عرض بيانات البضاعة المستلمة وتوزيع التكاليف ومبالغ السداد"
      size="2xl"
      footer={
        <div className="flex justify-between items-center w-full">
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="md"
              leftIcon={<IconPrinter className="w-4 h-4" />}
              onClick={() => window.print()}
            >
              طباعة الفاتورة
            </Button>
            {invoice &&
              (invoice.status === PurchaseInvoiceStatus.Pending ||
                invoice.status === PurchaseInvoiceStatus.Draft) &&
              onEditInvoice && (
                <Button
                  variant="primary"
                  size="md"
                  leftIcon={<IconEdit className="w-4 h-4" />}
                  onClick={() => onEditInvoice(invoice)}
                >
                  تعديل الفاتورة المفتوحة
                </Button>
              )}
          </div>
          <Button variant="secondary" size="md" onClick={onClose}>
            إغلاق
          </Button>
        </div>
      }
    >
      {isLoading ? (
        <div className="py-16 text-center text-slate-400">جاري تحميل تفاصيل الفاتورة...</div>
      ) : error ? (
        <div className="p-4 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-center text-sm">
          {error}
        </div>
      ) : invoice ? (
        <div className="flex flex-col gap-4 py-1 text-xs">
          {/* Header Info Grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 p-3 bg-slate-50 border border-slate-200 rounded-xl">
            <div>
              <span className="text-slate-400 block text-[11px]">المورد:</span>
              <span className="font-bold text-slate-900 text-sm">{invoice.supplierName || '-'}</span>
            </div>
            <div>
              <span className="text-slate-400 block text-[11px]">المستودع المستلم:</span>
              <span className="font-semibold text-slate-800">{invoice.warehouseName || '-'}</span>
            </div>
            <div>
              <span className="text-slate-400 block text-[11px]">الفرع:</span>
              <span className="font-semibold text-slate-800">{invoice.branchName || '-'}</span>
            </div>
            <div>
              <span className="text-slate-400 block text-[11px]">تاريخ الفاتورة:</span>
              <span className="font-semibold text-slate-800">
                {new Date(invoice.invoiceDate).toLocaleDateString('ar-LY')}
              </span>
            </div>
          </div>

          {/* Items Table */}
          <div className="border border-slate-200 rounded-xl overflow-hidden">
            <div className="p-2.5 bg-slate-100/70 border-b border-slate-200 font-bold text-slate-700">
              بنود الأصناف المسجلة ({invoice.items?.length || 0})
            </div>
            <div className="overflow-x-auto max-h-60">
              <table className="w-full text-right text-xs">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold">
                  <tr>
                    <th className="py-2.5 px-3">اسم الصنف</th>
                    <th className="py-2.5 px-2 text-center">الكمية</th>
                    <th className="py-2.5 px-3 text-center">سعر الوحدة</th>
                    <th className="py-2.5 px-2 text-center">الخصم</th>
                    <th className="py-2.5 px-3 text-left font-bold">الإجمالي</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {invoice.items?.map((it, idx) => (
                    <tr key={idx} className="hover:bg-slate-50/50">
                      <td className="py-2.5 px-3 font-bold text-slate-900">{it.productName}</td>
                      <td className="py-2.5 px-2 text-center font-bold text-slate-800">
                        {it.quantity} {it.unitName || ''}
                      </td>
                      <td className="py-2.5 px-3 text-center font-mono num-ltr">
                        {it.unitCost?.toFixed(2)} د.ل
                      </td>
                      <td className="py-2.5 px-2 text-center font-mono text-slate-500 num-ltr">
                        {it.discountAmount > 0 ? `${it.discountAmount.toFixed(2)} د.ل` : '-'}
                      </td>
                      <td className="py-2.5 px-3 text-left font-bold text-slate-900 font-mono num-ltr">
                        {(it.totalCost || it.lineTotal || it.quantity * it.unitCost).toFixed(2)} د.ل
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Financial Summary */}
          <div className="p-3 bg-emerald-50/50 border border-emerald-200 rounded-xl grid grid-cols-2 sm:grid-cols-4 gap-3 text-center">
            <div>
              <span className="text-slate-500 text-[11px] block">المجموع الفرعي</span>
              <span className="font-extrabold text-slate-900 num-ltr">{invoice.subTotal?.toFixed(2)} د.ل</span>
            </div>
            <div>
              <span className="text-slate-500 text-[11px] block">الخصم</span>
              <span className="font-bold text-slate-700 num-ltr">{invoice.discountAmount?.toFixed(2)} د.ل</span>
            </div>
            <div>
              <span className="text-emerald-800 text-[11px] font-bold block">الصافي المطلوب</span>
              <span className="font-black text-emerald-800 text-sm num-ltr">
                {invoice.totalAmount?.toFixed(2)} د.ل
              </span>
            </div>
            <div>
              <span className="text-rose-700 text-[11px] font-bold block">المتبقي (الدين)</span>
              <span
                className={`font-black text-sm num-ltr ${
                  invoice.remainingAmount > 0 ? 'text-rose-600' : 'text-slate-400'
                }`}
              >
                {invoice.remainingAmount?.toFixed(2)} د.ل
              </span>
            </div>
          </div>

          {invoice.notes && (
            <div className="p-2.5 bg-slate-50 border border-slate-200 rounded-lg text-slate-600 text-xs">
              <strong>ملاحظات: </strong>
              {invoice.notes}
            </div>
          )}
        </div>
      ) : null}
    </Modal>
  );
};

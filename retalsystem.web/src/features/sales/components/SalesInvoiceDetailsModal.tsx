import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { salesService } from '../services/salesService';
import type { SalesInvoiceDto } from '../types/sales.types';
import { InvoiceStatusLabels } from '../types/sales.types';
import { IconPrinter, IconAlertCircle } from '@tabler/icons-react';

interface SalesInvoiceDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  invoiceId?: string | null;
}

export const SalesInvoiceDetailsModal: React.FC<SalesInvoiceDetailsModalProps> = ({
  isOpen,
  onClose,
  invoiceId,
}) => {
  const [invoice, setInvoice] = useState<SalesInvoiceDto | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && invoiceId) {
      setIsLoading(true);
      setError(null);
      salesService
        .getInvoiceById(invoiceId)
        .then((data) => setInvoice(data))
        .catch((err) => {
          console.error('Failed to load sales invoice details:', err);
          setError('تعذر تحميل تفاصيل الفاتورة من الخادم');
        })
        .finally(() => setIsLoading(false));
    } else {
      setInvoice(null);
    }
  }, [isOpen, invoiceId]);

  if (!isOpen) return null;

  const statusInfo = invoice?.status !== undefined ? InvoiceStatusLabels[invoice.status] : null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={invoice ? `تفاصيل فاتورة مبيعات #${invoice.invoiceNumber}` : 'تفاصيل الفاتورة'}
      subtitle="معاينة تفاصيل الفاتورة والبنود المباعة والمبالغ المحصلة والطباعة"
      size="2xl"
      footer={
        <div className="flex justify-between items-center w-full">
          <Button
            variant="outline"
            size="md"
            leftIcon={<IconPrinter className="w-4 h-4" />}
            onClick={() => window.print()}
          >
            طباعة الفاتورة
          </Button>
          <Button variant="secondary" size="md" onClick={onClose}>
            إغلاق
          </Button>
        </div>
      }
    >
      {isLoading ? (
        <div className="py-16 text-center text-slate-400">جاري تحميل تفاصيل الفاتورة...</div>
      ) : error ? (
        <div className="p-4 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-center text-sm flex items-center justify-center gap-2">
          <IconAlertCircle className="w-5 h-5 shrink-0" />
          <span>{error}</span>
        </div>
      ) : invoice ? (
        <div className="flex flex-col gap-4 py-1 text-xs">
          {/* Header Info Grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 p-3 bg-slate-50 border border-slate-200 rounded-xl">
            <div>
              <span className="text-slate-400 block text-[11px]">العميل:</span>
              <span className="font-bold text-slate-900 text-sm">{invoice.customerName || 'عميل نقدي'}</span>
            </div>
            <div>
              <span className="text-slate-400 block text-[11px]">المستودع / الصالة:</span>
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

          <div className="flex items-center justify-between px-1">
            <div className="flex items-center gap-2">
              <span className="text-slate-500 font-semibold">حالة الفاتورة:</span>
              {statusInfo && (
                <span className={`inline-block px-2.5 py-0.5 rounded-full text-[11px] font-bold ${statusInfo.bg} ${statusInfo.color}`}>
                  {statusInfo.text}
                </span>
              )}
            </div>
            <div className="flex items-center gap-2">
              <span className="text-slate-500 font-semibold">طريقة السداد:</span>
              <span className="font-bold text-slate-800">
                {invoice.paymentMethodName || (invoice.paymentMethod === 1 ? 'نقدي' : invoice.paymentMethod === 4 ? 'آجل' : 'أخرى')}
              </span>
            </div>
          </div>

          {/* Items Table */}
          <div className="border border-slate-200 rounded-xl overflow-hidden">
            <table className="w-full text-right text-xs">
              <thead className="bg-slate-50 text-slate-600 font-bold border-b border-slate-200">
                <tr>
                  <th className="py-2.5 px-3">#</th>
                  <th className="py-2.5 px-3">الصنف المباع</th>
                  <th className="py-2.5 px-3">الباركود / العبوة</th>
                  <th className="py-2.5 px-3 text-center">الكمية</th>
                  <th className="py-2.5 px-3 text-center">سعر البيع</th>
                  <th className="py-2.5 px-3 text-center">الخصم</th>
                  <th className="py-2.5 px-3 text-left">الإجمالي</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {(invoice.items || []).map((item, idx) => (
                  <tr key={idx} className="hover:bg-slate-50/50">
                    <td className="py-2.5 px-3 text-slate-400 font-mono">{idx + 1}</td>
                    <td className="py-2.5 px-3 font-bold text-slate-900">{item.productName}</td>
                    <td className="py-2.5 px-3 text-slate-500 font-mono text-[11px]">
                      {item.barcodeValue || item.barcodeTitle || '-'}
                    </td>
                    <td className="py-2.5 px-3 text-center font-bold text-slate-800">{item.quantity}</td>
                    <td className="py-2.5 px-3 text-center font-semibold text-slate-700 num-ltr">
                      {item.unitPrice.toFixed(2)} د.ل
                    </td>
                    <td className="py-2.5 px-3 text-center text-slate-500 num-ltr">
                      {item.discountAmount > 0 ? `-${item.discountAmount.toFixed(2)}` : '-'}
                    </td>
                    <td className="py-2.5 px-3 text-left font-black text-slate-900 num-ltr">
                      {(item.lineTotal ?? item.totalPrice ?? (item.quantity * item.unitPrice - item.discountAmount)).toFixed(2)} د.ل
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Totals Summary */}
          <div className="flex flex-col sm:flex-row justify-between items-start gap-4 pt-2">
            <div className="text-slate-500 text-xs max-w-sm">
              {invoice.notes && (
                <div className="p-2.5 bg-slate-50 border border-slate-200 rounded-lg">
                  <span className="font-bold text-slate-700 block mb-0.5">ملاحظات الفاتورة:</span>
                  <p>{invoice.notes}</p>
                </div>
              )}
            </div>

            <div className="w-full sm:w-72 bg-slate-50 border border-slate-200 rounded-xl p-3.5 flex flex-col gap-1.5 self-end">
              <div className="flex justify-between text-slate-600">
                <span>المجموع الفرعي:</span>
                <span className="font-bold num-ltr">{(invoice.subTotal || invoice.totalAmount).toFixed(2)} د.ل</span>
              </div>
              {(invoice.discountAmount || 0) > 0 && (
                <div className="flex justify-between text-slate-600">
                  <span>إجمالي الخصم:</span>
                  <span className="font-bold text-rose-600 num-ltr">-{(invoice.discountAmount || 0).toFixed(2)} د.ل</span>
                </div>
              )}
              <div className="flex justify-between font-black text-sm text-slate-900 border-t border-slate-200 pt-1.5 mt-0.5">
                <span>الإجمالي النهائي:</span>
                <span className="text-emerald-700 num-ltr">{invoice.totalAmount.toFixed(2)} د.ل</span>
              </div>
              <div className="flex justify-between text-slate-600 pt-1 border-t border-slate-100">
                <span>المبلغ المسدد:</span>
                <span className="font-bold text-emerald-800 num-ltr">{invoice.paidAmount.toFixed(2)} د.ل</span>
              </div>
              <div className="flex justify-between font-bold pt-1">
                <span>المتبقي (الذمة / الآجل):</span>
                <span className={`num-ltr ${invoice.remainingAmount > 0 ? 'text-rose-600' : 'text-slate-400'}`}>
                  {invoice.remainingAmount.toFixed(2)} د.ل
                </span>
              </div>
            </div>
          </div>
        </div>
      ) : null}
    </Modal>
  );
};

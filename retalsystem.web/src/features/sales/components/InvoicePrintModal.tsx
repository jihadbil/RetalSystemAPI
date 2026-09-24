import React from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import type { SalesInvoiceDto } from '../types/sales.types';
import { InvoiceStatusLabels } from '../types/sales.types';
import { IconPrinter } from '@tabler/icons-react';
import { useThermalPrint } from '../../../shared/hooks/useThermalPrint';

interface InvoicePrintModalProps {
  isOpen: boolean;
  onClose: () => void;
  invoice: SalesInvoiceDto | null;
}

export const InvoicePrintModal: React.FC<InvoicePrintModalProps> = ({ isOpen, onClose, invoice }) => {
  const { printDocument } = useThermalPrint();

  if (!invoice) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`فاتورة مبيعات ${invoice.invoiceNumber}`}
      subtitle="معاينة وطباعة الفاتورة بصيغة A4 الرسمية"
      size="xl"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose}>
            إغلاق
          </Button>
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconPrinter className="w-4 h-4" />}
            onClick={() => printDocument()}
          >
            طباعة الفاتورة (A4)
          </Button>
        </>
      }
    >
      {/* قالب الفاتورة A4 */}
      <div className="invoice-a4 bg-white p-6 rounded-xl border border-slate-200 text-slate-800 text-xs flex flex-col gap-6">
        {/* الترويسة */}
        <div className="flex items-start justify-between border-b border-slate-200 pb-5">
          <div className="flex flex-col">
            <h2 className="text-xl font-extrabold text-slate-900 tracking-tight">شركة ريتال للتجارة والخدمات</h2>
            <p className="text-slate-500 mt-1">{invoice.branchName || 'الفرع الرئيسي'}</p>
            <p className="text-slate-500">الرقم الضريبي: 300123456789</p>
          </div>

          <div className="flex flex-col text-left">
            <span className="text-lg font-black text-emerald-700">{invoice.invoiceNumber}</span>
            <span className="text-slate-500 mt-1">
              التاريخ: {new Date(invoice.invoiceDate).toLocaleDateString('ar-LY')}
            </span>
            <span className="font-bold text-slate-700 mt-1">
              الحالة: {InvoiceStatusLabels[invoice.status]?.text || 'مصدرة'}
            </span>
          </div>
        </div>

        {/* بيانات العميل والمستودع */}
        <div className="grid grid-cols-2 gap-4 p-4 rounded-xl bg-slate-50/70 border border-slate-100">
          <div>
            <span className="text-slate-400 font-semibold block mb-1">فاتورة موجهة إلى:</span>
            <h4 className="text-sm font-bold text-slate-900">{invoice.customerName || 'عميل نقدي'}</h4>
          </div>
          <div>
            <span className="text-slate-400 font-semibold block mb-1">المستودع المصروف منه:</span>
            <h4 className="text-sm font-bold text-slate-900">{invoice.warehouseName || 'المستودع الرئيسي'}</h4>
          </div>
        </div>

        {/* جدول بنود الفاتورة */}
        <div className="border border-slate-200 rounded-xl overflow-hidden">
          <table className="w-full text-right text-xs">
            <thead className="bg-slate-50 border-b border-slate-200 text-slate-600 font-bold">
              <tr>
                <th className="py-2.5 px-3">#</th>
                <th className="py-2.5 px-4">الصنف والوصف</th>
                <th className="py-2.5 px-3 text-center">الكمية</th>
                <th className="py-2.5 px-3 text-center">سعر الوحدة</th>
                <th className="py-2.5 px-3 text-center">الخصم</th>
                <th className="py-2.5 px-4 text-left">الإجمالي</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {invoice.items.map((item, idx) => (
                <tr key={idx}>
                  <td className="py-2.5 px-3 text-slate-400 font-mono">{idx + 1}</td>
                  <td className="py-2.5 px-4 font-bold text-slate-900">{item.productName}</td>
                  <td className="py-2.5 px-3 text-center font-bold">{item.quantity}</td>
                  <td className="py-2.5 px-3 text-center num-ltr">{item.unitPrice.toFixed(2)}</td>
                  <td className="py-2.5 px-3 text-center num-ltr text-slate-500">
                    {item.discountAmount > 0 ? `-${item.discountAmount.toFixed(2)}` : '-'}
                  </td>
                  <td className="py-2.5 px-4 text-left font-extrabold text-slate-900 num-ltr">
                    {item.totalPrice.toFixed(2)} د.ل
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* ملخص الحساب والتوقيعات */}
        <div className="flex flex-col sm:flex-row justify-between items-end gap-6 pt-2">
          <div className="text-xs text-slate-500 max-w-xs">
            <p className="font-semibold text-slate-700">ملاحظات وشروط:</p>
            <p className="mt-1 leading-relaxed">
              تعتبر هذه الفاتورة رسمية ومعتمدة من النظام السحابي. شكراً لتعاملكم معنا.
            </p>
          </div>

          <div className="w-full sm:w-72 flex flex-col gap-1.5 p-3.5 bg-slate-50 rounded-xl border border-slate-200/80">
            <div className="flex justify-between">
              <span>المجموع الفرعي:</span>
              <span className="font-bold num-ltr">{invoice.subTotal.toFixed(2)} د.ل</span>
            </div>
            {invoice.discountAmount > 0 && (
              <div className="flex justify-between text-slate-600">
                <span>الخصم:</span>
                <span className="font-bold num-ltr">-{invoice.discountAmount.toFixed(2)} د.ل</span>
              </div>
            )}
            <div className="flex justify-between text-base font-black text-emerald-800 border-t border-slate-200 pt-2 mt-1">
              <span>الإجمالي النهائي:</span>
              <span className="num-ltr">{invoice.totalAmount.toFixed(2)} د.ل</span>
            </div>
            <div className="flex justify-between text-slate-600 pt-1">
              <span>المبلغ المدفوع:</span>
              <span className="font-semibold num-ltr">{invoice.paidAmount.toFixed(2)} د.ل</span>
            </div>
            {invoice.remainingAmount > 0 && (
              <div className="flex justify-between text-rose-700 font-bold pt-1">
                <span>المتبقي في الذمة:</span>
                <span className="num-ltr">{invoice.remainingAmount.toFixed(2)} د.ل</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </Modal>
  );
};

import React from 'react';
import type { ReceiptData } from '../types/pos.types';

interface ReceiptPrintViewProps {
  receipt: ReceiptData | null;
}

export const ReceiptPrintView: React.FC<ReceiptPrintViewProps> = ({ receipt }) => {
  if (!receipt) return null;

  return (
    <div className="print-only receipt-80mm text-black font-mono">
      {/* ترويسة المتجر */}
      <div className="text-center mb-2">
        <h2 className="text-base font-bold uppercase">{receipt.tenantName || 'نظام ريتال'}</h2>
        <p className="text-xs">{receipt.branchName}</p>
        {receipt.taxNumber && <p className="text-[10px]">الرقم الضريبي: {receipt.taxNumber}</p>}
        <p className="text-[10px]">{receipt.date}</p>
      </div>

      <div className="receipt-divider" />

      {/* بيانات الفاتورة */}
      <div className="text-[11px] mb-2">
        <div className="flex justify-between">
          <span>رقم الفاتورة:</span>
          <span className="font-bold">{receipt.invoiceNumber}</span>
        </div>
        <div className="flex justify-between">
          <span>الكاشير:</span>
          <span>{receipt.cashierName}</span>
        </div>
        <div className="flex justify-between">
          <span>العميل:</span>
          <span>{receipt.customerName || 'عميل نقدي'}</span>
        </div>
      </div>

      <div className="receipt-divider" />

      {/* جدول الأصناف */}
      <table className="w-full text-right text-[11px] mb-2">
        <thead>
          <tr className="border-b border-black">
            <th className="py-1">الصنف</th>
            <th className="py-1 text-center">الكمية</th>
            <th className="py-1 text-center">السعر</th>
            <th className="py-1 text-left">الإجمالي</th>
          </tr>
        </thead>
        <tbody>
          {receipt.items.map((item, idx) => (
            <tr key={idx} className="border-b border-dashed border-gray-400">
              <td className="py-1 max-w-[110px] truncate">{item.name}</td>
              <td className="py-1 text-center">{item.quantity}</td>
              <td className="py-1 text-center num-ltr">{item.price.toFixed(2)}</td>
              <td className="py-1 text-left font-bold num-ltr">{item.total.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <div className="receipt-divider" />

      {/* الملخص المالي */}
      <div className="text-[11px] flex flex-col gap-1 mb-2">
        <div className="flex justify-between">
          <span>المجموع الفرعي:</span>
          <span className="num-ltr">{receipt.subtotal.toFixed(2)} د.ل</span>
        </div>
        {receipt.discount > 0 && (
          <div className="flex justify-between text-black">
            <span>الخصم:</span>
            <span className="num-ltr">-{receipt.discount.toFixed(2)} د.ل</span>
          </div>
        )}
        {receipt.tax > 0 && (
          <div className="flex justify-between">
            <span>الضريبة:</span>
            <span className="num-ltr">+{receipt.tax.toFixed(2)} د.ل</span>
          </div>
        )}
        <div className="flex justify-between text-sm font-bold border-t border-black pt-1">
          <span>الإجمالي النهائي:</span>
          <span className="num-ltr">{receipt.total.toFixed(2)} د.ل</span>
        </div>
        <div className="flex justify-between">
          <span>طريقة الدفع:</span>
          <span>{receipt.paymentMethod}</span>
        </div>
        <div className="flex justify-between">
          <span>المبلغ المستلم:</span>
          <span className="num-ltr">{receipt.paid.toFixed(2)} د.ل</span>
        </div>
        {receipt.change > 0 && (
          <div className="flex justify-between font-bold">
            <span>المتبقي (الفكة):</span>
            <span className="num-ltr">{receipt.change.toFixed(2)} د.ل</span>
          </div>
        )}
      </div>

      <div className="receipt-divider" />

      {/* كود QR وتذييل الإيصال */}
      <div className="flex flex-col items-center justify-center my-3 text-center">
        <div className="w-24 h-24 border-2 border-black flex items-center justify-center p-1 text-[9px] font-mono break-all leading-tight">
          [QR CODE]
          <br />
          {receipt.invoiceNumber}
        </div>
        <p className="text-[10px] mt-2 font-semibold">شكراً لتسوقكم معنا!</p>
        <p className="text-[9px] text-gray-700">البضاعة المباعة ترد وتستبدل خلال 3 أيام مع الفاتورة</p>
      </div>
    </div>
  );
};

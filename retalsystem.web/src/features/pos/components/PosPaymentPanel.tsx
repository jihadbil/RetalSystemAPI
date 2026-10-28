import React, { useState } from 'react';
import { usePos } from '../context/PosContext';
import { PaymentMethod, PaymentMethodLabels } from '../types/pos.types';
import { PosSplitPaymentModal } from './PosSplitPaymentModal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import {
  IconCash,
  IconCreditCard,
  IconClock,
  IconReceipt,
  IconPrinter,
} from '@tabler/icons-react';
import { useAuth } from '../../../shared/context/AuthContext';
import { Permissions } from '../../../shared/auth/permissions';

interface PosPaymentPanelProps {
  onOpenDraftsModal: () => void;
  onOpenDiscountModal: () => void;
}

export const PosPaymentPanel: React.FC<PosPaymentPanelProps> = ({
  onOpenDraftsModal,
  onOpenDiscountModal,
}) => {
  const { hasPermission } = useAuth();
  const canCreateSale = hasPermission(Permissions.Pos.CreateSale);
  const canApplyDiscount = hasPermission(Permissions.Pos.ApplyDiscount);
  const {
    splitPayments,
    applySplitPayments,
    customer,
    cartItems,
    subtotal,
    discountAmount,
    taxAmount,
    grandTotal,
    paidAmount,
    changeAmount,
    remainingAmount,
    paymentMethod,
    setPaymentMethod,
    setPaidAmount,
    setExactCash,
    setCardPayment,
    holdCurrentOrder,
    heldDrafts,
    completeSale,
    isCheckingOut,
    notes,
    setNotes,
  } = usePos();

  const [showNotes, setShowNotes] = useState(false);
  const [splitOpen, setSplitOpen] = useState(false);

  // أزرار الفئات النقدية السريعة
  const handleAddCash = (amount: number) => {
    setPaymentMethod(PaymentMethod.Cash);
    setPaidAmount(paidAmount + amount);
  };

  return (
    <div className="w-full lg:w-96 bg-white rounded-xl border border-slate-200 shadow-2xs flex flex-col p-4 gap-4 shrink-0">
      {splitOpen && <PosSplitPaymentModal total={grandTotal} payments={splitPayments || []} hasCustomer={Boolean(customer.id)}
        onClose={() => setSplitOpen(false)} onSave={payments => { applySplitPayments(payments); setSplitOpen(false); }} />}
      {/* عرض الإجمالي الكبير المعتمد في design.md (30px) */}
      <div className="p-4 rounded-xl bg-gradient-to-b from-emerald-50/80 to-emerald-50/20 border border-emerald-200/70 text-center">
        <span className="text-xs font-bold text-emerald-800 uppercase tracking-wide">
          الإجمالي المستحق للدفع
        </span>
        <div className="text-[30px] font-black text-emerald-700 leading-tight mt-1 num-ltr">
          {grandTotal.toFixed(2)}{' '}
          <span className="text-sm font-bold text-emerald-600">د.ل</span>
        </div>
      </div>

      {/* تفاصيل الحسابات المالية */}
      <div className="flex flex-col gap-1.5 text-xs border-b border-slate-100 pb-3">
        <div className="flex justify-between text-slate-600">
          <span>المجموع الفرعي:</span>
          <span className="font-semibold text-slate-800 num-ltr">{subtotal.toFixed(2)} د.ل</span>
        </div>

        <div className="flex justify-between items-center text-slate-600">
          {canApplyDiscount ? (
            <button
              type="button"
              onClick={onOpenDiscountModal}
              className="text-emerald-700 hover:text-emerald-800 underline font-medium cursor-pointer"
            >
              الخصم المطبق (F3):
            </button>
          ) : <span>الخصم المطبق:</span>}
          <span className="font-semibold text-slate-800 num-ltr">
            {discountAmount > 0 ? `-${discountAmount.toFixed(2)}` : '0.00'} د.ل
          </span>
        </div>

        {taxAmount > 0 && (
          <div className="flex justify-between text-slate-600">
            <span>الضريبة:</span>
            <span className="font-semibold text-slate-800 num-ltr">+{taxAmount.toFixed(2)} د.ل</span>
          </div>
        )}
      </div>

      {/* أزرار اختيار طريقة الدفع السريعة */}
      <div className="flex flex-col gap-2">
        <span className="text-xs font-semibold text-slate-700">طريقة الدفع:</span>
        <div className="grid grid-cols-2 gap-2">
          {/* كاش */}
          <button
            type="button"
            onClick={() => setPaymentMethod(PaymentMethod.Cash)}
            className={`flex items-center justify-center gap-2 h-11 rounded-lg border font-bold text-xs transition-all cursor-pointer ${
              paymentMethod === PaymentMethod.Cash
                ? 'bg-emerald-600 text-white border-emerald-600 shadow-xs'
                : 'border-slate-200 text-slate-700 hover:bg-slate-50'
            }`}
          >
            <IconCash className="w-4 h-4" />
            <span>نقداً (كاش)</span>
          </button>

          {/* بطاقة مصرفية (F9) */}
          <button
            type="button"
            onClick={setCardPayment}
            className={`flex items-center justify-center gap-2 h-11 rounded-lg border font-bold text-xs transition-all cursor-pointer ${
              paymentMethod === PaymentMethod.Card
                ? 'bg-emerald-600 text-white border-emerald-600 shadow-xs'
                : 'border-slate-200 text-slate-700 hover:bg-slate-50'
            }`}
          >
            <IconCreditCard className="w-4 h-4" />
            <span>بطاقة (F9)</span>
          </button>
        </div>
      </div>

      {/* حقل المبلغ المستلم والمتبقي (لا يتغير تلقائياً مع الإجمالي) */}
      <Button variant="secondary" onClick={() => setSplitOpen(true)} disabled={!cartItems.length || isCheckingOut}>دفع متعدد</Button>
      {splitPayments && <div className="rounded-lg bg-emerald-50 p-3 text-sm space-y-1" aria-label="توزيع الدفع المعتمد">
        {splitPayments.map(payment => <p key={payment.paymentMethod}>{PaymentMethodLabels[payment.paymentMethod]}: {payment.amount.toFixed(4)} د.ل</p>)}
        {changeAmount > 0 && <p>الفكة: {changeAmount.toFixed(4)} د.ل</p>}
        {remainingAmount > 0 && <p>على حساب العميل: {remainingAmount.toFixed(4)} د.ل</p>}
      </div>}
      {!splitPayments && <div className="flex flex-col gap-2 bg-slate-50/80 p-3 rounded-xl border border-slate-200/70">
        <div className="flex items-center justify-between">
          <label className="text-xs font-bold text-slate-700">المبلغ المستلم (نقد):</label>
          <button
            type="button"
            onClick={setExactCash}
            className="text-[11px] font-bold text-emerald-700 hover:text-emerald-800 underline cursor-pointer"
          >
            المبلغ كاملاً (F8)
          </button>
        </div>

        <Input
          type="number"
          step="0.5"
          min="0"
          value={paidAmount || ''}
          onChange={(e) => {
            const val = parseFloat(e.target.value);
            setPaidAmount(isNaN(val) ? 0 : val);
            setPaymentMethod(PaymentMethod.Cash);
          }}
          isLtr
          className="text-lg font-bold text-slate-900 num-ltr text-center h-11"
          placeholder="0.00"
        />

        {/* فئات نقدية سريعة */}
        <div className="grid grid-cols-4 gap-1.5 pt-1">
          {[5, 10, 20, 50].map((denom) => (
            <button
              key={denom}
              type="button"
              onClick={() => handleAddCash(denom)}
              className="py-1 rounded bg-white hover:bg-emerald-50 border border-slate-200 text-[11px] font-bold text-slate-700 transition-colors num-ltr"
            >
              +{denom}
            </button>
          ))}
        </div>

        {/* حساب الباقي / المتبقي */}
        <div className="pt-2 border-t border-slate-200 flex items-center justify-between text-xs">
          {changeAmount > 0 ? (
            <>
              <span className="font-bold text-emerald-800">الباقي للزبون (الفكة):</span>
              <span className="text-sm font-extrabold text-emerald-700 num-ltr">
                {changeAmount.toFixed(2)} د.ل
              </span>
            </>
          ) : remainingAmount > 0 ? (
            <>
              <span className="font-bold text-rose-700">المبلغ المتبقي:</span>
              <span className="text-sm font-extrabold text-rose-700 num-ltr">
                {remainingAmount.toFixed(2)} د.ل
              </span>
            </>
          ) : (
            <span className="text-slate-500 font-semibold text-center w-full">
              المبلغ مدفوع بالكامل
            </span>
          )}
        </div>
      </div>}

      {/* الملاحظات الاختيارية */}
      <div>
        <button
          type="button"
          onClick={() => setShowNotes(!showNotes)}
          className="text-[11px] text-slate-500 hover:text-slate-800 font-semibold cursor-pointer underline"
        >
          {showNotes ? 'إخفاء الملاحظات' : '+ إضافة ملاحظات على الفاتورة'}
        </button>
        {showNotes && (
          <textarea
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            placeholder="ملاحظات تظهر في أسفل الفاتورة..."
            className="w-full text-xs p-2.5 mt-2 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            rows={2}
          />
        )}
      </div>

      {/* الأزرار التنفيذية السفلية */}
      <div className="mt-auto flex flex-col gap-2 pt-2">
        {/* زر إتمام البيع والطباعة الفورية (F10) */}
        {canCreateSale && <Button
          variant="primary"
          size="lg"
          onClick={completeSale}
          isLoading={isCheckingOut}
          disabled={cartItems.length === 0}
          leftIcon={<IconPrinter className="w-5 h-5" />}
          className="w-full text-base font-extrabold h-12 shadow-md bg-emerald-600 hover:bg-emerald-700 active:bg-emerald-800"
        >
          إتمام البيع والطباعة (F10)
        </Button>}

        {/* أزرار تعليق واسترجاع الطلب (F5 / F6) */}
        <div className="grid grid-cols-2 gap-2">
          <Button
            variant="secondary"
            size="sm"
            onClick={holdCurrentOrder}
            disabled={cartItems.length === 0}
            leftIcon={<IconClock className="w-4 h-4 text-amber-600" />}
            title="تعليق الطلب الحالي لخدمة زبون آخر (F5)"
          >
            تعليق الطلب (F5)
          </Button>

          <Button
            variant="secondary"
            size="sm"
            onClick={onOpenDraftsModal}
            leftIcon={<IconReceipt className="w-4 h-4 text-emerald-600" />}
            title="استعادة طلب معلق (F6)"
          >
            المعلقات ({heldDrafts.length}) F6
          </Button>
        </div>
      </div>
    </div>
  );
};

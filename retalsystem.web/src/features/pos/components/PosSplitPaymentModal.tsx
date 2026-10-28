import { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { PaymentMethod, PaymentMethodLabels, PosTenderMethods, type PosPayment } from '../types/pos.types';
import { sumPayments, validateSplitPayments } from '../utils/splitPayments';

interface Props {
  total: number;
  payments: PosPayment[];
  hasCustomer: boolean;
  onSave: (payments: PosPayment[]) => void;
  onClose: () => void;
}

export function PosSplitPaymentModal({ total, payments, hasCustomer, onSave, onClose }: Props) {
  const [amounts, setAmounts] = useState<Record<number, string>>(() => Object.fromEntries(payments.map(p => [p.paymentMethod, String(p.amount)])));
  const [tendered, setTendered] = useState(String(payments.find(p => p.paymentMethod === PaymentMethod.Cash)?.tenderedAmount ?? ''));
  const [error, setError] = useState('');
  const draft = PosTenderMethods.filter(method => Number(amounts[method] || 0) !== 0).map(method => ({
    clientMovementId: payments.find(p => p.paymentMethod === method)?.clientMovementId || '',
    paymentMethod: method, amount: Number(amounts[method]),
    ...(method === PaymentMethod.Cash && tendered !== '' ? { tenderedAmount: Number(tendered) } : {}),
  }));
  const paid = sumPayments(draft);
  const save = () => {
    const next = draft.map(p => ({ ...p, clientMovementId: p.clientMovementId || crypto.randomUUID() }));
    const invalid = validateSplitPayments(next, total, hasCustomer);
    if (invalid) { setError(invalid); return; }
    onSave(next);
  };
  return <Modal isOpen onClose={onClose} title="الدفع بأكثر من وسيلة" size="md"
    subtitle="أدخل المبلغ المدفوع بكل وسيلة ثم احفظ التوزيع وأكمل البيع"
    footer={<><Button variant="secondary" onClick={onClose}>إلغاء</Button><Button onClick={save}>حفظ التوزيع</Button></>}>
    <div className="space-y-4">
      <p className="font-bold">إجمالي الفاتورة: <span dir="ltr">{total.toFixed(4).replace(/0{1,2}$/, '')} د.ل</span></p>
      {PosTenderMethods.map(method => <div key={method} className="flex items-center justify-between gap-3">
        <label htmlFor={`split-${method}`} className="font-semibold text-sm">{PaymentMethodLabels[method]}</label>
        <Input id={`split-${method}`} aria-label={`مبلغ ${PaymentMethodLabels[method]}`} type="number" min="0" step="0.0001" isLtr
          value={amounts[method] || ''} placeholder="0.00" className="max-w-44"
          onChange={e => { setAmounts(previous => ({ ...previous, [method]: e.target.value })); setError(''); }} />
      </div>)}
      {Number(amounts[PaymentMethod.Cash]) > 0 && <div>
        <label htmlFor="split-tendered" className="text-sm">النقد المستلم (اختياري لحساب الفكة)</label>
        <Input id="split-tendered" type="number" min="0" step="0.0001" isLtr value={tendered} onChange={e => setTendered(e.target.value)} />
      </div>}
      <div className="rounded-lg bg-slate-50 p-3 text-sm space-y-2">
        <p>مجموع المدفوع: <span dir="ltr">{Number.isFinite(paid) ? paid.toFixed(4) : '—'} د.ل</span></p>
        <p>المتبقي: <span dir="ltr">{Number.isFinite(paid) ? Math.max(0, total - paid).toFixed(4) : '—'} د.ل</span></p>
        {Number(tendered) > Number(amounts[PaymentMethod.Cash] || 0) && <p>الفكة: {(Number(tendered) - Number(amounts[PaymentMethod.Cash])).toFixed(4)} د.ل</p>}
        {hasCustomer && paid < total && <p className="text-amber-800">المتبقي سيُسجل على حساب العميل وفق سقفه الائتماني.</p>}
      </div>
      {error && <p role="alert" className="text-rose-700 text-sm">{error}</p>}
    </div>
  </Modal>;
}

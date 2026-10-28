import { useCallback, useEffect, useState } from 'react';
import { apiClient } from '../../../shared/services/apiClient';
import { Button } from '../../../shared/components/ui/Button';
import { Select } from '../../../shared/components/ui/Select';
import { Modal } from '../../../shared/components/ui/Modal';
import { useAuth } from '../../../shared/context/AuthContext';
import { useRequestGate } from '../../../shared/hooks/useRequestGate';
import { Permissions } from '../../../shared/auth/permissions';
import { PaymentMethod, PaymentMethodLabels } from '../../pos/types/pos.types';

interface SessionPayments {
  id: string; branchId: string; terminalName: string; cashDrawerName: string; openedAt: string; status: number; isSettled: boolean;
  totals: { paymentMethod: PaymentMethod; amount: number; safeId?: string; safeName?: string }[];
  movements: { id: string; invoiceNumber: string; paymentMethod: PaymentMethod; direction: number; amount: number; isSettled: boolean }[];
}

export function PosSessionPaymentsView() {
  const { currentBranchId, hasPermission } = useAuth();
  const [sessions, setSessions] = useState<SessionPayments[]>([]);
  const [selected, setSelected] = useState<SessionPayments | null>(null);
  const [accounts, setAccounts] = useState<Record<number, string>>({});
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const gate = useRequestGate();
  const settlementGate = useRequestGate();
  const load = useCallback(async () => {
    const current = gate.begin();
    try {
      const result = await apiClient.get<SessionPayments[]>('cash-sessions/payments', currentBranchId ? { branchId: currentBranchId } : undefined);
      if (current()) { setSessions(result); setError(''); }
    } catch (failure) { if (current()) setError(failure instanceof Error ? failure.message : 'تعذر تحميل حركات الورديات'); }
  }, [currentBranchId, gate]);
  useEffect(() => {
    setSessions([]); setSelected(null); setBusy(false);
    void load();
    return () => { gate.invalidate(); settlementGate.invalidate(); };
  }, [load, gate, settlementGate]);
  const settle = async () => {
    if (!selected || busy || !hasPermission(Permissions.Safes.Transfer)) return;
    if (selected.totals.some(total => !accounts[total.paymentMethod])) { setError('اختر حساب التسوية لكل وسيلة'); return; }
    setBusy(true); setError('');
    const current = settlementGate.begin();
    try {
      const result = await apiClient.post<SessionPayments>(`cash-sessions/${selected.id}/settle-payments`, {
        accounts: selected.totals.map(total => ({ paymentMethod: total.paymentMethod, safeId: accounts[total.paymentMethod] })),
      });
      if (!current()) return;
      setSelected(result);
      await load();
    } catch (failure) { if (current()) setError(failure instanceof Error ? failure.message : 'تعذر إتمام التسوية؛ أعد تحميل النتيجة قبل المحاولة'); }
    finally { if (current()) setBusy(false); }
  };
  return <div className="space-y-4">
    <div className="flex flex-wrap items-center justify-between gap-2">
      <div><h3 className="font-bold">حركات دفع الورديات</h3><p className="text-sm text-slate-600">إغلاق الوردية مستقل. يمكنك تسوية التحصيلات لاحقاً إلى حساب كل وسيلة.</p></div>
      <Button variant="secondary" onClick={() => void load()} disabled={busy}>تحديث الحركات</Button>
    </div>
    {error && !selected && <p role="alert" className="text-rose-700">{error}</p>}
    {!sessions.length && <p className="text-slate-500 p-5">لا توجد حركات دفع متعددة للورديات الحالية.</p>}
    {sessions.map(session => <div key={session.id} className="rounded-xl border bg-white p-4 flex flex-wrap items-center justify-between gap-3">
      <div><p className="font-semibold">{session.terminalName} — {session.cashDrawerName}</p>
        <p className="text-xs text-slate-500">{new Date(session.openedAt).toLocaleString('ar-LY')} · {session.isSettled ? 'تمت التسوية' : session.status === 3 ? 'مغلقة — غير مسواة' : session.status === 2 ? 'بانتظار اعتماد الجرد' : 'مفتوحة'}</p>
        <p className="text-sm mt-1">{session.totals.map(total => `${PaymentMethodLabels[total.paymentMethod]}: ${total.amount.toFixed(4)}`).join(' · ')}</p>
      </div>
      <Button variant="secondary" onClick={() => { setSelected(session); setAccounts({}); setError(''); }}>عرض الحركات والتسوية</Button>
    </div>)}
    {selected && <Modal isOpen title={`حركات درج ${selected.cashDrawerName}`} size="xl" onClose={() => { if (!busy) setSelected(null); }} closeOnBackdrop={!busy}
      footer={<><Button variant="secondary" disabled={busy} onClick={() => setSelected(null)}>إغلاق النافذة</Button>
        {!selected.isSettled && hasPermission(Permissions.Safes.Transfer) && <Button onClick={() => void settle()} isLoading={busy} disabled={selected.status !== 3}>تسوية اختيارية إلى الحسابات</Button>}</>}>
      <div className="space-y-4">
        {selected.isSettled ? <div className="text-emerald-700 space-y-1"><p className="font-semibold">تم تحويل حركات هذه الوردية إلى حساباتها.</p>
          {selected.totals.map(total => <p key={total.paymentMethod}>{PaymentMethodLabels[total.paymentMethod]}: {total.amount.toFixed(4)} د.ل {total.safeName ? `← ${total.safeName}` : ''}</p>)}
        </div> : <>
          {selected.status !== 3 && <p className="text-amber-800">يمكن التسوية بعد إغلاق الوردية واعتماد فرق الجرد. الإغلاق لا ينفذ التسوية تلقائياً.</p>}
          {selected.totals.map(total => <div key={total.paymentMethod} className="grid grid-cols-2 gap-3 items-center">
            <label className="text-sm font-semibold">{PaymentMethodLabels[total.paymentMethod]}: {total.amount.toFixed(4)} د.ل</label>
            <Select value={accounts[total.paymentMethod] || ''} label={`حساب تسوية ${PaymentMethodLabels[total.paymentMethod]}`}
              disabled={busy || selected.status !== 3 || !hasPermission(Permissions.Safes.Transfer)}
              helperText={total.paymentMethod === PaymentMethod.Cash ? 'اختر صندوقاً نقدياً' : 'اختر حساباً مصرفياً'}
              lookup={{ resource: 'safes', branchId: selected.branchId }}
              onLookupSelect={option => setAccounts(previous => ({ ...previous, [total.paymentMethod]: option.id }))}
              onChange={event => setAccounts(previous => ({ ...previous, [total.paymentMethod]: event.target.value }))} />
          </div>)}
        </>}
        <div className="overflow-x-auto"><table className="w-full text-sm text-right"><thead><tr className="border-b"><th className="p-2">الفاتورة</th><th>الوسيلة</th><th>الحركة</th><th>المبلغ</th><th>التسوية</th></tr></thead>
          <tbody>{selected.movements.map(movement => <tr key={movement.id} className="border-b"><td className="p-2">{movement.invoiceNumber}</td><td>{PaymentMethodLabels[movement.paymentMethod]}</td><td>{movement.direction === 1 ? 'قبض' : 'رد'}</td><td dir="ltr">{movement.amount.toFixed(4)}</td><td>{movement.isSettled ? 'تمت' : 'معلقة'}</td></tr>)}</tbody>
        </table></div>
        {error && <p role="alert" className="text-rose-700">{error}</p>}
      </div>
    </Modal>}
  </div>;
}

import { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Button } from '../../../shared/components/ui/Button';
import { accountingService } from '../services/accountingService';

export function PosTerminalFormModal({ branchId: initialBranch, onClose, onSaved }: {
  branchId?: string; onClose: () => void; onSaved: () => void;
}) {
  const [branchId, setBranchId] = useState(initialBranch || '');
  const [drawerId, setDrawerId] = useState('');
  const [name, setName] = useState('');
  const [code, setCode] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  return <Modal isOpen onClose={onClose} title="إضافة نقطة بيع">
    <form className="flex flex-col gap-3" onSubmit={async (event) => {
      event.preventDefault();
      if (saving || !branchId || !drawerId || !name.trim() || !code.trim()) return;
      setSaving(true); setError('');
      try { await accountingService.createPosTerminal({ branchId, defaultCashDrawerId: drawerId, code: code.trim(), name: name.trim() }); onSaved(); onClose(); }
      catch (error) { setError(error instanceof Error ? error.message : 'تعذر إنشاء نقطة البيع'); }
      finally { setSaving(false); }
    }}>
      <Input label="رمز نقطة البيع" value={code} onChange={(event) => setCode(event.target.value)} required disabled={saving} />
      <Input label="اسم نقطة البيع" value={name} onChange={(event) => setName(event.target.value)} required disabled={saving} />
      <Select lookup={{ resource: 'branches' }} label="الفرع" value={branchId}
        onChange={(event) => { setBranchId(event.target.value); setDrawerId(''); }} required disabled={saving} />
      <Select lookup={{ resource: 'cash-drawers', branchId }} label="درج النقود" value={drawerId}
        onChange={(event) => setDrawerId(event.target.value)} required disabled={saving || !branchId} />
      {error && <p role="alert" className="text-rose-600 text-sm">{error}</p>}
      <Button type="submit" isLoading={saving} disabled={!branchId || !drawerId || !name.trim() || !code.trim()}>حفظ نقطة البيع</Button>
    </form>
  </Modal>;
}

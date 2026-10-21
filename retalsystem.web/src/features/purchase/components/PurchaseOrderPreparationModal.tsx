import { lookupService, type LookupOption } from '../../../shared/services/lookupService';
import { useEffect, useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Select } from '../../../shared/components/ui/Select';
import { purchaseService } from '../services/purchaseService';
import { stockService } from '../../stock/services/stockService';
import type { WarehouseDto } from '../../stock/types/stock.types';

export function PurchaseOrderPreparationModal({ orderId, onClose, onSaved }: {
  orderId: string | null; onClose: () => void; onSaved: () => void;
}) {
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [suppliers, setSuppliers] = useState<LookupOption[]>([]);
  const [warehouseId, setWarehouseId] = useState('');
  const [supplierId, setSupplierId] = useState('');
  const [expectedDate, setExpectedDate] = useState<string | undefined>();
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  useEffect(() => {
    if (!orderId) return;
    let cancelled = false;
    setLoading(true); setError(''); setWarehouseId(''); setSupplierId('');
    void purchaseService.getOrderById(orderId).then(async (order) => {
      const [allWarehouses, allSuppliers] = await Promise.all([
        stockService.getWarehouses(order.branchId, order.warehouseId), lookupService.list('suppliers'),
      ]);
      if (cancelled) return;
      const activeWarehouses = allWarehouses.filter((warehouse) => warehouse.isActive);
      const activeSuppliers = allSuppliers.filter((supplier) => supplier.isActive);
      setWarehouses(activeWarehouses); setSuppliers(activeSuppliers);
      setWarehouseId(activeWarehouses.some((warehouse) => warehouse.id === order.warehouseId) ? order.warehouseId! : activeWarehouses[0]?.id || '');
      setSupplierId(activeSuppliers.some((supplier) => supplier.id === order.supplierId) ? order.supplierId : activeSuppliers[0]?.id || '');
      setExpectedDate(order.expectedDate);
    }).catch((err: unknown) => {
      if (!cancelled) setError(err instanceof Error ? err.message : 'تعذر تحميل أمر الشراء');
    }).finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [orderId]);

  const save = async () => {
    if (!orderId || loading || saving || !warehouseId || !supplierId) return;
    setSaving(true); setError('');
    try {
      await purchaseService.prepareOrder(orderId, { supplierId, warehouseId, expectedDate });
      onSaved(); onClose();
    } catch (err) { setError(err instanceof Error ? err.message : 'تعذر حفظ بيانات الاستلام'); }
    finally { setSaving(false); }
  };
  return <Modal isOpen={!!orderId} onClose={onClose} title="تجهيز أمر الشراء للاستلام" size="md" footer={<>
    <Button variant="secondary" onClick={onClose} disabled={saving}>إلغاء</Button>
    <Button onClick={save} isLoading={saving} disabled={loading || !warehouseId || !supplierId}>حفظ بيانات الاستلام</Button>
  </>}>
    <div className="flex flex-col gap-3">
      {error && <p className="text-xs text-rose-600">{error}</p>}
      <Select lookup={{ resource: 'suppliers' }} label="المورد" value={supplierId} onChange={(e) => setSupplierId(e.target.value)} disabled={loading || saving}
        options={suppliers.map((supplier) => ({ value: supplier.id, label: supplier.name }))} />
      <Select lookup={{ resource: 'warehouses' }} onLookupSelect={(option) => setWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="المستودع المستلم" value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)} disabled={loading || saving}
        options={warehouses.map((warehouse) => ({ value: warehouse.id, label: warehouse.name }))} />
    </div>
  </Modal>;
}

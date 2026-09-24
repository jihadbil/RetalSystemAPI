import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { stockService } from '../../stock/services/stockService';
import { supplierService } from '../../suppliers/services/supplierService';
import { accountingService } from '../../accounting/services/accountingService';
import { catalogService } from '../../catalog/services/catalogService';
import { useAuth } from '../../../shared/context/AuthContext';
import type { WarehouseDto } from '../../stock/types/stock.types';
import type { SupplierDto } from '../../suppliers/types/supplier.types';
import type { SafeDto } from '../../accounting/types/accounting.types';
import type { ProductDto } from '../../catalog/types/catalog.types';
import { IconPlus, IconTrash, IconArrowBackUp } from '@tabler/icons-react';

interface PurchaseReturnModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: unknown) => Promise<void>;
}

interface ReturnItemState {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  notes?: string;
}

export const PurchaseReturnModal: React.FC<PurchaseReturnModalProps> = ({ isOpen, onClose, onSave }) => {
  const { currentBranchId } = useAuth();

  const [returnNumber, setReturnNumber] = useState('');
  const [returnDate, setReturnDate] = useState('');
  const [supplierId, setSupplierId] = useState('');
  const [warehouseId, setWarehouseId] = useState('');
  const [refundSafeId, setRefundSafeId] = useState('');
  const [paymentMethod, setPaymentMethod] = useState<number>(1); // 1=Cash
  const [reason, setReason] = useState<number>(1); // 1=Defective
  const [notes, setNotes] = useState('');

  const [items, setItems] = useState<ReturnItemState[]>([]);
  const [selectedProductId, setSelectedProductId] = useState('');

  // Dropdown lists
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [suppliers, setSuppliers] = useState<SupplierDto[]>([]);
  const [safes, setSafes] = useState<SafeDto[]>([]);
  const [products, setProducts] = useState<ProductDto[]>([]);

  const [isLoadingData, setIsLoadingData] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      const now = new Date();
      const dateStr = now.toISOString().slice(0, 10);
      const rand = Math.floor(100 + Math.random() * 900);
      setReturnNumber(`PRTN-${dateStr.replace(/-/g, '')}-${rand}`);
      setReturnDate(dateStr);
      setItems([]);
      setSelectedProductId('');
      setNotes('');
      setError(null);
      setPaymentMethod(1);
      setReason(1);

      setIsLoadingData(true);
      Promise.all([
        stockService.getWarehouses(),
        supplierService.getAll(),
        accountingService.getSafes(),
        catalogService.getProducts(),
      ])
        .then(([whs, supps, sfs, prods]) => {
          setWarehouses(whs.filter((w) => w.isActive));
          if (whs.length > 0) setWarehouseId(whs[0].id);

          setSuppliers(supps.filter((s) => s.isActive));
          if (supps.length > 0) setSupplierId(supps[0].id);

          setSafes(sfs.filter((s) => s.isActive));
          if (sfs.length > 0) setRefundSafeId(sfs[0].id);

          const productList = Array.isArray(prods) ? prods : prods?.items || [];
          setProducts(productList.filter((p) => p.isActive));
        })
        .catch((err) => {
          console.error('Failed to load purchase return modal dropdowns:', err);
          setError('فشل تحميل القوائم الأساسية لمرتجع المشتريات');
        })
        .finally(() => setIsLoadingData(false));
    }
  }, [isOpen]);

  const handleAddItem = () => {
    if (!selectedProductId) return;
    const prod = products.find((p) => p.id === selectedProductId);
    if (!prod) return;

    if (items.some((i) => i.productId === prod.id)) {
      setError('هذا الصنف مضاف بالفعل في قائمة المرتجع');
      return;
    }

    setItems((prev) => [
      ...prev,
      {
        productId: prod.id,
        productName: prod.name,
        quantity: 1,
        unitPrice: prod.costPrice || prod.salePrice || 0,
        notes: '',
      },
    ]);
    setSelectedProductId('');
    setError(null);
  };

  const handleRemoveItem = (idx: number) => {
    setItems((prev) => prev.filter((_, i) => i !== idx));
  };

  const handleItemChange = (idx: number, field: keyof ReturnItemState, val: unknown) => {
    setItems((prev) =>
      prev.map((item, i) => (i === idx ? { ...item, [field]: val } : item))
    );
  };

  const totalAmount = items.reduce((sum, item) => sum + item.quantity * item.unitPrice, 0);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!supplierId) {
      setError('يرجى تحديد المورد');
      return;
    }
    if (!warehouseId) {
      setError('يرجى تحديد المستودع المسحوبة منه البضاعة المرتجعة');
      return;
    }
    if (items.length === 0) {
      setError('يرجى إضافة صنف واحد على الأقل للمرتجع');
      return;
    }
    for (const item of items) {
      if (item.quantity <= 0) {
        setError(`الكمية للصنف ${item.productName} يجب أن تكون أكبر من صفر`);
        return;
      }
      if (item.unitPrice < 0) {
        setError(`سعر تكلفة الصنف ${item.productName} لا يمكن أن يكون سالباً`);
        return;
      }
    }

    setIsSubmitting(true);
    setError(null);
    try {
      const payload = {
        returnNumber,
        returnDate: new Date(returnDate).toISOString(),
        supplierId,
        branchId: currentBranchId || warehouses.find((w) => w.id === warehouseId)?.branchId,
        warehouseId,
        paymentMethod: Number(paymentMethod),
        refundSafeId: refundSafeId || undefined,
        reason: Number(reason),
        notes: notes.trim() || undefined,
        items: items.map((i) => ({
          productId: i.productId,
          quantity: Number(i.quantity),
          unitPrice: Number(i.unitPrice),
          notes: i.notes?.trim() || undefined,
        })),
      };

      await onSave(payload);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل حفظ مرتجع المشتريات';
      setError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إنشاء مرتجع مشتريات جديد"
      subtitle="تسجيل إرجاع بضاعة لمورد واسترداد قيمتها أو خصمها من رصيده"
      size="lg"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose} disabled={isSubmitting}>
            إلغاء
          </Button>
          <Button
            variant="primary"
            size="md"
            onClick={handleSubmit}
            isLoading={isSubmitting}
            disabled={isLoadingData || items.length === 0 || !supplierId}
          >
            اعتماد مرتجع المشتريات
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 py-1">
        {error && (
          <div className="p-3 bg-rose-50 text-rose-700 text-xs rounded-lg border border-rose-200">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <Input
            label="رقم إشعار المرتجع *"
            value={returnNumber}
            onChange={(e) => setReturnNumber(e.target.value)}
            isLtr
            required
          />

          <Input
            label="تاريخ الإرجاع *"
            type="date"
            value={returnDate}
            onChange={(e) => setReturnDate(e.target.value)}
            required
          />

          <Select
            label="المورد *"
            value={supplierId}
            onChange={(e) => setSupplierId(e.target.value)}
            options={suppliers.map((s) => ({ value: s.id, label: s.name }))}
            required
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <Select
            label="المستودع المسحوب منه *"
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            options={warehouses.map((w) => ({ value: w.id, label: w.name }))}
            required
          />

          <Select
            label="سبب الإرجاع *"
            value={reason}
            onChange={(e) => setReason(Number(e.target.value))}
            options={[
              { value: 1, label: 'بضاعة معيبة أو تالفة' },
              { value: 2, label: 'غير مطابق للمواصفات' },
              { value: 3, label: 'قريب أو منتهي الصلاحية' },
              { value: 4, label: 'فائض عن حاجة المخزون' },
              { value: 5, label: 'سبب آخر' },
            ]}
          />

          <Select
            label="طريقة استرداد القيمة *"
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(Number(e.target.value))}
            options={[
              { value: 1, label: 'نقداً (استلام إلى الخزينة)' },
              { value: 2, label: 'بطاقة مصرفية' },
              { value: 3, label: 'تحويل مصرفي' },
              { value: 4, label: 'آجل / تخفيض مديونية المورد' },
            ]}
          />
        </div>

        {paymentMethod !== 4 && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <Select
              label="الخزينة المستلمة للمبلغ *"
              value={refundSafeId}
              onChange={(e) => setRefundSafeId(e.target.value)}
              options={safes.map((s) => ({
                value: s.id,
                label: `${s.name} (الرصيد: ${s.currentBalance.toLocaleString('ar-LY')} د.ل)`,
              }))}
            />
            <Input
              label="ملاحظات إضافية"
              placeholder="سبب الإرجاع أو تفاصيل فاتورة الشراء الأصلية"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        )}

        {/* قسم إضافة الأصناف */}
        <div className="border border-slate-200 rounded-xl p-3 bg-slate-50/50 flex flex-col gap-3">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-800 flex items-center gap-1.5">
              <IconArrowBackUp className="w-4 h-4 text-amber-600" />
              أصناف المرتجع للمورد
            </span>
            <span className="text-xs text-slate-500">
              إجمالي القيمة المسترجعة:{' '}
              <span className="font-bold text-emerald-700 font-mono">
                {totalAmount.toLocaleString('ar-LY')} د.ل
              </span>
            </span>
          </div>

          <div className="flex gap-2">
            <div className="flex-1">
              <Select
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
                options={[
                  { value: '', label: '-- اختر الصنف لإضافته للمرتجع --' },
                  ...products.map((p) => ({
                    value: p.id,
                    label: `${p.name} (سعر التكلفة: ${p.costPrice || 0} د.ل)`,
                  })),
                ]}
              />
            </div>
            <Button
              type="button"
              variant="secondary"
              onClick={handleAddItem}
              disabled={!selectedProductId}
            >
              <IconPlus className="w-4 h-4" />
              إضافة الصنف
            </Button>
          </div>

          {items.length > 0 && (
            <div className="overflow-x-auto border border-slate-200 rounded-lg bg-white">
              <table className="w-full text-xs text-right">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-600">
                  <tr>
                    <th className="p-2.5">الصنف</th>
                    <th className="p-2.5 w-24">الكمية</th>
                    <th className="p-2.5 w-28">سعر التكلفة (د.ل)</th>
                    <th className="p-2.5 w-28">الإجمالي</th>
                    <th className="p-2.5">ملاحظة</th>
                    <th className="p-2.5 w-12 text-center">حذف</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {items.map((item, idx) => (
                    <tr key={item.productId} className="hover:bg-slate-50/50">
                      <td className="p-2.5 font-semibold text-slate-800">{item.productName}</td>
                      <td className="p-2">
                        <Input
                          type="number"
                          min="1"
                          value={item.quantity}
                          onChange={(e) =>
                            handleItemChange(idx, 'quantity', Math.max(1, Number(e.target.value)))
                          }
                          className="h-8 py-1 text-center"
                        />
                      </td>
                      <td className="p-2">
                        <Input
                          type="number"
                          step="0.01"
                          min="0"
                          value={item.unitPrice}
                          onChange={(e) =>
                            handleItemChange(idx, 'unitPrice', Math.max(0, Number(e.target.value)))
                          }
                          className="h-8 py-1 text-center font-mono"
                        />
                      </td>
                      <td className="p-2.5 font-mono font-bold text-slate-700">
                        {(item.quantity * item.unitPrice).toLocaleString('ar-LY')} د.ل
                      </td>
                      <td className="p-2">
                        <Input
                          placeholder="ملاحظة..."
                          value={item.notes || ''}
                          onChange={(e) => handleItemChange(idx, 'notes', e.target.value)}
                          className="h-8 py-1"
                        />
                      </td>
                      <td className="p-2 text-center">
                        <button
                          type="button"
                          onClick={() => handleRemoveItem(idx)}
                          className="p-1 rounded text-rose-500 hover:bg-rose-50 transition-colors"
                          title="حذف البند"
                        >
                          <IconTrash className="w-4 h-4" />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </form>
    </Modal>
  );
};

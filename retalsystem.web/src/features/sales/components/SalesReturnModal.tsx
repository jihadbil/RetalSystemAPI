import { lookupService, type LookupOption, type ReturnSourceInvoice } from '../../../shared/services/lookupService';
import { createDocumentNumber } from '../../../shared/utils/documentNumber';
import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { stockService } from '../../stock/services/stockService';
import { salesReturnLines } from '../../../shared/utils/returnLines';
import { useDebounce } from '../../../shared/hooks/useDebounce';
import { useProductBarcodes } from '../../catalog/hooks/useProductBarcodes';
import { useAuth } from '../../../shared/context/AuthContext';
import type { WarehouseDto } from '../../stock/types/stock.types';
import type { ProductDto } from '../../catalog/types/catalog.types';
import { IconPlus, IconTrash, IconReceiptRefund } from '@tabler/icons-react';

interface SalesReturnModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: unknown) => Promise<void>;
}

interface ReturnItemState {
  productBarCodeId?: string;
  maxQuantity?: number;
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  notes?: string;
}

export const SalesReturnModal: React.FC<SalesReturnModalProps> = ({ isOpen, onClose, onSave }) => {
  const { currentBranchId, hasPermission } = useAuth();

  const [invoiceId, setInvoiceId] = useState('');
  const [invoices, setInvoices] = useState<ReturnSourceInvoice[]>([]);
  const [originalInvoice, setOriginalInvoice] = useState<ReturnSourceInvoice | null>(null);
  const [invoiceSearch, setInvoiceSearch] = useState('');
  const debouncedInvoiceSearch = useDebounce(invoiceSearch, 300);
  const [loadingInvoice, setLoadingInvoice] = useState(false);
  const canReturnWithoutInvoice = hasPermission('Permissions.SalesReturns.ReturnWithoutInvoice');
  const invoiceLines = originalInvoice ? salesReturnLines(originalInvoice) : [];
  const [returnNumber, setReturnNumber] = useState('');
  const [returnDate, setReturnDate] = useState('');
  const [warehouseId, setWarehouseId] = useState('');
  const [customerId, setCustomerId] = useState('');
  const [refundSafeId, setRefundSafeId] = useState('');
  const [paymentMethod, setPaymentMethod] = useState<number>(1); // 1=Cash
  const [reason, setReason] = useState<number>(1); // 1=Defective
  const [notes, setNotes] = useState('');

  const [items, setItems] = useState<ReturnItemState[]>([]);
  const [selectedProductId, setSelectedProductId] = useState('');

  // Dropdown lists
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [customers, setCustomers] = useState<LookupOption[]>([]);
  const [safes, setSafes] = useState<LookupOption[]>([]);
  const [products, setProducts] = useState<ProductDto[]>([]);

  const [isLoadingData, setIsLoadingData] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { product: selectedProduct, barcodeId, setBarcodeId, error: barcodeError } = useProductBarcodes(invoiceId ? '' : selectedProductId, isOpen);
  const needsBarcode = warehouses.find((w) => w.id === warehouseId)?.type === 1;
  useEffect(() => {
    if (!isOpen) return;
    let cancelled = false;
    void lookupService.page<ReturnSourceInvoice>('sales-return-invoices', { pageNumber: 1, pageSize: 100, branchId: currentBranchId || undefined, search: debouncedInvoiceSearch || undefined })
      .then((result) => { if (!cancelled) setInvoices(result.items.filter((invoice) => [1, 2, 3].includes(invoice.status))); })
      .catch(() => { if (!cancelled) setError('تعذر تحميل الفواتير الأصلية'); });
    return () => { cancelled = true; };
  }, [isOpen, currentBranchId, debouncedInvoiceSearch]);
  useEffect(() => {
    setItems([]); setSelectedProductId(''); setOriginalInvoice(null);
    if (!isOpen || !invoiceId) { setLoadingInvoice(false); return; }
    let cancelled = false;
    setLoadingInvoice(true);
    void lookupService.page<ReturnSourceInvoice>('sales-return-invoices', { id: invoiceId, pageSize: 1 }).then((result) => {
      if (!result.items[0]) throw new Error('الفاتورة غير متاحة للإرجاع');
      return result.items[0];
    }).then(async (invoice) => {
      const available = await stockService.getWarehouses(invoice.branchId, invoice.warehouseId);
      if (!available.some((warehouse) => warehouse.id === invoice.warehouseId)) throw new Error('مستودع الفاتورة الأصلية غير متاح للإرجاع');
      if (cancelled) return;
      if (![1, 2, 3].includes(invoice.status)) throw new Error('الفاتورة غير مرحلة أو ملغاة');
      setWarehouses(available);
      setOriginalInvoice(invoice); setWarehouseId(invoice.warehouseId || ''); setCustomerId(invoice.customerId || '');
      if (invoice.hasSessionPayments) setPaymentMethod(1);
    }).catch((err: unknown) => {
      if (!cancelled) setError(err instanceof Error ? err.message : 'تعذر تحميل الفاتورة');
    }).finally(() => { if (!cancelled) setLoadingInvoice(false); });
    return () => { cancelled = true; };
  }, [isOpen, invoiceId]);

  useEffect(() => {
    let cancelled = false;
    if (isOpen) {
      const now = new Date();
      const dateStr = now.toISOString().slice(0, 10);
      setReturnNumber(createDocumentNumber('SRTN'));
      setReturnDate(dateStr);
      setInvoiceId(''); setOriginalInvoice(null); setInvoiceSearch('');
      setItems([]);
      setSelectedProductId('');
      setNotes('');
      setError(null);
      setPaymentMethod(1);
      setReason(1);

      setIsLoadingData(true);
      Promise.all([
        stockService.getWarehouses(currentBranchId || undefined),
        lookupService.list('customers'),
        lookupService.list('safes', currentBranchId || undefined),
        lookupService.page<ProductDto>('products'),
      ])
        .then(([whs, custs, sfs, prods]) => {
          if (cancelled) return;
          const availableWarehouses = whs.filter((w) => w.isActive);
          setWarehouses(availableWarehouses); setWarehouseId(availableWarehouses[0]?.id || '');

          setCustomers(custs.filter((c) => c.isActive));
          if (custs.length > 0) setCustomerId(custs[0].id);

          setSafes(sfs.filter((s) => s.isActive));
          if (sfs.length > 0) setRefundSafeId(sfs[0].id);

          const productList = Array.isArray(prods) ? prods : prods?.items || [];
          setProducts(productList.filter((p) => p.isActive));
        })
        .catch((err) => {
          if (cancelled) return;
          console.error('Failed to load return modal dropdowns:', err);
          setError('فشل تحميل القوائم الأساسية لمرتجع المبيعات');
        })
        .finally(() => { if (!cancelled) setIsLoadingData(false); });
    }
    return () => { cancelled = true; };
  }, [isOpen, currentBranchId]);

  const handleAddItem = () => {
    if (!selectedProductId) return;
    if (invoiceId) {
      const line = invoiceLines.find((line) => line.key === selectedProductId);
      if (!line || items.some((item) => item.productId === line.productId && item.productBarCodeId === line.productBarCodeId)) return;
      setItems((previous) => [...previous, { ...line, maxQuantity: line.quantity, quantity: 1 }]);
      setSelectedProductId(''); setError(null); return;
    }
    if (!canReturnWithoutInvoice) { setError('اختر الفاتورة الأصلية'); return; }
    if (needsBarcode && !barcodeId) { setError('اختر باركود الصنف'); return; }
    const prod = selectedProduct;
    if (!prod) return;

    if (items.some((i) => i.productId === prod.id && i.productBarCodeId === (needsBarcode ? barcodeId : undefined))) {
      setError('هذا الصنف مضاف بالفعل في قائمة المرتجع');
      return;
    }

    setItems((prev) => [
      ...prev,
      {
        productId: prod.id,
        productBarCodeId: needsBarcode ? barcodeId : undefined,
        productName: prod.name,
        quantity: 1,
        unitPrice: prod.salePrice || 0,
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
    if (loadingInvoice || invoiceId && !originalInvoice || !invoiceId && !canReturnWithoutInvoice) { setError('اختر فاتورة أصلية صالحة وانتظر تحميل بنودها'); return; }
    if (!warehouseId) {
      setError('يرجى تحديد المستودع المستلم للبضاعة المرتجعة');
      return;
    }
    if (items.length === 0) {
      setError('يرجى إضافة صنف واحد على الأقل للمرتجع');
      return;
    }
    for (const item of items) {
      if (!Number.isInteger(item.quantity) || item.quantity <= 0 || item.maxQuantity !== undefined && item.quantity > item.maxQuantity) {
        setError(`الكمية للصنف ${item.productName} يجب أن تكون أكبر من صفر`);
        return;
      }
      if (item.unitPrice < 0) {
        setError(`سعر الصنف ${item.productName} لا يمكن أن يكون سالباً`);
        return;
      }
    }

    setIsSubmitting(true);
    setError(null);
    try {
      const payload = {
        originalInvoiceId: invoiceId || undefined,
        returnNumber,
        returnDate: new Date(returnDate).toISOString(),
        branchId: originalInvoice?.branchId || currentBranchId || warehouses.find((w) => w.id === warehouseId)?.branchId,
        warehouseId,
        customerId: customerId || undefined,
        refundedAmount: paymentMethod === 4 ? 0 : originalInvoice ? undefined : totalAmount,
        paymentMethod: Number(paymentMethod),
        refundSafeId: refundSafeId || undefined,
        reason: Number(reason),
        notes: notes.trim() || undefined,
        items: items.map((i) => ({
          productId: i.productId,
          productBarCodeId: i.productBarCodeId,
          quantity: Number(i.quantity),
          unitPrice: Number(i.unitPrice),
          notes: i.notes?.trim() || undefined,
        })),
      };

      await onSave(payload);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل حفظ مرتجع المبيعات';
      setError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إنشاء مرتجع مبيعات جديد"
      subtitle="تسجيل استرجاع بضاعة من عميل وإعادة قيمتها أو قيدها في حسابه"
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
            disabled={isLoadingData || loadingInvoice || !!invoiceId && !originalInvoice || !invoiceId && !canReturnWithoutInvoice || items.length === 0}
          >
            اعتماد مرتجع المبيعات
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

        <Input label="البحث عن الفاتورة الأصلية" value={invoiceSearch} onChange={(e) => setInvoiceSearch(e.target.value)} placeholder="رقم الفاتورة..." />
        <Select label="الفاتورة الأصلية" value={invoiceId} onChange={(e) => { setInvoiceId(e.target.value); setError(null); }} options={[
          { value: '', label: canReturnWithoutInvoice ? 'دون فاتورة أصلية' : '-- اختر الفاتورة الأصلية --' },
          ...(originalInvoice && !invoices.some((invoice) => invoice.id === originalInvoice.id) ? [{ value: originalInvoice.id, label: originalInvoice.invoiceNumber }] : []),
          ...invoices.map((invoice) => ({ value: invoice.id, label: `${invoice.invoiceNumber} — ${invoice.customerName || ''}` })),
        ]} />
        {originalInvoice && <p className="text-xs text-slate-500">تُحتسب القيمة النهائية والكميات المتبقية بعد المرتجعات السابقة في الخادم وفق الفاتورة الأصلية.</p>}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <Input
            label="رقم المرتجع *"
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
            lookup={{ resource: 'warehouses', branchId: currentBranchId || undefined }} onLookupSelect={(option) => setWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="المستودع / الصالة المستلمة *"
            disabled={!!invoiceId}
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            options={warehouses.map((w) => ({ value: w.id, label: w.name }))}
            required
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
          <Select
            lookup={originalInvoice ? undefined : { resource: 'customers' }} label="العميل"
            disabled={!!invoiceId}
            value={customerId}
            onChange={(e) => setCustomerId(e.target.value)}
            options={originalInvoice ? [{ value: originalInvoice.customerId || '', label: originalInvoice.customerName || 'عميل نقدي' }] : [
              { value: '', label: 'عميل عام / نقدي' },
              ...customers.map((c) => ({ value: c.id, label: c.name })),
            ]}
          />

          <Select
            label="سبب الإرجاع *"
            value={reason}
            onChange={(e) => setReason(Number(e.target.value))}
            options={[
              { value: 1, label: 'بضاعة معيبة أو تالفة' },
              { value: 2, label: 'صنف خاطئ' },
              { value: 3, label: 'تغيير رأي العميل' },
              { value: 4, label: 'منتهي الصلاحية' },
              { value: 5, label: 'سبب آخر' },
            ]}
          />

          <Select
            label="طريقة رد المبلغ *"
            value={paymentMethod}
            onChange={(e) => setPaymentMethod(Number(e.target.value))}
            options={[
              { value: 1, label: originalInvoice?.hasSessionPayments ? 'الرد بالوسائل الأصلية في الوردية' : 'نقداً (من الخزينة)' },
              ...(!originalInvoice?.hasSessionPayments ? [{ value: 3, label: 'بطاقة مصرفية' }, { value: 2, label: 'تحويل مصرفي' }] : []),
              { value: 4, label: 'آجل / تخفيض رصيد العميل' },
            ]}
          />
        </div>

        {originalInvoice?.hasSessionPayments && <p className="text-sm text-amber-800">يُوزع الرد على وسائل القبض الأصلية، ويتطلب وردية مفتوحة للدرج الأصلي. يدخل الرد في التسوية الاختيارية للوردية.</p>}
        {paymentMethod !== 4 && !originalInvoice?.hasSessionPayments && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <Select
              lookup={{ resource: 'safes', branchId: currentBranchId || undefined }} label="الخزينة المنصرف منها المبلغ *"
              value={refundSafeId}
              onChange={(e) => setRefundSafeId(e.target.value)}
              options={safes.map((s) => ({
                value: s.id,
                label: s.name,
              }))}
            />
            <Input
              label="ملاحظات إضافية"
              placeholder="سبب الإرجاع أو تفاصيل الفاتورة"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        )}

        {/* قسم إضافة الأصناف */}
        <div className="border border-slate-200 rounded-xl p-3 bg-slate-50/50 flex flex-col gap-3">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-800 flex items-center gap-1.5">
              <IconReceiptRefund className="w-4 h-4 text-emerald-600" />
              أصناف المرتجع
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
              <Select lookup={invoiceId ? undefined : { resource: 'products' }} label="الصنف"
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
                options={[
                  { value: '', label: '-- اختر الصنف لإضافته للمرتجع --' },
                  ...(originalInvoice ? invoiceLines.map((line) => ({ value: line.key, label: `${line.productName} — ${line.productBarCodeId || ''} (الكمية الأصلية: ${line.quantity})` })) : products.map((p) => ({ value: p.id, label: p.name }))),
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

          {!invoiceId && needsBarcode && selectedProduct && <Select label="باركود الصنف" value={barcodeId} onChange={(e) => setBarcodeId(e.target.value)} options={[
            { value: '', label: '-- اختر الباركود --' },
            ...(selectedProduct.barCodes || []).map((barcode) => ({ value: barcode.id || "", label: `${barcode.title || ''} ${barcode.barCode}` })),
          ]} />}
          {barcodeError && <p className="text-xs text-rose-600">{barcodeError}</p>}
          {items.length > 0 && (
            <div className="overflow-x-auto border border-slate-200 rounded-lg bg-white">
              <table className="w-full text-xs text-right">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-600">
                  <tr>
                    <th className="p-2.5">الصنف</th>
                    <th className="p-2.5 w-24">الكمية</th>
                    <th className="p-2.5 w-28">السعر (د.ل)</th>
                    <th className="p-2.5 w-28">الإجمالي</th>
                    <th className="p-2.5">ملاحظة</th>
                    <th className="p-2.5 w-12 text-center">حذف</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {items.map((item, idx) => (
                    <tr key={`${item.productId}:${item.productBarCodeId || ""}`} className="hover:bg-slate-50/50">
                      <td className="p-2.5 font-semibold text-slate-800">{item.productName}</td>
                      <td className="p-2">
                        <Input
                          type="number"
                          min="1"
                          step="1"
                          max={item.maxQuantity}
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
                          readOnly
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

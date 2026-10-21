import { lookupService, type LookupOption } from '../../../shared/services/lookupService';
import { createDocumentNumber } from '../../../shared/utils/documentNumber';
import React, { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { stockService } from '../../stock/services/stockService';
import { useAuth } from '../../../shared/context/AuthContext';
import type { WarehouseDto } from '../../stock/types/stock.types';
import type { ProductDto } from '../../catalog/types/catalog.types';
import {
  InvoiceStatus,
  SalesPaymentMethod,
  type CreateSalesInvoiceItemRequest,
  type CreateSalesInvoiceRequest,
} from '../types/sales.types';
import {
  IconReceipt,
  IconSearch,
  IconPlus,
  IconTrash,
  IconCheck,
  IconAlertCircle,
} from '@tabler/icons-react';

interface SalesInvoiceModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreateSalesInvoiceRequest) => Promise<void>;
}

export const SalesInvoiceModal: React.FC<SalesInvoiceModalProps> = ({
  isOpen,
  onClose,
  onSave,
}) => {
  const { currentBranchId } = useAuth();

  // ════════ Master Data Lists ════════
  const [branches, setBranches] = useState<LookupOption[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [customers, setCustomers] = useState<LookupOption[]>([]);
  const [safes, setSafes] = useState<LookupOption[]>([]);
  const [products, setProducts] = useState<ProductDto[]>([]);

  // ════════ Invoice Main Fields ════════
  const [invoiceNumber, setInvoiceNumber] = useState('');
  const [invoiceDate, setInvoiceDate] = useState('');
  const [branchId, setBranchId] = useState('');
  const [warehouseId, setWarehouseId] = useState('');
  const [customerId, setCustomerId] = useState<string>('');
  const [paymentMethod, setPaymentMethod] = useState<number>(SalesPaymentMethod.Cash);
  const [paymentSafeId, setPaymentSafeId] = useState<string>('');
  const [notes, setNotes] = useState('');

  // ════════ Items Table State ════════
  const [items, setItems] = useState<CreateSalesInvoiceItemRequest[]>([]);
  const [discountAmount, setDiscountAmount] = useState<number>(0);
  const [paidAmount, setPaidAmount] = useState<number>(0);
  const [isPaidManuallyModified, setIsPaidManuallyModified] = useState(false);

  // ════════ Item Entry State ════════
  const [searchBarcode, setSearchBarcode] = useState('');
  const [isSearchingBarcode, setIsSearchingBarcode] = useState(false);
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedProduct, setSelectedProduct] = useState<ProductDto | null>(null);
  const [selectedBarcodeId, setSelectedBarcodeId] = useState<string | undefined>();
  const [quantityToAdd, setQuantityToAdd] = useState<number>(1);
  const [unitPriceToAdd, setUnitPriceToAdd] = useState<number>(0);
  const [discountToAdd, setDiscountToAdd] = useState<number>(0);
  const [searchErrorMessage, setSearchErrorMessage] = useState<string | null>(null);

  // ════════ Status & Feedback ════════
  const [, setIsLoadingData] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // ── 1. Initialize Master Data on Open ──
  useEffect(() => {
    let cancelled = false;
    if (!isOpen) return;

    const now = new Date();
    const dateFormatted = now.toISOString().slice(0, 10);

    setInvoiceNumber(createDocumentNumber('INV'));
    setInvoiceDate(dateFormatted);
    setPaymentMethod(SalesPaymentMethod.Cash);
    setCustomerId('');
    setNotes('');
    setItems([]);
    setDiscountAmount(0);
    setPaidAmount(0);
    setIsPaidManuallyModified(false);
    resetItemEntry();
    setFormError(null);

    setIsLoadingData(true);
    Promise.all([
      lookupService.list('branches').catch(() => []),
      stockService.getWarehouses().catch(() => []),
      lookupService.list('customers').catch(() => []),
      lookupService.list('safes').catch(() => []),
      lookupService.page<ProductDto>('products', { pageNumber: 1, pageSize: 200 }).catch(() => ({ items: [] })),
    ])
      .then(([bList, wList, cList, sfList, pResult]) => {
          if (cancelled) return;
        setBranches(bList);
        const activeWarehouses = wList.filter((w) => w.isActive);
        setWarehouses(activeWarehouses);
        const activeCustomers = cList.filter((c) => c.isActive);
        setCustomers(activeCustomers);
        const productList = pResult.items || [];
        setProducts(productList);

        let initialBranchId = currentBranchId || (bList.length > 0 ? bList[0].id : '');
        if (!initialBranchId && bList.length > 0) initialBranchId = bList[0].id;
        setBranchId(initialBranchId);

        setWarehouseId(activeWarehouses.find((w) => w.branchId === initialBranchId)?.id || '');

        const branchSafes = sfList.filter((s) => s.isActive && (!initialBranchId || s.branchId === initialBranchId));
        setSafes(branchSafes);
        if (branchSafes.length > 0) setPaymentSafeId(branchSafes[0].id);
      })
      .catch((err) => {
          if (cancelled) return;
        console.error('Failed to load sales invoice dropdowns:', err);
        setFormError('تعذر تحميل بيانات الفروع أو المستودعات أو العملاء');
      })
      .finally(() => { if (!cancelled) setIsLoadingData(false); });
    return () => { cancelled = true; };
  }, [isOpen, currentBranchId]);

  const branchWarehouses = warehouses.filter((warehouse) => warehouse.branchId === branchId);
  useEffect(() => {
    if (!isOpen || !warehouses.length) return;
    const available = warehouses.filter((warehouse) => warehouse.branchId === branchId);
    setWarehouseId((previous) => available.some((warehouse) => warehouse.id === previous) ? previous : available[0]?.id || '');
  }, [isOpen, branchId, warehouses]);

  // ── 2. Reload Safes on Branch Change ──
  useEffect(() => {
    let cancelled = false;
    if (!branchId) return;
    lookupService
      .list('safes', branchId)
      .then((sfList) => {
          if (cancelled) return;
        setSafes(sfList);
        if (sfList.length > 0 && (!paymentSafeId || !sfList.some((s) => s.id === paymentSafeId))) {
          setPaymentSafeId(sfList[0].id);
        }
      })
      .catch(() => {
          if (cancelled) return;});
    return () => { cancelled = true; };
  }, [branchId, paymentSafeId]);

  // ── 3. Reset Item Entry ──
  function resetItemEntry() {
    selectionVersion.current++;
    setIsSearchingBarcode(false);
    setSelectedBarcodeId(undefined);
    setSelectedProduct(null);
    setSelectedProductId('');
    setSearchBarcode('');
    setQuantityToAdd(1);
    setUnitPriceToAdd(0);
    setDiscountToAdd(0);
    setSearchErrorMessage(null);
  }

  // ── 4. On Product Selected ──
  const selectionVersion = useRef(0);
  const invalidateSelection = useCallback(() => { selectionVersion.current++; }, []);
  useEffect(() => invalidateSelection, [isOpen, invalidateSelection]);
  const handleSelectProduct = useCallback(async (summary: ProductDto, scannedBarcode?: string) => {
    const version = ++selectionVersion.current;
    setIsSearchingBarcode(false);
    setSelectedProduct(null);
    setSelectedBarcodeId(undefined);
    setSelectedProductId(summary.id);
    try {
      const product = summary.barCodes?.length ? summary : await lookupService.product(summary.id);
      if (version !== selectionVersion.current) return;
      const barcode = scannedBarcode
        ? product.barCodes?.find((b) => b.barCode.toLowerCase() === scannedBarcode.toLowerCase())
        : product.barCodes?.[0];
      setSelectedProduct(product);
      setSelectedBarcodeId(barcode?.id);
      setUnitPriceToAdd(Number(product.salePrice) || 0);
      setSearchBarcode(barcode?.barCode || scannedBarcode || product.code || '');
      setSearchErrorMessage(null);
    } catch (err) {
      if (version === selectionVersion.current)
        setSearchErrorMessage(err instanceof Error ? err.message : 'تعذر تحميل تفاصيل الصنف وباركوداته');
    }
  }, []);

  // ── 5. Lookup Barcode ──
  const handleLookupBarcode = async () => {
    if (!searchBarcode.trim()) return;
    const version = ++selectionVersion.current;
    setIsSearchingBarcode(true);
    setSearchErrorMessage(null);

    try {
      const trimmed = searchBarcode.trim();
      const product = await lookupService.barcode(trimmed);
      if (version !== selectionVersion.current) return;
      if (product && product.id) {
        await handleSelectProduct(product, trimmed);
      } else {
        // Fallback: check in loaded products list
        const local = products.find(
          (p) =>
            p.code.toLowerCase() === trimmed.toLowerCase() ||
            p.barCodes?.some((b) => b.barCode.toLowerCase() === trimmed.toLowerCase())
        );
        if (local) {
          await handleSelectProduct(local, trimmed);
        } else {
          setSearchErrorMessage(`لم يتم العثور على صنف بالباركود: ${trimmed}`);
        }
      }
    } catch {
      if (version !== selectionVersion.current) return;
      const local = products.find(
        (p) =>
          p.code.toLowerCase() === searchBarcode.trim().toLowerCase() ||
          p.barCodes?.some((b) => b.barCode.toLowerCase() === searchBarcode.trim().toLowerCase())
      );
      if (local) {
        await handleSelectProduct(local, searchBarcode.trim());
      } else {
        setSearchErrorMessage(`لم يتم العثور على صنف بالباركود: ${searchBarcode.trim()}`);
      }
    } finally {
      if (version === selectionVersion.current) setIsSearchingBarcode(false);
    }
  };

  // ── 6. Add Item to Invoice ──
  const handleAddItem = () => {
    if (!selectedProduct) {
      setFormError('يرجى اختيار الصنف أولاً');
      return;
    }
    if (quantityToAdd <= 0) {
      setFormError('الكمية يجب أن تكون أكبر من صفر');
      return;
    }
    if (unitPriceToAdd < 0) {
      setFormError('سعر الوحدة لا يمكن أن يكون سالباً');
      return;
    }

    setFormError(null);
    const lineTotal = Math.max(0, quantityToAdd * unitPriceToAdd - discountToAdd);

    const newItem: CreateSalesInvoiceItemRequest = {
      productId: selectedProduct.id,
      productName: selectedProduct.name,
      productCode: selectedProduct.code,
      productBarCodeId: selectedBarcodeId,
      barCode: searchBarcode,
      quantity: quantityToAdd,
      unitPrice: unitPriceToAdd,
      unitCost: selectedProduct.costPrice,
      discountAmount: discountToAdd,
      lineTotal,
    };

    setItems((prev) => [...prev, newItem]);
    resetItemEntry();
  };

  // ── 7. Remove Item ──
  const handleRemoveItem = (index: number) => {
    setItems((prev) => prev.filter((_, idx) => idx !== index));
  };

  // ── 8. Financial Totals ──
  const subTotal = useMemo(() => {
    return items.reduce((sum, item) => sum + item.lineTotal, 0);
  }, [items]);

  const totalAmount = useMemo(() => {
    return Math.max(0, subTotal - discountAmount);
  }, [subTotal, discountAmount]);

  useEffect(() => {
    if (!isPaidManuallyModified) {
      if (paymentMethod === SalesPaymentMethod.Cash || paymentMethod === SalesPaymentMethod.CreditCard) {
        setPaidAmount(totalAmount);
      } else if (paymentMethod === SalesPaymentMethod.Credit) {
        setPaidAmount(0);
      }
    }
  }, [totalAmount, paymentMethod, isPaidManuallyModified]);

  const remainingAmount = Math.max(0, totalAmount - paidAmount);

  // ── 9. Submit Invoice ──
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);

    if (!invoiceNumber.trim()) {
      setFormError('رقم الفاتورة مطلوب');
      return;
    }
    if (!branchId) {
      setFormError('يرجى تحديد الفرع');
      return;
    }
    if (!warehouseId) {
      setFormError('يرجى اختيار صالة العرض أو المستودع المسحوب منه');
      return;
    }
    if (items.length === 0) {
      setFormError('يجب إضافة بند واحد على الأقل داخل الفاتورة');
      return;
    }

    setIsSubmitting(true);
    try {
      const payload: CreateSalesInvoiceRequest = {
        invoiceNumber: invoiceNumber.trim(),
        invoiceDate: new Date(invoiceDate).toISOString(),
        branchId,
        warehouseId,
        customerId: customerId || undefined,
        status: InvoiceStatus.Paid,
        paymentMethod,
        subTotal,
        discountAmount,
        taxAmount: 0,
        totalAmount,
        paidAmount,
        paymentSafeId: paymentSafeId || undefined,
        notes: notes.trim() || undefined,
        items: items.map((i) => ({
          productId: i.productId,
          productBarCodeId: i.productBarCodeId,
          quantity: i.quantity,
          unitPrice: i.unitPrice,
          unitCost: i.unitCost || 0,
          discountAmount: i.discountAmount,
        })),
      };

      await onSave(payload);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل إصدار وحفظ فاتورة المبيعات';
      setFormError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إصدار فاتورة مبيعات جديدة"
      subtitle="إصدار فواتير بيع معتمدة نقدية أو آجلة وتحديث أرصدة المخزون وحسابات العملاء"
      size="4xl"
      footer={
        <div className="flex flex-col sm:flex-row items-center justify-between gap-3 w-full">
          <div className="text-xs font-semibold text-rose-600">
            {formError && (
              <div className="flex items-center gap-1.5 bg-rose-50 px-3 py-1.5 rounded-lg border border-rose-200">
                <IconAlertCircle className="w-4 h-4 shrink-0" />
                <span>{formError}</span>
              </div>
            )}
          </div>
          <div className="flex items-center gap-2">
            <Button variant="secondary" size="md" onClick={onClose} disabled={isSubmitting}>
              إلغاء النافذة
            </Button>
            <Button
              variant="primary"
              size="md"
              leftIcon={<IconCheck className="w-4 h-4" />}
              onClick={handleSubmit}
              isLoading={isSubmitting}
              disabled={items.length === 0}
            >
              إصدار واعتماد الفاتورة
            </Button>
          </div>
        </div>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 py-1">
        {/* ════════ Top Status Banner ════════ */}
        <div className="flex flex-wrap items-center justify-between gap-3 px-3 py-2 bg-slate-50 border border-slate-200 rounded-xl">
          <div className="flex items-center gap-2">
            <span className="text-xs font-bold text-slate-500">حالة الفاتورة:</span>
            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800 border border-emerald-200">
              <span className="w-1.5 h-1.5 rounded-full bg-emerald-600"></span>
              فاتورة مبيعات معتمدة
            </span>
          </div>
          <div className="flex items-center gap-3 text-xs">
            <span className="text-slate-500">
              رقم الفاتورة: <strong className="font-mono text-slate-900">{invoiceNumber}</strong>
            </span>
            <span className="text-slate-300">|</span>
            <span className="text-slate-500">
              عدد البنود: <strong className="text-emerald-700">{items.length}</strong>
            </span>
          </div>
        </div>

        {/* ════════ 1. MAIN INVOICE FIELDS ════════ */}
        <div className="p-3.5 bg-white border border-slate-200 rounded-xl shadow-xs">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
            <Input
              label="رقم الفاتورة *"
              value={invoiceNumber}
              onChange={(e) => setInvoiceNumber(e.target.value)}
              isLtr
              required
            />

            <Select
              lookup={{ resource: 'branches' }} label="الفرع *"
              value={branchId}
              onChange={(e) => setBranchId(e.target.value)}
              required
            >
              {branches.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.name}
                </option>
              ))}
            </Select>

            <Select
              lookup={{ resource: 'warehouses', branchId: branchId }} onLookupSelect={(option) => setWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="صالة العرض / المستودع *"
              value={warehouseId}
              onChange={(e) => setWarehouseId(e.target.value)}
              required
            >
              {branchWarehouses.map((w) => (
                <option key={w.id} value={w.id}>
                  {w.name}
                </option>
              ))}
            </Select>

            <Select
              lookup={{ resource: 'customers' }} label="العميل"
              value={customerId}
              onChange={(e) => setCustomerId(e.target.value)}
            >
              <option value="">(عميل نقدي عام)</option>
              {customers.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </Select>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 mt-3 pt-3 border-t border-slate-100">
            <Select
              label="طريقة الدفع"
              value={paymentMethod}
              onChange={(e) => {
                const val = Number(e.target.value);
                setPaymentMethod(val);
                setIsPaidManuallyModified(false);
              }}
            >
              <option value={SalesPaymentMethod.Cash}>💵 نقداً</option>
              <option value={SalesPaymentMethod.CreditCard}>💳 بطاقة مصرفية</option>
              <option value={SalesPaymentMethod.BankTransfer}>🏦 تحويل مصرفي</option>
              <option value={SalesPaymentMethod.Credit}>📑 آجل / دين على حساب العميل</option>
            </Select>

            <Input
              label="تاريخ الفاتورة"
              type="date"
              value={invoiceDate}
              onChange={(e) => setInvoiceDate(e.target.value)}
            />

            <Select
              lookup={{ resource: 'safes', branchId }} label="الخزينة / الحساب للتحصيل"
              value={paymentSafeId}
              onChange={(e) => setPaymentSafeId(e.target.value)}
              disabled={paymentMethod === SalesPaymentMethod.Credit}
            >
              {safes.map((sf) => (
                <option key={sf.id} value={sf.id}>
                  {sf.name}
                </option>
              ))}
            </Select>

            <Input
              label="ملاحظات الفاتورة"
              placeholder="أي تفاصيل خاصة بالفاتورة..."
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </div>
        </div>

        {/* ════════ 2. ITEM SELECTION & ADDITION PANEL ════════ */}
        <div className="p-3.5 bg-gradient-to-b from-sky-50/40 to-slate-50/60 border border-sky-300 rounded-xl flex flex-col gap-3 shadow-xs">
          <div className="text-xs font-bold text-sky-950 flex items-center gap-1.5">
            <IconReceipt className="w-4 h-4 text-sky-700" />
            <span>إضافة بنود الأصناف للفاتورة</span>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-12 gap-2.5 items-end">
            {/* Barcode Search */}
            <div className="sm:col-span-3">
              <label className="block text-[11px] font-semibold text-slate-700 mb-1">
                مسح الباركود (Enter للبحث)
              </label>
              <div className="flex gap-1">
                <Input
                  placeholder="امسح الباركود..."
                  value={searchBarcode}
                  onChange={(e) => setSearchBarcode(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault();
                      handleLookupBarcode();
                    }
                  }}
                  className="h-9 font-mono"
                  isLtr
                />
                <Button
                  type="button"
                  variant="secondary"
                  size="sm"
                  onClick={handleLookupBarcode}
                  isLoading={isSearchingBarcode}
                  className="px-2.5"
                >
                  <IconSearch className="w-4 h-4 text-sky-700" />
                </Button>
              </div>
            </div>

            {/* Product Selector Dropdown */}
            <div className="sm:col-span-4">
              <label className="block text-[11px] font-semibold text-slate-700 mb-1">الصنف *</label>
              <Select lookup={{ resource: 'products' }} label="الصنف"
                value={selectedProductId}
                onChange={(e) => {
                  const id = e.target.value;
                  if (id) void handleSelectProduct({ id } as ProductDto);
                  else resetItemEntry();
                }}
                className="h-9"
              >
                <option value="">(اختر الصنف من القائمة...)</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.name} ({p.salePrice.toFixed(2)} د.ل)
                  </option>
                ))}
              </Select>
            </div>

            {/* Quantity */}
            <div className="sm:col-span-1">
              <label className="block text-[11px] font-semibold text-slate-700 mb-1 text-center">الكمية</label>
              <Input
                type="number"
                min="1"
                value={quantityToAdd}
                onChange={(e) => setQuantityToAdd(Math.max(1, Number(e.target.value)))}
                className="h-9 text-center font-bold"
                isLtr
              />
            </div>

            {/* Unit Price */}
            <div className="sm:col-span-2">
              <label className="block text-[11px] font-semibold text-slate-700 mb-1 text-center">
                سعر البيع (د.ل)
              </label>
              <Input
                type="number"
                step="0.01"
                min="0"
                value={unitPriceToAdd}
                onChange={(e) => setUnitPriceToAdd(Number(e.target.value))}
                className="h-9 text-center font-bold text-sky-900"
                isLtr
              />
            </div>

            {/* Item Discount */}
            <div className="sm:col-span-1">
              <label className="block text-[11px] font-semibold text-slate-700 mb-1 text-center">الخصم</label>
              <Input
                type="number"
                step="0.01"
                min="0"
                value={discountToAdd}
                onChange={(e) => setDiscountToAdd(Number(e.target.value))}
                className="h-9 text-center"
                isLtr
              />
            </div>

            {/* Add Button */}
            <div className="sm:col-span-1">
              <Button
                type="button"
                variant="primary"
                size="md"
                onClick={handleAddItem}
                disabled={!selectedProduct}
                className="w-full h-9 bg-sky-700 hover:bg-sky-800 text-white font-bold p-0 flex items-center justify-center"
                title="إضافة البند"
              >
                <IconPlus className="w-5 h-5" />
              </Button>
            </div>
          </div>

          {searchErrorMessage && (
            <div className="text-xs text-rose-600 font-semibold bg-rose-50 px-3 py-1.5 rounded-lg border border-rose-200">
              {searchErrorMessage}
            </div>
          )}
        </div>

        {/* ════════ 3. ITEMS DATA GRID ════════ */}
        <div className="border border-slate-200 rounded-xl overflow-hidden shadow-2xs">
          <div className="p-2.5 bg-slate-50 border-b border-slate-200 flex items-center justify-between">
            <span className="text-xs font-bold text-slate-800">
              بنود الفاتورة المسجلة ({items.length})
            </span>
            {items.length > 0 && (
              <span className="text-xs font-bold text-sky-800">
                إجمالي القطع: {items.reduce((s, i) => s + i.quantity, 0)} قطعة
              </span>
            )}
          </div>

          <div className="overflow-x-auto max-h-56">
            <table className="w-full text-right text-xs">
              <thead className="bg-slate-100/70 border-b border-slate-200 text-slate-600 font-semibold sticky top-0">
                <tr>
                  <th className="py-2.5 px-3">اسم الصنف</th>
                  <th className="py-2.5 px-3">الكود / الباركود</th>
                  <th className="py-2.5 px-2 text-center">الكمية</th>
                  <th className="py-2.5 px-3 text-center">سعر الوحدة</th>
                  <th className="py-2.5 px-2 text-center">الخصم</th>
                  <th className="py-2.5 px-3 text-center font-bold">الإجمالي</th>
                  <th className="py-2.5 px-2 text-center w-10"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {items.length === 0 ? (
                  <tr>
                    <td colSpan={7} className="py-8 text-center text-slate-400">
                      لم يتم إضافة أي أصناف للفاتورة بعد — اختر الصنف واضغط زر الإضافة (+)
                    </td>
                  </tr>
                ) : (
                  items.map((item, idx) => (
                    <tr key={idx} className="hover:bg-slate-50/70 transition-colors">
                      <td className="py-2.5 px-3 font-bold text-slate-900">{item.productName}</td>
                      <td className="py-2.5 px-3 font-mono text-[11px] text-slate-500">
                        {item.barCode || item.productCode || '-'}
                      </td>
                      <td className="py-2.5 px-2 text-center font-bold text-slate-800">
                        {item.quantity}
                      </td>
                      <td className="py-2.5 px-3 text-center font-mono num-ltr">
                        {item.unitPrice.toFixed(2)} د.ل
                      </td>
                      <td className="py-2.5 px-2 text-center font-mono text-slate-500 num-ltr">
                        {item.discountAmount > 0 ? `${item.discountAmount.toFixed(2)} د.ل` : '-'}
                      </td>
                      <td className="py-2.5 px-3 text-center font-extrabold text-sky-950 font-mono num-ltr">
                        {item.lineTotal.toFixed(2)} د.ل
                      </td>
                      <td className="py-2.5 px-2 text-center">
                        <button
                          type="button"
                          onClick={() => handleRemoveItem(idx)}
                          className="p-1 rounded hover:bg-rose-50 text-slate-400 hover:text-rose-600 transition-colors"
                          title="حذف البند"
                        >
                          <IconTrash className="w-4 h-4" />
                        </button>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </div>

        {/* ════════ 4. FINANCIAL SUMMARY ════════ */}
        <div className="p-3.5 bg-slate-50 border border-slate-200 rounded-xl">
          <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 items-center">
            <div>
              <span className="block text-[11px] font-medium text-slate-500">المجموع الفرعي:</span>
              <span className="text-base font-extrabold text-slate-900 num-ltr">
                {subTotal.toFixed(2)} د.ل
              </span>
            </div>

            <div>
              <label className="block text-[11px] font-medium text-slate-500 mb-0.5">
                الخصم الإجمالي:
              </label>
              <Input
                type="number"
                step="0.01"
                min="0"
                value={discountAmount}
                onChange={(e) => setDiscountAmount(Number(e.target.value))}
                className="h-8 text-center text-xs font-bold"
                isLtr
              />
            </div>

            <div className="col-span-2 sm:col-span-1">
              <span className="block text-[11px] font-bold text-sky-900">الصافي المطلوب:</span>
              <span className="text-lg font-black text-sky-800 num-ltr">
                {totalAmount.toFixed(2)} د.ل
              </span>
            </div>

            <div>
              <label className="block text-[11px] font-medium text-slate-500 mb-0.5">المدفوع:</label>
              <Input
                type="number"
                step="0.01"
                min="0"
                value={paidAmount}
                onChange={(e) => {
                  setPaidAmount(Number(e.target.value));
                  setIsPaidManuallyModified(true);
                }}
                className="h-8 text-center text-xs font-bold text-emerald-900"
                isLtr
              />
            </div>

            <div>
              <span className="block text-[11px] font-bold text-rose-700">المتبقي (الدين):</span>
              <span
                className={`text-base font-black num-ltr ${
                  remainingAmount > 0 ? 'text-rose-600' : 'text-slate-400'
                }`}
              >
                {remainingAmount.toFixed(2)} د.ل
              </span>
            </div>
          </div>
        </div>
      </form>
    </Modal>
  );
};

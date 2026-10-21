import { lookupService, type LookupOption } from '../../../shared/services/lookupService';
import { createDocumentNumber } from '../../../shared/utils/documentNumber';
import React, { useState, useEffect, useRef } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { stockService } from '../../stock/services/stockService';
import { useAuth } from '../../../shared/context/AuthContext';
import type { WarehouseDto } from '../../stock/types/stock.types';
import type { ProductDto } from '../../catalog/types/catalog.types';
import type { CreatePurchaseOrderRequest } from '../types/purchase.types';
import { IconPlus, IconTrash, IconShoppingCart, IconBarcode, IconAlertCircle } from '@tabler/icons-react';

interface PurchaseOrderModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreatePurchaseOrderRequest) => Promise<void>;
}

interface OrderItemRow {
  productId: string;
  productName: string;
  productBarCodeId: string;
  barcodeValue: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export const PurchaseOrderModal: React.FC<PurchaseOrderModalProps> = ({ isOpen, onClose, onSave }) => {
  const { currentBranchId } = useAuth();

  const [orderNumber, setOrderNumber] = useState('');
  const [orderDate, setOrderDate] = useState('');
  const [expectedDate, setExpectedDate] = useState('');
  const [branchId, setBranchId] = useState('');
  const [warehouseId, setWarehouseId] = useState('');
  const [supplierId, setSupplierId] = useState('');

  // Dropdown lists
  const [branches, setBranches] = useState<LookupOption[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [suppliers, setSuppliers] = useState<LookupOption[]>([]);
  const [products, setProducts] = useState<ProductDto[]>([]);

  // Item entry
  const [searchBarcode, setSearchBarcode] = useState('');
  const [selectedProductId, setSelectedProductId] = useState('');
  const [selectedProduct, setSelectedProduct] = useState<ProductDto | null>(null);
  const [selectedBarcodeId, setSelectedBarcodeId] = useState('');
  const [quantity, setQuantity] = useState<number>(1);
  const [unitPrice, setUnitPrice] = useState<number>(0);

  const [items, setItems] = useState<OrderItemRow[]>([]);
  const [isLoadingData, setIsLoadingData] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const productRequestVersion = useRef(0);

  useEffect(() => {
    productRequestVersion.current += 1;
    if (!isOpen) return;
    let cancelled = false;

    const now = new Date();
    setOrderNumber(createDocumentNumber('PO'));
    setOrderDate(now.toISOString().slice(0, 10));

    const exp = new Date();
    exp.setDate(exp.getDate() + 5);
    setExpectedDate(exp.toISOString().slice(0, 10));

    setItems([]);
    setSelectedProductId('');
    setSelectedProduct(null);
    setSelectedBarcodeId('');
    setQuantity(1);
    setUnitPrice(0);
    setSearchBarcode('');
    setFormError(null);

    setIsLoadingData(true);
    Promise.all([
      lookupService.list('branches').catch(() => []),
      stockService.getWarehouses().catch(() => []),
      lookupService.list('suppliers').catch(() => []),
      lookupService.page<ProductDto>('products', { pageNumber: 1, pageSize: 200 }).catch(() => ({ items: [] })),
    ])
      .then(([bList, wList, sList, pResult]) => {
        if (cancelled) return;
        setBranches(bList);
        const activeWarehouses = wList.filter((w) => w.isActive);
        setWarehouses(activeWarehouses);
        const activeSuppliers = sList.filter((s) => s.isActive);
        setSuppliers(activeSuppliers);
        setProducts(pResult.items || []);

        const initialBranchId = currentBranchId || (bList.length > 0 ? bList[0].id : '');
        setBranchId(initialBranchId);

        setWarehouseId(activeWarehouses.find((w) => w.branchId === initialBranchId)?.id || '');
        if (activeSuppliers.length > 0) setSupplierId(activeSuppliers[0].id);
      })
      .catch((err) => {
        if (cancelled) return;
        console.error('Failed to load purchase order dependencies:', err);
        setFormError('تعذر تحميل البيانات المساعدة من الخادم');
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

  // Product selection handler
  const applyProduct = (product: ProductDto, scannedBarcode?: string) => {
    const barcode = product.barCodes?.find((item) => item.barCode.toLowerCase() === scannedBarcode?.toLowerCase())
      || product.barCodes?.[0];
    setSelectedProductId(product.id);
    setSelectedProduct(product);
    setSelectedBarcodeId(barcode?.id || '');
    setSearchBarcode(barcode?.barCode || '');
    setUnitPrice(product.costPrice || 0);
  };

  const handleSelectProduct = async (prodId: string) => {
    const version = ++productRequestVersion.current;
    setSelectedProductId(prodId);
    setSelectedProduct(null);
    setSelectedBarcodeId('');
    setSearchBarcode('');
    setUnitPrice(0);
    if (!prodId) {
      return;
    }

    try {
      const fullProd = await lookupService.product(prodId);
      if (version !== productRequestVersion.current) return;
      applyProduct(fullProd);
    } catch {
      if (version === productRequestVersion.current) setFormError('تعذر تحميل تفاصيل الصنف والباركودات');
    }
  };

  // Barcode search on enter
  const handleBarcodeSearch = async (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key !== 'Enter' || !searchBarcode.trim()) return;
    e.preventDefault();
    const version = ++productRequestVersion.current;
    const scannedBarcode = searchBarcode.trim();
    setSelectedProduct(null);
    setSelectedProductId('');
    setSelectedBarcodeId('');

    try {
      const found = await lookupService.barcode(scannedBarcode);
      if (version !== productRequestVersion.current) return;
      if (found) {
        applyProduct(found, scannedBarcode);
      } else {
        setFormError(`لم يتم العثور على صنف بالباركود: ${searchBarcode}`);
      }
    } catch {
      if (version === productRequestVersion.current) setFormError('خطأ أثناء البحث عن الباركود');
    }
  };

  // Add Item to order
  const handleAddItem = () => {
    if (!selectedProduct || selectedProduct.id !== selectedProductId) {
      setFormError('يرجى اختيار صنف أولاً');
      return;
    }
    if (!selectedBarcodeId) {
      setFormError('يجب أن يحتوي الصنف على باركود صالح لحفظ أمر الشراء');
      return;
    }
    if (quantity <= 0) {
      setFormError('يجب أن تكون الكمية أكبر من صفر');
      return;
    }
    if (unitPrice < 0) {
      setFormError('سعر الوحدة التقديري لا يمكن أن يكون سالباً');
      return;
    }

    const barcodeObj = selectedProduct.barCodes?.find((b) => b.id === selectedBarcodeId);
    const barcodeVal = barcodeObj?.barCode || searchBarcode || '-';

    // Check if already in order
    const existingIndex = items.findIndex((i) => i.productBarCodeId === selectedBarcodeId);
    if (existingIndex >= 0) {
      const updated = [...items];
      updated[existingIndex].quantity += quantity;
      updated[existingIndex].lineTotal = updated[existingIndex].quantity * updated[existingIndex].unitPrice;
      setItems(updated);
    } else {
      const newItem: OrderItemRow = {
        productId: selectedProductId,
        productName: selectedProduct.name,
        productBarCodeId: selectedBarcodeId,
        barcodeValue: barcodeVal,
        quantity,
        unitPrice,
        lineTotal: quantity * unitPrice,
      };
      setItems([...items, newItem]);
    }

    // Reset entry
    setSelectedProductId('');
    setSelectedProduct(null);
    setSelectedBarcodeId('');
    setSearchBarcode('');
    setQuantity(1);
    setUnitPrice(0);
    setFormError(null);
  };

  const handleRemoveItem = (index: number) => {
    setItems(items.filter((_, idx) => idx !== index));
  };

  const totalOrderAmount = items.reduce((acc, curr) => acc + curr.lineTotal, 0);

  const handleSubmit = async () => {
    if (!orderNumber.trim()) {
      setFormError('رقم أمر الشراء مطلوب');
      return;
    }
    if (!branchId) {
      setFormError('يرجى تحديد الفرع الطالب');
      return;
    }
    if (items.length === 0) {
      setFormError('يجب إضافة بند واحد على الأقل في أمر الشراء');
      return;
    }

    setIsSubmitting(true);
    setFormError(null);

    try {
      const payload: CreatePurchaseOrderRequest = {
        orderNumber: orderNumber.trim(),
        branchId,
        supplierId: supplierId || undefined,
        warehouseId: warehouseId || undefined,
        orderDate: new Date(orderDate).toISOString(),
        expectedDate: expectedDate ? new Date(expectedDate).toISOString() : undefined,
        items: items.map((i) => ({
          productBarCodeId: i.productBarCodeId,
          quantity: i.quantity,
          unitPrice: i.unitPrice,
        })),
      };

      await onSave(payload);
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل إنشاء أمر الشراء';
      setFormError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إنشاء أمر شراء وتوريد جديد"
      subtitle="إصدار طلبيات الشراء للموردين ومتابعة توريد الأصناف المستهدفة"
      size="2xl"
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
              إلغاء
            </Button>
            <Button
              variant="primary"
              size="md"
              leftIcon={<IconShoppingCart className="w-4 h-4" />}
              onClick={handleSubmit}
              isLoading={isSubmitting}
            >
              حفظ واعتماد أمر الشراء
            </Button>
          </div>
        </div>
      }
    >
      <div className="flex flex-col gap-5 py-1 text-xs">
        {/* بيانات الأمر الأساسية */}
        <div className="p-4 bg-slate-50 border border-slate-200 rounded-xl grid grid-cols-1 sm:grid-cols-3 gap-3">
          <Input
            label="رقم أمر الشراء *"
            value={orderNumber}
            onChange={(e) => setOrderNumber(e.target.value)}
            disabled={isLoadingData}
          />

          <Input
            label="تاريخ الطلب *"
            type="date"
            value={orderDate}
            onChange={(e) => setOrderDate(e.target.value)}
            disabled={isLoadingData}
          />

          <Input
            label="تاريخ التوريد المتوقع"
            type="date"
            value={expectedDate}
            onChange={(e) => setExpectedDate(e.target.value)}
            disabled={isLoadingData}
          />

          <Select
            lookup={{ resource: 'branches' }} label="الفرع الطالب *"
            value={branchId}
            onChange={(e) => setBranchId(e.target.value)}
            options={branches.map((b) => ({ value: b.id, label: b.name }))}
            disabled={isLoadingData}
          />

          <Select
            lookup={{ resource: 'warehouses', branchId: branchId }} onLookupSelect={(option) => setWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="المستودع المستلم"
            value={warehouseId}
            onChange={(e) => setWarehouseId(e.target.value)}
            options={[
              { value: '', label: 'بدون تحديد مستودع' },
              ...branchWarehouses.map((w) => ({ value: w.id, label: w.name })),
            ]}
            disabled={isLoadingData}
          />

          <Select
            lookup={{ resource: 'suppliers' }} label="المورد المستهدف"
            value={supplierId}
            onChange={(e) => setSupplierId(e.target.value)}
            options={[
              { value: '', label: 'طلب توريد عام (بدون مورد)' },
              ...suppliers.map((s) => ({ value: s.id, label: s.name })),
            ]}
            disabled={isLoadingData}
          />
        </div>

        {/* سطر إضافة صنف للطلب */}
        <div className="p-4 bg-emerald-50/40 border border-emerald-100 rounded-xl flex flex-col gap-3">
          <span className="font-bold text-slate-800 text-sm flex items-center gap-1.5">
            <IconBarcode className="w-4 h-4 text-emerald-600" />
            إضافة صنف إلى أمر الشراء
          </span>

          <div className="grid grid-cols-1 sm:grid-cols-12 gap-2.5 items-end">
            <div className="sm:col-span-3">
              <Input
                placeholder="امسح باركود واضغط Enter"
                value={searchBarcode}
                onChange={(e) => setSearchBarcode(e.target.value)}
                onKeyDown={handleBarcodeSearch}
              />
            </div>

            <div className="sm:col-span-4">
              <Select lookup={{ resource: 'products' }} label="الصنف"
                value={selectedProductId}
                onChange={(e) => handleSelectProduct(e.target.value)}
                options={[
                  { value: '', label: 'أو اختر الصنف من القائمة...' },
                  ...products.map((p) => ({ value: p.id, label: `${p.name} (${p.code || '-'})` })),
                ]}
              />
            </div>

            {selectedProduct && selectedProduct.barCodes && selectedProduct.barCodes.length > 1 && (
              <div className="sm:col-span-2">
                <Select
                  value={selectedBarcodeId}
                  onChange={(e) => setSelectedBarcodeId(e.target.value)}
                  options={selectedProduct.barCodes.map((b) => ({
                    value: b.id || '',
                    label: `${b.barCode}${b.description ? ` (${b.description})` : ''}`,
                  }))}
                />
              </div>
            )}

            <div className="sm:col-span-2">
              <Input
                label="الكمية"
                type="number"
                min="1"
                value={quantity}
                onChange={(e) => setQuantity(Math.max(1, Number(e.target.value) || 1))}
              />
            </div>

            <div className="sm:col-span-2">
              <Input
                label="سعر التكلفة التقديري"
                type="number"
                step="0.01"
                min="0"
                value={unitPrice}
                onChange={(e) => setUnitPrice(Math.max(0, Number(e.target.value) || 0))}
              />
            </div>

            <div className="sm:col-span-1">
              <Button
                variant="primary"
                size="md"
                onClick={handleAddItem}
                className="w-full flex items-center justify-center p-2.5"
                title="إضافة البند"
              >
                <IconPlus className="w-5 h-5" />
              </Button>
            </div>
          </div>
        </div>

        {/* جدول بنود أمر الشراء */}
        <div className="border border-slate-200 rounded-xl overflow-hidden">
          <table className="w-full text-right text-xs">
            <thead className="bg-slate-50 text-slate-600 font-bold border-b border-slate-200">
              <tr>
                <th className="py-2.5 px-3">#</th>
                <th className="py-2.5 px-3">اسم الصنف</th>
                <th className="py-2.5 px-3">الباركود</th>
                <th className="py-2.5 px-3 text-center">الكمية المطلوبة</th>
                <th className="py-2.5 px-3 text-center">سعر الوحدة التقديري</th>
                <th className="py-2.5 px-3 text-left">الإجمالي المتوقع</th>
                <th className="py-2.5 px-3 text-center w-12">حذف</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={7} className="py-8 text-center text-slate-400">
                    لم يتم إضافة أصناف لأمر الشراء بعد. اختر صنفاً أو امسح باركود لإضافته.
                  </td>
                </tr>
              ) : (
                items.map((item, idx) => (
                  <tr key={idx} className="hover:bg-slate-50/50">
                    <td className="py-2 px-3 text-slate-400 font-mono">{idx + 1}</td>
                    <td className="py-2 px-3 font-bold text-slate-900">{item.productName}</td>
                    <td className="py-2 px-3 font-mono text-slate-500 text-[11px]">{item.barcodeValue}</td>
                    <td className="py-2 px-3 text-center font-bold text-slate-800">{item.quantity}</td>
                    <td className="py-2 px-3 text-center num-ltr">{item.unitPrice.toFixed(2)} د.ل</td>
                    <td className="py-2 px-3 text-left font-black text-slate-900 num-ltr">
                      {item.lineTotal.toFixed(2)} د.ل
                    </td>
                    <td className="py-2 px-3 text-center">
                      <button
                        type="button"
                        onClick={() => handleRemoveItem(idx)}
                        className="p-1 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded transition-colors cursor-pointer"
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

        {/* إجمالي أمر الشراء */}
        <div className="flex justify-between items-center bg-slate-50 border border-slate-200 rounded-xl p-3.5">
          <div className="text-slate-600">
            عدد البنود المطلوبة: <span className="font-bold text-slate-900">{items.length}</span> أصناف
          </div>
          <div className="flex items-center gap-2">
            <span className="font-bold text-slate-700">إجمالي التكلفة التقديرية للطلبية:</span>
            <span className="text-base font-black text-emerald-700 num-ltr">
              {totalOrderAmount.toFixed(2)} د.ل
            </span>
          </div>
        </div>
      </div>
    </Modal>
  );
};

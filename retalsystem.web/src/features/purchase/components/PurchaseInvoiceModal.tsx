import { lookupService, type LookupOption } from '../../../shared/services/lookupService';
import { allocatePurchaseLine, purchaseLineTotal } from '../utils/purchasePriceAllocation';
import { createDocumentNumber } from '../../../shared/utils/documentNumber';
import React, { useState, useEffect, useMemo, useCallback, useRef } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { QuickProductModal } from './QuickProductModal';
import { QuickUnitModal } from './QuickUnitModal';
import { stockService } from '../../stock/services/stockService';
import { useAuth } from '../../../shared/context/AuthContext';
import { Permissions } from '../../../shared/auth/permissions';
import type { WarehouseDto } from '../../stock/types/stock.types';
import type { ProductDto } from '../../catalog/types/catalog.types';
import {
  PurchaseInvoiceStatus,
  PurchasePaymentMethod,
  type PurchaseInvoiceDto,
  type CreatePurchaseInvoiceItemRequest,
  type CreatePurchaseInvoiceRequest,
  type PackagingUnitOption,
  type FlavorBreakdownItem,
} from '../types/purchase.types';
import {
  IconFileInvoice,
  IconBarcode,
  IconPlus,
  IconTrash,
  IconSearch,
  IconCheck,
  IconAlertCircle,
} from '@tabler/icons-react';

interface PurchaseInvoiceModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreatePurchaseInvoiceRequest) => Promise<void>;
  invoiceToEdit?: PurchaseInvoiceDto | null;
  onUpdate?: (id: string, data: CreatePurchaseInvoiceRequest) => Promise<void>;
}

export const PurchaseInvoiceModal: React.FC<PurchaseInvoiceModalProps> = ({
  isOpen,
  onClose,
  onSave,
  invoiceToEdit,
  onUpdate,
}) => {
  const { currentBranchId, hasPermission } = useAuth();
  const lookupVersion = useRef(0);
  const invalidateLookup = useCallback(() => { lookupVersion.current++; }, []);
  useEffect(() => invalidateLookup, [isOpen, invalidateLookup]);
  const canCreateProduct = hasPermission(Permissions.Products.Create);
  const canCreateUnit = hasPermission(Permissions.Units.Create);

  // ════════ Master Data Lists ════════
  const [branches, setBranches] = useState<LookupOption[]>([]);
  const [suppliers, setSuppliers] = useState<LookupOption[]>([]);
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [safes, setSafes] = useState<LookupOption[]>([]);
  const [categories, setCategories] = useState<LookupOption[]>([]);
  const [units, setUnits] = useState<LookupOption[]>([]);

  // ════════ Invoice Main Fields ════════
  const [invoiceNumber, setInvoiceNumber] = useState('');
  const [invoiceDate, setInvoiceDate] = useState('');
  const [branchId, setBranchId] = useState('');
  const [supplierId, setSupplierId] = useState('');
  const [warehouseId, setWarehouseId] = useState('');
  const [paymentMethod, setPaymentMethod] = useState<number>(PurchasePaymentMethod.Cash);
  const [paymentSafeId, setPaymentSafeId] = useState<string>('');
  const [notes, setNotes] = useState('');

  // ════════ Items Table State ════════
  const [items, setItems] = useState<CreatePurchaseInvoiceItemRequest[]>([]);
  const [discountAmount, setDiscountAmount] = useState<number>(0);
  const [taxAmount, setTaxAmount] = useState<number>(0);
  const [paidAmount, setPaidAmount] = useState<number>(0);
  const [isPaidManuallyModified, setIsPaidManuallyModified] = useState(false);

  // ════════ Smart Barcode & Product Entry ════════
  const [searchBarcode, setSearchBarcode] = useState('');
  const [isSearchingBarcode, setIsSearchingBarcode] = useState(false);
  const [selectedProduct, setSelectedProduct] = useState<ProductDto | null>(null);
  const [barcodeSearchMessage, setBarcodeSearchMessage] = useState<string | null>(null);

  // ════════ Reference Badges ════════
  const [previousCostPrice, setPreviousCostPrice] = useState<number>(0);
  const [productAverageCost, setProductAverageCost] = useState<number>(0);
  const [currentSalePrice, setCurrentSalePrice] = useState<number>(0);

  // ════════ Packaging Units & Breakdown ════════
  const [packagingOptions, setPackagingOptions] = useState<PackagingUnitOption[]>([]);
  const [selectedUnitOption, setSelectedUnitOption] = useState<PackagingUnitOption | null>(null);
  const [unitsPerPackage, setUnitsPerPackage] = useState<number>(1);
  const [packageQuantity, setPackageQuantity] = useState<number>(1);
  const [invoiceEnteredPrice, setInvoiceEnteredPrice] = useState<number>(0);

  // ════════ Flavor & Barcode Distribution Matrix ════════
  const [hasMultipleFlavors, setHasMultipleFlavors] = useState<boolean>(false);
  const [flavorBreakdowns, setFlavorBreakdowns] = useState<FlavorBreakdownItem[]>([]);

  // ════════ Target Sale Price & Profit Analytics ════════
  const [targetSalePrice, setTargetSalePrice] = useState<number>(0);
  const [itemDiscount, setItemDiscount] = useState<number>(0);

  // ════════ Sub-modals for Quick Creation ════════
  const [isQuickProductOpen, setIsQuickProductOpen] = useState(false);
  const [isQuickUnitOpen, setIsQuickUnitOpen] = useState(false);

  // ════════ UI Status & Errors ════════
  const [isLoadingData, setIsLoadingData] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);

  // ── 1. Initialize Master Data on Open ──
  useEffect(() => {
    if (!isOpen) return;
    let cancelled = false;

    if (invoiceToEdit) {
      setInvoiceNumber(invoiceToEdit.invoiceNumber || '');
      setInvoiceDate(
        invoiceToEdit.invoiceDate
          ? invoiceToEdit.invoiceDate.slice(0, 10)
          : new Date().toISOString().slice(0, 10)
      );
      setBranchId(invoiceToEdit.branchId || '');
      setSupplierId(invoiceToEdit.supplierId || '');
      setWarehouseId(invoiceToEdit.warehouseId || '');
      setPaymentMethod(invoiceToEdit.paymentMethod ?? PurchasePaymentMethod.Cash);
      setPaymentSafeId(invoiceToEdit.paymentSafeId || '');
      setNotes(invoiceToEdit.notes || '');
      setDiscountAmount(invoiceToEdit.discountAmount || 0);
      setTaxAmount(invoiceToEdit.taxAmount || 0);
      setPaidAmount(invoiceToEdit.paidAmount || 0);
      setIsPaidManuallyModified(true);
      resetProductForm();
      setFormError(null);

      const loadedItems: CreatePurchaseInvoiceItemRequest[] = (invoiceToEdit.items || []).map((it) => {
        const q = Number(it.quantity) || 1;
        const uCost = Number(it.unitCost ?? it.unitPrice ?? 0);
        const disc = Number(it.discountAmount) || 0;
        const lTotal = Number(it.lineTotal ?? it.totalCost ?? (q * uCost - disc));
        return {
          productId: it.productId,
          productName: it.productName || 'صنف',
          productBarCodeId: it.productBarCodeId,
          barCode: it.barCode || it.code || '',
          packageUnitName: it.unitName || 'قطعة',
          packageQuantity: q,
          unitsPerPackage: 1,
          invoicePackagePrice: uCost,
          quantity: q,
          unitPrice: uCost,
          salePrice: 0,
          discountAmount: disc,
          lineTotal: lTotal,
          profitPerPiece: 0,
          profitMarginPercentage: 0,
          packagingSummary: `${q} ${it.unitName || 'قطعة'}`,
          flavorsSummaryText: it.barCode || it.code || 'افتراضي',
          breakdowns: it.breakdowns,
        };
      });
      setItems(loadedItems);
    } else {
      const now = new Date();
      const dateFormatted = now.toISOString().slice(0, 10);

      setInvoiceNumber(createDocumentNumber('PINV'));
      setInvoiceDate(dateFormatted);
      setPaymentMethod(PurchasePaymentMethod.Cash);
      setNotes('');
      setItems([]);
      setDiscountAmount(0);
      setTaxAmount(0);
      setPaidAmount(0);
      setIsPaidManuallyModified(false);
      resetProductForm();
      setFormError(null);
    }

    const applyMasterData = (
      bList: LookupOption[],
      sList: LookupOption[],
      wList: WarehouseDto[],
      sfList: LookupOption[],
      cList: LookupOption[],
      uList: LookupOption[]
    ) => {
      setBranches(bList);
      const activeSuppliers = sList.filter((s) => s.isActive);
      setSuppliers(activeSuppliers);
      const activeWarehouses = wList.filter((w) => w.isActive);
      setWarehouses(activeWarehouses);
      setCategories(cList);
      setUnits(uList);

      if (!invoiceToEdit) {
        // Set initial branch for new invoice
        let initialBranchId = currentBranchId || (bList.length > 0 ? bList[0].id : '');
        if (!initialBranchId && bList.length > 0) initialBranchId = bList[0].id;
        setBranchId(initialBranchId);

        if (activeSuppliers.length > 0) setSupplierId(activeSuppliers[0].id);
        setWarehouseId(activeWarehouses.find((w) => w.branchId === initialBranchId)?.id || '');

        // Filter safes for this branch
        const branchSafes = sfList.filter((s) => s.isActive && (!initialBranchId || s.branchId === initialBranchId));
        setSafes(branchSafes);
        if (branchSafes.length > 0) setPaymentSafeId(branchSafes[0].id);
      } else {
        // Maintain the branch safes for the existing invoice
        const bId = invoiceToEdit.branchId;
        const branchSafes = sfList.filter((s) => s.isActive && (!bId || s.branchId === bId));
        setSafes(branchSafes);
        if (invoiceToEdit.paymentSafeId) {
          setPaymentSafeId(invoiceToEdit.paymentSafeId);
        } else if (branchSafes.length > 0) {
          setPaymentSafeId(branchSafes[0].id);
        }
      }
    };

      setIsLoadingData(true);
      Promise.all([
        lookupService.list('branches').catch(() => []),
        lookupService.list('suppliers').catch(() => []),
        stockService.getWarehouses(undefined, invoiceToEdit?.warehouseId),
        lookupService.list('safes').catch(() => []),
        canCreateProduct ? lookupService.list('categories') : Promise.resolve([]),
        canCreateProduct ? lookupService.list('units') : Promise.resolve([]),
      ])
        .then(([bList, sList, wList, sfList, cList, uList]) => {
          if (cancelled) return;
          applyMasterData(bList, sList, wList, sfList, cList, uList);
        })
        .catch((err) => {
          if (cancelled) return;
          console.error('Failed to load initial purchase invoice master data:', err);
          setFormError('تعذر تحميل بيانات الفروع أو المستودعات أو الموردين');
        })
        .finally(() => { if (!cancelled) setIsLoadingData(false); });
    return () => { cancelled = true; };
  }, [isOpen, invoiceToEdit, currentBranchId, canCreateProduct]);

  // ── Reload safes when selected branch changes ──
  const branchWarehouses = warehouses.filter((warehouse) => warehouse.branchId === branchId);
  useEffect(() => {
    if (!isOpen || !warehouses.length) return;
    const available = warehouses.filter((warehouse) => warehouse.branchId === branchId);
    setWarehouseId((previous) => available.some((warehouse) => warehouse.id === previous) ? previous : available[0]?.id || '');
  }, [isOpen, branchId, warehouses]);

  useEffect(() => {
    if (!isOpen || !branchId) return;
    let cancelled = false;
    lookupService
      .list('safes', branchId)
      .then((sfList) => {
        if (cancelled) return;
        setSafes(sfList);
        setPaymentSafeId((prev) => {
          if (prev && sfList.some((s) => s.id === prev)) return prev;
          return sfList.length > 0 ? sfList[0].id : '';
        });
      })
      .catch(() => {});
    return () => { cancelled = true; };
  }, [isOpen, branchId]);

  // ── Helper to reset item entry form ──
  function resetProductForm() {
    lookupVersion.current++; setIsSearchingBarcode(false);
    setSelectedProduct(null);
    setSearchBarcode('');
    setPreviousCostPrice(0);
    setProductAverageCost(0);
    setCurrentSalePrice(0);
    setPackagingOptions([]);
    setSelectedUnitOption(null);
    setUnitsPerPackage(1);
    setPackageQuantity(1);
    setInvoiceEnteredPrice(0);
    setHasMultipleFlavors(false);
    setFlavorBreakdowns([]);
    setTargetSalePrice(0);
    setItemDiscount(0);
    setBarcodeSearchMessage(null);
  }

  // ── 2. On Product Selected / Loaded: Calculate Packaging & Flavors ──
  const handleProductLoaded = useCallback(
    (product: ProductDto, scannedBarcode?: string) => {
      lookupVersion.current++; setIsSearchingBarcode(false);
      setSelectedProduct(product);
      setBarcodeSearchMessage(null);

      const prevCost = Number(product.costPrice) || 0;
      const avgCost = Number(product.currentStock) && product.costPrice ? product.costPrice : prevCost;
      const sale = Number(product.salePrice) || 0;

      setPreviousCostPrice(prevCost);
      setProductAverageCost(avgCost > 0 ? avgCost : prevCost);
      setCurrentSalePrice(sale);
      setTargetSalePrice(sale > 0 ? sale : prevCost * 1.25);

      // Packaging unit options from product units
      const options: PackagingUnitOption[] = [];
      if (product.units && product.units.length > 0) {
        for (const u of product.units) {
          const factor = u.conversionFactor > 0 ? u.conversionFactor : 1;
          if (!options.some((o) => o.conversionFactor === factor)) {
            options.push({
              name: u.unitName || 'وحدة',
              conversionFactor: factor,
              displayText: `${u.unitName || 'وحدة'} (${factor} قطعة)`,
            });
          }
        }
      } else {
        options.push({
          name: 'قطعة / حبة',
          conversionFactor: 1,
          displayText: 'قطعة / حبة (1 قطعة)',
        });
      }
      options.push({
        name: 'مخصص (يدوي)',
        conversionFactor: 1,
        displayText: 'مخصص (يدوي)',
      });

      setPackagingOptions(options);
      const defaultOpt = options[0];
      setSelectedUnitOption(defaultOpt);
      const factor = defaultOpt.conversionFactor;
      setUnitsPerPackage(factor);
      setPackageQuantity(1);

      const basePrice = prevCost > 0 ? prevCost : sale;
      setInvoiceEnteredPrice(basePrice * factor);

      // Multi-barcodes / Flavors breakdown
      if (product.barCodes && product.barCodes.length > 1) {
        setHasMultipleFlavors(true);
        const breakdowns: FlavorBreakdownItem[] = product.barCodes.map((bc) => {
          const isScanned =
            scannedBarcode &&
            bc.barCode.trim().toLowerCase() === scannedBarcode.trim().toLowerCase();
          const pkgQty = isScanned ? 1 : 0;
          return {
            productBarCodeId: bc.id || '',
            barCode: bc.barCode,
            title: bc.description || bc.barCode,
            packageQuantity: pkgQty,
            unitsPerPackage: factor,
            totalPieces: pkgQty * factor,
            unitPrice: factor > 0 ? basePrice : 0,
            totalCost: pkgQty * factor * basePrice,
          };
        });
        setFlavorBreakdowns(breakdowns);
        const sumPkg = breakdowns.reduce((sum, b) => sum + b.packageQuantity, 0);
        if (sumPkg > 0) setPackageQuantity(sumPkg);
      } else {
        setHasMultipleFlavors(false);
        setFlavorBreakdowns([]);
      }
    },
    []
  );

  // ── 3. Barcode Lookup ──
  const handleLookupBarcode = async () => {
    if (!searchBarcode.trim()) return;
    const version = ++lookupVersion.current;
    setIsSearchingBarcode(true);
    setBarcodeSearchMessage(null);

    try {
      const trimmed = searchBarcode.trim();
      const product = await lookupService.barcode(trimmed);
      if (version !== lookupVersion.current) return;
      if (product && product.id) {
        handleProductLoaded(product, trimmed);
      } else {
        setSelectedProduct(null);
        setBarcodeSearchMessage(`لم يتم العثور على صنف بالباركود: ${trimmed}`);
      }
    } catch {
      if (version !== lookupVersion.current) return;
      setSelectedProduct(null);
      setBarcodeSearchMessage(`لم يتم العثور على صنف بالباركود: ${searchBarcode.trim()}`);
    } finally {
      if (version === lookupVersion.current) setIsSearchingBarcode(false);
    }
  };

  // ── 4. Unit & Packaging Change Handler ──
  const handleUnitOptionChange = (optName: string) => {
    const opt = packagingOptions.find((o) => o.name === optName);
    if (!opt) return;
    setSelectedUnitOption(opt);
    if (opt.name !== 'مخصص (يدوي)') {
      setUnitsPerPackage(opt.conversionFactor);
      if (previousCostPrice > 0) {
        setInvoiceEnteredPrice(previousCostPrice * opt.conversionFactor);
      }
    }
  };

  // ── 5. Calculated Values ──
  const factor = unitsPerPackage > 0 ? unitsPerPackage : 1;
  const totalPieceQuantity = Number((packageQuantity * factor).toFixed(4));
  const calculatedPieceCost = factor > 0 ? invoiceEnteredPrice / factor : invoiceEnteredPrice;

  // Profit Analytics
  const profitFromCurrentCost = targetSalePrice - calculatedPieceCost;
  const profitMarginCurrentPercentage =
    calculatedPieceCost > 0 ? (profitFromCurrentCost / calculatedPieceCost) * 100 : 0;

  const effectiveAvg = productAverageCost > 0 ? productAverageCost : calculatedPieceCost;
  const profitFromAverageCost = targetSalePrice - effectiveAvg;
  const profitMarginAveragePercentage =
    effectiveAvg > 0 ? (profitFromAverageCost / effectiveAvg) * 100 : 0;

  // ── 6. Flavor Breakdown Quantity Change ──
  const handleFlavorQuantityChange = (index: number, qty: number) => {
    setFlavorBreakdowns((prev) => {
      const updated = [...prev];
      const safeQty = Math.max(0, qty);
      updated[index] = {
        ...updated[index],
        packageQuantity: safeQty,
        unitsPerPackage: factor,
        totalPieces: safeQty * factor,
        unitPrice: calculatedPieceCost,
        totalCost: safeQty * factor * calculatedPieceCost,
      };

      const sumPkg = updated.reduce((sum, b) => sum + b.packageQuantity, 0);
      setPackageQuantity(sumPkg > 0 ? sumPkg : 1);
      return updated;
    });
  };

  // ── 7. Add Item to Invoice ──
  const handleAddItem = () => {
    if (!selectedProduct) {
      setFormError('يرجى مسح الباركود أو اختيار الصنف أولاً');
      return;
    }
    if (totalPieceQuantity <= 0) {
      setFormError('الكمية الإجمالية يجب أن تكون أكبر من صفر');
      return;
    }
    if (calculatedPieceCost < 0) {
      setFormError('سعر الشراء لا يمكن أن يكون سالباً');
      return;
    }

    setFormError(null);

    // Prepare breakdowns
    const breakdownsList =
      hasMultipleFlavors && flavorBreakdowns.some((b) => b.packageQuantity > 0)
        ? flavorBreakdowns
            .filter((b) => b.packageQuantity > 0)
            .map((b) => ({
              productBarCodeId: b.productBarCodeId,
              packageQuantity: b.packageQuantity,
              unitsPerPackage: factor,
              quantity: Number((b.packageQuantity * factor).toFixed(4)),
              unitPrice: calculatedPieceCost,
            }))
        : selectedProduct.barCodes && selectedProduct.barCodes.length > 0
        ? [
            {
              productBarCodeId: selectedProduct.barCodes[0].id || '',
              packageQuantity: packageQuantity,
              unitsPerPackage: factor,
              quantity: totalPieceQuantity,
              unitPrice: calculatedPieceCost,
            },
          ]
        : [];

    const flavorsSummary =
      hasMultipleFlavors && flavorBreakdowns.some((b) => b.packageQuantity > 0)
        ? flavorBreakdowns
            .filter((b) => b.packageQuantity > 0)
            .map((b) => `${b.title} (${b.packageQuantity})`)
            .join(' | ')
        : selectedProduct.barCodes?.[0]?.barCode || 'افتراضي';

    const packagingSummary =
      factor > 1
        ? `${packageQuantity} ${selectedUnitOption?.name || 'عبوة'} × ${factor} حبة`
        : `${totalPieceQuantity} قطعة`;

    const lineTotal = 0; // Computed from the exact package price below.

    const newItem: CreatePurchaseInvoiceItemRequest = {
      productId: selectedProduct.id,
      productName: selectedProduct.name,
      productBarCodeId: selectedProduct.barCodes?.[0]?.id,
      barCode: selectedProduct.barCodes?.[0]?.barCode || searchBarcode,
      packageUnitName: selectedUnitOption?.name || 'قطعة',
      packageQuantity,
      unitsPerPackage: factor,
      invoicePackagePrice: invoiceEnteredPrice,
      quantity: totalPieceQuantity,
      unitPrice: calculatedPieceCost,
      salePrice: targetSalePrice,
      discountAmount: itemDiscount,
      lineTotal,
      profitPerPiece: profitFromCurrentCost,
      profitMarginPercentage: profitMarginCurrentPercentage,
      breakdowns: breakdownsList,
      flavorsSummaryText: flavorsSummary,
      packagingSummary,
    };

    try {
      newItem.lineTotal = purchaseLineTotal(newItem);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'تعذر توزيع سعر العبوة');
      return;
    }
    setItems((prev) => [...prev, newItem]);
    resetProductForm();
  };

  // ── 8. Remove Item ──
  const handleRemoveItem = (index: number) => {
    setItems((prev) => prev.filter((_, idx) => idx !== index));
  };

  // ── 9. Totals Calculation ──
  const subTotal = useMemo(() => {
    return Number(items.reduce((sum, item) => sum + item.lineTotal, 0).toFixed(4));
  }, [items]);

  const totalAmount = useMemo(() => {
    return Math.max(0, Number((subTotal - discountAmount + taxAmount).toFixed(4)));
  }, [subTotal, discountAmount, taxAmount]);

  // Adjust paid amount if payment method is cash/card and user hasn't manually edited it
  useEffect(() => {
    if (!isPaidManuallyModified) {
      if (paymentMethod === PurchasePaymentMethod.Cash || paymentMethod === PurchasePaymentMethod.CreditCard) {
        setPaidAmount(totalAmount);
      } else if (paymentMethod === PurchasePaymentMethod.Credit) {
        setPaidAmount(0);
      }
    }
  }, [totalAmount, paymentMethod, isPaidManuallyModified]);

  const remainingAmount = Math.max(0, totalAmount - paidAmount);

  // ── 10. Submit / Save Invoice ──
  const handleSubmit = async (targetStatus: PurchaseInvoiceStatus) => {
    setFormError(null);

    if (!invoiceNumber.trim()) {
      setFormError('رقم الفاتورة مطلوب');
      return;
    }
    if (!branchId) {
      setFormError('يرجى اختيار الفرع المستلم');
      return;
    }
    if (!supplierId) {
      setFormError('يرجى اختيار المورد');
      return;
    }
    if (!warehouseId) {
      setFormError('يرجى اختيار المستودع أو الصالة المستلمة');
      return;
    }
    if (items.length === 0) {
      setFormError('يجب إضافة صنف واحد على الأقل داخل الفاتورة');
      return;
    }

    setIsSubmitting(true);

    try {
      const payload: CreatePurchaseInvoiceRequest = {
        invoiceNumber: invoiceNumber.trim(),
        invoiceDate: new Date(invoiceDate).toISOString(),
        supplierId,
        branchId,
        warehouseId,
        status: targetStatus,
        paymentMethod,
        subTotal,
        discountAmount,
        taxAmount,
        totalAmount,
        paidAmount,
        paymentSafeId: paymentSafeId || undefined,
        notes: notes.trim() || undefined,
        items: items.flatMap((item) => {
          // Existing posted lines already have an authoritative four-decimal price.
          if (item.unitsPerPackage === 1 && Number.isInteger(item.packageQuantity))
            return [{ productId: item.productId, productBarCodeId: item.productBarCodeId, quantity: item.quantity,
              unitPrice: item.unitPrice, discountAmount: item.discountAmount, breakdowns: item.breakdowns }];
          return allocatePurchaseLine(item);
        }),
      };

      if (invoiceToEdit && onUpdate) {
        await onUpdate(invoiceToEdit.id, payload);
      } else {
        await onSave(payload);
      }
      onClose();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل حفظ وترحيل فاتورة المشتريات';
      setFormError(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <>
      <Modal
        isOpen={isOpen}
        onClose={onClose}
        title={
          invoiceToEdit
            ? `تعديل فاتورة مشتريات مفتوحة #${invoiceToEdit.invoiceNumber}`
            : 'فاتورة مشتريات بضاعة'
        }
        subtitle={
          invoiceToEdit
            ? 'تعديل أو استكمال إدخال الأصناف والمبالغ ثم حفظها كمسودة أو إغلاقها وترحيلها'
            : 'إدخال البضائع وترحيلها. توزّع فروق التقريب على الوحدات مع الحفاظ على إجمالي العبوة.'
        }
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
              <Button variant="secondary" size="md" onClick={onClose} disabled={isSubmitting || isLoadingData}>
                إلغاء النافذة
              </Button>
              <span className="text-xs text-slate-600">يُرحّل الحفظ المخزون والحسابات فوراً.</span>
              <Button
                variant="outline"
                size="md"
                onClick={() => handleSubmit(PurchaseInvoiceStatus.Pending)}
                disabled={isSubmitting || isLoadingData || items.length === 0}
              >
                حفظ وترحيل بالمبلغ المدفوع
              </Button>
              <Button
                variant="primary"
                size="md"
                leftIcon={<IconCheck className="w-4 h-4" />}
                onClick={() => handleSubmit(PurchaseInvoiceStatus.Pending)}
                isLoading={isSubmitting || isLoadingData}
                disabled={items.length === 0}
              >
                ترحيل وسداد كامل
              </Button>
            </div>
          </div>
        }
      >
        <div className="flex flex-col gap-4 py-1">
          {/* ════════ Top Status & Meta Banner ════════ */}
          <div className="flex flex-wrap items-center justify-between gap-3 px-3 py-2 bg-slate-50 border border-slate-200 rounded-xl">
            <div className="flex items-center gap-2">
              <span className="text-xs font-bold text-slate-500">حالة الفاتورة:</span>
              <span
                className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-bold ${
                  invoiceToEdit
                    ? 'bg-amber-100 text-amber-800 border border-amber-200'
                    : 'bg-emerald-100 text-emerald-800 border border-emerald-200'
                }`}
              >
                <span
                  className={`w-1.5 h-1.5 rounded-full ${
                    invoiceToEdit ? 'bg-amber-600' : 'bg-emerald-600'
                  }`}
                ></span>
                {invoiceToEdit ? 'فاتورة جارية قيد التعديل والإدخال' : 'مفتوحة قيد الإدخال والاعتماد'}
              </span>
            </div>
            <div className="flex items-center gap-3 text-xs">
              <span className="text-slate-500">
                رقم الفاتورة: <strong className="font-mono text-slate-900">{invoiceNumber}</strong>
              </span>
              <span className="text-slate-300">|</span>
              <span className="text-slate-500">
                إجمالي الأصناف: <strong className="text-emerald-700">{items.length}</strong>
              </span>
            </div>
          </div>

          {invoiceToEdit && (
            <div className="flex items-center gap-2 px-3 py-2 bg-amber-50/80 border border-amber-200 rounded-xl text-amber-800 text-xs">
              <IconAlertCircle className="w-4 h-4 shrink-0 text-amber-600" />
              <span>
                أنت تقوم بتعديل فاتورة مفتوحة. بيانات الفرع والمورد والمستودع ثابتة لضمان صحة رصيد المخزن. يمكنك إضافة أصناف جديدة، تعديل الكميات والأسعار، ثم حفظ التعديلات أو إغلاق الفاتورة وترحيلها.
              </span>
            </div>
          )}

          {/* ════════ 1. MAIN INVOICE HEADER FIELDS ════════ */}
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
                lookup={{ resource: 'branches' }} label="الفرع المستلم *"
                value={branchId}
                onChange={(e) => setBranchId(e.target.value)}
                disabled={Boolean(invoiceToEdit)}
                required
              >
                {branches.map((b) => (
                  <option key={b.id} value={b.id}>
                    {b.name}
                  </option>
                ))}
              </Select>

              <Select
                lookup={{ resource: 'suppliers' }} label="المورد *"
                value={supplierId}
                onChange={(e) => setSupplierId(e.target.value)}
                disabled={Boolean(invoiceToEdit)}
                required
              >
                {suppliers.map((s) => (
                  <option key={s.id} value={s.id}>
                    {s.name}
                  </option>
                ))}
              </Select>

              <Select
                lookup={{ resource: 'warehouses', branchId: branchId }} onLookupSelect={(option) => setWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), stockService.warehouseOption(option)])} label="المستودع / الصالة المستلمة *"
                value={warehouseId}
                onChange={(e) => setWarehouseId(e.target.value)}
                disabled={Boolean(invoiceToEdit)}
                required
              >
                {branchWarehouses.map((w) => (
                  <option key={w.id} value={w.id}>
                    {w.name}
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
                <option value={PurchasePaymentMethod.Cash}>💵 نقداً</option>
                <option value={PurchasePaymentMethod.CreditCard}>💳 بطاقة مصرفية</option>
                <option value={PurchasePaymentMethod.BankTransfer}>🏦 تحويل مصرفي</option>
                <option value={PurchasePaymentMethod.Credit}>📑 آجل / دين على الحساب</option>
              </Select>

              <Input
                label="تاريخ الفاتورة"
                type="date"
                value={invoiceDate}
                onChange={(e) => setInvoiceDate(e.target.value)}
              />

              <Select
                lookup={{ resource: 'safes', branchId }} label="الخزينة / الحساب للسداد"
                value={paymentSafeId}
                onChange={(e) => setPaymentSafeId(e.target.value)}
                disabled={paymentMethod === PurchasePaymentMethod.Credit || Boolean(invoiceToEdit?.paymentSafeId)}
              >
                {safes.map((sf) => (
                  <option key={sf.id} value={sf.id}>
                    {sf.name}
                  </option>
                ))}
              </Select>

              <Input
                label="ملاحظات الفاتورة"
                placeholder="أي تفاصيل أو ملاحظات..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
              />
            </div>
          </div>

          {/* ════════ 2. SMART ITEM ENTRY & PROFIT ANALYTICS PANEL ════════ */}
          <div className="p-3.5 bg-gradient-to-b from-emerald-50/40 to-slate-50/60 border-2 border-emerald-500/40 rounded-xl flex flex-col gap-3 shadow-xs">
            {/* ── Row A: Barcode Search & Reference Badges ── */}
            <div className="flex flex-col lg:flex-row lg:items-end gap-3">
              {/* Barcode Search */}
              <div className="w-full lg:w-72">
                <label className="block text-xs font-bold text-emerald-950 mb-1">
                  أدخل الباركود (Enter للبحث) *
                </label>
                <div className="flex gap-1">
                  <Input
                    placeholder="امسح أو اكتب الباركود..."
                    value={searchBarcode}
                    onChange={(e) => setSearchBarcode(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') {
                        e.preventDefault();
                        handleLookupBarcode();
                      }
                    }}
                    className="h-9 font-mono font-semibold"
                    isLtr
                  />
                  <Button
                    variant="secondary"
                    size="sm"
                    onClick={handleLookupBarcode}
                    isLoading={isSearchingBarcode}
                    className="px-3"
                    title="بحث عن الصنف"
                  >
                    <IconSearch className="w-4 h-4 text-emerald-700" />
                  </Button>
                </div>
              </div>

              {/* Quick Add Product Button */}
              <Button
                variant="outline"
                size="sm"
                leftIcon={<IconPlus className="w-4 h-4 text-emerald-700" />}
                onClick={() => setIsQuickProductOpen(true)}
                className="h-9 whitespace-nowrap bg-white hover:bg-emerald-50 border-emerald-300 text-emerald-900"
              >
                + صنف جديد
              </Button>

              {/* Loaded Product Card */}
              <div className="flex-1">
                <label className="block text-xs font-bold text-slate-700 mb-1">الصنف المحدد</label>
                <div className="h-9 px-3 rounded-lg border border-slate-200 bg-white flex items-center justify-between gap-2 overflow-hidden shadow-2xs">
                  {selectedProduct ? (
                    <>
                      <div className="flex items-center gap-2 truncate">
                        <span className="w-2 h-2 rounded-full bg-emerald-500 shrink-0"></span>
                        <span className="font-bold text-xs text-slate-900 truncate">
                          {selectedProduct.name}
                        </span>
                        {selectedProduct.categoryName && (
                          <span className="text-[10px] px-1.5 py-0.5 bg-slate-100 text-slate-600 rounded">
                            {selectedProduct.categoryName}
                          </span>
                        )}
                      </div>
                      <span className="text-[10px] font-mono text-slate-400 shrink-0">
                        {selectedProduct.code}
                      </span>
                    </>
                  ) : (
                    <span className="text-xs text-slate-400 italic">
                      (امسح الباركود أو اضغط زر صنف جديد...)
                    </span>
                  )}
                </div>
              </div>

              {/* Reference Badges */}
              <div className="flex items-center gap-2 shrink-0">
                <div className="px-2.5 py-1 rounded-lg bg-emerald-50 border border-emerald-200 text-center min-w-20">
                  <div className="text-[9px] text-emerald-800 font-medium">سعر الشراء السابق</div>
                  <div className="text-xs font-bold text-emerald-950 num-ltr">
                    {previousCostPrice.toFixed(2)} د.ل
                  </div>
                </div>

                <div className="px-2.5 py-1 rounded-lg bg-indigo-50 border border-indigo-200 text-center min-w-20">
                  <div className="text-[9px] text-indigo-800 font-medium">متوسط سعر الشراء</div>
                  <div className="text-xs font-bold text-indigo-950 num-ltr">
                    {productAverageCost.toFixed(2)} د.ل
                  </div>
                </div>

                <div className="px-2.5 py-1 rounded-lg bg-amber-50 border border-amber-200 text-center min-w-20">
                  <div className="text-[9px] text-amber-800 font-medium">سعر البيع الحالي</div>
                  <div className="text-xs font-bold text-amber-950 num-ltr">
                    {currentSalePrice.toFixed(2)} د.ل
                  </div>
                </div>
              </div>
            </div>

            {barcodeSearchMessage && (
              <div className="text-xs text-rose-600 font-semibold bg-rose-50 px-3 py-1.5 rounded-lg border border-rose-200">
                {barcodeSearchMessage}
              </div>
            )}

            {/* ── Row B: Packaging Unit Selection & Cost Division ── */}
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-2.5 pt-2 border-t border-emerald-200/60 items-end">
              {/* Packaging Unit */}
              <div className="col-span-2 sm:col-span-1">
                <div className="flex items-center justify-between mb-1">
                  <label className="text-xs font-semibold text-slate-700">طريقة التنزيل / الوحدة</label>
                  {canCreateUnit && <button
                    type="button"
                    onClick={() => setIsQuickUnitOpen(true)}
                    className="text-[10px] text-emerald-700 hover:underline font-bold flex items-center gap-0.5"
                    title="إضافة وحدة قياس جديدة"
                  >
                    <IconPlus className="w-3 h-3" /> وحدة
                  </button>}
                </div>
                <Select
                  value={selectedUnitOption?.name || ''}
                  onChange={(e) => handleUnitOptionChange(e.target.value)}
                  disabled={!selectedProduct}
                  className="h-9"
                >
                  {packagingOptions.map((opt) => (
                    <option key={opt.name} value={opt.name}>
                      {opt.displayText}
                    </option>
                  ))}
                </Select>
              </div>

              {/* Units Per Package (Conversion factor) */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  القطع بالعبوة
                </label>
                <Input
                  type="number"
                  min="1"
                  value={unitsPerPackage}
                  onChange={(e) => setUnitsPerPackage(Math.max(1, Number(e.target.value)))}
                  disabled={!selectedProduct}
                  className="h-9 text-center font-bold"
                  isLtr
                />
              </div>

              {/* Package Quantity */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  الكمية المشتراة
                </label>
                <Input
                  type="number"
                  min="1"
                  value={packageQuantity}
                  onChange={(e) => setPackageQuantity(Math.max(1, Number(e.target.value)))}
                  disabled={!selectedProduct}
                  className="h-9 text-center font-bold"
                  isLtr
                />
              </div>

              {/* Invoice Entered Package Price */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">
                  سعر العبوة بالفاتورة (د.ل)
                </label>
                <Input
                  type="number"
                  step="0.01"
                  min="0"
                  value={invoiceEnteredPrice}
                  onChange={(e) => setInvoiceEnteredPrice(Number(e.target.value))}
                  disabled={!selectedProduct}
                  className="h-9 text-center font-bold text-emerald-900"
                  isLtr
                />
              </div>

              {/* Calculated Piece Cost Output */}
              <div className="col-span-2 sm:col-span-2 p-2 rounded-lg bg-emerald-100/70 border border-emerald-300 flex items-center justify-between h-9">
                <span className="text-xs text-emerald-950 font-medium">سعر القطعة الناتج:</span>
                <span className="text-sm font-extrabold text-emerald-900 num-ltr">
                  {calculatedPieceCost.toFixed(2)} د.ل
                  <span className="text-[10px] text-emerald-700 font-normal mr-1">
                    (إجمالي: {totalPieceQuantity} حبة)
                  </span>
                </span>
              </div>
            </div>

            {/* ── Row C: Flavor & Barcode Distribution Matrix (if multi-flavor) ── */}
            {hasMultipleFlavors && flavorBreakdowns.length > 0 && (
              <div className="p-3 bg-white border border-emerald-300 rounded-xl flex flex-col gap-2">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-1.5 text-xs font-bold text-emerald-950">
                    <IconBarcode className="w-4 h-4 text-emerald-600" />
                    <span>توزيع كميات النكهات والباركودات المستلمة للمستودع:</span>
                  </div>
                  <span className="text-xs font-extrabold text-emerald-800">
                    إجمالي العبوات الموزعة: {packageQuantity} عبوة ({totalPieceQuantity} قطعة)
                  </span>
                </div>

                <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-2">
                  {flavorBreakdowns.map((fb, idx) => (
                    <div
                      key={fb.barCode}
                      className="p-2 bg-slate-50 border border-slate-200 rounded-lg flex flex-col gap-1 text-xs"
                    >
                      <div className="font-bold text-slate-800 truncate" title={fb.title}>
                        {fb.title}
                      </div>
                      <div className="text-[10px] font-mono text-slate-400">{fb.barCode}</div>
                      <div className="flex items-center justify-between gap-2 mt-1">
                        <span className="text-slate-500 text-[11px]">عبوات:</span>
                        <input
                          type="number"
                          min="0"
                          value={fb.packageQuantity}
                          onChange={(e) => handleFlavorQuantityChange(idx, Number(e.target.value))}
                          className="w-14 h-7 text-center font-bold bg-white border border-slate-300 rounded text-xs"
                        />
                        <span className="text-[10px] font-semibold text-emerald-700">
                          {fb.packageQuantity * factor} حبة
                        </span>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}

            {/* ── Row D: Target Sale Price, Profit Analytics & Add Button ── */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-6 gap-2.5 pt-2 border-t border-emerald-200/60 items-end">
              {/* Target Sale Price */}
              <div className="col-span-1 lg:col-span-1">
                <label className="block text-xs font-bold text-slate-800 mb-1">
                  سعر البيع المحدد (د.ل) *
                </label>
                <Input
                  type="number"
                  step="0.01"
                  min="0"
                  value={targetSalePrice}
                  onChange={(e) => setTargetSalePrice(Number(e.target.value))}
                  disabled={!selectedProduct}
                  className="h-9 font-bold text-slate-900"
                  isLtr
                />
              </div>

              {/* Profit from Current Cost Indicator */}
              <div className="p-2 rounded-lg bg-emerald-50 border border-emerald-200 h-9 flex items-center justify-between col-span-1 lg:col-span-1">
                <span className="text-[10px] text-emerald-800 font-medium">الربح من التكلفة:</span>
                <span
                  className={`text-xs font-extrabold num-ltr ${
                    profitFromCurrentCost >= 0 ? 'text-emerald-700' : 'text-rose-600'
                  }`}
                >
                  {profitFromCurrentCost >= 0 ? `+${profitFromCurrentCost.toFixed(2)}` : profitFromCurrentCost.toFixed(2)}{' '}
                  د.ل
                  <span className="text-[10px] font-medium mr-1">
                    ({profitMarginCurrentPercentage.toFixed(1)}%)
                  </span>
                </span>
              </div>

              {/* Profit from Average Cost Indicator */}
              <div className="p-2 rounded-lg bg-indigo-50 border border-indigo-200 h-9 flex items-center justify-between col-span-1 lg:col-span-1">
                <span className="text-[10px] text-indigo-800 font-medium">الربح من المتوسط:</span>
                <span
                  className={`text-xs font-extrabold num-ltr ${
                    profitFromAverageCost >= 0 ? 'text-indigo-700' : 'text-rose-600'
                  }`}
                >
                  {profitFromAverageCost >= 0 ? `+${profitFromAverageCost.toFixed(2)}` : profitFromAverageCost.toFixed(2)}{' '}
                  د.ل
                  <span className="text-[10px] font-medium mr-1">
                    ({profitMarginAveragePercentage.toFixed(1)}%)
                  </span>
                </span>
              </div>

              {/* Item Discount */}
              <div>
                <label className="block text-xs font-semibold text-slate-700 mb-1">خصم البند (د.ل)</label>
                <Input
                  type="number"
                  step="0.01"
                  min="0"
                  value={itemDiscount}
                  onChange={(e) => setItemDiscount(Number(e.target.value))}
                  disabled={!selectedProduct}
                  className="h-9 text-center"
                  isLtr
                />
              </div>

              {/* Add Item Button */}
              <div className="col-span-2">
                <Button
                  variant="primary"
                  size="md"
                  leftIcon={<IconPlus className="w-4 h-4" />}
                  onClick={handleAddItem}
                  disabled={!selectedProduct}
                  className="w-full h-9 font-bold bg-emerald-700 hover:bg-emerald-800 text-white shadow-xs"
                >
                  إضافة البند للفاتورة
                </Button>
              </div>
            </div>
          </div>

          {/* ════════ 3. ITEMS DATA GRID ════════ */}
          <div className="border border-slate-200 rounded-xl overflow-hidden shadow-2xs">
            <div className="p-2.5 bg-slate-50 border-b border-slate-200 flex items-center justify-between">
              <span className="text-xs font-bold text-slate-800 flex items-center gap-1.5">
                <IconFileInvoice className="w-4 h-4 text-emerald-700" />
                بنود الفاتورة المسجلة ({items.length})
              </span>
              {items.length > 0 && (
                <span className="text-xs font-bold text-emerald-800">
                  إجمالي القطع المستلمة: {items.reduce((s, i) => s + i.quantity, 0)} قطعة
                </span>
              )}
            </div>

            <div className="overflow-x-auto max-h-64">
              <table className="w-full text-right text-xs">
                <thead className="bg-slate-100/70 border-b border-slate-200 text-slate-600 font-semibold sticky top-0">
                  <tr>
                    <th className="py-2.5 px-3">اسم الصنف</th>
                    <th className="py-2.5 px-3">تفصيل النكهات</th>
                    <th className="py-2.5 px-3">طريقة التنزيل</th>
                    <th className="py-2.5 px-2 text-center">إجمالي القطع</th>
                    <th className="py-2.5 px-2 text-center">سعر شراء القطعة</th>
                    <th className="py-2.5 px-2 text-center">سعر البيع</th>
                    <th className="py-2.5 px-2 text-center">الخصم</th>
                    <th className="py-2.5 px-3 text-center font-bold">إجمالي البند</th>
                    <th className="py-2.5 px-2 text-center">نسبة الربح</th>
                    <th className="py-2.5 px-2 text-center w-10"></th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {items.length === 0 ? (
                    <tr>
                      <td colSpan={10} className="py-8 text-center text-slate-400">
                        لم يتم إضافة أي بنود بعد — امسح الباركود واضغط "إضافة البند للفاتورة"
                      </td>
                    </tr>
                  ) : (
                    items.map((item, idx) => (
                      <tr key={idx} className="hover:bg-slate-50/70 transition-colors">
                        <td className="py-2.5 px-3 font-bold text-slate-900">{item.productName}</td>
                        <td className="py-2.5 px-3 text-emerald-800 text-[11px] font-medium max-w-xs truncate">
                          {item.flavorsSummaryText}
                        </td>
                        <td className="py-2.5 px-3 text-slate-600 text-[11px]">
                          {item.packagingSummary}
                        </td>
                        <td className="py-2.5 px-2 text-center font-bold text-slate-800">
                          {item.quantity}
                        </td>
                        <td className="py-2.5 px-2 text-center font-mono num-ltr">
                          {item.unitPrice.toFixed(2)} د.ل
                        </td>
                        <td className="py-2.5 px-2 text-center font-mono text-indigo-700 num-ltr">
                          {item.salePrice.toFixed(2)} د.ل
                        </td>
                        <td className="py-2.5 px-2 text-center font-mono text-slate-500 num-ltr">
                          {item.discountAmount > 0 ? `${item.discountAmount.toFixed(2)} د.ل` : '-'}
                        </td>
                        <td className="py-2.5 px-3 text-center font-extrabold text-emerald-900 font-mono num-ltr">
                          {item.lineTotal.toFixed(2)} د.ل
                        </td>
                        <td className="py-2.5 px-2 text-center font-bold text-emerald-700">
                          {item.profitMarginPercentage.toFixed(1)}%
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

          {/* ════════ 4. FINANCIAL TOTALS SUMMARY ════════ */}
          <div className="p-3.5 bg-slate-50 border border-slate-200 rounded-xl">
            <div className="grid grid-cols-2 sm:grid-cols-6 gap-3 items-center">
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

              <Input
                label="الضريبة"
                type="number"
                step="0.0001"
                min="0"
                value={taxAmount}
                onChange={(e) => setTaxAmount(Number(e.target.value))}
                isLtr
              />

              <div className="col-span-2 sm:col-span-1">
                <span className="block text-[11px] font-bold text-emerald-800">الصافي المطلوب:</span>
                <span className="text-lg font-black text-emerald-700 num-ltr">
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
        </div>
      </Modal>

      {/* ── Sub-dialogs ── */}
      {canCreateProduct && <QuickProductModal
        isOpen={isQuickProductOpen}
        onClose={() => setIsQuickProductOpen(false)}
        prefillBarcode={searchBarcode}
        categories={categories}
        units={units}
        onProductCreated={(prod) => {
          handleProductLoaded(prod, prod.barCodes?.[0]?.barCode || searchBarcode);
          setIsQuickProductOpen(false);
        }}
      />}

      {canCreateUnit && <QuickUnitModal
        isOpen={isQuickUnitOpen}
        onClose={() => setIsQuickUnitOpen(false)}
        onUnitCreated={(newUnit) => {
          setUnits((prev) => [...prev, newUnit]);
          const newOpt: PackagingUnitOption = {
            name: newUnit.name,
            conversionFactor: 1,
            displayText: `${newUnit.name} (1 قطعة)`,
          };
          setPackagingOptions((prev) => [newOpt, ...prev]);
          setSelectedUnitOption(newOpt);
          setUnitsPerPackage(1);
          setIsQuickUnitOpen(false);
        }}
      />}
    </>
  );
};

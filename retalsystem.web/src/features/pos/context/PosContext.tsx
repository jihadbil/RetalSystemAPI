import { useRequestGate } from '../../../shared/hooks/useRequestGate';
import { loadPendingCheckout, savePendingCheckout, settlePendingCheckout, withCheckoutLock, type PendingCheckout } from '../services/pendingCheckout';
import React, { createContext, useContext, useState, useEffect, useCallback, useMemo, useRef } from 'react';
import type { CartItem, PosDraft, QuickProductItem, ReceiptData } from '../types/pos.types';
import { PaymentMethod } from '../types/pos.types';
import { posService } from '../services/posService';
import type { PosDeviceContext, PosPaymentAccount } from '../services/posService';
import { soundService } from '../../../shared/services/soundService';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import { useThermalPrint } from '../../../shared/hooks/useThermalPrint';
import { stockService } from '../../stock/services/stockService';
import type { WarehouseDto } from '../../stock/types/stock.types';
import { Permissions } from '../../../shared/auth/permissions';
import { createDocumentNumber } from '../../../shared/utils/documentNumber';
import { calculatePosTotals } from '../utils/posTotals';

interface PosContextType {
  cartItems: CartItem[];
  customer: { id?: string; name: string; phone?: string };
  discount: number;
  taxRate: number;
  paidAmount: number;
  paymentMethod: PaymentMethod;
  notes: string;
  heldDrafts: PosDraft[];
  selectedItemIndex: number;
  lastReceipt: ReceiptData | null;
  isCheckingOut: boolean;
  pendingCheckout: PendingCheckout | null;
  recoveryError: string;
  recoverPendingSale: () => Promise<boolean>;
  deviceContext: PosDeviceContext | null;
  warehouses: WarehouseDto[];
  selectedWarehouseId: string;
  paymentAccounts: PosPaymentAccount[];
  paymentSafeId: string;
  setPaymentSafeId: (id: string, option?: PosPaymentAccount) => void;
  setSelectedWarehouseId: (warehouseId: string, option?: WarehouseDto) => void;
  pairDevice: (pairingCode: string) => Promise<boolean>;
  openCashSession: (openingFloat: number) => Promise<boolean>;
  closeCashSession: (countedBalance: number, notes?: string) => Promise<boolean>;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  grandTotal: number;
  changeAmount: number;
  remainingAmount: number;
  // أفعال السلة
  addItemByBarcode: (barcode: string) => Promise<boolean>;
  addCartItem: (product: QuickProductItem | CartItem) => void;
  updateQuantity: (productId: string, newQty: number) => void;
  setExactQuantityForSelected: (qty: number) => void;
  updateItemDiscount: (productId: string, discount: number) => void;
  removeItem: (productId: string) => void;
  clearCart: () => void;
  setSelectedItemIndex: (idx: number) => void;
  // أفعال الحسابات
  setCustomer: (cust: { id?: string; name: string; phone?: string }) => void;
  setDiscount: (discount: number) => void;
  setPaidAmount: (amount: number) => void;
  setPaymentMethod: (method: PaymentMethod) => void;
  setNotes: (notes: string) => void;
  setExactCash: () => void;
  setCardPayment: () => void;
  // أفعال المسودات (F5 / F6)
  holdCurrentOrder: () => boolean;
  recallDraft: (draftId: string) => void;
  deleteDraft: (draftId: string) => void;
  // إتمام البيع والطباعة (F10)
  completeSale: () => Promise<boolean>;
  printLastReceipt: () => void;
}

const PosContext = createContext<PosContextType | null>(null);

export const PosProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { user, currentBranchId, currentTenantId, hasPermission, isLocked } = useAuth();
  const { showSuccess, showError, showWarning } = useToast();
  const { printDocument } = useThermalPrint();
  const [deviceContext, setDeviceContext] = useState<PosDeviceContext | null>(null);
  const deviceGate = useRequestGate();
  const draftScope = `${currentTenantId || 'no-tenant'}:${deviceContext?.terminal.branchId || currentBranchId || 'no-branch'}:${user?.id || 'anonymous'}`;
  const draftScopeRef = useRef(draftScope);
  const skipNextDraftSaveRef = useRef(false);
  const checkoutInFlight = useRef(false);
  const cartGeneration = useRef(0);
  const [pendingCheckout, setPendingCheckout] = useState<PendingCheckout | null>(null);
  const [recoveryError, setRecoveryError] = useState('');

  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [customer, setCustomer] = useState<{ id?: string; name: string; phone?: string }>({
    name: 'عميل نقدي',
  });
  const [discount, setDiscount] = useState<number>(0);
  const [taxRate] = useState<number>(0); // النسبة الضريبية الافتراضية
  const [paidAmount, setPaidAmount] = useState<number>(0);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.Cash);
  const [notes, setNotes] = useState<string>('');
  const [heldDrafts, setHeldDrafts] = useState<PosDraft[]>(() => posService.loadDrafts(draftScope));
  const [selectedItemIndex, setSelectedItemIndex] = useState<number>(0);
  const [lastReceipt, setLastReceipt] = useState<ReceiptData | null>(null);
  const [isCheckingOut, setIsCheckingOut] = useState(false);
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [selectedWarehouseId, setSelectedWarehouseId] = useState('');
  const [paymentAccounts, setPaymentAccounts] = useState<PosPaymentAccount[]>([]);
  const [paymentSafeId, setPaymentSafeId] = useState('');
  useEffect(() => {
    let cancelled = false;
    const branchId = deviceContext?.terminal.branchId || currentBranchId;
    setPaymentAccounts([]);
    setPaymentSafeId('');
    if (branchId) void posService.getPaymentAccounts(branchId).then((accounts) => {
      if (cancelled) return;
      setPaymentAccounts(accounts);
      if (accounts.length === 1) setPaymentSafeId(accounts[0].id);
    }).catch(() => { if (!cancelled) showError('تعذر تحميل حسابات التحصيل المصرفي'); });
    return () => { cancelled = true; };
  }, [currentBranchId, deviceContext?.terminal.branchId, showError]);

  const refreshDeviceContext = useCallback(async () => {
    const isCurrent = deviceGate.begin();
    try { const context = await posService.getDeviceContext(); if (isCurrent()) setDeviceContext(context); }
    catch { if (isCurrent()) setDeviceContext(null); }
  }, [deviceGate]);

  useEffect(() => {
    setDeviceContext(null);
    const timer = window.setTimeout(() => { void refreshDeviceContext(); }, 0);
    return () => { window.clearTimeout(timer); deviceGate.invalidate(); };
  }, [refreshDeviceContext, currentTenantId, user?.id, deviceGate]);

  const pairDevice = useCallback(async (pairingCode: string) => {
    if (!currentTenantId) { showError('تعذر الربط', 'لا يوجد مستأجر حالي في جلسة الدخول'); return false; }
    try { await posService.claimDevice(currentTenantId, pairingCode); await refreshDeviceContext(); return true; }
    catch (err) { showError('فشل ربط الجهاز', err instanceof Error ? err.message : 'رمز الربط غير صالح'); return false; }
  }, [currentTenantId, refreshDeviceContext, showError]);

  const openCashSession = useCallback(async (openingFloat: number) => {
    if (!hasPermission(Permissions.Pos.OpenSession)) {
      showWarning('الإجراء غير مسموح', 'لا تملك صلاحية فتح وردية نقدية');
      return false;
    }
    try { await posService.openCashSession(openingFloat); await refreshDeviceContext(); return true; }
    catch (err) { showError('فشل فتح الوردية', err instanceof Error ? err.message : 'تعذر فتح الوردية'); return false; }
  }, [hasPermission, refreshDeviceContext, showError, showWarning]);

  const closeCashSession = useCallback(async (countedBalance: number, closingNotes?: string) => {
    if (!hasPermission(Permissions.Pos.CloseSession)) {
      showWarning('الإجراء غير مسموح', 'لا تملك صلاحية إغلاق الوردية النقدية');
      return false;
    }
    try { await posService.closeCashSession(countedBalance, closingNotes); await refreshDeviceContext(); return true; }
    catch (err) { showError('فشل إغلاق الوردية', err instanceof Error ? err.message : 'تعذر إغلاق الوردية'); return false; }
  }, [hasPermission, refreshDeviceContext, showError, showWarning]);

  const applyDiscount = useCallback((value: number) => {
    if (!hasPermission(Permissions.Pos.ApplyDiscount)) {
      showWarning('الإجراء غير مسموح', 'لا تملك صلاحية تطبيق خصم في نقطة البيع');
      return;
    }
    setDiscount(value);
  }, [hasPermission, showWarning]);

  useEffect(() => {
    if (draftScopeRef.current !== draftScope) {
      draftScopeRef.current = draftScope;
      skipNextDraftSaveRef.current = true;
      setHeldDrafts(posService.loadDrafts(draftScope));
      cartGeneration.current++;
      setCartItems([]); setCustomer({ name: 'عميل نقدي' });
      setDiscount(0); setPaidAmount(0); setNotes(''); setLastReceipt(null);
    }
  }, [draftScope]);

  useEffect(() => {
    const refresh = () => {
      try { setPendingCheckout(loadPendingCheckout(draftScope)); setRecoveryError(''); }
      catch (error) { setRecoveryError(error instanceof Error ? error.message : 'تعذر قراءة البيع المعلق'); }
    };
    refresh();
    window.addEventListener('storage', refresh);
    return () => window.removeEventListener('storage', refresh);
  }, [draftScope]);

  // مزامنة المسودات في تخزين معزول لكل مستأجر وفرع ومستخدم.
  useEffect(() => {
    if (skipNextDraftSaveRef.current) {
      skipNextDraftSaveRef.current = false;
      return;
    }
    posService.saveDrafts(draftScope, heldDrafts);
  }, [draftScope, heldDrafts]);

  useEffect(() => {
    const branchId = deviceContext?.terminal.branchId || currentBranchId;
    if (!branchId) {
      setWarehouses([]);
      setSelectedWarehouseId('');
      return;
    }

    let isCancelled = false;
    void stockService.getWarehouses()
      .then((allWarehouses) => {
        if (isCancelled) return;
        const branchWarehouses = allWarehouses.filter((warehouse) => warehouse.isActive && warehouse.branchId === branchId);
        setWarehouses(branchWarehouses);
        setSelectedWarehouseId((current) => {
          if (branchWarehouses.some((warehouse) => warehouse.id === current)) return current;
          return branchWarehouses.length === 1 ? branchWarehouses[0].id : '';
        });
      })
      .catch(() => {
        if (!isCancelled) {
          setWarehouses([]);
          setSelectedWarehouseId('');
        }
      });

    return () => { isCancelled = true; };
  }, [currentBranchId, deviceContext?.terminal.branchId]);

  const selectWarehouse = useCallback((warehouseId: string, option?: WarehouseDto) => {
    const branchId = deviceContext?.terminal.branchId || currentBranchId;
    if (option) {
      if (!option.isActive || option.branchId !== branchId) return;
      setWarehouses((previous) => [...previous.filter((warehouse) => warehouse.id !== option.id), option]);
    }
    setSelectedWarehouseId(warehouseId);
  }, [deviceContext?.terminal.branchId, currentBranchId]);

  const selectPaymentAccount = useCallback((id: string, option?: PosPaymentAccount) => {
    if (option) {
      if (option.branchId !== (deviceContext?.terminal.branchId || currentBranchId)) return;
      setPaymentAccounts((previous) => [...previous.filter((account) => account.id !== option.id), option]);
    }
    setPaymentSafeId(id);
  }, [deviceContext?.terminal.branchId, currentBranchId]);

  // الحسابات المالية اللحظية
  const { subtotal, discountAmount, taxAmount, grandTotal, changeAmount, remainingAmount } = useMemo(
    () => calculatePosTotals(cartItems, discount, taxRate, paidAmount),
    [cartItems, discount, taxRate, paidAmount],
  );

  // إضافة صنف إلى السلة مع قاعدة الصالة: التجميع بمفتاح ProductId
  const addCartItem = useCallback((product: QuickProductItem | CartItem) => {
    const pId = 'productId' in product ? product.productId : (product as CartItem).productId;
    const price = 'price' in product ? product.price : (product as CartItem).unitPrice;
    const name = 'name' in product ? product.name : (product as CartItem).productName;
    const barCode = product.barCode || '';
    const productBarCodeId = 'productBarCodeId' in product ? product.productBarCodeId : undefined;
    const lineId = ('lineId' in product && product.lineId) || productBarCodeId || barCode || pId;

    setCartItems((prevItems) => {
      const existingIdx = prevItems.findIndex(
        (item) => (item.lineId || item.productBarCodeId || item.barCode || item.productId) === lineId
      );
      if (existingIdx >= 0) {
        const updated = [...prevItems];
        const current = updated[existingIdx];
        const newQty = current.quantity + 1;
        updated[existingIdx] = {
          ...current,
          quantity: newQty,
          lineTotal: Math.max(0, newQty * current.unitPrice - current.discountAmount),
        };
        setSelectedItemIndex(existingIdx);
        return updated;
      }

      const newItem: CartItem = {
        lineId,
        productId: pId,
        productBarCodeId,
        productName: name,
        code: product.code,
        barCode: barCode,
        unitName: 'unitName' in product ? (product as CartItem).unitName : 'قطعة',
        unitPrice: price,
        quantity: 1,
        discountAmount: 0,
        lineTotal: price,
      };

      setSelectedItemIndex(prevItems.length);
      return [...prevItems, newItem];
    });

    soundService.playScanBeep();
  }, []);

  // مسح وإضافة بالباركود
  const addItemByBarcode = useCallback(async (barcode: string): Promise<boolean> => {
    if (!barcode.trim()) return false;
    const scope = draftScope;
    const generation = cartGeneration.current;
    try {
      const item = await posService.findProductByBarcode(barcode.trim());
      if (scope !== draftScopeRef.current || generation !== cartGeneration.current || checkoutInFlight.current) return false;
      if (item) {
        addCartItem(item);
        return true;
      } else {
        soundService.playErrorBuzz();
        showWarning('الصنف غير موجود', `لم يتم العثور على أي صنف بالباركود: ${barcode}`);
        return false;
      }
    } catch (err: unknown) {
      soundService.playErrorBuzz();
      const msg = err instanceof Error ? err.message : 'فشل البحث عن الصنف';
      showError('خطأ في الاتصال', msg);
      return false;
    }
  }, [addCartItem, showWarning, showError, draftScope]);

  // تعديل كمية صنف
  const updateQuantity = useCallback((lineId: string, newQty: number) => {
    if (newQty <= 0) {
      setCartItems((prev) => prev.filter(
        (item) => (item.lineId || item.productBarCodeId || item.barCode || item.productId) !== lineId
      ));
      return;
    }
    setCartItems((prev) =>
      prev.map((item) => {
        if ((item.lineId || item.productBarCodeId || item.barCode || item.productId) === lineId) {
          return {
            ...item,
            quantity: newQty,
            lineTotal: Math.max(0, newQty * item.unitPrice - item.discountAmount),
          };
        }
        return item;
      })
    );
  }, []);

  // تعديل كمية الصنف النشط حالياً
  const setExactQuantityForSelected = useCallback((qty: number) => {
    setCartItems((prev) => {
      if (selectedItemIndex < 0 || selectedItemIndex >= prev.length) return prev;
      const target = prev[selectedItemIndex];
      if (qty <= 0) {
        return prev.filter((_, idx) => idx !== selectedItemIndex);
      }
      const updated = [...prev];
      updated[selectedItemIndex] = {
        ...target,
        quantity: qty,
        lineTotal: Math.max(0, qty * target.unitPrice - target.discountAmount),
      };
      return updated;
    });
  }, [selectedItemIndex]);

  // خصم صنف فردي
  const updateItemDiscount = useCallback((lineId: string, discountAmt: number) => {
    setCartItems((prev) =>
      prev.map((item) => {
        if ((item.lineId || item.productBarCodeId || item.barCode || item.productId) === lineId) {
          const validDisc = Math.max(0, Math.min(discountAmt, item.quantity * item.unitPrice));
          return {
            ...item,
            discountAmount: validDisc,
            lineTotal: Math.max(0, item.quantity * item.unitPrice - validDisc),
          };
        }
        return item;
      })
    );
  }, []);

  // حذف صنف من السلة
  const removeItem = useCallback((lineId: string) => {
    setCartItems((prev) => prev.filter(
      (item) => (item.lineId || item.productBarCodeId || item.barCode || item.productId) !== lineId
    ));
  }, []);

  // تفريغ السلة
  const clearCart = useCallback(() => {
    cartGeneration.current++;
    setCartItems([]);
    setDiscount(0);
    setPaidAmount(0);
    setNotes('');
    setCustomer({ name: 'عميل نقدي' });
  }, []);

  // تعليق الطلب الحالي في المسودات (F5)
  const holdCurrentOrder = useCallback((): boolean => {
    if (cartItems.length === 0) {
      showWarning('السلة فارغة', 'لا توجد أصناف في السلة لتعليقها');
      return false;
    }

    const newDraft: PosDraft = {
      id: createDocumentNumber('DRAFT'),
      savedAt: new Date().toISOString(),
      branchId: currentBranchId || undefined,
      customerId: customer.id,
      customerName: customer.name,
      items: [...cartItems],
      discount,
      tax: taxAmount,
      paid: paidAmount,
      paymentMethod,
      notes,
      total: grandTotal,
      label: `${new Date().toLocaleTimeString('ar-LY', { hour: '2-digit', minute: '2-digit' })} · ${
        customer.name || 'عميل نقدي'
      } · ${cartItems.length} أصناف · ${grandTotal.toFixed(2)} د.ل`,
    };

    setHeldDrafts((prev) => [newDraft, ...prev]);
    clearCart();
    showSuccess('تم تعليق الطلب (F5)', `تم حفظ المسودة بنجاح (#${newDraft.id.slice(-4)})`);
    return true;
  }, [cartItems, currentBranchId, customer, discount, taxAmount, paidAmount, paymentMethod, notes, grandTotal, clearCart, showSuccess, showWarning]);

  // استعادة الطلب المعلق المحدد (F6)
  const recallDraft = useCallback((draftId: string) => {
    const draft = heldDrafts.find((d) => d.id === draftId);
    if (!draft) return;

    if (cartItems.length > 0) {
      showWarning('تنبيه', 'السلة الحالية تحتوي على أصناف نشطة، يرجى تعليقها أولاً (F5) قبل استعادة طلب آخر');
      return;
    }

    setCartItems(draft.items);
    setCustomer({ id: draft.customerId, name: draft.customerName || 'عميل نقدي' });
    setDiscount(draft.discount || 0);
    setPaidAmount(draft.paid || 0);
    setPaymentMethod(draft.paymentMethod || PaymentMethod.Cash);
    setNotes(draft.notes || '');

    // حذف المسودة المستعادة من قائمة المعلقات
    setHeldDrafts((prev) => prev.filter((d) => d.id !== draftId));
    showSuccess('تمت استعادة الطلب (F6)', `تم تحميل المسودة بنجاح`);
  }, [heldDrafts, cartItems.length, showSuccess, showWarning]);

  const deleteDraft = useCallback((draftId: string) => {
    setHeldDrafts((prev) => prev.filter((d) => d.id !== draftId));
    showSuccess('تم الحذف', 'تم حذف المسودة المعلقة');
  }, [showSuccess]);

  // وضع المبلغ المستلم مساوياً للإجمالي (F8)
  const setExactCash = useCallback(() => {
    setPaidAmount(grandTotal);
    setPaymentMethod(PaymentMethod.Cash);
  }, [grandTotal]);

  // تحويل طريقة الدفع للبطاقة المصرفية فوراً (F9)
  const setCardPayment = useCallback(() => {
    setPaymentMethod(PaymentMethod.Card);
    setPaidAmount(grandTotal);
  }, [grandTotal]);

  const recoverPendingSale = useCallback(async (): Promise<boolean> => {
    if (isLocked || checkoutInFlight.current || !hasPermission(Permissions.Pos.CreateSale)) return false;
    const scope = draftScope;
    checkoutInFlight.current = true;
    cartGeneration.current++;
    setIsCheckingOut(true);
    try {
      return await withCheckoutLock(scope, async () => {
        const pending = loadPendingCheckout(scope);
        if (!pending) { setPendingCheckout(null); return true; }
        const receipt = await settlePendingCheckout(pending, posService.checkout, false);
        if (draftScopeRef.current !== scope) return true;
        setLastReceipt(receipt); setPendingCheckout(null); setRecoveryError('');
        const currentItems = cartItems.map((item) => ({ productId: item.productId, productBarCodeId: item.productBarCodeId,
          barCode: item.barCode, quantity: item.quantity, unitPrice: item.unitPrice, discountAmount: item.discountAmount, totalPrice: item.lineTotal }));
        if (JSON.stringify(currentItems) === JSON.stringify(pending.request.items)) clearCart();
        showSuccess('تم استعادة نتيجة البيع', `فاتورة رقم: ${receipt.invoiceNumber}`);
        return true;
      });
    } catch (error) {
      if (draftScopeRef.current === scope) showError('تعذر حسم نتيجة البيع', error instanceof Error ? error.message : 'أعد المحاولة بنفس رقم العملية');
      return false;
    } finally { checkoutInFlight.current = false; setIsCheckingOut(false); }
  }, [draftScope, hasPermission, isLocked, showSuccess, showError, cartItems, clearCart]);

  // إتمام البيع والطباعة (F10)
  const completeSale = useCallback(async (): Promise<boolean> => {
    if (isLocked || isCheckingOut || checkoutInFlight.current) return false;
    if (pendingCheckout || recoveryError) {
      showWarning('محاولة بيع معلقة', 'استعد نتيجة المحاولة السابقة قبل بدء بيع جديد');
      return false;
    }
    if (!hasPermission(Permissions.Pos.CreateSale)) {
      showWarning('الإجراء غير مسموح', 'لا تملك صلاحية إتمام المبيعات من نقطة البيع');
      return false;
    }

    if (discountAmount > 0 && !hasPermission(Permissions.Pos.ApplyDiscount)) {
      showWarning('الخصم غير مسموح', 'أزل الخصم أو اطلب من مستخدم مخوّل إتمام العملية');
      return false;
    }

    if (cartItems.length === 0) {
      soundService.playErrorBuzz();
      showWarning('السلة فارغة', 'أضف أصنافاً أولاً لإتمام الفاتورة');
      return false;
    }

    if (paymentMethod === PaymentMethod.Cash && paidAmount < grandTotal) {
      soundService.playErrorBuzz();
      showWarning('المبلغ غير كافٍ', `المبلغ المستلم (${paidAmount.toFixed(2)} د.ل) أقل من الإجمالي (${grandTotal.toFixed(2)} د.ل)`);
      return false;
    }

    if (paymentMethod === PaymentMethod.Cash && !deviceContext?.currentSession) {
      showWarning('وردية غير مفتوحة', 'اربط الجهاز بنقطة البيع وافتح وردية درج النقود قبل البيع النقدي');
      return false;
    }

    const effectiveBranchId = deviceContext?.terminal.branchId || currentBranchId;
    if (paymentMethod !== PaymentMethod.Cash && grandTotal > 0 && !paymentAccounts.some((account) => account.id === paymentSafeId)) {
      showWarning('حساب التحصيل غير محدد', 'اختر الحساب المصرفي الذي سيستقبل مبلغ البيع');
      return false;
    }
    if (!effectiveBranchId) {
      showWarning('الفرع غير محدد', 'حدد فرع نقطة البيع قبل إتمام العملية');
      return false;
    }

    if (!selectedWarehouseId || !warehouses.some((warehouse) => warehouse.id === selectedWarehouseId)) {
      showWarning('المستودع غير محدد', 'اختر مستودعاً نشطاً تابعاً لفرع نقطة البيع');
      return false;
    }

    checkoutInFlight.current = true;
    cartGeneration.current++;
    setIsCheckingOut(true);
    try {
      const invoicePaidAmount = paymentMethod === PaymentMethod.Cash || paymentMethod === PaymentMethod.Card
        ? grandTotal
        : Math.min(grandTotal, Math.max(0, paidAmount));
      const invoicePayload = {
        branchId: effectiveBranchId,
        warehouseId: selectedWarehouseId,
        customerId: customer.id,
        paymentMethod,
        subTotal: subtotal,
        discountAmount,
        taxAmount,
        totalAmount: grandTotal,
        paidAmount: invoicePaidAmount,
        cashSessionId: paymentMethod === PaymentMethod.Cash ? deviceContext?.currentSession?.id : undefined,
        paymentSafeId: paymentMethod !== PaymentMethod.Cash ? paymentSafeId : undefined,
        notes,
        items: cartItems.map((item) => ({
          productId: item.productId,
          productBarCodeId: item.productBarCodeId,
          barCode: item.barCode,
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          discountAmount: item.discountAmount,
          totalPrice: item.lineTotal,
        })),
      };

      const invoiceNumber = createDocumentNumber('POS');

      // تجهيز بيانات الإيصال الحراري للطباعة
      const receipt: ReceiptData = {
        invoiceNumber,
        date: new Date().toLocaleString('ar-LY'),
        cashierName: user?.fullName || user?.userName || 'الكاشير',
        customerName: customer.name,
        branchName: user?.branchName || 'الفرع الرئيسي',
        tenantName: user?.tenantName || 'نظام ريتال للحلول الذكية',
        items: cartItems.map((item) => ({
          name: item.productName,
          quantity: item.quantity,
          price: item.unitPrice,
          total: item.lineTotal,
        })),
        subtotal,
        discount: discountAmount,
        tax: taxAmount,
        total: grandTotal,
        paid: paymentMethod === PaymentMethod.Card ? grandTotal : paidAmount,
        change: paymentMethod === PaymentMethod.Card ? 0 : changeAmount,
        paymentMethod: paymentMethod === PaymentMethod.Card ? 'بطاقة مصرفية' : 'نقداً',
        qrCodeValue: `${invoiceNumber}|${grandTotal.toFixed(2)}|${new Date().toISOString()}`,
      };

      await withCheckoutLock(draftScope, async () => {
        const pending: PendingCheckout = { version: 1, scope: draftScope,
          request: { ...invoicePayload, invoiceNumber }, receipt };
        savePendingCheckout(pending);
        setPendingCheckout(pending);
        const savedReceipt = await settlePendingCheckout(pending, posService.checkout, true);
        if (draftScopeRef.current !== draftScope) return;
        setLastReceipt(savedReceipt);
        setPendingCheckout(null);
        soundService.playSuccessChime();
        showSuccess('تم إتمام البيع بنجاح (F10)', `فاتورة رقم: ${savedReceipt.invoiceNumber} — الإجمالي: ${grandTotal.toFixed(2)} د.ل`);
        clearCart();
      });
      if (draftScopeRef.current !== draftScope) return true;

      // إطلاق أمر الطباعة
      setTimeout(() => {
        printDocument();
      }, 200);

      return true;
    } catch (err: unknown) {
      soundService.playErrorBuzz();
      const msg = err instanceof Error ? err.message : 'فشل حفظ الفاتورة على الخادم';
      if (draftScopeRef.current === draftScope) {
        try { setPendingCheckout(loadPendingCheckout(draftScope)); }
        catch (error) { setRecoveryError(error instanceof Error ? error.message : 'تعذر قراءة البيع المعلق'); }
        showError('خطأ أثناء حفظ الفاتورة', msg);
      }
      return false;
    } finally {
      checkoutInFlight.current = false;
      setIsCheckingOut(false);
    }
  }, [
    cartItems,
    pendingCheckout,
    recoveryError,
    draftScope,
    isLocked,
    isCheckingOut,
    paymentSafeId,
    paymentAccounts,
    paymentMethod,
    paidAmount,
    grandTotal,
    currentBranchId,
    customer,
    subtotal,
    discountAmount,
    taxAmount,
    notes,
    user,
    changeAmount,
    clearCart,
    printDocument,
    showSuccess,
    showWarning,
    showError,
    deviceContext,
    selectedWarehouseId,
    warehouses,
    hasPermission,
  ]);

  const printLastReceipt = useCallback(() => {
    if (!hasPermission(Permissions.Pos.Reprint)) {
      showWarning('الإجراء غير مسموح', 'لا تملك صلاحية إعادة طباعة الإيصالات');
      return;
    }
    if (lastReceipt) {
      printDocument();
    }
  }, [hasPermission, lastReceipt, printDocument, showWarning]);

  return (
    <PosContext.Provider
      value={{
        cartItems,
        customer,
        discount,
        taxRate,
        paidAmount,
        paymentMethod,
        notes,
        heldDrafts,
        selectedItemIndex,
        lastReceipt,
        isCheckingOut,
        pendingCheckout,
        recoveryError,
        recoverPendingSale,
        deviceContext,
        warehouses,
        selectedWarehouseId,
        paymentAccounts,
        paymentSafeId,
        setPaymentSafeId: selectPaymentAccount,
        setSelectedWarehouseId: selectWarehouse,
        pairDevice,
        openCashSession,
        closeCashSession,
        subtotal,
        discountAmount,
        taxAmount,
        grandTotal,
        changeAmount,
        remainingAmount,
        addItemByBarcode,
        addCartItem,
        updateQuantity,
        setExactQuantityForSelected,
        updateItemDiscount,
        removeItem,
        clearCart,
        setSelectedItemIndex,
        setCustomer,
        setDiscount: applyDiscount,
        setPaidAmount,
        setPaymentMethod,
        setNotes,
        setExactCash,
        setCardPayment,
        holdCurrentOrder,
        recallDraft,
        deleteDraft,
        completeSale,
        printLastReceipt,
      }}
    >
      {children}
    </PosContext.Provider>
  );
};

// This module intentionally exports the provider and its colocated hook.
// eslint-disable-next-line react-refresh/only-export-components
export const usePos = (): PosContextType => {
  const context = useContext(PosContext);
  if (!context) {
    throw new Error('usePos must be used within a PosProvider');
  }
  return context;
};

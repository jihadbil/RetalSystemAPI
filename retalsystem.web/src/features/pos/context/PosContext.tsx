import React, { createContext, useContext, useState, useEffect, useCallback, useMemo } from 'react';
import type { CartItem, PosDraft, QuickProductItem, ReceiptData } from '../types/pos.types';
import { PaymentMethod } from '../types/pos.types';
import { posService } from '../services/posService';
import type { PosDeviceContext } from '../services/posService';
import { soundService } from '../../../shared/services/soundService';
import { useAuth } from '../../../shared/context/AuthContext';
import { useToast } from '../../../shared/context/ToastContext';
import { useThermalPrint } from '../../../shared/hooks/useThermalPrint';

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
  deviceContext: PosDeviceContext | null;
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
  const { user, currentBranchId, currentTenantId } = useAuth();
  const { showSuccess, showError, showWarning } = useToast();
  const { printDocument } = useThermalPrint();

  const [cartItems, setCartItems] = useState<CartItem[]>([]);
  const [customer, setCustomer] = useState<{ id?: string; name: string; phone?: string }>({
    name: 'عميل نقدي',
  });
  const [discount, setDiscount] = useState<number>(0);
  const [taxRate] = useState<number>(0); // النسبة الضريبية الافتراضية
  const [paidAmount, setPaidAmount] = useState<number>(0);
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.Cash);
  const [notes, setNotes] = useState<string>('');
  const [heldDrafts, setHeldDrafts] = useState<PosDraft[]>(() => posService.loadDrafts());
  const [selectedItemIndex, setSelectedItemIndex] = useState<number>(0);
  const [lastReceipt, setLastReceipt] = useState<ReceiptData | null>(null);
  const [isCheckingOut, setIsCheckingOut] = useState(false);
  const [deviceContext, setDeviceContext] = useState<PosDeviceContext | null>(null);

  const refreshDeviceContext = useCallback(async () => {
    try { setDeviceContext(await posService.getDeviceContext()); }
    catch { setDeviceContext(null); }
  }, []);

  useEffect(() => {
    const timer = window.setTimeout(() => { void refreshDeviceContext(); }, 0);
    return () => window.clearTimeout(timer);
  }, [refreshDeviceContext]);

  const pairDevice = useCallback(async (pairingCode: string) => {
    if (!currentTenantId) { showError('تعذر الربط', 'لا يوجد مستأجر حالي في جلسة الدخول'); return false; }
    try { await posService.claimDevice(currentTenantId, pairingCode); await refreshDeviceContext(); return true; }
    catch (err) { showError('فشل ربط الجهاز', err instanceof Error ? err.message : 'رمز الربط غير صالح'); return false; }
  }, [currentTenantId, refreshDeviceContext, showError]);

  const openCashSession = useCallback(async (openingFloat: number) => {
    try { await posService.openCashSession(openingFloat); await refreshDeviceContext(); return true; }
    catch (err) { showError('فشل فتح الوردية', err instanceof Error ? err.message : 'تعذر فتح الوردية'); return false; }
  }, [refreshDeviceContext, showError]);

  const closeCashSession = useCallback(async (countedBalance: number, closingNotes?: string) => {
    try { await posService.closeCashSession(countedBalance, closingNotes); await refreshDeviceContext(); return true; }
    catch (err) { showError('فشل إغلاق الوردية', err instanceof Error ? err.message : 'تعذر إغلاق الوردية'); return false; }
  }, [refreshDeviceContext, showError]);

  // مزامنة المسودات في التخزين المحلي
  useEffect(() => {
    posService.saveDrafts(heldDrafts);
  }, [heldDrafts]);

  // الحسابات المالية اللحظية
  const subtotal = useMemo(() => {
    return cartItems.reduce((acc, item) => acc + item.lineTotal, 0);
  }, [cartItems]);

  const discountAmount = useMemo(() => {
    return Math.min(subtotal, Math.max(0, discount));
  }, [subtotal, discount]);

  const taxAmount = useMemo(() => {
    return Math.round((subtotal - discountAmount) * (taxRate / 100) * 100) / 100;
  }, [subtotal, discountAmount, taxRate]);

  const grandTotal = useMemo(() => {
    return Math.max(0, subtotal - discountAmount + taxAmount);
  }, [subtotal, discountAmount, taxAmount]);

  // المبلغ المستلم لا يتغير تلقائياً مع الإجمالي (قاعدة design.md)
  const changeAmount = useMemo(() => {
    return Math.max(0, paidAmount - grandTotal);
  }, [paidAmount, grandTotal]);

  const remainingAmount = useMemo(() => {
    return Math.max(0, grandTotal - paidAmount);
  }, [paidAmount, grandTotal]);

  // إضافة صنف إلى السلة مع قاعدة الصالة: التجميع بمفتاح ProductId
  const addCartItem = useCallback((product: QuickProductItem | CartItem) => {
    const pId = 'productId' in product ? product.productId : (product as CartItem).productId;
    const price = 'price' in product ? product.price : (product as CartItem).unitPrice;
    const name = 'name' in product ? product.name : (product as CartItem).productName;
    const barCode = product.barCode || '';

    setCartItems((prevItems) => {
      const existingIdx = prevItems.findIndex((item) => item.productId === pId);
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
        productId: pId,
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
    try {
      const item = await posService.findProductByBarcode(barcode.trim());
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
  }, [addCartItem, showWarning, showError]);

  // تعديل كمية صنف
  const updateQuantity = useCallback((productId: string, newQty: number) => {
    if (newQty <= 0) {
      setCartItems((prev) => prev.filter((item) => item.productId !== productId));
      return;
    }
    setCartItems((prev) =>
      prev.map((item) => {
        if (item.productId === productId) {
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
  const updateItemDiscount = useCallback((productId: string, discountAmt: number) => {
    setCartItems((prev) =>
      prev.map((item) => {
        if (item.productId === productId) {
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
  const removeItem = useCallback((productId: string) => {
    setCartItems((prev) => prev.filter((item) => item.productId !== productId));
  }, []);

  // تفريغ السلة
  const clearCart = useCallback(() => {
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
      id: `${Date.now()}`,
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

  // إتمام البيع والطباعة (F10)
  const completeSale = useCallback(async (): Promise<boolean> => {
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

    setIsCheckingOut(true);
    try {
      const invoicePayload = {
        branchId: currentBranchId,
        customerId: customer.id,
        paymentMethod,
        subTotal: subtotal,
        discountAmount,
        taxAmount,
        totalAmount: grandTotal,
        paidAmount: paymentMethod === PaymentMethod.Card ? grandTotal : paidAmount,
        cashSessionId: paymentMethod === PaymentMethod.Cash ? deviceContext?.currentSession?.id : undefined,
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

      const result = await posService.checkout(invoicePayload);

      // تجهيز بيانات الإيصال الحراري للطباعة
      const receipt: ReceiptData = {
        invoiceNumber: result.invoiceNumber,
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
        qrCodeValue: `${result.invoiceNumber}|${grandTotal.toFixed(2)}|${new Date().toISOString()}`,
      };

      setLastReceipt(receipt);
      soundService.playSuccessChime();
      showSuccess('تم إتمام البيع بنجاح (F10)', `فاتورة رقم: ${result.invoiceNumber} — الإجمالي: ${grandTotal.toFixed(2)} د.ل`);

      // إفراغ السلة بعد البيع
      clearCart();

      // إطلاق أمر الطباعة
      setTimeout(() => {
        printDocument();
      }, 200);

      return true;
    } catch (err: unknown) {
      soundService.playErrorBuzz();
      const msg = err instanceof Error ? err.message : 'فشل حفظ الفاتورة على الخادم';
      showError('خطأ أثناء حفظ الفاتورة', msg);
      return false;
    } finally {
      setIsCheckingOut(false);
    }
  }, [
    cartItems,
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
  ]);

  const printLastReceipt = useCallback(() => {
    if (lastReceipt) {
      printDocument();
    }
  }, [lastReceipt, printDocument]);

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
        deviceContext,
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
        setDiscount,
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

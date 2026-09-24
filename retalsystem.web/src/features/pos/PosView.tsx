import React, { useState, useRef, useEffect } from 'react';
import { PosProvider, usePos } from './context/PosContext';
import { PosCart } from './components/PosCart';
import { PosPaymentPanel } from './components/PosPaymentPanel';
import { QuickCustomerModal } from './components/QuickCustomerModal';
import { QuickItemsModal } from './components/QuickItemsModal';
import { PosDraftsModal } from './components/PosDraftsModal';
import { QuantityNumpadModal } from './components/QuantityNumpadModal';
import { ReceiptPrintView } from './components/ReceiptPrintView';
import { ConfirmDialog } from '../../shared/components/ui/ConfirmDialog';
import { Modal } from '../../shared/components/ui/Modal';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { useHotkeys } from '../../shared/hooks/useHotkeys';
import {
  IconBarcode,
  IconDiscount2,
  IconPackage,
  IconUser,
  IconClock,
} from '@tabler/icons-react';

const PosContent: React.FC = () => {
  const {
    cartItems,
    selectedItemIndex,
    customer,
    setCustomer,
    addItemByBarcode,
    addCartItem,
    setExactQuantityForSelected,
    clearCart,
    holdCurrentOrder,
    recallDraft,
    deleteDraft,
    heldDrafts,
    setExactCash,
    setCardPayment,
    completeSale,
    lastReceipt,
    discount,
    setDiscount,
    deviceContext,
    pairDevice,
    openCashSession,
    closeCashSession,
  } = usePos();

  const [barcodeInput, setBarcodeInput] = useState('');
  const [isCustomerModalOpen, setIsCustomerModalOpen] = useState(false);
  const [isQuickItemsModalOpen, setIsQuickItemsModalOpen] = useState(false);
  const [isDraftsModalOpen, setIsDraftsModalOpen] = useState(false);
  const [isNumpadModalOpen, setIsNumpadModalOpen] = useState(false);
  const [isDiscountModalOpen, setIsDiscountModalOpen] = useState(false);
  const [isClearCartConfirmOpen, setIsClearCartConfirmOpen] = useState(false);
  const [discountVal, setDiscountVal] = useState<string>(String(discount || ''));
  const [pairingCode, setPairingCode] = useState('');

  const barcodeInputRef = useRef<HTMLInputElement>(null);

  // التركيز التلقائي الدائم على حقل الباركود
  const focusBarcode = () => {
    setTimeout(() => {
      if (barcodeInputRef.current) {
        barcodeInputRef.current.focus();
        barcodeInputRef.current.select();
      }
    }, 50);
  };

  useEffect(() => {
    focusBarcode();
  }, []);

  // خريطة الاختصارات السريعة المعتمدة في design.md
  useHotkeys({
    F1: () => {
      focusBarcode();
    },
    F2: () => {
      if (cartItems.length > 0) {
        setIsNumpadModalOpen(true);
      }
    },
    F3: () => {
      setIsCustomerModalOpen(true);
    },
    F4: () => {
      setIsQuickItemsModalOpen(true);
    },
    F5: () => {
      holdCurrentOrder();
      focusBarcode();
    },
    F6: () => {
      setIsDraftsModalOpen(true);
    },
    F8: () => {
      setExactCash();
    },
    F9: () => {
      setCardPayment();
    },
    F10: () => {
      completeSale().then(() => focusBarcode());
    },
    Escape: () => {
      // إغلاق أي نافذة مفتوحة بأمان دون مسح السلة
      setIsCustomerModalOpen(false);
      setIsQuickItemsModalOpen(false);
      setIsDraftsModalOpen(false);
      setIsNumpadModalOpen(false);
      setIsDiscountModalOpen(false);
      setIsClearCartConfirmOpen(false);
      focusBarcode();
    },
  });

  // معالجة إدخال الباركود بالضغط على Enter
  const handleBarcodeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!barcodeInput.trim()) return;

    const barcode = barcodeInput.trim();
    setBarcodeInput('');
    await addItemByBarcode(barcode);
    focusBarcode();
  };

  const handleApplyDiscount = () => {
    const val = parseFloat(discountVal);
    setDiscount(isNaN(val) ? 0 : Math.max(0, val));
    setIsDiscountModalOpen(false);
    focusBarcode();
  };

  const activeItem = cartItems[selectedItemIndex] || cartItems[0];

  return (
    <div className="flex flex-col h-full gap-3 select-none">
      <div className="flex flex-wrap items-center gap-2 bg-white p-2.5 rounded-xl border border-slate-200 text-xs">
        {deviceContext ? (
          <>
            <span className="font-bold text-slate-800">نقطة البيع: {deviceContext.terminal.name}</span>
            <span className="text-slate-500">الدرج: {deviceContext.terminal.defaultCashDrawerName}</span>
            {deviceContext.currentSession ? (
              <>
                <span className="font-bold text-emerald-700">وردية مفتوحة</span>
                <Button size="sm" variant="secondary" onClick={() => {
                  const value = window.prompt('أدخل الرصيد الفعلي المعدود عند الإغلاق');
                  if (value !== null && Number.isFinite(Number(value))) void closeCashSession(Number(value));
                }}>إغلاق الوردية</Button>
              </>
            ) : (
              <Button size="sm" variant="primary" onClick={() => {
                const value = window.prompt('أدخل الرصيد الافتتاحي المعدود في الدرج');
                if (value !== null && Number.isFinite(Number(value))) void openCashSession(Number(value));
              }}>فتح وردية</Button>
            )}
          </>
        ) : (
          <>
            <span className="font-bold text-amber-700">هذا المتصفح غير مربوط بنقطة بيع</span>
            <input className="h-8 px-2 border rounded-lg font-mono" value={pairingCode} onChange={(e) => setPairingCode(e.target.value)} placeholder="رمز الربط" />
            <Button size="sm" variant="primary" onClick={() => void pairDevice(pairingCode)}>ربط الجهاز</Button>
          </>
        )}
      </div>
      {/* الشريط العلوي للباركود والاختصارات السريعة */}
      <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2.5 bg-white p-3 rounded-xl border border-slate-200 shadow-2xs">
        {/* حقل مسح الباركود السريع (F1) */}
        <form onSubmit={handleBarcodeSubmit} className="flex-1 flex items-center relative">
          <div className="absolute right-3.5 text-emerald-600 pointer-events-none">
            <IconBarcode className="w-5 h-5" />
          </div>

          <input
            ref={barcodeInputRef}
            type="text"
            placeholder="امسح الباركود هنا ثم اضغط Enter (F1)..."
            value={barcodeInput}
            onChange={(e) => setBarcodeInput(e.target.value)}
            className="w-full h-11 pr-11 pl-20 text-sm font-semibold rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 bg-slate-50/50 focus:bg-white transition-colors"
          />

          <span className="absolute left-3 text-[11px] font-bold text-slate-400 font-mono bg-slate-100 px-1.5 py-0.5 rounded">
            F1 / Enter
          </span>
        </form>

        {/* أزرار الإجراءات السريعة في الشريط */}
        <div className="flex items-center gap-1.5 shrink-0 overflow-x-auto">
          <Button
            variant="secondary"
            size="md"
            leftIcon={<IconPackage className="w-4 h-4 text-emerald-600" />}
            onClick={() => setIsQuickItemsModalOpen(true)}
            title="الأصناف السريعة (F4)"
          >
            الأصناف (F4)
          </Button>

          <Button
            variant="secondary"
            size="md"
            leftIcon={<IconUser className="w-4 h-4 text-emerald-600" />}
            onClick={() => setIsCustomerModalOpen(true)}
            title="اختيار عميل (F3)"
          >
            عميل (F3)
          </Button>

          <Button
            variant="secondary"
            size="md"
            leftIcon={<IconClock className="w-4 h-4 text-amber-600" />}
            onClick={() => setIsDraftsModalOpen(true)}
            title="الطلبات المعلقة (F6)"
          >
            المعلقات ({heldDrafts.length})
          </Button>
        </div>
      </div>

      {/* مساحة العمل: السلة على اليمين ولوحة الدفع على اليسار، وتتحول لعمود تحت 780px */}
      <div className="flex-1 flex flex-col lg:flex-row gap-3 min-h-0 overflow-hidden">
        {/* سلة البيع الواسعة */}
        <PosCart
          onOpenCustomerModal={() => setIsCustomerModalOpen(true)}
          onOpenNumpadModal={() => setIsNumpadModalOpen(true)}
          onClearCartConfirm={() => setIsClearCartConfirmOpen(true)}
        />

        {/* لوحة الحسابات والدفع الثابتة */}
        <PosPaymentPanel
          onOpenDraftsModal={() => setIsDraftsModalOpen(true)}
          onOpenDiscountModal={() => {
            setDiscountVal(String(discount || ''));
            setIsDiscountModalOpen(true);
          }}
        />
      </div>

      {/* شريط الإرشادات السريعة للاختصارات في أسفل الشاشة */}
      <div className="hidden md:flex items-center justify-between px-3 py-1.5 bg-white border border-slate-200 rounded-lg text-[11px] text-slate-500 font-medium">
        <div className="flex items-center gap-3">
          <span><strong className="text-emerald-700">F1:</strong> الباركود</span>
          <span><strong className="text-emerald-700">Enter:</strong> مسح وإضافة</span>
          <span><strong className="text-emerald-700">F2:</strong> تعديل الكمية</span>
          <span><strong className="text-emerald-700">F3:</strong> عميل سريع</span>
          <span><strong className="text-emerald-700">F4:</strong> أصناف سريعة</span>
          <span><strong className="text-emerald-700">F5:</strong> تعليق الطلب</span>
          <span><strong className="text-emerald-700">F6:</strong> استعادة الطلب</span>
          <span><strong className="text-emerald-700">F8:</strong> نقد كامل</span>
          <span><strong className="text-emerald-700">F9:</strong> بطاقة</span>
          <span><strong className="text-emerald-700">F10:</strong> إتمام البيع</span>
          <span><strong className="text-emerald-700">F11:</strong> كاشير كامل</span>
          <span><strong className="text-slate-700">Esc:</strong> إغلاق حوار</span>
        </div>
        <span className="text-emerald-700 font-semibold">جاهز لمسح الباركود</span>
      </div>

      {/* نافذة تعديل الكمية باللمس والأرقام (F2) */}
      <QuantityNumpadModal
        isOpen={isNumpadModalOpen}
        onClose={() => {
          setIsNumpadModalOpen(false);
          focusBarcode();
        }}
        itemName={activeItem?.productName || ''}
        initialQuantity={activeItem?.quantity || 1}
        onConfirm={(qty) => {
          setExactQuantityForSelected(qty);
          focusBarcode();
        }}
      />

      {/* نافذة اختيار أو إضافة عميل سريع (F3) */}
      <QuickCustomerModal
        isOpen={isCustomerModalOpen}
        onClose={() => {
          setIsCustomerModalOpen(false);
          focusBarcode();
        }}
        currentCustomerName={customer.name}
        onSelectCustomer={(cust) => {
          setCustomer(cust);
          focusBarcode();
        }}
      />

      {/* نافذة الأصناف السريعة (F4) */}
      <QuickItemsModal
        isOpen={isQuickItemsModalOpen}
        onClose={() => {
          setIsQuickItemsModalOpen(false);
          focusBarcode();
        }}
        onSelectItem={(item) => {
          addCartItem(item);
          focusBarcode();
        }}
      />

      {/* نافذة الطلبات المعلقة (F5 / F6) */}
      <PosDraftsModal
        isOpen={isDraftsModalOpen}
        onClose={() => {
          setIsDraftsModalOpen(false);
          focusBarcode();
        }}
        drafts={heldDrafts}
        onRecall={(id) => {
          recallDraft(id);
          focusBarcode();
        }}
        onDelete={deleteDraft}
      />

      {/* نافذة تطبيق خصم الفاتورة (F3 تفصيلي) */}
      <Modal
        isOpen={isDiscountModalOpen}
        onClose={() => {
          setIsDiscountModalOpen(false);
          focusBarcode();
        }}
        title="تطبيق خصم على الفاتورة"
        size="sm"
      >
        <div className="flex flex-col gap-3">
          <Input
            label="قيمة الخصم بالدينار (د.ل)"
            type="number"
            step="0.5"
            min="0"
            value={discountVal}
            onChange={(e) => setDiscountVal(e.target.value)}
            leftIcon={<IconDiscount2 className="w-4 h-4" />}
            autoFocus
            isLtr
          />
          <Button variant="primary" size="md" onClick={handleApplyDiscount} className="mt-2">
            تطبيق الخصم
          </Button>
        </div>
      </Modal>

      {/* نافذة تأكيد تفريغ السلة */}
      <ConfirmDialog
        isOpen={isClearCartConfirmOpen}
        onClose={() => {
          setIsClearCartConfirmOpen(false);
          focusBarcode();
        }}
        onConfirm={() => {
          clearCart();
          setIsClearCartConfirmOpen(false);
          focusBarcode();
        }}
        title="تفريغ سلة المبيعات"
        message="هل أنت متأكد من تفريغ كافة البنود الموجودة في السلة الحالية؟"
        confirmText="تفريغ السلة"
        cancelText="تراجع"
        variant="danger"
      />

      {/* قالب الطباعة الحرارية 80mm المخصص لأمر window.print() */}
      <ReceiptPrintView receipt={lastReceipt} />
    </div>
  );
};

export const PosView: React.FC = () => {
  return (
    <PosProvider>
      <PosContent />
    </PosProvider>
  );
};

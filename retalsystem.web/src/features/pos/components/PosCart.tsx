import React from 'react';
import { usePos } from '../context/PosContext';
import { EmptyState } from '../../../shared/components/ui/EmptyState';
import {
  IconPlus,
  IconMinus,
  IconTrash,
  IconUser,
  IconBarcode,
  IconShoppingCart,
} from '@tabler/icons-react';

interface PosCartProps {
  onOpenCustomerModal: () => void;
  onOpenNumpadModal: (itemIndex: number) => void;
  onClearCartConfirm: () => void;
}

export const PosCart: React.FC<PosCartProps> = ({
  onOpenCustomerModal,
  onOpenNumpadModal,
  onClearCartConfirm,
}) => {
  const {
    cartItems,
    customer,
    selectedItemIndex,
    setSelectedItemIndex,
    updateQuantity,
    removeItem,
  } = usePos();

  return (
    <div className="flex-1 flex flex-col bg-white rounded-xl border border-slate-200 shadow-2xs overflow-hidden">
      {/* شريط معلومات العميل وإحصاء السلة العلوي */}
      <div className="flex items-center justify-between px-4 py-3 bg-slate-50/70 border-b border-slate-200">
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={onOpenCustomerModal}
            className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-white border border-slate-200 hover:border-emerald-400 hover:bg-emerald-50/30 text-xs font-bold text-slate-800 transition-colors cursor-pointer shadow-2xs"
            title="تغيير العميل (F3)"
          >
            <IconUser className="w-4 h-4 text-emerald-600" />
            <span>العميل: {customer.name}</span>
            <span className="text-[10px] bg-slate-100 text-slate-500 px-1 py-0.2 rounded font-mono">F3</span>
          </button>
        </div>

        <div className="flex items-center gap-3">
          <span className="text-xs text-slate-500 font-medium">
            عدد البنود:{' '}
            <span className="font-bold text-slate-800">{cartItems.length}</span> (
            <span className="font-bold text-slate-800">
              {cartItems.reduce((sum, i) => sum + i.quantity, 0)}
            </span>{' '}
            قطعة)
          </span>

          {cartItems.length > 0 && (
            <button
              type="button"
              onClick={onClearCartConfirm}
              className="text-xs text-rose-600 hover:text-rose-700 font-semibold cursor-pointer hover:underline"
            >
              تفريغ السلة
            </button>
          )}
        </div>
      </div>

      {/* جدول بنود السلة */}
      <div className="flex-1 overflow-y-auto">
        {cartItems.length === 0 ? (
          <div className="py-16">
            <EmptyState
              title="سلة المبيعات فارغة"
              description="ابدأ بمسح الباركود باستخدام قارئ الباركود (F1) أو اضغط (F4) لاختيار الأصناف السريعة."
              icon={<IconShoppingCart className="w-8 h-8 stroke-[1.5]" />}
            />
          </div>
        ) : (
          <table className="w-full text-right text-xs">
            <thead className="bg-slate-50 sticky top-0 z-10 border-b border-slate-200 text-slate-500 font-semibold select-none">
              <tr>
                <th className="py-3 px-3 w-10 text-center">#</th>
                <th className="py-3 px-4">الصنف والباركود</th>
                <th className="py-3 px-3 text-center">السعر</th>
                <th className="py-3 px-3 text-center w-36">الكمية (F2)</th>
                <th className="py-3 px-3 text-center">الخصم</th>
                <th className="py-3 px-4 text-left">الإجمالي</th>
                <th className="py-3 px-3 w-12 text-center">حذف</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {cartItems.map((item, index) => {
                const isSelected = selectedItemIndex === index;
                return (
                  <tr
                    key={item.productId}
                    onClick={() => setSelectedItemIndex(index)}
                    className={`transition-colors cursor-pointer ${
                      isSelected ? 'bg-emerald-50/60 font-medium' : 'hover:bg-slate-50/60'
                    }`}
                  >
                    {/* رقم السطر */}
                    <td className="py-3 px-3 text-center text-slate-400 font-mono text-[11px]">
                      {index + 1}
                    </td>

                    {/* اسم الصنف والباركود */}
                    <td className="py-3 px-4">
                      <div className="font-bold text-slate-900 text-sm">{item.productName}</div>
                      <div className="flex items-center gap-2 text-[11px] text-slate-400 mt-0.5">
                        {item.barCode && (
                          <span className="flex items-center gap-1 num-ltr">
                            <IconBarcode className="w-3.5 h-3.5" />
                            {item.barCode}
                          </span>
                        )}
                        <span>· {item.unitName || 'قطعة'}</span>
                      </div>
                    </td>

                    {/* سعر الوحدة */}
                    <td className="py-3 px-3 text-center num-ltr text-slate-700 font-semibold">
                      {item.unitPrice.toFixed(2)}
                    </td>

                    {/* أزرار وتعديل الكمية */}
                    <td className="py-3 px-3 text-center">
                      <div className="inline-flex items-center gap-1 bg-white border border-slate-200 rounded-lg p-0.5 shadow-2xs">
                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            updateQuantity(item.productId, item.quantity - 1);
                          }}
                          className="w-7 h-7 rounded-md bg-slate-50 hover:bg-slate-100 flex items-center justify-center text-slate-600 transition-colors cursor-pointer"
                          title="إنقاص الكمية"
                        >
                          <IconMinus className="w-3.5 h-3.5" />
                        </button>

                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            setSelectedItemIndex(index);
                            onOpenNumpadModal(index);
                          }}
                          className="w-10 h-7 text-xs font-bold text-slate-900 text-center num-ltr rounded hover:bg-emerald-50 flex items-center justify-center transition-colors cursor-pointer"
                          title="انقر لتعديل الكمية بلوحة الأرقام (F2)"
                        >
                          {item.quantity}
                        </button>

                        <button
                          type="button"
                          onClick={(e) => {
                            e.stopPropagation();
                            updateQuantity(item.productId, item.quantity + 1);
                          }}
                          className="w-7 h-7 rounded-md bg-slate-50 hover:bg-slate-100 flex items-center justify-center text-slate-600 transition-colors cursor-pointer"
                          title="زيادة الكمية"
                        >
                          <IconPlus className="w-3.5 h-3.5" />
                        </button>
                      </div>
                    </td>

                    {/* الخصم على السطر */}
                    <td className="py-3 px-3 text-center num-ltr text-slate-500">
                      {item.discountAmount > 0 ? `-${item.discountAmount.toFixed(2)}` : '-'}
                    </td>

                    {/* إجمالي السطر */}
                    <td className="py-3 px-4 text-left font-extrabold text-emerald-800 text-sm num-ltr">
                      {item.lineTotal.toFixed(2)} د.ل
                    </td>

                    {/* حذف السطر */}
                    <td className="py-3 px-3 text-center">
                      <button
                        type="button"
                        onClick={(e) => {
                          e.stopPropagation();
                          removeItem(item.productId);
                        }}
                        className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors cursor-pointer"
                        title="حذف الصنف من السلة"
                      >
                        <IconTrash className="w-4 h-4" />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};

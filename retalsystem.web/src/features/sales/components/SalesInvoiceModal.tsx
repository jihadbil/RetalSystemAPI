import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { IconPlus, IconTrash } from '@tabler/icons-react';

interface SalesInvoiceModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (invoiceData: unknown) => Promise<void>;
}

export const SalesInvoiceModal: React.FC<SalesInvoiceModalProps> = ({
  isOpen,
  onClose,
  onSave,
}) => {
  const [customerName, setCustomerName] = useState('عميل نقدي');
  const [items, setItems] = useState<{ productName: string; quantity: number; unitPrice: number }[]>([
    { productName: '', quantity: 1, unitPrice: 0 },
  ]);
  const [discount, setDiscount] = useState<number>(0);
  const [notes] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const subtotal = items.reduce((sum, item) => sum + (item.quantity * item.unitPrice || 0), 0);
  const total = Math.max(0, subtotal - discount);

  const handleAddItem = () => {
    setItems((prev) => [...prev, { productName: '', quantity: 1, unitPrice: 0 }]);
  };

  const handleRemoveItem = (index: number) => {
    setItems((prev) => prev.filter((_, idx) => idx !== index));
  };

  const handleItemChange = (index: number, field: 'productName' | 'quantity' | 'unitPrice', val: string | number) => {
    setItems((prev) => {
      const updated = [...prev];
      updated[index] = { ...updated[index], [field]: val };
      return updated;
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const validItems = items.filter((i) => i.productName.trim() && i.quantity > 0);
    if (validItems.length === 0) return;

    setIsLoading(true);
    try {
      await onSave({
        customerName,
        items: validItems,
        discount,
        notes,
        total,
      });
      onClose();
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="إنشاء فاتورة مبيعات جديدة"
      subtitle="إصدار فاتورة بيع نقدية أو آجلة للعملاء مع تحديد البنود والأسعار"
      size="xl"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose} disabled={isLoading}>
            إلغاء
          </Button>
          <Button variant="primary" size="md" onClick={handleSubmit} isLoading={isLoading}>
            إصدار وحفظ الفاتورة
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 py-1">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Input
            label="اسم العميل *"
            value={customerName}
            onChange={(e) => setCustomerName(e.target.value)}
            required
          />
          <Input
            label="قيمة الخصم الإجمالي (د.ل)"
            type="number"
            min="0"
            value={discount}
            onChange={(e) => setDiscount(Number(e.target.value))}
            isLtr
          />
        </div>

        {/* جدول بنود الفاتورة */}
        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-700">بنود الفاتورة:</span>
            <Button
              variant="secondary"
              size="sm"
              leftIcon={<IconPlus className="w-3.5 h-3.5 text-emerald-600" />}
              onClick={handleAddItem}
            >
              + إضافة بند
            </Button>
          </div>

          <div className="border border-slate-200 rounded-xl overflow-hidden">
            <table className="w-full text-right text-xs">
              <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold">
                <tr>
                  <th className="py-2.5 px-3">اسم الصنف</th>
                  <th className="py-2.5 px-3 w-28 text-center">الكمية</th>
                  <th className="py-2.5 px-3 w-32 text-center">سعر الوحدة</th>
                  <th className="py-2.5 px-3 w-32 text-left">الإجمالي</th>
                  <th className="py-2.5 px-2 w-10 text-center"></th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {items.map((item, idx) => (
                  <tr key={idx}>
                    <td className="p-2">
                      <Input
                        placeholder="اسم الصنف أو الكود..."
                        value={item.productName}
                        onChange={(e) => handleItemChange(idx, 'productName', e.target.value)}
                        className="h-8"
                        required
                      />
                    </td>
                    <td className="p-2">
                      <Input
                        type="number"
                        min="1"
                        value={item.quantity}
                        onChange={(e) => handleItemChange(idx, 'quantity', Number(e.target.value))}
                        className="h-8 text-center"
                        isLtr
                        required
                      />
                    </td>
                    <td className="p-2">
                      <Input
                        type="number"
                        step="0.1"
                        min="0"
                        value={item.unitPrice}
                        onChange={(e) => handleItemChange(idx, 'unitPrice', Number(e.target.value))}
                        className="h-8 text-center"
                        isLtr
                        required
                      />
                    </td>
                    <td className="p-2 text-left font-bold text-slate-900 num-ltr">
                      {((item.quantity || 0) * (item.unitPrice || 0)).toFixed(2)} د.ل
                    </td>
                    <td className="p-2 text-center">
                      {items.length > 1 && (
                        <button
                          type="button"
                          onClick={() => handleRemoveItem(idx)}
                          className="p-1 text-slate-400 hover:text-rose-600 transition-colors"
                        >
                          <IconTrash className="w-4 h-4" />
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* ملخص الحساب */}
        <div className="flex justify-between items-center p-3 rounded-xl bg-slate-50 border border-slate-200">
          <span className="text-xs text-slate-500">المجموع: {subtotal.toFixed(2)} د.ل</span>
          <div className="text-sm font-extrabold text-emerald-700">
            الإجمالي النهائي: <span className="num-ltr">{total.toFixed(2)} د.ل</span>
          </div>
        </div>
      </form>
    </Modal>
  );
};

import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { IconPlus, IconTrash } from '@tabler/icons-react';

interface PurchaseInvoiceModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: unknown) => Promise<void>;
}

export const PurchaseInvoiceModal: React.FC<PurchaseInvoiceModalProps> = ({
  isOpen,
  onClose,
  onSave,
}) => {
  const [supplierName, setSupplierName] = useState('');
  const [supplierInvoiceNumber, setSupplierInvoiceNumber] = useState('');
  const [items, setItems] = useState<{ productName: string; quantity: number; unitCost: number }[]>([
    { productName: '', quantity: 1, unitCost: 0 },
  ]);
  const [discount] = useState<number>(0);
  const [notes] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const subtotal = items.reduce((sum, item) => sum + (item.quantity * item.unitCost || 0), 0);
  const total = Math.max(0, subtotal - discount);

  const handleAddItem = () => {
    setItems((prev) => [...prev, { productName: '', quantity: 1, unitCost: 0 }]);
  };

  const handleRemoveItem = (index: number) => {
    setItems((prev) => prev.filter((_, idx) => idx !== index));
  };

  const handleItemChange = (index: number, field: 'productName' | 'quantity' | 'unitCost', val: string | number) => {
    setItems((prev) => {
      const updated = [...prev];
      updated[index] = { ...updated[index], [field]: val };
      return updated;
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const validItems = items.filter((i) => i.productName.trim() && i.quantity > 0);
    if (validItems.length === 0 || !supplierName.trim()) return;

    setIsLoading(true);
    try {
      await onSave({
        supplierName: supplierName.trim(),
        supplierInvoiceNumber: supplierInvoiceNumber.trim(),
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
      title="تسجيل فاتورة مشتريات جديدة"
      subtitle="تسجيل استلام بضائع من الموردين وتحديث تكلفة وأرصدة المخزون"
      size="xl"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose} disabled={isLoading}>
            إلغاء
          </Button>
          <Button variant="primary" size="md" onClick={handleSubmit} isLoading={isLoading}>
            حفظ واعتماد الفاتورة (ذري)
          </Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-4 py-1">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Input
            label="اسم المورد أو الشركة الموردة *"
            placeholder="مثال: شركة البن العربي للتوريدات"
            value={supplierName}
            onChange={(e) => setSupplierName(e.target.value)}
            required
            autoFocus
          />

          <Input
            label="رقم فاتورة المورد (اختياري)"
            placeholder="مثال: INV-7890"
            value={supplierInvoiceNumber}
            onChange={(e) => setSupplierInvoiceNumber(e.target.value)}
            isLtr
          />
        </div>

        {/* بنود المشتريات */}
        <div className="flex flex-col gap-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-bold text-slate-700">الأصناف المستلمة وتكلفتها:</span>
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
                  <th className="py-2.5 px-3 w-28 text-center">الكمية المستلمة</th>
                  <th className="py-2.5 px-3 w-32 text-center">سعر التكلفة (الوحدة)</th>
                  <th className="py-2.5 px-3 w-32 text-left">إجمالي التكلفة</th>
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
                        value={item.unitCost}
                        onChange={(e) => handleItemChange(idx, 'unitCost', Number(e.target.value))}
                        className="h-8 text-center"
                        isLtr
                        required
                      />
                    </td>
                    <td className="p-2 text-left font-bold text-slate-900 num-ltr">
                      {((item.quantity || 0) * (item.unitCost || 0)).toFixed(2)} د.ل
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

        <div className="flex justify-between items-center p-3 rounded-xl bg-slate-50 border border-slate-200">
          <span className="text-xs text-slate-500">إجمالي البنود: {subtotal.toFixed(2)} د.ل</span>
          <div className="text-sm font-extrabold text-slate-900">
            الإجمالي المستحق للمورد: <span className="num-ltr text-emerald-800">{total.toFixed(2)} د.ل</span>
          </div>
        </div>
      </form>
    </Modal>
  );
};

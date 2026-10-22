import React, { useState, useEffect, useRef } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { IconBackspace, IconCheck } from '@tabler/icons-react';

interface QuantityNumpadModalProps {
  isOpen: boolean;
  onClose: () => void;
  itemName: string;
  initialQuantity: number;
  onConfirm: (quantity: number) => void;
}

export const QuantityNumpadModal: React.FC<QuantityNumpadModalProps> = ({
  isOpen,
  onClose,
  itemName,
  initialQuantity,
  onConfirm,
}) => {
  const [valStr, setValStr] = useState<string>(String(initialQuantity));
  const replaceNextDigit = useRef(true);

  useEffect(() => {
    if (isOpen) {
      setValStr(String(initialQuantity));
      replaceNextDigit.current = true;
    }
  }, [isOpen, initialQuantity]);

  const handleDigit = (digit: string) => {
    if (valStr === '0' || replaceNextDigit.current) {
      setValStr(digit);
    } else {
      setValStr((prev) => (prev.length < 5 ? prev + digit : prev));
    }
    replaceNextDigit.current = false;
  };

  const handleBackspace = () => {
    replaceNextDigit.current = false;
    setValStr((prev) => (prev.length > 1 ? prev.slice(0, -1) : '0'));
  };

  const handleClear = () => {
    replaceNextDigit.current = false;
    setValStr('0');
  };

  const handleSave = () => {
    const num = parseInt(valStr, 10);
    if (!isNaN(num) && num > 0) {
      onConfirm(num);
      onClose();
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="تعديل كمية الصنف (F2)" size="sm">
      <div className="flex flex-col gap-4">
        <div className="text-center p-2 rounded-lg bg-slate-50 border border-slate-100">
          <p className="text-xs text-slate-500">الصنف المحدد:</p>
          <p className="text-sm font-bold text-slate-900 truncate">{itemName || 'لا يوجد صنف محدد'}</p>
        </div>

        {/* شاشة عرض الكمية */}
        <div className="h-14 bg-slate-900 text-emerald-400 rounded-xl flex items-center justify-center text-3xl font-extrabold font-mono tracking-wider shadow-inner num-ltr">
          {valStr}
        </div>

        {/* لوحة أزرار الأرقام Numpad */}
        <div className="grid grid-cols-3 gap-2">
          {['1', '2', '3', '4', '5', '6', '7', '8', '9'].map((digit) => (
            <button
              key={digit}
              type="button"
              onClick={() => handleDigit(digit)}
              className="h-12 bg-white hover:bg-slate-100 active:bg-slate-200 border border-slate-200 rounded-xl text-lg font-bold text-slate-800 transition-colors shadow-2xs cursor-pointer"
            >
              {digit}
            </button>
          ))}

          <button
            type="button"
            onClick={handleClear}
            className="h-12 bg-rose-50 hover:bg-rose-100 text-rose-700 border border-rose-200 rounded-xl text-xs font-bold transition-colors cursor-pointer"
          >
            مسح (C)
          </button>

          <button
            type="button"
            onClick={() => handleDigit('0')}
            className="h-12 bg-white hover:bg-slate-100 active:bg-slate-200 border border-slate-200 rounded-xl text-lg font-bold text-slate-800 transition-colors shadow-2xs cursor-pointer"
          >
            0
          </button>

          <button
            type="button"
            onClick={handleBackspace}
            className="h-12 bg-slate-100 hover:bg-slate-200 text-slate-700 border border-slate-200 rounded-xl flex items-center justify-center transition-colors cursor-pointer"
            title="تراجع"
          >
            <IconBackspace className="w-5 h-5" />
          </button>
        </div>

        {/* زر التأكيد */}
        <Button
          variant="primary"
          size="lg"
          onClick={handleSave}
          leftIcon={<IconCheck className="w-5 h-5" />}
          className="w-full mt-1"
        >
          تأكيد الكمية (Enter)
        </Button>
      </div>
    </Modal>
  );
};

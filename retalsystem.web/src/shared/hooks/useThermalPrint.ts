import { useCallback, useState } from 'react';

/**
 * خطاف مخصص لإطلاق طباعة الإيصال أو الفاتورة عبر نافذة الطباعة المباشرة
 */
export function useThermalPrint() {
  const [isPrinting, setIsPrinting] = useState(false);

  const printDocument = useCallback((onBefore?: () => void, onAfter?: () => void) => {
    setIsPrinting(true);
    if (onBefore) onBefore();

    // تأخير طفيف لضمان تحديث الـ DOM الخاص بالقالب القابل للطباعة
    setTimeout(() => {
      window.print();
      setIsPrinting(false);
      if (onAfter) onAfter();
    }, 150);
  }, []);

  return {
    isPrinting,
    printDocument,
  };
}

import { useState, useEffect } from 'react';

/**
 * خطاف مخصص لتهدئة القيمة المدخلة (Debounce) لمنع تكرار استدعاء الـ API مع كل نقرة زر
 * @param value القيمة المراد تهدئتها
 * @param delay الوقت بالمللي ثانية (الافتراضي 300ms كما في وثيقة التصميم)
 */
export function useDebounce<T>(value: T, delay = 300): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

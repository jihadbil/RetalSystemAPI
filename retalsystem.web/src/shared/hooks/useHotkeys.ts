import { useEffect, useRef } from 'react';

type HotkeyHandler = (e: KeyboardEvent) => void;

interface HotkeyMap {
  [key: string]: HotkeyHandler;
}

/**
 * خطاف مخصص لالتقاط اختصارات لوحة المفاتيح لنقطة البيع (F1 - F11, Esc, وغيرها)
 * يسمح للأزرار الوظيفية F-keys بالعمل حتى لو كان التركيز داخل حقل إدخال
 */
export function useHotkeys(keyMap: HotkeyMap, enabled = true): void {
  const mapRef = useRef<HotkeyMap>(keyMap);

  useEffect(() => {
    mapRef.current = keyMap;
  }, [keyMap]);

  useEffect(() => {
    if (!enabled) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (document.querySelector('dialog[open]')) return;
      const key = e.key;

      // أزرار F1 إلى F11 يجب منع سلوك المتصفح الافتراضي لها (مثل F1 Help أو F3 Find)
      if (['F1', 'F2', 'F3', 'F4', 'F5', 'F6', 'F7', 'F8', 'F9', 'F10', 'F11'].includes(key)) {
        if (mapRef.current[key]) {
          e.preventDefault();
          mapRef.current[key](e);
        }
        return;
      }

      // أزرار الحروف والمسافات العادية لا تفعل إذا كان المستخدم يكتب في حقل نصي
      const target = e.target as HTMLElement;
      const isInput = target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable);

      if (key === 'Escape') {
        if (mapRef.current['Escape']) {
          e.preventDefault();
          mapRef.current['Escape'](e);
        }
        return;
      }

      if (!isInput && mapRef.current[key]) {
        e.preventDefault();
        mapRef.current[key](e);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [enabled]);
}

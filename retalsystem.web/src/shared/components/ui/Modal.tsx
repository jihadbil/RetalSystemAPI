import React, { useEffect } from 'react';
import { IconX } from '@tabler/icons-react';

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '4xl' | 'full';

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  subtitle?: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
  size?: ModalSize;
  closeOnBackdrop?: boolean;
}

export const Modal: React.FC<ModalProps> = ({
  isOpen,
  onClose,
  title,
  subtitle,
  children,
  footer,
  size = 'md',
  closeOnBackdrop = true,
}) => {
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen) {
        e.preventDefault();
        onClose();
      }
    };
    if (isOpen) {
      document.body.style.overflow = 'hidden';
      window.addEventListener('keydown', handleKeyDown);
    }
    return () => {
      document.body.style.overflow = '';
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  const sizeClasses: Record<ModalSize, string> = {
    sm: 'max-w-sm',
    md: 'max-w-md',
    lg: 'max-w-lg',
    xl: 'max-w-2xl',
    '2xl': 'max-w-4xl',
    '4xl': 'max-w-6xl',
    full: 'max-w-[95vw] h-[92vh]',
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-2 sm:p-4 bg-slate-900/50 backdrop-blur-xs animate-in fade-in duration-200">
      <div
        className="fixed inset-0"
        onClick={() => closeOnBackdrop && onClose()}
        aria-hidden="true"
      />

      <div
        className={`relative z-10 w-full bg-white rounded-xl shadow-2xl border border-slate-200 flex flex-col max-h-[94vh] overflow-hidden ${sizeClasses[size]} animate-in zoom-in-95 duration-200`}
        role="dialog"
        aria-modal="true"
      >
        {/* ترويسة النافذة */}
        <div className="flex items-center justify-between px-4 sm:px-5 py-3.5 sm:py-4 border-b border-slate-100 bg-slate-50/50 shrink-0">
          <div>
            <h3 className="text-sm sm:text-base font-bold text-slate-900 leading-snug">{title}</h3>
            {subtitle && <p className="text-xs text-slate-500 mt-0.5">{subtitle}</p>}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="p-1.5 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors cursor-pointer"
            title="إغلاق (Esc)"
          >
            <IconX className="w-5 h-5" />
          </button>
        </div>

        {/* جسم النافذة القابل للتمرير */}
        <div className="p-3.5 sm:p-5 overflow-y-auto flex-1">{children}</div>

        {/* تذييل النافذة */}
        {footer && (
          <div className="flex items-center justify-end gap-2.5 px-4 sm:px-5 py-3 sm:py-3.5 border-t border-slate-100 bg-slate-50/50 shrink-0">
            {footer}
          </div>
        )}
      </div>
    </div>
  );
};

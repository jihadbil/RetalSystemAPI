import React, { createContext, useContext, useState, useCallback } from 'react';
import { IconCheck, IconAlertTriangle, IconAlertCircle, IconInfoCircle, IconX } from '@tabler/icons-react';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastItem {
  id: string;
  type: ToastType;
  title: string;
  message?: string;
  action?: {
    label: string;
    onClick: () => void;
  };
}

interface ToastContextType {
  showToast: (type: ToastType, title: string, message?: string, action?: ToastItem['action']) => void;
  showSuccess: (title: string, message?: string, action?: ToastItem['action']) => void;
  showError: (title: string, message?: string) => void;
  showWarning: (title: string, message?: string) => void;
  showInfo: (title: string, message?: string) => void;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextType | null>(null);

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [toasts, setToasts] = useState<ToastItem[]>([]);

  const removeToast = useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  const showToast = useCallback(
    (type: ToastType, title: string, message?: string, action?: ToastItem['action']) => {
      const id = `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
      setToasts((prev) => [...prev, { id, type, title, message, action }]);

      setTimeout(() => {
        removeToast(id);
      }, 4000);
    },
    [removeToast]
  );

  const showSuccess = useCallback((title: string, message?: string, action?: ToastItem['action']) => {
    showToast('success', title, message, action);
  }, [showToast]);

  const showError = useCallback((title: string, message?: string) => {
    showToast('error', title, message);
  }, [showToast]);

  const showWarning = useCallback((title: string, message?: string) => {
    showToast('warning', title, message);
  }, [showToast]);

  const showInfo = useCallback((title: string, message?: string) => {
    showToast('info', title, message);
  }, [showToast]);

  return (
    <ToastContext.Provider value={{ showToast, showSuccess, showError, showWarning, showInfo, removeToast }}>
      {children}
      {/* شريط الإشعارات العائمة التفاعلية أعلى يسار الشاشة */}
      <div className="fixed top-5 left-5 z-50 flex flex-col gap-2.5 max-w-sm w-full pointer-events-none">
        {toasts.map((toast) => (
          <div
            key={toast.id}
            className={`pointer-events-auto flex items-start gap-3 p-3.5 rounded-xl border shadow-lg transition-all duration-300 transform translate-y-0 ${
              toast.type === 'success'
                ? 'bg-white border-emerald-200 text-slate-800'
                : toast.type === 'error'
                ? 'bg-white border-rose-200 text-slate-800'
                : toast.type === 'warning'
                ? 'bg-white border-amber-200 text-slate-800'
                : 'bg-white border-sky-200 text-slate-800'
            }`}
          >
            <div className="shrink-0 mt-0.5">
              {toast.type === 'success' && <IconCheck className="w-5 h-5 text-emerald-600" />}
              {toast.type === 'error' && <IconAlertCircle className="w-5 h-5 text-rose-600" />}
              {toast.type === 'warning' && <IconAlertTriangle className="w-5 h-5 text-amber-500" />}
              {toast.type === 'info' && <IconInfoCircle className="w-5 h-5 text-sky-600" />}
            </div>

            <div className="flex-1 min-w-0">
              <h4 className="text-sm font-semibold leading-tight text-slate-900">{toast.title}</h4>
              {toast.message && <p className="text-xs text-slate-600 mt-1 leading-normal">{toast.message}</p>}
              {toast.action && (
                <button
                  type="button"
                  onClick={() => {
                    toast.action?.onClick();
                    removeToast(toast.id);
                  }}
                  className="mt-2 text-xs font-semibold text-emerald-700 hover:text-emerald-800 underline block"
                >
                  {toast.action.label}
                </button>
              )}
            </div>

            <button
              type="button"
              onClick={() => removeToast(toast.id)}
              className="text-slate-400 hover:text-slate-600 p-1 -m-1 rounded-md transition-colors"
              title="إغلاق"
            >
              <IconX className="w-4 h-4" />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};

export const useToast = (): ToastContextType => {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
};

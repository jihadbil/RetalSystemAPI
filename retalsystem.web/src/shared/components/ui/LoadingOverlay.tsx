import React from 'react';

interface LoadingOverlayProps {
  isLoading?: boolean;
  isVisible?: boolean;
  message?: string;
  children?: React.ReactNode;
}

export const LoadingOverlay: React.FC<LoadingOverlayProps> = ({
  isLoading,
  isVisible,
  message = 'جاري التحميل...',
  children,
}) => {
  const active = isLoading ?? isVisible ?? false;
  return (
    <div className="relative">
      {children}

      {active && (
        <div className="absolute inset-0 z-30 bg-white/70 backdrop-blur-2xs flex flex-col items-center justify-center p-4 transition-opacity duration-200">
          <div className="flex flex-col items-center gap-3 p-4 rounded-xl bg-white shadow-md border border-slate-100">
            <svg
              className="animate-spin h-7 w-7 text-emerald-600"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
            >
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
              <path
                className="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              ></path>
            </svg>
            <span className="text-xs font-semibold text-slate-700">{message}</span>
          </div>
        </div>
      )}
    </div>
  );
};

export const TableSkeleton: React.FC<{ rows?: number; columns?: number }> = ({
  rows = 5,
  columns = 5,
}) => {
  return (
    <div className="w-full animate-pulse">
      <div className="h-10 bg-slate-100 rounded-lg mb-2 w-full"></div>
      <div className="flex flex-col gap-2">
        {Array.from({ length: rows }).map((_, r) => (
          <div key={r} className="flex items-center gap-4 h-12 px-4 bg-slate-50/60 rounded-lg border border-slate-100">
            {Array.from({ length: columns }).map((_, c) => (
              <div
                key={c}
                className="h-3 bg-slate-200 rounded-sm"
                style={{ width: `${Math.floor(60 + Math.random() * 40)}px` }}
              ></div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
};

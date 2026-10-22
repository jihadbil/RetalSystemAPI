import React from 'react';
import { IconChevronRight, IconChevronLeft } from '@tabler/icons-react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  totalCount?: number;
  totalItems?: number;
  pageSize: number;
  onPageChange: (page: number) => void;
  onPageSizeChange?: (pageSize: number) => void;
  isLoading?: boolean;
}

export const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  totalCount,
  totalItems,
  pageSize,
  onPageChange,
  onPageSizeChange,
  isLoading = false,
}) => {
  const count = totalCount ?? totalItems ?? 0;
  const startItem = count === 0 ? 0 : (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(currentPage * pageSize, count);

  // حساب أرقام الصفحات المعروضة
  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisible = 5;

    if (totalPages <= maxVisible) {
      for (let i = 1; i <= totalPages; i++) pages.push(i);
    } else {
      pages.push(1);
      if (currentPage > 3) pages.push('...');

      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - 2) pages.push('...');
      pages.push(totalPages);
    }
    return pages;
  };

  return (
    <div className="flex flex-col sm:flex-row items-center justify-between gap-3 px-4 py-3 border-t border-slate-200/80 bg-white select-none">
      {/* عدد السجلات المعروضة */}
      <div className="text-xs text-slate-500 font-medium">
        عرض <span className="font-semibold text-slate-700 num-ltr">{startItem}</span> إلى{' '}
        <span className="font-semibold text-slate-700 num-ltr">{endItem}</span> من أصل{' '}
        <span className="font-semibold text-slate-700 num-ltr">{count}</span> سجل
      </div>

      <div className="flex items-center gap-2">
        {/* اختيار حجم الصفحة */}
        {onPageSizeChange && (
          <div className="flex items-center gap-1.5 ml-3">
            <span className="text-xs text-slate-500">لكل صفحة:</span>
            <select
              value={pageSize}
              onChange={(e) => onPageSizeChange(Number(e.target.value))}
              disabled={isLoading}
              className="text-xs h-8 px-2 border border-slate-200 rounded-md bg-white text-slate-700 focus:outline-none focus:ring-1 focus:ring-emerald-500"
            >
              <option value={10}>10</option>
              <option value={25}>25</option>
              <option value={50}>50</option>
              <option value={100}>100</option>
            </select>
          </div>
        )}

        {/* زر الصفحة السابقة */}
        <button
          type="button"
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage <= 1 || isLoading}
          className="p-1.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          title="الصفحة السابقة"
        >
          <IconChevronRight className="w-4 h-4" />
        </button>

        {/* أرقام الصفحات */}
        <div className="flex items-center gap-1">
          {getPageNumbers().map((p, idx) =>
            typeof p === 'number' ? (
              <button
                key={idx}
                type="button"
                onClick={() => onPageChange(p)}
                disabled={isLoading}
                className={`min-w-[32px] h-8 px-2 text-xs font-semibold rounded-lg border transition-colors ${
                  currentPage === p
                    ? 'bg-emerald-600 text-white border-emerald-600 shadow-xs'
                    : 'border-slate-200 text-slate-700 hover:bg-slate-50'
                }`}
              >
                {p}
              </button>
            ) : (
              <span key={idx} className="px-1 text-xs text-slate-400">
                {p}
              </span>
            )
          )}
        </div>

        {/* زر الصفحة التالية */}
        <button
          type="button"
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage >= totalPages || isLoading}
          className="p-1.5 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          title="الصفحة التالية"
        >
          <IconChevronLeft className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
};

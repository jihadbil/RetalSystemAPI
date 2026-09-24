import { useState, useCallback } from 'react';

export interface UsePaginationOptions {
  initialPage?: number;
  initialPageSize?: number;
}

export function usePagination(options: UsePaginationOptions = {}) {
  const [page, setPage] = useState(options.initialPage || 1);
  const [pageSize, setPageSize] = useState(options.initialPageSize || 10);
  const [totalCount, setTotalCount] = useState(0);

  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));

  const nextPage = useCallback(() => {
    setPage((prev) => Math.min(prev + 1, totalPages));
  }, [totalPages]);

  const prevPage = useCallback(() => {
    setPage((prev) => Math.max(prev - 1, 1));
  }, []);

  const goToPage = useCallback((newPage: number) => {
    setPage(() => Math.max(1, Math.min(newPage, totalPages)));
  }, [totalPages]);

  const changePageSize = useCallback((newSize: number) => {
    setPageSize(newSize);
    setPage(1); // العودة للصفحة الأولى دائماً عند تغيير حجم الصفحة
  }, []);

  const resetPage = useCallback(() => {
    setPage(1);
  }, []);

  return {
    page,
    pageSize,
    totalCount,
    totalPages,
    setTotalCount,
    nextPage,
    prevPage,
    goToPage,
    changePageSize,
    resetPage,
    hasNextPage: page < totalPages,
    hasPrevPage: page > 1,
  };
}

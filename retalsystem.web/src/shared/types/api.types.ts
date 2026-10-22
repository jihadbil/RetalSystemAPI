// عقود واستجابات الـ API الموحدة لنظام ريتال

export interface ApiResponse<T = unknown> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
  errorCode?: string;
  statusCode?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface BalanceSummary {
  totalDebt: number;
  totalCredit: number;
  withDebt: number;
}

export interface PagedResultWithSummary<T, TSummary> extends PagedResult<T> {
  summary: TSummary;
}

export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
  errorCode?: string;
}

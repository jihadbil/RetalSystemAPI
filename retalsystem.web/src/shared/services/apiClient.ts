// عميل HTTP الموحد لنظام ريتال مع حقن التوكن والتعامل مع الأخطاء
import type { ApiResponse, ProblemDetails } from '../types/api.types';

const BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7226/api';

export class ApiError extends Error {
  statusCode: number;
  errorCode?: string;
  errors?: string[];

  constructor(message: string, statusCode: number, errorCode?: string, errors?: string[]) {
    super(message);
    this.name = 'ApiError';
    this.statusCode = statusCode;
    this.errorCode = errorCode;
    this.errors = errors;
  }
}

interface RequestOptions extends RequestInit {
  params?: Record<string, string | number | boolean | undefined | null>;
  timeoutMs?: number;
}

function isJsonContentType(contentType: string | null): boolean {
  return !!contentType && /^(?:application\/json|[\w.+-]+\/[\w.+-]+\+json)(?:\s*;|$)/i.test(contentType.trim());
}

function parseApiError(data: unknown, status: number, fallback: string): ApiError {
  if (!data || typeof data !== 'object') return new ApiError(typeof data === 'string' && data ? data : fallback, status);
  const problem = data as ProblemDetails;
  const response = data as ApiResponse;
  const errors = problem.errors ? Object.values(problem.errors).flat().filter((e) => typeof e === 'string') : [];
  return new ApiError(errors[0] || response.message || problem.detail || problem.title || fallback,
    status, problem.errorCode || response.errorCode, errors);
}

class ApiClient {
  private getAuthToken(): string | null {
    return localStorage.getItem('retal_token') || sessionStorage.getItem('retal_token');
  }

  private getBranchId(): string | null {
    return localStorage.getItem('retal_branch_id') || sessionStorage.getItem('retal_branch_id');
  }

  private getTenantId(): string | null {
    return localStorage.getItem('retal_tenant_id') || sessionStorage.getItem('retal_tenant_id');
  }

  private buildUrl(endpoint: string, params?: Record<string, string | number | boolean | undefined | null>): string {
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
    const url = new URL(`${BASE_URL}/${cleanEndpoint}`);
    
    if (params) {
      Object.entries(params).forEach(([key, val]) => {
        if (val !== undefined && val !== null && val !== '') {
          url.searchParams.append(key, String(val));
        }
      });
    }
    return url.toString();
  }

  async request<T>(endpoint: string, options: RequestOptions = {}): Promise<T> {
    const { params, timeoutMs = 30000, ...fetchOptions } = options;
    const url = this.buildUrl(endpoint, params);
    const cleanEndpoint = endpoint.replace(/^\/+/, '').toLowerCase();
    const isLoginRequest = cleanEndpoint === 'auth/login';

    const headers = new Headers(fetchOptions.headers);
    if (!headers.has('Content-Type') && !(fetchOptions.body instanceof FormData)) {
      headers.set('Content-Type', 'application/json');
    }
    headers.set('Accept', 'application/json');

    const token = this.getAuthToken();
    if (token && !isLoginRequest) {
      headers.set('Authorization', `Bearer ${token}`);
    }

    const branchId = this.getBranchId();
    if (branchId) {
      headers.set('X-Branch-Id', branchId);
    }

    const tenantId = this.getTenantId();
    if (tenantId) {
      headers.set('X-Tenant-Id', tenantId);
    }

    // إدارة الـ Timeout والـ Abort
    const controller = new AbortController();
    const id = setTimeout(() => controller.abort(), timeoutMs);
    const signal = fetchOptions.signal
      ? AbortSignal.any([fetchOptions.signal, controller.signal])
      : controller.signal;

    try {
      const response = await fetch(url, {
        ...fetchOptions,
        headers,
        signal,
        credentials: 'include',
      });

      clearTimeout(id);

      if (response.status === 401 && !isLoginRequest) {
        if (token === this.getAuthToken()) window.dispatchEvent(new CustomEvent('retal:unauthorized'));
        throw new ApiError('انتهت صلاحية الجلسة، يرجى تسجيل الدخول مجدداً', 401, 'UNAUTHORIZED');
      }

      if (response.status === 204) {
        return {} as T;
      }

      const contentType = response.headers.get('content-type');
      let data: unknown;
      if (isJsonContentType(contentType)) {
        data = await response.json();
      } else {
        data = await response.text();
      }

      if (!response.ok) {
        throw parseApiError(data, response.status, 'حدث خطأ أثناء معالجة الطلب');
      }

      // إذا كان الخادم يغلف بـ ApiResponse<T>
      if (data && typeof data === 'object' && 'success' in (data as Record<string, unknown>)) {
        const apiResp = data as ApiResponse<T>;
        if (!apiResp.success) {
          throw new ApiError(apiResp.message || 'فشلت العملية', response.status, apiResp.errorCode, apiResp.errors);
        }
        return (apiResp.data !== undefined ? apiResp.data : apiResp) as T;
      }

      return data as T;
    } catch (err: unknown) {
      clearTimeout(id);
      if (err instanceof ApiError) {
        throw err;
      }
      if (err instanceof DOMException && err.name === 'AbortError') {
        throw new ApiError('تم إلغاء الطلب أو انتهت مهلة الاتصال بالخادم', 408, 'TIMEOUT');
      }
      const msg = err instanceof Error ? err.message : 'تعذر الاتصال بالخادم، يرجى التحقق من الشبكة';
      throw new ApiError(msg, 0, 'NETWORK_ERROR');
    }
  }

  get<T>(
    endpoint: string,
    paramsOrOptions?: Record<string, string | number | boolean | undefined | null> | RequestOptions,
    maybeOptions?: RequestOptions
  ): Promise<T> {
    if (paramsOrOptions && 'params' in paramsOrOptions) {
      return this.request<T>(endpoint, { ...(paramsOrOptions as RequestOptions), method: 'GET' });
    }
    return this.request<T>(endpoint, {
      ...maybeOptions,
      method: 'GET',
      params: paramsOrOptions as Record<string, string | number | boolean | undefined | null>,
    });
  }

  post<T>(endpoint: string, body?: unknown, options?: RequestOptions): Promise<T> {
    const isFormData = body instanceof FormData;
    return this.request<T>(endpoint, {
      ...options,
      method: 'POST',
      body: isFormData ? body : JSON.stringify(body),
    });
  }

  put<T>(endpoint: string, body?: unknown, options?: RequestOptions): Promise<T> {
    const isFormData = body instanceof FormData;
    return this.request<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: isFormData ? body : JSON.stringify(body),
    });
  }

  patch<T>(endpoint: string, body?: unknown, options?: RequestOptions): Promise<T> {
    const isFormData = body instanceof FormData;
    return this.request<T>(endpoint, {
      ...options,
      method: 'PATCH',
      body: isFormData ? body : body ? JSON.stringify(body) : undefined,
    });
  }

  delete<T>(endpoint: string, options?: RequestOptions): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'DELETE' });
  }

  async download(
    endpoint: string,
    params?: Record<string, string | number | boolean | undefined | null>,
    timeoutMs = 30000
  ): Promise<{ blob: Blob; fileName?: string }> {
    const url = this.buildUrl(endpoint, params);
    const headers = new Headers({ Accept: 'application/octet-stream' });

    const token = this.getAuthToken();
    if (token) headers.set('Authorization', `Bearer ${token}`);

    const branchId = this.getBranchId();
    if (branchId) headers.set('X-Branch-Id', branchId);

    const tenantId = this.getTenantId();
    if (tenantId) headers.set('X-Tenant-Id', tenantId);

    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

    try {
      const response = await fetch(url, {
        method: 'GET',
        headers,
        signal: controller.signal,
        credentials: 'include',
      });

      if (response.status === 401) {
        if (token === this.getAuthToken()) window.dispatchEvent(new CustomEvent('retal:unauthorized'));
        throw new ApiError('انتهت صلاحية الجلسة، يرجى تسجيل الدخول مجدداً', 401, 'UNAUTHORIZED');
      }

      if (!response.ok) {
        const contentType = response.headers.get('content-type') || '';
        const data = isJsonContentType(contentType)
          ? await response.json()
          : await response.text();
        throw parseApiError(data, response.status, 'تعذر تنزيل الملف من الخادم');
      }

      const disposition = response.headers.get('content-disposition') || '';
      const utf8Match = disposition.match(/filename\*=UTF-8''([^;]+)/i);
      const plainMatch = disposition.match(/filename="?([^";]+)"?/i);
      const encodedName = utf8Match?.[1] || plainMatch?.[1];
      const fileName = encodedName ? decodeURIComponent(encodedName) : undefined;

      return { blob: await response.blob(), fileName };
    } catch (err: unknown) {
      if (err instanceof ApiError) throw err;
      if (err instanceof DOMException && err.name === 'AbortError') {
        throw new ApiError('انتهت مهلة تنزيل الملف', 408, 'TIMEOUT');
      }
      const message = err instanceof Error ? err.message : 'تعذر تنزيل الملف من الخادم';
      throw new ApiError(message, 0, 'NETWORK_ERROR');
    } finally {
      clearTimeout(timeoutId);
    }
  }
}

export const apiClient = new ApiClient();

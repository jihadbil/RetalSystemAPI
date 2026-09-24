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

    const headers = new Headers(fetchOptions.headers);
    if (!headers.has('Content-Type') && !(fetchOptions.body instanceof FormData)) {
      headers.set('Content-Type', 'application/json');
    }
    headers.set('Accept', 'application/json');

    const token = this.getAuthToken();
    if (token) {
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
    const signal = fetchOptions.signal || controller.signal;

    try {
      const response = await fetch(url, {
        ...fetchOptions,
        headers,
        signal,
        credentials: 'include',
      });

      clearTimeout(id);

      if (response.status === 401) {
        window.dispatchEvent(new CustomEvent('retal:unauthorized'));
        throw new ApiError('انتهت صلاحية الجلسة، يرجى تسجيل الدخول مجدداً', 401, 'UNAUTHORIZED');
      }

      if (response.status === 204) {
        return {} as T;
      }

      const contentType = response.headers.get('content-type');
      let data: unknown;
      if (contentType && contentType.includes('application/json')) {
        data = await response.json();
      } else {
        data = await response.text();
      }

      if (!response.ok) {
        // تفكيك أخطاء ValidationProblemDetails أو ApiResponse
        const prob = data as ProblemDetails;
        const apiResp = data as ApiResponse;
        
        let errorMsg = 'حدث خطأ أثناء معالجة الطلب';
        let errorsList: string[] = [];

        if (prob && prob.errors) {
          errorsList = Object.values(prob.errors).flat();
          errorMsg = errorsList[0] || prob.title || errorMsg;
        } else if (apiResp && apiResp.message) {
          errorMsg = apiResp.message;
          errorsList = apiResp.errors || [];
        } else if (typeof data === 'string' && data.length > 0) {
          errorMsg = data;
        }

        throw new ApiError(errorMsg, response.status, prob?.errorCode || apiResp?.errorCode, errorsList);
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
}

export const apiClient = new ApiClient();

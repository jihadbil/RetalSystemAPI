import { apiClient } from '../../../shared/services/apiClient';
import type { LoginRequest, LoginResponse, UserDto } from '../types/auth.types';

interface AuthApiResponse {
  token: string;
  refreshToken?: string;
  expiresAt?: string;
  user?: LoginResponse['user'];
  userId?: string;
  userName?: string;
  roles?: string[];
  permissions?: string[];
  tenantId?: string;
  tenantName?: string;
  branchId?: string;
  branchName?: string;
}

export const authService = {
  async getSession(signal?: AbortSignal): Promise<UserDto> {
    return apiClient.get<UserDto>('auth/session', undefined, { signal, cache: 'no-store' });
  },

  async login(credentials: LoginRequest): Promise<LoginResponse> {
    try {
      const raw = await apiClient.post<AuthApiResponse>('auth/login', credentials);
      
      // إذا كانت الاستجابة تحتوي مسبقاً على كائن user
      if (raw && raw.user) {
        return {
          ...(raw as LoginResponse),
          user: {
            ...raw.user,
            roles: raw.user.roles || [],
            permissions: raw.user.permissions || [],
          },
        };
      }

      // إذا كانت الاستجابة مسطحة قادمة من الـ API الخلفي (AuthResponseDto)
      const roles: string[] = raw?.roles || [];
      const isCashier = roles.some((r: string) => r.toLowerCase() === 'cashier' || r === 'كاشير');
      const canAccessAdmin = roles.some((r: string) => ['admin', 'manager', 'مدير'].includes(r.toLowerCase())) || !isCashier;

      return {
        token: raw.token,
        refreshToken: raw.refreshToken,
        expiresAt: raw.expiresAt || new Date(Date.now() + 86400000).toISOString(),
        user: {
          id: raw.userId || 'u-1',
          userName: raw.userName || credentials.userName,
          fullName: raw.userName || credentials.userName,
          roles: roles.length > 0 ? roles : ['User'],
          permissions: raw.permissions || [],
          tenantId: raw.tenantId ? String(raw.tenantId) : undefined,
          tenantName: raw.tenantName || 'شركة ريتال',
          branchId: raw.branchId ? String(raw.branchId) : undefined,
          branchName: raw.branchName || 'الفرع الرئيسي',
          isCashier,
          canAccessAdmin,
        },
      };
    } catch (err) {
      console.error('[authService.login] Authentication failed:', err);
      throw err;
    }
  },

  async logout(): Promise<void> {
    await apiClient.post('auth/logout');
  },
};

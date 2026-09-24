// أنواع ونماذج المصادقة والجلسة

export interface LoginRequest {
  userName: string;
  password: string;
  rememberMe?: boolean;
}

export interface UserDto {
  id: string;
  userName: string;
  fullName: string;
  email?: string;
  roles: string[];
  tenantId?: string;
  tenantName?: string;
  branchId?: string;
  branchName?: string;
  isCashier?: boolean;
  canAccessAdmin?: boolean;
}

export interface LoginResponse {
  token: string;
  refreshToken?: string;
  expiresAt: string;
  user: UserDto;
}

export interface AuthSession {
  token: string;
  user: UserDto;
  isAuthenticated: boolean;
  isLocked: boolean;
}

import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import type { UserDto, LoginResponse } from '../../features/auth/types/auth.types';
import { authService } from '../../features/auth/services/authService';
import { isPrivilegedUser, userHasPermission } from '../auth/authorization';

interface AuthContextType {
  user: UserDto | null;
  token: string | null;
  isAuthenticated: boolean;
  isInitializing: boolean;
  sessionError: string | null;
  refreshSession: () => void;
  isLocked: boolean;
  currentBranchId: string | null;
  currentTenantId: string | null;
  login: (response: LoginResponse, rememberMe?: boolean) => void;
  logout: () => Promise<void>;
  lockScreen: () => void;
  unlockScreen: (password: string) => Promise<boolean>;
  switchBranch: (branchId: string, branchName?: string) => void;
  isCashier: boolean;
  canAccessAdmin: boolean;
  hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setToken] = useState<string | null>(() => {
    return localStorage.getItem('retal_token') || sessionStorage.getItem('retal_token');
  });

  const [user, setUser] = useState<UserDto | null>(() => {
    const stored = localStorage.getItem('retal_user') || sessionStorage.getItem('retal_user');
    if (!stored) return null;
    try {
      return JSON.parse(stored);
    } catch {
      return null;
    }
  });

  const [isInitializing, setIsInitializing] = useState(() => Boolean(token));
  const [sessionError, setSessionError] = useState<string | null>(null);
  const [sessionRevision, setSessionRevision] = useState(0);

  const [isLocked, setIsLocked] = useState<boolean>(() => {
    return sessionStorage.getItem('retal_locked') === 'true';
  });

  const [currentBranchId, setCurrentBranchId] = useState<string | null>(() => {
    return localStorage.getItem('retal_branch_id') || sessionStorage.getItem('retal_branch_id') || user?.branchId || null;
  });

  const [currentTenantId, setCurrentTenantId] = useState<string | null>(() => {
    return localStorage.getItem('retal_tenant_id') || sessionStorage.getItem('retal_tenant_id') || user?.tenantId || null;
  });

  const clearSession = useCallback(() => {
    localStorage.removeItem('retal_token');
    localStorage.removeItem('retal_user');
    localStorage.removeItem('retal_branch_id');
    localStorage.removeItem('retal_tenant_id');

    sessionStorage.removeItem('retal_token');
    sessionStorage.removeItem('retal_user');
    sessionStorage.removeItem('retal_branch_id');
    sessionStorage.removeItem('retal_tenant_id');
    sessionStorage.removeItem('retal_locked');

    setToken(null);
    setUser(null);
    setIsLocked(false);
    setCurrentBranchId(null);
    setCurrentTenantId(null);
    setIsInitializing(false);
    setSessionError(null);
  }, []);

  const logout = useCallback(async () => {
    const request = token ? authService.logout() : Promise.resolve();
    clearSession();
    try {
      await request;
    } catch {
      // إنهاء الجلسة محلياً حتى عند تعذر الوصول إلى الخادم.
    }
  }, [clearSession, token]);

  const refreshSession = useCallback(() => {
    setIsInitializing(true);
    setSessionError(null);
    setSessionRevision((revision) => revision + 1);
  }, []);

  // لا تُستخدم صلاحيات الجلسة المخزنة قبل التحقق من رمز الوصول على الخادم.
  useEffect(() => {
    if (!token) return;
    const controller = new AbortController();
    const storage = localStorage.getItem('retal_token') === token ? localStorage : sessionStorage;
    const isCurrent = () => !controller.signal.aborted && storage.getItem('retal_token') === token;

    void authService.getSession(controller.signal).then((sessionUser) => {
      if (!isCurrent()) return;
      let cachedUser: UserDto | null = null;
      try { cachedUser = JSON.parse(storage.getItem('retal_user') || 'null'); } catch { /* استبدال البيانات التالفة */ }
      const sameUser = cachedUser?.id === sessionUser.id && cachedUser?.tenantId === sessionUser.tenantId;
      const branchId = (sameUser && storage.getItem('retal_branch_id')) || sessionUser.branchId || null;
      const restoredUser: UserDto = {
        ...sessionUser,
        fullName: (sameUser && cachedUser?.fullName) || sessionUser.fullName,
        tenantName: sameUser ? cachedUser?.tenantName : undefined,
        branchId: branchId || undefined,
        branchName: sameUser && cachedUser?.branchId === branchId ? cachedUser.branchName : undefined,
      };
      storage.setItem('retal_user', JSON.stringify(restoredUser));
      if (branchId) storage.setItem('retal_branch_id', branchId);
      if (sessionUser.tenantId) storage.setItem('retal_tenant_id', sessionUser.tenantId);
      setUser(restoredUser);
      setCurrentBranchId(branchId);
      setCurrentTenantId(sessionUser.tenantId || null);
      setSessionError(null);
    }).catch((error: unknown) => {
      if (isCurrent()) {
        setSessionError(error instanceof Error ? error.message : 'تعذر التحقق من الجلسة، يرجى إعادة المحاولة');
      }
    }).finally(() => {
      if (isCurrent()) setIsInitializing(false);
    });

    return () => controller.abort();
  }, [token, sessionRevision]);

  // الاستماع لحدث 401 للخروج التلقائي
  useEffect(() => {
    const handleUnauthorized = () => {
      clearSession();
    };
    window.addEventListener('retal:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('retal:unauthorized', handleUnauthorized);
  }, [clearSession]);

  const login = useCallback((response: LoginResponse, rememberMe = false) => {
    const storage = rememberMe ? localStorage : sessionStorage;

    localStorage.removeItem('retal_token');
    localStorage.removeItem('retal_user');
    localStorage.removeItem('retal_branch_id');
    localStorage.removeItem('retal_tenant_id');
    sessionStorage.removeItem('retal_token');
    sessionStorage.removeItem('retal_user');
    sessionStorage.removeItem('retal_branch_id');
    sessionStorage.removeItem('retal_tenant_id');
    
    const rawUser = response.user;
    const branchId = rawUser.branchId ? String(rawUser.branchId) : null;
    const tenantId = rawUser.tenantId ? String(rawUser.tenantId) : null;
    const token = response.token;

    storage.setItem('retal_token', token);
    storage.setItem('retal_user', JSON.stringify(rawUser));
    if (branchId) {
      storage.setItem('retal_branch_id', branchId);
    }
    if (tenantId) {
      storage.setItem('retal_tenant_id', tenantId);
    }

    setToken(token);
    setIsInitializing(true);
    setSessionError(null);
    setSessionRevision((revision) => revision + 1);
    setUser(rawUser);
    setIsLocked(false);
    sessionStorage.removeItem('retal_locked');
    setCurrentBranchId(branchId);
    setCurrentTenantId(tenantId);
  }, []);

  const lockScreen = useCallback(() => {
    setIsLocked(true);
    sessionStorage.setItem('retal_locked', 'true');
  }, []);

  const unlockScreen = useCallback(async (password: string): Promise<boolean> => {
    if (!user?.userName || !password) return false;
    try {
      const activeBranchId = currentBranchId;
      const activeBranchName = user.branchName;
      const response = await authService.login({ userName: user.userName, password });
      const rememberMe = Boolean(localStorage.getItem('retal_token'));
      login(response, rememberMe);
      if (activeBranchId && activeBranchId !== response.user.branchId) {
        const storage = rememberMe ? localStorage : sessionStorage;
        const updatedUser = {
          ...response.user,
          branchId: activeBranchId,
          branchName: activeBranchName,
        };
        storage.setItem('retal_branch_id', activeBranchId);
        storage.setItem('retal_user', JSON.stringify(updatedUser));
        setCurrentBranchId(activeBranchId);
        setUser(updatedUser);
      }
      setIsLocked(false);
      sessionStorage.removeItem('retal_locked');
      return true;
    } catch {
      return false;
    }
  }, [currentBranchId, login, user]);

  const switchBranch = useCallback((branchId: string, branchName?: string) => {
    setCurrentBranchId(branchId);
    if (localStorage.getItem('retal_token')) {
      localStorage.setItem('retal_branch_id', branchId);
    } else {
      sessionStorage.setItem('retal_branch_id', branchId);
    }

    if (user && branchName) {
      const updatedUser = { ...user, branchId, branchName };
      setUser(updatedUser);
      const storage = localStorage.getItem('retal_token') ? localStorage : sessionStorage;
      storage.setItem('retal_user', JSON.stringify(updatedUser));
    }
  }, [user]);

  // فحص الصلاحيات بمبدأ المنع الافتراضي Deny by default
  const normalizedRoles = user?.roles?.map((role) => role.toLowerCase()) || [];
  const isPrivileged = isPrivilegedUser(user);
  const hasPermission = (permission: string) => userHasPermission(user, permission);
  const isCashier = Boolean(normalizedRoles.includes('cashier') || user?.isCashier);
  const canAccessAdmin = isPrivileged || ['Permissions.Users.View', 'Permissions.Roles.View', 'Permissions.Branches.View', 'Permissions.Tenants.View'].some(hasPermission);

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isAuthenticated: Boolean(token && user),
        isInitializing,
        sessionError,
        refreshSession,
        isLocked,
        currentBranchId,
        currentTenantId,
        login,
        logout,
        lockScreen,
        unlockScreen,
        switchBranch,
        isCashier,
        canAccessAdmin,
        hasPermission,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

import type { ReactNode } from 'react';
import { useAuth } from '../../context/AuthContext';

interface PermissionGateProps {
  permission: string;
  children: ReactNode;
  fallback?: ReactNode;
}

export function PermissionGate({ permission, children, fallback = null }: PermissionGateProps) {
  const { hasPermission } = useAuth();
  return hasPermission(permission) ? children : fallback;
}

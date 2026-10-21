export interface AuthorizationUser {
  roles?: string[];
  permissions?: string[];
}

const PRIVILEGED_ROLES = new Set(['superadmin']);

export const isPrivilegedUser = (user: AuthorizationUser | null | undefined): boolean =>
  Boolean(user?.roles?.some((role) => PRIVILEGED_ROLES.has(role.trim().toLowerCase())));

export const userHasPermission = (
  user: AuthorizationUser | null | undefined,
  permission: string
): boolean => isPrivilegedUser(user) || Boolean(user?.permissions?.some((granted) => granted.toLowerCase() === permission.toLowerCase()));

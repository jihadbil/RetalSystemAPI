// أنواع ونماذج المستخدمين وإدارة الصلاحيات (RBAC)

export interface UserItemDto {
  id: string;
  userName: string;
  fullName: string;
  email?: string;
  phone?: string;
  role: string;
  branchId?: string;
  branchName?: string;
  isActive: boolean;
  createdAt?: string;
}

export interface CreateUserRequest {
  userName: string;
  password: string;
  email?: string;
  phone?: string;
  role: string;
  branchId?: string;
  isActive: boolean;
}

export interface PermissionDefinition {
  key: string;
  name: string;
  category: string;
  description: string;
}

export interface RolePermissionsDto {
  roleName: string;
  permissions: string[];
}

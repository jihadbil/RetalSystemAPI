import { apiClient } from '../../../shared/services/apiClient';
import type { UserItemDto, CreateUserRequest, PermissionDefinition } from '../types/user.types';
import type { PagedResult } from '../../../shared/types/api.types';

export interface UpdateUserRequest {
  email?: string;
  phone?: string;
  role: string;
  branchId?: string;
  isActive: boolean;
}

export interface RoleDto {
  id: string;
  name: string;
  label: string;
  description: string;
}

interface RolePermissionsResponse {
  permissions?: string[];
}

interface UserApiDto {
  id: string;
  userName: string;
  email?: string;
  phoneNumber?: string;
  roles?: string[];
  branchId?: string;
  branchName?: string;
  isActive: boolean;
}

interface PermissionApiDto {
  code: string;
  name: string;
  category: string;
  description: string;
}

const toUserItem = (user: UserApiDto): UserItemDto => ({
  id: user.id,
  userName: user.userName,
  fullName: user.userName,
  email: user.email,
  phone: user.phoneNumber,
  role: user.roles?.[0] || 'User',
  branchId: user.branchId,
  branchName: user.branchName,
  isActive: user.isActive,
});

export const userService = {
  async getPaged(
    pageNumber = 1,
    pageSize = 10,
    search?: string,
    branchId?: string
  ): Promise<PagedResult<UserItemDto>> {
    try {
      const params: Record<string, string | number> = { pageNumber, pageSize };
      if (search) params.search = search;
      if (branchId) params.branchId = branchId;

      const result = await apiClient.get<PagedResult<UserApiDto>>('users', params);
      return { ...result, items: result.items.map(toUserItem) };
    } catch (err) {
      console.error('[userService.getPaged] Failed to fetch users:', err);
      throw err;
    }
  },

  async getById(id: string): Promise<UserItemDto> {
    try {
      return toUserItem(await apiClient.get<UserApiDto>(`users/${id}`));
    } catch (err) {
      console.error(`[userService.getById] Failed to fetch user ${id}:`, err);
      throw err;
    }
  },

  async create(data: CreateUserRequest): Promise<UserItemDto> {
    try {
      const payload = {
        userName: data.userName,
        password: data.password,
        email: data.email || '',
        phoneNumber: data.phone,
        role: data.role,
        branchId: data.branchId,
      };
      return toUserItem(await apiClient.post<UserApiDto>('users', payload));
    } catch (err) {
      console.error('[userService.create] Failed to create user:', err);
      throw err;
    }
  },

  async update(id: string, data: UpdateUserRequest): Promise<UserItemDto> {
    try {
      const payload = {
        email: data.email || '',
        phoneNumber: data.phone,
        role: data.role,
        branchId: data.branchId,
        isActive: data.isActive,
      };
      return toUserItem(await apiClient.put<UserApiDto>(`users/${id}`, payload));
    } catch (err) {
      console.error(`[userService.update] Failed to update user ${id}:`, err);
      throw err;
    }
  },

  async delete(id: string): Promise<void> {
    try {
      await apiClient.delete(`users/${id}`);
    } catch (err) {
      console.error(`[userService.delete] Failed to delete user ${id}:`, err);
      throw err;
    }
  },

  async toggleActive(id: string, currentActive: boolean, currentUser?: UserItemDto): Promise<void> {
    try {
      await apiClient.put(`users/${id}`, {
        email: currentUser?.email || '',
        phoneNumber: currentUser?.phone,
        role: currentUser?.role || 'User',
        branchId: currentUser?.branchId,
        isActive: !currentActive,
      });
    } catch (err) {
      console.error(`[userService.toggleActive] Failed to toggle user ${id}:`, err);
      throw err;
    }
  },

  async resetPassword(id: string, newPassword: string): Promise<void> {
    try {
      await apiClient.post(`users/${id}/reset-password`, {
        newPassword,
        confirmPassword: newPassword,
      });
    } catch (err) {
      console.error(`[userService.resetPassword] Failed for user ${id}:`, err);
      throw err;
    }
  },

  // الأدوار والصلاحيات
  async getRoles(): Promise<RoleDto[]> {
    try {
      const res = await apiClient.get<RoleDto[]>('users/roles');
      return (res || []).map((r) => ({
        id: r.id || r.name,
        name: r.name,
        label: r.label || r.name,
        description: r.description || '',
      }));
    } catch (err) {
      console.error('[userService.getRoles] Failed to fetch roles:', err);
      throw err;
    }
  },

  async getPermissions(): Promise<PermissionDefinition[]> {
    try {
      const permissions = await apiClient.get<PermissionApiDto[]>('users/permissions');
      return permissions.map((permission) => ({
        key: permission.code,
        name: permission.name,
        category: permission.category,
        description: permission.description,
      }));
    } catch (err) {
      console.error('[userService.getPermissions] Failed to fetch permissions:', err);
      throw err;
    }
  },

  async getAllPermissions(): Promise<PermissionDefinition[]> {
    return this.getPermissions();
  },

  async getRolePermissions(roleId: string): Promise<string[]> {
    try {
      const res = await apiClient.get<string[] | RolePermissionsResponse>(`users/roles/${roleId}/permissions`);
      if (Array.isArray(res)) return res;
      if (res?.permissions && Array.isArray(res.permissions)) return res.permissions;
      return [];
    } catch (err) {
      console.error(`[userService.getRolePermissions] Failed for role ${roleId}:`, err);
      throw err;
    }
  },

  async updateRolePermissions(roleId: string, permissions: string[]): Promise<void> {
    try {
      await apiClient.put(`users/roles/${roleId}/permissions`, { roleId, permissions });
    } catch (err) {
      console.error(`[userService.updateRolePermissions] Failed for role ${roleId}:`, err);
      throw err;
    }
  },
};

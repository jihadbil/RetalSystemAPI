import { apiClient } from '../../../shared/services/apiClient';
import type { UserItemDto, CreateUserRequest, PermissionDefinition } from '../types/user.types';
import type { PagedResult } from '../../../shared/types/api.types';

export interface UpdateUserRequest {
  fullName: string;
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

      return await apiClient.get<PagedResult<UserItemDto>>('users', params);
    } catch (err) {
      console.error('[userService.getPaged] Failed to fetch users:', err);
      throw err;
    }
  },

  async getById(id: string): Promise<UserItemDto> {
    try {
      return await apiClient.get<UserItemDto>(`users/${id}`);
    } catch (err) {
      console.error(`[userService.getById] Failed to fetch user ${id}:`, err);
      throw err;
    }
  },

  async create(data: CreateUserRequest): Promise<UserItemDto> {
    try {
      const payload = {
        ...data,
        phoneNumber: data.phone,
      };
      return await apiClient.post<UserItemDto>('users', payload);
    } catch (err) {
      console.error('[userService.create] Failed to create user:', err);
      throw err;
    }
  },

  async update(id: string, data: UpdateUserRequest): Promise<UserItemDto> {
    try {
      const payload = {
        ...data,
        phoneNumber: data.phone,
      };
      return await apiClient.put<UserItemDto>(`users/${id}`, payload);
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
        fullName: currentUser?.fullName || '',
        email: currentUser?.email,
        phone: currentUser?.phone,
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
      return await apiClient.get<PermissionDefinition[]>('users/permissions');
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

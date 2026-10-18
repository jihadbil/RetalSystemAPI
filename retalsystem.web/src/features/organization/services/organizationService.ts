import { apiClient } from '../../../shared/services/apiClient';
import type { BranchDto, TenantSettingsDto } from '../types/organization.types';

export interface CreateBranchRequest {
  name: string;
  phone?: string;
  address?: string;
  isActive: boolean;
  phones?: { name?: string; phoneNumber: string }[];
}

export interface UpdateBranchRequest extends CreateBranchRequest {
  id: string;
}

interface BranchApiDto extends Partial<BranchDto> {
  id: string;
  name: string;
  phones?: { phoneNumber?: string }[];
}

interface BranchListResponse {
  items?: BranchApiDto[];
}

interface TenantApiDto extends Partial<TenantSettingsDto> {
  id: string;
  name: string;
  commercialName?: string;
  phoneNumber?: string;
  contactEmail?: string;
  defaultCurrencySymbol?: string;
  commercialRegistrationNumber?: string;
  invoiceHeaderNote?: string;
  invoiceFooterNote?: string;
}

export const normalizeTenant = (data: TenantApiDto): TenantSettingsDto => ({
  ...data, companyName: data.commercialName || data.name, phone: data.phoneNumber || '',
  email: data.contactEmail || '', currencySymbol: data.defaultCurrencySymbol || 'د.ل',
  commercialRegister: data.commercialRegistrationNumber || '', defaultTaxRate: data.defaultTaxRate ?? 0,
  receiptHeader: data.invoiceHeaderNote || '', receiptFooter: data.invoiceFooterNote || '',
});

export function buildTenantPayload(data: Partial<TenantSettingsDto>) {
  const { companyName, phone, email, currencySymbol, commercialRegister, receiptHeader, receiptFooter, ...rest } = data;
  return {
    ...rest,
    ...(companyName !== undefined && { commercialName: companyName }),
    ...(phone !== undefined && { phoneNumber: phone }),
    ...(email !== undefined && { contactEmail: email.trim() || null }),
    ...(currencySymbol !== undefined && { defaultCurrencySymbol: currencySymbol }),
    ...(commercialRegister !== undefined && { commercialRegistrationNumber: commercialRegister }),
    ...(receiptHeader !== undefined && { invoiceHeaderNote: receiptHeader }),
    ...(receiptFooter !== undefined && { invoiceFooterNote: receiptFooter }),
  };
}

export const organizationService = {
  // الفروع
  async getBranches(): Promise<BranchDto[]> {
    try {
      const res = await apiClient.get<BranchApiDto[] | BranchListResponse>('branches');
      const list = Array.isArray(res) ? res : res.items || [];
      return list.map((b) => ({
        id: b.id,
        name: b.name,
        phone: b.phones?.[0]?.phoneNumber || b.phone || '',
        address: b.address || '',
        isActive: b.isActive !== false,
        tenantId: b.tenantId,
      }));
    } catch (err) {
      console.error('[organizationService.getBranches] Failed to fetch branches:', err);
      throw err;
    }
  },

  async createBranch(data: CreateBranchRequest): Promise<BranchDto> {
    try {
      const phones = data.phones || (data.phone?.trim() ? [{ name: 'الهاتف الرئيسي', phoneNumber: data.phone.trim() }] : []);
      const payload = {
        name: data.name.trim(),
        address: data.address?.trim() || null,
        isActive: data.isActive !== false,
        phones,
      };
      return await apiClient.post<BranchDto>('branches', payload);
    } catch (err) {
      console.error('[organizationService.createBranch] Failed to create branch:', err);
      throw err;
    }
  },

  async updateBranch(id: string, data: UpdateBranchRequest): Promise<BranchDto> {
    try {
      const phones = data.phones || (data.phone?.trim() ? [{ name: 'الهاتف الرئيسي', phoneNumber: data.phone.trim() }] : []);
      const payload = {
        id,
        name: data.name.trim(),
        address: data.address?.trim() || null,
        isActive: data.isActive !== false,
        phones,
      };
      return await apiClient.put<BranchDto>(`branches/${id}`, payload);
    } catch (err) {
      console.error(`[organizationService.updateBranch] Failed to update branch ${id}:`, err);
      throw err;
    }
  },

  async deleteBranch(id: string): Promise<void> {
    try {
      await apiClient.delete(`branches/${id}`);
    } catch (err) {
      console.error(`[organizationService.deleteBranch] Failed to delete branch ${id}:`, err);
      throw err;
    }
  },

  async toggleBranchActive(id: string): Promise<BranchDto> {
    try {
      return await apiClient.patch<BranchDto>(`branches/${id}/toggle-active`);
    } catch (err) {
      console.error(`[organizationService.toggleBranchActive] Failed to toggle branch ${id}:`, err);
      throw err;
    }
  },

  // إعدادات المنشأة / المستأجر
  async getTenantSettings(): Promise<TenantSettingsDto> {
    try {
      return normalizeTenant(await apiClient.get<TenantApiDto>('tenants/me'));
    } catch (err) {
      console.error('[organizationService.getTenantSettings] Failed to fetch tenant settings:', err);
      throw err;
    }
  },

  async updateTenantSettings(data: Partial<TenantSettingsDto>): Promise<TenantSettingsDto> {
    try {
      return normalizeTenant(await apiClient.put<TenantApiDto>('tenants/me', buildTenantPayload(data)));
    } catch (err) {
      console.error('[organizationService.updateTenantSettings] Failed to update tenant settings:', err);
      throw err;
    }
  },

  // Aliases for compatibility
  async getTenantProfile(): Promise<TenantSettingsDto> {
    return this.getTenantSettings();
  },

  async updateTenantProfile(data: Partial<TenantSettingsDto>): Promise<TenantSettingsDto> {
    return this.updateTenantSettings(data);
  },
};

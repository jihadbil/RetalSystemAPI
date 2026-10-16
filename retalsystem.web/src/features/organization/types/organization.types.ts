// أنواع ونماذج الفروع والمنشأة وإعدادات النظام

export interface BranchDto {
  id: string;
  name: string;
  phone?: string;
  address?: string;
  isActive: boolean;
  tenantId?: string;
}

export interface TenantSettingsDto {
  id: string;
  name: string;
  companyName: string;
  logoUrl?: string;
  taxNumber?: string;
  commercialRegister?: string;
  phone?: string;
  email?: string;
  address?: string;
  currencySymbol: string;
  defaultTaxRate: number;
  receiptHeader?: string;
  receiptFooter?: string;
}

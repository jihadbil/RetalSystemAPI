import React, { useState, useEffect, useCallback } from 'react';
import { BranchDto, TenantSettingsDto } from './types/organization.types';
import { organizationService } from './services/organizationService';
import { BranchFormModal } from './components/BranchFormModal';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Badge } from '../../shared/components/ui/Badge';
import { Tabs, TabItem } from '../../shared/components/ui/Tabs';
import { ConfirmDialog } from '../../shared/components/ui/ConfirmDialog';
import { useToast } from '../../shared/context/ToastContext';
import {
  IconBuildingStore,
  IconSettings,
  IconPlus,
  IconEdit,
  IconTrash,
  IconPhone,
  IconMapPin,
  IconReceiptTax,
  IconBuildingBank,
  IconCheck,
} from '@tabler/icons-react';

interface OrganizationViewProps {
  initialTab?: 'branches' | 'settings';
}

export const OrganizationView: React.FC<OrganizationViewProps> = ({ initialTab = 'branches' }) => {
  const { showSuccess, showError } = useToast();
  const [activeTab, setActiveTab] = useState<'branches' | 'settings'>(initialTab);

  useEffect(() => {
    if (initialTab) {
      setActiveTab(initialTab);
    }
  }, [initialTab]);

  // الفروع
  const [branches, setBranches] = useState<BranchDto[]>([]);
  const [, setIsLoadingBranches] = useState(false);
  const [isBranchFormOpen, setIsBranchFormOpen] = useState(false);
  const [branchToEdit, setBranchToEdit] = useState<BranchDto | null>(null);
  const [branchToDelete, setBranchToDelete] = useState<BranchDto | null>(null);

  // إعدادات المنشأة
  const [tenantProfile, setTenantProfile] = useState<TenantSettingsDto | null>(null);
  const [, setIsLoadingProfile] = useState(false);
  const [isSavingProfile, setIsSavingProfile] = useState(false);

  const [orgError, setOrgError] = useState<string | null>(null);

  const loadBranches = useCallback(async () => {
    try {
      setIsLoadingBranches(true);
      setOrgError(null);
      const data = await organizationService.getBranches();
      setBranches(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل بيانات الفروع';
      setOrgError(msg);
      showError('خطأ في تحميل الفروع', msg);
    } finally {
      setIsLoadingBranches(false);
    }
  }, [showError]);

  const loadProfile = useCallback(async () => {
    try {
      setIsLoadingProfile(true);
      const data = await organizationService.getTenantProfile();
      setTenantProfile(data);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل بيانات المنشأة';
      showError('خطأ في إعدادات المنشأة', msg);
    } finally {
      setIsLoadingProfile(false);
    }
  }, [showError]);

  useEffect(() => {
    loadBranches();
    loadProfile();
  }, [loadBranches, loadProfile]);

  const handleDeleteBranch = async () => {
    if (!branchToDelete) return;
    try {
      await organizationService.deleteBranch(branchToDelete.id);
      showSuccess(`تم حذف الفرع "${branchToDelete.name}" بنجاح`);
      setBranchToDelete(null);
      loadBranches();
    } catch {
      showError('حدث خطأ أثناء حذف الفرع');
    }
  };

  const handleSaveProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!tenantProfile) return;

    try {
      setIsSavingProfile(true);
      await organizationService.updateTenantProfile(tenantProfile);
      showSuccess('تم حفظ إعدادات وبيانات المنشأة بنجاح');
    } catch (err: unknown) {
      showError(err instanceof Error ? err.message : 'فشل حفظ إعدادات المنشأة');
    } finally {
      setIsSavingProfile(false);
    }
  };

  const tabs: TabItem[] = [
    { id: 'branches', label: 'فروع ومنافذ المؤسسة', icon: <IconBuildingStore className="w-4 h-4" /> },
    { id: 'settings', label: 'إعدادات المنشأة والهوية', icon: <IconSettings className="w-4 h-4" /> },
  ];

  return (
    <div className="flex flex-col gap-5">
      {/* الترويسة */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-slate-900 tracking-tight">إدارة الفروع وإعدادات المؤسسة</h1>
          <p className="text-xs text-slate-500 font-medium mt-0.5">
            تهيئة الفروع، بيانات السجل والضريبة، وقوالب ترويسة وتذييل الفواتير
          </p>
        </div>

        {activeTab === 'branches' && (
          <Button
            variant="primary"
            onClick={() => {
              setBranchToEdit(null);
              setIsBranchFormOpen(true);
            }}
          >
            <IconPlus className="w-4 h-4" />
            إضافة فرع جديد
          </Button>
        )}
      </div>

      <Tabs tabs={tabs} activeTab={activeTab} onChange={(id) => setActiveTab(id as 'branches' | 'settings')} />

      {orgError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل بيانات الفروع أو المنشأة من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{orgError}</p>
          </div>
          <Button
            variant="secondary"
            size="sm"
            onClick={() => {
              loadBranches();
              loadProfile();
            }}
          >
            إعادة المحاولة
          </Button>
        </div>
      )}

      {activeTab === 'branches' && (
        <div className="flex flex-col gap-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {branches.map((b) => (
              <div
                key={b.id}
                className="p-5 rounded-2xl bg-white border border-slate-200/80 shadow-2xs flex flex-col justify-between gap-4 hover:border-emerald-300 transition-all"
              >
                <div>
                  <div className="flex items-start justify-between gap-2">
                    <div className="flex items-center gap-2.5">
                      <div className="w-10 h-10 rounded-xl bg-emerald-50 text-emerald-700 flex items-center justify-center">
                        <IconBuildingStore className="w-5 h-5" />
                      </div>
                      <div>
                        <h3 className="font-extrabold text-sm text-slate-900 leading-tight">{b.name}</h3>
                        <span className="text-[11px] font-mono text-emerald-700 font-bold">{b.code}</span>
                      </div>
                    </div>

                    <Badge variant={b.isActive ? 'success' : 'neutral'}>
                      {b.isActive ? 'نشط' : 'معطل'}
                    </Badge>
                  </div>

                  <div className="mt-4 flex flex-col gap-2 text-xs text-slate-600">
                    {b.phone && (
                      <div className="flex items-center gap-2 font-mono num-ltr text-right">
                        <IconPhone className="w-3.5 h-3.5 text-slate-400" />
                        <span>{b.phone}</span>
                      </div>
                    )}
                    {b.address && (
                      <div className="flex items-center gap-2 text-slate-500">
                        <IconMapPin className="w-3.5 h-3.5 text-slate-400 shrink-0" />
                        <span className="truncate">{b.address}</span>
                      </div>
                    )}
                  </div>
                </div>

                <div className="pt-3 border-t border-slate-100 flex items-center justify-end gap-1.5">
                  <button
                    type="button"
                    title="تعديل بيانات الفرع"
                    onClick={() => {
                      setBranchToEdit(b);
                      setIsBranchFormOpen(true);
                    }}
                    className="p-1.5 rounded-lg text-slate-600 hover:bg-slate-100 transition-colors"
                  >
                    <IconEdit className="w-4 h-4" />
                  </button>

                  <button
                    type="button"
                    title="حذف الفرع"
                    onClick={() => setBranchToDelete(b)}
                    className="p-1.5 rounded-lg text-rose-600 hover:bg-rose-50 transition-colors"
                  >
                    <IconTrash className="w-4 h-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'settings' && tenantProfile && (
        <form onSubmit={handleSaveProfile} className="flex flex-col gap-5">
          <div className="p-6 bg-white rounded-2xl border border-slate-200/80 shadow-2xs flex flex-col gap-6">
            <h2 className="text-base font-extrabold text-slate-900 border-b border-slate-100 pb-3">
              البيانات الرسمية والضريبية للمنشأة
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input
                label="الاسم التجاري للمنشأة *"
                value={tenantProfile.companyName}
                onChange={(e) => setTenantProfile({ ...tenantProfile, companyName: e.target.value })}
                leftIcon={<IconBuildingStore className="w-4 h-4" />}
              />

              <Input
                label="الرقم الضريبي (15 خانة) *"
                value={tenantProfile.taxNumber || ''}
                onChange={(e) => setTenantProfile({ ...tenantProfile, taxNumber: e.target.value })}
                leftIcon={<IconReceiptTax className="w-4 h-4" />}
                className="num-ltr text-right font-mono"
              />

              <Input
                label="رقم السجل التجاري"
                value={tenantProfile.commercialRegister || ''}
                onChange={(e) => setTenantProfile({ ...tenantProfile, commercialRegister: e.target.value })}
                leftIcon={<IconBuildingBank className="w-4 h-4" />}
                className="num-ltr text-right font-mono"
              />

              <Input
                label="رقم الهاتف الموحد / خدمة العملاء"
                value={tenantProfile.phone || ''}
                onChange={(e) => setTenantProfile({ ...tenantProfile, phone: e.target.value })}
                leftIcon={<IconPhone className="w-4 h-4" />}
                className="num-ltr text-right"
              />

              <Input
                label="البريد الإلكتروني الرسمي"
                value={tenantProfile.email || ''}
                onChange={(e) => setTenantProfile({ ...tenantProfile, email: e.target.value })}
                className="num-ltr text-right"
              />

              <Input
                label="رمز العملة الافتراضية"
                value={tenantProfile.currencySymbol}
                onChange={(e) => setTenantProfile({ ...tenantProfile, currencySymbol: e.target.value })}
              />

              <Input
                label="نسبة ضريبة القيمة المضافة الافتراضية (%)"
                type="number"
                value={tenantProfile.defaultTaxRate}
                onChange={(e) =>
                  setTenantProfile({ ...tenantProfile, defaultTaxRate: parseFloat(e.target.value) || 0 })
                }
                className="num-ltr text-right"
              />

              <Input
                label="العنوان الجغرافي والمدينة"
                value={tenantProfile.address || ''}
                onChange={(e) => setTenantProfile({ ...tenantProfile, address: e.target.value })}
                leftIcon={<IconMapPin className="w-4 h-4" />}
              />
            </div>

            <h2 className="text-base font-extrabold text-slate-900 border-b border-slate-100 pb-3 pt-4">
              نصوص ترويسة وتذييل الفاتورة الحرارية (80mm)
            </h2>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-slate-700">ترويسة الفاتورة (أعلى الإيصال)</label>
                <textarea
                  className="w-full px-3 py-2 rounded-xl border border-slate-200 bg-white text-slate-900 text-xs focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 resize-none h-24"
                  value={tenantProfile.receiptHeader || ''}
                  onChange={(e) => setTenantProfile({ ...tenantProfile, receiptHeader: e.target.value })}
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <label className="text-xs font-bold text-slate-700">تذييل الفاتورة (أسفل الإيصال)</label>
                <textarea
                  className="w-full px-3 py-2 rounded-xl border border-slate-200 bg-white text-slate-900 text-xs focus:outline-none focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 resize-none h-24"
                  value={tenantProfile.receiptFooter || ''}
                  onChange={(e) => setTenantProfile({ ...tenantProfile, receiptFooter: e.target.value })}
                />
              </div>
            </div>

            <div className="flex justify-end pt-4 border-t border-slate-100">
              <Button variant="primary" type="submit" isLoading={isSavingProfile}>
                <IconCheck className="w-4 h-4" />
                حفظ التغييرات
              </Button>
            </div>
          </div>
        </form>
      )}

      <BranchFormModal
        isOpen={isBranchFormOpen}
        onClose={() => setIsBranchFormOpen(false)}
        branchToEdit={branchToEdit}
        onSaved={loadBranches}
      />

      <ConfirmDialog
        isOpen={!!branchToDelete}
        onClose={() => setBranchToDelete(null)}
        onConfirm={handleDeleteBranch}
        title="تأكيد حذف الفرع"
        message={`هل أنت متأكد من رغبتك في حذف الفرع "${branchToDelete?.name}"؟`}
        confirmText="نعم، احذف الفرع"
        cancelText="إلغاء"
        variant="danger"
      />
    </div>
  );
};

import { useRequestGate } from '../../shared/hooks/useRequestGate';
import React, { useState, useEffect, useCallback } from 'react';
import { UserItemDto } from './types/user.types';
import { userService } from './services/userService';
import { UserFormModal } from './components/UserFormModal';
import { ResetPasswordModal } from './components/ResetPasswordModal';
import { PermissionsMatrixModal } from './components/PermissionsMatrixModal';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Badge } from '../../shared/components/ui/Badge';
import { Tabs, TabItem } from '../../shared/components/ui/Tabs';
import { ConfirmDialog } from '../../shared/components/ui/ConfirmDialog';
import { Pagination } from '../../shared/components/ui/Pagination';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { useToast } from '../../shared/context/ToastContext';
import { useAuth } from '../../shared/context/AuthContext';
import { Permissions } from '../../shared/auth/permissions';
import {
  IconUsers,
  IconShield,
  IconPlus,
  IconSearch,
  IconEdit,
  IconTrash,
  IconKey,
  IconShieldCheck,
  IconBuildingStore,
  IconPhone,
  IconMail,
} from '@tabler/icons-react';

export const UsersView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const { hasPermission } = useAuth();
  const canViewUsers = hasPermission('Permissions.Users.View');
  const canCreate = hasPermission(Permissions.Users.Create);
  const canEdit = hasPermission(Permissions.Users.Edit);
  const canDelete = hasPermission(Permissions.Users.Delete);
  const canResetPassword = hasPermission(Permissions.Users.ResetPassword);
  const canViewRoles = hasPermission(Permissions.Roles.View);
  const canManageRoles = hasPermission(Permissions.Roles.Manage);
  const [selectedTab, setActiveTab] = useState('users');
  const activeTab = (selectedTab === 'users' ? canViewUsers : canViewRoles) ? selectedTab : canViewUsers ? 'users' : 'roles';

  // المستخدمين
  const [users, setUsers] = useState<UserItemDto[]>([]);
  const [isLoadingUsers, setIsLoadingUsers] = useState(false);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);

  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // الأدوار
  const [roles, setRoles] = useState<{ id: string; name: string; label: string; description: string }[]>([]);

  // النوافذ المنبثقة
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [userToEdit, setUserToEdit] = useState<UserItemDto | null>(null);

  const [isResetOpen, setIsResetOpen] = useState(false);
  const [userToReset, setUserToReset] = useState<UserItemDto | null>(null);

  const [isMatrixOpen, setIsMatrixOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<{ id: string; name: string; label: string } | null>(null);

  const [userToDelete, setUserToDelete] = useState<UserItemDto | null>(null);

  const [userError, setUserError] = useState<string | null>(null);

  const loadUsersGate = useRequestGate();
  useEffect(() => loadUsersGate.invalidate, [loadUsersGate, pageNumber, pageSize, debouncedSearch, showError]);
  const loadUsers = useCallback(async () => {
    const isCurrent = loadUsersGate.begin();
    setUsers([]);
    try {
      setIsLoadingUsers(true);
      setUserError(null);
      const res = await userService.getPaged(pageNumber, pageSize, debouncedSearch);
      if (!isCurrent()) return;
      setUsers(res.items);
      setTotalPages(res.totalPages);
      setTotalCount(res.totalCount);
    } catch (err: unknown) {
      if (!isCurrent()) return;
      const msg = err instanceof Error ? err.message : 'فشل تحميل قائمة المستخدمين';
      setUserError(msg);
      showError('خطأ في تحميل المستخدمين', msg);
    } finally {
      if (isCurrent()) {
      setIsLoadingUsers(false);
      }
    }
  }, [loadUsersGate, pageNumber, pageSize, debouncedSearch, showError]);

  const loadRolesGate = useRequestGate();
  useEffect(() => loadRolesGate.invalidate, [loadRolesGate]);
  const loadRoles = useCallback(async () => {
    const isCurrent = loadRolesGate.begin();
    try {
      const data = await userService.getRoles();
      if (!isCurrent()) return;
      setRoles(data);
    } catch (err: unknown) {
      if (!isCurrent()) return;
      console.error('Failed to load roles:', err);
    }
  }, [loadRolesGate]);

  useEffect(() => {
    if (canViewUsers) loadUsers();
    if (canViewRoles) loadRoles();
  }, [loadUsers, loadRoles, canViewUsers, canViewRoles]);

  const handleDelete = async () => {
    if (!userToDelete) return;
    try {
      await userService.delete(userToDelete.id);
      showSuccess(`تم حذف حساب المستخدم "${userToDelete.userName}" بنجاح`);
      setUserToDelete(null);
      loadUsers();
    } catch {
      showError('حدث خطأ أثناء حذف المستخدم');
    }
  };

  const tabs: TabItem[] = [
    ...(canViewUsers ? [{ id: 'users', label: 'المستخدمين والموظفين', icon: <IconUsers className="w-4 h-4" /> }] : []),
    ...(canViewRoles
      ? [{ id: 'roles', label: 'الأدوار ومصفوفة الصلاحيات (RBAC)', icon: <IconShield className="w-4 h-4" /> }]
      : []),
  ];

  const getRoleBadgeVariant = (role: string) => {
    switch (role) {
      case 'Admin':
        return 'danger';
      case 'Cashier':
        return 'success';
      case 'Accountant':
        return 'info';
      case 'StockManager':
        return 'warning';
      default:
        return 'neutral';
    }
  };

  return (
    <div className="flex flex-col gap-5">
      {/* الترويسة الرئيسية */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-slate-900 tracking-tight">إدارة المستخدمين والصلاحيات</h1>
          <p className="text-xs text-slate-500 font-medium mt-0.5">
            حسابات الموظفين، تعيين كلمات المرور، ومصفوفة الصلاحيات والأدوار الوظيفية
          </p>
        </div>

        {activeTab === 'users' && canCreate && (
          <Button
            variant="primary"
            onClick={() => {
              setUserToEdit(null);
              setIsFormOpen(true);
            }}
          >
            <IconPlus className="w-4 h-4" />
            إضافة مستخدم جديد
          </Button>
        )}
      </div>

      <Tabs tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />

      {userError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل بيانات المستخدمين من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{userError}</p>
          </div>
          <Button variant="secondary" size="sm" onClick={() => loadUsers()}>
            إعادة المحاولة
          </Button>
        </div>
      )}

      {activeTab === 'users' && (
        <div className="flex flex-col gap-4">
          {/* شريط البحث */}
          <div className="p-3 bg-white rounded-2xl border border-slate-200/80 shadow-2xs flex items-center justify-between">
            <div className="w-full sm:w-80">
              <Input
                placeholder="بحث باسم المستخدم أو الاسم الكامل..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                leftIcon={<IconSearch className="w-4 h-4 text-slate-400" />}
              />
            </div>
          </div>

          {/* جدول المستخدمين */}
          <div className="relative bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
            <LoadingOverlay isVisible={isLoadingUsers} />

            {users.length === 0 && !isLoadingUsers ? (
              <EmptyState
                icon={<IconUsers className="w-10 h-10 text-slate-300" />}
                title="لا يوجد مستخدمين مطابقين"
                description="لم يتم العثور على أي حساب مستخدم يطابق معايير البحث."
                actionLabel={canCreate ? 'إضافة مستخدم' : undefined}
                onAction={canCreate ? () => {
                  setUserToEdit(null);
                  setIsFormOpen(true);
                } : undefined}
              />
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs min-w-[650px]">
                  <thead className="bg-slate-50/90 border-b border-slate-200 text-slate-500 font-bold">
                    <tr>
                      <th className="py-3 px-4">المستخدم</th>
                      <th className="py-3 px-4">الدور الوظيفي</th>
                      <th className="py-3 px-4">الفرع المخصص</th>
                      <th className="py-3 px-4">الاتصال</th>
                      <th className="py-3 px-4 text-center">الحالة</th>
                      <th className="py-3 px-4 text-center">إجراءات سريعة</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 font-medium">
                    {users.map((u) => (
                      <tr key={u.id} className="hover:bg-slate-50/70 transition-colors">
                        <td className="py-3 px-4 font-mono font-bold text-slate-800 num-ltr text-right">
                          @{u.userName}
                        </td>
                        <td className="py-3 px-4">
                          <Badge variant={getRoleBadgeVariant(u.role)}>
                            {roles.find((r) => r.name === u.role)?.label || u.role}
                          </Badge>
                        </td>
                        <td className="py-3 px-4">
                          <div className="flex items-center gap-1.5 text-slate-600">
                            <IconBuildingStore className="w-3.5 h-3.5 text-slate-400" />
                            <span>{u.branchName || 'الفرع الرئيسي'}</span>
                          </div>
                        </td>
                        <td className="py-3 px-4">
                          {u.phone && (
                            <div className="flex items-center gap-1 font-mono num-ltr text-right text-slate-600">
                              <IconPhone className="w-3.5 h-3.5 text-slate-400" />
                              <span>{u.phone}</span>
                            </div>
                          )}
                          {u.email && (
                            <div className="flex items-center gap-1 text-[11px] text-slate-400 mt-0.5">
                              <IconMail className="w-3.5 h-3.5" />
                              <span className="truncate max-w-[130px]">{u.email}</span>
                            </div>
                          )}
                        </td>
                        <td className="py-3 px-4 text-center">
                          <Badge variant={u.isActive ? 'success' : 'neutral'}>
                            {u.isActive ? 'نشط' : 'معطل'}
                          </Badge>
                        </td>
                        <td className="py-3 px-4 text-center">
                          <div className="flex items-center justify-center gap-1.5">
                            {canResetPassword && <button
                              type="button"
                              title="إعادة تعيين كلمة المرور"
                              onClick={() => {
                                setUserToReset(u);
                                setIsResetOpen(true);
                              }}
                              className="p-1.5 rounded-lg text-amber-700 hover:bg-amber-50 transition-colors"
                            >
                              <IconKey className="w-4 h-4" />
                            </button>}

                            {canEdit && <button
                              type="button"
                              title="تعديل المستخدم"
                              onClick={() => {
                                setUserToEdit(u);
                                setIsFormOpen(true);
                              }}
                              className="p-1.5 rounded-lg text-slate-600 hover:bg-slate-100 transition-colors"
                            >
                              <IconEdit className="w-4 h-4" />
                            </button>}

                            {canDelete && u.userName !== 'admin' && (
                              <button
                                type="button"
                                title="حذف المستخدم"
                                onClick={() => setUserToDelete(u)}
                                className="p-1.5 rounded-lg text-rose-600 hover:bg-rose-50 transition-colors"
                              >
                                <IconTrash className="w-4 h-4" />
                              </button>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <div className="p-3 border-t border-slate-100">
              <Pagination
                currentPage={pageNumber}
                totalPages={totalPages}
                totalItems={totalCount}
                pageSize={pageSize}
                onPageChange={setPageNumber}
                onPageSizeChange={(newSize) => {
                  setPageSize(newSize);
                  setPageNumber(1);
                }}
              />
            </div>
          </div>
        </div>
      )}

      {activeTab === 'roles' && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {roles.map((r) => (
            <div
              key={r.id}
              className="p-5 rounded-2xl bg-white border border-slate-200/80 shadow-2xs flex flex-col justify-between gap-4"
            >
              <div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2.5">
                    <div className="w-10 h-10 rounded-xl bg-slate-100 text-slate-800 flex items-center justify-center">
                      <IconShieldCheck className="w-5 h-5 text-emerald-600" />
                    </div>
                    <div>
                      <h3 className="font-extrabold text-sm text-slate-900 leading-tight">{r.label}</h3>
                      <span className="text-[11px] font-mono text-slate-400">{r.name}</span>
                    </div>
                  </div>

                  <Badge variant={getRoleBadgeVariant(r.name)}>
                    {r.name === 'Admin' ? 'كامل الصلاحيات' : 'صلاحيات مخصصة'}
                  </Badge>
                </div>

                <p className="mt-3 text-xs text-slate-500 leading-relaxed">{r.description}</p>
              </div>

              <div className="pt-3 border-t border-slate-100 flex justify-end">
                {canManageRoles && <Button
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    setSelectedRole({ id: r.id, name: r.name, label: r.label });
                    setIsMatrixOpen(true);
                  }}
                >
                  <IconShield className="w-4 h-4" />
                  إدارة وتعديل الصلاحيات
                </Button>}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* النوافذ المنبثقة */}
      <UserFormModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        userToEdit={userToEdit}
        onSaved={loadUsers}
      />

      {isResetOpen && <ResetPasswordModal
        isOpen={isResetOpen}
        onClose={() => setIsResetOpen(false)}
        user={userToReset}
      />}

      <PermissionsMatrixModal
        isOpen={isMatrixOpen}
        onClose={() => setIsMatrixOpen(false)}
        roleName={selectedRole?.name || null}
        roleId={selectedRole?.id || null}
        roleLabel={selectedRole?.label || ''}
      />

      <ConfirmDialog
        isOpen={!!userToDelete}
        onClose={() => setUserToDelete(null)}
        onConfirm={handleDelete}
        title="تأكيد حذف حساب المستخدم"
        message={`هل أنت متأكد من رغبتك في حذف حساب المستخدم "@${userToDelete?.userName}"؟`}
        confirmText="نعم، احذف الحساب"
        cancelText="إلغاء"
        variant="danger"
      />
    </div>
  );
};

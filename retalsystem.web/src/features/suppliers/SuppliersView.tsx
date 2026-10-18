import React, { useState, useEffect, useCallback } from 'react';
import { SupplierDto } from './types/supplier.types';
import { supplierService } from './services/supplierService';
import { SupplierFormModal } from './components/SupplierFormModal';
import { SupplierPaymentModal } from './components/SupplierPaymentModal';
import { SupplierStatementModal } from './components/SupplierStatementModal';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Badge } from '../../shared/components/ui/Badge';
import { ConfirmDialog } from '../../shared/components/ui/ConfirmDialog';
import { Pagination } from '../../shared/components/ui/Pagination';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { useToast } from '../../shared/context/ToastContext';
import { useAuth } from '../../shared/context/AuthContext';
import { Permissions } from '../../shared/auth/permissions';
import {
  IconPlus,
  IconSearch,
  IconTruckDelivery,
  IconCash,
  IconFileText,
  IconEdit,
  IconTrash,
  IconPhone,
  IconMail,
  IconBuildingStore,
  IconArrowDownLeft,
} from '@tabler/icons-react';

export const SuppliersView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const { hasPermission } = useAuth();
  const canCreate = hasPermission(Permissions.Suppliers.Create);
  const canEdit = hasPermission(Permissions.Suppliers.Edit);
  const canDelete = hasPermission(Permissions.Suppliers.Delete);
  const [suppliers, setSuppliers] = useState<SupplierDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);

  // الفلترة
  const [activeTab, setActiveTab] = useState<'all' | 'with_balance' | 'active'>('all');

  // الصفحات
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // النوافذ المنبثقة
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [supplierToEdit, setSupplierToEdit] = useState<SupplierDto | null>(null);

  const [isPaymentOpen, setIsPaymentOpen] = useState(false);
  const [supplierToPay, setSupplierToPay] = useState<SupplierDto | null>(null);

  const [isStatementOpen, setIsStatementOpen] = useState(false);
  const [supplierForStatement, setSupplierForStatement] = useState<SupplierDto | null>(null);

  const [supplierToDelete, setSupplierToDelete] = useState<SupplierDto | null>(null);

  const [supplierError, setSupplierError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      setSupplierError(null);
      const result = await supplierService.getPaged(pageNumber, pageSize, debouncedSearch);

      let items = result.items;
      if (activeTab === 'with_balance') {
        items = items.filter((s) => s.currentBalance > 0);
      } else if (activeTab === 'active') {
        items = items.filter((s) => s.isActive);
      }

      setSuppliers(items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل قائمة الموردين';
      setSupplierError(msg);
      showError('خطأ في تحميل الموردين', msg);
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, pageSize, debouncedSearch, activeTab, showError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // إحصائيات سريعة
  const totalPayables = suppliers.reduce((acc, s) => acc + (s.currentBalance || 0), 0);
  const suppliersWithBalance = suppliers.filter((s) => s.currentBalance > 0).length;

  const handleDelete = async () => {
    if (!supplierToDelete) return;
    try {
      await supplierService.delete(supplierToDelete.id);
      showSuccess(`تم حذف المورد "${supplierToDelete.name}" بنجاح`);
      setSupplierToDelete(null);
      loadData();
    } catch {
      showError('حدث خطأ أثناء محاولة حذف المورد');
    }
  };

  return (
    <div className="flex flex-col gap-5">
      {/* ترويسة الصفحة */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-slate-900 tracking-tight">إدارة الموردين والجهات الدائنة</h1>
          <p className="text-xs text-slate-500 font-medium mt-0.5">
            سجل الموردين، حسابات التوريد، مستحقات الشركات وسندات الصرف
          </p>
        </div>

        {canCreate && <Button
          variant="primary"
          onClick={() => {
            setSupplierToEdit(null);
            setIsFormOpen(true);
          }}
          className="self-start sm:self-auto"
        >
          <IconPlus className="w-4 h-4" />
          إضافة مورد جديد
        </Button>}
      </div>

      {/* بطاقات الإحصائيات والمؤشرات */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-slate-400">إجمالي الموردين</span>
            <div className="text-2xl font-black text-slate-900 mt-1">{totalCount}</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-slate-50 text-slate-700 flex items-center justify-center">
            <IconTruckDelivery className="w-6 h-6" />
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-amber-200/60 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-amber-600">إجمالي المستحقات للموردين (دائن)</span>
            <div className="text-2xl font-black text-amber-800 num-ltr text-right mt-1">
              {totalPayables.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
              <span className="text-xs font-normal">د.ل</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
            <IconArrowDownLeft className="w-6 h-6" />
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-emerald-200/60 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-emerald-600">موردين لهم مستحقات معلقة</span>
            <div className="text-2xl font-black text-emerald-800 mt-1">{suppliersWithBalance}</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
            <IconCash className="w-6 h-6" />
          </div>
        </div>
      </div>

      {supplierError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل بيانات الموردين من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{supplierError}</p>
          </div>
          <Button variant="secondary" size="sm" onClick={() => loadData()}>
            إعادة المحاولة
          </Button>
        </div>
      )}

      {/* شريط البحث والفلترة */}
      <div className="p-3.5 bg-white rounded-2xl border border-slate-200/80 shadow-2xs flex flex-col sm:flex-row items-center justify-between gap-3">
        <div className="w-full sm:w-80">
          <Input
            placeholder="بحث باسم المورد، اسم الشركة، أو الهاتف..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            leftIcon={<IconSearch className="w-4 h-4 text-slate-400" />}
          />
        </div>

        <div className="flex items-center gap-1.5 self-stretch sm:self-auto overflow-x-auto">
          <button
            type="button"
            onClick={() => setActiveTab('all')}
            className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
              activeTab === 'all'
                ? 'bg-slate-900 text-white'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            الكل
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('with_balance')}
            className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
              activeTab === 'with_balance'
                ? 'bg-amber-600 text-white'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            لهم مستحقات مالية
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('active')}
            className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
              activeTab === 'active'
                ? 'bg-emerald-600 text-white'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            النشطين فقط
          </button>
        </div>
      </div>

      {/* جدول الموردين */}
      <div className="relative bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <LoadingOverlay isVisible={isLoading} />

        {suppliers.length === 0 && !isLoading ? (
          <EmptyState
            icon={<IconTruckDelivery className="w-10 h-10 text-slate-300" />}
            title="لا يوجد موردين مطابقين"
            description="لم يتم العثور على أي مورد مسجل يطابق معايير البحث. يمكنك إضافة مورد جديد الآن."
            actionLabel={canCreate ? 'إضافة مورد جديد' : undefined}
            onAction={canCreate ? () => {
              setSupplierToEdit(null);
              setIsFormOpen(true);
            } : undefined}
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-right text-xs min-w-[700px]">
              <thead className="bg-slate-50/90 border-b border-slate-200 text-slate-500 font-bold">
                <tr>
                  <th className="py-3 px-4">المورد والشركة</th>
                  <th className="py-3 px-4">الاتصال</th>
                  <th className="py-3 px-4">الرقم الضريبي</th>
                  <th className="py-3 px-4 text-left">الرصيد المستحق (دائن)</th>
                  <th className="py-3 px-4 text-center">الحالة</th>
                  <th className="py-3 px-4 text-center">إجراءات سريعة</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 font-medium">
                {suppliers.map((s) => (
                  <tr key={s.id} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-3 px-4">
                      <div className="font-extrabold text-slate-900 text-sm">{s.name}</div>
                      {s.companyName && (
                        <div className="flex items-center gap-1 text-[11px] text-slate-500 mt-0.5">
                          <IconBuildingStore className="w-3.5 h-3.5 text-slate-400" />
                          <span>{s.companyName}</span>
                        </div>
                      )}
                    </td>

                    <td className="py-3 px-4">
                      <div className="flex items-center gap-1.5 font-mono text-slate-700 num-ltr text-right">
                        <IconPhone className="w-3.5 h-3.5 text-slate-400" />
                        <span>{s.phone}</span>
                      </div>
                      {s.email && (
                        <div className="flex items-center gap-1.5 text-slate-400 text-[11px] mt-0.5">
                          <IconMail className="w-3.5 h-3.5" />
                          <span className="truncate max-w-[150px]">{s.email}</span>
                        </div>
                      )}
                    </td>

                    <td className="py-3 px-4">
                      {s.taxNumber ? (
                        <div className="font-mono text-slate-700 num-ltr text-right">
                          {s.taxNumber}
                        </div>
                      ) : (
                        <span className="text-slate-300">-</span>
                      )}
                    </td>

                    <td className="py-3 px-4 text-left num-ltr">
                      {s.currentBalance > 0 ? (
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-black bg-amber-50 text-amber-800 border border-amber-200/80">
                          {s.currentBalance.toLocaleString('ar-SA', { minimumFractionDigits: 2 })} د.ل
                        </span>
                      ) : (
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold bg-slate-100 text-slate-500">
                          0.00 د.ل
                        </span>
                      )}
                    </td>

                    <td className="py-3 px-4 text-center">
                      <Badge variant={s.isActive ? 'success' : 'neutral'}>
                        {s.isActive ? 'نشط' : 'معطل'}
                      </Badge>
                    </td>

                    <td className="py-3 px-4 text-center">
                      <div className="flex items-center justify-center gap-1.5">
                        {canEdit && <button
                          type="button"
                          title="تسجيل سند صرف"
                          onClick={() => {
                            setSupplierToPay(s);
                            setIsPaymentOpen(true);
                          }}
                          className="p-1.5 rounded-lg text-emerald-700 hover:bg-emerald-50 transition-colors"
                        >
                          <IconCash className="w-4 h-4" />
                        </button>}

                        <button
                          type="button"
                          title="كشف حساب المورد"
                          onClick={() => {
                            setSupplierForStatement(s);
                            setIsStatementOpen(true);
                          }}
                          className="p-1.5 rounded-lg text-indigo-700 hover:bg-indigo-50 transition-colors"
                        >
                          <IconFileText className="w-4 h-4" />
                        </button>

                        {canEdit && <button
                          type="button"
                          title="تعديل بيانات المورد"
                          onClick={() => {
                            void supplierService.getById(s.id).then((details) => {
                              setSupplierToEdit(details);
                              setIsFormOpen(true);
                            }).catch(() => showError('تعذر تحميل تفاصيل المورد للتعديل'));
                          }}
                          className="p-1.5 rounded-lg text-slate-600 hover:bg-slate-100 transition-colors"
                        >
                          <IconEdit className="w-4 h-4" />
                        </button>}

                        {canDelete && <button
                          type="button"
                          title="حذف المورد"
                          onClick={() => setSupplierToDelete(s)}
                          className="p-1.5 rounded-lg text-rose-600 hover:bg-rose-50 transition-colors"
                        >
                          <IconTrash className="w-4 h-4" />
                        </button>}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}

        {/* ترقيم الصفحات */}
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

      {/* النوافذ المنبثقة */}
      <SupplierFormModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        supplierToEdit={supplierToEdit}
        onSaved={loadData}
      />

      <SupplierPaymentModal
        isOpen={isPaymentOpen}
        onClose={() => setIsPaymentOpen(false)}
        supplier={supplierToPay}
        onPaymentSuccess={loadData}
      />

      <SupplierStatementModal
        isOpen={isStatementOpen}
        onClose={() => setIsStatementOpen(false)}
        supplier={supplierForStatement}
      />

      <ConfirmDialog
        isOpen={!!supplierToDelete}
        onClose={() => setSupplierToDelete(null)}
        onConfirm={handleDelete}
        title="تأكيد حذف المورد"
        message={`هل أنت متأكد من رغبتك في حذف المورد "${supplierToDelete?.name}"؟ لا يمكن التراجع عن هذا الإجراء.`}
        confirmText="نعم، احذف المورد"
        cancelText="إلغاء"
        variant="danger"
      />
    </div>
  );
};

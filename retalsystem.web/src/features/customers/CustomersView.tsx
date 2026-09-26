import React, { useState, useEffect, useCallback } from 'react';
import { CustomerDto } from './types/customer.types';
import { customerService } from './services/customerService';
import { CustomerFormModal } from './components/CustomerFormModal';
import { CustomerPaymentModal } from './components/CustomerPaymentModal';
import { CustomerStatementModal } from './components/CustomerStatementModal';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Badge } from '../../shared/components/ui/Badge';
import { ConfirmDialog } from '../../shared/components/ui/ConfirmDialog';
import { Pagination } from '../../shared/components/ui/Pagination';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { useToast } from '../../shared/context/ToastContext';
import {
  IconPlus,
  IconSearch,
  IconUsers,
  IconCash,
  IconFileText,
  IconEdit,
  IconTrash,
  IconPhone,
  IconMail,
  IconArrowUpRight,
} from '@tabler/icons-react';

export const CustomersView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);

  // الفلترة
  const [activeTab, setActiveTab] = useState<'all' | 'active' | 'with_balance'>('all');

  // الصفحات
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);

  // النوافذ المنبثقة
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [customerToEdit, setCustomerToEdit] = useState<CustomerDto | null>(null);

  const [isPaymentOpen, setIsPaymentOpen] = useState(false);
  const [customerToPay, setCustomerToPay] = useState<CustomerDto | null>(null);

  const [isStatementOpen, setIsStatementOpen] = useState(false);
  const [customerForStatement, setCustomerForStatement] = useState<CustomerDto | null>(null);

  const [customerToDelete, setCustomerToDelete] = useState<CustomerDto | null>(null);

  const [customerError, setCustomerError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      setIsLoading(true);
      setCustomerError(null);
      const isActiveParam = activeTab === 'all' ? undefined : activeTab === 'active';
      const result = await customerService.getPaged(pageNumber, pageSize, debouncedSearch, isActiveParam);

      let items = result.items;
      if (activeTab === 'with_balance') {
        items = items.filter((c) => c.currentBalance > 0);
      }

      setCustomers(items);
      setTotalPages(result.totalPages);
      setTotalCount(result.totalCount);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل قائمة العملاء';
      setCustomerError(msg);
      showError('خطأ في تحميل العملاء', msg);
    } finally {
      setIsLoading(false);
    }
  }, [pageNumber, pageSize, debouncedSearch, activeTab, showError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // إحصائيات سريعة
  const totalReceivables = customers.reduce((acc, c) => acc + (c.currentBalance || 0), 0);
  const customersWithDebt = customers.filter((c) => c.currentBalance > 0).length;

  const handleDelete = async () => {
    if (!customerToDelete) return;
    try {
      await customerService.delete(customerToDelete.id);
      showSuccess(`تم حذف العميل "${customerToDelete.name}" بنجاح`);
      setCustomerToDelete(null);
      loadData();
    } catch {
      showError('حدث خطأ أثناء محاولة حذف العميل');
    }
  };

  return (
    <div className="flex flex-col gap-5">
      {/* الترويسة العليا */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-slate-900 tracking-tight">إدارة العملاء والذمم</h1>
          <p className="text-xs text-slate-500 font-medium mt-0.5">
            سجل العملاء، إدارة الأرصدة المدينة، الحدود الائتمانية وسندات القبض
          </p>
        </div>

        <Button
          variant="primary"
          onClick={() => {
            setCustomerToEdit(null);
            setIsFormOpen(true);
          }}
          className="self-start sm:self-auto"
        >
          <IconPlus className="w-4 h-4" />
          إضافة عميل جديد
        </Button>
      </div>

      {/* بطاقات المؤشرات الإحصائية */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-slate-400">إجمالي عدد العملاء</span>
            <div className="text-2xl font-black text-slate-900 mt-1">{totalCount}</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-slate-50 text-slate-700 flex items-center justify-center">
            <IconUsers className="w-6 h-6" />
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-rose-200/60 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-rose-500">إجمالي المديونيات المستحقة</span>
            <div className="text-2xl font-black text-rose-700 num-ltr text-right mt-1">
              {totalReceivables.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
              <span className="text-xs font-normal">د.ل</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-rose-50 text-rose-600 flex items-center justify-center">
            <IconArrowUpRight className="w-6 h-6" />
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-emerald-200/60 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-emerald-600">عملاء لديهم أرصدة مدينة</span>
            <div className="text-2xl font-black text-emerald-800 mt-1">{customersWithDebt}</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
            <IconCash className="w-6 h-6" />
          </div>
        </div>
      </div>

      {customerError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل بيانات العملاء من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{customerError}</p>
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
            placeholder="بحث بالاسم، الجوال، أو الرقم الضريبي..."
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
                ? 'bg-rose-600 text-white'
                : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            عليهم مديونيات
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

      {/* جدول بيانات العملاء */}
      <div className="relative bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
        <LoadingOverlay isVisible={isLoading} />

        {customers.length === 0 && !isLoading ? (
          <EmptyState
            icon={<IconUsers className="w-10 h-10 text-slate-300" />}
            title="لا يوجد عملاء مطابقين"
            description="لم يتم العثور على أي عميل يطابق معايير البحث المحددة. يمكنك إضافة عميل جديد الآن."
            actionLabel="إضافة عميل جديد"
            onAction={() => {
              setCustomerToEdit(null);
              setIsFormOpen(true);
            }}
          />
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-right text-xs">
              <thead className="bg-slate-50/90 border-b border-slate-200 text-slate-500 font-bold">
                <tr>
                  <th className="py-3 px-4">العميل</th>
                  <th className="py-3 px-4">معلومات الاتصال</th>
                  <th className="py-3 px-4">الرقم الضريبي / السجل</th>
                  <th className="py-3 px-4 text-left">سقف الائتمان</th>
                  <th className="py-3 px-4 text-left">الرصيد الحالي</th>
                  <th className="py-3 px-4 text-center">الحالة</th>
                  <th className="py-3 px-4 text-center">إجراءات سريعة</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 font-medium">
                {customers.map((c) => (
                  <tr key={c.id} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-3 px-4">
                      <div className="font-extrabold text-slate-900 text-sm">{c.name}</div>
                      {c.address && (
                        <div className="text-[11px] text-slate-400 mt-0.5 truncate max-w-xs">
                          {c.address}
                        </div>
                      )}
                    </td>

                    <td className="py-3 px-4">
                      <div className="flex items-center gap-1.5 font-mono text-slate-700 num-ltr text-right">
                        <IconPhone className="w-3.5 h-3.5 text-slate-400" />
                        <span>{c.phone}</span>
                      </div>
                      {c.email && (
                        <div className="flex items-center gap-1.5 text-slate-400 text-[11px] mt-0.5">
                          <IconMail className="w-3.5 h-3.5" />
                          <span className="truncate max-w-[150px]">{c.email}</span>
                        </div>
                      )}
                    </td>

                    <td className="py-3 px-4">
                      {c.taxNumber ? (
                        <div className="font-mono text-slate-700 num-ltr text-right">
                          {c.taxNumber}
                        </div>
                      ) : (
                        <span className="text-slate-300">-</span>
                      )}
                      {c.commercialRegister && (
                        <div className="text-[10px] text-slate-400 font-mono mt-0.5 num-ltr text-right">
                          س.ت: {c.commercialRegister}
                        </div>
                      )}
                    </td>

                    <td className="py-3 px-4 text-left num-ltr font-bold text-slate-700">
                      {c.creditLimit > 0
                        ? `${c.creditLimit.toLocaleString('ar-SA')} د.ل`
                        : 'غير محدد'}
                    </td>

                    <td className="py-3 px-4 text-left num-ltr">
                      {c.currentBalance > 0 ? (
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-black bg-rose-50 text-rose-700 border border-rose-200/80">
                          {c.currentBalance.toLocaleString('ar-SA', { minimumFractionDigits: 2 })} د.ل
                        </span>
                      ) : (
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold bg-slate-100 text-slate-500">
                          0.00 د.ل
                        </span>
                      )}
                    </td>

                    <td className="py-3 px-4 text-center">
                      <Badge variant={c.isActive ? 'success' : 'neutral'}>
                        {c.isActive ? 'نشط' : 'معطل'}
                      </Badge>
                    </td>

                    <td className="py-3 px-4 text-center">
                      <div className="flex items-center justify-center gap-1.5">
                        <button
                          type="button"
                          title="تسجيل سند قبض"
                          onClick={() => {
                            setCustomerToPay(c);
                            setIsPaymentOpen(true);
                          }}
                          className="p-1.5 rounded-lg text-emerald-700 hover:bg-emerald-50 transition-colors"
                        >
                          <IconCash className="w-4 h-4" />
                        </button>

                        <button
                          type="button"
                          title="كشف حساب العميل"
                          onClick={() => {
                            setCustomerForStatement(c);
                            setIsStatementOpen(true);
                          }}
                          className="p-1.5 rounded-lg text-indigo-700 hover:bg-indigo-50 transition-colors"
                        >
                          <IconFileText className="w-4 h-4" />
                        </button>

                        <button
                          type="button"
                          title="تعديل بيانات العميل"
                          onClick={() => {
                            setCustomerToEdit(c);
                            setIsFormOpen(true);
                          }}
                          className="p-1.5 rounded-lg text-slate-600 hover:bg-slate-100 transition-colors"
                        >
                          <IconEdit className="w-4 h-4" />
                        </button>

                        <button
                          type="button"
                          title="حذف العميل"
                          onClick={() => setCustomerToDelete(c)}
                          className="p-1.5 rounded-lg text-rose-600 hover:bg-rose-50 transition-colors"
                        >
                          <IconTrash className="w-4 h-4" />
                        </button>
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
      <CustomerFormModal
        isOpen={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        customerToEdit={customerToEdit}
        onSaved={loadData}
      />

      <CustomerPaymentModal
        isOpen={isPaymentOpen}
        onClose={() => setIsPaymentOpen(false)}
        customer={customerToPay}
        onPaymentSuccess={loadData}
      />

      <CustomerStatementModal
        isOpen={isStatementOpen}
        onClose={() => setIsStatementOpen(false)}
        customer={customerForStatement}
      />

      <ConfirmDialog
        isOpen={!!customerToDelete}
        onClose={() => setCustomerToDelete(null)}
        onConfirm={handleDelete}
        title="تأكيد حذف العميل"
        message={`هل أنت متأكد من رغبتك في حذف العميل "${customerToDelete?.name}"؟ لا يمكن التراجع عن هذا الإجراء إذا كانت هناك حركات مسجلة.`}
        confirmText="نعم، احذف العميل"
        cancelText="إلغاء"
        variant="danger"
      />
    </div>
  );
};

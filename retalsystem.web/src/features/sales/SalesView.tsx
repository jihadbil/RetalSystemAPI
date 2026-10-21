import React, { useState, useEffect, useCallback, useRef } from 'react';
import { salesService } from './services/salesService';
import type { SalesInvoiceDto, SalesReturnDto } from './types/sales.types';
import { InvoiceStatusLabels } from './types/sales.types';
import { SalesInvoiceModal } from './components/SalesInvoiceModal';
import { SalesInvoiceDetailsModal } from './components/SalesInvoiceDetailsModal';
import { SalesReturnModal } from './components/SalesReturnModal';
import { InvoicePrintModal } from './components/InvoicePrintModal';
import { Card } from '../../shared/components/ui/Card';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Tabs } from '../../shared/components/ui/Tabs';
import { Pagination } from '../../shared/components/ui/Pagination';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { usePagination } from '../../shared/hooks/usePagination';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { useToast } from '../../shared/context/ToastContext';
import { useAuth } from '../../shared/context/AuthContext';
import { Permissions } from '../../shared/auth/permissions';
import {
  IconReceipt,
  IconArrowBackUp,
  IconPlus,
  IconSearch,
  IconPrinter,
  IconEye,
} from '@tabler/icons-react';

export const SalesView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const { hasPermission, currentBranchId, isCashier } = useAuth();
  const canViewInvoices = hasPermission(Permissions.SalesInvoices.View);
  const canCreateInvoice = !isCashier && hasPermission(Permissions.SalesInvoices.Create);
  const canCreateReturn = hasPermission(Permissions.SalesReturns.Create);
  const canViewReturns = hasPermission(Permissions.SalesReturns.View);
  const canPrint = hasPermission(Permissions.SalesInvoices.Print);
  const [selectedTab, setActiveTab] = useState<'invoices' | 'returns'>('invoices');
  const activeTab = (selectedTab === 'invoices' ? canViewInvoices : canViewReturns)
    ? selectedTab : canViewInvoices ? 'invoices' : 'returns';

  const [invoices, setInvoices] = useState<SalesInvoiceDto[]>([]);
  const [returns, setReturns] = useState<SalesReturnDto[]>([]);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);
  const [isLoading, setIsLoading] = useState(false);

  const pagination = usePagination({ initialPageSize: 10 });
  const { page, pageSize, setTotalCount, resetPage } = pagination;
  const loadVersion = useRef(0);
  useEffect(() => { resetPage(); }, [currentBranchId, debouncedSearch, activeTab, resetPage]);

  // النوافذ المنبثقة
  const [isInvoiceModalOpen, setIsInvoiceModalOpen] = useState(false);
  const [isReturnModalOpen, setIsReturnModalOpen] = useState(false);
  const [viewingInvoice, setViewingInvoice] = useState<SalesInvoiceDto | null>(null);
  const [selectedDetailsInvoiceId, setSelectedDetailsInvoiceId] = useState<string | null>(null);

  const [loadError, setLoadError] = useState<string | null>(null);

  const loadSalesData = useCallback(async () => {
    const version = ++loadVersion.current;
    setIsLoading(true);
    setLoadError(null);
    try {
      if (activeTab === 'invoices') {
        const res = await salesService.getInvoices({
          pageNumber: page,
          pageSize,
          search: debouncedSearch,
          branchId: currentBranchId || undefined,
        });
        if (version !== loadVersion.current) return;
        setInvoices(res.items);
        setTotalCount(res.totalCount);
      } else {
        const rets = await salesService.getReturns({ branchId: currentBranchId || undefined });
        if (version !== loadVersion.current) return;
        setReturns(rets);
      }
    } catch (err: unknown) {
      if (version !== loadVersion.current) return;
      const msg = err instanceof Error ? err.message : 'فشل تحميل بيانات المبيعات';
      setLoadError(msg);
      showError('خطأ في تحميل المبيعات', msg);
    } finally {
      if (version === loadVersion.current) setIsLoading(false);
    }
  }, [activeTab, page, pageSize, setTotalCount, debouncedSearch, showError, currentBranchId]);

  useEffect(() => {
    loadSalesData();
  }, [loadSalesData]);

  const tabs = [
    ...(canViewInvoices ? [{ id: 'invoices', label: 'فواتير المبيعات', icon: <IconReceipt className="w-4 h-4" />, badge: pagination.totalCount }] : []),
    ...(canViewReturns
      ? [{ id: 'returns', label: 'مرتجعات المبيعات', icon: <IconArrowBackUp className="w-4 h-4" />, badge: returns.length }]
      : []),
  ];

  return (
    <div className="flex flex-col gap-5 max-w-7xl mx-auto">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-3">
        <div>
          <h1 className="text-xl sm:text-2xl font-extrabold text-slate-900 tracking-tight">
            إدارة المبيعات والمرتجعات
          </h1>
          <p className="text-xs text-slate-500 mt-1">
            سجل فواتير البيع الصادرة، التحصيل المالي، طباعة الفواتير A4، ومرتجعات العملاء
          </p>
        </div>

        {activeTab === 'invoices' && canCreateInvoice ? (
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconPlus className="w-4 h-4" />}
            onClick={() => setIsInvoiceModalOpen(true)}
          >
            + فاتورة بيع جديدة
          </Button>
        ) : activeTab === 'returns' && canCreateReturn ? (
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconPlus className="w-4 h-4" />}
            onClick={() => setIsReturnModalOpen(true)}
          >
            + تسجيل مرتجع مبيعات
          </Button>
        ) : null}
      </div>

      <Tabs tabs={tabs} activeTab={activeTab} onChange={(t) => setActiveTab(t as typeof activeTab)} />

      {loadError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل البيانات من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{loadError}</p>
          </div>
          <Button variant="secondary" size="sm" onClick={() => loadSalesData()}>
            إعادة المحاولة
          </Button>
        </div>
      )}

      {/* تبويب فواتير المبيعات */}
      {activeTab === 'invoices' && (
        <Card noPadding>
          <div className="p-4 border-b border-slate-100 flex items-center justify-between bg-slate-50/40">
            <div className="w-full sm:w-80">
              <Input
                placeholder="ابحث برقم الفاتورة أو اسم العميل..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                leftIcon={<IconSearch className="w-4 h-4" />}
              />
            </div>
          </div>

          <LoadingOverlay isLoading={isLoading}>
            {invoices.length === 0 ? (
              <div className="py-12">
                <EmptyState
                  isSearch={Boolean(search)}
                  actionText={canCreateInvoice ? '+ إنشاء أول فاتورة' : undefined}
                  onAction={canCreateInvoice ? () => setIsInvoiceModalOpen(true) : undefined}
                />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs min-w-[700px]">
                  <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                    <tr>
                      <th className="py-3 px-4">رقم الفاتورة</th>
                      <th className="py-3 px-3">التاريخ</th>
                      <th className="py-3 px-3">العميل</th>
                      <th className="py-3 px-3">الفرع / المستودع</th>
                      <th className="py-3 px-3 text-center">الإجمالي</th>
                      <th className="py-3 px-3 text-center">المدفوع</th>
                      <th className="py-3 px-3 text-center">المتبقي</th>
                      <th className="py-3 px-3 text-center">الحالة</th>
                      <th className="py-3 px-4 text-center w-28">إجراءات</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {invoices.map((inv) => {
                      const st = InvoiceStatusLabels[inv.status] || { text: 'غير محدد', bg: 'bg-slate-50', color: 'text-slate-700' };
                      return (
                        <tr key={inv.id} className="hover:bg-slate-50/70 transition-colors">
                          <td className="py-3 px-4 font-mono font-bold text-slate-800">{inv.invoiceNumber}</td>
                          <td className="py-3 px-3 text-slate-500">
                            {new Date(inv.invoiceDate).toLocaleDateString('ar-LY')}
                          </td>
                          <td className="py-3 px-3 font-semibold text-slate-700">{inv.customerName || 'عميل نقدي'}</td>
                          <td className="py-3 px-3 text-slate-500">{inv.branchName || 'الرئيسي'}</td>
                          <td className="py-3 px-3 text-center font-extrabold text-slate-900 num-ltr">
                            {inv.totalAmount.toFixed(2)} د.ل
                          </td>
                          <td className="py-3 px-3 text-center font-bold text-emerald-700 num-ltr">
                            {inv.paidAmount.toFixed(2)} د.ل
                          </td>
                          <td className="py-3 px-3 text-center font-semibold text-rose-600 num-ltr">
                            {inv.remainingAmount > 0 ? `${inv.remainingAmount.toFixed(2)} د.ل` : '-'}
                          </td>
                          <td className="py-3 px-3 text-center">
                            <span className={`inline-block px-2.5 py-0.5 rounded-full text-[11px] font-bold ${st.bg} ${st.color}`}>
                              {st.text}
                            </span>
                          </td>
                          <td className="py-3 px-4 text-center">
                            <div className="flex items-center justify-center gap-1">
                          <button
                                type="button"
                                onClick={() => setSelectedDetailsInvoiceId(inv.id)}
                                className="p-1.5 text-slate-500 hover:text-sky-700 hover:bg-sky-50 rounded-lg transition-colors cursor-pointer"
                                title="عرض تفاصيل وبنود الفاتورة"
                              >
                                <IconEye className="w-4 h-4" />
                          </button>
                          {canPrint && <button
                                type="button"
                                onClick={() => setViewingInvoice(inv)}
                                className="p-1.5 text-slate-500 hover:text-emerald-700 hover:bg-emerald-50 rounded-lg transition-colors cursor-pointer"
                                title="معاينة وطباعة الفاتورة A4"
                              >
                                <IconPrinter className="w-4 h-4" />
                          </button>}
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </LoadingOverlay>

          <Pagination
            currentPage={pagination.page}
            totalPages={pagination.totalPages}
            totalCount={pagination.totalCount}
            pageSize={pagination.pageSize}
            onPageChange={pagination.goToPage}
            isLoading={isLoading}
          />
        </Card>
      )}

      {/* تبويب مرتجعات المبيعات */}
      {activeTab === 'returns' && (
        <Card noPadding>
          <div className="overflow-x-auto">
            <table className="w-full text-right text-xs min-w-[650px]">
              <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                <tr>
                  <th className="py-3 px-4">رقم المرتجع</th>
                  <th className="py-3 px-3">الفاتورة الأصلية</th>
                  <th className="py-3 px-3">التاريخ</th>
                  <th className="py-3 px-3">العميل</th>
                  <th className="py-3 px-3 text-center">المبلغ المسترد</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {returns.map((ret) => (
                  <tr key={ret.id} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-3 px-4 font-mono font-bold text-slate-800">{ret.returnNumber}</td>
                    <td className="py-3 px-3 font-mono text-slate-600">{ret.originalInvoiceNumber || '-'}</td>
                    <td className="py-3 px-3 text-slate-500">
                      {new Date(ret.returnDate).toLocaleDateString('ar-LY')}
                    </td>
                    <td className="py-3 px-3 text-slate-700">{ret.customerName || 'عميل نقدي'}</td>
                    <td className="py-3 px-3 text-center font-extrabold text-rose-700 num-ltr">
                      {ret.totalAmount.toFixed(2)} د.ل
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      )}

      {/* نافذة إنشاء فاتورة مبيعات */}
      <SalesInvoiceModal
        isOpen={isInvoiceModalOpen}
        onClose={() => setIsInvoiceModalOpen(false)}
        onSave={async (data) => {
          try {
            await salesService.createInvoice(data);
            showSuccess('تم إصدار الفاتورة', 'تم حفظ فاتورة المبيعات بنجاح');
            loadSalesData();
            setIsInvoiceModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل حفظ الفاتورة على الخادم';
            showError('خطأ أثناء حفظ الفاتورة', msg);
            throw err;
          }
        }}
      />

      {/* نافذة تفاصيل وبنود فاتورة المبيعات */}
      <SalesInvoiceDetailsModal
        isOpen={Boolean(selectedDetailsInvoiceId)}
        onClose={() => setSelectedDetailsInvoiceId(null)}
        invoiceId={selectedDetailsInvoiceId}
      />

      {/* نافذة معاينة وطباعة الفاتورة A4 */}
      <InvoicePrintModal
        isOpen={Boolean(viewingInvoice)}
        onClose={() => setViewingInvoice(null)}
        invoice={viewingInvoice}
      />

      {/* نافذة إنشاء مرتجع مبيعات */}
      <SalesReturnModal
        isOpen={isReturnModalOpen}
        onClose={() => setIsReturnModalOpen(false)}
        onSave={async (data) => {
          try {
            await salesService.createReturn(data);
            showSuccess('تم تسجيل المرتجع', 'تم اعتماد مرتجع المبيعات بنجاح');
            loadSalesData();
            setIsReturnModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل حفظ مرتجع المبيعات';
            showError('خطأ أثناء حفظ المرتجع', msg);
            throw err;
          }
        }}
      />
    </div>
  );
};

import React, { useState, useEffect, useCallback } from 'react';
import { purchaseService } from './services/purchaseService';
import type {
  PurchaseInvoiceDto,
  PurchaseOrderDto,
  PurchaseReturnDto,
} from './types/purchase.types';
import { PurchaseInvoiceModal } from './components/PurchaseInvoiceModal';
import { PurchaseInvoiceDetailsModal } from './components/PurchaseInvoiceDetailsModal';
import { PurchaseOrderModal } from './components/PurchaseOrderModal';
import { PurchaseReturnModal } from './components/PurchaseReturnModal';
import { Card } from '../../shared/components/ui/Card';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Badge } from '../../shared/components/ui/Badge';
import { Tabs } from '../../shared/components/ui/Tabs';
import { Pagination } from '../../shared/components/ui/Pagination';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { usePagination } from '../../shared/hooks/usePagination';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { useToast } from '../../shared/context/ToastContext';
import {
  IconShoppingCart,
  IconFileInvoice,
  IconArrowBackUp,
  IconPlus,
  IconSearch,
  IconEye,
} from '@tabler/icons-react';
import { PurchaseStatusLabels } from './types/purchase.types';

export const PurchaseView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const [activeTab, setActiveTab] = useState<'invoices' | 'orders' | 'returns'>('invoices');

  const [invoices, setInvoices] = useState<PurchaseInvoiceDto[]>([]);
  const [orders, setOrders] = useState<PurchaseOrderDto[]>([]);
  const [returns, setReturns] = useState<PurchaseReturnDto[]>([]);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);
  const [isLoading, setIsLoading] = useState(false);

  const pagination = usePagination({ initialPageSize: 10 });
  const { page, pageSize, setTotalCount } = pagination;
  const [isInvoiceModalOpen, setIsInvoiceModalOpen] = useState(false);
  const [isOrderModalOpen, setIsOrderModalOpen] = useState(false);
  const [isReturnModalOpen, setIsReturnModalOpen] = useState(false);
  const [isDetailsModalOpen, setIsDetailsModalOpen] = useState(false);
  const [selectedInvoiceId, setSelectedInvoiceId] = useState<string | null>(null);

  const [loadError, setLoadError] = useState<string | null>(null);

  const loadPurchaseData = useCallback(async () => {
    setIsLoading(true);
    setLoadError(null);
    try {
      if (activeTab === 'invoices') {
        const res = await purchaseService.getInvoices({
          pageNumber: page,
          pageSize,
          search: debouncedSearch,
        });
        setInvoices(res.items);
        setTotalCount(res.totalCount);
      } else if (activeTab === 'orders') {
        const ords = await purchaseService.getOrders();
        setOrders(ords);
      } else {
        const rets = await purchaseService.getReturns();
        setReturns(rets);
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل بيانات المشتريات';
      setLoadError(msg);
      showError('خطأ في تحميل المشتريات', msg);
    } finally {
      setIsLoading(false);
    }
  }, [activeTab, page, pageSize, setTotalCount, debouncedSearch, showError]);

  useEffect(() => {
    loadPurchaseData();
  }, [loadPurchaseData]);

  const tabs = [
    { id: 'invoices', label: 'فواتير المشتريات', icon: <IconFileInvoice className="w-4 h-4" />, badge: pagination.totalCount },
    { id: 'orders', label: 'أوامر الشراء', icon: <IconShoppingCart className="w-4 h-4" />, badge: orders.length },
    { id: 'returns', label: 'مرتجعات الموردين', icon: <IconArrowBackUp className="w-4 h-4" />, badge: returns.length },
  ];

  return (
    <div className="flex flex-col gap-5 max-w-7xl mx-auto">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-3">
        <div>
          <h1 className="text-xl sm:text-2xl font-extrabold text-slate-900 tracking-tight">
            إدارة المشتريات والتوريدات
          </h1>
          <p className="text-xs text-slate-500 mt-1">
            تسجيل فواتير الشراء، تتبع أوامر التوريد، وتسجيل المرتجعات للموردين
          </p>
        </div>

        {activeTab === 'invoices' && (
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconPlus className="w-4 h-4" />}
            onClick={() => setIsInvoiceModalOpen(true)}
          >
            + فاتورة شراء جديدة
          </Button>
        )}
        {activeTab === 'orders' && (
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconPlus className="w-4 h-4" />}
            onClick={() => setIsOrderModalOpen(true)}
          >
            + أمر شراء جديد
          </Button>
        )}
        {activeTab === 'returns' && (
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconPlus className="w-4 h-4" />}
            onClick={() => setIsReturnModalOpen(true)}
          >
            + تسجيل مرتجع مورد
          </Button>
        )}
      </div>

      <Tabs tabs={tabs} activeTab={activeTab} onChange={(t) => setActiveTab(t as typeof activeTab)} />

      {loadError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل البيانات من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{loadError}</p>
          </div>
          <Button variant="secondary" size="sm" onClick={() => loadPurchaseData()}>
            إعادة المحاولة
          </Button>
        </div>
      )}

      {/* تبويب فواتير المشتريات */}
      {activeTab === 'invoices' && (
        <Card noPadding>
          <div className="p-4 border-b border-slate-100 flex items-center justify-between bg-slate-50/40">
            <div className="w-full sm:w-80">
              <Input
                placeholder="ابحث برقم الفاتورة أو اسم المورد..."
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
                  actionText="+ تسجيل أول فاتورة شراء"
                  onAction={() => setIsInvoiceModalOpen(true)}
                />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs">
                  <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                    <tr>
                      <th className="py-3 px-4">رقم الفاتورة</th>
                      <th className="py-3 px-3">التاريخ</th>
                      <th className="py-3 px-3">المورد</th>
                      <th className="py-3 px-3">المستودع المستلم</th>
                      <th className="py-3 px-3 text-center">إجمالي التكلفة</th>
                      <th className="py-3 px-3 text-center">المدفوع</th>
                      <th className="py-3 px-3 text-center">المتبقي للمورد</th>
                      <th className="py-3 px-4 text-center">الحالة</th>
                      <th className="py-3 px-3 text-center">الإجراءات</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {invoices.map((inv) => (
                      <tr key={inv.id} className="hover:bg-slate-50/70 transition-colors">
                        <td className="py-3 px-4 font-mono font-bold text-slate-800">{inv.invoiceNumber}</td>
                        <td className="py-3 px-3 text-slate-500">
                          {new Date(inv.invoiceDate).toLocaleDateString('ar-LY')}
                        </td>
                        <td className="py-3 px-3 font-semibold text-slate-800">{inv.supplierName}</td>
                        <td className="py-3 px-3 text-slate-500">{inv.warehouseName}</td>
                        <td className="py-3 px-3 text-center font-extrabold text-slate-900 num-ltr">
                          {inv.totalAmount.toFixed(2)} د.ل
                        </td>
                        <td className="py-3 px-3 text-center font-bold text-emerald-700 num-ltr">
                          {inv.paidAmount.toFixed(2)} د.ل
                        </td>
                        <td className="py-3 px-3 text-center font-semibold text-rose-600 num-ltr">
                          {inv.remainingAmount > 0 ? `${inv.remainingAmount.toFixed(2)} د.ل` : '-'}
                        </td>
                        <td className="py-3 px-4 text-center">
                          {inv.status === 2 ? (
                            <Badge variant="success" size="sm">
                              مغلقة ومرحلة
                            </Badge>
                          ) : inv.status === 1 ? (
                            <Badge variant="warning" size="sm">
                              قيد الإدخال
                            </Badge>
                          ) : (
                            <Badge variant="info" size="sm">
                              {PurchaseStatusLabels[inv.status]?.text || 'مسودة'}
                            </Badge>
                          )}
                        </td>
                        <td className="py-3 px-3 text-center">
                          <button
                            type="button"
                            onClick={() => {
                              setSelectedInvoiceId(inv.id);
                              setIsDetailsModalOpen(true);
                            }}
                            className="p-1 rounded-lg hover:bg-slate-200 text-slate-500 hover:text-emerald-700 transition-colors"
                            title="عرض تفاصيل الفاتورة والبنود"
                          >
                            <IconEye className="w-4 h-4" />
                          </button>
                        </td>
                      </tr>
                    ))}
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

      {/* تبويب أوامر الشراء */}
      {activeTab === 'orders' && (
        <Card noPadding>
          {orders.length === 0 ? (
            <div className="py-12">
              <EmptyState
                isSearch={false}
                actionText="+ إنشاء أول أمر شراء"
                onAction={() => setIsOrderModalOpen(true)}
              />
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-right text-xs">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                  <tr>
                    <th className="py-3 px-4">رقم الأمر</th>
                    <th className="py-3 px-3">التاريخ</th>
                    <th className="py-3 px-3">المورد</th>
                    <th className="py-3 px-3 text-center">القيمة التقديرية</th>
                    <th className="py-3 px-4 text-center">الحالة</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {orders.map((ord) => (
                    <tr key={ord.id} className="hover:bg-slate-50/70 transition-colors">
                      <td className="py-3 px-4 font-mono font-bold text-slate-800">{ord.orderNumber}</td>
                      <td className="py-3 px-3 text-slate-500">
                        {new Date(ord.orderDate).toLocaleDateString('ar-LY')}
                      </td>
                      <td className="py-3 px-3 font-semibold text-slate-800">{ord.supplierName || 'طلب توريد عام'}</td>
                      <td className="py-3 px-3 text-center font-bold text-slate-900 num-ltr">
                        {ord.totalAmount.toFixed(2)} د.ل
                      </td>
                      <td className="py-3 px-4 text-center">
                        <Badge variant="info" size="sm">
                          {ord.status}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Card>
      )}

      {/* تبويب مرتجعات المشتريات */}
      {activeTab === 'returns' && (
        <Card noPadding>
          <div className="overflow-x-auto">
            <table className="w-full text-right text-xs">
              <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                <tr>
                  <th className="py-3 px-4">رقم المرتجع</th>
                  <th className="py-3 px-3">التاريخ</th>
                  <th className="py-3 px-3">المورد</th>
                  <th className="py-3 px-3 text-center">قيمة المرتجع</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {returns.map((ret) => (
                  <tr key={ret.id} className="hover:bg-slate-50/70 transition-colors">
                    <td className="py-3 px-4 font-mono font-bold text-slate-800">{ret.returnNumber}</td>
                    <td className="py-3 px-3 text-slate-500">
                      {new Date(ret.returnDate).toLocaleDateString('ar-LY')}
                    </td>
                    <td className="py-3 px-3 font-semibold text-slate-800">{ret.supplierName}</td>
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

      {/* نافذة إنشاء فاتورة مشتريات */}
      <PurchaseInvoiceModal
        isOpen={isInvoiceModalOpen}
        onClose={() => setIsInvoiceModalOpen(false)}
        onSave={async (data) => {
          try {
            await purchaseService.createInvoice(data);
            showSuccess('تم تسجيل الفاتورة', 'تم حفظ فاتورة المشتريات وتحديث المخزون بنجاح');
            loadPurchaseData();
            setIsInvoiceModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل حفظ فاتورة المشتريات';
            showError('خطأ أثناء حفظ الفاتورة', msg);
          }
        }}
      />

      {/* نافذة إنشاء أمر شراء */}
      <PurchaseOrderModal
        isOpen={isOrderModalOpen}
        onClose={() => setIsOrderModalOpen(false)}
        onSave={async (data) => {
          try {
            await purchaseService.createOrder(data);
            showSuccess('تم إنشاء أمر الشراء', 'تم حفظ أمر الشراء بنجاح');
            loadPurchaseData();
            setIsOrderModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل إنشاء أمر الشراء';
            showError('خطأ أثناء إنشاء أمر الشراء', msg);
          }
        }}
      />

      {/* نافذة تفاصيل فاتورة مشتريات */}
      <PurchaseInvoiceDetailsModal
        isOpen={isDetailsModalOpen}
        onClose={() => {
          setIsDetailsModalOpen(false);
          setSelectedInvoiceId(null);
        }}
        invoiceId={selectedInvoiceId}
      />

      {/* نافذة إنشاء مرتجع مشتريات */}
      <PurchaseReturnModal
        isOpen={isReturnModalOpen}
        onClose={() => setIsReturnModalOpen(false)}
        onSave={async (data) => {
          try {
            await purchaseService.createReturn(data);
            showSuccess('تم تسجيل المرتجع', 'تم حفظ مرتجع المشتريات وتحديث الأرصدة بنجاح');
            loadPurchaseData();
            setIsReturnModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل حفظ مرتجع المشتريات';
            showError('خطأ أثناء حفظ المرتجع', msg);
          }
        }}
      />
    </div>
  );
};

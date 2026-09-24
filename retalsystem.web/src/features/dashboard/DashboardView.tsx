import React, { useCallback, useEffect, useState } from 'react';
import { dashboardService } from './services/dashboardService';
import type { DashboardSummary } from './types/dashboard.types';
import { Card } from '../../shared/components/ui/Card';
import { Badge } from '../../shared/components/ui/Badge';
import { Button } from '../../shared/components/ui/Button';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { SalesTrendChart } from '../../shared/components/charts/SalesTrendChart';
import { TopProductsDonut } from '../../shared/components/charts/TopProductsDonut';
import {
  IconCashRegister,
  IconReceipt,
  IconPackage,
  IconWallet,
  IconTrendingUp,
  IconShoppingCart,
  IconUsers,
  IconAlertTriangle,
  IconArrowUpRight,
  IconRefresh,
} from '@tabler/icons-react';
import { useAuth } from '../../shared/context/AuthContext';
import { useToast } from '../../shared/context/ToastContext';

interface DashboardViewProps {
  onNavigate: (view: string) => void;
}

export const DashboardView: React.FC<DashboardViewProps> = ({ onNavigate }) => {
  const { currentBranchId } = useAuth();
  const { showSuccess, showError } = useToast();
  const [data, setData] = useState<DashboardSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [dashboardError, setDashboardError] = useState<string | null>(null);

  const fetchDashboard = useCallback(async () => {
    setIsLoading(true);
    setDashboardError(null);
    try {
      const result = await dashboardService.getSummary(currentBranchId || undefined);
      setData(result);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل بيانات ومؤشرات لوحة التحكم من الخادم';
      setDashboardError(msg);
      showError('خطأ في لوحة التحكم', msg);
    } finally {
      setIsLoading(false);
    }
  }, [currentBranchId, showError]);

  useEffect(() => {
    fetchDashboard();
  }, [fetchDashboard]);

  const kpis = data?.kpis;

  return (
    <div className="flex flex-col gap-6 max-w-7xl mx-auto">
      {dashboardError && (
        <div className="p-4 bg-rose-50 border border-rose-200 rounded-xl flex items-center justify-between text-rose-800 text-xs">
          <div className="flex items-center gap-2">
            <IconAlertTriangle className="w-5 h-5 text-rose-600 shrink-0" />
            <div>
              <p className="font-bold">تعذر جلب مؤشرات لوحة التحكم الحية من الخادم:</p>
              <p className="text-rose-700 mt-0.5">{dashboardError}</p>
            </div>
          </div>
          <Button variant="outline" size="sm" onClick={fetchDashboard} className="border-rose-300 text-rose-800 hover:bg-rose-100">
            إعادة المحاولة
          </Button>
        </div>
      )}

      {/* ترويسة لوحة التحكم والإجراءات السريعة */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h1 className="text-xl sm:text-2xl font-extrabold text-slate-900 tracking-tight">
            لوحة العمليات والتحليلات الحية
          </h1>
          <p className="text-xs text-slate-500 mt-1">
            متابعة لحظية لحركة المبيعات والمشتريات والمخزون والسيولة النقدية
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2.5">
          <Button
            variant="primary"
            size="md"
            leftIcon={<IconCashRegister className="w-4 h-4" />}
            onClick={() => onNavigate('pos')}
          >
            نقطة البيع (F1)
          </Button>

          <Button
            variant="secondary"
            size="md"
            leftIcon={<IconReceipt className="w-4 h-4" />}
            onClick={() => onNavigate('sales')}
          >
            فاتورة بيع
          </Button>

          <Button
            variant="secondary"
            size="md"
            leftIcon={<IconPackage className="w-4 h-4" />}
            onClick={() => onNavigate('catalog')}
          >
            إضافة صنف
          </Button>

          <Button
            variant="ghost"
            size="md"
            onClick={() => {
              fetchDashboard();
              showSuccess('تم التحديث', 'تم تحديث مؤشرات لوحة التحكم بنجاح');
            }}
            title="تحديث البيانات"
          >
            <IconRefresh className="w-4 h-4" />
          </Button>
        </div>
      </div>

      <LoadingOverlay isLoading={isLoading}>
        {/* شبكة بطاقات مؤشرات الأداء (KPIs) */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* مبيعات اليوم */}
          <Card className="hover:border-emerald-300 transition-colors">
            <div className="flex items-start justify-between">
              <div>
                <span className="text-xs font-semibold text-slate-500">مبيعات اليوم</span>
                <h3 className="text-2xl font-extrabold text-slate-900 mt-1 num-ltr">
                  {(kpis?.todaySales || 0).toLocaleString('en-US', { minimumFractionDigits: 2 })}{' '}
                  <span className="text-xs font-semibold text-emerald-700">د.ل</span>
                </h3>
              </div>
              <div className="w-10 h-10 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
                <IconTrendingUp className="w-5 h-5" />
              </div>
            </div>
            <div className="flex items-center gap-1.5 mt-3 text-xs text-slate-500">
              <span className="inline-flex items-center text-emerald-600 font-bold">
                <IconArrowUpRight className="w-3.5 h-3.5" />
                +{kpis?.salesGrowthPercentage || 0}%
              </span>
              <span>مقارنة بالأمس ({kpis?.yesterdaySales || 0} د.ل)</span>
            </div>
          </Card>

          {/* الأرباح التقديرية اليوم */}
          <Card className="hover:border-emerald-300 transition-colors">
            <div className="flex items-start justify-between">
              <div>
                <span className="text-xs font-semibold text-slate-500">صافي الأرباح اليومية</span>
                <h3 className="text-2xl font-extrabold text-emerald-700 mt-1 num-ltr">
                  {(kpis?.todayProfit || 0).toLocaleString('en-US', { minimumFractionDigits: 2 })}{' '}
                  <span className="text-xs font-semibold text-slate-700">د.ل</span>
                </h3>
              </div>
              <div className="w-10 h-10 rounded-xl bg-emerald-50 text-emerald-700 flex items-center justify-center">
                <IconWallet className="w-5 h-5" />
              </div>
            </div>
            <div className="flex items-center gap-1 mt-3 text-xs text-slate-500">
              <span>عدد الفواتير الصادرة اليوم:</span>
              <span className="font-bold text-slate-800">{kpis?.todayOrdersCount || 0}</span>
            </div>
          </Card>

          {/* مشتريات اليوم */}
          <Card className="hover:border-slate-300 transition-colors">
            <div className="flex items-start justify-between">
              <div>
                <span className="text-xs font-semibold text-slate-500">مشتريات اليوم</span>
                <h3 className="text-2xl font-extrabold text-slate-900 mt-1 num-ltr">
                  {(kpis?.todayPurchases || 0).toLocaleString('en-US', { minimumFractionDigits: 2 })}{' '}
                  <span className="text-xs font-semibold text-slate-700">د.ل</span>
                </h3>
              </div>
              <div className="w-10 h-10 rounded-xl bg-sky-50 text-sky-600 flex items-center justify-center">
                <IconShoppingCart className="w-5 h-5" />
              </div>
            </div>
            <div className="flex items-center gap-1 mt-3 text-xs text-slate-500">
              <span>متوسط قيمة الفاتورة:</span>
              <span className="font-bold text-slate-800 num-ltr">{kpis?.averageOrderValue || 0} د.ل</span>
            </div>
          </Card>

          {/* الذمم المدينة */}
          <Card className="hover:border-slate-300 transition-colors">
            <div className="flex items-start justify-between">
              <div>
                <span className="text-xs font-semibold text-slate-500">ديون العملاء (الذمم المدينة)</span>
                <h3 className="text-2xl font-extrabold text-amber-700 mt-1 num-ltr">
                  {(kpis?.totalReceivables || 0).toLocaleString('en-US', { minimumFractionDigits: 2 })}{' '}
                  <span className="text-xs font-semibold text-slate-700">د.ل</span>
                </h3>
              </div>
              <div className="w-10 h-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center">
                <IconUsers className="w-5 h-5" />
              </div>
            </div>
            <div className="flex items-center gap-1 mt-3 text-xs text-slate-500">
              <span>مستحقات الموردين:</span>
              <span className="font-bold text-slate-800 num-ltr">{kpis?.totalPayables || 0} د.ل</span>
            </div>
          </Card>
        </div>

        {/* القسم الأوسط: الرسوم البيانية */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mt-6">
          {/* منحنى المبيعات على مدار الأسبوع */}
          <Card
            title="مسار المبيعات خلال الأيام الماضية"
            subtitle="حركة المبيعات مقارنة بصافي الأرباح التقديرية"
            className="lg:col-span-2"
          >
            <SalesTrendChart data={data?.dailySalesTrend || []} />
          </Card>

          {/* الأصناف الأكثر مبيعاً */}
          <Card
            title="الأصناف الأكثر مبيعاً"
            subtitle="الأصناف الأعلى توليداً للإيرادات"
            action={
              <button
                type="button"
                onClick={() => onNavigate('catalog')}
                className="text-xs font-semibold text-emerald-600 hover:text-emerald-700"
              >
                عرض الكتالوج
              </button>
            }
          >
            <TopProductsDonut products={data?.topSellingProducts || []} />
          </Card>
        </div>

        {/* القسم السفلي: رادار النواقص والفواتير الأخيرة */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mt-6">
          {/* رادار النواقص والمخزون الحرج */}
          <Card
            title={
              <div className="flex items-center gap-2">
                <IconAlertTriangle className="w-4 h-4 text-amber-500" />
                <span>رادار النواقص والمخزون الحرج</span>
              </div>
            }
            subtitle="أصناف وصلت أو قاربت على حد إعادة الطلب"
            action={
              <Button
                variant="outline"
                size="sm"
                onClick={() => onNavigate('purchases')}
              >
                + أمر شراء
              </Button>
            }
            noPadding
          >
            <div className="overflow-x-auto">
              <table className="w-full text-right text-xs">
                <thead className="bg-slate-50/70 border-b border-slate-100 text-slate-500 font-semibold">
                  <tr>
                    <th className="py-2.5 px-4">الصنف</th>
                    <th className="py-2.5 px-3">المستودع</th>
                    <th className="py-2.5 px-3">الرصيد</th>
                    <th className="py-2.5 px-3">حد الطلب</th>
                    <th className="py-2.5 px-4 text-center">الحالة</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {data?.lowStockAlerts.map((item, i) => (
                    <tr key={i} className="hover:bg-slate-50/50 transition-colors">
                      <td className="py-2.5 px-4 font-bold text-slate-800">{item.productName}</td>
                      <td className="py-2.5 px-3 text-slate-500">{item.warehouseName}</td>
                      <td className="py-2.5 px-3 font-semibold num-ltr text-rose-600">{item.currentQuantity}</td>
                      <td className="py-2.5 px-3 num-ltr text-slate-500">{item.minStockLevel}</td>
                      <td className="py-2.5 px-4 text-center">
                        <Badge
                          variant={item.status === 'نفد' ? 'danger' : 'warning'}
                          size="sm"
                        >
                          {item.status}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>

          {/* أحدث الفواتير */}
          <Card
            title="أحدث فواتير المبيعات"
            subtitle="آخر العمليات الصادرة من نقاط البيع"
            action={
              <button
                type="button"
                onClick={() => onNavigate('sales')}
                className="text-xs font-semibold text-emerald-600 hover:text-emerald-700"
              >
                كافة الفواتير
              </button>
            }
            noPadding
          >
            <div className="overflow-x-auto">
              <table className="w-full text-right text-xs">
                <thead className="bg-slate-50/70 border-b border-slate-100 text-slate-500 font-semibold">
                  <tr>
                    <th className="py-2.5 px-4">رقم الفاتورة</th>
                    <th className="py-2.5 px-3">العميل</th>
                    <th className="py-2.5 px-3">الإجمالي</th>
                    <th className="py-2.5 px-3">طريقة الدفع</th>
                    <th className="py-2.5 px-4 text-center">الحالة</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {data?.recentInvoices.map((inv) => (
                    <tr key={inv.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="py-2.5 px-4 font-mono font-semibold text-slate-700">{inv.invoiceNumber}</td>
                      <td className="py-2.5 px-3 text-slate-600">{inv.customerName}</td>
                      <td className="py-2.5 px-3 font-bold text-slate-900 num-ltr">{inv.totalAmount.toFixed(2)} د.ل</td>
                      <td className="py-2.5 px-3 text-slate-500">{inv.paymentMethod}</td>
                      <td className="py-2.5 px-4 text-center">
                        <Badge variant="success" size="sm">
                          {inv.status}
                        </Badge>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Card>
        </div>
      </LoadingOverlay>
    </div>
  );
};

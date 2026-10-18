import React, { useState, useEffect, useCallback } from 'react';
import { stockService } from './services/stockService';
import type {
  StockLevelDto,
  WarehouseDto,
  StockAdjustmentDto,
  StockTransferDto,
} from './types/stock.types';
import { StockAdjustmentModal } from './components/StockAdjustmentModal';
import { StockTransferModal } from './components/StockTransferModal';
import { WarehouseModal } from './components/WarehouseModal';
import { Card } from '../../shared/components/ui/Card';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Badge } from '../../shared/components/ui/Badge';
import { Tabs } from '../../shared/components/ui/Tabs';
import { Pagination } from '../../shared/components/ui/Pagination';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { usePagination } from '../../shared/hooks/usePagination';
import { useToast } from '../../shared/context/ToastContext';
import { useAuth } from '../../shared/context/AuthContext';
import { Permissions } from '../../shared/auth/permissions';
import {
  IconBuildingWarehouse,
  IconArrowsExchange,
  IconAdjustments,
  IconSearch,
  IconPlus,
} from '@tabler/icons-react';

export const StockView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const { hasPermission } = useAuth();
  const canViewAdjustments = hasPermission(Permissions.StockAdjustments.View);
  const canCreateAdjustment = hasPermission(Permissions.StockAdjustments.Create);
  const canViewTransfers = hasPermission(Permissions.StockTransfers.View);
  const canCreateTransfer = hasPermission(Permissions.StockTransfers.Create);
  const canViewWarehouses = hasPermission(Permissions.Warehouses.View);
  const canCreateWarehouse = hasPermission(Permissions.Warehouses.Create);
  const [activeTab, setActiveTab] = useState<'levels' | 'adjustments' | 'transfers' | 'warehouses'>('levels');

  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);
  const [selectedWarehouseId, setSelectedWarehouseId] = useState<string>('');
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);

  // خطافات الترقيم الصفحي لكل تبويب
  const stockPagination = usePagination({ initialPageSize: 10 });
  const adjPagination = usePagination({ initialPageSize: 10 });
  const trfPagination = usePagination({ initialPageSize: 10 });
  const {
    page: stockPage,
    pageSize: stockPageSize,
    resetPage: resetStockPage,
    setTotalCount: setStockTotalCount,
  } = stockPagination;
  const {
    page: adjustmentPage,
    pageSize: adjustmentPageSize,
    resetPage: resetAdjustmentPage,
    setTotalCount: setAdjustmentTotalCount,
  } = adjPagination;
  const {
    page: transferPage,
    pageSize: transferPageSize,
    resetPage: resetTransferPage,
    setTotalCount: setTransferTotalCount,
  } = trfPagination;

  const [stockLevels, setStockLevels] = useState<StockLevelDto[]>([]);
  const [adjustments, setAdjustments] = useState<StockAdjustmentDto[]>([]);
  const [transfers, setTransfers] = useState<StockTransferDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // النوافذ المنبثقة
  const [isAdjModalOpen, setIsAdjModalOpen] = useState(false);
  const [isTrfModalOpen, setIsTrfModalOpen] = useState(false);
  const [isWhModalOpen, setIsWhModalOpen] = useState(false);

  const [stockError, setStockError] = useState<string | null>(null);

  const loadWarehouses = useCallback(async () => {
    try {
      const whs = await stockService.getWarehouses();
      setWarehouses(whs);
      if (whs.length > 0) {
        setSelectedWarehouseId((prev) => (!prev || prev === 'all' ? whs[0].id : prev));
      }
    } catch (err: unknown) {
      console.error('Failed to load warehouses:', err);
    }
  }, []);

  // إعادة الترقيم للصفحة الأولى عند البحث أو تغيير المستودع
  useEffect(() => {
    resetStockPage();
  }, [debouncedSearch, selectedWarehouseId, resetStockPage]);

  useEffect(() => {
    resetAdjustmentPage();
    resetTransferPage();
  }, [debouncedSearch, resetAdjustmentPage, resetTransferPage]);

  const loadStockData = useCallback(async () => {
    setIsLoading(true);
    setStockError(null);
    try {
      if (activeTab === 'levels') {
        if (!selectedWarehouseId || selectedWarehouseId === 'all') {
          if (warehouses.length > 0) {
            setSelectedWarehouseId(warehouses[0].id);
          }
          setIsLoading(false);
          return;
        }
        const currentWh = warehouses.find((w) => w.id === selectedWarehouseId);
        const res = await stockService.getStockLevels(
          selectedWarehouseId,
          {
            pageNumber: stockPage,
            pageSize: stockPageSize,
            search: debouncedSearch,
          },
          currentWh?.type
        );
        setStockLevels(res.items);
        setStockTotalCount(res.totalCount);
      } else if (activeTab === 'adjustments') {
        const res = await stockService.getAdjustments({
          pageNumber: adjustmentPage,
          pageSize: adjustmentPageSize,
          warehouseId: selectedWarehouseId && selectedWarehouseId !== 'all' ? selectedWarehouseId : undefined,
          search: debouncedSearch,
        });
        setAdjustments(res.items);
        setAdjustmentTotalCount(res.totalCount);
      } else if (activeTab === 'transfers') {
        const res = await stockService.getTransfers({
          pageNumber: transferPage,
          pageSize: transferPageSize,
          fromWarehouseId: selectedWarehouseId && selectedWarehouseId !== 'all' ? selectedWarehouseId : undefined,
          search: debouncedSearch,
        });
        setTransfers(res.items);
        setTransferTotalCount(res.totalCount);
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل بيانات المخزون';
      setStockError(msg);
      showError('خطأ في تحميل المخزون', msg);
    } finally {
      setIsLoading(false);
    }
  }, [
    activeTab,
    selectedWarehouseId,
    debouncedSearch,
    warehouses,
    stockPage,
    stockPageSize,
    setStockTotalCount,
    adjustmentPage,
    adjustmentPageSize,
    setAdjustmentTotalCount,
    transferPage,
    transferPageSize,
    setTransferTotalCount,
    showError,
  ]);

  useEffect(() => {
    loadWarehouses();
  }, [loadWarehouses]);

  useEffect(() => {
    loadStockData();
  }, [loadStockData]);

  const tabs = [
    { id: 'levels', label: 'أرصدة المخزون والجرد', icon: <IconBuildingWarehouse className="w-4 h-4" />, badge: stockPagination.totalCount },
    ...(canViewAdjustments
      ? [{ id: 'adjustments', label: 'التسويات الجردية', icon: <IconAdjustments className="w-4 h-4" />, badge: adjPagination.totalCount }]
      : []),
    ...(canViewTransfers
      ? [{ id: 'transfers', label: 'التحويلات بين المخازن', icon: <IconArrowsExchange className="w-4 h-4" />, badge: trfPagination.totalCount }]
      : []),
    ...(canViewWarehouses
      ? [{ id: 'warehouses', label: 'دليل المستودعات', icon: <IconBuildingWarehouse className="w-4 h-4" />, badge: warehouses.length }]
      : []),
  ];

  return (
    <div className="flex flex-col gap-5 max-w-7xl mx-auto">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-3">
        <div>
          <h1 className="text-xl sm:text-2xl font-extrabold text-slate-900 tracking-tight">
            إدارة المخازن والجرد
          </h1>
          <p className="text-xs text-slate-500 mt-1">
            متابعة أرصدة الأصناف في المستودعات بالترقيم الصفحي، التسويات الدورية، وتحويلات البضائع
          </p>
        </div>

        {/* أزرار الإجراءات */}
        <div className="flex items-center gap-2">
          {activeTab === 'adjustments' && canCreateAdjustment && (
            <Button
              variant="primary"
              size="md"
              leftIcon={<IconPlus className="w-4 h-4" />}
              onClick={() => setIsAdjModalOpen(true)}
            >
              + تسوية جردية
            </Button>
          )}

          {activeTab === 'transfers' && canCreateTransfer && (
            <Button
              variant="primary"
              size="md"
              leftIcon={<IconPlus className="w-4 h-4" />}
              onClick={() => setIsTrfModalOpen(true)}
            >
              + أمر تحويل مخزني
            </Button>
          )}

          {activeTab === 'warehouses' && canCreateWarehouse && (
            <Button
              variant="primary"
              size="md"
              leftIcon={<IconPlus className="w-4 h-4" />}
              onClick={() => setIsWhModalOpen(true)}
            >
              + إضافة مستودع
            </Button>
          )}
        </div>
      </div>

      <Tabs tabs={tabs} activeTab={activeTab} onChange={(t) => setActiveTab(t as typeof activeTab)} />

      {stockError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل بيانات المخزون من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{stockError}</p>
          </div>
          <Button variant="secondary" size="sm" onClick={() => loadStockData()}>
            إعادة المحاولة
          </Button>
        </div>
      )}

      {/* تبويب أرصدة المخزون والجرد */}
      {activeTab === 'levels' && (
        <Card noPadding>
          <div className="p-4 border-b border-slate-100 flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/40">
            <div className="w-full sm:w-72">
              <Input
                placeholder="ابحث بالصنف أو الكود..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                leftIcon={<IconSearch className="w-4 h-4" />}
              />
            </div>

            <div className="flex items-center gap-2 w-full sm:w-auto">
              <span className="text-xs text-slate-500 whitespace-nowrap">المستودع المحدد:</span>
              <select
                value={selectedWarehouseId}
                onChange={(e) => {
                  setSelectedWarehouseId(e.target.value);
                  stockPagination.resetPage();
                }}
                className="h-10 px-3 text-xs rounded-lg border border-slate-200 bg-white text-slate-700 focus:ring-2 focus:ring-emerald-500 focus:outline-none shrink-0"
              >
                {warehouses.length === 0 ? (
                  <option value="">لا توجد مستودعات مسجلة</option>
                ) : (
                  warehouses.map((w) => (
                    <option key={w.id} value={w.id}>
                      {w.name} ({w.type === 1 ? 'مخزن رئيسي' : 'صالة عرض'})
                    </option>
                  ))
                )}
              </select>
            </div>
          </div>

          <LoadingOverlay isLoading={isLoading}>
            {stockLevels.length === 0 ? (
              <div className="py-12">
                <EmptyState isSearch={Boolean(search)} />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs min-w-[650px]">
                  <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                    <tr>
                      <th className="py-3 px-4">الصنف</th>
                      <th className="py-3 px-3">الكود / الباركود</th>
                      <th className="py-3 px-3">المستودع</th>
                      <th className="py-3 px-3 text-center">الرصيد الفعلي</th>
                      <th className="py-3 px-3 text-center">حد الطلب</th>
                      <th className="py-3 px-3 text-center">سعر البيع</th>
                      <th className="py-3 px-4 text-center">حالة التوفر</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {stockLevels.map((item, idx) => (
                      <tr key={idx} className="hover:bg-slate-50/60 transition-colors">
                        <td className="py-3 px-4 font-bold text-slate-800">{item.productName}</td>
                        <td className="py-3 px-3 text-slate-500 font-mono">{item.code || '-'}</td>
                        <td className="py-3 px-3 text-slate-600">{item.warehouseName}</td>
                        <td className="py-3 px-3 text-center font-extrabold text-sm num-ltr text-slate-900">
                          {item.currentQuantity}
                        </td>
                        <td className="py-3 px-3 text-center num-ltr text-slate-400">{item.minStockLevel}</td>
                        <td className="py-3 px-3 text-center font-bold text-emerald-700 num-ltr">
                          {item.salePrice > 0 ? `${item.salePrice.toFixed(2)} د.ل` : '-'}
                        </td>
                        <td className="py-3 px-4 text-center">
                          <Badge
                            variant={
                              item.status === 'نفد'
                                ? 'danger'
                                : item.status === 'منخفض'
                                ? 'warning'
                                : 'success'
                            }
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
            )}
          </LoadingOverlay>

          {stockLevels.length > 0 && (
            <Pagination
              currentPage={stockPagination.page}
              totalPages={stockPagination.totalPages}
              totalCount={stockPagination.totalCount}
              pageSize={stockPagination.pageSize}
              onPageChange={stockPagination.goToPage}
              onPageSizeChange={stockPagination.changePageSize}
              isLoading={isLoading}
            />
          )}
        </Card>
      )}

      {/* تبويب التسويات الجردية */}
      {activeTab === 'adjustments' && (
        <Card noPadding>
          <div className="p-4 border-b border-slate-100 flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/40">
            <div className="w-full sm:w-72">
              <Input
                placeholder="ابحث برقم التسوية..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                leftIcon={<IconSearch className="w-4 h-4" />}
              />
            </div>
          </div>

          <LoadingOverlay isLoading={isLoading}>
            {adjustments.length === 0 ? (
              <div className="py-12">
                <EmptyState isSearch={Boolean(search)} />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs min-w-[550px]">
                  <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                    <tr>
                      <th className="py-3 px-4">رقم التسوية</th>
                      <th className="py-3 px-3">التاريخ</th>
                      <th className="py-3 px-3">المستودع</th>
                      <th className="py-3 px-3">السبب والمبرر</th>
                      <th className="py-3 px-3 text-center">عدد البنود</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {adjustments.map((adj) => (
                      <tr key={adj.id} className="hover:bg-slate-50/60 transition-colors">
                        <td className="py-3 px-4 font-mono font-bold text-slate-800">{adj.adjustmentNumber}</td>
                        <td className="py-3 px-3 text-slate-500">
                          {new Date(adj.adjustmentDate).toLocaleDateString('ar-LY')}
                        </td>
                        <td className="py-3 px-3 text-slate-700">{adj.warehouseName || '-'}</td>
                        <td className="py-3 px-3 text-slate-600">{adj.reasonName || adj.reason || '-'}</td>
                        <td className="py-3 px-3 text-center font-bold num-ltr text-slate-800">
                          {adj.itemCount ?? 0}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </LoadingOverlay>

          {adjustments.length > 0 && (
            <Pagination
              currentPage={adjPagination.page}
              totalPages={adjPagination.totalPages}
              totalCount={adjPagination.totalCount}
              pageSize={adjPagination.pageSize}
              onPageChange={adjPagination.goToPage}
              onPageSizeChange={adjPagination.changePageSize}
              isLoading={isLoading}
            />
          )}
        </Card>
      )}

      {/* تبويب التحويلات المخزنية */}
      {activeTab === 'transfers' && (
        <Card noPadding>
          <div className="p-4 border-b border-slate-100 flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/40">
            <div className="w-full sm:w-72">
              <Input
                placeholder="ابحث برقم التحويل..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                leftIcon={<IconSearch className="w-4 h-4" />}
              />
            </div>
          </div>

          <LoadingOverlay isLoading={isLoading}>
            {transfers.length === 0 ? (
              <div className="py-12">
                <EmptyState isSearch={Boolean(search)} />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs min-w-[600px]">
                  <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                    <tr>
                      <th className="py-3 px-4">رقم التحويل</th>
                      <th className="py-3 px-3">التاريخ</th>
                      <th className="py-3 px-3">من المستودع</th>
                      <th className="py-3 px-3">إلى المستودع</th>
                      <th className="py-3 px-3 text-center">عدد البنود</th>
                      <th className="py-3 px-4 text-center">الحالة</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {transfers.map((trf) => (
                      <tr key={trf.id} className="hover:bg-slate-50/60 transition-colors">
                        <td className="py-3 px-4 font-mono font-bold text-slate-800">{trf.transferNumber}</td>
                        <td className="py-3 px-3 text-slate-500">
                          {new Date(trf.transferDate).toLocaleDateString('ar-LY')}
                        </td>
                        <td className="py-3 px-3 text-slate-700">{trf.fromWarehouseName || trf.sourceWarehouseName || '-'}</td>
                        <td className="py-3 px-3 text-slate-700">{trf.toWarehouseName || trf.destinationWarehouseName || '-'}</td>
                        <td className="py-3 px-3 text-center font-bold num-ltr text-slate-800">
                          {trf.itemCount ?? 0}
                        </td>
                        <td className="py-3 px-4 text-center">
                          <Badge variant="success" size="sm">
                            {trf.statusName || 'مكتمل ومستلم'}
                          </Badge>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </LoadingOverlay>

          {transfers.length > 0 && (
            <Pagination
              currentPage={trfPagination.page}
              totalPages={trfPagination.totalPages}
              totalCount={trfPagination.totalCount}
              pageSize={trfPagination.pageSize}
              onPageChange={trfPagination.goToPage}
              onPageSizeChange={trfPagination.changePageSize}
              isLoading={isLoading}
            />
          )}
        </Card>
      )}

      {/* تبويب دليل المستودعات */}
      {activeTab === 'warehouses' && (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
          {warehouses.map((wh) => (
            <Card key={wh.id} className="hover:border-emerald-300 transition-colors">
              <div className="flex items-start justify-between">
                <h4 className="text-sm font-bold text-slate-900">{wh.name}</h4>
                <div className="w-10 h-10 rounded-xl bg-emerald-50 text-emerald-700 flex items-center justify-center">
                  <IconBuildingWarehouse className="w-5 h-5" />
                </div>
              </div>

              <div className="mt-3 pt-3 border-t border-slate-100 flex flex-col gap-1 text-xs text-slate-600">
                <div>
                  النوع: <span className="font-semibold text-slate-800">{wh.type === 1 ? 'مخزن رئيسي' : 'صالة عرض'}</span>
                </div>
                <div>
                  أمين المستودع: <span className="font-semibold text-slate-800">{wh.storekeeperName || '-'}</span>
                </div>
                <div>
                  الهاتف: <span className="num-ltr text-slate-700">{wh.phone || '-'}</span>
                </div>
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* النوافذ المنبثقة */}
      <StockAdjustmentModal
        isOpen={isAdjModalOpen}
        onClose={() => setIsAdjModalOpen(false)}
        warehouses={warehouses}
        onSave={async (data) => {
          try {
            await stockService.createAdjustment(data);
            showSuccess('تمت التسوية', 'تم اعتماد التسوية الجردية بنجاح');
            loadStockData();
            setIsAdjModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل حفظ التسوية الجردية';
            showError('خطأ أثناء حفظ التسوية', msg);
            throw err;
          }
        }}
      />

      <StockTransferModal
        isOpen={isTrfModalOpen}
        onClose={() => setIsTrfModalOpen(false)}
        warehouses={warehouses}
        onSave={async (data) => {
          try {
            await stockService.createTransfer(data);
            showSuccess('تم التحويل', 'تم إصدار أمر التحويل المخزني بنجاح');
            loadStockData();
            setIsTrfModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل تنفيذ أمر التحويل';
            showError('خطأ أثناء التحويل', msg);
          }
        }}
      />

      <WarehouseModal
        isOpen={isWhModalOpen}
        onClose={() => setIsWhModalOpen(false)}
        onSave={async (data) => {
          try {
            await stockService.createWarehouse(data);
            showSuccess('تمت الإضافة', 'تم تسجيل المستودع الجديد بنجاح');
            loadWarehouses();
            setIsWhModalOpen(false);
          } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : 'فشل إضافة المستودع';
            showError('خطأ أثناء إضافة المستودع', msg);
          }
        }}
      />
    </div>
  );
};

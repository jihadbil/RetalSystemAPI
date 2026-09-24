import React, { useState, useEffect, useCallback } from 'react';
import { catalogService } from './services/catalogService';
import type { ProductDto, CategoryDto, UnitDto, CreateProductRequest } from './types/catalog.types';
import { ProductFormModal } from './components/ProductFormModal';
import { ProductImportModal } from './components/ProductImportModal';
import { CategoriesPanel } from './components/CategoriesPanel';
import { UnitsPanel } from './components/UnitsPanel';
import { Card } from '../../shared/components/ui/Card';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Pagination } from '../../shared/components/ui/Pagination';
import { LoadingOverlay, TableSkeleton } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { ConfirmDialog } from '../../shared/components/ui/ConfirmDialog';
import { Tabs } from '../../shared/components/ui/Tabs';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { usePagination } from '../../shared/hooks/usePagination';
import { useToast } from '../../shared/context/ToastContext';
import {
  IconPlus,
  IconFileSpreadsheet,
  IconSearch,
  IconEdit,
  IconTrash,
  IconPackage,
  IconBarcode,
  IconCategory,
  IconScale,
  IconDownload,
} from '@tabler/icons-react';

export const CatalogView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const [activeMainTab, setActiveMainTab] = useState<'products' | 'categories' | 'units'>('products');

  // بيانات المنتجات
  const [products, setProducts] = useState<ProductDto[]>([]);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [units, setUnits] = useState<UnitDto[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string>('all');
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 300);

  const [isLoading, setIsLoading] = useState(false);
  const pagination = usePagination({ initialPageSize: 10 });
  const { page, pageSize, setTotalCount } = pagination;

  // النوافذ المنبثقة
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState<ProductDto | null>(null);
  const [isImportOpen, setIsImportOpen] = useState(false);
  const [productToDelete, setProductToDelete] = useState<ProductDto | null>(null);

  const [catalogError, setCatalogError] = useState<string | null>(null);

  // جلب التصنيفات والوحدات
  const loadMetadata = useCallback(async () => {
    try {
      const [cats, uns] = await Promise.all([catalogService.getCategories(), catalogService.getUnits()]);
      setCategories(cats);
      setUnits(uns);
    } catch (err: unknown) {
      console.error('Failed to load categories or units:', err);
    }
  }, []);

  useEffect(() => {
    loadMetadata();
  }, [loadMetadata]);

  // جلب قائمة المنتجات
  const loadProducts = useCallback(async () => {
    setIsLoading(true);
    setCatalogError(null);
    try {
      const res = await catalogService.getProducts({
        pageNumber: page,
        pageSize,
        search: debouncedSearch,
        categoryId: selectedCategory === 'all' ? undefined : selectedCategory,
      });
      setProducts(res.items);
      setTotalCount(res.totalCount);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل قائمة المنتجات من الخادم';
      setCatalogError(msg);
      showError('خطأ في تحميل الأصناف', msg);
    } finally {
      setIsLoading(false);
    }
  }, [page, pageSize, setTotalCount, debouncedSearch, selectedCategory, showError]);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  const handleSaveProduct = async (data: CreateProductRequest) => {
    try {
      if (editingProduct) {
        await catalogService.updateProduct(editingProduct.id, data);
        showSuccess('تم التعديل', `تم تحديث بيانات الصنف ${data.name} بنجاح`);
      } else {
        await catalogService.createProduct(data);
        showSuccess('تمت الإضافة', `تمت إضافة الصنف ${data.name} بنجاح`);
      }
      loadProducts();
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل حفظ الصنف';
      showError('خطأ', msg);
    }
  };

  const handleDeleteProduct = async () => {
    if (!productToDelete) return;
    try {
      await catalogService.deleteProduct(productToDelete.id);
      showSuccess('تم الحذف', `تم حذف الصنف ${productToDelete.name}`);
      setProductToDelete(null);
      loadProducts();
    } catch {
      showError('خطأ', 'فشل حذف الصنف');
    }
  };

  const handleExportCsv = () => {
    const headers = ['الكود', 'الاسم', 'التصنيف', 'سعر البيع', 'سعر التكلفة', 'الرصيد', 'الباركود'];
    const rows = products.map((p) => [
      p.code,
      `"${p.name}"`,
      `"${p.categoryName || ''}"`,
      p.salePrice,
      p.costPrice,
      p.currentStock || 0,
      `"${p.barCodes?.[0]?.barCode || ''}"`,
    ]);
    const csvContent = 'data:text/csv;charset=utf-8,\uFEFF' + [headers.join(','), ...rows.map((e) => e.join(','))].join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `products_catalog_${Date.now()}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    showSuccess('تصدير البيانات', 'تم تنزيل ملف الأصناف بنجاح');
  };

  const mainTabs = [
    { id: 'products', label: 'دليل الأصناف والمنتجات', icon: <IconPackage className="w-4 h-4" />, badge: pagination.totalCount },
    { id: 'categories', label: 'التصنيفات', icon: <IconCategory className="w-4 h-4" /> },
    { id: 'units', label: 'وحدات القياس', icon: <IconScale className="w-4 h-4" /> },
  ];

  return (
    <div className="flex flex-col gap-5 max-w-7xl mx-auto">
      {/* الترويسة الرئيسية والتبويبات */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-slate-200 pb-3">
        <div>
          <h1 className="text-xl sm:text-2xl font-extrabold text-slate-900 tracking-tight">
            كتالوج الأصناف والمنتجات
          </h1>
          <p className="text-xs text-slate-500 mt-1">
            إدارة بيانات المنتجات والأسعار والوحدات والباركودات واستيراد وتصدير ملفات Excel
          </p>
        </div>

        {/* أزرار الإجراءات الأساسية */}
        {activeMainTab === 'products' && (
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="md"
              leftIcon={<IconFileSpreadsheet className="w-4 h-4 text-emerald-600" />}
              onClick={() => setIsImportOpen(true)}
              title="استيراد أصناف من ملف إكسل"
            >
              استيراد Excel
            </Button>

            <Button
              variant="primary"
              size="md"
              leftIcon={<IconPlus className="w-4 h-4" />}
              onClick={() => {
                setEditingProduct(null);
                setIsFormOpen(true);
              }}
            >
              + إضافة صنف جديد
            </Button>
          </div>
        )}
      </div>

      <Tabs
        tabs={mainTabs}
        activeTab={activeMainTab}
        onChange={(tab) => setActiveMainTab(tab as 'products' | 'categories' | 'units')}
      />

      {catalogError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل بيانات الأصناف من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{catalogError}</p>
          </div>
          <Button variant="secondary" size="sm" onClick={() => loadProducts()}>
            إعادة المحاولة
          </Button>
        </div>
      )}

      {/* تبويب الأصناف */}
      {activeMainTab === 'products' && (
        <Card noPadding>
          {/* شريط أدوات البحث والفلترة */}
          <div className="p-4 border-b border-slate-100 flex flex-col sm:flex-row items-center justify-between gap-3 bg-slate-50/40">
            <div className="flex flex-1 items-center gap-3 w-full">
              <div className="w-full sm:w-72">
                <Input
                  placeholder="ابحث بالاسم أو الكود أو الباركود..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                  leftIcon={<IconSearch className="w-4 h-4" />}
                />
              </div>

              <select
                value={selectedCategory}
                onChange={(e) => setSelectedCategory(e.target.value)}
                className="h-10 px-3 text-xs rounded-lg border border-slate-200 bg-white text-slate-700 focus:ring-2 focus:ring-emerald-500 focus:outline-none shrink-0"
              >
                <option value="all">كافة التصنيفات</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
                ))}
              </select>
            </div>

            <Button
              variant="secondary"
              size="sm"
              leftIcon={<IconDownload className="w-3.5 h-3.5 text-slate-500" />}
              onClick={handleExportCsv}
              title="تصدير قائمة المنتجات الحالية"
            >
              تصدير CSV
            </Button>
          </div>

          {/* جدول المنتجات */}
          <LoadingOverlay isLoading={isLoading}>
            {isLoading && products.length === 0 ? (
              <div className="p-4">
                <TableSkeleton rows={5} columns={6} />
              </div>
            ) : products.length === 0 ? (
              <div className="py-12">
                <EmptyState
                  isSearch={Boolean(search || selectedCategory !== 'all')}
                  actionText={search ? 'مسح الفلاتر' : '+ إضافة أول صنف'}
                  onAction={() => {
                    if (search || selectedCategory !== 'all') {
                      setSearch('');
                      setSelectedCategory('all');
                    } else {
                      setEditingProduct(null);
                      setIsFormOpen(true);
                    }
                  }}
                />
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs">
                  <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold select-none">
                    <tr>
                      <th className="py-3 px-4 w-14 text-center">صورة</th>
                      <th className="py-3 px-4">الصنف والكود</th>
                      <th className="py-3 px-3">التصنيف</th>
                      <th className="py-3 px-3 text-center">الباركود</th>
                      <th className="py-3 px-3 text-center">سعر البيع</th>
                      <th className="py-3 px-3 text-center">سعر التكلفة</th>
                      <th className="py-3 px-3 text-center">الرصيد المتوفر</th>
                      <th className="py-3 px-4 text-center w-24">إجراءات</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {products.map((item) => (
                      <tr key={item.id} className="hover:bg-slate-50/70 transition-colors">
                        {/* صورة الصنف بنسبة مربعة ثابتة */}
                        <td className="py-3 px-4 text-center">
                          <div className="w-10 h-10 rounded-lg bg-slate-100 border border-slate-200 flex items-center justify-center text-slate-400 overflow-hidden mx-auto aspect-square">
                            {item.primaryImageUrl ? (
                              <img
                                src={item.primaryImageUrl}
                                alt={item.name}
                                className="w-full h-full object-cover"
                              />
                            ) : (
                              <IconPackage className="w-5 h-5 text-slate-400" />
                            )}
                          </div>
                        </td>

                        {/* الاسم والكود */}
                        <td className="py-3 px-4">
                          <div className="font-bold text-slate-900 text-sm">{item.name}</div>
                          <div className="text-[11px] text-slate-400 font-mono mt-0.5">{item.code}</div>
                        </td>

                        {/* التصنيف */}
                        <td className="py-3 px-3 text-slate-600">
                          {item.categoryName || 'بدون تصنيف'}
                        </td>

                        {/* الباركود */}
                        <td className="py-3 px-3 text-center font-mono num-ltr text-slate-600">
                          {item.barCodes?.[0]?.barCode ? (
                            <span className="inline-flex items-center gap-1 bg-slate-50 border border-slate-200/80 px-2 py-0.5 rounded text-[11px]">
                              <IconBarcode className="w-3.5 h-3.5 text-slate-400" />
                              {item.barCodes[0].barCode}
                            </span>
                          ) : (
                            <span className="text-slate-300">-</span>
                          )}
                        </td>

                        {/* سعر البيع */}
                        <td className="py-3 px-3 text-center font-extrabold text-emerald-700 text-sm num-ltr">
                          {item.salePrice.toFixed(2)} د.ل
                        </td>

                        {/* سعر التكلفة */}
                        <td className="py-3 px-3 text-center font-semibold text-slate-500 num-ltr">
                          {item.costPrice.toFixed(2)} د.ل
                        </td>

                        {/* الرصيد المتوفر */}
                        <td className="py-3 px-3 text-center">
                          <span
                            className={`font-bold num-ltr ${
                              (item.currentStock || 0) <= 0
                                ? 'text-rose-600'
                                : (item.currentStock || 0) <= item.minStockLevel
                                ? 'text-amber-600'
                                : 'text-slate-800'
                            }`}
                          >
                            {item.currentStock || 0}
                          </span>
                          {(item.currentStock || 0) <= item.minStockLevel && (
                            <span className="block text-[10px] text-rose-500 font-bold mt-0.5">
                              {(item.currentStock || 0) <= 0 ? 'نفد المخزون' : 'مخزون حرج'}
                            </span>
                          )}
                        </td>

                        {/* أزرار التعديل والحذف */}
                        <td className="py-3 px-4 text-center">
                          <div className="flex items-center justify-center gap-1">
                            <button
                              type="button"
                              onClick={() => {
                                setEditingProduct(item);
                                setIsFormOpen(true);
                              }}
                              className="p-1.5 text-slate-500 hover:text-emerald-700 hover:bg-emerald-50 rounded-lg transition-colors cursor-pointer"
                              title="تعديل الصنف"
                            >
                              <IconEdit className="w-4 h-4" />
                            </button>

                            <button
                              type="button"
                              onClick={() => setProductToDelete(item)}
                              className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors cursor-pointer"
                              title="حذف الصنف"
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
          </LoadingOverlay>

          {/* مكون الترقيم */}
          <Pagination
            currentPage={pagination.page}
            totalPages={pagination.totalPages}
            totalCount={pagination.totalCount}
            pageSize={pagination.pageSize}
            onPageChange={pagination.goToPage}
            onPageSizeChange={pagination.changePageSize}
            isLoading={isLoading}
          />
        </Card>
      )}

      {/* تبويب التصنيفات */}
      {activeMainTab === 'categories' && (
        <CategoriesPanel
          categories={categories}
          onAddCategory={async (cat) => {
            await catalogService.createCategory(cat);
            loadMetadata();
          }}
        />
      )}

      {/* تبويب وحدات القياس */}
      {activeMainTab === 'units' && <UnitsPanel units={units} />}

      {/* نافذة إنشاء / تعديل صنف */}
      <ProductFormModal
        isOpen={isFormOpen}
        onClose={() => {
          setIsFormOpen(false);
          setEditingProduct(null);
        }}
        productToEdit={editingProduct}
        categories={categories}
        units={units}
        onSave={handleSaveProduct}
      />

      {/* معالج استيراد Excel */}
      <ProductImportModal
        isOpen={isImportOpen}
        onClose={() => setIsImportOpen(false)}
        onSuccess={loadProducts}
      />

      {/* نافذة تأكيد حذف صنف */}
      <ConfirmDialog
        isOpen={Boolean(productToDelete)}
        onClose={() => setProductToDelete(null)}
        onConfirm={handleDeleteProduct}
        title="حذف الصنف نهائياً"
        message={`هل أنت متأكد من رغبتك في حذف الصنف "${productToDelete?.name}" من النظام؟ لا يمكن التراجع عن هذه العملية.`}
        confirmText="حذف الصنف"
        variant="danger"
      />
    </div>
  );
};

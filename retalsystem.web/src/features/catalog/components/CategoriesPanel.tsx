import { Select } from '../../../shared/components/ui/Select';
import React, { useState } from 'react';
import type { CategoryDto, CreateCategoryRequest, UpdateCategoryRequest } from '../types/catalog.types';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Modal } from '../../../shared/components/ui/Modal';
import { ConfirmDialog } from '../../../shared/components/ui/ConfirmDialog';
import {
  IconPlus,
  IconEdit,
  IconTrash,
  IconSearch,
  IconCornerDownLeft,
  IconFolder,
  IconHierarchy,
} from '@tabler/icons-react';
import { useToast } from '../../../shared/context/ToastContext';

interface CategoriesPanelProps {
  categories: CategoryDto[];
  onSearchChange?: (search: string) => void;
  onAddCategory: (cat: CreateCategoryRequest) => Promise<void>;
  onUpdateCategory?: (id: string, cat: UpdateCategoryRequest) => Promise<void>;
  onDeleteCategory?: (id: string) => Promise<void>;
  canCreate?: boolean;
  canEdit?: boolean;
  canDelete?: boolean;
}

export const CategoriesPanel: React.FC<CategoriesPanelProps> = ({
  categories,
  onSearchChange,
  onAddCategory,
  onUpdateCategory,
  onDeleteCategory,
  canCreate = false,
  canEdit = false,
  canDelete = false,
}) => {
  const { showSuccess, showError } = useToast();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<CategoryDto | null>(null);
  const [categoryToDelete, setCategoryToDelete] = useState<CategoryDto | null>(null);

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [parentCategoryId, setParentCategoryId] = useState<string>('');
  const [search, setSearch] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const handleOpenAddModal = () => {
    setEditingCategory(null);
    setName('');
    setDescription('');
    setParentCategoryId('');
    setIsModalOpen(true);
  };

  const handleOpenEditModal = (cat: CategoryDto) => {
    setEditingCategory(cat);
    setName(cat.name);
    setDescription(cat.description || '');
    setParentCategoryId(cat.parentCategoryId || '');
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    setIsLoading(true);
    const parentIdToSave = parentCategoryId && parentCategoryId.trim() !== '' ? parentCategoryId.trim() : null;

    try {
      if (editingCategory) {
        if (onUpdateCategory) {
          await onUpdateCategory(editingCategory.id, {
            name: name.trim(),
            description: description.trim() || undefined,
            parentCategoryId: parentIdToSave,
            isActive: editingCategory.isActive,
          });
          showSuccess('تم التحديث', `تم تعديل تصنيف "${name}" بنجاح`);
        }
      } else {
        await onAddCategory({
          name: name.trim(),
          description: description.trim() || undefined,
          parentCategoryId: parentIdToSave,
          isActive: true,
        });
        showSuccess('تمت الإضافة', `تمت إضافة تصنيف "${name}" بنجاح`);
      }

      setName('');
      setDescription('');
      setParentCategoryId('');
      setEditingCategory(null);
      setIsModalOpen(false);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل حفظ بيانات التصنيف';
      showError('خطأ في العملية', msg);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!categoryToDelete || !onDeleteCategory) return;
    setIsDeleting(true);
    try {
      await onDeleteCategory(categoryToDelete.id);
      showSuccess('تم الحذف', `تم حذف تصنيف "${categoryToDelete.name}" بنجاح`);
      setCategoryToDelete(null);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'تعذر حذف التصنيف، قد يحتوي على منتجات أو تصنيفات فرعية مرتبطة';
      showError('فشل الحذف', msg);
    } finally {
      setIsDeleting(false);
    }
  };

  // تصفية التصنيفات بالبحث
  const filteredCategories = categories.filter((cat) => {
    const q = search.trim().toLowerCase();
    if (!q) return true;
    return (
      cat.name.toLowerCase().includes(q) ||
      (cat.parentCategoryName && cat.parentCategoryName.toLowerCase().includes(q))
    );
  });

  // قائمة التصنيفات المتاحة لتكون تصنيفاً أباً (نستثني التصنيف نفسه عند التعديل لمنع الحلقات الدائرية)


  return (
    <div className="flex flex-col gap-4">
      {/* الترويسة وأزرار الإجراءات والبحث */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 bg-white p-4 rounded-xl border border-slate-200 shadow-2xs">
        <div>
          <h3 className="text-base font-bold text-slate-900 flex items-center gap-2">
            <IconHierarchy className="w-5 h-5 text-emerald-600" />
            <span>تصنيفات المنتجات والهيكل الشجري</span>
          </h3>
          <p className="text-xs text-slate-500 mt-0.5">
            إدارة فئات الكتالوج وربط التصنيفات الفرعية بالتصنيف الأب لتنظيم الأصناف
          </p>
        </div>

        <div className="flex items-center gap-2">
          <div className="w-full sm:w-56">
            <Input
              placeholder="بحث في التصنيفات..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); onSearchChange?.(e.target.value); }}
              leftIcon={<IconSearch className="w-4 h-4" />}
            />
          </div>

          {canCreate && <Button
            variant="primary"
            size="md"
            leftIcon={<IconPlus className="w-4 h-4" />}
            onClick={handleOpenAddModal}
            className="shrink-0"
          >
            + تصنيف جديد
          </Button>}
        </div>
      </div>

      {/* بطاقات التصنيفات */}
      {filteredCategories.length === 0 ? (
        <div className="p-12 text-center bg-white rounded-xl border border-slate-200 text-slate-500 text-sm">
          {search ? 'لا توجد تصنيفات مطابقة للبحث' : 'لم يتم تسجيل أي تصنيفات حتى الآن. ابدأ بإضافة تصنيف جديد.'}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
          {filteredCategories.map((cat) => {
            const isSubCategory = Boolean(cat.parentCategoryId);
            const parentName =
              cat.parentCategoryName ||
              categories.find((c) => c.id === cat.parentCategoryId)?.name;

            // حساب عدد التصنيفات الفرعية المباشرة التابعة لهذا التصنيف
            const subCategoriesCount = cat.subCategoriesCount ?? categories.filter((c) => c.parentCategoryId === cat.id).length;

            return (
              <div
                key={cat.id}
                className="p-4 rounded-xl border border-slate-200 bg-white hover:border-emerald-300 hover:shadow-xs transition-all flex flex-col justify-between gap-3 group"
              >
                <div className="flex items-start justify-between gap-2">
                  <div className="flex items-start gap-3 min-w-0">
                    <div
                      className={`w-10 h-10 rounded-xl flex items-center justify-center shrink-0 ${
                        isSubCategory
                          ? 'bg-sky-50 text-sky-700 border border-sky-200/60'
                          : 'bg-emerald-50 text-emerald-700 border border-emerald-200/60'
                      }`}
                    >
                      {isSubCategory ? (
                        <IconCornerDownLeft className="w-5 h-5" />
                      ) : (
                        <IconFolder className="w-5 h-5" />
                      )}
                    </div>

                    <div className="flex flex-col min-w-0">
                      <h4 className="text-sm font-bold text-slate-900 truncate leading-snug">
                        {cat.name}
                      </h4>

                      {/* شارة التبعية لتصنيف أب أو تصنيف رئيسي */}
                      <div className="mt-1 flex flex-wrap items-center gap-1.5">
                        {isSubCategory ? (
                          <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded-md text-[11px] font-semibold bg-sky-50 text-sky-800 border border-sky-200/70">
                            <IconCornerDownLeft className="w-3 h-3 text-sky-600" />
                            <span>فرعي من: <strong className="font-bold">{parentName || 'تصنيف أب'}</strong></span>
                          </span>
                        ) : (
                          <span className="inline-flex items-center px-2 py-0.5 rounded-md text-[11px] font-semibold bg-emerald-50 text-emerald-800 border border-emerald-200/70">
                            تصنيف رئيسي
                          </span>
                        )}

                        {subCategoriesCount > 0 && (
                          <span className="inline-flex items-center px-1.5 py-0.5 rounded-md text-[10px] font-bold bg-slate-100 text-slate-600">
                            {subCategoriesCount} فروع
                          </span>
                        )}
                      </div>

                      {cat.description && (
                        <p className="text-[11px] text-slate-500 mt-1 line-clamp-1">
                          {cat.description}
                        </p>
                      )}
                    </div>
                  </div>

                  {/* إجراءات التعديل والحذف */}
                  <div className="flex items-center gap-1 shrink-0 opacity-80 group-hover:opacity-100 transition-opacity">
                    {canEdit && <button
                      type="button"
                      onClick={() => handleOpenEditModal(cat)}
                      className="p-1.5 text-slate-500 hover:text-emerald-700 hover:bg-emerald-50 rounded-lg transition-colors cursor-pointer"
                      title="تعديل بيانات التصنيف والتصنيف الأب"
                    >
                      <IconEdit className="w-4 h-4" />
                    </button>}

                    {canDelete && onDeleteCategory && (
                      <button
                        type="button"
                        onClick={() => setCategoryToDelete(cat)}
                        className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors cursor-pointer"
                        title="حذف التصنيف"
                      >
                        <IconTrash className="w-4 h-4" />
                      </button>
                    )}
                  </div>
                </div>

                <div className="flex items-center justify-between pt-2 border-t border-slate-100 text-xs text-slate-400">
                  <span>الأصناف المسجلة:</span>
                  <span className="font-extrabold text-slate-700 num-ltr">
                    {cat.productsCount ?? cat.productCount ?? 0}
                  </span>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* نافذة إضافة / تعديل تصنيف وربطه بتصنيف أب */}
      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title={editingCategory ? 'تعديل بيانات التصنيف' : 'إضافة تصنيف جديد'}
        subtitle="يمكنك تحديد تصنيف أب لإنشاء هيكلية شجرية كتلك الموجودة في تطبيق سطح المكتب"
        size="md"
      >
        <form onSubmit={handleSave} className="flex flex-col gap-4">
          <Input
            label="اسم التصنيف *"
            placeholder="مثال: مشروبات ساخنة، معلبات، إلكترونيات..."
            value={name}
            onChange={(e) => setName(e.target.value)}
            autoFocus
            required
          />

          {/* اختيار التصنيف الأب (Parent Category) متطابق تماماً مع سطح المكتب */}
          <div>
            <label className="text-xs font-semibold text-slate-700 block mb-1">
              التصنيف الأب (اختياري)
            </label>
            <Select lookup={{ resource: 'categories' }} label="التصنيف الأب" value={parentCategoryId} onChange={(event) => setParentCategoryId(event.target.value)} />
            <p className="text-[11px] text-slate-400 mt-1">
              اختر تصنيفاً أعلى لربطه به كفرع، أو اتركه كـ "تصنيف رئيسي" إذا كان يمثل قسماً مستقلاً.
            </p>
          </div>

          <div>
            <label className="text-xs font-semibold text-slate-700 block mb-1">
              وصف التصنيف (اختياري)
            </label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="وصف مختصر للأصناف التابعة لهذا التصنيف..."
              className="w-full text-xs p-2.5 rounded-lg border border-slate-200 focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 focus:outline-none"
              rows={2}
            />
          </div>

          <div className="flex items-center justify-end gap-2 mt-2 pt-3 border-t border-slate-100">
            <Button variant="secondary" size="md" onClick={() => setIsModalOpen(false)} disabled={isLoading}>
              إلغاء
            </Button>
            <Button variant="primary" size="md" type="submit" isLoading={isLoading}>
              {editingCategory ? 'حفظ التعديلات' : 'حفظ التصنيف'}
            </Button>
          </div>
        </form>
      </Modal>

      {/* نافذة تأكيد حذف التصنيف */}
      <ConfirmDialog
        isOpen={Boolean(categoryToDelete)}
        onClose={() => setCategoryToDelete(null)}
        onConfirm={handleDeleteConfirm}
        title="تأكيد حذف التصنيف"
        message={`هل أنت متأكد من رغبتك في حذف تصنيف "${categoryToDelete?.name}"؟`}
        confirmText="حذف التصنيف"
        cancelText="تراجع"
        variant="danger"
        isLoading={isDeleting}
      />
    </div>
  );
};

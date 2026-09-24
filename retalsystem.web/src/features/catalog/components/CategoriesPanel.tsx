import React, { useState } from 'react';
import type { CategoryDto } from '../types/catalog.types';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Modal } from '../../../shared/components/ui/Modal';
import { IconCategory, IconPlus } from '@tabler/icons-react';
import { useToast } from '../../../shared/context/ToastContext';

interface CategoriesPanelProps {
  categories: CategoryDto[];
  onAddCategory: (cat: { name: string; description?: string }) => Promise<void>;
}

export const CategoriesPanel: React.FC<CategoriesPanelProps> = ({ categories, onAddCategory }) => {
  const { showSuccess } = useToast();
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    setIsLoading(true);
    try {
      await onAddCategory({ name: name.trim(), description: description.trim() });
      showSuccess('تمت الإضافة', `تمت إضافة تصنيف ${name} بنجاح`);
      setName('');
      setDescription('');
      setIsModalOpen(false);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-base font-bold text-slate-900">تصنيفات المنتجات</h3>
          <p className="text-xs text-slate-500">إدارة فئات الكتالوج لتنظيم الأصناف وتسهيل البحث</p>
        </div>
        <Button
          variant="primary"
          size="sm"
          leftIcon={<IconPlus className="w-4 h-4" />}
          onClick={() => setIsModalOpen(true)}
        >
          + تصنيف جديد
        </Button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
        {categories.map((cat) => (
          <div
            key={cat.id}
            className="p-4 rounded-xl border border-slate-200 bg-white hover:border-emerald-300 transition-colors flex items-center justify-between shadow-2xs"
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center">
                <IconCategory className="w-5 h-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-slate-900">{cat.name}</h4>
                <p className="text-xs text-slate-400 mt-0.5">{cat.productsCount || 0} صنف مسجل</p>
              </div>
            </div>
          </div>
        ))}
      </div>

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="إضافة تصنيف جديد"
        size="sm"
      >
        <form onSubmit={handleSave} className="flex flex-col gap-3">
          <Input
            label="اسم التصنيف *"
            placeholder="مثال: الحلويات والمكسرات"
            value={name}
            onChange={(e) => setName(e.target.value)}
            autoFocus
            required
          />

          <div>
            <label className="text-xs font-semibold text-slate-700 block mb-1">وصف مختصر</label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="ملاحظات حول هذا التصنيف..."
              className="w-full text-xs p-2.5 rounded-lg border border-slate-200 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
              rows={2}
            />
          </div>

          <div className="flex items-center justify-end gap-2 mt-2 pt-3 border-t border-slate-100">
            <Button variant="secondary" size="md" onClick={() => setIsModalOpen(false)}>
              إلغاء
            </Button>
            <Button variant="primary" size="md" type="submit" isLoading={isLoading}>
              حفظ التصنيف
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
};

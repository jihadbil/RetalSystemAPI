import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { posService } from '../services/posService';
import type { QuickProductItem } from '../types/pos.types';
import { IconPackage, IconSearch } from '@tabler/icons-react';
import { Input } from '../../../shared/components/ui/Input';

interface QuickItemsModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSelectItem: (item: QuickProductItem) => void;
}

export const QuickItemsModal: React.FC<QuickItemsModalProps> = ({
  isOpen,
  onClose,
  onSelectItem,
}) => {
  const [items, setItems] = useState<QuickProductItem[]>([]);
  const [search, setSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('الكل');

  useEffect(() => {
    if (isOpen) {
      posService.getQuickItems().then(setItems);
    }
  }, [isOpen]);

  const categories = ['الكل', ...Array.from(new Set(items.map((i) => i.categoryName || 'أخرى')))];

  const filtered = items.filter((item) => {
    const matchSearch = item.name.includes(search) || item.code.includes(search);
    const matchCat = selectedCategory === 'الكل' || item.categoryName === selectedCategory;
    return matchSearch && matchCat;
  });

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="الأصناف السريعة (F4)"
      subtitle="انقر على أي صنف لإضافته مباشرة إلى سلة المبيعات"
      size="xl"
    >
      <div className="flex flex-col gap-4">
        {/* شريط البحث وفلاتر التصنيفات */}
        <div className="flex flex-col sm:flex-row items-center gap-3">
          <div className="w-full sm:w-72">
            <Input
              placeholder="ابحث عن صنف سريع..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              leftIcon={<IconSearch className="w-4 h-4" />}
              autoFocus
            />
          </div>

          <div className="flex items-center gap-1.5 overflow-x-auto w-full pb-1">
            {categories.map((cat) => (
              <button
                key={cat}
                type="button"
                onClick={() => setSelectedCategory(cat)}
                className={`text-xs px-3 py-1.5 rounded-lg font-semibold transition-colors cursor-pointer shrink-0 ${
                  selectedCategory === cat
                    ? 'bg-emerald-600 text-white shadow-2xs'
                    : 'bg-slate-100 hover:bg-slate-200 text-slate-700'
                }`}
              >
                {cat}
              </button>
            ))}
          </div>
        </div>

        {/* شبكة الأصناف السريعة */}
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3 max-h-[55vh] overflow-y-auto p-1">
          {filtered.map((item) => (
            <button
              key={item.productId}
              type="button"
              onClick={() => {
                onSelectItem(item);
                onClose();
              }}
              className="flex flex-col items-start p-3.5 rounded-xl border border-slate-200 hover:border-emerald-400 bg-white hover:bg-emerald-50/40 transition-all duration-150 text-right shadow-2xs cursor-pointer group"
            >
              <div className="w-10 h-10 rounded-lg bg-emerald-50 text-emerald-700 flex items-center justify-center mb-2 group-hover:scale-105 transition-transform">
                <IconPackage className="w-5 h-5" />
              </div>

              <span className="text-xs font-bold text-slate-900 group-hover:text-emerald-800 line-clamp-1 mb-1">
                {item.name}
              </span>

              <span className="text-[10px] text-slate-400 mb-2 truncate w-full">
                {item.categoryName} · {item.code}
              </span>

              <div className="mt-auto pt-2 border-t border-slate-100 w-full flex items-center justify-between">
                <span className="text-xs font-extrabold text-emerald-700 num-ltr">
                  {item.price.toFixed(2)} د.ل
                </span>
                <span className="text-[10px] bg-emerald-100 text-emerald-800 font-bold px-1.5 py-0.5 rounded-md">
                  + إضافة
                </span>
              </div>
            </button>
          ))}
        </div>
      </div>
    </Modal>
  );
};

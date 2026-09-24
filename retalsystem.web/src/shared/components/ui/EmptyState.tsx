import React from 'react';
import { IconInbox, IconSearch } from '@tabler/icons-react';
import { Button } from './Button';

interface EmptyStateProps {
  title?: string;
  description?: string;
  isSearch?: boolean;
  actionText?: string;
  actionLabel?: string;
  onAction?: () => void;
  icon?: React.ReactNode;
}

export const EmptyState: React.FC<EmptyStateProps> = ({
  title,
  description,
  isSearch = false,
  actionText,
  actionLabel,
  onAction,
  icon,
}) => {
  const buttonLabel = actionText || actionLabel;
  const defaultTitle = isSearch ? 'لا توجد نتائج مطابقة لبحثك' : 'لا توجد بيانات حالياً';
  const defaultDescription = isSearch
    ? 'جرّب تعديل كلمات البحث أو مسح الفلاتر المطبقة للعثور على السجلات.'
    : 'لم يتم العثور على أي سجلات في هذا القسم بعد، يمكنك البدء بإضافة سجل جديد.';

  return (
    <div className="flex flex-col items-center justify-center py-12 px-4 text-center max-w-sm mx-auto">
      <div className="w-14 h-14 rounded-2xl bg-slate-100 flex items-center justify-center text-slate-400 mb-3.5 shadow-inner">
        {icon ? (
          icon
        ) : isSearch ? (
          <IconSearch className="w-7 h-7 stroke-[1.5]" />
        ) : (
          <IconInbox className="w-7 h-7 stroke-[1.5]" />
        )}
      </div>

      <h4 className="text-sm font-bold text-slate-800 mb-1">{title || defaultTitle}</h4>
      <p className="text-xs text-slate-500 leading-relaxed mb-4">{description || defaultDescription}</p>

      {buttonLabel && onAction && (
        <Button variant={isSearch ? 'secondary' : 'primary'} size="sm" onClick={onAction}>
          {buttonLabel}
        </Button>
      )}
    </div>
  );
};

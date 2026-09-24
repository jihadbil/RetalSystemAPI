import React from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { EmptyState } from '../../../shared/components/ui/EmptyState';
import type { PosDraft } from '../types/pos.types';
import { IconClock, IconTrash, IconCheck, IconReceipt } from '@tabler/icons-react';

interface PosDraftsModalProps {
  isOpen: boolean;
  onClose: () => void;
  drafts: PosDraft[];
  onRecall: (draftId: string) => void;
  onDelete: (draftId: string) => void;
}

export const PosDraftsModal: React.FC<PosDraftsModalProps> = ({
  isOpen,
  onClose,
  drafts,
  onRecall,
  onDelete,
}) => {
  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="الطلبات المعلقة والمسودات (F5 / F6)"
      subtitle="اختر الطلب المعلق لاستعادته إلى السلة ومتابعة عملية الدفع"
      size="lg"
    >
      {drafts.length === 0 ? (
        <EmptyState
          title="لا توجد طلبات معلقة حالياً"
          description="يمكنك تعليق أي طلب نشط بالضغط على زر تعليق الطلب أو اختصار (F5) لخدمة زبون آخر مؤقتاً."
          icon={<IconClock className="w-8 h-8 stroke-[1.5]" />}
        />
      ) : (
        <div className="flex flex-col gap-3 max-h-[60vh] overflow-y-auto p-1">
          {drafts.map((draft) => (
            <div
              key={draft.id}
              className="flex flex-col sm:flex-row sm:items-center justify-between p-4 rounded-xl border border-slate-200 bg-white hover:border-emerald-300 hover:shadow-xs transition-all gap-3"
            >
              <div className="flex items-start gap-3">
                <div className="w-10 h-10 rounded-xl bg-amber-50 text-amber-600 flex items-center justify-center shrink-0 mt-0.5">
                  <IconReceipt className="w-5 h-5" />
                </div>

                <div>
                  <div className="flex items-center gap-2">
                    <h4 className="text-sm font-bold text-slate-900">
                      {draft.customerName || 'عميل نقدي'}
                    </h4>
                    <span className="text-[11px] text-slate-400 font-mono">
                      #{draft.id.slice(-4)}
                    </span>
                  </div>

                  <p className="text-xs text-slate-500 mt-1">
                    {draft.items.length} أصناف · وقت التعليق:{' '}
                    <span className="font-semibold text-slate-700">
                      {new Date(draft.savedAt).toLocaleTimeString('ar-LY', {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </span>
                  </p>

                  <div className="text-xs font-extrabold text-emerald-700 mt-1 num-ltr">
                    الإجمالي: {draft.total.toFixed(2)} د.ل
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-2 self-end sm:self-center">
                <Button
                  variant="danger"
                  size="sm"
                  onClick={() => onDelete(draft.id)}
                  title="حذف المسودة"
                >
                  <IconTrash className="w-4 h-4" />
                </Button>

                <Button
                  variant="primary"
                  size="sm"
                  leftIcon={<IconCheck className="w-4 h-4" />}
                  onClick={() => {
                    onRecall(draft.id);
                    onClose();
                  }}
                >
                  استعادة الطلب (F6)
                </Button>
              </div>
            </div>
          ))}
        </div>
      )}
    </Modal>
  );
};

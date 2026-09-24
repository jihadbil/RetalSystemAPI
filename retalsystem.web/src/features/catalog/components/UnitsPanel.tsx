import React from 'react';
import type { UnitDto } from '../types/catalog.types';
import { Badge } from '../../../shared/components/ui/Badge';
import { IconScale } from '@tabler/icons-react';

interface UnitsPanelProps {
  units: UnitDto[];
}

export const UnitsPanel: React.FC<UnitsPanelProps> = ({ units }) => {
  return (
    <div className="flex flex-col gap-4">
      <div>
        <h3 className="text-base font-bold text-slate-900">وحدات القياس المعتمدة</h3>
        <p className="text-xs text-slate-500">الوحدات المستخدمة في عمليات الشراء والبيع والتحويلات</p>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
        {units.map((u) => (
          <div
            key={u.id}
            className="p-4 rounded-xl border border-slate-200 bg-white flex items-center justify-between shadow-2xs"
          >
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 rounded-lg bg-slate-100 text-slate-700 flex items-center justify-center">
                <IconScale className="w-5 h-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-slate-900">{u.name}</h4>
                <p className="text-xs text-slate-400 mt-0.5">الرمز: {u.symbol}</p>
              </div>
            </div>

            <Badge variant={u.isBaseUnit ? 'success' : 'neutral'} size="sm">
              {u.isBaseUnit ? 'وحدة أساسية' : 'وحدة تحويل'}
            </Badge>
          </div>
        ))}
      </div>
    </div>
  );
};

import React from 'react';
import type { DailySalesPoint } from '../../../features/dashboard/types/dashboard.types';

interface SalesTrendChartProps {
  data: DailySalesPoint[];
}

export const SalesTrendChart: React.FC<SalesTrendChartProps> = ({ data }) => {
  if (!data || data.length === 0) {
    return (
      <div className="h-48 flex items-center justify-center text-xs text-slate-400">
        لا تتوفر بيانات كافية لعرض الرسم البياني
      </div>
    );
  }

  const maxSales = Math.max(...data.map((d) => d.salesAmount), 100);

  return (
    <div className="w-full">
      <div className="flex items-end gap-3 h-48 pt-6 pb-2 px-2 border-b border-slate-100">
        {data.map((point, index) => {
          const heightPercent = Math.min(100, Math.max(10, (point.salesAmount / maxSales) * 100));
          return (
            <div key={index} className="flex-1 flex flex-col items-center h-full justify-end group relative">
              {/* تلميح القيمة فوق العمود عند التحويم */}
              <div className="absolute -top-7 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none bg-slate-900 text-white text-[10px] font-bold py-0.5 px-2 rounded-md whitespace-nowrap z-10 shadow-sm num-ltr">
                {point.salesAmount.toLocaleString('en-US', { minimumFractionDigits: 0 })} د.ل
              </div>

              {/* عمود المبيعات */}
              <div
                className="w-full max-w-[36px] bg-emerald-100 group-hover:bg-emerald-500 rounded-t-md transition-all duration-300 relative overflow-hidden"
                style={{ height: `${heightPercent}%` }}
              >
                <div
                  className="absolute bottom-0 inset-x-0 bg-emerald-600 transition-all duration-300 opacity-80"
                  style={{ height: `${Math.min(100, (point.profitAmount / Math.max(point.salesAmount, 1)) * 100)}%` }}
                  title={`الربح: ${point.profitAmount.toFixed(0)} د.ل`}
                />
              </div>

              {/* اسم اليوم */}
              <span className="text-[11px] text-slate-500 font-medium mt-2 truncate w-full text-center">
                {point.dayName || new Date(point.date).toLocaleDateString('ar-LY', { weekday: 'short' })}
              </span>
            </div>
          );
        })}
      </div>

      <div className="flex items-center justify-center gap-6 mt-3 text-xs text-slate-500 font-medium">
        <div className="flex items-center gap-1.5">
          <span className="w-3 h-3 rounded-xs bg-emerald-500 inline-block" />
          <span>إجمالي المبيعات</span>
        </div>
        <div className="flex items-center gap-1.5">
          <span className="w-3 h-3 rounded-xs bg-emerald-700 inline-block" />
          <span>صافي الربح التقديري</span>
        </div>
      </div>
    </div>
  );
};

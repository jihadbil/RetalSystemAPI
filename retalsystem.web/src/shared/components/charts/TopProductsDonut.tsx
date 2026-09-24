import React from 'react';
import type { TopSellingProduct } from '../../../features/dashboard/types/dashboard.types';

interface TopProductsDonutProps {
  products: TopSellingProduct[];
}

const COLORS = ['#059669', '#10B981', '#34D399', '#6EE7B7', '#A7F3D0'];

export const TopProductsDonut: React.FC<TopProductsDonutProps> = ({ products }) => {
  if (!products || products.length === 0) {
    return (
      <div className="h-48 flex items-center justify-center text-xs text-slate-400">
        لا توجد مبيعات مسجلة حتى الآن
      </div>
    );
  }

  const topItems = products.slice(0, 5);
  const totalSales = topItems.reduce((sum, p) => sum + p.totalSales, 0) || 1;

  return (
    <div className="flex flex-col gap-3 py-1">
      {topItems.map((prod, idx) => {
        const percent = Math.round((prod.totalSales / totalSales) * 100);
        const color = COLORS[idx % COLORS.length];

        return (
          <div key={prod.productId || idx} className="flex flex-col gap-1">
            <div className="flex items-center justify-between text-xs">
              <span className="font-semibold text-slate-800 truncate max-w-[180px]">
                {prod.productName}
              </span>
              <div className="flex items-center gap-2">
                <span className="text-slate-500">{prod.quantitySold} قطعة</span>
                <span className="font-bold text-slate-900 num-ltr">
                  {prod.totalSales.toLocaleString('en-US', { minimumFractionDigits: 0 })} د.ل
                </span>
              </div>
            </div>

            {/* شريط النسبة المئوية */}
            <div className="w-full h-2 bg-slate-100 rounded-full overflow-hidden flex">
              <div
                className="h-full rounded-full transition-all duration-500"
                style={{
                  width: `${percent}%`,
                  backgroundColor: color,
                }}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
};

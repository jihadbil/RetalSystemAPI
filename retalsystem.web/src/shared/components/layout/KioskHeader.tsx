import React, { useState, useEffect } from 'react';
import { IconArrowsMinimize, IconBuildingStore, IconUser, IconClock } from '@tabler/icons-react';
import { useAuth } from '../../context/AuthContext';

interface KioskHeaderProps {
  onExitKiosk: () => void;
}

export const KioskHeader: React.FC<KioskHeaderProps> = ({ onExitKiosk }) => {
  const { user } = useAuth();
  const [time, setTime] = useState<string>('');

  useEffect(() => {
    const updateTime = () => {
      const now = new Date();
      setTime(now.toLocaleTimeString('ar-LY', { hour: '2-digit', minute: '2-digit', second: '2-digit' }));
    };
    updateTime();
    const timer = setInterval(updateTime, 1000);
    return () => clearInterval(timer);
  }, []);

  return (
    <div className="h-12 bg-slate-900 text-white px-4 flex items-center justify-between shadow-md shrink-0 select-none">
      <div className="flex items-center gap-4">
        <span className="font-extrabold text-sm tracking-wide text-emerald-400">
          ريتـــال <span className="text-white text-xs">نقطة البيع السريعة</span>
        </span>

        <div className="hidden sm:flex items-center gap-1.5 text-xs text-slate-300">
          <IconBuildingStore className="w-4 h-4 text-emerald-400" />
          <span>{user?.branchName || 'الفرع الرئيسي'}</span>
        </div>

        <div className="hidden sm:flex items-center gap-1.5 text-xs text-slate-300">
          <IconUser className="w-4 h-4 text-emerald-400" />
          <span>الكاشير: {user?.fullName || user?.userName}</span>
        </div>
      </div>

      <div className="flex items-center gap-4">
        <div className="flex items-center gap-1.5 text-xs text-slate-300 num-ltr font-mono">
          <IconClock className="w-4 h-4 text-emerald-400" />
          <span>{time}</span>
        </div>

        <button
          type="button"
          onClick={onExitKiosk}
          className="flex items-center gap-1.5 px-2.5 py-1 rounded-md bg-slate-800 hover:bg-slate-700 text-slate-200 text-xs font-semibold transition-colors cursor-pointer"
          title="الخروج من وضع الشاشة الكاملة (F11)"
        >
          <IconArrowsMinimize className="w-3.5 h-3.5 text-emerald-400" />
          <span>خروج (F11)</span>
        </button>
      </div>
    </div>
  );
};

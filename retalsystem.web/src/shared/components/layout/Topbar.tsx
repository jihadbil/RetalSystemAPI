import React, { useState, useEffect } from 'react';
import {
  IconMenu2,
  IconBuildingStore,
  IconLock,
  IconLogout,
  IconCashRegister,
  IconChevronDown,
  IconUser,
  IconCheck,
} from '@tabler/icons-react';
import { useAuth } from '../../context/AuthContext';
import { organizationService } from '../../../features/organization/services/organizationService';
import type { BranchDto } from '../../../features/organization/types/organization.types';
import { Button } from '../ui/Button';

interface TopbarProps {
  onToggleSidebar: () => void;
  onOpenPos: () => void;
}

export const Topbar: React.FC<TopbarProps> = ({ onToggleSidebar, onOpenPos }) => {
  const { user, logout, lockScreen, currentBranchId, switchBranch } = useAuth();
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [showBranchMenu, setShowBranchMenu] = useState(false);
  const [branches, setBranches] = useState<BranchDto[]>([]);

  useEffect(() => {
    organizationService
      .getBranches()
      .then((list) => {
        setBranches(list.filter((b) => b.isActive));
      })
      .catch((err) => {
        console.error('Failed to load branches for Topbar:', err);
      });
  }, []);

  const activeBranch = branches.find((b) => b.id === currentBranchId);
  const branchDisplayName = activeBranch?.name || user?.branchName || 'الفرع الرئيسي';

  return (
    <header className="h-16 bg-white border-b border-slate-200/80 px-4 flex items-center justify-between sticky top-0 z-30 shadow-2xs">
      {/* الجانب الأيمن: زر طي القائمة وقائمة الفروع */}
      <div className="flex items-center gap-3">
        <button
          type="button"
          onClick={onToggleSidebar}
          className="p-2 rounded-lg text-slate-500 hover:text-slate-800 hover:bg-slate-100 transition-colors cursor-pointer"
          title="طي / فتح القائمة الجانبية (F11)"
        >
          <IconMenu2 className="w-5 h-5" />
        </button>

        {/* محول الفرع النشط */}
        <div className="relative">
          <button
            type="button"
            onClick={() => branches.length > 1 && setShowBranchMenu(!showBranchMenu)}
            disabled={branches.length <= 1}
            className={`hidden sm:flex items-center gap-2 px-3 py-1.5 rounded-lg border text-xs font-semibold transition-colors ${
              branches.length > 1
                ? 'bg-slate-50 hover:bg-slate-100 border-slate-200/80 text-slate-700 cursor-pointer'
                : 'bg-slate-50 border-slate-200/70 text-slate-700 cursor-default'
            }`}
            title={branches.length > 1 ? 'انقر للتبديل بين فروع المنشأة' : 'الفرع النشط حالياً'}
          >
            <IconBuildingStore className="w-4 h-4 text-emerald-600 shrink-0" />
            <span className="text-slate-500 font-normal">الفرع:</span>
            <span className="text-emerald-800 font-bold">{branchDisplayName}</span>
            {branches.length > 1 && (
              <IconChevronDown className="w-3.5 h-3.5 text-slate-400 shrink-0 mr-0.5" />
            )}
          </button>

          {showBranchMenu && branches.length > 1 && (
            <>
              <div
                className="fixed inset-0 z-40"
                onClick={() => setShowBranchMenu(false)}
                aria-hidden="true"
              />
              <div className="absolute right-0 mt-1 w-56 bg-white rounded-xl shadow-lg border border-slate-200/90 py-1.5 z-50 animate-in fade-in zoom-in-95 duration-150">
                <div className="px-3 py-1.5 border-b border-slate-100 text-[11px] font-bold text-slate-500 uppercase tracking-wider">
                  تبديل الفرع النشط
                </div>
                <div className="max-h-60 overflow-y-auto py-1">
                  {branches.map((b) => {
                    const isSelected = b.id === (currentBranchId || activeBranch?.id);
                    return (
                      <button
                        key={b.id}
                        type="button"
                        onClick={() => {
                          switchBranch(b.id, b.name);
                          setShowBranchMenu(false);
                        }}
                        className={`w-full flex items-center justify-between px-3.5 py-2 text-xs font-medium text-right transition-colors cursor-pointer ${
                          isSelected
                            ? 'bg-emerald-50 text-emerald-800 font-bold'
                            : 'text-slate-700 hover:bg-slate-50'
                        }`}
                      >
                        <div className="flex flex-col">
                          <span>{b.name}</span>
                          {b.address && (
                            <span className="text-[10px] text-slate-400 font-normal">{b.address}</span>
                          )}
                        </div>
                        {isSelected && <IconCheck className="w-4 h-4 text-emerald-600 shrink-0 mr-2" />}
                      </button>
                    );
                  })}
                </div>
              </div>
            </>
          )}
        </div>
      </div>

      {/* الجانب الأيسر: اختصار نقطة البيع السريع، قفل الشاشة، والمستخدم */}
      <div className="flex items-center gap-2 sm:gap-3">
        {/* زر فتح نقطة البيع السريع */}
        <Button
          variant="primary"
          size="sm"
          leftIcon={<IconCashRegister className="w-4 h-4" />}
          onClick={onOpenPos}
          className="hidden xs:inline-flex"
        >
          نقطة البيع (F1)
        </Button>

        {/* زر قفل الشاشة السريع */}
        <button
          type="button"
          onClick={lockScreen}
          className="p-2 rounded-lg text-slate-500 hover:text-amber-700 hover:bg-amber-50 transition-colors cursor-pointer"
          title="قفل الشاشة السريع (Lock)"
        >
          <IconLock className="w-5 h-5" />
        </button>

        {/* القائمة المنسدلة لبيانات المستخدم وتسجيل الخروج */}
        <div className="relative">
          <button
            type="button"
            onClick={() => setShowUserMenu(!showUserMenu)}
            className="flex items-center gap-2 p-1.5 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer border border-transparent hover:border-slate-200"
          >
            <div className="w-8 h-8 rounded-full bg-emerald-100 text-emerald-800 flex items-center justify-center font-bold text-xs">
              {user?.fullName ? user.fullName[0] : <IconUser className="w-4 h-4" />}
            </div>
            <div className="hidden md:flex flex-col text-right">
              <span className="text-xs font-bold text-slate-900 leading-tight">
                {user?.fullName || user?.userName || 'المستخدم'}
              </span>
              <span className="text-[10px] text-slate-500 font-medium">
                {user?.roles?.[0] || 'كاشير'}
              </span>
            </div>
            <IconChevronDown className="w-3.5 h-3.5 text-slate-400" />
          </button>

          {showUserMenu && (
            <>
              <div
                className="fixed inset-0 z-40"
                onClick={() => setShowUserMenu(false)}
                aria-hidden="true"
              />
              <div className="absolute left-0 mt-1 w-52 bg-white rounded-xl shadow-lg border border-slate-200/90 py-1 z-50 animate-in fade-in zoom-in-95 duration-150">
                <div className="px-3.5 py-2.5 border-b border-slate-100">
                  <p className="text-xs font-bold text-slate-900 truncate">{user?.fullName || user?.userName}</p>
                  <p className="text-[11px] text-slate-500 truncate">{user?.email || 'لا يوجد بريد إلكتروني'}</p>
                </div>

                <button
                  type="button"
                  onClick={() => {
                    setShowUserMenu(false);
                    lockScreen();
                  }}
                  className="w-full flex items-center gap-2.5 px-3.5 py-2 text-xs font-medium text-slate-700 hover:bg-slate-50 transition-colors"
                >
                  <IconLock className="w-4 h-4 text-slate-500" />
                  <span>قفل الشاشة السريع</span>
                </button>

                <div className="border-t border-slate-100 my-1" />

                <button
                  type="button"
                  onClick={() => {
                    setShowUserMenu(false);
                    logout();
                  }}
                  className="w-full flex items-center gap-2.5 px-3.5 py-2 text-xs font-semibold text-rose-600 hover:bg-rose-50 transition-colors"
                >
                  <IconLogout className="w-4 h-4" />
                  <span>تسجيل الخروج</span>
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    </header>
  );
};

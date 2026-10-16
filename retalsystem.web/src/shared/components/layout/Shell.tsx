import React, { useState } from 'react';
import { Sidebar, NavigationItem } from './Sidebar';
import { Topbar } from './Topbar';
import { KioskHeader } from './KioskHeader';
import { useAuth } from '../../context/AuthContext';
import { useHotkeys } from '../../hooks/useHotkeys';
import { Modal } from '../ui/Modal';
import { Button } from '../ui/Button';
import { Input } from '../ui/Input';
import { IconLock, IconKey } from '@tabler/icons-react';

interface ShellProps {
  currentView: NavigationItem;
  onNavigate: (view: NavigationItem) => void;
  children: React.ReactNode;
}

export const Shell: React.FC<ShellProps> = ({ currentView, onNavigate, children }) => {
  const { isLocked, unlockScreen } = useAuth();
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const [isKioskMode, setIsKioskMode] = useState(false);
  const [pinInput, setPinInput] = useState('');
  const [pinError, setPinError] = useState('');
  const [isUnlocking, setIsUnlocking] = useState(false);

  const handleToggleSidebar = () => {
    if (typeof window !== 'undefined' && window.innerWidth < 1024) {
      setIsMobileMenuOpen((prev) => !prev);
    } else {
      setIsSidebarCollapsed((prev) => !prev);
    }
  };

  // اختصار F11 لتبديل الشريط الجانبي أو وضع الكاشير
  useHotkeys({
    F11: () => {
      if (currentView === 'pos') {
        setIsKioskMode((prev) => !prev);
      } else {
        handleToggleSidebar();
      }
    },
  });

  const handleUnlock = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!pinInput.trim()) {
      setPinError('يرجى إدخال كلمة المرور');
      return;
    }

    setIsUnlocking(true);
    try {
      const success = await unlockScreen(pinInput);
      if (success) {
        setPinInput('');
        setPinError('');
      } else {
        setPinError('كلمة المرور غير صحيحة');
      }
    } finally {
      setIsUnlocking(false);
    }
  };

  const isPosKiosk = currentView === 'pos' && isKioskMode;

  return (
    <div className="min-h-screen bg-[#F6F8F7] text-slate-900 flex flex-col font-sans">
      {/* وضع الكاشير الكامل (Kiosk Mode) */}
      {isPosKiosk ? (
        <div className="flex flex-col h-screen overflow-hidden">
          <KioskHeader onExitKiosk={() => setIsKioskMode(false)} />
          <main className="flex-1 overflow-hidden">{children}</main>
        </div>
      ) : (
        <div className="flex min-h-screen relative">
          {/* طبقة التعتيم الخلفية عند فتح القائمة على الهواتف والشاشات الصغيرة */}
          {isMobileMenuOpen && (
            <div
              className="fixed inset-0 z-40 bg-slate-900/50 backdrop-blur-xs lg:hidden transition-opacity"
              onClick={() => setIsMobileMenuOpen(false)}
              aria-hidden="true"
            />
          )}

          {/* القائمة الجانبية */}
          <Sidebar
            currentView={currentView}
            onNavigate={(view) => {
              onNavigate(view);
              setIsMobileMenuOpen(false);
              if (view !== 'pos') setIsKioskMode(false);
            }}
            isCollapsed={isSidebarCollapsed}
            onToggleCollapse={handleToggleSidebar}
            isMobileOpen={isMobileMenuOpen}
            onCloseMobile={() => setIsMobileMenuOpen(false)}
          />

          {/* مساحة العمل الرئيسية: متجاوبة 100% على الهواتف بدون هامش ثابت */}
          <div
            className={`flex-1 flex flex-col min-w-0 transition-all duration-200 mr-0 ${
              isSidebarCollapsed ? 'lg:mr-18' : 'lg:mr-64'
            }`}
          >
            <Topbar
              onToggleSidebar={handleToggleSidebar}
              onOpenPos={() => onNavigate('pos')}
            />

            <main className="flex-1 p-3 sm:p-5 lg:p-6 overflow-y-auto">{children}</main>
          </div>
        </div>
      )}

      {/* نافذة قفل الشاشة السريع المنبثقة */}
      <Modal
        isOpen={isLocked}
        onClose={() => {}}
        title="شاشة النظام مقفلة"
        subtitle="أعد إدخال كلمة مرور حسابك لفك القفل"
        size="sm"
        closeOnBackdrop={false}
      >
        <form onSubmit={handleUnlock} className="flex flex-col items-center gap-4 py-2">
          <div className="w-14 h-14 rounded-full bg-amber-50 text-amber-600 flex items-center justify-center">
            <IconLock className="w-7 h-7" />
          </div>

          <p className="text-xs text-slate-500 text-center">
            جلسة العمل محفوظة. أدخل كلمة مرور حسابك الحالي للمتابعة.
          </p>

          <div className="w-full">
            <Input
              type="password"
              placeholder="كلمة المرور"
              value={pinInput}
              onChange={(e) => {
                setPinInput(e.target.value);
                setPinError('');
              }}
              error={pinError}
              leftIcon={<IconKey className="w-4 h-4" />}
              autoFocus
              autoComplete="current-password"
              className="text-center font-mono tracking-widest text-lg"
            />
          </div>

          <Button variant="primary" size="md" type="submit" className="w-full mt-2" isLoading={isUnlocking}>
            فتح الشاشة ومتابعة العمل
          </Button>
        </form>
      </Modal>
    </div>
  );
};

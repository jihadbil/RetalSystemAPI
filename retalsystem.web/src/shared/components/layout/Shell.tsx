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
  const [isKioskMode, setIsKioskMode] = useState(false);
  const [pinInput, setPinInput] = useState('');
  const [pinError, setPinError] = useState('');

  // اختصار F11 لتبديل الشريط الجانبي أو وضع الكاشير
  useHotkeys({
    F11: () => {
      if (currentView === 'pos') {
        setIsKioskMode((prev) => !prev);
      } else {
        setIsSidebarCollapsed((prev) => !prev);
      }
    },
  });

  const handleUnlock = (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!pinInput.trim()) {
      setPinError('يرجى إدخال رمز PIN');
      return;
    }

    const success = unlockScreen(pinInput.trim());
    if (success) {
      setPinInput('');
      setPinError('');
    } else {
      setPinError('رمز PIN غير صحيح (الافتراضي 1234)');
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
        <div className="flex min-h-screen">
          {/* القائمة الجانبية */}
          <Sidebar
            currentView={currentView}
            onNavigate={(view) => {
              onNavigate(view);
              if (view !== 'pos') setIsKioskMode(false);
            }}
            isCollapsed={isSidebarCollapsed}
            onToggleCollapse={() => setIsSidebarCollapsed(!isSidebarCollapsed)}
          />

          {/* مساحة العمل الرئيسية */}
          <div
            className={`flex-1 flex flex-col min-w-0 transition-all duration-200 ${
              isSidebarCollapsed ? 'mr-18' : 'mr-64'
            }`}
          >
            <Topbar
              onToggleSidebar={() => setIsSidebarCollapsed(!isSidebarCollapsed)}
              onOpenPos={() => onNavigate('pos')}
            />

            <main className="flex-1 p-4 sm:p-5 lg:p-6 overflow-y-auto">{children}</main>
          </div>
        </div>
      )}

      {/* نافذة قفل الشاشة السريع المنبثقة */}
      <Modal
        isOpen={isLocked}
        onClose={() => {}}
        title="شاشة النظام مقفلة"
        subtitle="أدخل رمز PIN لفك القفل والعودة للمبيعات"
        size="sm"
        closeOnBackdrop={false}
      >
        <form onSubmit={handleUnlock} className="flex flex-col items-center gap-4 py-2">
          <div className="w-14 h-14 rounded-full bg-amber-50 text-amber-600 flex items-center justify-center">
            <IconLock className="w-7 h-7" />
          </div>

          <p className="text-xs text-slate-500 text-center">
            جلسة العمل محفوظة بأمان. أدخل رمز الحماية المكون من 4 أرقام للمتابعة (الافتراضي 1234).
          </p>

          <div className="w-full">
            <Input
              type="password"
              placeholder="رمز PIN"
              value={pinInput}
              onChange={(e) => {
                setPinInput(e.target.value);
                setPinError('');
              }}
              error={pinError}
              leftIcon={<IconKey className="w-4 h-4" />}
              autoFocus
              maxLength={8}
              className="text-center font-mono tracking-widest text-lg"
            />
          </div>

          <Button variant="primary" size="md" type="submit" className="w-full mt-2">
            فتح الشاشة ومتابعة العمل
          </Button>
        </form>
      </Modal>
    </div>
  );
};

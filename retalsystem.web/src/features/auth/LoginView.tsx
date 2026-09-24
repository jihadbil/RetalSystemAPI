import React, { useState } from 'react';
import { useAuth } from '../../shared/context/AuthContext';
import { useToast } from '../../shared/context/ToastContext';
import { authService } from './services/authService';
import { Input } from '../../shared/components/ui/Input';
import { Button } from '../../shared/components/ui/Button';
import { IconUser, IconLock, IconShieldLock } from '@tabler/icons-react';

export const LoginView: React.FC = () => {
  const { login } = useAuth();
  const { showError, showSuccess } = useToast();

  const [userName, setUserName] = useState('admin');
  const [password, setPassword] = useState('admin');
  const [rememberMe, setRememberMe] = useState(true);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!userName.trim() || !password.trim()) {
      setError('يرجى إدخال اسم المستخدم وكلمة المرور');
      return;
    }

    setIsLoading(true);
    setError('');

    try {
      const response = await authService.login({
        userName: userName.trim(),
        password: password.trim(),
        rememberMe,
      });

      login(response, rememberMe);
      const displayName = response?.user?.fullName || response?.user?.userName || 'المستخدم';
      showSuccess('تم تسجيل الدخول بنجاح', `مرحباً بك مجدداً، ${displayName}`);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تسجيل الدخول، تحقق من البيانات المدخلة';
      setError(msg);
      showError('خطأ في المصادقة', msg);
    } finally {
      setIsLoading(false);
    }
  };

  const handleQuickFill = (role: 'admin' | 'cashier') => {
    setUserName(role);
    setPassword(role);
    setError('');
  };

  return (
    <div className="min-h-screen w-full bg-[#F6F8F7] flex flex-col items-center justify-center p-4 relative overflow-hidden font-sans">
      {/* خلفية تزيينية زمردية خفيفة */}
      <div className="absolute top-0 inset-x-0 h-64 bg-gradient-to-b from-emerald-600/10 to-transparent pointer-events-none" />

      <div className="w-full max-w-md relative z-10 flex flex-col items-center">
        {/* الشعار والعنوان */}
        <div className="flex flex-col items-center mb-6 text-center">
          <div className="w-16 h-16 rounded-2xl bg-gradient-to-tr from-emerald-700 to-emerald-500 flex items-center justify-center text-white font-extrabold text-3xl shadow-lg shadow-emerald-500/20 mb-3">
            R
          </div>
          <h1 className="text-2xl font-extrabold text-slate-900 tracking-tight">
            نظام ريتـــال <span className="text-emerald-600 text-sm font-bold">PRO</span>
          </h1>
          <p className="text-xs text-slate-500 mt-1">منظومة إدارة المبيعات ونقاط البيع السحابية والمخازن</p>
        </div>

        {/* بطاقة تسجيل الدخول */}
        <div className="w-full bg-white rounded-2xl border border-slate-200 shadow-md p-6 sm:p-8">
          <div className="flex items-center gap-2 mb-6 pb-4 border-b border-slate-100">
            <IconShieldLock className="w-5 h-5 text-emerald-600" />
            <h2 className="text-base font-bold text-slate-800">تسجيل الدخول للنظام</h2>
          </div>

          <form onSubmit={handleSubmit} className="flex flex-col gap-4">
            <Input
              label="اسم المستخدم"
              placeholder="مثال: admin"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              leftIcon={<IconUser className="w-4 h-4" />}
              autoFocus
              required
            />

            <Input
              label="كلمة المرور"
              type="password"
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              leftIcon={<IconLock className="w-4 h-4" />}
              required
            />

            <div className="flex items-center justify-between pt-1 text-xs">
              <label className="flex items-center gap-2 cursor-pointer select-none text-slate-600">
                <input
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  className="rounded border-slate-300 text-emerald-600 focus:ring-emerald-500 w-4 h-4"
                />
                <span>تذكرني على هذا الجهاز</span>
              </label>

              <span className="text-slate-400">PIN: 1234</span>
            </div>

            {error && (
              <div className="p-3 rounded-lg bg-rose-50 border border-rose-200 text-xs text-rose-700 font-medium leading-relaxed">
                {error}
              </div>
            )}

            <Button
              type="submit"
              variant="primary"
              size="lg"
              isLoading={isLoading}
              className="w-full mt-2"
            >
              تسجيل الدخول
            </Button>
          </form>

          {/* أزرار التجربة السريعة */}
          <div className="mt-6 pt-5 border-t border-slate-100 flex flex-col gap-2">
            <span className="text-[11px] font-semibold text-slate-400 text-center">حسابات تجريبية سريعة:</span>
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => handleQuickFill('admin')}
                className="flex-1 py-1.5 px-3 rounded-lg border border-slate-200 hover:border-emerald-300 hover:bg-emerald-50/50 text-xs font-semibold text-slate-700 transition-colors cursor-pointer"
              >
                مدير (Admin)
              </button>
              <button
                type="button"
                onClick={() => handleQuickFill('cashier')}
                className="flex-1 py-1.5 px-3 rounded-lg border border-slate-200 hover:border-emerald-300 hover:bg-emerald-50/50 text-xs font-semibold text-slate-700 transition-colors cursor-pointer"
              >
                كاشير (Cashier)
              </button>
            </div>
          </div>
        </div>

        <p className="text-[11px] text-slate-400 mt-6 text-center">
          جميع الحقوق محفوظة لنظام ريتال لإدارة الأعمال © 2026
        </p>
      </div>
    </div>
  );
};

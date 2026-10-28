import React, { lazy, Suspense, useState } from 'react';
import { useAuth } from './shared/context/AuthContext';
import { Shell } from './shared/components/layout/Shell';
import { NavigationItem } from './shared/components/layout/Sidebar';
import { LoginView } from './features/auth/LoginView';
import { VIEW_ACCESS, canOpenView } from './shared/auth/viewAccess';
import { Button } from './shared/components/ui/Button';

const DashboardView = lazy(() => import('./features/dashboard/DashboardView').then((module) => ({ default: module.DashboardView })));
const PosView = lazy(() => import('./features/pos/PosView').then((module) => ({ default: module.PosView })));
const SalesView = lazy(() => import('./features/sales/SalesView').then((module) => ({ default: module.SalesView })));
const PurchaseView = lazy(() => import('./features/purchase/PurchaseView').then((module) => ({ default: module.PurchaseView })));
const CatalogView = lazy(() => import('./features/catalog/CatalogView').then((module) => ({ default: module.CatalogView })));
const StockView = lazy(() => import('./features/stock/StockView').then((module) => ({ default: module.StockView })));
const CustomersView = lazy(() => import('./features/customers/CustomersView').then((module) => ({ default: module.CustomersView })));
const SuppliersView = lazy(() => import('./features/suppliers/SuppliersView').then((module) => ({ default: module.SuppliersView })));
const AccountingView = lazy(() => import('./features/accounting/AccountingView').then((module) => ({ default: module.AccountingView })));
const OrganizationView = lazy(() => import('./features/organization/OrganizationView').then((module) => ({ default: module.OrganizationView })));
const UsersView = lazy(() => import('./features/users/UsersView').then((module) => ({ default: module.UsersView })));

const VIEW_ORDER = Object.keys(VIEW_ACCESS) as NavigationItem[];

export function App() {
  const { user, isAuthenticated, isInitializing, sessionError, refreshSession, logout, hasPermission } = useAuth();
  const [currentView, setCurrentView] = useState<NavigationItem>('dashboard');

  if (isInitializing || sessionError) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50 p-6 text-center">
        <div className="max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
          {isInitializing ? <p role="status">جارٍ التحقق من الجلسة والصلاحيات...</p> : <>
            <h1 className="text-lg font-bold text-slate-900">تعذر التحقق من الجلسة</h1>
            <p role="alert" className="mt-2 text-sm text-slate-600">{sessionError}</p>
          </>}
          <div className="mt-4 flex justify-center gap-2">
            {!isInitializing && <Button onClick={refreshSession}>إعادة المحاولة</Button>}
            <Button variant="secondary" onClick={() => void logout()}>العودة لتسجيل الدخول</Button>
          </div>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <LoginView />;
  }

  const effectiveView = canOpenView(currentView, hasPermission)
    ? currentView
    : VIEW_ORDER.find((view) => canOpenView(view, hasPermission));

  if (!effectiveView) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50 p-6 text-center">
        <div className="max-w-md rounded-2xl border border-amber-200 bg-white p-6 shadow-sm">
          <h1 className="text-lg font-bold text-slate-900">لا توجد شاشات مصرح بها</h1>
          <p className="mt-2 text-sm text-slate-600">راجع مسؤول النظام لإسناد الصلاحيات المناسبة إلى حسابك.</p>
          <p className="mt-2 text-sm text-slate-500">الحساب الحالي: {user?.userName}</p>
          <div className="mt-4 flex justify-center gap-2">
            <Button onClick={refreshSession}>تحديث الصلاحيات</Button>
            <Button variant="secondary" onClick={() => void logout()}>تسجيل الخروج</Button>
          </div>
        </div>
      </div>
    );
  }

  const navigate = (view: NavigationItem) => {
    if (canOpenView(view, hasPermission)) setCurrentView(view);
  };

  const renderCurrentView = () => {
    switch (effectiveView) {
      case 'dashboard':
        return <DashboardView onNavigate={(v) => navigate(v as NavigationItem)} />;
      case 'pos':
        return <PosView />;
      case 'sales':
        return <SalesView />;
      case 'purchases':
        return <PurchaseView />;
      case 'catalog':
        return <CatalogView />;
      case 'stock':
        return <StockView />;
      case 'customers':
        return <CustomersView />;
      case 'suppliers':
        return <SuppliersView />;
      case 'accounting':
        return <AccountingView />;
      case 'branches':
        return <OrganizationView initialTab="branches" />;
      case 'settings':
        return <OrganizationView initialTab="settings" />;
      case 'users':
        return <UsersView />;
      default:
        return <DashboardView onNavigate={(v) => navigate(v as NavigationItem)} />;
    }
  };

  return (
    <Shell currentView={effectiveView} onNavigate={navigate}>
      <Suspense
        fallback={
          <div className="min-h-[50vh] flex items-center justify-center text-sm font-semibold text-slate-500">
            جارٍ تحميل الشاشة...
          </div>
        }
      >
        {renderCurrentView()}
      </Suspense>
    </Shell>
  );
}

export default App;

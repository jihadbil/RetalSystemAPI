import React, { lazy, Suspense, useState } from 'react';
import { useAuth } from './shared/context/AuthContext';
import { Shell } from './shared/components/layout/Shell';
import { NavigationItem } from './shared/components/layout/Sidebar';
import { LoginView } from './features/auth/LoginView';

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

const VIEW_PERMISSIONS: Record<NavigationItem, string> = {
  dashboard: 'Permissions.Dashboard.View',
  pos: 'Permissions.Pos.View',
  sales: 'Permissions.SalesInvoices.View',
  purchases: 'Permissions.PurchaseInvoices.View',
  catalog: 'Permissions.Products.View',
  stock: 'Permissions.Stock.View',
  customers: 'Permissions.Customers.View',
  suppliers: 'Permissions.Suppliers.View',
  accounting: 'Permissions.Safes.View',
  branches: 'Permissions.Branches.View',
  users: 'Permissions.Users.View',
  settings: 'Permissions.Tenants.View',
};

const VIEW_ORDER = Object.keys(VIEW_PERMISSIONS) as NavigationItem[];

export function App() {
  const { isAuthenticated, hasPermission } = useAuth();
  const [currentView, setCurrentView] = useState<NavigationItem>('dashboard');

  if (!isAuthenticated) {
    return <LoginView />;
  }

  const effectiveView = hasPermission(VIEW_PERMISSIONS[currentView])
    ? currentView
    : VIEW_ORDER.find((view) => hasPermission(VIEW_PERMISSIONS[view]));

  if (!effectiveView) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-slate-50 p-6 text-center">
        <div className="max-w-md rounded-2xl border border-amber-200 bg-white p-6 shadow-sm">
          <h1 className="text-lg font-bold text-slate-900">لا توجد شاشات مصرح بها</h1>
          <p className="mt-2 text-sm text-slate-600">راجع مسؤول النظام لإسناد الصلاحيات المناسبة إلى حسابك.</p>
        </div>
      </div>
    );
  }

  const navigate = (view: NavigationItem) => {
    if (hasPermission(VIEW_PERMISSIONS[view])) setCurrentView(view);
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

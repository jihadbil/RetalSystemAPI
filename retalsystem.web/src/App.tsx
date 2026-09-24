import React, { useState } from 'react';
import { useAuth } from './shared/context/AuthContext';
import { Shell } from './shared/components/layout/Shell';
import { NavigationItem } from './shared/components/layout/Sidebar';
import { LoginView } from './features/auth/LoginView';
import { DashboardView } from './features/dashboard/DashboardView';
import { PosView } from './features/pos/PosView';
import { SalesView } from './features/sales/SalesView';
import { PurchaseView } from './features/purchase/PurchaseView';
import { CatalogView } from './features/catalog/CatalogView';
import { StockView } from './features/stock/StockView';
import { CustomersView } from './features/customers/CustomersView';
import { SuppliersView } from './features/suppliers/SuppliersView';
import { AccountingView } from './features/accounting/AccountingView';
import { OrganizationView } from './features/organization/OrganizationView';
import { UsersView } from './features/users/UsersView';

export function App() {
  const { isAuthenticated } = useAuth();
  const [currentView, setCurrentView] = useState<NavigationItem>('dashboard');

  if (!isAuthenticated) {
    return <LoginView />;
  }

  const renderCurrentView = () => {
    switch (currentView) {
      case 'dashboard':
        return <DashboardView onNavigate={(v) => setCurrentView(v as NavigationItem)} />;
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
        return <DashboardView onNavigate={(v) => setCurrentView(v as NavigationItem)} />;
    }
  };

  return (
    <Shell currentView={currentView} onNavigate={setCurrentView}>
      {renderCurrentView()}
    </Shell>
  );
}

export default App;

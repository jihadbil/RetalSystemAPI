import type { NavigationItem } from '../components/layout/Sidebar';

export const VIEW_ACCESS: Record<NavigationItem, readonly string[]> = {
  dashboard: ['Permissions.Dashboard.View'], pos: ['Permissions.Pos.View'],
  sales: ['Permissions.SalesInvoices.View', 'Permissions.SalesReturns.View'],
  purchases: ['Permissions.PurchaseInvoices.View', 'Permissions.PurchaseOrders.View', 'Permissions.PurchaseReturns.View'],
  catalog: ['Permissions.Products.View', 'Permissions.Categories.View', 'Permissions.Units.View'],
  stock: ['Permissions.Stock.View', 'Permissions.Warehouses.View', 'Permissions.StockAdjustments.View', 'Permissions.StockTransfers.View'],
  customers: ['Permissions.Customers.View'], suppliers: ['Permissions.Suppliers.View'],
  accounting: ['Permissions.Safes.View', 'Permissions.Expenses.View'],
  branches: ['Permissions.Branches.View'], users: ['Permissions.Users.View', 'Permissions.Roles.View'],
  settings: ['Permissions.Tenants.View'],
};

export const canOpenView = (view: NavigationItem, hasPermission: (permission: string) => boolean) =>
  VIEW_ACCESS[view].some(hasPermission);

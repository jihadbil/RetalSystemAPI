import React from 'react';
import {
  IconDashboard,
  IconCashRegister,
  IconReceipt,
  IconShoppingCart,
  IconPackage,
  IconBuildingWarehouse,
  IconUsers,
  IconTruckDelivery,
  IconVault,
  IconBuildingStore,
  IconUserCheck,
  IconSettings,
  IconX,
} from '@tabler/icons-react';
import { useAuth } from '../../context/AuthContext';
import { canOpenView } from '../../auth/viewAccess';

export type NavigationItem =
  | 'dashboard'
  | 'pos'
  | 'sales'
  | 'purchases'
  | 'catalog'
  | 'stock'
  | 'customers'
  | 'suppliers'
  | 'accounting'
  | 'branches'
  | 'users'
  | 'settings';

interface SidebarProps {
  currentView: NavigationItem;
  onNavigate: (view: NavigationItem) => void;
  isCollapsed: boolean;
  onToggleCollapse: () => void;
  isMobileOpen?: boolean;
  onCloseMobile?: () => void;
}

interface NavGroup {
  label: string;
  items: {
    id: NavigationItem;
    label: string;
    icon: React.ReactNode;
    isPrimary?: boolean;
    badge?: string;
    permission: string;
  }[];
}

export const Sidebar: React.FC<SidebarProps> = ({
  currentView,
  onNavigate,
  isCollapsed,
  isMobileOpen = false,
  onCloseMobile,
}) => {
  const { canAccessAdmin, hasPermission } = useAuth();
  const showFullText = !isCollapsed || isMobileOpen;

  const navGroups: NavGroup[] = [
    {
      label: 'الرئيسية ونقطة البيع',
      items: [
        {
          id: 'dashboard',
          label: 'لوحة التحكم',
          icon: <IconDashboard className="w-5 h-5" />,
          permission: 'Permissions.Dashboard.View',
        },
        {
          id: 'pos',
          label: 'نقطة البيع (POS)',
          icon: <IconCashRegister className="w-5 h-5" />,
          isPrimary: true,
          badge: 'F1',
          permission: 'Permissions.Pos.View',
        },
      ],
    },
    {
      label: 'العمليات التجارية',
      items: [
        {
          id: 'sales',
          label: 'المبيعات والمرتجعات',
          icon: <IconReceipt className="w-5 h-5" />,
          permission: 'Permissions.SalesInvoices.View',
        },
        {
          id: 'purchases',
          label: 'المشتريات والموردين',
          icon: <IconShoppingCart className="w-5 h-5" />,
          permission: 'Permissions.PurchaseInvoices.View',
        },
        {
          id: 'catalog',
          label: 'دليل الأصناف والمنتجات',
          icon: <IconPackage className="w-5 h-5" />,
          permission: 'Permissions.Products.View',
        },
        {
          id: 'stock',
          label: 'المخازن والجرد',
          icon: <IconBuildingWarehouse className="w-5 h-5" />,
          permission: 'Permissions.Stock.View',
        },
      ],
    },
    {
      label: 'العملاء والمالية',
      items: [
        {
          id: 'customers',
          label: 'العملاء والذمم',
          icon: <IconUsers className="w-5 h-5" />,
          permission: 'Permissions.Customers.View',
        },
        {
          id: 'suppliers',
          label: 'الموردون والمستحقات',
          icon: <IconTruckDelivery className="w-5 h-5" />,
          permission: 'Permissions.Suppliers.View',
        },
        {
          id: 'accounting',
          label: 'الخزائن والمصروفات',
          icon: <IconVault className="w-5 h-5" />,
          permission: 'Permissions.Safes.View',
        },
      ],
    },
    ...(canAccessAdmin
      ? [
          {
            label: 'الإدارة والنظام',
            items: [
              {
                id: 'branches' as NavigationItem,
                label: 'الفروع والمستودعات',
                icon: <IconBuildingStore className="w-5 h-5" />,
                permission: 'Permissions.Branches.View',
              },
              {
                id: 'users' as NavigationItem,
                label: 'المستخدمين والصلاحيات',
                icon: <IconUserCheck className="w-5 h-5" />,
                permission: 'Permissions.Users.View',
              },
              {
                id: 'settings' as NavigationItem,
                label: 'إعدادات المنشأة',
                icon: <IconSettings className="w-5 h-5" />,
                permission: 'Permissions.Tenants.View',
              },
            ],
          },
        ]
      : []),
  ];

  const visibleGroups = navGroups
    .map((group) => ({ ...group, items: group.items.filter((item) => canOpenView(item.id, hasPermission)) }))
    .filter((group) => group.items.length > 0);

  return (
    <aside
      className={`fixed top-0 right-0 z-50 h-screen bg-white border-l border-slate-200/80 flex flex-col transition-all duration-300 ease-in-out
        ${isMobileOpen ? 'translate-x-0 shadow-2xl' : 'translate-x-full lg:translate-x-0'}
        ${isCollapsed ? 'w-72 sm:w-64 lg:w-18' : 'w-72 sm:w-64'}
      `}
    >
      {/* شعار النظام مع زر الإغلاق للشاشات الصغيرة */}
      <div className="h-16 flex items-center justify-between px-4 border-b border-slate-100 gap-3 shrink-0">
        <div className="flex items-center gap-3 min-w-0">
          <div className="w-10 h-10 rounded-xl bg-gradient-to-tr from-emerald-700 to-emerald-500 flex items-center justify-center text-white font-black text-xl shadow-xs shrink-0">
            R
          </div>
          {showFullText && (
            <div className="flex flex-col min-w-0">
              <span className="font-extrabold text-base text-slate-900 leading-tight tracking-tight truncate">
                ريتـــال <span className="text-emerald-600 font-semibold text-xs">PRO</span>
              </span>
              <span className="text-[11px] text-slate-400 font-medium truncate">نظام المبيعات السحابي</span>
            </div>
          )}
        </div>

        {/* زر إغلاق القائمة على الجوال */}
        <button
          type="button"
          onClick={onCloseMobile}
          className="lg:hidden p-1.5 text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg transition-colors cursor-pointer"
          aria-label="إغلاق القائمة"
        >
          <IconX className="w-5 h-5" />
        </button>
      </div>

      {/* عناصر القائمة الجانبية */}
      <div className="flex-1 overflow-y-auto px-2.5 py-4 flex flex-col gap-5">
        {visibleGroups.map((group, gIdx) => (
          <div key={gIdx} className="flex flex-col gap-1">
            {showFullText && (
              <span className="px-3 text-[11px] font-bold text-slate-400 uppercase tracking-wider mb-1">
                {group.label}
              </span>
            )}
            {group.items.map((item) => {
              const isActive = currentView === item.id;
              return (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => {
                    onNavigate(item.id);
                    onCloseMobile?.();
                  }}
                  title={!showFullText ? item.label : undefined}
                  className={`flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-semibold transition-all duration-150 cursor-pointer ${
                    isActive
                      ? item.isPrimary
                        ? 'bg-emerald-600 text-white shadow-xs'
                        : 'bg-emerald-50 text-emerald-800 border border-emerald-200/60'
                      : item.isPrimary
                      ? 'bg-emerald-50/70 hover:bg-emerald-100 text-emerald-800'
                      : 'text-slate-600 hover:text-slate-900 hover:bg-slate-50'
                  } ${!showFullText ? 'lg:justify-center lg:px-0' : ''}`}
                >
                  <span className={`${isActive ? (item.isPrimary ? 'text-white' : 'text-emerald-600') : 'text-slate-500'}`}>
                    {item.icon}
                  </span>
                  {showFullText && <span className="flex-1 text-right truncate">{item.label}</span>}
                  {showFullText && item.badge && (
                    <span
                      className={`text-[10px] font-mono font-bold px-1.5 py-0.5 rounded-md ${
                        isActive ? 'bg-emerald-700 text-white' : 'bg-slate-200 text-slate-600'
                      }`}
                    >
                      {item.badge}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        ))}
      </div>

      {/* تذييل القائمة الجانبية */}
      {showFullText && (
        <div className="p-3 border-t border-slate-100 bg-slate-50/50 flex items-center justify-between text-xs text-slate-500">
          <span>النسخة 2.4 (الويب)</span>
          <span className="text-[11px] text-emerald-600 font-semibold">متصل بالخادم</span>
        </div>
      )}
    </aside>
  );
};

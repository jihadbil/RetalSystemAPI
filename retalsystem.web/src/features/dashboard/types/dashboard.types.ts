// أنواع ونماذج لوحة التحكم والتحليلات الحية

export interface DashboardKpi {
  todaySales: number;
  yesterdaySales: number;
  salesGrowthPercentage: number;
  todayOrdersCount: number;
  averageOrderValue: number;
  todayProfit: number;
  todayPurchases: number;
  thisMonthSales: number;
  thisMonthPurchases: number;
  totalReceivables: number;
  totalPayables: number;
}

export interface DashboardCounts {
  productsCount: number;
  categoriesCount: number;
  branchesCount: number;
  warehousesCount: number;
  customersCount: number;
  suppliersCount: number;
  tenantsCount: number;
}

export interface DailySalesPoint {
  date: string;
  dayName: string;
  salesAmount: number;
  profitAmount: number;
  ordersCount: number;
  barHeightPercentage?: number;
  formattedSales?: string;
}

export interface TopSellingProduct {
  productId: string;
  productName: string;
  categoryName: string;
  quantitySold: number;
  totalSales: number;
  totalProfit: number;
  percentageOfTotalSales: number;
}

export interface LowStockItem {
  productId: string;
  productName: string;
  categoryName: string;
  warehouseName: string;
  currentQuantity: number;
  minStockLevel: number;
  costPrice: number;
  salePrice: number;
  status: 'نفد' | 'حرج' | 'منخفض';
  statusBadgeColor?: string;
}

export interface RecentInvoice {
  id: string;
  invoiceNumber: string;
  invoiceDate: string;
  customerName: string;
  branchName: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentMethod: string;
  status: string;
}

export interface DashboardSummary {
  kpis: DashboardKpi;
  counts: DashboardCounts;
  dailySalesTrend: DailySalesPoint[];
  topSellingProducts: TopSellingProduct[];
  lowStockAlerts: LowStockItem[];
  recentInvoices: RecentInvoice[];
}

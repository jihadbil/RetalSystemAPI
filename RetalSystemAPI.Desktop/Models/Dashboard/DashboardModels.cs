using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Dashboard;

public class DashboardSummaryDto
{
    public DashboardKpiDto Kpis { get; set; } = new();
    public DashboardCountsDto Counts { get; set; } = new();
    public List<DailySalesPointDto> DailySalesTrend { get; set; } = new();
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();
    public List<LowStockItemDto> LowStockAlerts { get; set; } = new();
    public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
}

public class DashboardKpiDto
{
    public decimal TodaySales { get; set; }
    public decimal YesterdaySales { get; set; }
    public decimal SalesGrowthPercentage { get; set; }
    public int TodayOrdersCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal TodayProfit { get; set; }
    public decimal TodayPurchases { get; set; }
    public decimal ThisMonthSales { get; set; }
    public decimal ThisMonthPurchases { get; set; }
    public decimal TotalReceivables { get; set; }
    public decimal TotalPayables { get; set; }
}

public class DashboardCountsDto
{
    public int ProductsCount { get; set; }
    public int CategoriesCount { get; set; }
    public int BranchesCount { get; set; }
    public int WarehousesCount { get; set; }
    public int CustomersCount { get; set; }
    public int SuppliersCount { get; set; }
    public int TenantsCount { get; set; }
}

public class DailySalesPointDto
{
    public DateTime Date { get; set; }
    public string DayName { get; set; } = string.Empty;
    public decimal SalesAmount { get; set; }
    public decimal ProfitAmount { get; set; }
    public int OrdersCount { get; set; }

    // حقول مساعدة للرسم البياني في الـ UI
    public double BarHeightPercentage { get; set; }
    public string FormattedSales => $"{SalesAmount:N0} د.ل";
}

public class TopSellingProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal PercentageOfTotalSales { get; set; }

    public string FormattedTotalSales => $"{TotalSales:N2} د.ل";
    public string FormattedQuantity => $"{QuantitySold} قطعة";
}

public class LowStockItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public string Status { get; set; } = "منخفض"; // "نفد" | "حرج" | "منخفض"

    public string StatusBadgeColor => Status switch
    {
        "نفد" => "#EF4444",
        "حرج" => "#F59E0B",
        _ => "#EAB308"
    };
}

public class RecentInvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string FormattedAmount => $"{TotalAmount:N2} د.ل";
    public string FormattedTime => InvoiceDate.ToString("hh:mm tt");
}

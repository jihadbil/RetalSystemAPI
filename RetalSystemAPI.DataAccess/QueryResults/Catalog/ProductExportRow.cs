namespace RetalSystemAPI.DataAccess.QueryResults;
public sealed record ProductExportRow(Guid Id, string Name, decimal CostPrice, decimal SalePrice, Guid CategoryId, string? Description);

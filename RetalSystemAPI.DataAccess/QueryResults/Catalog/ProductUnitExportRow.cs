namespace RetalSystemAPI.DataAccess.QueryResults;
public sealed record ProductUnitExportRow(Guid ProductId, Guid UnitId, int ConversionFactor, bool IsDefault);

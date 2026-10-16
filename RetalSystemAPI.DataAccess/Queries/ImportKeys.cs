namespace RetalSystemAPI.DataAccess.Queries;
public sealed record ImportKeys(IReadOnlyCollection<Guid> Ids, IReadOnlyCollection<string> Values);

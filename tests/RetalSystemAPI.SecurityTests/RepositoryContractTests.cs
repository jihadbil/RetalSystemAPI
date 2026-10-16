using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.DataAccess.Extensions;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using Xunit;

namespace RetalSystemAPI.SecurityTests;
public sealed class RepositoryContractTests
{
    [Fact]
    public async Task Dependency_injection_exposes_specialized_repositories_on_the_same_scoped_context()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataAccess(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["ConnectionStrings:DefaultConnection"] = "Server=unused;Database=Metadata;Integrated Security=True" }).Build());
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        Assert.Null(scope.ServiceProvider.GetService<IRepository<Product>>());
        var category = new Category
        {
            Name = "staged"
        };
        await scope.ServiceProvider.GetRequiredService<ICategoryRepository>().AddAsync(category);
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        Assert.Contains(uow.ChangeTrackerEntries(), entry => ReferenceEquals(entry.Entity, category));
        Assert.IsType<ProductRepository>(scope.ServiceProvider.GetRequiredService<IProductRepository>());
    }

    [Fact]
    public void Generic_contract_contains_only_seven_basic_operations()
    {
        var names = typeof(IRepository<>).GetMethods().Select(m => m.Name).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { "AddAsync", "AddRangeAsync", "ExistsByIdAsync", "GetByIdAsync", "GetByIdForUpdateAsync", "SoftDelete", "Update" }, names);
        Assert.True(typeof(Repository<>).IsAbstract);
    }

    [Fact]
    public void Specialized_contracts_do_not_expose_query_engines_or_open_projections()
    {
        var interfaces = typeof(IRepository<>).Assembly.GetTypes().Where(t => t.IsInterface && t.Name.EndsWith("Repository"));
        foreach (var contract in interfaces)
            foreach (var method in contract.GetMethods())
            {
                Assert.False(method.IsGenericMethod, $"Open projection in {contract.Name}.{method.Name}");
                Assert.DoesNotContain(method.GetParameters(), p => IsQueryEngine(p.ParameterType));
                Assert.False(IsQueryEngine(method.ReturnType));
            }

        Assert.DoesNotContain(typeof(RetalSystemAPI.Services.Catalog.Implementations.ProductService).Assembly.GetTypes(), t => t.Namespace?.Contains("Specifications") == true || t.BaseType?.Name.StartsWith("BaseSpecification") == true);
    }

    [Fact]
    public async Task Read_only_and_update_reads_have_distinct_tracking_contracts()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        var category = new Category
        {
            Name = "category"
        };
        db.Add(category);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var repo = new CategoryRepository(db);
        var readOnly = await repo.GetByIdAsync(category.Id);
        Assert.NotNull(readOnly);
        Assert.Empty(db.ChangeTracker.Entries());
        var tracked = await repo.GetByIdForUpdateAsync(category.Id);
        Assert.NotNull(tracked);
        Assert.Equal(EntityState.Unchanged, db.Entry(tracked).State);
        tracked.Name = "saved";
        await db.SaveChangesAsync();
        Assert.Equal("saved", (await repo.GetByIdAsync(category.Id))!.Name);
        repo.SoftDelete(tracked);
        Assert.Null(await repo.GetByIdAsync(category.Id));
        Assert.Null(await repo.GetByIdForUpdateAsync(category.Id));
    }

    [Fact]
    public async Task Import_candidates_are_scoped_and_complete_across_batches()
    {
        await using var db = ModelIntegrityTests.CreateContext(Guid.NewGuid());
        var category = new Category
        {
            Name = "category"
        };
        var candidates = Enumerable.Range(0, 1005).Select(i => new Product { Name = $"P{i}", Category = category }).ToArray();
        var unrelated = new Product
        {
            Name = "unrelated",
            Category = category
        };
        db.AddRange(candidates);
        db.Add(unrelated);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var rows = await new ProductRepository(db).GetImportCandidatesForUpdateAsync(new ImportKeys([], candidates.Select(p => p.Name.ToLowerInvariant()).ToArray()));
        Assert.Equal(candidates.Length, rows.Count);
        Assert.DoesNotContain(rows, p => p.Id == unrelated.Id);
        Assert.Equal(candidates.Length, db.ChangeTracker.Entries<Product>().Count());
    }

    private static bool IsQueryEngine(Type type) => typeof(LambdaExpression).IsAssignableFrom(type) || typeof(IQueryable).IsAssignableFrom(type) || type.Name.StartsWith("ISpecification") || type.Name.StartsWith("Expression`") || (type.IsGenericType && type.GetGenericArguments().Any(IsQueryEngine));
}

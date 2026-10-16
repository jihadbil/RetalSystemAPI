using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using RetalSystemAPI.DataAccess.Context;
using Xunit;

namespace RetalSystemAPI.SecurityTests;

[CollectionDefinition("Design time configuration", DisableParallelization = true)]
public sealed class DesignTimeConfigurationCollection { }

[Collection("Design time configuration")]
public sealed class DesignDbContextFactoryTests
{
    [Fact]
    public void Reads_environment_connection_and_allows_explicit_command_line_override()
    {
        const string key = "ConnectionStrings__DefaultConnection";
        var previous = Environment.GetEnvironmentVariable(key);
        try
        {
            const string environmentConnection = "Server=unused;Database=Environment;Integrated Security=True";
            const string argumentConnection = "Server=unused;Database=Argument;Integrated Security=True";
            Environment.SetEnvironmentVariable(key, environmentConnection);
            var factory = new DesignDbContextFactory();
            using var environment = factory.CreateDbContext(["--environment", "Production"]);
            Assert.Equal("Environment", new SqlConnectionStringBuilder(environment.Database.GetConnectionString()).InitialCatalog);
            using var argument = factory.CreateDbContext(["--environment", "Production", "--ConnectionStrings:DefaultConnection", argumentConnection]);
            Assert.Equal("Argument", new SqlConnectionStringBuilder(argument.Database.GetConnectionString()).InitialCatalog);
        }
        finally
        {
            Environment.SetEnvironmentVariable(key, previous);
        }
    }

    [Fact]
    public void Rejects_empty_connection_instead_of_targeting_a_default_database()
    {
        var error = Assert.Throws<InvalidOperationException>(() => new DesignDbContextFactory()
            .CreateDbContext(["--environment", "Production", "--ConnectionStrings:DefaultConnection", ""]));
        Assert.Contains("DefaultConnection", error.Message);
    }
}

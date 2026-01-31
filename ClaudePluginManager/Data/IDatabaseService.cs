using System.Data;
using System.Threading.Tasks;

namespace ClaudePluginManager.Data;

public interface IDatabaseService
{
    string DatabasePath { get; }
    IDbConnection CreateConnection();
    Task InitializeAsync();
    Task<int> GetSchemaVersionAsync();
}

using System.Threading;
using System.Threading.Tasks;

namespace ClaudePluginManager.Services;

public record ProcessResult(int ExitCode, string Output, string Error)
{
    public bool Success => ExitCode == 0;
}

public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(string command, params string[] arguments);
    Task<ProcessResult> RunAsync(string command, string[] arguments, CancellationToken cancellationToken);
}

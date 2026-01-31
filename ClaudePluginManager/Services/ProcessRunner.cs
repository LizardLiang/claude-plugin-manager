using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClaudePluginManager.Services;

public class ProcessRunner : IProcessRunner
{
    public Task<ProcessResult> RunAsync(string command, params string[] arguments)
    {
        return RunAsync(command, arguments, CancellationToken.None);
    }

    public async Task<ProcessResult> RunAsync(string command, string[] arguments, CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = string.Join(" ", arguments.Select(EscapeArgument)),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var output = await outputTask;
            var error = await errorTask;

            return new ProcessResult(process.ExitCode, output, error);
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or FileNotFoundException)
        {
            // Command not found
            return new ProcessResult(-1, "", $"{command}: command not found");
        }
    }

    private static string EscapeArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg))
            return "\"\"";

        if (!arg.Contains(' ') && !arg.Contains('"') && !arg.Contains('\''))
            return arg;

        // Escape double quotes and wrap in double quotes
        return $"\"{arg.Replace("\"", "\\\"")}\"";
    }
}

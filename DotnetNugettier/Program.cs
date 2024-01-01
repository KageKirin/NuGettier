using System;
using System.IO;

#nullable enable

namespace DotnetNugettier;

public static partial class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            CancellationTokenSource cancellationTokenSource = new();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            using var process = new System.Diagnostics.Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.FileName = @"nugettier";
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.StartInfo.Arguments = string.Join(" ", args);

            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            var exitCode = process.ExitCode;
            var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

            if (!string.IsNullOrEmpty(stdout))
                Console.WriteLine(stdout);

            if (!string.IsNullOrEmpty(stderr))
                Console.Error.WriteLine(stderr);

            return exitCode;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }
    }
}

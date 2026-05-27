using System.Diagnostics;
using NUnit.Framework;

namespace E2ETests;

/// <summary>
/// One-time setup fixture that starts the Mnemi web app before any E2E tests run
/// and stops it when all tests are complete.
///
/// This allows the tests to run from Visual Studio Test Explorer without manually
/// starting the app first.
/// </summary>
[SetUpFixture]
public sealed class AppTestFixture
{
    private static Process? _serverProcess;

    public const string BaseUrl = "http://localhost:5071";
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(30);

    private static string GetProjectPath() =>
        Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..", "..", "src", "App", "Ui.Web", "Ui.Web.csproj"));

    [OneTimeSetUp]
    public async Task StartServerAsync()
    {
        // Check if the server is already running
        if (await IsServerRunningAsync())
        {
            TestContext.Progress.WriteLine("Mnemi server already running on {0}", BaseUrl);
            return;
        }

        TestContext.Progress.WriteLine("Starting Mnemi server...");

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{GetProjectPath()}\" --urls \"{BaseUrl}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Environment =
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development",
                ["E2E_TEST_AUTH_BYPASS"] = "true"
            }
        };

        _serverProcess = new Process { StartInfo = startInfo };

        // Capture output to detect when the server is ready
        var tcs = new TaskCompletionSource<bool>();
        _serverProcess.OutputDataReceived += (_, e) =>
        {
            if (e.Data?.Contains("Now listening on") == true)
                tcs.TrySetResult(true);
        };
        _serverProcess.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                TestContext.Progress.WriteLine("[server] {0}", e.Data);
        };

        _serverProcess.Start();
        _serverProcess.BeginOutputReadLine();
        _serverProcess.BeginErrorReadLine();

        // Wait for server to be ready, with timeout
        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(StartupTimeout));
        if (completedTask != tcs.Task)
        {
            TestContext.Error.WriteLine(
                "Server did not start within {0} seconds. Check that the project builds and port {1} is available.",
                StartupTimeout.TotalSeconds, BaseUrl);
            throw new InvalidOperationException($"Server failed to start on {BaseUrl}");
        }

        // Give the server a moment to fully initialize
        await Task.Delay(2000);

        TestContext.Progress.WriteLine("Mnemi server started on {0}", BaseUrl);
    }

    [OneTimeTearDown]
    public void StopServer()
    {
        if (_serverProcess is { HasExited: false })
        {
            TestContext.Progress.WriteLine("Stopping Mnemi server...");

            // Try graceful shutdown first
            if (!_serverProcess.CloseMainWindow())
            {
                // Fall back to kill if CloseMainWindow isn't supported
                _serverProcess.Kill(entireProcessTree: true);
            }

            if (!_serverProcess.WaitForExit(5000))
            {
                _serverProcess.Kill(entireProcessTree: true);
                _serverProcess.WaitForExit(3000);
            }

            _serverProcess.Dispose();
            _serverProcess = null;
            TestContext.Progress.WriteLine("Mnemi server stopped.");
        }
    }

    private static async Task<bool> IsServerRunningAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"{BaseUrl}/");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

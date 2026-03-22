namespace LivestreamApp.Tests.Integration.Infrastructure;

/// <summary>
/// Skips the test if Docker is not available on the current machine.
/// Allows integration tests to run in CI (with Docker) but skip gracefully in local dev without Docker.
/// </summary>
public sealed class DockerAvailableFactAttribute : FactAttribute
{
    public DockerAvailableFactAttribute()
    {
        if (!IsDockerAvailable())
            Skip = "Docker is not available. Skipping integration test.";
    }

    private static bool IsDockerAvailable()
    {
        // Try common docker locations — handles cases where docker is not in PATH
        var candidates = new[]
        {
            "docker",
            @"C:\Program Files\Docker\Docker\resources\bin\docker.exe",
            "/usr/bin/docker",
            "/usr/local/bin/docker"
        };

        foreach (var candidate in candidates)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = candidate,
                        Arguments = "ps",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit(5000);
                if (process.ExitCode == 0) return true;
            }
            catch { /* try next */ }
        }

        return false;
    }
}

using Velopack;
using Velopack.Sources;

namespace Checkers.Services;

public static class UpdateService
{
    private const string RepoUrl = "https://github.com/goddamnn69/Checkers";

    public static async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        var mgr = new UpdateManager(new GithubSource(RepoUrl, null, false));
        return await mgr.CheckForUpdatesAsync();
    }

    public static async Task DownloadAndApplyAsync(UpdateInfo update)
    {
        var mgr = new UpdateManager(new GithubSource(RepoUrl, null, false));
        await mgr.DownloadUpdatesAsync(update);
        mgr.ApplyUpdatesAndRestart(update);
    }
}

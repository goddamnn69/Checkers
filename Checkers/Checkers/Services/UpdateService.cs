using Velopack;
using Velopack.Sources;

namespace Checkers.Services;

public static class UpdateService
{
    // TODO: Replace with your actual GitHub repository URL after creating the repo
    private const string RepoUrl = "https://github.com/USERNAME/Checkers";

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

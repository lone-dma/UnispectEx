using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Velopack;
using Velopack.Sources;

namespace UnispectEx
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            VelopackApp.Build().Run();
            var updater = new UpdateManager(
                source: new GithubSource(
                    repoUrl: "https://github.com/lone-dma/UnispectEx",
                    accessToken: null,
                    prerelease: false));
            if (updater.IsInstalled) // Use the persistent data directory (one level up from 'current') when installed
            {
                Directory.SetCurrentDirectory("..");
            }
            _ = Task.Run(() => CheckForUpdatesAsync(updater)); // Run continuations on the thread pool.
        }

        private static async Task CheckForUpdatesAsync(UpdateManager updater)
        {
            try
            {
                if (!updater.IsInstalled)
                    return;

                var newVersion = await updater.CheckForUpdatesAsync();
                if (newVersion is not null)
                {
                    var result = MessageBox.Show(
                        messageBoxText: $"A new version ({newVersion.TargetFullRelease.Version}) is available.\n\nWould you like to update now?",
                        caption: "UnispectEx",
                        button: MessageBoxButton.YesNo,
                        icon: MessageBoxImage.Question,
                        defaultResult: MessageBoxResult.Yes,
                        options: MessageBoxOptions.DefaultDesktopOnly);

                    if (result == MessageBoxResult.Yes)
                    {
                        await updater.DownloadUpdatesAsync(newVersion);
                        updater.ApplyUpdatesAndRestart(newVersion);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    messageBoxText: $"An unhandled exception occurred while checking for updates: {ex}",
                    caption: "UnispectEx",
                    button: MessageBoxButton.OK,
                    icon: MessageBoxImage.Warning,
                    defaultResult: MessageBoxResult.OK,
                    options: MessageBoxOptions.DefaultDesktopOnly);
            }
        }
    }
}

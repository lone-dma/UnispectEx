using System.IO;
using System.Windows;
using Velopack;

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
            var updater = new UpdateManager(".");
            if (updater.IsInstalled) // Use the persistent data directory (one level up from 'current') when installed
            {
                Directory.SetCurrentDirectory("..");
            }
        }
    }
}

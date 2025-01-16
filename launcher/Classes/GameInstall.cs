﻿using System.IO;

using static launcher.Logger;
using static launcher.ControlReferences;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace launcher
{
    /// <summary>
    /// The GameInstall class handles the installation process of a game.
    /// It includes methods to start the installation, download necessary files,
    /// decompress them, and repair any corrupted files if detected.
    ///
    /// The Start method performs the following steps:
    /// 1. Sets the installation state to "INSTALLING".
    /// 2. Creates a temporary directory to store downloaded files.
    /// 3. Fetches the list of base game files.
    /// 4. Prepares download tasks for the base game files.
    /// 5. Downloads the base game files.
    /// 6. Prepares decompression tasks for the downloaded files.
    /// 7. Decompresses the downloaded files.
    /// 8. If any bad files are detected, attempts to repair the game files.
    /// 9. Updates or creates the launcher configuration.
    /// 10. Sets the installation state to false, indicating the installation is complete.
    /// 11. Marks the game as installed.
    /// 12. Cleans up the temporary directory used for downloading files.
    ///
    /// The AttemptGameRepair method tries to repair the game files if any bad files are detected.
    /// It makes multiple attempts (up to a maximum defined by Global.MAX_REPAIR_ATTEMPTS) to repair the files.
    /// </summary>
    public class GameInstall
    {
        public static async void Start()
        {
            if (!AppState.IsOnline)
                return;

            if (Utilities.GetCurrentBranch().is_local_branch)
                return;

            //Install started
            DownloadManager.SetInstallState(true, "INSTALLING");

            //Set download limits
            DownloadManager.ConfigureConcurrency();
            DownloadManager.ConfigureDownloadSpeed();

            //Create branch library directory to store downloaded files
            string branchDirectory = FileManager.GetBranchDirectory();

            //Fetch compressed base game file list
            DownloadManager.UpdateStatusLabel("Fetching game files list", Source.Installer);
            BaseGameFiles baseGameFiles = await DataFetcher.FetchBaseGameFiles(true);

            //Prepare download tasks
            DownloadManager.UpdateStatusLabel("Preparing game download", Source.Installer);
            var downloadTasks = DownloadManager.InitializeDownloadTasks(baseGameFiles, branchDirectory);

            //Download base game files
            DownloadManager.UpdateStatusLabel("Downloading game files", Source.Installer);
            await Task.WhenAll(downloadTasks);

            //Prepare decompression tasks
            DownloadManager.UpdateStatusLabel("Preparing game decompression", Source.Installer);
            var decompressionTasks = DecompressionManager.PrepareTasks(downloadTasks);

            //Decompress base game files
            DownloadManager.UpdateStatusLabel("Decompressing game files", Source.Installer);
            await Task.WhenAll(decompressionTasks);

            //if bad files detected, attempt game repair
            if (AppState.BadFilesDetected)
            {
                DownloadManager.UpdateStatusLabel("Reparing game files", Source.Installer);
                await AttemptGameRepair();
            }

            //Install finished
            DownloadManager.SetInstallState(false);

            string branch = Utilities.GetCurrentBranch().branch;

            //Set branch as installed
            Ini.Set(branch, "Is_Installed", true);
            Ini.Set(branch, "Version", Utilities.GetCurrentBranch().currentVersion);

            Utilities.SetupAdvancedMenu();
            Utilities.SendNotification($"R5Reloaded ({Utilities.GetCurrentBranch().branch}) has been installed!", BalloonIcon.Info);

            MessageBoxResult result = MessageBox.Show("The game installation is complete.Would you like to install the HD Textures? you can always choose to install them at another time, they are not required to play.", "Install HD Textures", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
                Ini.Set(branch, "Download_HD_Textures", true);
            else
                Ini.Set(branch, "Download_HD_Textures", false);

            //Install optional files if HD textures are enabled
            if (Ini.Get(branch, "Download_HD_Textures", false))
                Task.Run(() => InstallOptionalFiles());
        }

        private static async Task InstallOptionalFiles()
        {
            DownloadManager.SetOptionalInstallState(true);

            //Set download limits
            DownloadManager.ConfigureConcurrency();
            DownloadManager.ConfigureDownloadSpeed();

            //Create branch library directory to store downloaded files
            string branchDirectory = FileManager.GetBranchDirectory();

            //Fetch compressed base game file list
            DownloadManager.UpdateStatusLabel("Fetching optional files list", Source.Installer);
            BaseGameFiles optionalGameFiles = await DataFetcher.FetchOptionalGameFiles(true);

            //Prepare download tasks
            DownloadManager.UpdateStatusLabel("Preparing optional download", Source.Installer);
            var optionaldownloadTasks = DownloadManager.InitializeDownloadTasks(optionalGameFiles, branchDirectory);

            //Download base game files
            DownloadManager.UpdateStatusLabel("Downloading optional files", Source.Installer);
            await Task.WhenAll(optionaldownloadTasks);

            //Prepare decompression tasks
            DownloadManager.UpdateStatusLabel("Preparing decompression", Source.Installer);
            var decompressionTasks = DecompressionManager.PrepareTasks(optionaldownloadTasks);

            //Decompress base game files
            DownloadManager.UpdateStatusLabel("Decompressing optional files", Source.Installer);
            await Task.WhenAll(decompressionTasks);

            DownloadManager.SetOptionalInstallState(false);

            Utilities.SendNotification($"R5Reloaded ({Utilities.GetCurrentBranch().branch}) optional files have been installed!", BalloonIcon.Info);
        }

        private static async Task AttemptGameRepair()
        {
            bool isRepaired = false;

            for (int i = 0; i < Constants.Launcher.MAX_REPAIR_ATTEMPTS; i++)
            {
                isRepaired = await GameRepair.Start();
                if (isRepaired) break;
            }

            AppState.BadFilesDetected = !isRepaired;
        }
    }
}
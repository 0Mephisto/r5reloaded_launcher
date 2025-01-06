﻿using System.Net.Http;
using System.Windows.Controls;

namespace launcher
{
    /// <summary>
    /// The Global class contains static fields and constants that are used throughout the launcher application.
    /// It includes configuration settings, HTTP client setup, and various flags and counters to manage the state of the application.
    ///
    /// Fields and Constants:
    /// - launcherVersion: The current version of the launcher.
    /// - serverConfig: Configuration settings for the server (nullable).
    /// - launcherConfig: Configuration settings for the launcher (nullable).
    /// - client: An instance of HttpClient with a timeout of 30 seconds, used for making HTTP requests.
    /// - launcherPath: The file path where the launcher is located.
    /// - MAX_REPAIR_ATTEMPTS: The maximum number of attempts to repair the launcher.
    /// - filesLeft: The number of files left to process.
    /// - isInstalling: A flag indicating if the installation process is ongoing.
    /// - isInstalled: A flag indicating if the launcher is installed.
    /// - updateRequired: A flag indicating if an update is required.
    /// - updateCheckLoop: A flag indicating if the update check loop is active.
    /// - badFilesDetected: A flag indicating if any bad files have been detected.
    /// - downloadSemaphore: A semaphore to limit the number of concurrent downloads to 100.
    /// - badFiles: A list of bad files detected during the process.
    /// </summary>
    public static class Global
    {
        public const string launcherVersion = "0.3.0";

        public static ServerConfig? serverConfig;
        public static LauncherConfig? launcherConfig;

        public static HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        public static string launcherPath = "";
        public const int MAX_REPAIR_ATTEMPTS = 5;
        public static int filesLeft = 0;
        public static bool isInstalling = false;
        public static bool isInstalled = false;
        public static bool updateRequired = false;
        public static bool updateCheckLoop = false;
        public static bool badFilesDetected = false;

        public static SemaphoreSlim downloadSemaphore = new SemaphoreSlim(100);
        public static List<string> badFiles = new List<string>();
    }
}
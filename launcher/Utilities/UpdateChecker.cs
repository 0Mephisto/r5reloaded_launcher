﻿using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using static launcher.Utilities.Logger;
using System.IO;
using static launcher.Global.References;
using launcher.Global;
using launcher.Managers;
using launcher.BranchUtils;
using launcher.Game;

namespace launcher.Utilities
{
    public static class UpdateChecker
    {
        private static bool iqnoredLauncherUpdate = false;

        public static async Task Start()
        {
            if (!AppState.IsOnline)
                return;

            if (string.IsNullOrEmpty((string)Ini.Get(Ini.Vars.Launcher_Version)) && (string)Ini.Get(Ini.Vars.Launcher_Version) == Launcher.VERSION)
            {
                Ini.Set(Ini.Vars.Launcher_Version, Launcher.VERSION);
            }

            LogInfo(Source.UpdateChecker, "Update worker started");

            while (true)
            {
                LogInfo(Source.UpdateChecker, "Checking for updates");

                try
                {
                    var newServerConfig = await GetServerConfigAsync();
                    var newGithubConfig = await GetGithubConfigAsync();
                    if (newServerConfig == null)
                    {
                        LogError(Source.UpdateChecker, "Failed to fetch new server config");
                        continue;
                    }

                    if (ShouldUpdateLauncher(newServerConfig, newGithubConfig))
                    {
                        HandleLauncherUpdate();
                    }
                    else
                    {
                        LogInfo(Source.UpdateChecker, $"Update for launcher is not available (latest version: {newServerConfig.launcherVersion})");
                    }

                    if (ShouldUpdateGame(newServerConfig))
                    {
                        HandleGameUpdate();
                    }
                }
                catch (HttpRequestException ex)
                {
                    LogError(Source.UpdateChecker, $"HTTP Request Failed: {ex.Message}");
                }
                catch (JsonSerializationException ex)
                {
                    LogError(Source.UpdateChecker, $"JSON Deserialization Failed: {ex.Message}");
                }
                catch (Exception ex)
                {
                    LogError(Source.UpdateChecker, $"Unexpected Error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        private static async Task<ServerConfig> GetServerConfigAsync()
        {
            HttpResponseMessage response = null;
            try
            {
                response = await Networking.HttpClient.GetAsync(Launcher.CONFIG_URL);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ServerConfig>(responseString);
            }
            catch (HttpRequestException ex)
            {
                LogError(Source.UpdateChecker, $"HTTP Request Failed: {ex.Message}");
                return null;
            }
            finally
            {
                response?.Dispose();
            }
        }

        private static async Task<List<GithubItems>> GetGithubConfigAsync()
        {
            HttpResponseMessage response = null;
            try
            {
                Networking.HttpClient.DefaultRequestHeaders.Add("User-Agent", "request");
                response = await Networking.HttpClient.GetAsync(Launcher.GITHUB_API_URL);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();

                Networking.HttpClient.DefaultRequestHeaders.Remove("User-Agent");

                return JsonConvert.DeserializeObject<List<GithubItems>>(responseString);
            }
            catch (HttpRequestException ex)
            {
                LogError(Source.UpdateChecker, $"HTTP Request Failed: {ex.Message}");
                return null;
            }
            finally
            {
                response?.Dispose();
            }
        }

        private static bool IsNewVersion(string version, string newVersion)
        {
            if (((string)Ini.Get(Ini.Vars.Launcher_Version)).Contains("nightly"))
            {
                return true;
            }

            var currentParts = version.Split('.').Select(int.Parse).ToArray();
            var newParts = newVersion.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Max(currentParts.Length, newParts.Length); i++)
            {
                int currentPart = i < currentParts.Length ? currentParts[i] : 0;
                int newPart = i < newParts.Length ? newParts[i] : 0;

                if (currentPart < newPart)
                    return true;
                if (currentPart > newPart)
                    return false;
            }

            return false;
        }

        private static bool IsNewNightlyVersion(string version, List<GithubItems> newGithubConfig)
        {
            return GetLatestNightlyTag(newGithubConfig) != version;
        }

        private static string GetLatestNightlyTag(List<GithubItems> newGithubConfig)
        {
            string latest = "";
            foreach (var root in newGithubConfig)
            {
                if (root.prerelease && root.tag_name.Contains("nightly"))
                {
                    latest = root.tag_name;
                }
            }

            return latest;
        }

        private static bool ShouldUpdateLauncher(ServerConfig newServerConfig, List<GithubItems> newGithubConfig)
        {
            if ((bool)Ini.Get(Ini.Vars.Nightly_Builds))
            {
                if (!iqnoredLauncherUpdate && !AppState.IsInstalling && IsNewNightlyVersion((string)Ini.Get(Ini.Vars.Launcher_Version), newGithubConfig))
                {
                    var messageBoxResult = MessageBox.Show("A new nightly version of the launcher is available. Would you like to update now?", "Launcher Update", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (messageBoxResult == MessageBoxResult.No)
                    {
                        iqnoredLauncherUpdate = true;
                        return false;
                    }

                    Ini.Set(Ini.Vars.Launcher_Version, newGithubConfig[0].tag_name);
                    return true;
                }

                return false;
            }

            if (!iqnoredLauncherUpdate && !AppState.IsInstalling && newServerConfig.launcherallowUpdates && IsNewVersion(Launcher.VERSION, newServerConfig.launcherVersion))
            {
                var messageBoxResult = MessageBox.Show("A new version of the launcher is available. Would you like to update now?", "Launcher Update", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (messageBoxResult == MessageBoxResult.No)
                {
                    iqnoredLauncherUpdate = true;
                    return false;
                }

                Ini.Set(Ini.Vars.Launcher_Version, newServerConfig.launcherVersion);
                return true;
            }

            return false;
        }

        private static bool ShouldUpdateGame(ServerConfig newServerConfig)
        {
            return !AppState.IsInstalling &&
                   newServerConfig.branches[GetBranch.Index()].allow_updates &&
                   Configuration.LauncherConfig != null &&
                   !GetBranch.IsLocalBranch() &&
                   GetBranch.Installed() &&
                   GetBranch.LocalVersion() != GetBranch.ServerVersion();
        }

        private static void HandleLauncherUpdate()
        {
            LogInfo(Source.UpdateChecker, "Updating launcher...");
            UpdateLauncher();
        }

        private static void UpdateLauncher()
        {
            if (!File.Exists($"{Launcher.PATH}\\launcher_data\\updater.exe"))
            {
                LogError(Source.UpdateChecker, "Self updater not found");
                return;
            }

            string extraArgs = (bool)Ini.Get(Ini.Vars.Nightly_Builds) ? " -nightly" : "";

            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c start \"\" \"{Launcher.PATH}\\launcher_data\\updater.exe\"{extraArgs}"
            };

            Process.Start(startInfo);

            Environment.Exit(0);
        }

        private static void HandleGameUpdate()
        {
            if (GetBranch.IsLocalBranch())
                return;

            if (GetBranch.UpdateAvailable())
                return;

            appDispatcher.Invoke(() =>
            {
                SetBranch.UpdateAvailable(true);
                Update_Button.Visibility = Visibility.Visible;
            });
        }
    }

    public class Asset
    {
        public string url { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public Uploader uploader { get; set; }
        public string content_type { get; set; }
        public string state { get; set; }
        public int size { get; set; }
        public int download_count { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string browser_download_url { get; set; }
    }

    public class Author
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public string user_view_type { get; set; }
        public bool site_admin { get; set; }
    }

    public class GithubItems
    {
        public string url { get; set; }
        public string assets_url { get; set; }
        public string upload_url { get; set; }
        public string html_url { get; set; }
        public int id { get; set; }
        public Author author { get; set; }
        public string node_id { get; set; }
        public string tag_name { get; set; }
        public string target_commitish { get; set; }
        public string name { get; set; }
        public bool draft { get; set; }
        public bool prerelease { get; set; }
        public DateTime created_at { get; set; }
        public DateTime published_at { get; set; }
        public List<Asset> assets { get; set; }
        public string tarball_url { get; set; }
        public string zipball_url { get; set; }
        public string body { get; set; }
    }

    public class Uploader
    {
        public string login { get; set; }
        public int id { get; set; }
        public string node_id { get; set; }
        public string avatar_url { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string html_url { get; set; }
        public string followers_url { get; set; }
        public string following_url { get; set; }
        public string gists_url { get; set; }
        public string starred_url { get; set; }
        public string subscriptions_url { get; set; }
        public string organizations_url { get; set; }
        public string repos_url { get; set; }
        public string events_url { get; set; }
        public string received_events_url { get; set; }
        public string type { get; set; }
        public string user_view_type { get; set; }
        public bool site_admin { get; set; }
    }
}
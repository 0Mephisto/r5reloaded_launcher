﻿using Newtonsoft.Json;

namespace launcher
{
    /// <summary>
    /// The DataFetcher class is responsible for fetching various types of data from remote servers.
    /// It includes methods to fetch server configuration, game patch files, JSON data from a given URL,
    /// and base game files. The class uses asynchronous programming to ensure non-blocking operations
    /// when fetching data over the network.
    /// </summary>
    public class DataFetcher
    {
        public static ServerConfig FetchServerConfig()
        {
            var response = Global.client.GetAsync("https://cdn.r5r.org/launcher/config.json").Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<ServerConfig>(responseString);
        }

        public static async Task<GamePatch> FetchPatchFiles()
        {
            int selectedBranchIndex = Utilities.GetCmbBranchIndex();
            string patchURL = Global.serverConfig.branches[selectedBranchIndex].patch_url + "\\patch.json";
            string patchFile = await FetchJson(patchURL);
            return JsonConvert.DeserializeObject<GamePatch>(patchFile);
        }

        public static async Task<BaseGameFiles> FetchBaseGameFiles(bool compressed)
        {
            string fileName = compressed ? "checksums_zst.json" : "checksums.json";
            string baseGameChecksumUrl = $"{Global.serverConfig.base_game_url}\\{fileName}";
            string baseGameZstChecksums = await FetchJson(baseGameChecksumUrl);
            return JsonConvert.DeserializeObject<BaseGameFiles>(baseGameZstChecksums);
        }

        public static async Task<string> FetchJson(string url)
        {
            var response = await Global.client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
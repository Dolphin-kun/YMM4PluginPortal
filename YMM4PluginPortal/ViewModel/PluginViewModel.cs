using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YMM4PluginPortal.Model;
using YMM4PluginPortal.ViewModel.Helper;
using YukkuriMovieMaker.Plugin;

namespace YMM4PluginPortal.ViewModel
{
    internal class PluginViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Plugin> Plugins { get; set; }
        public ICommand OpenUrlCommand { get; }

        public PluginViewModel()
        {
            Plugins = [];
            OpenUrlCommand = new RelayCommand(ExecuteOpenUrl, CanExecuteOpenUrl);
            _ = LoadPluginsAsync();
        }

        #region コマンド
        private void ExecuteOpenUrl(object? parameter)
        {
            if (parameter is string url && !string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"URLを開けませんでした: {ex.Message}");
                }
            }
        }

        private bool CanExecuteOpenUrl(object? parameter)
        {
            return parameter is string url && !string.IsNullOrEmpty(url);
        }
        #endregion

        private async Task LoadPluginsAsync()
        {
            var apiUrl = "https://ymme.ymm4-info.net/api/get";
            using var httpClient = new HttpClient();
            try
            {
                var pluginUrls = await GetPluginLinkDataAsync(httpClient);
                var json = await httpClient.GetStringAsync(apiUrl);
                var pluginData = JsonSerializer.Deserialize<Dictionary<string, Plugin>>(json);

                if (pluginData != null)
                {
                    var localVersions = GetLocalPluginVersions();
                    foreach (var kvp in pluginData)
                    {
                        var plugin = kvp.Value;
                        if (!string.IsNullOrEmpty(plugin.Id) && localVersions.TryGetValue(plugin.Id, out string? localVer))
                        {
                            plugin.LocalVersion = localVer;
                            plugin.IsDownloaded = true;
                            plugin.IsUpdateAvailable = IsNewerVersionAvailable(plugin.Version, localVer);
                        }
                        else
                        {
                            plugin.LocalVersion = "未インストール";
                        }

                        if (!string.IsNullOrEmpty(plugin.Title) && pluginUrls.TryGetValue(plugin.Title, out var linkInfo))
                        {
                            plugin.DownloadUrl = linkInfo.Url;
                            ProcessSocialLinks(plugin, linkInfo);
                        }
                    }


                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Plugins.Clear();
                        foreach (var plugin in pluginData.Values)
                        {
                            Plugins.Add(plugin);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"データの取得に失敗しました: {ex.Message}");
            }

        }


        private static void ProcessSocialLinks(Plugin plugin, PluginLink linkInfo)
        {
            plugin.SocialLinks.Clear();
            if (linkInfo.Links is null) return;

            foreach(var url in linkInfo.Links)
            {
                if (url.Contains("youtube.com") || url.Contains("youtu.be"))
                {
                    plugin.SocialLinks.Add(new DisplayLink
                    {
                        Name = "YouTube",
                        Url = url,
                        IconUrl = IconImage.YouTube
                    });
                }
                else if (url.Contains("nicovideo.jp") || url.Contains("nico.ms"))
                {
                    plugin.SocialLinks.Add(new DisplayLink
                    {
                        Name = "ニコニコ",
                        Url = url,
                        IconUrl = IconImage.Niconico
                    });
                }
                else if (url.Contains("x.com") || url.Contains("twitter.com"))
                {
                    plugin.SocialLinks.Add(new DisplayLink
                    {
                        Name = "X (Twitter)",
                        Url = url,
                        IconUrl = IconImage.X
                    });
                }
                else if (url.Contains("note.com"))
                {
                    plugin.SocialLinks.Add(new DisplayLink
                    {
                        Name = "Note",
                        Url = url,
                        IconUrl = IconImage.Note
                    });
                }
                else if (url.Contains("ymm4-info.net"))
                {
                    plugin.SocialLinks.Add(new DisplayLink
                    {
                        Name = "YMM4情報サイト",
                        Url = url,
                        IconUrl = IconImage.YMM4Info
                    });
                }
            }
        }

        private static async Task<Dictionary<string, PluginLink>> GetPluginLinkDataAsync(HttpClient client)
        {
            var yamlUrl = "https://manjubox.net/ymm4plugins.yml";
            var linkData = new Dictionary<string, PluginLink>();
            try
            {
                var yamlString = await client.GetStringAsync(yamlUrl);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();

                var pluginLinks = deserializer.Deserialize<List<PluginLink>>(yamlString);

                foreach (var link in pluginLinks)
                {
                    if (!string.IsNullOrEmpty(link.Name) && !linkData.ContainsKey(link.Name))
                    {
                        linkData.Add(link.Name, link);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"YAMLファイルの読み込みに失敗しました: {ex.Message}");
            }
            return linkData;
        }

        private static bool IsNewerVersionAvailable(string? serverVersionStr, string? localVersionStr)
        {
            if (string.IsNullOrEmpty(serverVersionStr) || string.IsNullOrEmpty(localVersionStr))
            {
                return false;
            }

            if (Version.TryParse(serverVersionStr, out var serverVersion) &&
                Version.TryParse(localVersionStr, out var localVersion))
            {
                return serverVersion > localVersion;
            }

            return false;
        }

        private static Dictionary<string, string> GetLocalPluginVersions()
        {
            var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var assembly in PluginAssemblyLoader.Assemblies)
                {
                    var assemblyName = assembly.GetName();
                    var name = assemblyName.Name;
                    var version = assemblyName.Version?.ToString() ?? "N/A";

                    if (name != null && !versions.ContainsKey(name))
                    {
                        versions.Add(name, version);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ローカルプラグインのバージョン取得中にエラーが発生しました: {ex.Message}");
            }
            return versions;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

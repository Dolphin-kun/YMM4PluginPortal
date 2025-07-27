using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using YMM4PluginPortal.Model;
using YukkuriMovieMaker.Plugin;

namespace YMM4PluginPortal.ViewModel
{
    internal class PluginViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Plugin> Plugins { get; set; }

        public PluginViewModel()
        {
            Plugins = [];
            _ = LoadPluginsAsync();
        }

        private async Task LoadPluginsAsync()
        {
            var apiUrl = "https://ymme.ymm4-info.net/api/get";
            using var httpClient = new HttpClient();
            try
            {
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
                        Debug.WriteLine($"Name: {name}, Version: {version}");
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

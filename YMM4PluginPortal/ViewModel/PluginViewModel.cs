using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using YMM4PluginPortal.Model;

namespace YMM4PluginPortal.ViewModel
{
    internal class PluginViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Plugin> _plugins;
        public ObservableCollection<Plugin> Plugins
        {
            get { return _plugins; }
            set
            {
                _plugins = value;
                OnPropertyChanged(nameof(Plugins));
            }
        }

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

        private Dictionary<string, string> GetLocalPluginVersions()
        {
            var versions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string? portalPluginPath = Assembly.GetExecutingAssembly().Location;
                if (string.IsNullOrEmpty(portalPluginPath)) return versions;

                string? portalPluginDir = Path.GetDirectoryName(portalPluginPath);
                if (string.IsNullOrEmpty(portalPluginDir)) return versions;

                string? allPluginsDir = Path.GetDirectoryName(portalPluginDir);
                if (string.IsNullOrEmpty(allPluginsDir) || !Directory.Exists(allPluginsDir))
                {
                    Debug.WriteLine($"Plugin directory not found: {allPluginsDir}");
                    return versions;

                }


                var dllFiles = new List<string>();
                GetAllDllFilesRecursive(allPluginsDir, dllFiles);

                foreach (var dllPath in dllFiles)
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(dllPath);
                        var version = assemblyName.Version?.ToString() ?? "N/A";
                        var fileName = Path.GetFileNameWithoutExtension(dllPath);

                        if (fileName != null && !versions.ContainsKey(fileName))
                        {
                            versions.Add(fileName, version);
                            Debug.WriteLine($"Found local plugin: {fileName}, Version: {version} DirPath: {dllPath}");
                        }
                    }
                    catch (BadImageFormatException) { /* .NETアセンブリでないDLLは無視 */ }
                    catch (Exception ex) { Debug.WriteLine($"Error reading version from {dllPath}: {ex.Message}"); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ローカルプラグインのスキャン中にエラーが発生しました: {ex.Message}");
            }
            return versions;
        }

        private void GetAllDllFilesRecursive(string directory, List<string> dllFiles)
        {
            try
            {
                // 現在のディレクトリにあるDLLファイルを追加
                dllFiles.AddRange(Directory.GetFiles(directory, "*.dll"));

                // 現在のディレクトリにある全てのサブディレクトリを取得
                var subDirectories = Directory.GetDirectories(directory);
                // 各サブディレクトリに対して、この関数自身を再度呼び出す（再帰）
                foreach (var subDir in subDirectories)
                {
                    GetAllDllFilesRecursive(subDir, dllFiles);
                }
            }
            catch (Exception ex)
            {
                // アクセス権限のないフォルダなどでエラーが発生しても処理を続ける
                Debug.WriteLine($"Could not access directory {directory}: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

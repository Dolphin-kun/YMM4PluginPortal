
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace YMM4PluginPortal.Model
{
    public class Plugin : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _author = string.Empty;
        private string _createdAt = string.Empty;
        private string _version = string.Empty;
        private string _topics = string.Empty;
        private List<string> _keywords = [];
        private List<string> _category = [];

        [JsonPropertyName("id")]
        public string Id { get => _id; set => SetField(ref _id, value); }

        [JsonPropertyName("title")]
        public string Title { get => _title; set => SetField(ref _title, value); }

        [JsonPropertyName("description")]
        public string Description { get => _description; set => SetField(ref _description, value); }

        [JsonPropertyName("author")]
        public string Author { get => _author; set => SetField(ref _author, value); }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get => _createdAt; set => SetField(ref _createdAt, value); }

        [JsonPropertyName("version")]
        public string Version { get => _version; set => SetField(ref _version, value); }

        [JsonPropertyName("topics")]
        public string Topics { get => _topics; set => SetField(ref _topics, value); }

        [JsonPropertyName("keywords")]
        public List<string> Keywords { get => _keywords; set => SetField(ref _keywords, value); }

        [JsonPropertyName("category")]
        public List<string> Category { get => _category; set => SetField(ref _category, value); }

        private string? _localVersion;
        public string? LocalVersion
        {
            get => _localVersion;
            set
            {
                _localVersion = value;
                OnPropertyChanged(nameof(LocalVersion));
            }
        }

        private bool _isDownloaded;
        [JsonIgnore]
        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                _isDownloaded = value;
                OnPropertyChanged(nameof(IsDownloaded));
            }
        }

        private bool _isUpdateAvailable;
        [JsonIgnore]
        public bool IsUpdateAvailable
        {
            get => _isUpdateAvailable;
            set
            {
                _isUpdateAvailable = value;
                OnPropertyChanged(nameof(IsUpdateAvailable));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

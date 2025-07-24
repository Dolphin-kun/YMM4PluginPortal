
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace YMM4PluginPortal.Model
{
    public class Plugin : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private string _description;
        private string _author;
        private string _createdAt;
        private string _version;
        private string _topics;
        private List<string> _keywords;
        private List<string> _category;
        private string _localVersion;

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

        public string LocalVersion { get => _localVersion; set => SetField(ref _localVersion, value); }

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

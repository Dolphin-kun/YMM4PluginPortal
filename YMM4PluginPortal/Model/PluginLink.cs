namespace YMM4PluginPortal.Model
{
    internal class PluginLink
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public List<string> Links { get; set; } = [];
    }
}

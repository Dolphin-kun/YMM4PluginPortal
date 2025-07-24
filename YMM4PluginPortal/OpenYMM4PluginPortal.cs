using YMM4PluginPortal.View;
using YukkuriMovieMaker.Plugin;

namespace YMM4PluginPortal
{
    public class OpenYMM4PluginPortal : IToolPlugin
    {
        public string Name => "YMM4PluginPortal";

        public Type ViewModelType => typeof(OpenYMM4PluginPortalView);

        public Type ViewType => typeof(YMM4PluginPortalControl);
    }
}
using System.Drawing;
using System.IO;
using System.Reflection;

namespace WebNavigator.Utils
{
    public static class ResourceHelper
    {
        public static Icon GetEmbeddedIcon()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "WebNavigator.logo.ico";
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    return new Icon(stream);
                }
            }
            
            return SystemIcons.Application;
        }

        public static Image GetEmbeddedIconAsImage()
        {
            var icon = GetEmbeddedIcon();
            return icon.ToBitmap();
        }
    }
}
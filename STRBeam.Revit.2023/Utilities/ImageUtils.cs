using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace STRBeam.Revit.Utilities
{
    public static class ImageUtils
    {
        public static BitmapImage LoadImage(Assembly a, string name)
        {
            var img = new BitmapImage();

            try
            {
                var resourceName = a.GetManifestResourceNames().FirstOrDefault(x => x.Contains(name));
                var stram = a.GetManifestResourceStream(resourceName);

                img.BeginInit();
                img.StreamSource = stram;
                img.EndInit();
            }
            catch (Exception)
            {
                // Ignored
            }

            return img;
        }
    }
}

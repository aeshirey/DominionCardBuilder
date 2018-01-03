using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Dominion_Card_Builder
{
    public static class Utility
    {
        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DeleteObject(IntPtr value);


        private static PrivateFontCollection FontCollection = new PrivateFontCollection();

        static Utility()
        {
            //if (File.Exists("OptimusPrincepsSemiBold.ttf"))
            //{
            FontCollection.AddFontFile("OptimusPrincepsSemiBold.ttf");
            //}

            //if (File.Exists("OptimusPrinceps.ttf"))
            //{
            FontCollection.AddFontFile("OptimusPrinceps.ttf");
            //}

        }

        public static FontFamily[] FontFamilies()
        {
            return FontCollection.Families;
        }

        public static string Serialize<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var dcjs = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                dcjs.WriteObject(ms, obj);

                ms.Position = 0;

                using (var sr = new StreamReader(ms))
                {
                    var ret = sr.ReadToEnd();
                    return ret;
                }
            }
        }

        public static object Deserialize<T>(string serialized)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            {
                sw.Write(serialized);
                var dcjs = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                sw.Flush();
                ms.Position = 0;
                object o = dcjs.ReadObject(ms);
                return (T)o;
            }
        }

        /// <summary>
        /// LINQ-like method to perform an action on each element of an IEnumerable
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> inputs, Action<T> action)
        {
            foreach (var input in inputs)
            {
                action(input);
            }
        }

        /// <summary>
        /// LINQ-like method to perform an action on each element of an IEnumerable
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> inputs, Action<T, int> action)
        {
            int i = 1;
            foreach (var input in inputs)
            {
                action(input, i++);
            }
        }

        public static IEnumerable<FileInfo> LoadFiles(string folder, string pattern = "*")
        {
            if (!Directory.Exists(folder))
            {
                throw new DirectoryNotFoundException($"Couldn't find folder {folder}");
            }

            var di = new DirectoryInfo(folder);
            return di.EnumerateFiles(pattern);
        }

        /// <summary>
        /// Convert an Image to a BitmapSource for use in an Image control
        /// </summary>
        public static BitmapSource GetImageStream(System.Drawing.Image image)
        {
            var bitmap = new Bitmap(image);
            IntPtr bmpPt = bitmap.GetHbitmap();
            BitmapSource bitmapSource =
             System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                   bmpPt,
                   IntPtr.Zero,
                   Int32Rect.Empty,
                   BitmapSizeOptions.FromEmptyOptions());

            //freeze bitmapSource and clear memory to avoid memory leaks
            bitmapSource.Freeze();
            DeleteObject(bmpPt);

            return bitmapSource;
        }

        public static string DownloadFile(string url)
        {
            string tempFile = Path.GetTempFileName();

            using (var wc = new WebClient())
            {
                wc.DownloadFile(url, tempFile);
            }

            return tempFile;
        }

        public static IEnumerable<ListBoxItem> ListItems(this ItemsControl control)
        {
            for (int i = 0; i < control.Items.Count; i++)
            {
                yield return (ListBoxItem)control.Items[i];
            }
        }
    }
}

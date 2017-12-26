using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dominion_Card_Builder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int BadgeWidth = 80;
        private const int BadgeHeight = BadgeWidth;
        private const int CardWidth = 825;
        private const int CardHeight = 1125;

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr value);

        private readonly IList<PlacedBadge> PlacedBadges = new List<PlacedBadge>();

        private static BitmapSource GetImageStream(System.Drawing.Image myImage)
        {
            var bitmap = new Bitmap(myImage);
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

        private readonly System.Drawing.Image CoinIcon;
        private readonly System.Drawing.Image CoinBadge;
        private readonly System.Drawing.Image VPIcon;
        private readonly System.Drawing.Image VPBadge;

        public MainWindow()
        {
            InitializeComponent();

            this.CoinIcon = System.Drawing.Image.FromFile(@"C:\Users\adams\Desktop\coinIcon.png");
            this.CoinBadge = System.Drawing.Image.FromFile(@"C:\Users\adams\Desktop\coinBadge.png");
            this.VPIcon = System.Drawing.Image.FromFile(@"C:\Users\adams\Desktop\vpIcon.png");
            this.VPBadge = System.Drawing.Image.FromFile(@"C:\Users\adams\Desktop\vpBadge.png");

            // temporarily make testing easier:
            tbDescription.Text = @"Roll a die.

If it turns up 5 or 6, +1 coin.";
            tbImageLocation.Text = @"C:\Users\adams\Desktop\goons.png";
            comboCardType_SelectionChanged(null, null);
            //
        }

        private void comboCardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var actionCard = @"C:\Users\adams\Desktop\action.png";

            imageCardPreview.Source = RenderCard(
                templateLocation: actionCard,
                title: tbCardTitle.Text,
                description: tbDescription.Text,
                cost: tbCost.Text,
                cardType: tbCardType.Text);
        }

        //private void DrawStringCenteredAround(Graphics graph, string text, Font font, int centeredX, int centeredY)
        //{
        //    var location =  new new System.Drawing.Point(
        //                (int)(width - cardTypeSize.Width) / 2,
        //                (int)(cardTypeYLocation - cardTypeSize.Height / 2));
        //    graph.DrawString(text, font, System.Drawing.Brushes.Black, location);
        //}

        private BitmapSource RenderCard(string templateLocation, string title, string description, string cost, string cardType)
        {
            var centeredText = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };


            using (System.Drawing.Image dest = new Bitmap(CardWidth, CardHeight))
            {
                Graphics graph = Graphics.FromImage(dest);
                graph.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // IMAGE
                {
                    var image = System.Drawing.Image.FromFile(tbImageLocation.Text);
                    var destRect = new RectangleF(93, 163, 642, 415);
                    var srcRect = new RectangleF(0, 0, image.Width, image.Height);
                    graph.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
                }

                // TEMPLATE
                {
                    var template = System.Drawing.Image.FromFile(templateLocation);
                    graph.DrawImage(template, 0, 0, CardWidth, CardHeight);
                }

                // TITLE
                {
                    const int titleYLocation = 120;
                    var titleFont = new Font("Sitka Text", 24, System.Drawing.FontStyle.Bold);
                    var titleSize = graph.MeasureString(title, titleFont);
                    var titleLocation = new System.Drawing.Point(
                        (int)(CardWidth - titleSize.Width) / 2,
                        (int)(titleYLocation - titleSize.Height / 2));
                    graph.DrawString(title, titleFont, System.Drawing.Brushes.Black, titleLocation);
                }

                // CARD TYPE
                {
                    const int cardTypeYLocation = 988;
                    const int cardTypeAvailableWidth = 540;
                    const int cardTypeXOffset = 190;
                    var cardTypeFont = new Font("Sitka Text", 18, System.Drawing.FontStyle.Bold);
                    var cardTypeSize = graph.MeasureString(cardType, cardTypeFont, cardTypeAvailableWidth);
                    var cardTypeLocation = new System.Drawing.Point(
                        (int)(cardTypeAvailableWidth - cardTypeSize.Width) / 2 + cardTypeXOffset,
                        (int)(cardTypeYLocation - cardTypeSize.Height / 2));
                    graph.DrawString(cardType, cardTypeFont, System.Drawing.Brushes.Black, cardTypeLocation);
                }

                // DESCRIPTION
                {
                    const int descriptionYLocation = 775;
                    const int descriptionAvailableWidth = 630;
                    const int descriptionAvailableHeight = 345;
                    var descriptionFont = new Font("Arial", 16);
                    var descriptionSize = graph.MeasureString(description, descriptionFont, descriptionAvailableWidth, centeredText);

                    var descriptionRect = new RectangleF(
                        (CardWidth - descriptionAvailableWidth) / 2,
                        descriptionYLocation - (descriptionAvailableHeight / 2),
                        descriptionAvailableWidth,
                        descriptionAvailableHeight
                        );

                    graph.DrawString(
                        description,
                        descriptionFont,
                        System.Drawing.Brushes.Black,
                        descriptionRect,
                        centeredText
                        );
                }

                // COST
                {
                    var costLocation = new System.Drawing.Point(140, 1000);
                    var costFont = new Font("Sitka Text", 24, System.Drawing.FontStyle.Bold);
                    var costSize = graph.MeasureString(cost, costFont);
                    var cl = new System.Drawing.Point(costLocation.X - (int)costSize.Width / 2, costLocation.Y - (int)costSize.Height / 2);
                    graph.DrawString(cost, costFont, System.Drawing.Brushes.Black, cl);
                }

                // BADGES
                foreach (var badge in this.PlacedBadges)
                {
                    var destRect = new RectangleF(badge.X - BadgeWidth / 2, badge.Y - BadgeHeight / 2, BadgeWidth, BadgeHeight);
                    var srcRect = new RectangleF(0, 0, badge.Image.Width, badge.Image.Height);
                    graph.DrawImage(badge.Image, destRect, srcRect, GraphicsUnit.Pixel);

                    if (!string.IsNullOrEmpty(badge.Text))
                    {
                        var badgeFont = new Font("Sitka Text", 24, System.Drawing.FontStyle.Bold);
                        var badgeSize = graph.MeasureString(badge.Text, badgeFont);
                        var badgeLocation = new System.Drawing.Point(
                            (int)(badge.X),
                            (int)(badge.Y - (BadgeHeight - badgeSize.Height) / 2));
                        graph.DrawString(badge.Text, badgeFont, System.Drawing.Brushes.Black, badgeLocation, centeredText);

                    }
                }

                // Update the preview
                return GetImageStream(dest);
            }
        }

        private void tbCost_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int val;
            if (!int.TryParse(tbCost.Text, out val))
            {
                tbCost.Text = "3";
                return;
            }

            val = e.Delta < 0 ? val - 1 : val + 1;

            tbCost.Text = val.ToString();
        }

        private void btnBrowseImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "JPG files (*.jpg)|*.jpg|PNG files (*.png)|*.png|Bitmap images (*.bmp)|*.bmp|All files (*.*)|*.*",
                RestoreDirectory = true
            };

            if (dlg.ShowDialog() != true)
            {
                return;
            }

            tbImageLocation.Text = dlg.FileName;
        }

        private void imageCardPreview_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // the x and y coords of the mouse are within the imageCardPreview, but we're ultimately
            // going to draw on the full-sized image. calculate how much we need to increase the
            // values by
            float scaleBy = CardHeight / (float)imageCardPreview.ActualHeight;

            int x = (int)(scaleBy * e.GetPosition(sender as IInputElement).X),
                y = (int)(scaleBy * e.GetPosition(sender as IInputElement).Y);


            var cm = new ContextMenu();

            var availableBadges = new[]
            {
                Tuple.Create("Coin", CoinBadge),
                Tuple.Create("Victory Points", VPBadge)
            };

            foreach (var availableBadge in availableBadges)
            {
                var miParent = new MenuItem
                {
                    Icon = new System.Windows.Controls.Image { Source = GetImageStream(availableBadge.Item2) },
                    Header = availableBadge.Item1
                };

                var miChild = new MenuItem { Header = "(blank)", Tag = "" };
                miChild.Click += (s, a) => addBadgeAt(x, y, availableBadge.Item2, "");
                miParent.Items.Add(miChild);

                for (int i = 1; i < 10; i++)
                {
                    miChild = new MenuItem { Header = i.ToString(), Tag = i.ToString() };
                    miChild.Click += (s, a) => addBadgeAt(x, y, availableBadge.Item2, (s as MenuItem).Tag as string);
                    miParent.Items.Add(miChild);
                }
                cm.Items.Add(miParent);
            }

            imageCardPreview.ContextMenu = cm;
        }

        private void addBadgeAt(int x, int y, System.Drawing.Image image, string text)
        {
            PlacedBadges.Add(new PlacedBadge
            {
                X = x,
                Y = y,
                Text = text,
                Image = image
            });

            comboCardType_SelectionChanged(null, null);
        }
    }
}
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
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
        private const int BadgeWidth = 60;
        private const int BadgeHeight = BadgeWidth;
        private const int CardWidth = 825;
        private const int CardHeight = 1125;

        private readonly IList<PlacedBadge> PlacedBadges = new List<PlacedBadge>();
        private readonly IDictionary<string, string> DownloadedImages = new Dictionary<string, string>();
        private readonly Tuple<string, System.Drawing.Image>[] Badges;

        public MainWindow()
        {
            InitializeComponent();

            // Load card templates
            Utility.LoadFiles("Templates", "*.png")
                .ForEach(file =>
                {
                    comboTemplate.Items.Add(new ComboBoxItem
                    {
                        Content = file.Name.Substring(0, file.Name.Length - file.Extension.Length),
                        Tag = file.FullName
                    });
                });

            // Load all badges
            this.Badges = Utility.LoadFiles("Resources", "*Badge.png")
                .Select(fi => Tuple.Create(
                    fi.Name.Replace("Badge.png", string.Empty),
                    System.Drawing.Image.FromFile(fi.FullName)
                    ))
                .ToArray();

            // temporarily make testing easier:
            tbDescription.Text = @"Roll a die.

If it turns up 5 or 6, +1 coin.";
            tbImageLocation.Text = @"C:\Users\adams\Desktop\goons.png";
            comboTemplate.SelectedIndex = 0;
            RenderCard();


            //PrivateFonts.AddFontFile(@"Resources\OptimusPrincepsSemiBold.ttf");
            //PrivateFonts.AddFontFile(@"Resources\OptimusPrinceps.ttf");

            //var ifc = new InstalledFontCollection();

            //var fams = ifc.Families
            //    .OrderBy(f => f.Name)
            //    .Select(f => f.Name)
            //    .ToList();
        }

        private void comboCardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RenderCard();
        }

        private void RenderCard()
        {
            var dest = RenderCardImage();
            imageCardPreview.Source = Utility.GetImageStream(dest);
        }

        private System.Drawing.Image RenderCardImage()
        {
            var templateLocation = (string)(comboTemplate.SelectedItem as ComboBoxItem).Tag;
            var title = tbCardTitle.Text;
            var description = tbDescription.Text;
            var cost = tbCost.Text;
            var cardType = tbCardType.Text;

            var centeredText = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };


            System.Drawing.Image dest = new Bitmap(CardWidth, CardHeight);
            Graphics graph = Graphics.FromImage(dest);
            graph.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // IMAGE
            {
                System.Drawing.Image image;
                if (File.Exists(tbImageLocation.Text))
                {
                    image = System.Drawing.Image.FromFile(tbImageLocation.Text);
                }
                else if (DownloadedImages.ContainsKey(tbImageLocation.Text))
                {
                    image = System.Drawing.Image.FromFile(DownloadedImages[tbImageLocation.Text]);
                }
                else
                {
                    // TODO - image doesn't exist.
                    throw new ApplicationException("Couldn't load image from specified location.");
                }

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
                //var titleFont = new Font("Sitka Text", 24, System.Drawing.FontStyle.Bold);
                var ff = Utility.FontFamilies();
                var n = ff[0].Name;
                if (ff[0].IsStyleAvailable(System.Drawing.FontStyle.Bold))
                {
                    n += " Bold";
                }
                var titleFont = new Font(n /*OptimusPrinceps*/, 24);//, System.Drawing.FontStyle.Bold);
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

            return dest;
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

            RenderCard();
        }

        #region UI Event Handlers
        private void MenuItem_SaveCard(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                //CheckFileExists = true,
                Filter = "JSON files (*.json)|*.json|All Files (*.*)|*.*",
                InitialDirectory = ""
            };

            if (sfd.ShowDialog() != true)
            {
                return;
            }

            using (var sw = new StreamWriter(sfd.FileName))
            {
                int cost;
                var cardObj = new SavedCard
                {
                    Title = tbCardTitle.Text,
                    Description = tbDescription.Text,
                    Cost = int.TryParse(tbCost.Text, out cost) ? cost : 0,
                    Image = tbImageLocation.Text,
                    CardType = tbCardType.Text,
                    Badges = this.PlacedBadges.ToList()
                };

                var json = Utility.Serialize(cardObj);
                sw.Write(json);
            }
        }

        private void MenuItem_ExportImage(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                AddExtension = true,
                //CheckFileExists = true,
                Filter = "PNG files (*.png)|*.png|All Files (*.*)|*.*",
                InitialDirectory = ""
            };

            if (sfd.ShowDialog() != true)
            {
                return;
            }

            var image = RenderCardImage();

            image.Save(sfd.FileName);
        }
        #endregion

        private void MenuItem_OpenCard(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                Filter = "JSON files (*.json)|*.json|All Files (*.*)|*.*",
                InitialDirectory = ""
            };

            if (ofd.ShowDialog() != true || !File.Exists(ofd.FileName))
            {
                return;
            }

            var json = File.ReadAllText(ofd.FileName);

            var cardObj = (SavedCard)Utility.Deserialize<SavedCard>(json);

            // if the Image is a URL, let's download it now
            try
            {
                var uri = new Uri(cardObj.Image);
                var local = Utility.DownloadFile(cardObj.Image);
                DownloadedImages[cardObj.Image] = local;
            }
            catch (UriFormatException)
            {

            }


            tbCardTitle.Text = cardObj.Title;
            tbDescription.Text = cardObj.Description;
            tbCost.Text = cardObj.Cost.ToString();
            tbImageLocation.Text = cardObj.Image;
            tbCardType.Text = cardObj.CardType;
            this.PlacedBadges.Clear();
            foreach (var badge in cardObj.Badges)
            {
                this.PlacedBadges.Add(badge);
            }

            // TODO: render the new card
        }

        private void MenuItem_NewCard(object sender, RoutedEventArgs e)
        {
            tbCardTitle.Text = string.Empty;
            tbDescription.Text = string.Empty;
            tbCost.Text = string.Empty;
            tbImageLocation.Text = string.Empty;
            tbCardType.Text = string.Empty;
            this.PlacedBadges.Clear();
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

            foreach (var badge in this.Badges)
            {
                var miParent = new MenuItem
                {
                    Icon = new System.Windows.Controls.Image { Source = Utility.GetImageStream(badge.Item2) },
                    Header = badge.Item1
                };

                var miChild = new MenuItem { Header = "(blank)", Tag = "" };
                miChild.Click += (s, a) => addBadgeAt(x, y, badge.Item2, "");
                miParent.Items.Add(miChild);

                for (int i = 1; i < 10; i++)
                {
                    miChild = new MenuItem { Header = i.ToString(), Tag = i.ToString() };
                    miChild.Click += (s, a) => addBadgeAt(x, y, badge.Item2, (s as MenuItem).Tag as string);
                    miParent.Items.Add(miChild);
                }
                cm.Items.Add(miParent);
            }

            if (this.PlacedBadges.Count > 0)
            {
                var mi = new MenuItem()
                {
                    Header = "Clear all Badges"
                };
                mi.Click += (s, a) =>
                {
                    this.PlacedBadges.Clear();
                    RenderCard();
                };

                cm.Items.Add(mi);
            }

            imageCardPreview.ContextMenu = cm;
        }

        private void tbImageLocation_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // check if this points to a URL that should be downloaded, a local file that exists, or something else (an error)

            if (File.Exists(tbImageLocation.Text))
            {
                // okay, cool.
                tbImageLocation.Background = System.Windows.Media.Brushes.LightGreen;
                return;
            }

            // did we already download this remote path?
            if (DownloadedImages.ContainsKey(tbImageLocation.Text))
            {
                // also cool
                tbImageLocation.Background = System.Windows.Media.Brushes.LightGreen;
                return;
            }

            Uri uri;
            try
            {
                uri = new Uri(tbImageLocation.Text);
            }
            catch (UriFormatException)
            {
                // not cool
                tbImageLocation.Background = System.Windows.Media.Brushes.LightPink;
                return;
            }

            var local = Utility.DownloadFile(tbImageLocation.Text);

            DownloadedImages[tbImageLocation.Text] = local;
            tbImageLocation.Background = System.Windows.Media.Brushes.LightGreen;
        }
    }
}
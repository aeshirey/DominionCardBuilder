using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominion_Card_Builder
{
    public enum ImageScaling
    {
        /// <summary>Ignore the aspect ratio, making the image scaled down on both X- and Y-axes.</summary>
        Scaled,

        /// <summary>Keep the aspect ratio, but make the image fit on the X-axis, clipping on Y.</summary>
        KeepAspectX,

        /// <summary>Keep the aspect ratio, but make the image fit on the Y-axis, clipping on X.</summary>
        KeepAspectY
    }

    public class Card
    {
        /// <summary>
        /// The name of the card, appearing at the top
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The template to use for this card (for example, "action"), assumed to be available as "Templates/&lt;template-name&gt;.png"
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        /// The type of card (such as "Action-Reaction"), appearing at the bottom, to the right of the <see cref="Cost"/>
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// The (possibly multi-line) description of the card, appearing in the middle
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The cost of the card, appearing at the bottom, to the left of the <see cref="CardType"/>
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// The local path to the image to be used for the card, between the <see cref="Title"/> and <see cref="Description"/>
        /// </summary>
        public string Image { get; set; }

        public ImageScaling Scaling { get; set; }

        /// <summary>
        /// The position and type of badges, if any, to be used in the <see cref="Description"/>
        /// </summary>
        public ICollection<PlacedBadge> Badges { get; set; }


    }
}

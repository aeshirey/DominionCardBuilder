using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Web.Script.Serialization;
using System.Runtime.Serialization;

namespace Dominion_Card_Builder
{
    [DataContract]
    public class PlacedBadge
    {
        [DataMember]
        public int X { get; set; }

        [DataMember]
        public int Y { get; set; }


        [DataMember]
        public string Text { get; set; }

        [IgnoreDataMember]
        public Image Image { get; set; }

        [DataMember]
        public string ImagePath { get; set; }
    }
}

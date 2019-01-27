/// Auteur : Raphael DOUZON
/// Date de création : 13/06/2016


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Samples.Kinect.BodyBasics.DataModel
{
    //la position de la main dans les 3 dimensions
    class HandPosition
    {
        // la position de la main dans x,y
        public Point Position { get; set; }

        //la position de la main dans la profondeur
        public int HandDepth { get; set; }

        //constructeur
        public HandPosition(Point p, int depth)
        {
            Position = p;
            HandDepth = depth;
        }
    }
}

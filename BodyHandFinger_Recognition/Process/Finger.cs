using System;
using System.Windows;
using System.Windows.Media.Media3D; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Samples.Kinect.BodyBasics.Treatment;

namespace Microsoft.Samples.Kinect.BodyBasics.Process
{
    public class Finger
    {
        /* 
         * Structure for a finger
         * @param indexListPts : permet de pouvoir utiliser la liste de points du contour de la main lorsqu'on le souhaite. 
         * @param extremity : extremity of finger  
         * @param Jointure1 : Jointure under extremity  
         * @param Jointure2 : Jointure under Jointure1
         * @param otherExtremity : otherExtremity under Jointure2 
         * @param name : finger's name
         */
        public struct StructFinger
        {
            public int indexListPts { get; set; }
            public Point3D extremity { get; set; }
            public Point3D Jointure1 { get; set; }
            public Point3D Jointure2 { get; set; }
            public Point3D otherExtremity { get; set; }
            public Vector3D vOtherToJoint2 { get; set; }
            public Vector3D vJoint2ToJoin1 { get; set; }
            public Vector3D vJoin1Toextremity { get; set; }
            public StateFinger type { get; set; }
        }


        /*
         * Structure d'un doigt 
         */ 
        public StructFinger st_finger { get; set; }

        /*
         * Constructeur Finger
         * - Structure Finger utilisée
         */
        public Finger()
        {
            st_finger = new StructFinger();
        }

    }
}


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
         * 
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
            public Point extremity { get; set; }
            public Point Jointure1 { get; set; }
            public Point Jointure2 { get; set; }
            public Point otherExtremity { get; set; }
            public Point3D extremity3D { get; set; }
            public Point3D Jointure13D { get; set; }
            public Point3D Jointure23D { get; set; }
            public Point3D otherExtremity3D { get; set; }
            public Vector3D vOtherToJoint2 { get; set; }
            public Vector3D vJoint2ToJoin1 { get; set; }
            public Vector3D vJoin1Toextremity { get; set; }
            public String name { get; set; }
        }

        /*
         * Les points des extrémités du doigt (non utilisée)
         */ 
        public Point P1 { get; set; }
        public Point P2 { get; set; }

        /*
         * Structure d'un doigt 
         */ 
        public StructFinger st_finger { get; set; }

        /*
         * The length of the finger
         */ 
        public float L { get; set; }

        /*
         * the angle of the finger with axis X. angle max=180° && angle min=-180°
         */ 
        public float Angle { get; set; }

        /*
         * Constructeur Finger
         * - Structure Finger utilisée
         */
        public Finger()
        {
            st_finger = new StructFinger();
        }

        /*
         * place les points dans le squelette du doigt
         */ 
        public void setPoints(Point p1, Point p2)
        {
            this.P1 = p1;
            this.P2 = p2;
            L = (float)Values.DistPoints(p1, p2);
            //Angle = (float)Values.CalculAngle(p1, p2);
        }
    }
}


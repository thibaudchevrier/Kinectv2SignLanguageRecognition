using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Microsoft.Samples.Kinect.BodyBasics.Process;

/* -- SOMMAIRE --
 * public static double DistPoints(Point p1, Point p2)
 * public Point PointMid(Point p1, Point p2)
 * public Point FindJointure2(Point extremity, Point otherExtremity)
 * public static Point TransG(Point p1, int Length)
 * public double CalculAngle(Point p1, Point p2, Point p3)
 * public double CalculDeterminant(Point p1, Point p2, Point p3)
 * public static byte AverageByte(byte[] tab, int nbValues)
 * 
 */
namespace Microsoft.Samples.Kinect.BodyBasics.Treatment
{
    //cette classe contient certaines valeurs et fonctions basiques utilisées l'ensemble du programme
    class Values
    {
        //dimensions de l'aimage principale
        public const int DISPLAY_WIDTH = 512;
        public const int DISPLAY_HEIGHT = 424;

        //dimensions du graphique de l'image de coupe
        public const int SIZE_GRAPH_W = 600;
        public const int SIZE_GRAPH_H = 50;

        public const int DISPLAY_HEIGHT_CROSS = 300;

        public const int NB_LAYER_BIN = 10;

        public const int DISPLAY_X_FINGERS = 13;

        public const int L_FINGER_MIN = 13;

        public int DEPTH_FRAME_Length = 217088;

        public int WINDOW_LENGTH = 0;

        public uint BYTES_PER_PIXELS = 0;

        private static volatile Values instance;
        private static object syncRoot = new Object();


        public FrameDescription dframeDescription { get; set; }
        public FrameDescription iframeDescription { get; set; }
        public CoordinateMapper coordinate { get; set; }
        public ushort[] depthFrameData { get; set; }
        public ushort[] irFrameData { get; set; }
        public ushort minDepth { get; set; }
        public int maxDepth { get; set; }

        //création de l'instance du singleton
        public static Values Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new Values();
                    }
                }
                return instance;
            }
        }

        private Values()
        {
            this.depthFrameData = new ushort[424 * 512];
            this.irFrameData = new ushort[424 * 512];
            coordinate = null;
            dframeDescription = null;
            iframeDescription = null;
            minDepth = 0;
            maxDepth = 0;
        }

        //Défini les dimensions de l'image de profondeur
        //ces valeurs ne sont initialisées qu'une seule fois
        //frameHeigt : la hauteur de l'image principale
        //frameWidth : la largeur de l'image principale
        //bPP : (byte per pixel) le nombre d'octets par pixels pour l'image à afficher
        public void SetDimensionsDepthFrame(int frame)
        {
            // DEPTH_FRAME_HEIGHT = frameHeight;
            // DEPTH_FRAME_WIDTH = frameWidth;
            WINDOW_LENGTH = frame;
        }


        //Methode pour calculer les différentes distances entre points ou point milieu
        public static Point DiffPoints(Point p1, Point p2)
        {
            double x, y;
            x = p2.X - p1.X;
            y = p2.Y - p1.Y;
            return new Point(x, y);
        }

        /*
         * Méthode pour calculer la distance entre 2 points 2D
         */
        public static double DistPoints3D(Point3D p1, Point3D p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        /*
         * Méthode pour calculer le point median entre deux points 2D. 
         */
        public Point PointMid(Point p1, Point p2)
        {
            int x, y;
            x = (int)((p1.X + p2.X) / 2);
            y = (int)((p1.Y + p2.Y) / 2);
            return new Point(x, y);
        }

        /*
        * Méthode pour calculer le point median entre deux points 3D. 
        */
        public Point3D PointMid3D(Point3D p1, Point3D p2)
        {
            int x, y, z;
            x = (int)((p1.X + p2.X) / 2);
            y = (int)((p1.Y + p2.Y) / 2);
            z = (int)((p1.Z + p2.Z) / 2);
            return new Point3D(x, y, z);
        }

        /*
         * calcul l'angle au point p2 
         */
        public double CalculAngle(Point3D p1, Point3D p2, Point3D p3)
        {
            double a = DistPoints3D(p1, p2);
            double b = DistPoints3D(p2, p3);
            double c = DistPoints3D(p3, p1);
            double alpha = Math.Acos(((a * a) + (b * b) - (c * c)) / (2 * a * b));
            double deter = CalculDeterminant(p1, p2, p3);
            alpha = alpha * (180 / Math.PI);
            if (deter < 0)
                return alpha;
            else
                return -alpha;
        }



        /*
         * calcul du determinant au point p2
         */
        private double CalculDeterminant(Point3D p1, Point3D p2, Point3D p3)
        {
            double deter = (p1.X - p2.X) * (p3.Y - p2.Y) - (p1.Y - p2.Y) * (p3.X - p2.X);
            return deter;
        }


        /*
        * calcul du determinant au point p2
        */
        public Point Conv3Dto2D(Point3D p1)
        {
            return new Point(p1.X, p1.Y);
        }

        public Vector3D MakeVector3D(Point3D fromPt, Point3D ToPt)
        {
            Vector3D vector3D = new Vector3D();
            vector3D.X = ToPt.X - fromPt.X;
            vector3D.Y = ToPt.Y - fromPt.Y;
            vector3D.Z = ToPt.Z - fromPt.Z;
            return vector3D;
        }
    }
}

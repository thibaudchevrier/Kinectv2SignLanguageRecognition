/// Auteur : Raphael DOUZON
/// Date de création : 13/06/2016


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Samples.Kinect.BodyBasics.DataModel
{
    //cette classe contient certaines valeurs et fonctions basiques utilisées l'ensemble du programme
    class Values
    {
        //dimensions de l'aimage principale
        public const int DISPLAY_WIDTH = 100;
        public const int DISPLAY_HEIGHT = 100;

        //dimensions du graphique de l'image de coupe
        public const int SIZE_GRAPH_W = 600;
        public const int SIZE_GRAPH_H = 50;

        public const int DISPLAY_HEIGHT_CROSS = 300;

        public const int NB_LAYER_BIN = 10;

        public readonly ArrayList LIST_CROSS_X;


        public const int DISPLAY_X_FINGERS = 13;

        public const int L_FINGER_MIN = 13;

        public int DEPTH_FRAME_WIDTH = 0;
        public int DEPTH_FRAME_HEIGHT = 0;

        public uint BYTES_PER_PIXELS = 0;

        private static volatile Values instance;
        private static object syncRoot = new Object();

        //Constructeur du singleton
        private Values()
        {

            //définition de la liste des lignes de coupe de l'image de profondeur
            LIST_CROSS_X = new ArrayList();
            //LIST_CROSS_X.Add(new CrossSection(20, 0, 0, 255));
            //LIST_CROSS_X.Add(new CrossSection(40, 0, 255, 255));
            //LIST_CROSS_X.Add(new CrossSection(45, 0, 255, 0));
           // LIST_CROSS_X.Add(new CrossSection(50, 255, 255, 0));
            //LIST_CROSS_X.Add(new CrossSection(55, 255,0 , 0));
            //LIST_CROSS_X.Add(new CrossSection(60, 255, 0, 255));
            //LIST_CROSS_X.Add(new CrossSection(65, 255, 255, 255));
        }

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

        //Défini les dimensions de l'image de profondeur
        //ces valeurs ne sont initialisées qu'une seule fois
        //frameHeigt : la hauteur de l'image principale
        //frameWidth : la largeur de l'image principale
        //bPP : (byte per pixel) le nombre d'octets par pixels pour l'image à afficher
        public void setDimensionsDepthFrame(int frameHeight, int frameWidth, uint bPP)
        {
            DEPTH_FRAME_HEIGHT = frameHeight;
            DEPTH_FRAME_WIDTH = frameWidth;
            BYTES_PER_PIXELS = bPP;
        }

        //Methode pour calculer les différentes distances entre points ou point milieu
        public static Point DiffPoints(Point p1, Point p2)
        {
            double x, y;
            x = p2.X - p1.X;
            y = p2.Y - p1.Y;
            return new Point(x, y);
        }

        //Méthode pour calculer la distance entre 2 points
        public static double DistPoints(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        //Méthode pour calculer le point median entre
        public static Point PointMid(Point p1, Point p2)
        {
            int x, y;
            x = (int)((p1.X + p2.X) / 2);
            y = (int)((p1.Y + p2.Y) / 2);
            return new Point(x, y);
        }

        //calcul l'angle entre 2 axes qui sont représentés par des points ici
        public static double CalculAngle(Point p1, Point p2)
        {
            double y = p1.Y - p2.Y;
            double x = p1.X - p2.X;
            double alpha = 0;
            if (y < 0.01 && y > -0.01)
            {
                if (x > 0)
                    alpha = 0;
                else
                    alpha = 180;
            }
            else
            {
                if (x >= 0)
                    alpha = Math.Atan(y / x) * 180 / 3.14;
                else
                {
                    if (y < 0)
                        alpha = -180 + Math.Atan(y / x) * 180 / 3.14;
                    else
                        alpha = 180 + Math.Atan(y / x) * 180 / 3.14;
                }
            }
            return alpha;
        }

        //calcul de la moyenne des valeurs dans un tableau dans les nbValues premières casse
        //tab : le tableau en une dimension de taille variable
        //nbValues : la taille du tableau ou le nombre de valeurs sur lesquelles faire la moyenne
        //return : la valeur moyenne du tableau
        public static byte AverageByte(byte[] tab, int nbValues)
        {
            int value = 0;
            for (int i = 0; i < nbValues; i++)
            {
                value += tab[i];
            }
            value = (int)Math.Ceiling(((decimal)value / (decimal)nbValues));

            return (byte)value;
        }
    }
}


/// Auteur : Raphael DOUZON
/// Date de création : 13/06/2016

using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Samples.Kinect.BodyBasics.Treatment;

namespace Microsoft.Samples.Kinect.BodyBasics.Process
{
    //la classe qui gère la reconnaissance de la position des mains
    public class Hand
    {
        /*
         * Strucure composée d'un pixel et d'un etat (correspond a une extréminté ou un creux) 
         * - Etat : Left or Right 
         * - Hollow
         * - Center
         */ 
        public struct StructHand 
        {
            public Point extremityHollow { get; set; }
            public string etat { get; set; }
        }

        public List<Point> listPts { get; set; }
        public List<StructHand> listStruct { get; set; }
        public List<Finger.StructFinger> fingerHand { get; set; }

        public Point tip = new Point();
        public Point center = new Point();
        public Point thumb = new Point();
        public Point wrist = new Point();

        /* Preparation pour traiter les données en 3D */ 
        public Point3D center3D = new Point3D();
        public Point3D tip3D = new Point3D();
        public Point3D thumb3D = new Point3D();
        public Point3D wrist3D = new Point3D();


        /*
         * Le nombre de doigts dépliés (non utilisée) 
         */
        public int NumberFingOut { get; set; }

        /*
         * Le constructeur de la main
         * 
         * @param listPts : la liste des points du contour de la main
         * @param listStruct : représente la liste des points d'extremités, segment du milieu du doigt
        */
        public Hand()
        {
            listPts = new List<Point>();
            listStruct = new List<StructHand>();
            fingerHand = new List<Finger.StructFinger>();
            center = new Point();
            tip = new Point();
            thumb = new Point();
            wrist = new Point();

            /*Non utilisé pour le moment*/
            center3D = new Point3D();
            tip3D = new Point3D();
            thumb3D = new Point3D();
            wrist3D = new Point3D();
        }

        /*
         * Methode permettant d'ajouter des points a la liste du contour d'une main. 
         */ 
        public void setListPts(int xP, int yP)
        {
            this.listPts.Add(new Point((int)xP, (int)yP));
        }

        /*
         * Methode permettant de vider les listes du contour des mains.
         */ 
        public void CleanListHand()
        {
            this.listPts.Clear();
            this.listStruct.Clear();
            this.fingerHand.Clear();
        }
    }
}
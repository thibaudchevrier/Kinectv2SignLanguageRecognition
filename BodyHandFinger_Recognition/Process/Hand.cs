/// Auteur : Raphael DOUZON
/// Date de création : 13/06/2016

using Microsoft.Kinect;
using System;
using System.Windows.Media.Media3D;
using System.Collections;
using System.Collections.Generic;
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
         * - 5 Fingers 
         */ 
        public struct StructHand 
        {
           public Point3D extremityHollow { get; set; }
           public string etat { get; set; }
           public List<Point3D> hollow { get; set; }
        }

    /*
     * la position de la main dans la profondeur
     */ 
        public List<Point3D> listPts { get; set; }
        public List<StructHand> listStruct { get; set; }
        public List<Finger.StructFinger> fingerHand { get; set; }
        public StateHand state { get; set; }

        /*
         * Le nombre de doigts dépliés (non utilisée) 
         */
        private int NumberFingOut { get; set; }

        /*
         * Le constructeur de la main
         * 
         * @param listPts : la liste des points du contour de la main
         * @param listStruct : représente la liste des points d'extremités, segment du milieu du doigt
        */
        public Hand(StateHand state)
        {

            listPts = new List<Point3D>();
            listStruct = new List<StructHand>();
            fingerHand = new List<Finger.StructFinger>();
            this.state = state;
    }

        /*
         * Methode permettant d'ajouter des points a la liste du contour d'une main. 
         */ 
        public void SetListPts(int xP, int yP, int zP)
        {
            this.listPts.Add(new Point3D((int)xP, (int)yP, (int)zP));
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
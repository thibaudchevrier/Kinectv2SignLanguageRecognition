/// Auteur : Raphael DOUZON
/// Date de création : 13/06/2016


using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Samples.Kinect.BodyBasics.Process;
using System.Windows.Media.Media3D;

namespace Microsoft.Samples.Kinect.BodyBasics.Treatment
{
    class RecoFingers
    {
        private Values calcul = Values.Instance;

        /*
        * Methode pour preparer le transfert en 3D du code, on initialise le z des points 2D à 0.
        */
        public Finger.StructFinger MakePoint3D(Finger.StructFinger structFinger)
        {
            structFinger.extremity = new Point3D(structFinger.extremity.X, structFinger.extremity.Y, 0);
            structFinger.Jointure1 = new Point3D(structFinger.Jointure1.X, structFinger.Jointure1.Y, 0);
            structFinger.Jointure2 = new Point3D(structFinger.Jointure2.X, structFinger.Jointure2.Y, 0);
            structFinger.otherExtremity = new Point3D(structFinger.otherExtremity.X, structFinger.otherExtremity.Y, 0);
            return structFinger;
        }

        /*
        * Méthode pour calculer la jointure1 d'un doigt.
        * On divise par 3 le compteur qui correspond au nombre de pixels entre le hollow et l'extrémité. 
        * On le soustrait à l'index pour obtenir le pixel se situant avant l'index. 
        * On l'ajoute pour obtenir le pixel se situant après l'index. 
        * Puis on va chercher le mileu de ces deux pixels qui se trouvent dans la listePts du contour de la main. 
        */
        private Point3D FindJointure1(Finger.StructFinger strucFinger, int compteur, List<Point3D> listePts)
        {
            int x, y, z;
            int compt = compteur / 3;
            int indiceAvant = strucFinger.indexListPts - (compt);
            int indiceApres = strucFinger.indexListPts + (compt);
            if (indiceAvant < 0)
                indiceAvant = listePts.Count - 1 + (strucFinger.indexListPts + indiceAvant);
            if (indiceAvant > listePts.Count - 1)
                indiceAvant = (indiceAvant - listePts.Count);
            if (indiceApres > listePts.Count - 1)
                indiceApres = (indiceApres - listePts.Count);
            if (indiceApres < 0)
                indiceApres = listePts.Count - 1 + (strucFinger.indexListPts + indiceApres);

            x = (int)((listePts[indiceAvant].X + listePts[indiceApres].X) / 2);
            y = (int)((listePts[indiceAvant].Y + listePts[indiceApres].Y) / 2);
            z = (int)((listePts[indiceAvant].Z + listePts[indiceApres].Z) / 2);
            return new Point3D(x, y, z);
        }

        /*
        * Méthode pour calculer la jointure1 d'un doigt.
        * On divise par 3 le compteur qui correspond au nombre de pixels entre le hollow et l'extrémité. 
        * On le soustrait à l'index pour obtenir le pixel se situant avant l'index. 
        * On l'ajoute pour obtenir le pixel se situant après l'index. 
        * Puis on va chercher le mileu de ces deux pixels qui se trouvent dans la listePts du contour de la main. 
        */
        private Point3D FindJointure2(Finger.StructFinger strucFinger, int compteur, List<Point3D> listePts)
        {
            int x, y, z;
            int compt = compteur / 3;
            int indiceAvant = strucFinger.indexListPts - (2 * compt);
            int indiceApres = strucFinger.indexListPts + (2 * compt);
            if (indiceAvant < 0)
                indiceAvant = listePts.Count - 1 + (strucFinger.indexListPts + indiceAvant);
            if (indiceAvant > listePts.Count - 1)
                indiceAvant = (indiceAvant - listePts.Count);
            if (indiceApres > listePts.Count - 1)
                indiceApres = (indiceApres - listePts.Count);
            if (indiceApres < 0)
                indiceApres = listePts.Count - 1 + (strucFinger.indexListPts + indiceApres);

            x = (int)((listePts[indiceAvant].X + listePts[indiceApres].X) / 2);
            y = (int)((listePts[indiceAvant].Y + listePts[indiceApres].Y) / 2);
            z = (int)((listePts[indiceAvant].Z + listePts[indiceApres].Z) / 2);
            return new Point3D(x, y, z);
        }

        /*
         * make a finger's jonction according to extremity and otherextremiy. 
         * Modification of jointure1 and jointure2. 
         * 2D.
         */
        public Finger.StructFinger MakeFingerJointure(Finger.StructFinger structFinger, int compteur, List<Point3D> listePts)
        {

            structFinger.Jointure1 = FindJointure1(structFinger, compteur, listePts);
            structFinger.Jointure2 = FindJointure2(structFinger, compteur, listePts);
            return structFinger;
        }

        /* 
         * Methode permettant de concevoir les différents vecteurs d'un doigt. 
         * 3D. 
         * 
         */
        public Finger.StructFinger MakeFingerVector(Finger.StructFinger structFinger)
        {
            structFinger.vOtherToJoint2 = calcul.MakeVector3D(structFinger.otherExtremity, structFinger.Jointure2);
            structFinger.vJoint2ToJoin1= calcul.MakeVector3D(structFinger.Jointure2, structFinger.Jointure1);
            structFinger.vJoin1Toextremity = calcul.MakeVector3D(structFinger.Jointure1, structFinger.extremity);
            return structFinger;
        }

        /*
         * Methode permettant d'identifier les doigts d'une main. 
         * 
         * 
         */
        public Finger.StructFinger IdentifyFinger(Hand hand, Finger.StructFinger structFinger, JointType center, JointType thumb)
        {
            int i = 0; 
            for (i = 0; i < hand.fingerHand.Count; i++)
            {

            }


            return structFinger;
        }
    }

}

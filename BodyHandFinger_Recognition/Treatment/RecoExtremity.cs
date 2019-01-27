using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Samples.Kinect.BodyBasics.Process;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.BodyBasics.Treatment
{
    class RecoExtremity
    {
        private RecoFingers recoFinger;
        private Point3D[] pixelExt;
        private Hand.StructHand[] handStruct;
        private Finger.StructFinger[] fingerStruct;
        //private static Treatment.RecoFingers recoFinger = new RecoFingers();

        public RecoExtremity()
        {
            recoFinger = new Treatment.RecoFingers();
        }

        //Récupère les extremités en fonction d'un point central fixe
        //compare les distances avec le point centrale entre plusieurs points voisins et en déduit les sommets et les creux
        //listExtremity : la liste des extrémités qui va être remplie
        //listFingers : la liste des doigts qui va être remplie
        //listPts : la liste des points du contour de la main
        //matDepth: la matrice contenant l'image de profondeur
        public void SearchExtremity(Hand hand)
        {
            hand.listStruct.Clear();
            pixelExt = null;
            
            //l'indice du point en cours
            int i; double angle = 0; ; int count = 0; Values calcul = Values.Instance; string result = null; int compt_between_Ext_Hol = 0;
            /* Nombre de pixels par defaut pour un doigt */
            int nbrePixel = 30;
            pixelExt = new Point3D[hand.listPts.Count];
            handStruct = new Hand.StructHand[hand.listPts.Count];
            fingerStruct = new Finger.StructFinger[hand.listPts.Count];
            /*
             * L'index choisit est fixé à 10 mais pour une meilleure approche il devrait être dynmique selon la profondeur de la main. 
             */
            int valueDiffIndex = 10;

            /*
             * Calcul de l'angle entre le point courant et deux autres points à plus ou moins valueDiffIndex permettant de calculer l'angle 
             * dans le but de reprérer des points d'extrémités de doigt. 
             */ 
            for (i = 0; i < hand.listPts.Count; i++)
            {
                if (i > valueDiffIndex-1 && hand.listPts.Count > (i + valueDiffIndex) && i < (hand.listPts.Count - valueDiffIndex))
                {
                    angle = calcul.CalculAngle(hand.listPts[i - valueDiffIndex], hand.listPts[i], hand.listPts[i + valueDiffIndex]);
                }
                if (i < valueDiffIndex-1 && hand.listPts.Count > (i + valueDiffIndex))
                {
                    angle = calcul.CalculAngle(hand.listPts[hand.listPts.Count - (valueDiffIndex - i)], hand.listPts[i], hand.listPts[i + valueDiffIndex]);
                }
                else if (i > (hand.listPts.Count - valueDiffIndex) && hand.listPts.Count > 100)
                {
                    angle = calcul.CalculAngle(hand.listPts[i - valueDiffIndex], hand.listPts[i], hand.listPts[i - (hand.listPts.Count - valueDiffIndex)]);
                }
                /*
                 * Selection des extremités avec un angle compris entre 0 et 70
                 */ 
                if (angle <= 70 && angle > 0)
                {
                    pixelExt[i] = hand.listPts[i];
                    result = "extremity";
                }
                /*
                 * selection des creux d'une main avec un angle compris entre -80 et 0
                 */ 
                else if (angle >= -80 && angle < 0)
                {
                    pixelExt[i] = hand.listPts[i];
                    result = "hollow"; 
                }
                /*
                 * La condition permet de compter le nombre de points enregistrés juste avant avec la condition de l'angle. 
                 * On récupère la valeur du compteur et on divise celle-ci par deux afin de récupérer une extrémité parmi cette liste d'extrémités 
                 */ 
                if (i > 0)
                {
                    if (pixelExt[i].X != 0 && pixelExt[i].Y != 0)
                    {
                        count++;
                    }
                    else if (pixelExt[i].X == 0 && pixelExt[i].Y == 0 && pixelExt[i - 1].X != 0 && pixelExt[i - 1].Y != 0)
                    {
                        handStruct[i].extremityHollow = pixelExt[(i - 1) - (count / 2)];
                        handStruct[i].etat = result;
                        if(result == "extremity")
                        {
                            fingerStruct[i].extremity = pixelExt[(i - 1) - (count / 2)];
                            fingerStruct[i].indexListPts = (i - 1) - (count / 2);
                            // Le compteur n'est pas encore adapter pour tous les doigts. 
                            if (nbrePixel > compt_between_Ext_Hol && compt_between_Ext_Hol != 0)
                            {
                                nbrePixel = compt_between_Ext_Hol;
                            }

                            if (compt_between_Ext_Hol != 0 && compt_between_Ext_Hol < 100)
                            {
                                fingerStruct[i].otherExtremity = PointMakeDirectionFinger((i - 1) - (count / 2), hand.listPts, compt_between_Ext_Hol);
                                fingerStruct[i] = recoFinger.MakeFingerJointure(fingerStruct[i], compt_between_Ext_Hol, hand.listPts);
                            }

                            else
                            {
                                fingerStruct[i].otherExtremity = PointMakeDirectionFinger((i - 1) - (count / 2), hand.listPts, nbrePixel);
                                fingerStruct[i] = recoFinger.MakeFingerJointure(fingerStruct[i], nbrePixel, hand.listPts);
                            }
                            hand.fingerHand.Add(fingerStruct[i]);
                        }
                        hand.listStruct.Add(handStruct[i]);
                        compt_between_Ext_Hol = 0;
                        count = 0;
                    }
                    else
                    {
                        compt_between_Ext_Hol++;
                    }
                }
            }
        }

        /* 
         * Methode qui va cherher le milieu du segment se trouvant dans la {liste} a plus ou moins {value} de l'{index}.
         * Cette methode retourne un point qui permettra de construire avec le point extremité associé la direction du doigt.
         * (Idée de bissectrice de l'extremite finalement). 
         *
         * @param index L'index du point extremité
         * @param liste Liste du contour ou retrouver les points. (Même index utilisé entre cette liste et le point extremite).
         * @param value La valeur choisie pour selectionner les deux points formant le segment. (Dans notre cas nous prenons 10).
         * 
         * @return pointCenter Le point milieu du segment formé des points de la liste se situant aux index : index-value et index+value.
         */ 
        public Point3D PointMakeDirectionFinger(int index, List<Point3D> liste, int value)
        {
            Point3D pointCenter = new Point3D();
            Values calcul = Values.Instance;
            if (index > (value-1) && liste.Count > (index + value) && index < (liste.Count - value))
            {
                pointCenter = calcul.PointMid3D(liste[index - value], liste[index + value]);
            }
            if (index < value && liste.Count > (index + value))
            { 
                pointCenter = calcul.PointMid3D((liste[liste.Count - (value - index)]), liste[index + value]);
            }
            else if (index > (liste.Count - value) && liste.Count > 100)
            {
                pointCenter = calcul.PointMid3D(liste[index - value], liste[index - (liste.Count - value)]);
            }
            return pointCenter; 
        }

        public void FingerVector(Hand hand)
        {
            for (int i = 0; i<hand.fingerHand.Count; i++)
            {
                hand.fingerHand[i] = recoFinger.MakeFingerVector(hand.fingerHand[i]);
            }
        }
    }
}

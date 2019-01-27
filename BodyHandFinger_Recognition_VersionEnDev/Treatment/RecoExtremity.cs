using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Samples.Kinect.BodyBasics.Process;
using Microsoft.Kinect;

namespace Microsoft.Samples.Kinect.BodyBasics.Treatment
{
    class RecoExtremity
    {
        Point[] pixelExt;
        Hand.StructHand[] handStruct;
        Finger.StructFinger[] fingerStruct;
        private Values calcul = Values.Instance;
        private static Treatment.RecoFingers recoFinger = new RecoFingers();

        public RecoExtremity()
        {
            
        }

        /*
         * Récupère les extremités en fonction de la liste de point du contour de la main; 
         * selon l'angle et le signe de l'angle nous pouvons en déduire une extrémité ou un creux. 
         * 
         * listExtremity : la liste des extrémités qui va être remplie
         * listFingers : la liste des doigts qui va être remplie
         * listPts : la liste des points du contour de la main
         */ 
        public void searchExtremity(Hand hand)
        {
            hand.listStruct.Clear();
            hand.fingerHand.Clear();
            pixelExt = null;
            
            //l'indice du point en cours
            int i; double angle = 0; ; int count = 0; Values calcul = Values.Instance; string result = null; int compt_between_Ext_Hol = 0;
            int nbrePixel = 20;
            pixelExt = new Point[hand.listPts.Count];
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
                if (i > valueDiffIndex - 1 && hand.listPts.Count > (i + valueDiffIndex) && i < (hand.listPts.Count - valueDiffIndex))
                {
                    angle = calcul.CalculAngle(hand.listPts[i - valueDiffIndex], hand.listPts[i], hand.listPts[i + valueDiffIndex]);
                }
                if (i < valueDiffIndex - 1 && hand.listPts.Count > (i + valueDiffIndex))
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
                            if (nbrePixel > compt_between_Ext_Hol && compt_between_Ext_Hol != 0) {
                                nbrePixel = compt_between_Ext_Hol;
                            }
                            if (compt_between_Ext_Hol != 0)
                                fingerStruct[i].otherExtremity = recoFinger.pointMakeDirectionFinger((i - 1) - (count / 2), hand.listPts, compt_between_Ext_Hol);
                            else
                                fingerStruct[i].otherExtremity = recoFinger.pointMakeDirectionFinger((i - 1) - (count / 2), hand.listPts, nbrePixel);

                            fingerStruct[i] = recoFinger.makeFingerJointure(fingerStruct[i]);  
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
         * ///// METHODE EN COURS DE DEVELOPPEMENT //////
         * Récupère les extremités lorsque la main est fermée. (utilisée lorsque la première méthode ne fonctionne pas) 
         * 
         * listExtremity : la liste des extrémités qui va être remplie
         * listFingers : la liste des doigts qui va être remplie
         * listPts : la liste des points du contour de la main
         */
        public void searchExtremityHandClose(Hand hand)
        {
            Values calcul = Values.Instance;
            hand.listStruct.Clear();
            hand.fingerHand.Clear();
            pixelExt = null;

            int i; double angle = 0;
            int count = 0;
            string result = null;
            int compt_suite = 0;
            double refAngle = -1;

            /* Initialisation */
            pixelExt = new Point[hand.listPts.Count];
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
                if (i > valueDiffIndex - 1 && hand.listPts.Count > (i + valueDiffIndex) && i < (hand.listPts.Count - valueDiffIndex))
                {
                    angle = calcul.CalculAngle(hand.listPts[i - valueDiffIndex], hand.listPts[i], hand.listPts[i + valueDiffIndex]);
                }
                if (i < valueDiffIndex - 1 && hand.listPts.Count > (i + valueDiffIndex))
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
                if (angle < 0 && angle > -120)
                {
                    angle = -1;
                    pixelExt[i] = hand.listPts[i];
                    result = "hollow";
                }
                    
                else if (angle > 0 && angle < 120)
                {
                    angle = 1;
                    pixelExt[i] = hand.listPts[i];
                    result = "extremity";
                }
                    
                if (i > 0)
                {
                    if (angle == refAngle)
                    {
                        refAngle = angle;
                        compt_suite = 0;
                        count++;
                    }
                    else if (compt_suite < 2)
                    {
                        compt_suite++;
                        count++;
                    }
                    else 
                    {
                        handStruct[i].extremityHollow = pixelExt[(i - 1) - (count / 2)];
                        handStruct[i].etat = result;
                        if (result == "extremity")
                        {
                            fingerStruct[i].extremity = pixelExt[(i - 1) - (count / 2)];
                            fingerStruct[i].indexListPts = (i - 1) - (count / 2);
                            hand.fingerHand.Add(fingerStruct[i]);
                        }
                        hand.listStruct.Add(handStruct[i]);
                        compt_suite = 0;
                        refAngle = angle;
                        count = 0;                      
                    }               
                }
            }
        }
    }
}

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
        public Finger.StructFinger makePoint3D(Finger.StructFinger structFinger)
        {
            structFinger.extremity3D = new Point3D(structFinger.extremity.X, structFinger.extremity.Y, 0);
            structFinger.Jointure13D = new Point3D(structFinger.Jointure1.X, structFinger.Jointure1.Y, 0);
            structFinger.Jointure23D = new Point3D(structFinger.Jointure2.X, structFinger.Jointure2.Y, 0);
            structFinger.otherExtremity3D = new Point3D(structFinger.otherExtremity.X, structFinger.otherExtremity.Y, 0);
            return structFinger;
        }

        /*
         * make a finger's jonction according to extremity and otherextremiy. 
         * Modification of jointure1 and jointure2. 
         * 2D.
         */
        public Finger.StructFinger makeFingerJointure(Finger.StructFinger structFinger )
        {
            structFinger.Jointure2 = calcul.FindJointure2(structFinger.extremity, structFinger.otherExtremity);
            structFinger.Jointure1 = calcul.FindJointure2(structFinger.otherExtremity, structFinger.extremity);
            return structFinger;
        }

        /* 
         * Methode permettant de concevoir les différents vecteurs d'un doigt. 
         * 3D. 
         * 
         */
        public Finger.StructFinger makeFingerVector(Finger.StructFinger structFinger)
        {
            structFinger.vOtherToJoint2 = calcul.makeVector3D(structFinger.otherExtremity3D, structFinger.Jointure23D);
            structFinger.vJoint2ToJoin1= calcul.makeVector3D(structFinger.Jointure23D, structFinger.Jointure13D);
            structFinger.vJoin1Toextremity = calcul.makeVector3D(structFinger.Jointure13D, structFinger.extremity3D);
            return structFinger;
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
        public Point pointMakeDirectionFinger(int index, List<Point> liste, int value)
        {
            Point pointCenter = new Point();
            Values calcul = Values.Instance;
            if (index > (value - 1) && liste.Count > (index + value) && index < (liste.Count - value))
            {
                pointCenter = calcul.PointMid(liste[index - value], liste[index + value]);
            }
            if (index < value && liste.Count > (index + value))
            {
                pointCenter = calcul.PointMid((liste[liste.Count - (value - index)]), liste[index + value]);
            }
            else if (index > (liste.Count - value) && liste.Count > 100)
            {
                pointCenter = calcul.PointMid(liste[index - value], liste[index - (liste.Count - value)]);
            }
            return pointCenter;
        }
        /*
         * Methode permettant d'identifier les doigts d'une main. 
         *  
         */
        public Finger.StructFinger identifyFinger(Hand hand, Finger.StructFinger finger, int index)
        {
            if (hand.fingerHand.Count == 5)
            {
                double a1 = calcul.CalculAngle(hand.fingerHand[0].extremity, hand.center, hand.fingerHand[1].extremity);
                double a2 = calcul.CalculAngle(hand.fingerHand[0].extremity, hand.center, hand.fingerHand[2].extremity);
                double a3 = calcul.CalculAngle(hand.fingerHand[0].extremity, hand.center, hand.fingerHand[3].extremity);
                double a4 = calcul.CalculAngle(hand.fingerHand[0].extremity, hand.center, hand.fingerHand[4].extremity);

                if ((a1 > 15 && a1 < 45 && a1 < a2 && a2 < a3 && a3 < a4) || (a1 < -15 && a1 > -45 && a1 > a2 && a2 > a3 && a3 > a4))
                {
                    if (index == 0)
                        finger.name = (StateFinger.THUMB).ToString();
                    else if (index == 1)
                        finger.name = (StateFinger.INDEX_FINGER).ToString();
                    else if (index == 2)
                        finger.name = (StateFinger.MIDDLE_FINGER).ToString();
                    else if (index == 3)
                        finger.name = (StateFinger.RING_FINGER).ToString();
                    else if (index == 4)
                       finger.name = (StateFinger.PINKY).ToString();
                }
                else
                {
                    if (index == 0)
                        finger.name = (StateFinger.PINKY).ToString();
                    else if (index == 1)
                        finger.name = (StateFinger.RING_FINGER).ToString();
                    else if (index == 2)
                        finger.name = (StateFinger.MIDDLE_FINGER).ToString();
                    else if (index == 3)
                        finger.name = (StateFinger.INDEX_FINGER).ToString();
                    else if (index == 4)
                        finger.name = (StateFinger.THUMB).ToString();
                }
                    
            }
            //double aThumb = calcul.CalculAngle(finger.extremity, hand.center, hand.thumb);
            //double aTip = calcul.CalculAngle(finger.extremity, hand.center, hand.tip);
            //double aWrist = calcul.CalculAngle(finger.extremity, hand.fingerHand[0]., hand.wrist);

            //    if ((aWrist < 180 && aWrist > 170) && (aThumb < 90 && aThumb > 30) && (aTip < 10 && aTip > 0) || (aWrist > -180 && aWrist < -170) && (aThumb > -90 && aThumb < -30))
            //        finger.name = (StateFinger.MIDDLE_FINGER).ToString();
                //else if (aThumb > 10 && aThumb < 100 && aTip > 10 && aTip < 40 && aWrist > 120 && aWrist < 180)
                //    finger.name = (StateFinger.INDEX_FINGER).ToString();
                //else if (aThumb > 20 && aThumb < 120 && aTip < 10 && aWrist > 140 && aWrist < 200)
                //    finger.name = (StateFinger.MIDDLE_FINGER).ToString();
                //else if (aThumb > 10 && aThumb < 100 && aTip > 10 && aTip < 40 && aWrist > 180 && aWrist < 220)
                //    finger.name = (StateFinger.RING_FINGER).ToString();
                //else if (aThumb < 10 && aTip > 25 && aWrist > 75)
                //    finger.name = (StateFinger.PINKY).ToString();
            return finger;
        }
    }

}

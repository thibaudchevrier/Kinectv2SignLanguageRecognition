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

namespace Microsoft.Samples.Kinect.BodyBasics.Treatment
{
    class RecoHand
    {

        private MatDisplay mat;
        private RecoExtremity recoE;

        private int xStartLeft;

        private int yStartLeft;

        private int xStartRight;

        private int yStartRight;

        //L'image binaire sur une 3eme dimensions pour pouvoir en enregistrer plusieurs
        //_matBin[L, X, Y]
        //L = couche de l'image de traitement
        //valeur max=1
        private Byte[,,] matBin = null;

        private Byte[,,] matBinInt = null;

        //Constructeur
        public RecoHand()
        {
            matBin = new Byte[4, Values.DISPLAY_WIDTH, Values.DISPLAY_HEIGHT];
            matBinInt = new Byte[4, Values.DISPLAY_WIDTH, Values.DISPLAY_HEIGHT];
            mat = new MatDisplay();
            recoE = new RecoExtremity();
        }



        //cette fonction applique un filtre pour lisser les bords d'une image binaire en un seul point
        //x1,x2,x3 et x4 :les cellules autour de la cellule à traiter
        private int Smooth(int x1, int x2, int x3, int x4, int c)
        {
            int val = x1 + x2 + x3 + x4;
            if(c == 1)
                return val == 0 || val == 1 || (val == 2 && (x1 + x3 == 2 || x2 + x4 == 2)) ? 0 : 1;
            else
                return val == 0 || val == 2 || (val == 4 && (x1 + x3 == 4 || x2 + x4 == 4)) ? 0 : 2;
        }

        //Créé une erosion autour de la main pour retirer les imperfections
        //matBin[L,X,Y] : la matrice 3D des images binaires à traiter 
        //layerBin : la valeur de selection de la matrice 2D dans matBin
        private int Erosion(int x1, int x2, int x3, int x4, int x5, int x6, int x7, int x8, int c)
        {
            int val = x1 + x2 + x3 + x4 + x5 + x6 + x7 + x8;
            if (c == 1)
                return val >= 7 ? 1 : 0;
            else
                return val >= 14 ? 2 : 0;
        }

        //cette fonction vérifie que le point n'est pas entouré par du vide ou est totalement entouré
        //x1,x2,x3 et x4 : les cellules autour de la cellule à traiter
        private int EnvelopHand(int x1, int x2, int x3, int x4, int c)
        {
            int val = x1 + x2 + x3 + x4;
            if (c == 1)
                return val == 4 || val == 0 ? 0 : 1;
            else
                return val == 8 || val == 0 ? 0 : 2;

        }

        //Recupération de la ligne de contour de la main dans _matListPoints
        //matBin[L,X,Y] : la matrice 3D des images binaires à traiter 
        //matListPoints[X,Y] : la matrice dans laquelle les points du contour de la main vont être placés
        //matColor[C,X,Y] : la matrice 2D à afficher
        //layerBin : la valeur de selection de la matrice 2D dans matBin
        private void Filter(Hand handRight, Hand handLeft, int layerBin)
        {
            int x1, x2, x3, x4, x5, x6, x7, x8;
            int pixelLeftCount = 0;int pixelRightCount = 0;
            // n |X1|n
            // X4|c |X2
            // n |X3|n 

            //Parcours de toutes les cellules de la matrice de l'image de profondeur à afficher
            //les bornes sont limitées pour appliquer le traitement sans dépassement de limites
            for (int i = 1; i < Values.DISPLAY_WIDTH-1; i++)
            {
                for (int j = 1; j < Values.DISPLAY_HEIGHT-1; j++)
                {
                    //c prend la valeur de l'image binaire traitée précédemment
                    //Attention il faut avoir traité matDepth au moins une fois sinon il y a une sortie de limites du tableau
                    //donc layerBin doit être strictement supérieur à zéro
                    int c = matBin[layerBin - 1, i, j];

                    //seuls les cellules où la profondeur est différente de 0 sont traitées
                    if (c != 0)
                    {
                        x1 = matBin[layerBin - 1, i, j - 1]; x3 = matBin[layerBin - 1, i, j + 1];x2 = matBin[layerBin - 1, i + 1, j]; x4 = matBin[layerBin - 1, i - 1, j];
                        if (layerBin == 1)
                            c = Smooth(x1, x2, x3, x4, c);
                        else if (layerBin == 2)
                        {
                            x5 = matBin[layerBin - 1, i - 1, j - 1]; x6 = matBin[layerBin - 1, i + 1, j - 1]; x7 = matBin[layerBin - 1, i - 1, j + 1]; x8 = matBin[layerBin - 1, i + 1, j + 1];
                            c = Erosion(x1, x2, x3, x4, x5, x6, x7, x8, c);
                        }
                        else if (layerBin == 3)
                        {
                            c = EnvelopHand(x1, x2, x3, x4, c);
                            if(c == 1)
                            {
                                pixelLeftCount++;
                                if (pixelLeftCount == 1)
                                {
                                    xStartLeft = i;
                                    yStartLeft = j;
                                }
                            }
                            else if(c == 2)
                            {
                                pixelRightCount++;
                                if (pixelRightCount == 1)
                                {
                                    xStartRight = i;
                                    yStartRight = j;
                                }
                            }
                            matBinInt[layerBin, i, j] = (byte)(c);
                        }
                    }
                    //la valeur est enregistrée dans matBin si besoin pour un traitement de plus
                    matBin[layerBin, i, j] = (byte)(c);
                    

                }
            }
        }

        private int Parcours(Byte[,,] mat, int layer, ref int i, ref int j)
        {
            int parcour = 0;
            int c = mat[layer, i, j];
            //si le point est bien dans la matrice -> éviter les erreurs
            if (i > 0 && i < Values.DISPLAY_WIDTH-1 && j > 0 && j < Values.DISPLAY_HEIGHT-1 && c != 0)
            {
                parcour = c;
                mat[layer, i, j] = 0;
                if (mat[layer, i, j - 1] != 0)
                {
                    j--;
                }
                else if (mat[layer, i + 1, j - 1] != 0)
                {
                    i++;
                    j--;
                }
                else if (mat[layer, i + 1, j] != 0)
                {
                    i++;
                }
                else if (mat[layer, i + 1, j + 1] != 0)
                {
                    i++;
                    j++;
                }
                else if (mat[layer, i, j + 1] != 0)
                {
                    j++;
                }
                else if (mat[layer, i - 1, j + 1] != 0)
                {
                    j++;
                    i--;
                }
                else if (mat[layer, i - 1, j] != 0)
                {
                    i--;
                }
                else if (mat[layer, i - 1, j - 1] != 0)
                {
                    i--;
                    j--;
                }
                else
                    parcour = 0;
            }
            return parcour;
        }

        //crée la liste des points connus
        //matListPoints[X,Y] : la matrice contenant le contour de la main
        //listPts : la liste qui va contenir les points du contour de la main
        private void TrisPixel(Hand hand, int c, int layer, int xint, int yint)
        {
            int parcour = 0;
            int x = xint; int y = yint;
            do
            {
                if (Parcours(this.matBin, layer, ref x, ref y) == c)
                {
                    hand.SetListPts(x, y, Values.Instance.depthFrameData[x + y*512]);
                }else if (Parcours(matBinInt, layer, ref x, ref y) == c && !(x == xint  && y == yint))
                    parcour = 0;
                else
                    parcour = 1;
            } while (parcour == 0);
        }

        /*
         * La methode qui traite la depthFrame pour reconnaitre le squelette des mains d'un corps.
         * hp : la position de la main en 3D
         * depthFrame : l'image de profondeur récupérée de la kinect
         * _displayPixels : le tableau de pixels de la main à afficher 
         * _pixelsCrossSection : le tableau de pixels de l'image de coupe 
         * handSkeleton : le squelette de la main
         */ 
        public void ProcessFrameHand(Hand handLeft, Hand handRight, Body body)
        {
            int i = 0; 
            /*
             * lissage du bord de la main pour retirer le bruit et stockage dans une matrice en binaire(0= vide, 1=interieur de la main)
             * extraihandre _depthPixels de l'image de profondeur
             */ 
             
            mat.ProcessDepthFrameData(body, Values.Instance.minDepth, this.matBin);

            for (int layerBin = 1; layerBin < 4; layerBin++)
                Filter(handLeft, handRight, layerBin);

            /*
             * lisse le bord de la main pour retirer le bruit et stockage dans une matrice en binaire(0= vide, 1=interieur de la main)
             * traitement différent de la fonction d'avant pour augmenter l'efficactité
             */ 

            TrisPixel(handLeft, 1,  3, xStartLeft, yStartLeft);
            TrisPixel(handRight, 2, 3, xStartRight, yStartRight);

            Array.Clear(matBin, 0, matBin.Length);
            Array.Clear(matBinInt, 0, matBin.Length);
            /*
             * Recherche des extremités importantes à partir de la liste de points
             */ 
            recoE.SearchExtremity(handLeft);
            recoE.SearchExtremity(handRight);
            /*
             * Construction des vecteurs des doigts
             */
            recoE.FingerVector(handLeft);
            recoE.FingerVector(handRight);
        }
    }
}

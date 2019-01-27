using Microsoft.Kinect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Samples.Kinect.BodyBasics.Process;


namespace Microsoft.Samples.Kinect.BodyBasics.Treatment
{
    class MatDisplay
    {
        // la référence de l'objet Values
        private Values values = null;
        private readonly float DEPTH_REF_HAND = 80; // 8cm

        //Constructeur
        public MatDisplay()
        {
            this.values = Values.Instance;
        }


        //Traite les donnéesde la frame reçue
        //depthFrameData : un pointeur pointant sur la frame contenant les pixels
        //depthFrameDataSize : la taille en octets de la frame contenant les pixels 
        //minDepth : la profondeur minimum
        //maxDepth : la profondeur maximum
        //handDepth : la profondeur de la main
        //pixelsDepth : le tableau 1D contenant les pixels de l'image de profondeur
        public void ProcessDepthFrameData(Body body,ushort minDepth, Byte[,,] matBin)
        {
            Joint jointHandLeft = body.Joints[JointType.HandLeft];
            Joint jointHandRight = body.Joints[JointType.HandRight];
            Joint jointWristLeft = body.Joints[JointType.WristLeft];
            Joint jointWristRight = body.Joints[JointType.WristRight];
            Joint jointTipLeft = body.Joints[JointType.HandTipLeft];
            Joint jointTipRight = body.Joints[JointType.HandTipRight];
            Joint jointThumbLeft = body.Joints[JointType.ThumbLeft];
            Joint jointThumbRight = body.Joints[JointType.ThumbRight];

            DepthSpacePoint depthPointHandLeft = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointHandLeft.Position);
            DepthSpacePoint depthPointWristLeft = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointWristLeft.Position);
            DepthSpacePoint depthPointTipLeft = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointTipLeft.Position);
            DepthSpacePoint depthPointThumbLeft = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointThumbLeft.Position);

            DepthSpacePoint depthPointHandRight = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointHandRight.Position);
            DepthSpacePoint depthPointWristRight = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointWristRight.Position);
            DepthSpacePoint depthPointTipRight = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointTipRight.Position);
            DepthSpacePoint depthPointThumbRight = DataTreatment.getInstance().coordinate.MapCameraPointToDepthSpace(jointThumbRight.Position);

            float handLeftX = depthPointHandLeft.X;
            float handLeftY = depthPointHandLeft.Y;
            float wristLeftX = depthPointWristLeft.X;
            float wristLeftY = depthPointWristLeft.Y;
            float tipLeftX = depthPointTipLeft.X;
            float tipLeftY = depthPointTipLeft.Y;
            float thumbLeftX = depthPointThumbLeft.X;
            float thumbLeftY = depthPointThumbLeft.Y;

            float handRightX = depthPointHandRight.X;
            float handRightY = depthPointHandRight.Y;
            float wristRightX = depthPointWristRight.X;
            float wristRightY = depthPointWristRight.Y;
            float tipRightX = depthPointTipRight.X;
            float tipRightY = depthPointTipRight.Y;
            float thumbRightX = depthPointThumbRight.X;
            float thumbRightY = depthPointThumbRight.Y;

            bool searchForLeftHand =  !float.IsInfinity(handLeftX) && !float.IsInfinity(handLeftY) && !float.IsInfinity(wristLeftX) && !float.IsInfinity(wristLeftY) && !float.IsInfinity(tipLeftX) && !float.IsInfinity(tipLeftY) && !float.IsInfinity(thumbLeftX) && !float.IsInfinity(thumbLeftY);
            bool searchForRightHand =  !float.IsInfinity(handRightX) && !float.IsInfinity(handRightY) && !float.IsInfinity(wristRightX) && !float.IsInfinity(wristRightY) && !float.IsInfinity(tipRightX) && !float.IsInfinity(tipRightY) && !float.IsInfinity(thumbRightX) && !float.IsInfinity(thumbRightY);

            if (searchForLeftHand || searchForRightHand)
            {
                double distanceLeft = searchForLeftHand ? CalculateDistance(handLeftX, handLeftY, tipLeftX, tipLeftY, thumbLeftX, thumbLeftY) : 0.0;
                double distanceRight = searchForRightHand ? CalculateDistance(handRightX, handRightY, tipRightX, tipRightY, thumbRightX, thumbRightY) : 0.0;

                int minLeftX = searchForLeftHand ? (int)(handLeftX - distanceLeft) : 0;
                int minLeftY = searchForLeftHand ? (int)(handLeftY - distanceLeft) : 0;
                int maxLeftX = searchForLeftHand ? (int)(handLeftX + distanceLeft) : 0;
                int maxLeftY = searchForLeftHand ? (int)(handLeftY + distanceLeft) : 0;

                int minRightX = searchForRightHand ? (int)(handRightX - distanceRight) : 0;
                int minRightY = searchForRightHand ? (int)(handRightY - distanceRight) : 0;
                int maxRightX = searchForRightHand ? (int)(handRightX + distanceRight) : 0;
                int maxRightY = searchForRightHand ? (int)(handRightY + distanceRight) : 0;

                float depthLeft = jointHandLeft.Position.Z * 1000; // m to mm
                float depthRight = jointHandRight.Position.Z * 1000;

                for (int i = 0; i < Values.DISPLAY_HEIGHT * Values.DISPLAY_WIDTH; ++i)
                {
                    ushort depth = DataTreatment.getInstance().depthFrameData[i];

                    int depthX = i % Values.DISPLAY_WIDTH;
                    int depthY = i / Values.DISPLAY_WIDTH;

                    bool isInBounds = depth >= minDepth && depth <= ushort.MaxValue;

                    bool conditionLeft = depth >= depthLeft - DEPTH_REF_HAND &&
                                         depth <= depthLeft + DEPTH_REF_HAND &&
                                         depthX >= minLeftX && depthX <= maxLeftX &&
                                         depthY >= minLeftY && depthY <= maxLeftY;

                    bool conditionRight = depth >= depthRight - DEPTH_REF_HAND &&
                                          depth <= depthRight + DEPTH_REF_HAND &&
                                          depthX >= minRightX && depthX <= maxRightX &&
                                          depthY >= minRightY && depthY <= maxRightY;

                    if (isInBounds && searchForLeftHand && conditionLeft)
                        matBin[0, depthX, depthY] = 1;
                    else if (isInBounds && searchForRightHand && conditionRight)
                        matBin[0, depthX, depthY] = 2;
                    else
                        matBin[0, depthX, depthY] = 0;
                }

            }
        }


        private double CalculateDistance(float handLeftX, float handLeftY, float tipLeftX, float tipLeftY, float thumbLeftX, float thumbLeftY)
        {
            double distanceLeftHandTip = Math.Sqrt(Math.Pow(tipLeftX - handLeftX, 2) + Math.Pow(tipLeftY - handLeftY, 2)) * 2;
            double distanceLeftHandThumb = Math.Sqrt(Math.Pow(thumbLeftX - handLeftX, 2) + Math.Pow(thumbLeftY - handLeftY, 2)) * 2;
            return Math.Max(distanceLeftHandTip, distanceLeftHandThumb);
        }     
    }
}

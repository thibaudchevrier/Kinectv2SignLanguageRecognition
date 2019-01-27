//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BodyBasics
{
    using System.Windows.Media.Media3D;
    using BodyBasics.Properties;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Samples.Kinect.BodyBasics.Process;
    using Microsoft.Samples.Kinect.BodyBasics.Treatment;


    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Process.OurBody Ourbody;
        private MultiSourceFrameReader reader;
        private static AutoResetEvent dataAvailable = new AutoResetEvent(false);
        private static object updateLock = new object();
        /// <summary>
        /// Font size of face property text 
        /// </summary>
        private const double DrawTextFontSize = 50;

        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 30;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroupGeneral;
        private DrawingGroup drawingGroup1;
        private DrawingGroup drawingGroup2;
        private DrawingGroup drawingGroup3;
        private DrawingGroup drawingGroup4;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSourceGeneral;
        private DrawingImage imageSource1;
        private DrawingImage imageSource2;
        private DrawingImage imageSource3;
        private DrawingImage imageSource4;


        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;


        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
       // private KinectHand[] hands = new KinectHand[12];
        Values val = Values.Instance;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// List of colors for each body tracked
        /// </summary>
        private List<Pen> bodyColors;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
           
            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // get the depth (display) extents
           val.dframeDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
           val.iframeDescription = this.kinectSensor.InfraredFrameSource.FrameDescription;

            

            // get the coordinate mapper
            this.val.coordinate = this.kinectSensor.CoordinateMapper;

            // get size of joint space
            this.displayWidth = val.dframeDescription.Width;
            this.displayHeight = val.dframeDescription.Height;

            // open the reader for the body frames
            this.reader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);

            // a bone defined as a line between two joints
            this.bones = new List<Tuple<JointType, JointType>>();

            // Torso
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

            // Right Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
            //this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
            // this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
            //this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

            //Left Arm
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
            // this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
            // this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
            // this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

            // Right Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

            // Left Leg
            this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
            this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

            // populate body colors, one for each BodyIndex
            this.bodyColors = new List<Pen>();
            
            this.bodyColors.Add(new Pen(Brushes.Red, 6));
            this.bodyColors.Add(new Pen(Brushes.Orange, 6));
            this.bodyColors.Add(new Pen(Brushes.Green, 6));
            this.bodyColors.Add(new Pen(Brushes.Blue, 6));
            this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
            this.bodyColors.Add(new Pen(Brushes.Violet, 6));
   
            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;
            // Create the drawing group we'll use for drawing
            this.drawingGroupGeneral = new DrawingGroup();
            this.drawingGroup1 = new DrawingGroup();
            this.drawingGroup2 = new DrawingGroup();
            this.drawingGroup3 = new DrawingGroup();
            this.drawingGroup4 = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSourceGeneral = new DrawingImage(this.drawingGroupGeneral);
            this.imageSource1 = new DrawingImage(this.drawingGroup1);
            this.imageSource2 = new DrawingImage(this.drawingGroup2);
            this.imageSource3 = new DrawingImage(this.drawingGroup3);
            this.imageSource4 = new DrawingImage(this.drawingGroup4);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // get the depth (display) extents
            //this.depthFrameReference = this.kinectSensor.DepthFrameSource.FrameDescription;
            //this.irFrameReference = this.kinectSensor.InfraredFrameSource.FrameDescription;

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSourceGeneral
        {
            get
            {
                return this.imageSourceGeneral;
            }
        }
        public ImageSource ImageSource1
        {
            get
            {
                return this.imageSource1;
            }
        }
        public ImageSource ImageSource2
        {
            get
            {
                return this.imageSource2;
            }
        }
        public ImageSource ImageSource3
        {
            get
            {
                return this.imageSource3;
            }
        }
        public ImageSource ImageSource4
        {
            get
            {
                return this.imageSource4;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.reader != null)
            {
                reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.reader != null)
            {
                // BodyFrameReader is IDisposable
                this.reader.Dispose();
                this.reader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            bool dataReceived = false;



            if (e.FrameReference.AcquireFrame() != null)
            {
                using (DepthFrame depthFrame = e.FrameReference.AcquireFrame().DepthFrameReference.AcquireFrame())
                {
                    using (InfraredFrame irFrame = e.FrameReference.AcquireFrame().InfraredFrameReference.AcquireFrame())
                    {
                        using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame().BodyFrameReference.AcquireFrame())
                        {

                            if ((depthFrame != null) && (irFrame != null))
                            {
                                FrameDescription depthFrameDescription = depthFrame.FrameDescription;
                                FrameDescription irFrameDescription = irFrame.FrameDescription;

                                int depthWidth = depthFrameDescription.Width;
                                int depthHeight = depthFrameDescription.Height;
                                int irWidth = irFrameDescription.Width;
                                int irHeight = irFrameDescription.Height;

                                // verify data and write the new registered frame data to the display bitmap
                                if (((depthWidth * depthHeight) == val.depthFrameData.Length) &&
                                        ((irWidth * irHeight) == val.irFrameData.Length))
                                {
                                    val.minDepth = depthFrame.DepthMinReliableDistance;
                                    val.maxDepth = depthFrame.DepthMaxReliableDistance;
                                    irFrame.CopyFrameDataToArray(val.irFrameData);
                                    depthFrame.CopyFrameDataToArray(val.depthFrameData);
                                }

                                if (bodyFrame != null)
                                {
                                    if (this.bodies == null)
                                    {
                                        this.bodies = new Body[bodyFrame.BodyCount];
                                    }
                                    /*
                                     * The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                                     * As long as those body objects are not disposed and not set to null in the array,
                                     * those body objects will be re-used.
                                     */
                                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                                    dataReceived = true;
                                }
                            }
                        }
                    }
                }
            }

            if (dataReceived)
            {

                using (DrawingContext dcGeneral = this.drawingGroupGeneral.Open())
                {
                    using (DrawingContext dc1 = this.drawingGroup1.Open())
                    {
                        using (DrawingContext dc2 = this.drawingGroup2.Open())
                        {
                            using (DrawingContext dc3 = this.drawingGroup3.Open())
                            {
                                using (DrawingContext dc4 = this.drawingGroup4.Open())
                                {
                                    /*
                                     * Draw a transparent background to set the render size
                                     */
                                    dcGeneral.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                                    dc1.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                                    dc2.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                                    dc3.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                                    dc4.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                                    int penIndex = 0;
                                    foreach (Body body in this.bodies)
                                    {
                                        Pen drawPen = this.bodyColors[penIndex++];

                                        if (body.IsTracked)
                                        {
                                            Ourbody = new OurBody(body);
                                            Ourbody.GetAndRefreshHandData();

                                            this.drawClippedEdges(body, dcGeneral);

                                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;
                                            // convert the joint points to depth (display) space
                                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                                            foreach (JointType jointType in joints.Keys)
                                            {
                                                // sometimes the depth(Z) of an inferred joint may show as negative
                                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                                CameraSpacePoint position = joints[jointType].Position;
                                                if (position.Z < 0)
                                                {
                                                    position.Z = InferredZPositionClamp;
                                                }
                                                DepthSpacePoint depthSpacePoint = this.val.coordinate.MapCameraPointToDepthSpace(position);
                                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                                            }
                                            this.drawBody(joints, jointPoints, dcGeneral, drawPen);
                                            this.drawJointureFinger(dcGeneral, drawPen);
                                            //this.drawHand(dcGeneral, drawPen);
                                            //this.drawExtremityHand(dc2, drawPen);
                                            // this.drawVectorFinger(dc4, drawPen);
                                            // this.drawAll(joints, jointPoints, dc3, drawPen);

                                            /* Clean all lists */
                                            Ourbody.handLeft.CleanListHand();
                                            Ourbody.handRight.CleanListHand();
                                            jointPoints.Clear();
                                           
                                        }
                                    }
                                    this.drawingGroup4.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                                }
                                this.drawingGroup3.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                            }
                            this.drawingGroup2.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                        }
                        this.drawingGroup1.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                    }
                    // prevent drawing outside of our render area
                    this.drawingGroupGeneral.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }   
            }

        }

        /*
         * ****************************************************************
         * Methods drawing
         * - Body 
         * - Hand contour + extremity 
         * - Fingers' details 
         * 
         * ****************************************************************
         */ 
        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void drawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            /* Draw the bones */
            foreach (var bone in this.bones)
            {
                this.drawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            /* Draw the joints */
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {
                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }
        }

        private void drawHand( DrawingContext drawingContext, Pen drawingPen)
        {
            List<Point3D> listeContour = new List<Point3D>();
            listeContour.AddRange(Ourbody.handLeft.listPts);
            listeContour.AddRange(Ourbody.handRight.listPts);

            foreach (Point3D point in listeContour)
            {
                drawingContext.DrawEllipse(Brushes.Blue, null, val.Conv3Dto2D(point), 1, 1);
            }
            listeContour.Clear();
            /*
             * ****************************************************************
             * Mode affichage de la main gauche puis la main droite. 
             * ****************************************************************
             * foreach (Point point in Ourbody.HandLeft.listPts)
             *      drawingContext.DrawEllipse(Brushes.Blue, null, point, 1, 1);
             * foreach (Point point in Ourbody.HandRight.listPts)
             *      drawingContext.DrawEllipse(Brushes.Blue, null, point, 1, 1);
             * ****************************************************************
             */
        }

        /*
         * Methode pour dessiner les extremités des mains.
         */ 
        private void drawExtremityHand(DrawingContext drawingContext, Pen drawingPen)
        {
            foreach (Hand.StructHand s_hand in Ourbody.handRight.listStruct)
            {
                if (s_hand.etat == "extremity")
                {
                    drawingContext.DrawEllipse(Brushes.Yellow, null, val.Conv3Dto2D(s_hand.extremityHollow), 3, 3);
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.Fuchsia, null, val.Conv3Dto2D(s_hand.extremityHollow), 3, 3);
                }
            }
            foreach (Hand.StructHand point in Ourbody.handLeft.listStruct)
            {
                if (point.etat == "extremity")
                {
                    drawingContext.DrawEllipse(Brushes.Yellow, null, val.Conv3Dto2D(point.extremityHollow), 3, 3);
                }
                else
                {
                    drawingContext.DrawEllipse(Brushes.Fuchsia, null, val.Conv3Dto2D(point.extremityHollow), 3, 3);
                }
            }
        }

        /*
         * Methode pour dessienr les jointures des mains. 
         */ 
        private void drawJointureFinger(DrawingContext drawingContext, Pen drawingPen)
        {
            foreach (Hand.StructHand s_hand in Ourbody.handRight.listStruct)
            {
                foreach (Finger.StructFinger s_finger in Ourbody.handRight.fingerHand)
                {
                    drawingContext.DrawEllipse(Brushes.Gold, null, val.Conv3Dto2D(s_finger.extremity), 3, 3);
                    drawingContext.DrawEllipse(Brushes.Red, null, val.Conv3Dto2D(s_finger.Jointure1), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Red, null, val.Conv3Dto2D(s_finger.Jointure2), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Gold, null, val.Conv3Dto2D(s_finger.otherExtremity), 3, 3);
                }
            }
            foreach (Hand.StructHand point in Ourbody.handLeft.listStruct)
            {
                foreach (Finger.StructFinger s_finger in Ourbody.handLeft.fingerHand)
                {
                    drawingContext.DrawEllipse(Brushes.Gold, null, val.Conv3Dto2D(s_finger.extremity), 3, 3);
                    drawingContext.DrawEllipse(Brushes.Red, null, val.Conv3Dto2D(s_finger.Jointure1), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Red, null, val.Conv3Dto2D(s_finger.Jointure2), 2, 2);
                    drawingContext.DrawEllipse(Brushes.Gold, null, val.Conv3Dto2D(s_finger.otherExtremity), 3, 3);
                }
            }
        }

        /*
        * Methode pour dessiner les vecteurs des mains. 
        */
        private void drawVectorFinger(DrawingContext drawingContext, Pen drawingPen)
        {
            foreach (Hand.StructHand s_hand in Ourbody.handRight.listStruct)
            {
                foreach (Finger.StructFinger s_finger in Ourbody.handRight.fingerHand)
                {
                    drawingContext.DrawLine(bodyColors[1], val.Conv3Dto2D(s_finger.otherExtremity), val.Conv3Dto2D(s_finger.Jointure2));
                    drawingContext.DrawLine(bodyColors[2], val.Conv3Dto2D(s_finger.Jointure2), val.Conv3Dto2D(s_finger.Jointure1));
                    drawingContext.DrawLine(bodyColors[3], val.Conv3Dto2D(s_finger.Jointure1), val.Conv3Dto2D(s_finger.extremity));
                }
            }
            foreach (Hand.StructHand point in Ourbody.handLeft.listStruct)
            {
                foreach (Finger.StructFinger s_finger in Ourbody.handLeft.fingerHand)
                {
                    drawingContext.DrawLine(bodyColors[1], val.Conv3Dto2D(s_finger.otherExtremity), val.Conv3Dto2D(s_finger.Jointure2));
                    drawingContext.DrawLine(bodyColors[2], val.Conv3Dto2D(s_finger.Jointure2), val.Conv3Dto2D(s_finger.Jointure1));
                    drawingContext.DrawLine(bodyColors[3], val.Conv3Dto2D(s_finger.Jointure1), val.Conv3Dto2D(s_finger.extremity));
                }
            }
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void drawAll(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            this.drawBody(joints, jointPoints, drawingContext, drawingPen);
            this.drawJointureFinger(drawingContext, drawingPen);
        }


        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void drawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);

        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void drawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }
    }

}

using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.BodyBasics.Treatment
{
    class DataTreatment
    {

        public FrameDescription DframeDescription { get; set; }
        public FrameDescription IframeDescription { get; set; }
        public CoordinateMapper coordinate { get; set; }
        public ushort[] depthFrameData { get; set; }
        public ushort[] irFrameData { get; set; }
        public ushort minDepth { get; set; }
        public int maxDepth { get; set; }


        private static DataTreatment instance = null;

        public static DataTreatment getInstance()
        {
                if (instance == null)
                {
                    instance = new DataTreatment();
                }
                return instance;
        }

        private DataTreatment()
        {
            this.depthFrameData = new ushort[424*512];
            this.irFrameData = new ushort[424 * 512];
            coordinate = null;
            DframeDescription = null;
            IframeDescription = null;
            minDepth = 0;
            maxDepth = 0;
        }

    }
}

using Microsoft.Kinect;
using System;
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
    public class OurBody
    {
        private static Treatment.RecoHand recoHand = new RecoHand();
        private Body body = null;
        /// <summary>
        /// The left hand data of the current body.
        /// </summary>
        public Hand HandLeft = new Hand();

        /// <summary>
        /// The right hand data of the current body.
        /// </summary>
        public Hand HandRight = new Hand();


        public OurBody(Body body) {
            this.body = body;
    }

        /*
         * Rafraîchir la position des mains d'un body 
         */
        public void GetAndRefreshHandData()
        {
            recoHand.processFrameHand(HandLeft, HandRight, body);
        }
    }


}

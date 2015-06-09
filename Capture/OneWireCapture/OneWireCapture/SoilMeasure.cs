using System;
using Microsoft.SPOT;
using OneWireCapture.Sensors;

namespace OneWireCapture
{
   

    public class SoilMeasure 
    {
        public Measure Temperature { get; set; }
        public Measure Resitivity { get; set; }
    }
}

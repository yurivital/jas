using System;
using Microsoft.SPOT;
using OneWireCapture.Sensors;

namespace OneWireCapture
{
    /// <summary>
    /// Represent an atmospherique conditions datas
    /// </summary>
    public class AirMeasure
    {
        /// <summary>
        /// Create a new instance of <see cref="AirMeasure"/>
        /// </summary>
        public AirMeasure()
        {
            Temperature = new Measure();
            Humidity = new Measure();
            WindSpeed = new Measure();
            Luminosity = new Measure();
        }
        /// <summary>
        /// Temperature of the air, in degee Celcius
        /// </summary>
        public Measure Temperature { get; set; }
        /// <summary>
        /// Relative Humidity of the air in percent (0-1)
        /// </summary>
        public Measure Humidity { get; set; }
        /// <summary>
        /// Speed the deplacing air in m.s^-1
        /// </summary>
        public Measure WindSpeed { get; set; }
        /// <summary>
        /// Luminosity in lumen
        /// </summary>
        public Measure Luminosity { get; set; }
    }
}

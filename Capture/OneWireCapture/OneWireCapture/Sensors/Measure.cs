using System;
using Microsoft.SPOT;

namespace OneWireCapture.Sensors
{
    /// <summary>
    /// Define one measure
    /// </summary>
    public class Measure
    {
        /// <summary>
        /// Sensor identification
        /// </summary>
        public String SensorId { get; set; }
        /// <summary>
        /// Value
        /// </summary>
        public float value { get; set; }
        /// <summary>
        /// timestamp of the value
        /// </summary>
        public DateTime timestamp { get; set; }
    }
}

using System;
using System.Text;

namespace OneWireCapture.Sensors
{
    /// <summary>
    /// Define a behavior of a Temperature Sensor
    /// </summary>
    public interface ITemperatureSensor
    {
        /// <summary>
        /// Get the last value of measured temperature in °C
        /// </summary>
        float Temperature { get; }
    }
}

using System;
using Microsoft.SPOT;
using OneWireCapture.OneWire;
using OneWireCapture.OneWire.Sensors;

namespace OneWireCapture.OneWire.Sensors
{
    /// <summary>
    /// Provider of MaximIntegrated sensors
    /// </summary>
   public static class MaximIntegratedSensorProvider
    {
       /// <summary>
       /// Create an instance of OneWireSensor
       /// </summary>
       /// <param name="adresse">Address of the sensor</param>
        /// <returns>Instance of sensor implementing <see cref="IOneWireSensor"/></returns>
       public static IOneWireSensor Create(RomAddress adresse)
       {
           DS18S20 sensor = new DS18S20(adresse);
           return sensor;
       }
    }
}

using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.IO;

namespace OneWireCapture
{
    /// <summary>
    /// Provide configuraiton settings 
    /// </summary>
    public class Config
    {
        /// <summary>
        /// store the instance of <see cref="Config"/>
        /// </summary>
        public static Config Instance = new Config();

        /// <summary>
        /// Get or set the Identifier of the captor inside the green house
        /// </summary>
        public string InnerTemperatureSensorId { get; set; }
        /// <summary>
        /// Get or set the Identifier of the captor outside the green house
        /// </summary>
        public string OuterTemperatureSensorId { get; set; }
        /// <summary>
        /// Get or set the Identifier of the captor in the soil
        /// </summary>
        public string SoilTemperatureSensorId { get; set; }

        /// <summary>
        /// Load the configuration
        /// </summary>
        public void Load()
        {
            bool HaveSd =  PersistentStorage.DetectSDCard();

            // Demo config
            if (!HaveSd)
            {
                InnerTemperatureSensorId = "Demo";
            }
        }
    }
}

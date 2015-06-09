using System;

namespace OneWireCapture.OneWire.Sensors
{
    /// <summary>
    /// Define the behavior of an OneWire Sensor
    /// </summary>
    public interface IOneWireSensor
    {
        /// <summary>
        /// Adresse of the sensor
        /// </summary>
        RomAddress Address
        {
            get;
        }

        /// <summary>
        /// Get or set the broadcast mode.
        /// </summary>
        bool BroadCastMode
        {
            get;
            set;
        }

         /// <summary>
        /// Get or Set the value that indicate if the sensor need to perfom a conversion
        /// </summary>
         bool SendConversionOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Associate the device with the OneWire Bus
        /// </summary>
        void Connect(OneWireNetwork network);

        /// <summary>
        /// Remove association between the sensor and the OneWire Bus
        /// </summary>
        void Disconnect();
        
        /// <summary>
        /// Send order on the bus
        /// </summary>
        void SendOrder();

        /// <summary>
        /// Read data from the device on the bus
        /// </summary>
        void ReadResult();
        
    }
}

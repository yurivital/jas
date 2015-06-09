using System;
using System.Text;
using System.Threading;
using OneWireCapture.Sensors;

namespace OneWireCapture.OneWire.Sensors
{
    /// <summary>
    /// Define a beavior of the MaximIntegrated DS18S20 temperature sensor chip
    /// </summary>
    public class DS18S20 : OneWireSensor, ITemperatureSensor
    {
        /// <summary>
        /// Temperature Acquisition order
        /// </summary>
        private const byte CONVERSION_ORDER = 0x44;
        private const byte READ_ORDER = 0xBE;
        /// <summary>
        /// store the value of the last Measured temperature
        /// </summary>
        private float _measuredTemperature;

        /// <summary>
        /// Get the last value of measured temperature in °C
        /// </summary>
        public float Temperature
        {
            get { return this._measuredTemperature; }
        }


        /// <summary>
        /// Create a new instance of <see cref="DS18S20"/>
        /// </summary>
        /// <param name="address">Rom adresse of the sensor</param>
        public DS18S20(RomAddress address)
            : base(address)
        {
        }

        /// <summary>
        /// Send an order in the Onewire bus
        /// </summary>
        public override void SendOrder()
        {
            if (BroadCastMode)
            {
                this._network.Broadcast();
            }
            else
            {
                this._network.Reset();
                this._network.Select(this._address);
            }
            this._network.Write(CONVERSION_ORDER);
            while (this._network.Read() == 0) ;
        }

        /// <summary>
        /// Read the value stored in the sensor
        /// </summary>
        public override void ReadResult()
        {
            byte[] dataBuffer = new byte[9];

            this._network.Reset();
            this._network.Select(this._address);
            this._network.Write(READ_ORDER);
            this._network.Read(dataBuffer, 0, dataBuffer.Length);

            //Convert the raw value
            unchecked
            {
                short raw = (short)(dataBuffer[1] << 8 | dataBuffer[0]);
                raw = (short)(raw << 3);
                if (dataBuffer[7] == 0x10)
                {
                    raw = (short)((raw & 0xFFF0) + 12 - dataBuffer[6]);
                }

                this._measuredTemperature  = raw / 16f;
            }
        }
    }
}

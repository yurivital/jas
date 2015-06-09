using System;
using OneWireCapture.OneWire.Sensors;
using OneWireCapture.OneWire;

namespace OneWireCapture.Sensors
{
    /// <summary>
    ///  Provide base fonctionalities of an Sensor on OneWire bus
    /// </summary>
    public abstract class OneWireSensor : IOneWireSensor
    {
        /// <summary>
        /// Store the ROM Regisity Adress of the Sensor
        /// </summary>
        protected RomAddress _address;
        /// <summary>
        /// Store the instance of the OneWire Network
        /// </summary>
        protected OneWireNetwork _network;
        /// <summary>
        /// Store the value that indicate the broadcast mode should be used by the device
        /// </summary>
        protected bool  _broadcastMode;

        /// <summary>
        /// Get or set the value that indicate the broadcast mode should be used by the device
        /// </summary>
        public virtual bool BroadCastMode
        {
            get
            {
                return _broadcastMode;
            }
            set
            {
                _broadcastMode = value;
            }
        }

        /// <summary>
        /// Store the value if the sensor need to send a conversion order
        /// </summary>
        private bool _sendConversionOrder;

        /// <summary>
        /// Get or Set the value that indicate if the sensor need to perfom a conversion
        /// </summary>
        public bool SendConversionOrder
        {
            get { return _sendConversionOrder; }
            set { _sendConversionOrder = value; }
        }
        /// <summary>
        /// Get the ROM Adresse of the sensor
        /// </summary>
        public virtual RomAddress Address
        {
            get
            {
                return _address;
            }
        }

       /// <summary>
        /// Create a new instance of <see cref="OneWireSensor"/>
       /// </summary>
       /// <param name="address"></param>
        public OneWireSensor(RomAddress address)
        {
            this._address = address;
        }

        /// <summary>
        /// Associate the sensor and the network
        /// </summary>
        /// <param name="network">Instance of <see cref="OneWireNetwork"/></param>
        public virtual  void Connect(OneWireNetwork network)
        {
            if (network == null)
            {
                throw new ArgumentNullException("network");
            }
            this._network = network;
        }

        /// <summary>
        /// Remove association between sensor and the network
        /// </summary>
        public  virtual void Disconnect()
        {
            this._network = null;
        }

        /// <summary>
        /// Send a order to the device. If broadcast mode is used, the device set the network to Broadcast.
        /// Otherwire, set the network in a selected state for him.
        /// </summary>
        public abstract void SendOrder();
        
        /// <summary>
        /// Read value store in the device
        /// </summary>
        public abstract void ReadResult();
        
    }
}

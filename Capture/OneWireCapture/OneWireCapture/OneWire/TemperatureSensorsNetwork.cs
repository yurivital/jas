using System;
using Microsoft.SPOT;
using OneWireCapture.OneWire.Sensors;
using OneWireCapture.Sensors;
using Microsoft.SPOT.Hardware;

namespace OneWireCapture.OneWire
{
    /// <summary>
    /// Temperature specialized OneWire Network
    /// </summary>
    public class TemperatureSensorsNetwork : OneWireNetwork
    {
        /// <summary>
        /// Store the references of found sensors
        /// </summary>
        private IOneWireSensor[] sensors;

        /// <summary>
        /// Get the date and time of the last perfomed network discovery action.
        /// </summary>
        public DateTime LastDiscovery { get; private set; }

        /// <summary>
        /// Create a new instance of <see cref="TemperatureSensorsNetwork"/>
        /// </summary>
        /// <param name="pin">Pin numner of data wire</param>
        public TemperatureSensorsNetwork(Cpu.Pin pin)
            : base(pin)
        {
            LastDiscovery = DateTime.MinValue;
        }

        /// <summary>
        /// Disconnect all attached sensors
        /// </summary>
        private void Cleanup()
        {
            if (sensors == null)
            {
                return;
            }

            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].Disconnect();
            }
        }

        /// <summary>
        /// Perform network discovery and configure found sensors
        /// </summary>
        public void Initialize()
        {
            // Cleanup
            Cleanup();
         
            RomAddress[] adresses = this.Discover();
            sensors = new IOneWireSensor[adresses.Length];
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i] = MaximIntegratedSensorProvider.Create(adresses[i]);
                sensors[i].Connect(this);
                if (i == 0)
                {
                    // The first one send conversion Order for all
                    sensors[i].SendConversionOrder = true;
                    sensors[i].BroadCastMode = true;
                }
                else
                {
                    sensors[i].SendConversionOrder = false;
                    sensors[i].BroadCastMode = false;
                }
            }
            LastDiscovery = DateTime.Now;
        }

        /// <summary>
        /// Perfom an conversion / reading cycle
        /// </summary>
        /// <returns>Array of value measured</returns>
        public Measure[] GetTemperature()
        {
            if (sensors == null)
            {
                return new Measure[0];
            }

            Measure[] measures = new Measure[sensors.Length];

            DateTime acquisitionDate = DateTime.Now;

            for (int i = 0; i < sensors.Length; i++)
            {
                IOneWireSensor sensor = sensors[i];
                sensor.SendOrder();
                sensor.ReadResult();
            }

            for (int i = 0; i < sensors.Length; i++)
            {
                IOneWireSensor sensor = sensors[i];
                ITemperatureSensor tempSensor = sensor as ITemperatureSensor;
                measures[i] = new Measure();
                measures[i].SensorId = sensor.Address.ToString();
                measures[i].timestamp = acquisitionDate;
                measures[i].value = tempSensor.Temperature;
            }
            return measures;
        }
    }
}

using GHIElectronics.NETMF.Hardware;
using OneWireCapture.Actuator;
using System;
using GHIElectronics.NETMF.SQLite;
using OneWireCapture.OneWire;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.FEZ;
using OneWireCapture.Sensors;
using System.Xml;
using GHIElectronics.NETMF.IO;

namespace OneWireCapture
{
    /// <summary>
    /// provide a comprehensive Greenhouse management
    /// </summary>

    public sealed class GreenHouse
    {
        /// <summary>
        /// Store the instance of the motorized Window
        /// </summary>
        private MotorizedWindow _window;

        /// <summary>
        /// Get the values of the conditions inside the greenhouse
        /// </summary>
        public AirMeasure InnerCondition { get; private set; }
        /// <summary>
        /// Get the values of the conditions outside the greenhouse
        /// </summary>
        public AirMeasure OuterCondition { get; private set; }
        /// <summary>
        /// Get th value of the confiitons in the soil
        /// </summary>
        public SoilMeasure SoilCondition { get; private set; }


        /// <summary>
        /// Creat a new instance of <see cref="GreenHouse""/>
        /// </summary>
        public GreenHouse()
        {
            _window = new MotorizedWindow(PWM.Pin.PWM1);
            InnerCondition = new AirMeasure();
            OuterCondition = new AirMeasure();
            SoilCondition = new SoilMeasure();
        }

        /// <summary>
        /// Perform measures and Regulate the greenhouse
        /// </summary>
        public void Regulate()
        {
            RegulateInnerCondition();
        }

        /// <summary>
        /// Add a new Temperature measure value
        /// </summary>
        /// <param name="temperature">temperature</param>
        public void AddTemperature(Measure[] temperatures)
        {
            if (temperatures == null)
            {
                return;
            }

            foreach (Measure temperature in temperatures)
            {
                if (temperature.SensorId == Config.Instance.InnerTemperatureSensorId)
                {
                    InnerCondition.Temperature = temperature;
                }

                if (temperature.SensorId == Config.Instance.OuterTemperatureSensorId)
                {
                    OuterCondition.Temperature = temperature;
                }

                if (temperature.SensorId == Config.Instance.SoilTemperatureSensorId)
                {
                    SoilCondition.Temperature = temperature;
                }
            }
        }

        /// <summary>
        /// Perfom the regulation of the temperature and the moisture of the greenhouse
        /// </summary>
        protected void RegulateInnerCondition()
        {
            // open the window for 25 -> 35 *C
            int baseTemp = (int) InnerCondition.Temperature.value - 25;
            if (baseTemp < 0) baseTemp = 0;
            if (baseTemp > 10) baseTemp = 10;

            int angleTemperature =(int) baseTemp * 10;

            int baseMoisture = (int) InnerCondition.Humidity.value - 70;
            if (baseMoisture < 0) baseMoisture = 0;
            if (baseMoisture > 30) baseMoisture = 30;
            int angleHumidity = (int)baseMoisture * 100 / 30;

             int angle =  System.Math.Max(angleHumidity, angleTemperature);

             _window.Open((ushort)angle);
        }
    }
}

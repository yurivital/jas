using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using GHIElectronics.NETMF.Hardware;

namespace OneWireCapture.Actuator
{
    /// <summary>
    /// Provide a Servomotor driver
    /// </summary>
    public class Servomotor : IDisposable
    {

        /// <summary>
        /// PWM port
        /// </summary>
        private PWM pwmPort;

        /// <summary>
        /// Create a new instance of <see cref="Servomotor"/>
        /// </summary>
        /// <param name="pwmCapablePin">CPU pin with pwm capability</param>
        public Servomotor(PWM.Pin pwmCapablePin)
        {

            pwmPort = new PWM(pwmCapablePin);
        }

        /// <summary>
        /// Store angle value
        /// </summary>
        private float _angle;

        /// <summary>
        /// Get or set the angle of the torque
        /// </summary>
        public float Angle
        {
            get
            {
                return _angle;
            }

            set
            {
                this._angle = value;
                Rotate();
            }
        }

        /// <summary>
        /// Send rotation order to the Servomotor
        /// </summary>
        private void Rotate()
        {
            //   20 000 000; - 20 ms of period
            //    1 250 000; - 1.25 ms  uptime implie a torque of 0°
            uint period = 20000000;
            uint highTime = (uint)(900 + ((2100 - 900) * _angle / 180)) * 1000;
            pwmPort.SetPulse(period, highTime);
        }

        /// <summary>
        /// Release PWM ressource
        /// </summary>
        public void Dispose()
        {
            pwmPort.Dispose();
        }
    }
}

using System;
using GHIElectronics.NETMF.Hardware;
using OneWireCapture.Actuator;

namespace OneWireCapture
{
    /// <summary>
    /// Manage the opening of a servo controlled window
    /// </summary>
    public class MotorizedWindow : IDisposable
    {
        private Servomotor _window;
        public MotorizedWindow(PWM.Pin pwmPin)
        {
            _window = new Servomotor(pwmPin);
        }

        /// <summary>
        /// Shut the windows, équivalent to set Angle = 0
        /// </summary>
        public void Close()
        {
            _window.Angle = 0;
        }

        /// <summary>
        /// Open the window
        /// </summary>
        /// <param name="percentage">A percentage value between 0 an 100</param>
        public void Open(ushort percentage)
        {
            if (percentage > 100) percentage = 100;
            
            _window.Angle = percentage * 1.8f;
        }

        /// <summary>
        /// Release the Servomotor ressource
        /// </summary>
        public void Dispose()
        {
            _window.Dispose();
        }
    }
}

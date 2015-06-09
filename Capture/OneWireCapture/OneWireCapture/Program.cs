using System;
using System.Threading;
using GHIElectronics.NETMF.FEZ;
using JasCapture.Form;
using Microsoft.SPOT.Hardware;
using OneWireCapture.OneWire;
using OneWireCapture.Sensors;


namespace OneWireCapture
{
    /// <summary>
    /// Program Entry Point
    /// </summary>
    public class Program
    {
        /// <summary>
        /// store the data link pin number for oneWire Bus
        /// </summary>
        private const Cpu.Pin oneWireDataPin = (Cpu.Pin)FEZ_Pin.Digital.Di13;

        /// <summary>
        /// store the lcd driver instance
        /// </summary>
        static FEZ_Components.FEZTouch lcd;

        /// <summary>
        /// Store the value that indicate if the screen must be on (true) or off (false)
        /// </summary>
        static bool screenActive = true;

        /// <summary>
        /// Store the temperature network driver instance
        /// </summary>
        static TemperatureSensorsNetwork temperatureBus;
        /// <summary>
        /// Store the greenhouse management instance
        /// </summary>
        static GreenHouse greenhouse;
        /// <summary>
        /// Store the temperature form instance
        /// </summary>
        static TemperatureForm temperatureForm = new TemperatureForm();
        /// <summary>
        /// store the cadencing timer instance
        /// </summary>
        private static Timer timer;
        /// <summary>
        /// Store the time state object.
        /// <remarks>This object is only for the timer callback</remarks>
        /// </summary>
        private static Object timerState = new object();

        static int demoTemperature = 24;

        /// <summary>
        /// Set-up the Lcd driver
        /// </summary>
        public static void InitializeLcd()
        {
            // Lcd wiring
            FEZ_Components.FEZTouch.LCDConfiguration lcdConfig = new FEZ_Components.FEZTouch.LCDConfiguration(
                FEZ_Pin.Digital.Di28,
                FEZ_Pin.Digital.Di20,
                FEZ_Pin.Digital.Di22,
                FEZ_Pin.Digital.Di23,
                new FEZ_Pin.Digital[8] { FEZ_Pin.Digital.Di51, FEZ_Pin.Digital.Di50, FEZ_Pin.Digital.Di49, FEZ_Pin.Digital.Di48, FEZ_Pin.Digital.Di47, FEZ_Pin.Digital.Di46, FEZ_Pin.Digital.Di45, FEZ_Pin.Digital.Di44 },
                FEZ_Pin.Digital.Di24,
                FEZ_Pin.Digital.Di26);
            // Touch Wiring
            FEZ_Components.FEZTouch.TouchConfiguration touchConfig = new FEZ_Components.FEZTouch.TouchConfiguration(
                SPI.SPI_module.SPI2, FEZ_Pin.Digital.Di25, FEZ_Pin.Digital.Di34);
            // create driver instance
            lcd = new FEZ_Components.FEZTouch(lcdConfig, touchConfig);

            // Capture touch event
            lcd.TouchUpEvent += new FEZ_Components.FEZTouch.TouchEventHandler(lcd_TouchUpEvent);
        }

        /// <summary>
        /// Event handler of the touch sceen
        /// </summary>
        /// <param name="x">X touched position</param>
        /// <param name="y">Y touched position</param>
        static void lcd_TouchUpEvent(int x, int y)
        {
            if (screenActive)
                lcd.SetBrightness(0);
            else
                lcd.SetBrightness(100);

            screenActive = !screenActive;
        }

        /// <summary>
        /// Display the splashscreen
        /// </summary>
        private static void ShowSplash()
        {
            SplashForm empty = new SplashForm();
            empty.Title = "Jupiter Active Sensor";
            empty.BackgroundColor = FEZ_Components.FEZTouch.Color.White;
            empty.Paint(lcd);
            System.Threading.Thread.Sleep(2000);
        }

        /// <summary>
        /// Temperature conversion and display
        /// </summary>
        /// <param name="state">state objet required by the timer delegate</param>
        public static void TemperaturesCapture(object state)
        {
            Measure[] measures = temperatureBus.GetTemperature();
            // No sensor = Demo mode
            if (measures.Length == 0)
            {
                Random rand = new Random();
                measures = new Measure[1];
                measures[0] = new Measure();
                measures[0].SensorId = Config.Instance.InnerTemperatureSensorId;
                measures[0].value = (++demoTemperature % 48); 
                measures[0].timestamp = DateTime.Now;
            }
            greenhouse.AddTemperature(measures);
            temperatureForm.AddMeasures(measures);
            temperatureForm.Paint(lcd);
        }


        /// <summary>
        /// Entry method. Called on at the program start-up
        /// </summary>
        public static void Main()
        {
            InitializeLcd();

            //ShowSplash();
            temperatureBus = new TemperatureSensorsNetwork(oneWireDataPin);
            temperatureBus.Initialize();
            Config.Instance.Load();
            greenhouse = new GreenHouse();

            bool ledState = false;
            Cpu.Pin led = (Cpu.Pin)FEZ_Pin.Digital.LED;
            OutputPort ledPort = new OutputPort(led, ledState);
               
            timer = new Timer((TimerCallback)TemperaturesCapture, 
                timerState,
                TimeSpan.Zero, 
                new TimeSpan(0, 0, 10));

            while (true)
            {
                greenhouse.Regulate();
                ledPort.Write(ledState);
                ledState = !ledState ;
                Thread.Sleep(1000);
            }
        }
    }
}

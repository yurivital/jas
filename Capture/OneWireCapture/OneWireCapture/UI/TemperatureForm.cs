using System;
using Microsoft.SPOT;
using JasCapture.Form.Base;
using OneWireCapture.Sensors;
using GHIElectronics.NETMF.FEZ;
using System.Collections;

namespace JasCapture.Form
{
    /// <summary>
    /// Display temperature information by text and graph
    /// </summary>
    public class TemperatureForm : GridForm
    {
        /// <summary>
        /// Store the instance of the plotter
        /// </summary>
        private GraphComponent graph;
        /// <summary>
        /// Store the instance of the viewport for the Graph
        /// </summary>
        private ViewPort graphPort;

        /// <summary>
        /// Create a new instance of <see cref="TemperatureForm"/>
        /// </summary>
        public TemperatureForm()
        {
            this.Title = "Temperatures";
            this.BackgroundColor = FEZ_Components.FEZTouch.Color.White;
            this.ForegroundColor = FEZ_Components.FEZTouch.Color.Blue;
            graph = new GraphComponent();
            graph.NbOfPoint = 100;
            graphPort = new ViewPort((IComponent)graph);

        }

        /// <summary>
        /// Draw the form to the screen
        /// </summary>
        /// <param name="screen">Display device instance</param>
        public override void Paint(IDrawer screen)
        {
            base.Paint(screen);
            graphPort.X = 0;
            graphPort.Width = screen.Width - 1;
            graphPort.Y = (int)(screen.Height * 0.4);
            graphPort.Heigth = (int)(screen.Height * 0.6) - 1;
            graphPort.Paint(screen);
        }

        /// <summary>
        /// Add a measures to the graph series and the text displays
        /// </summary>
        /// <param name="measures">Array of measures</param>
        public void AddMeasures(Measure[] measures)
        {
            if (measures.Length == 0)
            {
                this.ForegroundColor = FEZ_Components.FEZTouch.Color.Red;
                this.SetText("No data", 0);
            }
            else
            {
                this.ForegroundColor = FEZ_Components.FEZTouch.Color.Blue;
                for (int i = 0; i < measures.Length; i++)
                {
                    Measure measure = measures[i];
                    string name = measure.SensorId;
                    if (name.Length > 8)
                    {
                        name = name.Substring(0, 8);
                    }
                    string text = name + "= " + measure.value.ToString("N1");
                    text += "*C";
                    this.SetText(text, i);
                    graph.AddData(measure.SensorId, measure.value);
                    Debug.Print(measure.SensorId.ToString());
                    Debug.Print(measure.value.ToString());
                }
            }
        }
    }
}

using System;
using System.Collections;
using GHIElectronics.NETMF.FEZ;

namespace JasCapture.Form
{
    /// <summary>
    /// Draw a line graph chart
    /// </summary>
    public class GraphComponent : IComponent
    {
        /// <summary>
        /// store the value of the higher value in data series
        /// </summary>
        private float _dataMax = -120; // -120 is inferior of captor capacities 

        /// <summary>
        /// store the value of the lower value in data series
        /// </summary>
        private float _dataMin = 120; // 120 is superior of captor capacities

        /// <summary>
        /// Get or set the value of number of point displayed
        /// </summary>
        public ushort NbOfPoint { get; set; }

        /// <summary>
        /// Store the data serie indexed by serie name
        /// </summary>
        Hashtable series = new Hashtable();

        /// <summary>
        /// Append data to an data serie
        /// </summary>
        /// <param name="serieName">the name of the serie to append</param>
        /// <param name="data">Value of the data </param>
        public void AddData(String serieName, float data)
        {
            Serie currentSerie;

            // Add data serie to the dictonary
            if (!series.Contains(serieName))
            {
                currentSerie = new Serie();
                currentSerie.Name = serieName;
                currentSerie.NbOfPoint = this.NbOfPoint;
                series.Add(serieName, currentSerie);

            }
            else
            {
                currentSerie = (Serie)series[serieName];
            }

            // Add data to the serie and compute Stats
            currentSerie.Add(data);
            float value = currentSerie.Maximum;
            if (value > _dataMax)
            {
                _dataMax = value * 1.1f;
            }
            value = currentSerie.Minimum;
            if (value < _dataMin)
            {
                _dataMin = value - (int)System.Math.Abs((int)(value * 0.1f));
            }

        }

        /// <summary>
        /// Draw the graph on an device
        /// </summary>
        /// <param name="screen">Screen device</param>
        public void Draw(IDrawer screen)
        {

            if (_dataMin == _dataMax)
                _dataMax += 10;

            // Chart area
            screen.FillRectangle(0, 0, screen.Width, screen.Height, FEZ_Components.FEZTouch.Color.Black);
            // Y Axis

            if (_dataMin < 0)
            {
                // X Axis
                screen.DrawLine(0, screen.Height, screen.Width, screen.Height, GHIElectronics.NETMF.FEZ.FEZ_Components.FEZTouch.Color.Black);
            }

            foreach (var key in series.Keys)
            {
                FEZ_Components.FEZTouch.Color color = FEZ_Components.FEZTouch.Color.Green;
                Serie currentSerie = (Serie)series[key];
                int lastX = 0;
                int lastY = 0;

                for (ushort index = 0; index < currentSerie.Count; index++)
                {
                    int x = ComputePositionX(index, screen);
                    int y = ComputePositionY((float)currentSerie[index], screen);

                    // Draw line interval
                    if (index > 0)
                    {
                        lastX = ComputePositionX(index - 1, screen);
                        lastY = ComputePositionY((float)currentSerie[index - 1], screen);
                        screen.DrawLine(lastX, lastY, x, y, color);
                    }
                }
            }
        }

        /// <summary>
        /// Compute the Y pixel postion for an value
        /// </summary>
        /// <param name="value">value of the position</param>
        /// <param name="screen">Device to display in</param>
        /// <returns>Y pixel position</returns>
        private int ComputePositionY(float value, IDrawer screen)
        {
            int position = 0;
            float tempRange = _dataMax - _dataMin;
            float pixelPerDegre = screen.Height / (tempRange);
            float adjustedTemp = value - _dataMin;
            position = (int)System.Math.Floor(screen.Height - (pixelPerDegre * adjustedTemp));
            if (position < 0) position = 0;
            return position;
        }

        /// <summary>
        /// Compute the X pixel postion of an data index
        /// </summary>
        /// <param name="value">value of the index</param>
        /// <param name="screen">Device to display in</param>
        /// <returns>X pixel position</returns>
        private int ComputePositionX(int index, IDrawer screen)
        {
            int position = 0;
            float pixelPerPlot = screen.Width / (NbOfPoint - 1);
            position = (int)System.Math.Floor(index * pixelPerPlot);
            if (position < 0) position = 0;
            return position;
        }
    }
}

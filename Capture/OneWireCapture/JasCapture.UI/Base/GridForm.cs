using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.FEZ;

namespace JasCapture.Form.Base
{

    /// <summary>
    /// Provide base functionnality of a grid layout form
    /// </summary>
    public abstract class GridForm : FezForm
    {
        /// <summary>
        /// Number of allowed columns
        /// </summary>
        public const int ControlColumns = 2;
        /// <summary>
        /// Number of allowed rows
        /// </summary>
        public const int ControlRows = 3;

        /// <summary>
        /// Text grid buffers
        /// </summary>
        protected String[][] Controls;

        /// <summary>
        /// Create a new instance of <see cref="GridForm"/>
        /// </summary>
        public GridForm()
        {
            Controls = new String[ControlRows][];
            for (int i = 0; i < ControlRows; i++)
            {
                Controls[i] = new String[ControlColumns];
            }
        }

        /// <summary>
        /// Write text in the grid buffer
        /// </summary>
        /// <param name="text">Text positon</param>
        /// <param name="cardinality">Text position in the buffer</param>
        public void SetText(string text, int cardinality)
        {
            int colNum = (int)(cardinality / ControlRows);
            int rowNum = cardinality;

            if (cardinality > ControlRows)
            {
                rowNum = cardinality - ControlRows;
            }

            Controls[rowNum][colNum] = text;
        }


        /// <summary>
        /// Draw the Grid
        /// </summary>
        /// <param name="screen">Screen device</param>
        public override void Paint(IDrawer screen)
        {
            base.Paint(screen);
            for (int i = 0; i < ControlRows; i++)
            {
                for (int j = 0; j < ControlColumns; j++)
                {
                    String currentControl = Controls[i][j];

                    if (currentControl != null && currentControl != String.Empty)
                    {
                        int x = xOffset + (j * xDrawable / 2);
                        int y = yOffset + (i * FEZ_Components.FEZTouch.FONT_HEIGHT + 2);
                        screen.DrawString(currentControl, x, y, this.ForegroundColor, this.BackgroundColor);
                    }
                }
            }
        }
    }
}

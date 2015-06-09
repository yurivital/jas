using System;
using Microsoft.SPOT;
using GHIElectronics.NETMF.FEZ;

namespace JasCapture.Form.Base
{
    /// <summary>
    /// Provide base functionality for creating Forms
    /// </summary>
    public abstract class FezForm
    {
        /// <summary>
        /// Store the heigth size of the title bar
        /// </summary>
        protected const int titleBarSize = 25;

        /// <summary>
        /// Get or set the margin size
        /// </summary>
        public int Margin { get; set; }

        /// <summary>
        /// Store the size of the drawable X dimension
        /// </summary>
        protected int xDrawable { get { return FEZ_Components.FEZTouch.ScreenWidth - Margin * 2; } }

        /// <summary>
        /// Store the size of the drawable Y dimension
        /// </summary>
        protected int yDrawable { get { return FEZ_Components.FEZTouch.ScreenHeight - Margin - titleBarSize; } }

        /// <summary>
        /// Store the value of the x postion offeset for the form content
        /// </summary>
        protected int xOffset { get { return Margin; } }

        /// <summary>
        /// Store the value of the y postion offeset for the form content
        /// </summary>
        protected int yOffset { get { return titleBarSize + Margin; } }

        /// <summary>
        /// Get or Set the title
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// Get or set the font color
        /// </summary>
        public FEZ_Components.FEZTouch.Color ForegroundColor { get; set; }

        /// <summary>
        /// Get or set the background color
        /// </summary>
        public FEZ_Components.FEZTouch.Color BackgroundColor { get; set; }


        /// <summary>
        /// Store the flag value if the form has been already painted
        /// </summary>
        private bool initPaint = true;

        /// <summary>
        /// Create a new instance of <see cref="FezForm"/>
        /// </summary>
        public FezForm()
        {

        }

        /// <summary>
        /// Draw the form on the screen
        /// </summary>
        /// <param name="screen">Instance of IDrawable device</param>
        public virtual void Paint(IDrawer screen)
        {
            // Paint once
            if (!initPaint)
            {
                return;
            }
            screen.FillRectangle(0, titleBarSize, screen.Width, screen.Height - titleBarSize, this.BackgroundColor);
            initPaint = false;
            screen.FillRectangle(0, 0, screen.Width, titleBarSize, FEZ_Components.FEZTouch.Color.Black);
            screen.DrawString(this.Title, 5, 5, FEZ_Components.FEZTouch.Color.White, FEZ_Components.FEZTouch.Color.Black);
        }
    }

}

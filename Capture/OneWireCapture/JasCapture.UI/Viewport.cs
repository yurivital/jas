using System;
using Microsoft.SPOT;

namespace JasCapture.Form
{
    /// <summary>
    /// Define a viewport for partial screen display
    /// </summary>
    public class ViewPort
    {
        /// <summary>
        /// Get or set the top-left corner X position of the viewport
        /// </summary>
        public int X { get; set; }
        
        /// <summary>
        /// Get or set the top-left corner Y position of the viewport
        /// </summary>
        public int Y { get; set; }
        
        /// <summary>
        /// Get or set the heigth to display
        /// </summary>
        public int Heigth { get; set; }

        /// <summary>
        /// Get or set the width to display
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Store the instance of the form component to display
        /// </summary>
        private IComponent component;
        
        /// <summary>
        /// Create a new instance of <see cref="ViewPort"/> for an component  
        /// </summary>
        /// <param name="component"></param>
        public ViewPort(IComponent component)
        {
            this.component = component;
        }

        /// <summary>
        /// Paint the associated componant to screen in the viewport bounds
        /// </summary>
        /// <param name="screen">Instance of the display device</param>
        public void Paint(IDrawer screen)
        {
            ViewPortDevice device = new ViewPortDevice(screen);
            device.xOffset = X;
            device.yOffset = Y;
            device.yDrawable = Heigth;
            device.xDrawable = Width;
            component.Draw(device);
        }
    }
}

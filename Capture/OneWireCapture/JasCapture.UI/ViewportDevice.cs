using GHIElectronics.NETMF.FEZ;

namespace JasCapture.Form
{
    /// <summary>
    /// Provice partial screen display and drawing
    /// </summary>
    public class ViewPortDevice : IDrawer
    {
        /// <summary>
        /// Get or set the X drawable dimension (Width)
        /// </summary>
        public int xDrawable { get; set; }
        /// <summary>
        /// Get or set the Y drawable dimension (Height)
        /// </summary>
        public int yDrawable { get; set; }
        /// <summary>
        /// Get or set the X offset value
        /// </summary>
        public int xOffset { get; set; }
        /// <summary>
        /// Get or set the Y offset value
        /// </summary>
        public int yOffset { get; set; }
        /// <summary>
        /// Store the instance of a <see cref="IDrawer"/>
        /// </summary>
        IDrawer screen;

        /// <summary>
        /// Create a new instance of <see cref="ViewPortDevice"/>
        /// </summary>
        /// <param name="screen">Instance of an object implenting <see cref="IDrawer"/></param>
        public ViewPortDevice(IDrawer screen)
        {
            this.screen = screen;
        }

        /// <summary>
        /// Draw an image
        /// </summary>
        /// <param name="x">start X position</param>
        /// <param name="y">start Y position</param>
        /// <param name="image">Image to display</param>
        public void DrawImage(int x, int y, FEZ_Components.FEZTouch.Image image)
        {
            this.screen.DrawImage(x + xOffset, y + yOffset, image);
        }

        /// <summary>
        /// Draw a line
        /// </summary>
        /// <param name="x0">x strat position</param>
        /// <param name="y0">y start position</param>
        /// <param name="x1">x end position</param>
        /// <param name="y1">y end position</param>
        /// <param name="col">line color</param>
        public void DrawLine(int x0, int y0, int x1, int y1, FEZ_Components.FEZTouch.Color col)
        {
            this.screen.DrawLine(xOffset + x0, yOffset + y0, xOffset + x1, yOffset + y1, col);
        }

        /// <summary>
        /// Draw text
        /// </summary>
        /// <param name="str">Text to display</param>
        /// <param name="x">x start position</param>
        /// <param name="y">y start postion</param>
        /// <param name="foreColor">Color of the font</param>
        /// <param name="backColor">Color of the text background</param>
        public void DrawString(string str, int x, int y, FEZ_Components.FEZTouch.Color foreColor, FEZ_Components.FEZTouch.Color backColor)
        {
            this.screen.DrawString(str, x + xOffset, y + yOffset, foreColor, backColor);
        }

        /// <summary>
        /// Draw a rectangle
        /// </summary>
        /// <param name="x">x start position</param>
        /// <param name="y">y stat postion</param>
        /// <param name="width">Width rectangle size</param>
        /// <param name="height">Height rectangle size</param>
        /// <param name="col">Inner rectangle fill colo</param>
        public void FillRectangle(int x, int y, int width, int height, GHIElectronics.NETMF.FEZ.FEZ_Components.FEZTouch.Color col)
        {
            this.screen.FillRectangle(x + xOffset, y + yOffset, width, height, col);
        }

        /// <summary>
        ///  Get the Height screen sizz
        /// </summary>
        public int Height
        {
            get { return this.yDrawable; }
        }

        /// <summary>
        /// Get the Width screen size
        /// </summary>
        public int Width
        {
            get { return this.xDrawable; }
        }

        /// <summary>
        /// Create a color from Red Green Blue values
        /// </summary>
        /// <param name="red">Red value</param>
        /// <param name="green">Green value</param>
        /// <param name="blue">Blue value</param>
        /// <returns>Color instance</returns>
        public FEZ_Components.FEZTouch.Color ColorFromRGB(byte red, byte green, byte blue)
        {
            return this.screen.ColorFromRGB(red, green, blue);
        }

        /// <summary>
        /// Set the brightness of the screen
        /// </summary>
        /// <param name="percentage">percentage of ligth</param>
        public void SetBrightness(int percentage)
        {
            this.screen.SetBrightness(percentage);
        }

        /// <summary>
        /// Draw a pixel
        /// </summary>
        /// <param name="x">x pixel position</param>
        /// <param name="y">y pixel postion</param>
        /// <param name="col">Pixel color</param>
        public void SetPixel(int x, int y, FEZ_Components.FEZTouch.Color col)
        {
            this.SetPixel(x + xOffset, y + yOffset, col);
        }
    }
}

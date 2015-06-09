using System;
using GHIElectronics.NETMF.FEZ;
namespace JasCapture.Form
{
    /// <summary>
    /// Define the primitive for drawing to the screen
    /// </summary>
    public interface IDrawer
    {
        /// <summary>
        /// Get the screen heigth in pixel
        /// </summary>
        int Height { get; }
        /// <summary>
        /// Get the screen Width in pixel
        /// </summary>
        int Width { get; }
        
        /// <summary>
        /// Return a <see cref="FEZ_Components.FEZTouch.Color"/> from RGB values
        /// </summary>
        /// <param name="red">amount of red (0 - 255)</param>
        /// <param name="green">amount of green (0 - 255)</param>
        /// <param name="blue">amount of blue (0 - 255)</param>
        /// <returns>A dispalyable color</returns>
        FEZ_Components.FEZTouch.Color ColorFromRGB(byte red, byte green, byte blue);
        
        /// <summary>
        /// Draw an image
        /// </summary>
        /// <param name="x">X postion in pixel</param>
        /// <param name="y">Y position in pixel</param>
        /// <param name="image">Instance of image to display</param>
        void DrawImage(int x, int y, FEZ_Components.FEZTouch.Image image);
        
        /// <summary>
        /// Draw a line
        /// </summary>
        /// <param name="x0">X start postion in pixel</param>
        /// <param name="y0">Y start postion in pixel</param>
        /// <param name="x1">X end position in pixel</param>
        /// <param name="y1">Y end position in pixel</param>
        /// <param name="col">Line color</param>
        void DrawLine(int x0, int y0, int x1, int y1, FEZ_Components.FEZTouch.Color col);
        
        /// <summary>
        ///  Draw text on the screen
        /// </summary>
        /// <param name="str">text to display</param>
        /// <param name="x">X postion of the baseline in pixel</param>
        /// <param name="y">Y postion ot the baseline in pixel</param>
        /// <param name="foreColor">Color of the font</param>
        /// <param name="backColor">Color ot the text background</param>
        void DrawString(string str, int x, int y, FEZ_Components.FEZTouch.Color foreColor, FEZ_Components.FEZTouch.Color backColor);
        
        /// <summary>
        /// Draw a rectangle shape 
        /// </summary>
        /// <param name="x">X postion of the top-left corner</param>
        /// <param name="y">X postion of the top-left corner</param>
        /// <param name="width">width size in pixel</param>
        /// <param name="height">height size in pixel</param>
        /// <param name="col">Color of the background</param>
        void FillRectangle(int x, int y, int width, int height, FEZ_Components.FEZTouch.Color col);
        
        /// <summary>
        /// Set the brightness of the screen
        /// </summary>
        /// <param name="percentage">value between 0 and 100</param>
        void SetBrightness(int percentage);
        
        /// <summary>
        /// Draw an pixel
        /// </summary>
        /// <param name="x">X pixel position</param>
        /// <param name="y">Y pixel position</param>
        /// <param name="col">pixel color</param>
        void SetPixel(int x, int y, FEZ_Components.FEZTouch.Color col);
    }
}

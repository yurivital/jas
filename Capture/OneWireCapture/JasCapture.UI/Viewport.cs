using System;
using Microsoft.SPOT;

namespace JasCapture.Form
{
    public class ViewPort
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Heigth { get; set; }
        public int Width { get; set; }

        private IComponent component;

        public ViewPort(IComponent component)
        {
            this.component = component;
        }

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

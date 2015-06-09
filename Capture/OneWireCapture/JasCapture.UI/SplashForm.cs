using System;
using Microsoft.SPOT;
using JasCapture.Form.Base;
using GHIElectronics.NETMF.FEZ;

namespace JasCapture.Form
{
    /// <summary>
    ///  Display a splashscrren
    /// </summary>
   public class SplashForm :FezForm
    {
       /// <summary>
       /// Paint the splash screen
       /// </summary>
       /// <param name="screen">instance of screen device</param>
       public override void Paint(IDrawer screen)
       {
           base.Paint(screen);
           // Load an image from resources
           FEZ_Components.FEZTouch.Image img = new FEZ_Components.FEZTouch.Image(Resource.GetBytes(Resource.BinaryResources.jas2));
           // draw image
           screen.DrawImage((xDrawable- img.Width) / 2, this.yOffset + (yDrawable - img.Height) / 2, img);

    
       }
    }
}

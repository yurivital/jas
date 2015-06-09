using GHIElectronics.NETMF.FEZ;
namespace JasCapture.Form
{
    /// <summary>
    /// Define the behavior of an form components
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// Render componant to the screen
        /// </summary>
        /// <param name="screen">screen device</param>
        void Draw(IDrawer screen);
    }
}
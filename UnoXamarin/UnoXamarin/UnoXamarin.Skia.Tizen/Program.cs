using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace UnoXamarin.Skia.Tizen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new UnoXamarin.App(), args);
            host.Run();
        }
    }
}

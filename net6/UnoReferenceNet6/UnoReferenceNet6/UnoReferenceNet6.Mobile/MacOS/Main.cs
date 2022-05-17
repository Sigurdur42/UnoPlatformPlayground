using AppKit;

namespace UnoReferenceNet6.macOS
{
    internal static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.SharedApplication.Delegate = new App();
            NSApplication.Main(args);
        }
    }
}


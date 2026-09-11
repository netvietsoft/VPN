using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("  STARTING AUTOMATED TEST OF FASTVPN BYPASS LIFECYCLE");
        Console.WriteLine("=================================================");

        string vpnDir = @"E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN_Bypass";
        Directory.SetCurrentDirectory(vpnDir);

        AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) =>
        {
            string assemblyName = new AssemblyName(resolveArgs.Name).Name + ".dll";
            string assemblyPath = Path.Combine(vpnDir, assemblyName);
            if (File.Exists(assemblyPath))
            {
                return Assembly.LoadFrom(assemblyPath);
            }
            return null;
        };

        Thread staThread = new Thread(() =>
        {
            try
            {
                Console.WriteLine("[TEST STEP 1] Setting ResourceAssembly to FastVPN via reflection...");
                var fastVpnAssembly = typeof(NamecheapVPN.App).Assembly;
                var field = typeof(Application).GetField("_resourceAssembly", BindingFlags.Static | BindingFlags.NonPublic);
                if (field != null)
                {
                    field.SetValue(null, fastVpnAssembly);
                    Console.WriteLine("   Field _resourceAssembly set successfully!");
                }
                else
                {
                    Console.WriteLine("   Field _resourceAssembly not found.");
                }

                Console.WriteLine("[TEST STEP 2] Initializing NamecheapVPN.App...");
                var app = new NamecheapVPN.App();
                app.InitializeComponent();
                Console.WriteLine("   App.InitializeComponent() OK!");

                Console.WriteLine("[TEST STEP 3] Instantiating MainWindow...");
                var mainWin = new NamecheapVPN.MainWindow();
                Console.WriteLine("   MainWindow instantiated successfully!");

                Console.WriteLine("[TEST STEP 4] Testing BypassLoginAndEnter()...");
                mainWin.BypassLoginAndEnter();
                Console.WriteLine("   BypassLoginAndEnter() executed without exceptions!");

                Console.WriteLine("[TEST STEP 5] Verifying SDK & ExpandedWindow state...");
                var sdk = mainWin.GetSdk();
                if (sdk == null)
                {
                    Console.WriteLine("   [FAIL] SDKMonitor is null!");
                    return;
                }
                Console.WriteLine("   SDKMonitor is non-null: OK");

                if (sdk.VpnExpandedWindow == null)
                {
                    Console.WriteLine("   [FAIL] VpnExpandedWindow is null!");
                    return;
                }
                Console.WriteLine("   VpnExpandedWindow is non-null: OK");
                Console.WriteLine($"   VpnExpandedWindow Title: '{sdk.VpnExpandedWindow.Title}'");
                Console.WriteLine($"   VpnExpandedWindow Width: {sdk.VpnExpandedWindow.Width}, Height: {sdk.VpnExpandedWindow.Height}");
                Console.WriteLine($"   VpnExpandedWindow Visibility: {sdk.VpnExpandedWindow.Visibility}");
                Console.WriteLine($"   MainWindow Visibility: {mainWin.Visibility}");

                Console.WriteLine("=================================================");
                Console.WriteLine("  ALL LIFECYCLE TESTS PASSED PERFECTLY (100% OK)!");
                Console.WriteLine("=================================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TEST FAILED WITH EXCEPTION]: {ex.GetType().FullName}: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   InnerException: {ex.InnerException.GetType().FullName}: {ex.InnerException.Message}");
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        });

        staThread.SetApartmentState(ApartmentState.STA);
        staThread.Start();
        staThread.Join();
    }
}

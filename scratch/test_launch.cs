using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        try
        {
            string appDir = @"e:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows";
            Environment.CurrentDirectory = appDir;
            
            Console.WriteLine("Loading NextAiVPN.Desktop.dll...");
            var asm = Assembly.LoadFrom(System.IO.Path.Combine(appDir, "NextAiVPN.Desktop.dll"));
            Console.WriteLine("Loaded assembly: " + asm.FullName);
            
            var entryPoint = asm.EntryPoint;
            Console.WriteLine("Entry point: " + (entryPoint != null ? entryPoint.Name : "null"));

            // Let's create an instance of App and MainWindow
            var appType = asm.GetType("NextAiVPN.App");
            Console.WriteLine("App type: " + appType);

            var mainWinType = asm.GetType("NextAiVPN.MainWindow");
            Console.WriteLine("MainWindow type: " + mainWinType);

            Console.WriteLine("Attempting to invoke constructor of MainWindow...");
            var win = Activator.CreateInstance(mainWinType);
            Console.WriteLine("MainWindow instance created successfully: " + win);
        }
        catch (TargetInvocationException tie)
        {
            Console.WriteLine("TargetInvocationException: " + tie.InnerException);
            if (tie.InnerException is MissingMethodException mme)
            {
                Console.WriteLine("MissingMethodException Message: " + mme.Message);
                Console.WriteLine("Stack trace:\n" + mme.StackTrace);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);
        }
    }
}

using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        var asm = Assembly.LoadFrom(@"E:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\bin\Release\net10.0-windows\VpnSDK.dll");
        var core = asm.GetType("VpnSDK.SDKCore");
        Console.WriteLine("All SDKCore Fields:");
        foreach (var f in core.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            Console.WriteLine($"  {f.Name} ({f.FieldType.Name})");
        }
    }
}

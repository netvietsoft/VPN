using System;
using System.IO;

namespace NextAiTechnology.RebrandingTool
{
    class Program
    {
        static void Main()
        {
            string baseDir = @"e:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop";
            string assetsDir = Path.Combine(baseDir, "assets");
            string assetsTarget = Path.Combine(baseDir, "Assets");
            string tempDir = Path.Combine(baseDir, "Assets_Temp");

            Console.WriteLine("Adjusting Assets folder casing...");
            if (Directory.Exists(assetsDir))
            {
                Directory.Move(assetsDir, tempDir);
                Directory.Move(tempDir, assetsTarget);
                Console.WriteLine("Renamed assets -> Assets");
            }

            // Adjust subfolders: NextAiVPN and DarkMode
            string subNextAi = Path.Combine(assetsTarget, "nextaivpn");
            string subNextAiTarget = Path.Combine(assetsTarget, "NextAiVPN");
            string subNextAiTemp = Path.Combine(assetsTarget, "NextAiVPN_Temp");
            if (Directory.Exists(subNextAi))
            {
                Directory.Move(subNextAi, subNextAiTemp);
                Directory.Move(subNextAiTemp, subNextAiTarget);
                Console.WriteLine("Renamed nextaivpn -> NextAiVPN");
            }

            string subDark = Path.Combine(assetsTarget, "darkmode");
            string subDarkTarget = Path.Combine(assetsTarget, "DarkMode");
            string subDarkTemp = Path.Combine(assetsTarget, "DarkMode_Temp");
            if (Directory.Exists(subDark))
            {
                Directory.Move(subDark, subDarkTemp);
                Directory.Move(subDarkTemp, subDarkTarget);
                Console.WriteLine("Renamed darkmode -> DarkMode");
            }

            Console.WriteLine("Case normalization finished!");
        }
    }
}

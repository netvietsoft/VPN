using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        string path = @"e:\DECOMPILER\Soft\VPN\CONVERT\apps\desktop\NextAiVPN.Desktop\NextAiVPN\MainWindow.cs";
        var lines = File.ReadAllLines(path).ToList();
        
        int startIdx = -1;
        int endIdx = -1;
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains("PrivacyPolicyFooter.TextDecorations = null;") && startIdx == -1)
            {
                startIdx = i; // Line index where the issue begins
            }
            if (lines[i].Contains("private void Window_Closing") && endIdx == -1)
            {
                endIdx = i; // Line index where Window_Closing begins
            }
        }

        Console.WriteLine($"startIdx: {startIdx} ('{lines[startIdx]}')");
        Console.WriteLine($"endIdx: {endIdx} ('{lines[endIdx]}')");

        // Between startIdx (line 413, index 413) and endIdx (index 612):
        // Replace everything from startIdx to endIdx with:
        // 		PrivacyPolicyFooter.TextDecorations = null;
        // 	}
        // 
        // 	private void MainWindow_OnClosed(object sender, EventArgs e)
        // 	{
        // 		IsClosed = true;
        // 		try
        // 		{
        // 			GlobalEvents.StyleChanged -= MainWindowStyleChanged;
        // 			_sdk?.NextAiVpnSdkManager?.Dispose();
        // 			_sdk?.StreamingSdk?.Dispose();
        // 		}
        // 		catch
        // 		{
        // 		}
        // 	}
        // 

        var replacement = new[]
        {
            "\t\tPrivacyPolicyFooter.TextDecorations = null;",
            "\t}",
            "",
            "\tprivate void MainWindow_OnClosed(object sender, EventArgs e)",
            "\t{",
            "\t\tIsClosed = true;",
            "\t\ttry",
            "\t\t{",
            "\t\t\tGlobalEvents.StyleChanged -= MainWindowStyleChanged;",
            "\t\t\t_sdk?.NextAiVpnSdkManager?.Dispose();",
            "\t\t\t_sdk?.StreamingSdk?.Dispose();",
            "\t\t}",
            "\t\tcatch",
            "\t\t{",
            "\t\t}",
            "\t}",
            ""
        };

        lines.RemoveRange(startIdx, endIdx - startIdx);
        lines.InsertRange(startIdx, replacement);

        // Also let's check if NextAiTechnologyButton_OnClick is present
        bool hasTechClick = lines.Any(l => l.Contains("NextAiTechnologyButton_OnClick"));
        if (!hasTechClick)
        {
            int autoBypassIdx = lines.FindIndex(l => l.Contains("private void AutoBypassLogin()"));
            if (autoBypassIdx > 0)
            {
                var techClickLines = new[]
                {
                    "\tprivate void NextAiTechnologyButton_OnClick(object sender, RoutedEventArgs e)",
                    "\t{",
                    "\t\tif (_browserDefiner.DefineBrowser() != WebBrowserResult.WebView2)",
                    "\t\t{",
                    "\t\t\tUtils.Logger.Warning(\"Sign in nextaitechnology. WebView2 runtime is not found\", \"NextAiTechnologyButton_OnClick\", \"MainWindow.xaml.cs\", 500);",
                    "\t\t\treturn;",
                    "\t\t}",
                    "\t\t_signInNextAiTechnologyService.SignInValidations();",
                    "\t\tHide();",
                    "\t}",
                    ""
                };
                lines.InsertRange(autoBypassIdx, techClickLines);
            }
        }

        File.WriteAllLines(path, lines);
        Console.WriteLine("MainWindow.cs successfully updated!");
    }
}

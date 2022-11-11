using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FFmpegGui;

public abstract class ConversionToolService
{
    protected static async Task<bool> IsToolInstalledAsync(MainWindow.ConversionTool tool)
    {
        try
        {
            var process = Process.Start(tool.ToExeName());
            await process.WaitForExitAsync();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    protected static void LaunchWebsite(MainWindow.ConversionTool tool)
    {
        Process.Start(
            new ProcessStartInfo
            {
                FileName = tool.ToWebsite(),
                UseShellExecute = true
            });
    }
}
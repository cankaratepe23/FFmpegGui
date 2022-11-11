using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FFmpegGui;

public class FfmpegService : ConversionToolService
{
    private const MainWindow.ConversionTool Tool = MainWindow.ConversionTool.Ffmpeg;
    
    public static async Task<bool> IsToolInstalledAsync()
    {
        return await IsToolInstalledAsync(Tool);
    }
    
    public static void LaunchWebsite()
    {
        LaunchWebsite(Tool);
    }
}
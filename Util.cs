using System;

namespace FFmpegGui;

public static class Util
{
    public static void LogToUser(string message)
    {
        MainWindow.UserLogText = message + Environment.NewLine + MainWindow.UserLogText;
    }

    public static string ToExeName(this MainWindow.ConversionTool tool)
    {
        return tool switch
        {
            MainWindow.ConversionTool.Gifski => "gifski.exe",
            MainWindow.ConversionTool.Ffmpeg => "ffmpeg.exe",
            _ => throw new ArgumentOutOfRangeException(nameof(tool), tool, "Invalid tool.")
        };
    }
    
    public static string ToWebsite(this MainWindow.ConversionTool tool)
    {
        return tool switch
        {
            MainWindow.ConversionTool.Gifski => "https://gif.ski",
            MainWindow.ConversionTool.Ffmpeg => "https://ffmpeg.org/",
            _ => throw new ArgumentOutOfRangeException(nameof(tool), tool, "Invalid tool.")
        };
    }
}
using System;
using System.IO;

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
            MainWindow.ConversionTool.Ffmpeg => "https://www.gyan.dev/ffmpeg/builds/",
            _ => throw new ArgumentOutOfRangeException(nameof(tool), tool, "Invalid tool.")
        };
    }

    public static string ToDefaultTimestampString(this TimeSpan timeSpan)
    {
        return timeSpan.ToString(MainWindow.TimestampToStringFormat);
    }
    
    public static string GetTemporaryDirectory()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        return tempDirectory;
    }
}
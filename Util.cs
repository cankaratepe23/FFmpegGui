using System;

namespace FFmpegGui;

public static class Util
{
    public static void LogToUser(string message)
    {
        MainWindow.UserLogText = message + Environment.NewLine + MainWindow.UserLogText;
    }
}
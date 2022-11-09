using System;
using System.ComponentModel;
using System.Diagnostics;

namespace FFmpegGui;

public class GifskiService
{
    public static bool IsGifskiInstalled()
    {
        try
        {
            Process.Start("gifski.exe");
        }
        catch (Exception)
        {
            return false;
        }
        return true;
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace FFmpegGui;

public class GifskiService : ConversionToolService
{
    private const MainWindow.ConversionTool Tool = MainWindow.ConversionTool.Gifski;

    public static async Task<bool> IsToolInstalledAsync()
    {
        return await IsToolInstalledAsync(Tool);
    }

    public static void LaunchWebsite()
    {
        LaunchWebsite(Tool);
    }

    public static async Task<bool> InstallGifskiAsync()
    {
        Util.LogToUser("Loading https://gif.ski/ ...");
        var gifskiPage = new HtmlWeb();
        var gifskiPageDoc = await gifskiPage.LoadFromWebAsync("https://gif.ski/");
        var href = gifskiPageDoc.DocumentNode.SelectNodes("//a[@href]")
            .FirstOrDefault(n => Regex.IsMatch(
                n.GetAttributeValue("href", string.Empty), @"^\/gifski-.*\.zip$"));
        if (href == null)
        {
            Util.LogToUser(
                "Could not find the download link inside the web page. You need to install gifski manually.");
            return false;
        }

        Util.LogToUser("Downloading gifski...");
        var downloadLink = "https://gif.ski" + href.GetAttributeValue("href", string.Empty);
        var filename = downloadLink[(downloadLink.LastIndexOf('/') + 1)..];
        try
        {
            await using var downloadStream = await MainWindow.Client.GetStreamAsync(downloadLink);
            await using var fileStream = new FileStream(filename, FileMode.Create);
            await downloadStream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            Util.LogToUser($"An error occurred while downloading {filename}.");
            Util.LogToUser($"Exception details: {e.Message}");
            return false;
        }

        Util.LogToUser("Extracting...");
        try
        {
            using var archive = ZipFile.OpenRead(filename);
            (archive.GetEntry("win/gifski.exe") ??
             throw new InvalidOperationException($"Could not find win/gifski.exe in {filename}"))
                .ExtractToFile("gifski.exe");
        }
        catch (Exception e)
        {
            Util.LogToUser($"An error occurred while extracting {filename}.");
            Util.LogToUser($"Exception details: {e.Message}");
            return false;
        }

        Util.LogToUser($"Deleting the zip file {filename}");
        File.Delete(filename);
        Util.LogToUser("Done!");
        return true;
    }

    public static async Task<string> ConvertToGif(string framesDir, int fps, int quality, int width)
    {
        var args = "-o output.gif ";
        if (fps > 0)
        {
            args += $"--fps {fps} ";
        }

        if (quality > 0)
        {
            if (quality > 100)
                quality = 100;
            args += $"--quality {quality} ";
        }

        if (width > 0)
        {
            args += $"--width {width} ";
        }
        args += "frame*.png";
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "gifski.exe",
                WorkingDirectory = framesDir,
                Arguments = args,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
#if DEBUG
        proc.StartInfo.RedirectStandardError = true;
#endif
        proc.Start();
        await proc.WaitForExitAsync();
        Util.LogToUser("Created GIF file");
        return Path.Combine(framesDir, "output.gif");
    }
}
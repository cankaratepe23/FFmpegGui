using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace FFmpegGui;

public class GifskiService
{
    private static readonly HttpClient Client = new();

    private static void LogToUser(string message)
    {
        MainWindow.UserLogText = message + Environment.NewLine + MainWindow.UserLogText;
    }

    public static async Task<bool> IsGifskiInstalledAsync()
    {
        try
        {
            var process = Process.Start("gifski.exe");
            await process.WaitForExitAsync();
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public static async Task InstallGifskiAsync()
    {
        LogToUser("Loading https://gif.ski/ ...");
        var gifskiPage = new HtmlWeb();
        var gifskiPageDoc = await gifskiPage.LoadFromWebAsync("https://gif.ski/");
        var href = gifskiPageDoc.DocumentNode.SelectNodes("//a[@href]")
            .FirstOrDefault(n => Regex.IsMatch(
                n.GetAttributeValue("href", string.Empty), @"^\/gifski-.*\.zip$"));
        if (href == null)
        {
            LogToUser("Could not find the download link inside the web page. You need to install gifski manually.");
            return;
        }

        LogToUser("Downloading gifski...");
        var downloadLink = "https://gif.ski" + href.GetAttributeValue("href", string.Empty);
        var filename = downloadLink[(downloadLink.LastIndexOf('/') + 1)..];
        try
        {
            await using var downloadStream = await Client.GetStreamAsync(downloadLink);
            await using var fileStream = new FileStream(filename, FileMode.Create);
            await downloadStream.CopyToAsync(fileStream);
        }
        catch (Exception e)
        {
            LogToUser($"An error occurred while downloading {filename}.");
            LogToUser($"Exception details: {e.Message}");
            return;
        }

        LogToUser("Extracting...");
        try
        {
            using var archive = ZipFile.OpenRead(filename);
            (archive.GetEntry("win/gifski.exe") ??
             throw new InvalidOperationException($"Could not find win/gifski.exe in {filename}"))
                .ExtractToFile("gifski.exe");
        }
        catch (Exception e)
        {
            LogToUser($"An error occurred while extracting {filename}.");
            LogToUser($"Exception details: {e.Message}");
            return;
        }

        LogToUser($"Deleting the zip file {filename}");
        File.Delete(filename);
        LogToUser("Done!");
    }

    public static void LaunchWebsite()
    {
        var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = "https://gif.ski/",
                UseShellExecute = true
            });
    }
}
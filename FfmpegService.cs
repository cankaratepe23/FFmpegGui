using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SevenZipExtractor;

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

    public static async Task<bool> InstallFfmpegAsync()
    {
        Util.LogToUser("Downloading ffmpeg-git-full.7z... Please wait.");
        var downloadLink = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z";
        var filename = "ffmpeg-git-full.7z";
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
            using var archive = new ArchiveFile(filename);
            var ffmpegExe = archive.Entries.FirstOrDefault(e =>
                Regex.IsMatch(e.FileName, @"ffmpeg\.exe$"));
            if (ffmpegExe == null)
            {
                throw new Exception("Could not find the ffmpeg.exe file inside the downloaded zip.");
            }

            ffmpegExe.Extract(Tool.ToExeName());
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

    public static async Task<byte[]> GetFrameAtTimestampAsync(string filePath, string timestamp)
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                Arguments = $"-ss {timestamp} -i \"{filePath}\" -c:v bmp -frames:v 1 -f rawvideo -an -sn -",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
#if DEBUG
        proc.StartInfo.RedirectStandardError = true;
#endif
        proc.Start();

        using var reader = new BinaryReader(proc.StandardOutput.BaseStream);
        var readValues = new List<byte>();
        try
        {
            while (true)
            {
                readValues.Add(reader.ReadByte());
            }
        }
        catch (EndOfStreamException)
        {
        }

        await proc.WaitForExitAsync();
        return readValues.ToArray();
    }
}
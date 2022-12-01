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

            var ffprobeExe = archive.Entries.FirstOrDefault(e =>
                Regex.IsMatch(e.FileName, @"ffprobe\.exe$"));
            if (ffprobeExe == null)
            {
                throw new Exception("Could not find the ffprobe.exe file inside the downloaded zip.");
            }

            ffprobeExe.Extract("ffprobe.exe");
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

    public static async Task<VideoMetadata> GetVideoMetadata(string filePath)
    {
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffprobe.exe",
                WorkingDirectory = Directory.GetCurrentDirectory(),
                Arguments = $"-v 0 -of csv=p=0 -select_streams v:0 -show_entries stream=r_frame_rate -i \"{filePath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
#if DEBUG
        proc.StartInfo.RedirectStandardError = true;
#endif
        proc.Start();

        var returnValue = await proc.StandardOutput.ReadToEndAsync();
        returnValue = returnValue.Trim();
        await proc.WaitForExitAsync();

        if (string.IsNullOrWhiteSpace(returnValue) || !Regex.IsMatch(returnValue, "^[0-9]*/[0-9]*$") ||
            returnValue == "0/0")
        {
            throw new Exception("Video has wrong/invalid FPS.");
        }

        var numbers = Regex.Matches(returnValue, "[0-9]+");
        var fps = Convert.ToDouble(numbers[0].Value) / Convert.ToDouble(numbers[1].Value);

        return new VideoMetadata()
        {
            Fps = fps
        };
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

    public static async Task<string> GetFramesAsync(string filePath, int fps)
    {
        var args = $"-i \"{filePath}\" ";
        if (fps > 0)
        {
            args += $"-r {fps} ";
        }
        args += "frame%03d.png";
        var outputDir = Util.GetTemporaryDirectory();
        while (File.Exists(outputDir))
        {
            outputDir = Util.GetTemporaryDirectory();
        }

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                WorkingDirectory = outputDir,
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
        Util.LogToUser("Exported video frames");
        return outputDir;
    }
}
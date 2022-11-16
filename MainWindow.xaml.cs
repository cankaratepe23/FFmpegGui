using System;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace FFmpegGui;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    public enum ConversionTool
    {
        Ffmpeg,
        Gifski
    }

    private static string _userLogText = "";

    public static readonly HttpClient Client = new();

    public MainWindow()
    {
        ImagePreviewSource = FfmpegService.GetFrameAtTimestampAsync("res/sample square.png", "0").GetAwaiter()
            .GetResult();
        InitializeComponent();
    }

    public static bool IsDebug
    {
#if DEBUG
        get { return true; }
#else
        get { return false; }
#endif
    }

    public static bool IsNotDebug
    {
#if DEBUG
        get { return false; }
#else
        get { return true; }
#endif
    }

    public static string UserLogText
    {
        get => _userLogText;
        set
        {
            if (_userLogText == value)
            {
                return;
            }

            _userLogText = value;
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(UserLogText)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static event PropertyChangedEventHandler? StaticPropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void ToggleUsingDuration()
    {
        IsUsingDuration = !IsUsingDuration;
    }

    private void HandleTimestampChanged()
    {
        if (TxtTimestamp.Text == Timestamp)
        {
            return;
        }

        if (!Regex.IsMatch(TxtTimestamp.Text, "^([0-9]{1,2}:){0,2}([0-9]{1,2})(.([0-9]{1,3})){0,1}$"))
        {
            TxtTimestamp.Foreground = new SolidColorBrush(Colors.Red);
            return;
        }

        TxtTimestamp.Foreground = new SolidColorBrush(Colors.Black);

        Timestamp = TxtTimestamp.Text;
        LoadFrame();
    }

    private void CalculateNewTimestamp(TimeSpan delta)
    {
        var succeeded = TimeSpan.TryParse(Timestamp, out var timeSpan);
        Timestamp = (timeSpan + delta).ToString(@"hh\:mm\:ss\.fff");
        LoadFrame();
    }

    private async void LoadFile()
    {
        if (!File.Exists(FilePath))
        {
            TxtFilePath.Foreground = new SolidColorBrush(Colors.Red);
            return;
        }
        TxtFilePath.Foreground = new SolidColorBrush(Colors.Black);
        var metadata = await FfmpegService.GetVideoMetadata(FilePath);
        CurrentFps = metadata.Fps;
        LoadFrame();
    }
    
    private async void LoadFrame()
    {
        if (!File.Exists(FilePath))
        {
            TxtFilePath.Foreground = new SolidColorBrush(Colors.Red);
            return;
        }
        ImagePreviewSource = await Task.Run(() => FfmpegService.GetFrameAtTimestampAsync(FilePath, Timestamp));
    }

    private async Task<bool> EnsureGifskiInstalled()
    {
        if (!await GifskiService.IsToolInstalledAsync())
        {
            var style = new Style();
            style.Setters.Add(new Setter(MessageBox.CancelButtonContentProperty, "Go to website..."));
            var result = MessageBox.Show(
                "Could not find gifski." + Environment.NewLine +
                "Install it by downloading the latest command-line binaries from https://gif.ski/ " +
                "and either add it to your PATH and restart this application or place it in"
                + Environment.NewLine + Directory.GetCurrentDirectory()
                + Environment.NewLine + Environment.NewLine +
                "Would you like to try to install gifski automatically?",
                "Could not find gifski",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.No,
                style);
            if (MessageBoxResult.Yes == result)
            {
                return await Task.Run(GifskiService.InstallGifskiAsync);
            }

            if (MessageBoxResult.Cancel == result)
            {
                await Task.Run(GifskiService.LaunchWebsite);
            }

            return false;
        }

        return true;
    }

    private async Task<bool> EnsureFfmpegInstalled()
    {
        if (!await FfmpegService.IsToolInstalledAsync())
        {
            var style = new Style();
            style.Setters.Add(new Setter(MessageBox.CancelButtonContentProperty, "Go to website..."));
            var result = MessageBox.Show(
                "Could not find FFmpeg." + Environment.NewLine +
                "Install it by downloading the latest ffmpeg-git-full.7z from https://www.gyan.dev/ffmpeg/builds/ffmpeg-git-full.7z " +
                "and either add the ffmpeg.exe file to your PATH and restart this application or place it in"
                + Environment.NewLine + Directory.GetCurrentDirectory()
                + Environment.NewLine + Environment.NewLine +
                "Would you like to try to install FFmpeg automatically?",
                "Could not find FFmpeg",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.No,
                style);
            if (MessageBoxResult.Yes == result)
            {
                return await Task.Run(FfmpegService.InstallFfmpegAsync);
            }

            if (MessageBoxResult.Cancel == result)
            {
                await Task.Run(FfmpegService.LaunchWebsite);
            }

            return false;
        }

        return true;
    }

    #region Properties

    private bool _isCropEnabled;

    public bool IsCropEnabled
    {
        get => _isCropEnabled;
        set
        {
            if (value == _isCropEnabled)
            {
                return;
            }

            _isCropEnabled = value;
            NotifyPropertyChanged();
        }
    }

    private bool _isTrimEnabled;

    public bool IsTrimEnabled
    {
        get => _isTrimEnabled;
        set
        {
            if (value == _isTrimEnabled)
            {
                return;
            }

            _isTrimEnabled = value;
            NotifyPropertyChanged();
        }
    }

    private bool _isUsingDuration;

    public bool IsUsingDuration
    {
        get => _isUsingDuration;
        set
        {
            if (value == _isUsingDuration)
            {
                return;
            }

            _isUsingDuration = value;
            NotifyPropertyChanged();
        }
    }

    private bool ImageCropMouseDown { get; set; }

    private int ImageCropStartXPreview { get; set; }

    private int ImageCropStartYPreview { get; set; }

    private int ImageCropEndXPreview { get; set; }

    private int ImageCropEndYPreview { get; set; }


    private int _imageCropStartX;

    public int ImageCropStartX
    {
        get => _imageCropStartX;
        set
        {
            if (value == _imageCropStartX)
            {
                return;
            }

            _imageCropStartX = value;
            NotifyPropertyChanged();
        }
    }

    private int _imageCropStartY;

    public int ImageCropStartY
    {
        get => _imageCropStartY;
        set
        {
            if (value == _imageCropStartY)
            {
                return;
            }

            _imageCropStartY = value;
            NotifyPropertyChanged();
        }
    }

    private int _imageCropEndX;

    public int ImageCropEndX
    {
        get => _imageCropEndX;
        set
        {
            if (value == _imageCropEndX)
            {
                return;
            }

            _imageCropEndX = value;
            NotifyPropertyChanged();
        }
    }

    private int _imageCropEndY;

    public int ImageCropEndY
    {
        get => _imageCropEndY;
        set
        {
            if (value == _imageCropEndY)
            {
                return;
            }

            _imageCropEndY = value;
            NotifyPropertyChanged();
        }
    }

    public byte[] ImagePreviewSource
    {
        get => _imagePreviewSource;
        set
        {
            _imagePreviewSource = value;
            NotifyPropertyChanged();
        }
    }

    public string FilePath { get; set; }

    public string Timestamp
    {
        get => _timestamp;
        set {
            if (_timestamp == value)
            {
                return;
            }

            _timestamp = value;
            NotifyPropertyChanged();
        }
    } // TODO Maybe use a TimeSpan instead of string?

    public double CurrentFps { get; set; }

    #endregion

    #region Events

    private void BtnConvert_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.MessageBox.Show(
            $"IsCropEnabled: {IsCropEnabled}" + Environment.NewLine +
            $"IsTrimEnabled: {IsTrimEnabled}" + Environment.NewLine +
            $"IsUsingDuration: {IsUsingDuration}" + Environment.NewLine +
            $"StartPoint: {ImageCropStartX} - {ImageCropStartY} " + Environment.NewLine +
            $"EndPoint:{ImageCropEndX} - {ImageCropEndY}");
    }

    private async void BtnGifski_OnClick(object sender, RoutedEventArgs e)
    {
        if (await EnsureGifskiInstalled() && await EnsureFfmpegInstalled())
        {
            // TODO: Run ffmpeg to get frames and gifski to convert to gif
        }
        else
        {
            Util.LogToUser("You need FFmpeg and gifski installed to use gifski.");
        }
    }

    private bool _isFileTextboxEventDisabled;
    private byte[] _imagePreviewSource;
    private string _timestamp = "00:00:00.000";

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog();
        if (!openFileDialog.ShowDialog().GetValueOrDefault())
        {
            return;
        }

        _isFileTextboxEventDisabled = true;
        FilePath = openFileDialog.FileName;
        LoadFile();
        TxtFilePath.Text = openFileDialog.FileName;
        TxtFilePath.Focus();
        TxtFilePath.CaretIndex = TxtFilePath.Text.Length;
        TxtFilePath.ScrollToEnd();
        _isFileTextboxEventDisabled = false;
    }

    private void TxtFilePath_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (_isFileTextboxEventDisabled)
        {
            return;
        }

        if (TxtFilePath.Text == FilePath)
        {
            return;
        }

        FilePath = TxtFilePath.Text;
        LoadFile();
    }

    private void TxtTimestamp_OnLostFocus(object sender, RoutedEventArgs e)
    {
        HandleTimestampChanged();
    }


    private void TxtTimestamp_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        HandleTimestampChanged();
    }
    
    private void BtnRewindFast_OnClick(object sender, RoutedEventArgs e)
    {
        CalculateNewTimestamp(TimeSpan.FromSeconds(-1));
    }

    private void BtnRewind_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void BtnForward_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void BtnForwardFast_OnClick(object sender, RoutedEventArgs e)
    {
        CalculateNewTimestamp(TimeSpan.FromSeconds(1));
    }

    private void MenuDurationSwitch_Click(object sender, RoutedEventArgs e)
    {
        ToggleUsingDuration();
    }

    private void GrdToAndDurationGrid_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleUsingDuration();
        }
    }

    private void GridImagePreview_OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition((Grid) sender);
        if (ImageCropMouseDown)
        {
            ImageCropEndXPreview = (int) pos.X;
            ImageCropEndYPreview = (int) pos.Y;
        }
        else
        {
            ImageCropStartXPreview = (int) pos.X;
            ImageCropStartYPreview = (int) pos.Y;
        }
    }

    private void GridImagePreview_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsCropEnabled)
        {
            return;
        }

        ImageCropMouseDown = true;
        ImageCropStartX = ImageCropStartXPreview;
        ImageCropStartY = ImageCropStartYPreview;
    }

    private void GridImagePreview_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsCropEnabled)
        {
            return;
        }

        ImageCropEndX = ImageCropEndXPreview;
        ImageCropEndY = ImageCropEndYPreview;
        ImageCropMouseDown = false;
    }

    #endregion
}
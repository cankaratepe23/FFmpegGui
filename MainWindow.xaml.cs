using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace FFmpegGui;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : INotifyPropertyChanged
{
    private static string _userLogText = "";

    public MainWindow()
    {
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

    private void LoadFile(string inputFilePath)
    {
        MessageBox.Show($"Loaded file: {inputFilePath}");
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

    #endregion

    #region Events

    private void BtnConvert_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            $"IsCropEnabled: {IsCropEnabled}" + Environment.NewLine +
            $"IsTrimEnabled: {IsTrimEnabled}" + Environment.NewLine +
            $"IsUsingDuration: {IsUsingDuration}" + Environment.NewLine +
            $"StartPoint: {ImageCropStartX} - {ImageCropStartY} " + Environment.NewLine +
            $"EndPoint:{ImageCropEndX} - {ImageCropEndY}");
    }

    private async void BtnGifski_OnClick(object sender, RoutedEventArgs e)
    {
        if (!await GifskiService.IsGifskiInstalledAsync())
        {
            var style = new Style();
            style.Setters.Add(new Setter(Xceed.Wpf.Toolkit.MessageBox.CancelButtonContentProperty, "Go to website..."));
            var result = Xceed.Wpf.Toolkit.MessageBox.Show(
                "Could not find gifski." + Environment.NewLine +
                "Install it by downloading the latest command-line binaries from https://gif.ski/ " +
                "and either add it to your PATH or place it in"
                + Environment.NewLine + Directory.GetCurrentDirectory()
                + Environment.NewLine + Environment.NewLine +
                "Would you like to try to install gifski automatically?",
                "Could not find gifski",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning,
                MessageBoxResult.No,
                style);
            // TODO: Complete this
            if (MessageBoxResult.Yes == result)
            {
                await Task.Run(GifskiService.InstallGifskiAsync);
            }
            else if (MessageBoxResult.Cancel == result)
            {
                // Launch gifski website
            }
        }
        else
        {
            MessageBox.Show("Gifski is installed.");
        }
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog();
        if (!openFileDialog.ShowDialog().GetValueOrDefault())
        {
            return;
        }

        LoadFile(openFileDialog.FileName);
        TxtFilePath.Text = openFileDialog.FileName;
        TxtFilePath.Focus();
        TxtFilePath.CaretIndex = TxtFilePath.Text.Length;
        TxtFilePath.ScrollToEnd();
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
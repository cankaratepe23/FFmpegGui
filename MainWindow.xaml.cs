using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    public MainWindow()
    {
        InitializeComponent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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

    #endregion

    #region Events

    private void BtnConvert_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            $"IsCropEnabled: {IsCropEnabled}\n" +
            $"IsTrimEnabled: {IsTrimEnabled}\n" +
            $"IsUsingDuration: {IsUsingDuration}\n" +
            $"StartPoint: {_imageCropStartPoint.X} - {_imageCropStartPoint.Y} \n" +
            $"EndPoint:{_imageCropEndPoint.X} - {_imageCropEndPoint.Y}");
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

    #endregion


    private bool _trackCropStartPoint = true;
    private Point _imageCropStartPoint;
    private Point _imageCropEndPoint;

    private void ImageFramePreview_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        _trackCropStartPoint = false;
        _imageCropStartPoint = e.GetPosition((Image)sender);
    }

    private void ImageFramePreview_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _imageCropEndPoint = e.GetPosition((Image)sender);
        _trackCropStartPoint = true;
    }

    private void ImageFramePreview_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_trackCropStartPoint)
        {
            return;
        }

        _imageCropStartPoint = e.GetPosition((Image) sender);
    }
}
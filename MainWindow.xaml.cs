using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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
            $"IsCropEnabled: {IsCropEnabled}\nIsTrimEnabled: {IsTrimEnabled}\nIsUsingDuration: {IsUsingDuration}");
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
}
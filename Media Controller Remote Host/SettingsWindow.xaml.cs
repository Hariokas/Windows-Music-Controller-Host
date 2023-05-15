using System.Windows;

namespace Media_Controller_Remote_Host;

/// <summary>
///     Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindow(string currentIp, string currentPort)
    {
        InitializeComponent();
        IpAddress = currentIp;
        Port = currentPort;
        DataContext = this;
    }

    public string IpAddress { get; set; }
    public string Port { get; set; }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
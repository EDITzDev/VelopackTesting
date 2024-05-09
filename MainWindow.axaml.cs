using System.Text;

using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Interactivity;

using Velopack;
using Velopack.Sources;

namespace VelopackTesting;

public partial class MainWindow : Window
{
    private UpdateInfo? _update;
    private UpdateManager _updateManager;

    public MainWindow()
    {
        InitializeComponent();

        _updateManager = new UpdateManager(new GithubSource("https://github.com/EDITzDev/VelopackTesting", null, false));

        UpdateStatus();
    }

    private async void BtnCheckUpdateClick(object sender, RoutedEventArgs e)
    {
        Working();

        try
        {
            // ConfigureAwait(true) so that UpdateStatus() is called on the UI thread
            _update = await _updateManager.CheckForUpdatesAsync().ConfigureAwait(true);
        }
        catch
        {
            TextLog.Text = "Error checking for updates";
        }

        UpdateStatus();
    }

    private async void BtnDownloadUpdateClick(object sender, RoutedEventArgs e)
    {
        if (_update is null)
            return;

        Working();

        try
        {
            // ConfigureAwait(true) so that UpdateStatus() is called on the UI thread
            await _updateManager.DownloadUpdatesAsync(_update, Progress).ConfigureAwait(true);
        }
        catch
        {
            TextLog.Text = "Error downloading updates";
        }

        UpdateStatus();
    }

    private void BtnRestartApplyClick(object sender, RoutedEventArgs e)
    {
        if (_update is null)
            return;

        _updateManager.ApplyUpdatesAndRestart(_update);
    }

    private void Progress(int percent)
    {
        Dispatcher.UIThread.InvokeAsync(() => TextStatus.Text = $"Downloading ({percent}%)...");
    }

    private void Working()
    {
        BtnCheckUpdate.IsEnabled = false;
        BtnRestartApply.IsEnabled = false;
        BtnDownloadUpdate.IsEnabled = false;

        TextStatus.Text = "Working...";
    }

    private void UpdateStatus()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Velopack version: {VelopackRuntimeInfo.VelopackNugetVersion}");
        stringBuilder.AppendLine($"This app version: {(_updateManager.IsInstalled ? _updateManager.CurrentVersion : "(n/a - not installed)")}");

        if (_update is not null)
        {
            stringBuilder.AppendLine($"Update available: {_update.TargetFullRelease.Version}");
            BtnDownloadUpdate.IsEnabled = true;
        }
        else
        {
            BtnDownloadUpdate.IsEnabled = false;
        }

        if (_updateManager.IsUpdatePendingRestart)
        {
            stringBuilder.AppendLine("Update ready, pending restart to install");
            BtnRestartApply.IsEnabled = true;
        }
        else
        {
            BtnRestartApply.IsEnabled = false;
        }

        TextStatus.Text = stringBuilder.ToString();

        BtnCheckUpdate.IsEnabled = true;
    }

}
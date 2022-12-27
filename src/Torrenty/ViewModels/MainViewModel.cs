using MonoTorrent.Client;
using MonoTorrent;

namespace Torrenty.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    [ObservableProperty]
    bool failure = false;

    [ObservableProperty]
    string message = "Press the download button";

    [ObservableProperty]
    double percent = 0.0;

    [ObservableProperty]
    string results = "...";

    [ObservableProperty]
    bool success = false;

    [RelayCommand]
    async Task Cancel()
    {
        InvokeCommand.Cancel();
        await Task.Delay(5000);
    }

    [RelayCommand(IncludeCancelCommand = true)]
    async Task Invoke(CancellationToken token)
    {
        try
        {
            Failure = false;
            Message = "Download is in progress";
            Results = "...";
            Success = false;
            var storage = FileSystem.CacheDirectory;
            var configs = new EngineSettingsBuilder { CacheDirectory = storage };
            var handler = new ClientEngine(settings: configs.ToSettings());
            var deposit = Directory.CreateDirectory(Path.Combine(storage, Path.GetRandomFileName()[..8])).FullName;
            var address = @"
                magnet:?xt=urn:btih:EA149D4C641AA048F65CD0B61DA8CEDC5819F6C1&dn=Marvel+Week%2B+%2812-14-2022%29+%28-+Ne
                m+-%29&tr=udp%3A%2F%2Finferno.demonoid.is%3A3391%2Fannounce&tr=udp%3A%2F%2Fmovies.zsw.ca%3A6969%2Fannou
                nce&tr=https%3A%2F%2Finferno.demonoid.is%2Fannounce&tr=udp%3A%2F%2Ffe.dealclub.de%3A6969%2Fannounce&tr=
                udp%3A%2F%2Fadmin.videoenpoche.info%3A6969%2Fannounce&tr=http%3A%2F%2Fshare.camoe.cn%3A8080%2Fannounce&
                tr=http%3A%2F%2Ft.nyaatracker.com%3A80%2Fannounce&tr=udp%3A%2F%2Ftracker.tiny-vps.com%3A6969%2Fannounce
                &tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.dler.org%3A6969%2Fannou
                nce&tr=udp%3A%2F%2Ftracker.openbittorrent.com%3A6969%2Fannounce&tr=udp%3A%2F%2Fretracker.lanta-net.ru%3
                A2710%2Fannounce&tr=udp%3A%2F%2Ftracker.internetwarriors.net%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.o
                penbittorrent.com%3A80%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337%2Fannounce&tr=http%3A%2F
                %2Ftracker.openbittorrent.com%3A80%2Fannounce&tr=udp%3A%2F%2Fopentracker.i2p.rocks%3A6969%2Fannounce&tr
                =udp%3A%2F%2Ftracker.internetwarriors.net%3A1337%2Fannounce&tr=udp%3A%2F%2Ftracker.leechers-paradise.or
                g%3A6969%2Fannounce&tr=udp%3A%2F%2Fcoppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.zer0day.to%
                3A1337%2Fannounce
            ";
            var manager = await handler.AddAsync(MagnetLink.Parse(address), deposit);
            await manager.StartAsync();
            await manager.WaitForMetadataAsync(token);
            while (manager.State is not TorrentState.Seeding)
            {
                Percent = manager.Progress;
                await Task.Delay(TimeSpan.FromSeconds(2), token);
            }
            await manager.StopAsync();
            var element = Directory.GetDirectories(deposit).First();
            Message = "Download finished succcessfully";
            Percent = 100;
            Results = Directory.GetFiles(element).First();
            Success = true;
        }
        catch (OperationCanceledException)
        {
            Failure = true;
            Message = "Download was canceled";
            Percent = 0;
            Success = false;
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using Context.App;
using Context.App.Views;
using Context.Infrastructure;
using Context.Infrastructure.Persistence;
using Context.Presentation;
using Context.Presentation.Services;
using Context.Presentation.ViewModels;
using Dictionary;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Music;
using Plugin;
using Wallpaper;

namespace Context.Tests;

/// <summary>
/// 插件组合集成测试：按组合根的完整装配验证插件目录聚合（命令 + 设置）、
/// 建表，以及设置页在 STA 窗口 Loaded 路径下的真实初始化。
/// </summary>
public class PluginCompositionTests
{
    [Fact]
    public async Task FullComposition_AggregatesCommandsAndSettings_AndCreatesTables()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"context-composition-{Guid.NewGuid():N}.db");
        var services = new ServiceCollection();
        services.AddPresentation()
            .AddInfrastructure($"Data Source={dbPath}")
            .AddHotkeys()
            .AddDictionary()
            .AddWallpaper()
            .AddMusic();

        using var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<DatabaseInitializer>().InitializeAsync();

        var catalog = provider.GetRequiredService<IPluginCatalog>();

        Assert.Contains(catalog.GetCommands(), declaration => declaration.Id == "music.next");
        var setting = Assert.Single(catalog.GetSettings());
        Assert.Equal("music.libraryDirectory", setting.Id);

        Assert.Contains(await ListTablesAsync(dbPath), table => table == "plugin_settings");
    }

    [Fact]
    public void SettingsWindow_Loaded_UnderSta_PopulatesHotkeysAndPluginSettings()
    {
        Exception? error = null;
        var done = new ManualResetEventSlim(false);
        var hotkeyIds = new List<string>();
        var settingIds = new List<string>();

        // WPF 窗口需 STA 线程；xUnit 工作线程为 MTA
        var thread = new Thread(() =>
        {
            try
            {
                var dbPath = Path.Combine(Path.GetTempPath(), $"context-composition-{Guid.NewGuid():N}.db");
                var services = new ServiceCollection();
                services.AddPresentation()
                    .AddInfrastructure($"Data Source={dbPath}")
                    .AddWindows()
                    .AddHotkeys()
                    .AddDictionary()
                    .AddWallpaper()
                    .AddMusic();

                using var provider = services.BuildServiceProvider();
                provider.GetRequiredService<DatabaseInitializer>().InitializeAsync().GetAwaiter().GetResult();
                provider.GetRequiredService<PluginSettingsApplier>().ApplyAllAsync().GetAwaiter().GetResult();

                var window = provider.GetRequiredService<SettingsWindow>();
                var settingsViewModel = (SettingsViewModel)window.DataContext;
                var asyncCommand = (IAsyncRelayCommand)settingsViewModel.InitializeCommand;

                // 真实 Show：Loaded → 代码后置 OnLoaded → 初始化命令
                window.Show();
                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.ElapsedMilliseconds < 5000
                       && asyncCommand.ExecutionTask is { IsCompleted: false })
                {
                    PumpDispatcher();
                }

                // 命令必须已被窗口 Loaded 路径真实执行（守护 Loaded 初始化方式不再回退为失效的触发器）
                Assert.NotNull(asyncCommand.ExecutionTask);
                Assert.True(asyncCommand.ExecutionTask!.IsCompleted, "初始化命令 5 秒内未完成");
                Assert.Null(asyncCommand.ExecutionTask.Exception?.GetBaseException());

                hotkeyIds.AddRange(settingsViewModel.Hotkeys.Select(item => item.CommandId));
                settingIds.AddRange(settingsViewModel.Settings.Select(item => item.SettingId));
                window.Close();
            }
            catch (Exception exception)
            {
                error = exception;
            }
            finally
            {
                done.Set();
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(done.Wait(30000), "STA 线程超时");
        if (error is not null)
        {
            throw error;
        }

        Assert.Contains("music.next", hotkeyIds);
        Assert.Equal(["music.libraryDirectory"], settingIds);
    }

    private static void PumpDispatcher()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static async Task<List<string>> ListTablesAsync(string path)
    {
        var tables = new List<string>();
        await using var connection = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = path, Mode = SqliteOpenMode.ReadOnly }.ToString());
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}

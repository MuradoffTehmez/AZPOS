using System.Data.Common;
using System.Runtime.InteropServices;
using MarketPOS.Application;
using MarketPOS.Infrastructure;
using MarketPOS.Sync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MarketPOS.UI;

/// <summary>
/// Application entry point: configures logging, the DI container (Generic Host)
/// and top-level structured error handling with distinct exit codes.
/// </summary>
internal static class Program
{
    private const int ExitOk = 0;
    private const int ExitDatabaseError = 1;
    private const int ExitHardwareError = 2;
    private const int ExitUnexpectedError = 99;

    [STAThread]
    private static int Main()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", "marketpos-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();

        try
        {
            ApplicationConfiguration.Initialize();

            using var host = CreateHost();
            host.Start();

            using (var loginForm = host.Services.GetRequiredService<LoginForm>())
            {
                if (loginForm.ShowDialog() != DialogResult.OK)
                {
                    host.StopAsync().GetAwaiter().GetResult();
                    return ExitOk;
                }
            }

            var mainForm = host.Services.GetRequiredService<MainForm>();
            System.Windows.Forms.Application.Run(mainForm);

            host.StopAsync().GetAwaiter().GetResult();
            return ExitOk;
        }
        catch (DbException ex)
        {
            // Transient DB failures are retried lower down (TransientRetry); reaching
            // here means retries were exhausted or the failure is permanent.
            Log.Fatal(ex, "Database error — application terminated");
            ShowFatalError("Verilənlər bazası ilə əlaqə mümkün olmadı. Zəhmət olmasa sistem inzibatçısına müraciət edin.");
            return ExitDatabaseError;
        }
        catch (COMException ex)
        {
            Log.Fatal(ex, "Hardware (COM) error — application terminated");
            ShowFatalError("Kassa avadanlığı ilə əlaqə xətası baş verdi. Cihaz bağlantılarını yoxlayıb yenidən cəhd edin.");
            return ExitHardwareError;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unexpected error — application terminated");
            ShowFatalError("Gözlənilməz xəta baş verdi. Zəhmət olmasa sistem inzibatçısına müraciət edin.");
            return ExitUnexpectedError;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHost CreateHost() =>
        Host.CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                services.AddApplication();
                services.AddInfrastructure(context.Configuration);
                services.AddHostedService<SyncBackgroundService>();
                services.AddTransient<LoginForm>();
                services.AddTransient<CheckoutForm>();
                services.AddSingleton<MainForm>();
            })
            .Build();

    private static void ShowFatalError(string message) =>
        MessageBox.Show(message, "MarketPOS — Kritik xəta", MessageBoxButtons.OK, MessageBoxIcon.Error);
}

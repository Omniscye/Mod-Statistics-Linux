using Avalonia;
using Avalonia.Themes.Fluent;
using System;

namespace ModStatistics
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")))
            {
                Environment.SetEnvironmentVariable("DISPLAY", ":0");
            }
            
            DotNetEnv.Env.Load();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }

    public class App : Application
    {
        public override void Initialize()
        {
            Styles.Add(new FluentTheme());
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
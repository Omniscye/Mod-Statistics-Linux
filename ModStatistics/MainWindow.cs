using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace ModStatistics
{
    public class MainWindow : Window
    {
        private TextBlock _logs;
        private Button _runBtn;
        private ScrollViewer _scroll;

        public MainWindow()
        {
            Title = "Omniscye Mod Statistics";
            Width = 800;
            Height = 600;
            SystemDecorations = SystemDecorations.Full;

            var titleBlock = new TextBlock
            {
                Text = "Mod Statistics Dashboard",
                FontSize = 24,
                FontWeight = FontWeight.Bold,
                Margin = new Thickness(0, 0, 0, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            DockPanel.SetDock(titleBlock, Dock.Top);

            _runBtn = new Button
            {
                Content = "Run Statistics Fetch",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(10),
                Background = Brushes.DarkSlateBlue,
                Foreground = Brushes.White,
                Cursor = new Cursor(StandardCursorType.Hand) 
            };

            var btnBorder = new Border { Child = _runBtn };
            DockPanel.SetDock(btnBorder, Dock.Top);

            _logs = new TextBlock
            {
                Text = "Ready to fetch...",
                FontFamily = "Consolas, Monospace",
                TextWrapping = TextWrapping.Wrap
            };

            _scroll = new ScrollViewer
            {
                Content = _logs,
                Margin = new Thickness(0, 10, 0, 0),
                Background = Brushes.Black,
                Padding = new Thickness(10)
            };

            var scrollBorder = new Border 
            { 
                Child = _scroll, 
                BorderBrush = Brushes.Gray, 
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5)
            };

            var mainStack = new DockPanel
            {
                Margin = new Thickness(20)
            };

            mainStack.Children.Add(titleBlock);
            mainStack.Children.Add(btnBorder);
            mainStack.Children.Add(scrollBorder);
            
            Content = mainStack;

            _runBtn.Click += async (s, e) => await RunProcess();
        }

        private async Task RunProcess()
        {
            _runBtn.IsEnabled = false;
            _logs.Text = ""; 
            
            await Task.Run(async () => 
            {
                await ModStatsLogic.Run(LogUpdate);
            });

            _runBtn.IsEnabled = true;
        }

        private void LogUpdate(string message)
        {
            Dispatcher.UIThread.Post(() => 
            {
                _logs.Text += message + "\n";
                _scroll.ScrollToEnd();
            });
        }
    }
}
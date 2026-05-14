using System.Windows;
using GemmaChatWindows.ViewModels;
using GemmaChatWindows.Services;

namespace GemmaChatWindows
{
    public partial class MainWindow : Window
    {
        private bool _isPseudoMaximized;
        private Rect _restoreBounds;

        public MainWindow()
        {
            InitializeComponent();
            
            var llamaService = new LlamaService();
            var workspaceService = new WorkspaceService();
            var viewModel = new ChatViewModel(llamaService, workspaceService);
            
            DataContext = viewModel;

            InitializeWebViewAsync();

            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ChatViewModel.PreviewSource))
                {
                    UpdatePreview(viewModel.PreviewSource);
                }
            };
        }

        private async void InitializeWebViewAsync()
        {
            try 
            {
                // Force white background before initialization
                PreviewWebView.DefaultBackgroundColor = System.Drawing.Color.White;
                
                await PreviewWebView.EnsureCoreWebView2Async(null);
                
                // Match the original: white bg and light theme for the preview
                PreviewWebView.CoreWebView2.Profile.PreferredColorScheme = Microsoft.Web.WebView2.Core.CoreWebView2PreferredColorScheme.Light;
                PreviewWebView.DefaultBackgroundColor = System.Drawing.Color.White;

                var vm = (ChatViewModel)DataContext;
                if (!string.IsNullOrEmpty(vm.PreviewSource))
                    PreviewWebView.CoreWebView2.Navigate(vm.PreviewSource);
            }
            catch (System.Exception) { }
        }

        private void UpdatePreview(string url)
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (PreviewWebView.CoreWebView2 != null && !string.IsNullOrEmpty(url))
                {
                    PreviewWebView.CoreWebView2.Navigate(url);
                }
            });
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeClick(object sender, RoutedEventArgs e)
        {
            if (_isPseudoMaximized)
            {
                Left = _restoreBounds.Left;
                Top = _restoreBounds.Top;
                Width = _restoreBounds.Width;
                Height = _restoreBounds.Height;
                _isPseudoMaximized = false;
                return;
            }

            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;

            _restoreBounds = new Rect(Left, Top, Width, Height);
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Left;
            Top = workArea.Top;
            Width = workArea.Width;
            Height = workArea.Height;
            _isPseudoMaximized = true;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

using Microsoft.UI.Xaml;

namespace SingleInstanceExample
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        public void ShowArguments(string arguments)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                tbArguments.Text = arguments;
            });
        }
    }
}

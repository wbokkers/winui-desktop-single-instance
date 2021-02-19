using Microsoft.UI.Xaml;
using WimBokkers.WinUI;
using Windows.ApplicationModel;


namespace SingleInstanceExample
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private readonly SingleInstanceDesktopApp _singleInstanceApp;
        private MainWindow _window;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            _singleInstanceApp = new SingleInstanceDesktopApp("SPECIFY_A_UNIQUE_ID_FOR_YOUR_APP");
            _singleInstanceApp.Launched += OnSingleInstanceLaunched;
        }

        private void OnSingleInstanceLaunched(object sender, SingleInstanceLaunchEventArgs e)
        {
            if (e.IsFirstLaunch)
            {
                _window = new MainWindow();
                _window.Activate();
            }

            // Show the command line arguments in the window
            _window.ShowArguments(e.Arguments);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _singleInstanceApp.Launch(args.Arguments);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            // Save application state and stop any background activity
        }
    }
}

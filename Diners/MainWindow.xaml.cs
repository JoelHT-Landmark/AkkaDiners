namespace Diners
{
    using Serilog.Events;
    using System;
    using System.Windows;
    using System.Threading.Tasks;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TextBoxOutputter outputter;

        private DinersSystem system;

        private readonly App app = App.Current as App;

        public MainWindow()
        {
            InitializeComponent();

            this.SetupDinersSystem();
            this.StartButton.Click += StartDining;
            this.LoggingLevelSlider.ValueChanged += LoggingLevelChanged;

            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);
        }

        private void SetupDinersSystem()
        {
            Task.Factory.StartNew(() => {
                this.system = new DinersSystem();
            });
        }
        private void LoggingLevelChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var levelValue = Convert.ToInt32(e.NewValue);
            var level = (LogEventLevel) Enum.Parse(typeof(LogEventLevel), levelValue.ToString());

            app.SetLoggingLevel(level);

            this.LevelName.Content = level.ToString();
        }

        private void StartDining(object sender, RoutedEventArgs e)
        {
            this.system.StartDining();
        }


    }
}

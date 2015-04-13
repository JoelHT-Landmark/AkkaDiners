namespace Diners
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TextBoxOutputter outputter;

        private DinersSystem system;

        public MainWindow()
        {
            InitializeComponent();

            this.StartButton.Click += StartDining;

            outputter = new TextBoxOutputter(TestBox);
            Console.SetOut(outputter);
        }

        private void StartDining(object sender, RoutedEventArgs e)
        {
            this.system = new DinersSystem();
            this.system.StartDining();
        }
    }
}

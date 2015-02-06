using System.Windows;

namespace K4W.KinectDrone.Client
{
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        // Write logging
        private void WriteLog(string output)
        {
            Output.Content = "> " + output;
        }

    }
}

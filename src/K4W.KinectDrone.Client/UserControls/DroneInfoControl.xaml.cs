using System.ComponentModel;
using System.Windows;
using AR.Drone.Data.Navigation;
using K4W.KinectDrone.Core.Enums;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Windows.Media;

namespace K4W.KinectDrone.Client.UserControls
{
    /// <summary>
    /// Interaction logic for KinectConnectivityControl.xaml
    /// </summary>
    public partial class DroneInfoControl : UserControl
    {

        #region Properties
        public static readonly DependencyProperty ConnectionProperty = DependencyProperty.Register("Connection",
                                                                                                      typeof(DroneConnection),
                                                                                                      typeof(DroneInfoControl),
                                                                                                      new PropertyMetadata(DroneConnection.NotConnected));

        [Category("Kinecting for Windows"), Description("Default value for drone status")]
        public DroneConnection Connection
        {
            get { return (DroneConnection)GetValue(ConnectionProperty); }
            set { SetValue(ConnectionProperty, value); }
        }

        public static readonly DependencyProperty CurrentStateProperty = DependencyProperty.Register("CurrentState",
                                                                                                      typeof(NavigationState),
                                                                                                      typeof(DroneInfoControl),
                                                                                                      new PropertyMetadata(null));

        [Category("Kinecting for Windows"), Description("Default value for drone status")]
        public NavigationState CurrentState
        {
            get { return (NavigationState)GetValue(CurrentStateProperty); }
            set { SetValue(CurrentStateProperty, value); }
        }


        public static readonly DependencyProperty BatteryLevelProperty = DependencyProperty.Register("BatteryLevel",
                                                                                                     typeof(int),
                                                                                                     typeof(DroneInfoControl),
                                                                                                     new PropertyMetadata(0));

        [Category("Kinecting for Windows"), Description("Default value for drone battery level")]
        public int BatteryLevel
        {
            get { return (int)GetValue(BatteryLevelProperty); }
            set { SetValue(BatteryLevelProperty, value); }
        }
        #endregion Properties

        public DroneInfoControl()
        {
            InitializeComponent();
        }
    }
}
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Kinect;
using System.Windows.Controls;
using System.Windows.Media;

namespace K4W.KinectDrone.Client.UserControls
{
    /// <summary>
    /// Interaction logic for KinectConnectivityControl.xaml
    /// </summary>
    public partial class KinectInfoControl : UserControl
    {
        public static readonly DependencyProperty CurrentStatusProperty = DependencyProperty.Register("CurrentStatus",
                                                                                                      typeof(KinectStatus),
                                                                                                      typeof(KinectInfoControl),
                                                                                                      new PropertyMetadata(KinectStatus.Undefined));

        [Category("Kinecting for Windows"), Description("Default value for drone status")]
        public KinectStatus CurrentStatus
        {
            get { return (KinectStatus)GetValue(CurrentStatusProperty); }
            set { SetValue(CurrentStatusProperty, value); }
        }

        public KinectInfoControl()
        {
            InitializeComponent();
        }
    }
}
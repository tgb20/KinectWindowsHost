using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Solid.Arduino;
using Solid.Arduino.Firmata;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace KinectWIndowsHost
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        private KinectSensor sensor;
        private ISerialConnection connection;
        private ArduinoSession session;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindowLoaded;
        }

        private void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var sensorStatus = new KinectSensorChooser();
            sensorStatus.KinectChanged += KinectSensorChooserKinectChanged;
            kinectChooser.KinectSensorChooser = sensorStatus;
            sensorStatus.Start();

            connection = GetConnection();

            if (connection != null)
            {
                session = new ArduinoSession(connection);
            }
        }

        private static ISerialConnection GetConnection()
        {
            Console.WriteLine("Searching Arduino connection...");
            ISerialConnection connection = EnhancedSerialConnection.Find();

            if (connection == null)
                Console.WriteLine("No connection found. Make shure your Arduino board is attached to a USB port.");
            else
                Console.WriteLine($"Connected to port {connection.PortName} at {connection.BaudRate} baud.");

            return connection;
        }

        private void KinectSensorChooserKinectChanged(object sender, KinectChangedEventArgs e)
        {
            if(sensor != null)
            {
                sensor.SkeletonFrameReady -= KinectSkeletonFrameReady;
            }
            sensor = e.NewSensor;
            if(sensor == null)
            {
                return;
            }

            KinectStatus.Content = Convert.ToString(e.NewSensor.Status);

            sensor.SkeletonStream.Enable();
            sensor.SkeletonFrameReady += KinectSkeletonFrameReady;
        }

        private void KinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            var skeletons = new Skeleton[0];
            using(var skeletonFrame = e.OpenSkeletonFrame())
            {
                if(skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                }
            }

            if(skeletons.Length == 0)
            {
                return;
            }

            var skel = skeletons.FirstOrDefault(x => x.TrackingState == SkeletonTrackingState.Tracked);
            if(skel == null)
            {
                return;
            }
 
            var leftHand = skel.Joints[JointType.WristLeft];
            var rightHand = skel.Joints[JointType.WristRight];
            var shoulder = skel.Joints[JointType.ShoulderCenter];
            var leftElbow = skel.Joints[JointType.ElbowLeft];
            var rightElbow = skel.Joints[JointType.ElbowRight];


            XValueLeft.Text = leftHand.Position.X.ToString(CultureInfo.InvariantCulture);
            YValueLeft.Text = leftHand.Position.Y.ToString(CultureInfo.InvariantCulture);
            ZValueLeft.Text = leftHand.Position.Z.ToString(CultureInfo.InvariantCulture);

            // Check if at height
            if(leftHand.Position.Y >= shoulder.Position.Y - 0.15 && rightHand.Position.Y >= shoulder.Position.Y - 0.15)
            {
                InTPos.Text = "Yes";
                if(session != null)
                {
                    session.WriteLine("yes");
                }
            }
            else
            {
                InTPos.Text = "No";
                session.WriteLine("no");
            } 
        }
    }
}

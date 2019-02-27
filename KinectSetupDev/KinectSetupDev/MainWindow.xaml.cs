using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
namespace KinectSetupDev
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        const float MaxDepthDistance = 4095; // max value returned
        const float MinDepthDistance = 850; // min value returned
        const float MaxDepthDistanceOffset = MaxDepthDistance - MinDepthDistance;

        //KinectSensor _sensor;
        /*
            private void SetupKinectManually()
            {

                if (KinectSensor.KinectSensors.Count > 0)
                {
                    //use first Kinect
                    _sensor = KinectSensor.KinectSensors[0];
                            MessageBox.Show("Kinect Status = " + _sensor.Status.ToString()); 
                }
                else
                {
                    MessageBox.Show("No Kinects are connected"); 
                }


            }
            */


            private void StopKinect(KinectSensor sensor)
            {
                if (sensor != null)
                {
                    if (sensor.IsRunning)
                    {
                        //stop sensor 
                        sensor.Stop();

                        //stop audio if not null
                        if (sensor.AudioSource != null)
                        {
                            sensor.AudioSource.Stop();
                        }


                    }
                }
            }

            private void KinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                KinectSensor oldSensor = (KinectSensor)e.OldValue;

                StopKinect(oldSensor);

                KinectSensor newSensor = (KinectSensor)e.NewValue;

                if (newSensor == null)
                {
                    return;
                }

                //register for event and enable Kinect sensor features you want
                newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);
                newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                newSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                newSensor.SkeletonStream.Enable();


                try
                {
                    newSensor.Start();
                }
                catch (System.IO.IOException)
                {
                    //another app is using Kinect
                    KinectSensorChooser1.AppConflictOccurred();
                }



            }

            void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
            {

                using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
                {
                    if (depthFrame == null)
                    {
                        return;
                    }
                    byte[] pixels = GenerateColoredBytes(depthFrame);


                    int stride = depthFrame.Width * 4;
                    image1.Source =
                        BitmapSource.Create(depthFrame.Width, depthFrame.Height,
                        96, 96, PixelFormats.Bgr32, null, pixels, stride);
                }
            }
            private byte[] GenerateColoredBytes(DepthImageFrame depthFrame)
            {
                short[] rawDepthData = new short[depthFrame.PixelDataLength];
                depthFrame.CopyPixelDataTo(rawDepthData);
                Byte[] pixels = new byte[depthFrame.Height * depthFrame.Width * 4];
                const int BlueIndex = 0;
                const int GreenIndex = 1;
                const int RedIndex = 2;

                for (int depthIndex = 0, colorIndex = 0;
                    depthIndex < rawDepthData.Length && colorIndex < pixels.Length;
                    depthIndex++, colorIndex += 4)
                {
                    // get the player
                    int player = rawDepthData[depthIndex] & DepthImageFrame.PlayerIndexBitmask;

                    //get the depth value
                    int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                    //0.9M
                    //if(depth <= 900)
                    //{
                    //we are very close
                    //  pixels[colorIndex + BlueIndex] = 255;
                    //   pixels[colorIndex + GreenIndex] = 0;
                    //   pixels[colorIndex + RedIndex] = 0;
                    // }
                    // 0.9 -2M 
                    // else if (depth > 900 && depth < 2000)
                    // {
                    // we are at fine distance
                    //  pixels[colorIndex + BlueIndex] = 0;
                    //   pixels[colorIndex + GreenIndex] = 255;
                    //  pixels[colorIndex + RedIndex] = 0;
                    // }
                    // +2M
                    // else if (depth > 2000)
                    //{
                    //we are far away
                    //  pixels[colorIndex + BlueIndex] = 0;
                    //  pixels[colorIndex + GreenIndex] = 0;
                    //  pixels[colorIndex + RedIndex] = 255;
                    // }
                   // equal coloring for monochromatic histogram
                    byte intensity = CalculateIntensityFromDepth(depth);
                    pixels[colorIndex + BlueIndex] = intensity;
                    pixels[colorIndex + GreenIndex] = intensity;
                    pixels[colorIndex + RedIndex] = intensity;


                    //Color all players "gold"
                    //if (player > 0)
                    // {
                    //    pixels[colorIndex + BlueIndex] = Colors.Gold.B;
                    //    pixels[colorIndex + GreenIndex] = Colors.Gold.G;
                    //    pixels[colorIndex + RedIndex] = Colors.Gold.R;
                    //  }

                }
                return pixels;
            }

            public static byte CalculateIntensityFromDepth(int distance)
            {
               // formula for calculating monochrome intensity for histogram
                return (byte)(255 - (255 * Math.Max(distance - MinDepthDistance, 0)
                    / (MaxDepthDistanceOffset)));
            }





            private void Window_Loaded_1(object sender, RoutedEventArgs e)
            {
                //SetupKinectManually();
                KinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(KinectSensorChooser1_KinectSensorChanged);

            }

            private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
            {
                StopKinect(KinectSensorChooser1.Kinect);
            }
        }

    }

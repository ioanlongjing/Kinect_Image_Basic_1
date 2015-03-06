using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO; // Add Kinect Konect 的Function. 

namespace Image
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor;            // Kinect 本身
        private byte[] colorPixels;             // Kinect 抓到影像串流單張的影像
        private WriteableBitmap colorBitmap;    // 顯示的格式

        public MainWindow()
        {
            InitializeComponent();
        }

        void sensor_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            // 跑完程式, 釋放記憶體
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame != null)
                {

                    // Copy the pixel data from the image to a temporary arry
                    colorFrame.CopyPixelDataTo(this.colorPixels);

                    // Write the pixel data into our bitmap
                    // 把記憶體裡的Data, 扔給螢幕
                    this.colorBitmap.WritePixels(
                                                  new Int32Rect(
                                                                  0, 0,
                                                                  this.colorBitmap.PixelWidth,
                                                                  this.colorBitmap.PixelHeight
                                                                ),
                                                this.colorPixels,
                                                this.colorBitmap.PixelWidth * sizeof(int),
                                                0
                                                );
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Look through all sensors and start the first connected one.
            // This requires that a Kinect is connected at the time of app startup.
            // To make your app robust against plug/unplug, 
            // it is recommended to use KinectSensorChooser provided in Microsoft.Kinect.Toolkit (See components in Toolkit Browser).
            // 檢查PC 跟 Kinect 的連結
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            // 或者可以用下面的方式代替,
            // 只是上面方法, 有錯可以自己差旗標
            // sender = KinectSensor.KinectSensors[0];
            if (null != this.sensor)
            {
                // Turn on the color stream to receive color frames
                // 攝影機抓到的影像, 定影像規格
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);

                // Allocate space to put the pixels we'll receive
                // 給每張影像的記憶體
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                // Show 在螢幕上的顯示, 可以跟攝影機抓的不一樣
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth,
                                                        this.sensor.ColorStream.FrameHeight,
                                                        96.0, 96.0,
                                                        PixelFormats.Bgr32, null);

                // 把剛剛存在記憶體的影像, 扔到WPF 的Image 裡
                Image.Source = this.colorBitmap;

                this.sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorFrameReady);

                // Start the sensor!
                try
                {
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }



            }
            else
            {
                // Message.Content = "Kinect Don't Connect.";
                // 在方案管理的 Image - Resources.resx 裡面修改或新增
                Message.Content = Properties.Resources.NoKinectReady;
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
            }
        }

    }
}

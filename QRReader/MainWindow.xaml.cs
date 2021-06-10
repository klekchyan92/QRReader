using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using QRReader;
using ZXing;

namespace ScanQR
{
    public partial class MainWindow : Window
    {
        public FilterInfo CurrentDevice
        {
            get { return _currentDevice; }
            set { _currentDevice = value; }
        }

        private FilterInfo _currentDevice;

        private IVideoSource _videoSource;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            GetCurrentDevice();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopCamera();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartCamera();
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                BitmapImage bi;
                using (var bitmap = (Bitmap)eventArgs.Frame.Clone())
                {
                    bi = bitmap.ToBitmapImage();
                }
                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate { videoPlayer.Source = bi; }));
            }
            catch (Exception ex)
            {
                StopCamera();
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopCamera();
        }

        private void GetCurrentDevice()
        {
            CurrentDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice)[0];
        }

        private void StartCamera()
        {
            if (CurrentDevice != null)
            {
                _videoSource = new VideoCaptureDevice(CurrentDevice.MonikerString);
                _videoSource.NewFrame += video_NewFrame;
                _videoSource.Start();
            }
        }

        private void StopCamera()
        {
            if (_videoSource != null && _videoSource.IsRunning)
            {
                _videoSource.SignalToStop();
                _videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DispatcherTimer dispatcher = new DispatcherTimer();
            dispatcher.Interval = TimeSpan.FromSeconds(5);
            dispatcher.Tick += DispatcherTick;
            dispatcher.Start();
        }

        private void DispatcherTick(object sender, EventArgs e)
        {
            if (videoPlayer.Source != null)
            {
                BarcodeReader barcodeReader = new BarcodeReader();
                Result result = barcodeReader.Decode((BitmapSource)videoPlayer.Source);
                if (result != null)
                {
                    MessageBox.Show(result.ToString());
                    if (_videoSource.IsRunning)
                        StopCamera();
                }
            }
        }
    }
}

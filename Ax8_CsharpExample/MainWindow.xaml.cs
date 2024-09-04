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

using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Windows.Threading;

using System.Net;
using System.Threading;

namespace Ax8_CsharpExample
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private VideoCapture cam;
        private Mat frame;
        DispatcherTimer timer;
        bool is_initCam, is_initTimer;

        static string CamIP = "192.168.0.168";  // Change to your camera IP

        string rtspUrl = "rtsp://" + CamIP + "/avc";
        string fusionMode1 = "http://" + CamIP + "/prod/res/image/sysimg/fusion/fusionData/fusionMode?set=1";
        string fusionMode3 = "http://" + CamIP + "/prod/res/image/sysimg/fusion/fusionData/fusionMode?set=3";
        string useLevelSpan0 = "http://" + CamIP + "/prod/res/image/sysimg/fusion/fusionData/useLevelSpan?set=0";
        string useLevelSpan1 = "http://" + CamIP + "/prod/res/image/sysimg/fusion/fusionData/useLevelSpan?set=1";

        // MSX, IR, Visual 에 대한 참고 웹페이지
        //https://www.flir.com/support-center/instruments2/how-do-i-switch-between-visual-thermal-and-msx-image-modes-on-the-ax8-using-ethernetip/?srsltid=AfmBOoq-MMQLN_Rvre8f_VgmlHOIrdMCIclsRxVzgs0NIq5498Bbntey

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool init_camera()
        {
            try
            {
                // 연결할 카메라 선택 
                cam = new VideoCapture();
                cam.Open(rtspUrl);

                // 비디오 코덱을 16비트 그레이스케일 이미지로 지정 
                cam.Set(CaptureProperty.FourCC, VideoWriter.FourCC('Y', '1', '6', ' '));

                // RGB 변환 비활성화
                cam.Set(CaptureProperty.ConvertRgb, 0);

                frame = new Mat();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"카메라 초기화 오류: {ex.Message}");
                return false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 카메라, 타이머(0.01ms 간격) 초기화
            is_initCam = init_camera();
            is_initTimer = init_Timer(0.01);

            // 초기화 완료면 타이머 실행
            if (is_initTimer && is_initCam) timer.Start();
        }

        private bool init_Timer(double interval_ms)
        {
            try
            {
                timer = new DispatcherTimer();

                timer.Interval = TimeSpan.FromMilliseconds(interval_ms);
                timer.Tick += new EventHandler(timer_tick);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string save = DateTime.Now.ToString("yyyy-MM-dd-hh시mm분ss초");
            Cv2.ImWrite("./" + save + ".png", frame);

            MessageBox.Show("Image file saved.");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // MSX View
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadString(fusionMode3);
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse response)
                    {
                        Console.WriteLine($"Error: Status Code {(int)response.StatusCode} - {response.StatusCode}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // IR View
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadString(fusionMode1);

                    Thread.Sleep(500);

                    client.DownloadString(useLevelSpan1);
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse response)
                    {
                        Console.WriteLine($"Error: Status Code {(int)response.StatusCode} - {response.StatusCode}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            // Visual View
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadString(fusionMode1);

                    Thread.Sleep(500);

                    client.DownloadString(useLevelSpan0);
                }
                catch (WebException ex)
                {
                    if (ex.Response is HttpWebResponse response)
                    {
                        Console.WriteLine($"Error: Status Code {(int)response.StatusCode} - {response.StatusCode}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void timer_tick(object sender, EventArgs e)
        {
            // 0번 장비로 생성된 VideoCapture 객체에서 frame을 읽어옴
            cam.Read(frame);

            // 읽어온 Mat 데이터를 Bitmap 데이터로 변경 후 컨트롤에 그려줌
            Cam_1.Source = WriteableBitmapConverter.ToWriteableBitmap(frame);
        }
    }
}

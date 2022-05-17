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
using System.Windows.Shapes;

using Microsoft.Kinect;

namespace SWProject {
    /// <summary>
    /// Client.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Client : Window {

        //델리게이트 선언
        public delegate void ChildFormSnedDataHandler(string name, string age, string rrn);
        //이벤트 선언
        public event ChildFormSnedDataHandler ChildFormEvent;

        public Client() {
            InitializeComponent();

            InitializeNui();
            //this.btnPost.Click += btnPost_Click;
            pictogram.Source = new BitmapImage(new Uri(@"\Resources\man.png", UriKind.Relative));
        }

        private void Window_Closed(object sender, EventArgs e) {
            System.Environment.Exit(0);
        }

        KinectSensor nui = null;

        void InitializeNui() {

            nui = KinectSensor.KinectSensors[0];

            nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            nui.ColorStream.Enable();
            nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_ColorFrameReady);
            nui.SkeletonStream.Enable();

            nui.Start();
        }

        public void nui_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {
            ColorImageFrame ImageParam = e.OpenColorImageFrame();

            if(ImageParam == null) return;

            byte[] ImageBits = new byte[ImageParam.PixelDataLength];
            ImageParam.CopyPixelDataTo(ImageBits);

            BitmapSource src = null;
            src = BitmapSource.Create(ImageParam.Width,
                                        ImageParam.Height,
                                        96, 96,
                                        PixelFormats.Bgr32,
                                        null,
                                        ImageBits,
                                        ImageParam.Width * ImageParam.BytesPerPixel);
            ClientImage.Source = src;
        }

        private void btnPost_Click(object sender, RoutedEventArgs e) {

            string Name = txtClientName.Text;
            string Age = txtClientAge.Text;
            string RRN = txtClientRRN.Text + "-" + txtClientRRN2.Password;

            if(Name.Length == 0) {
                MessageBox.Show("이름을 입력하십시오.");
                return;
            }
            if(Age.Length == 0 ) {
                MessageBox.Show("나이를 입력하십시오.");
                return;
            }
            if(RRN.Length != 14) {
                MessageBox.Show("주민등록번호 제대로 입력하십시오.");
                return;
            }

            MessageBox.Show("전송 완료");
            //델리게이트 이벤트를 통해 부모폼으로 데이터 
            this.ChildFormEvent(Name, Age, RRN);
        }
    }
}

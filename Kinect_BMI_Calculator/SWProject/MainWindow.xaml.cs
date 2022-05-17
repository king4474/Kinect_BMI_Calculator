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

using Oracle.DataAccess.Client;
using Microsoft.Kinect;
using SWProject;

namespace SWProject {
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            LoginWindow();
            ClientWindow();

            InitializeNui();

            //키를 재기 위한 코드
            rectangle1.Width = 5;
            rectangle1.Opacity = 0.8;
            rectangle1.Visibility = Visibility.Collapsed;
        }

        private void Window_Closed(object sender, EventArgs e) {
            System.Environment.Exit(0);
        }

        double clientWeight = 0;
        double clientHeight = 0;
        double bmi = 0;
        void Bmi() {
            bmi = clientWeight / (clientHeight * clientHeight);
            Console.WriteLine(clientWeight);
            Console.WriteLine(clientHeight);
            Console.WriteLine(bmi);
            
            String strBMI = "";
            if(bmi < 18.5) {
                strBMI = "저체중";
            } else if(bmi < 22.9) {
                strBMI = "정상";
            } else if(bmi < 24.9) {
                strBMI = "과제충";
            } else if(bmi < 29.9) {
                strBMI = "비만";
            } else if(30 < bmi) {
                strBMI = "고도비만";
            }

            txtBMI.Text = string.Format("{0:0#} ({1})", bmi, strBMI);
        }

        static private void LoginWindow() {
            SWProject.Login LoginWindow = new SWProject.Login();
            LoginWindow.ShowDialog();
        }

        private void ClientWindow() {
            SWProject.Client ClientWindow = new SWProject.Client();
            ClientWindow.ChildFormEvent += EventMethod;
            ClientWindow.Show();
        }

        public void EventMethod(string name, string age, string rrn) {

            //자식폼에서 델리게이트로 이벤트 발생 시키면 현재 함수
            //EventMethod 호출

            txtRootName.Text = name;
            txtRootAge.Text = age;
            txtRootRRN.Text = rrn;

        }

        static public void LoginCheck(string loginCheck) {

            Console.WriteLine(loginCheck);
            if(!loginCheck.Equals("success")) {  // 프로그램 종료
                Environment.Exit(0);
            } 
        }

        KinectSensor nui = null;
        void InitializeNui() {
            
            nui = KinectSensor.KinectSensors[0];
            
            //nui.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            nui.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            nui.ColorStream.Enable();
            nui.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(nui_ColorFrameReady);
            nui.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(nui_DepthFrameReady);
            nui.SkeletonStream.Enable();
            nui.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(nui_AllFramesReady);
            nui.SkeletonStream.Enable();

            nui.Start();
        }

        Point pHead = new Point();
        Point pLeft = new Point();
        Point pRight = new Point();
        float fHeadZ = 0;
        float fHeadY = 0;
        float fLeftY = 0;
        float fRightY = 0;

        void CalcHeight() {
            try {
                rectangle1.Height = (pLeft.Y + pRight.Y) / 2 - pHead.Y;
                rectangle1.Visibility = Visibility.Visible;
                Canvas.SetLeft(rectangle1, pHead.X);
                Canvas.SetTop(rectangle1, pHead.Y);

                double dbVal = (fLeftY + fRightY) / 2 - fHeadY;

                double height = (dbVal * (fHeadZ * 100)) - fHeadZ * 2;
                txtRootHeight.Text = string.Format("{0:0} cm",height);
                clientHeight = Math.Round(height) / 100;
                Bmi();
            } catch {
                return;
            }
        }

        void SetRGB(byte[] nPlayers, int nPos, byte r, byte g, byte b) {
            nPlayers[nPos + 2] = r;
            nPlayers[nPos + 1] = g;
            nPlayers[nPos + 0] = b;
        }

        byte[] GetPlayer(DepthImageFrame PImage, short[] depthFrame, DepthImageStream depthStream) {
            byte[] playerCoded = new byte[PImage.Width * PImage.Height * 4];

            double lPixel = 0;
            double lDist = 0;
            long nPlayer = -1;

            for(int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < playerCoded.Length; i16++, i32 += 4) {
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                int nDistance = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                SetRGB(playerCoded, i32, 0x00, 0x00, 0x00);

                if(player > 0 && nPlayer <= 1) nPlayer = player;

                if(player == nPlayer) {
                    if(nDistance < depthStream.TooFarDepth && nDistance > depthStream.TooNearDepth) {
                        lDist += nDistance;
                        lPixel += 1;
                        SetRGB(playerCoded, i32, 0xFF, 0xFF, 0xFF);
                    }
                }
            }

            if(lPixel > 0) {
                txtPixel.Text = string.Format("{0}", lPixel);
                double dist = ((lDist / lPixel) / 100) / 10;
                txtDist.Text = string.Format("{0:#0.0#}m", dist);

                double weight = 0;
                try {
                    weight = (lPixel * lDist) / (80000000000 * (lPixel / 45500));
                    clientWeight = weight;
                } catch {
                    return playerCoded;
                }
                
                txtRootWeight.Text = string.Format("{0:0#}kg", weight);
            }
            return playerCoded;
        }

        void nui_ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {
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
            rootColorImage.Source = src;
        }

        void nui_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e) {
            DepthImageFrame ImageParam = e.OpenDepthImageFrame();

            if(ImageParam == null) return;

            short[] ImageBits = new short[ImageParam.PixelDataLength];
            ImageParam.CopyPixelDataTo(ImageBits);

            WriteableBitmap wb = new WriteableBitmap(ImageParam.Width,
                                                     ImageParam.Height,
                                                     96, 96,
                                                     PixelFormats.Bgr32,
                                                     null);
            wb.WritePixels(new Int32Rect(0, 0, ImageParam.Width, ImageParam.Height),
                           GetPlayer(ImageParam,
                                     ImageBits,
                                     ((KinectSensor)sender).DepthStream),
                           ImageParam.Width * 4,
                           0);

            rootImage.Source = wb;
        }

        void nui_AllFramesReady(object sender, AllFramesReadyEventArgs e) {
            SkeletonFrame sf = e.OpenSkeletonFrame();
            if(sf == null) return;
            Skeleton[] skeletonData = new Skeleton[sf.SkeletonArrayLength];
            sf.CopySkeletonDataTo(skeletonData);
            using(DepthImageFrame depthImageFrame = e.OpenDepthImageFrame()) {
                if(depthImageFrame != null) {
                    foreach(Skeleton sd in skeletonData) {
                        if(sd.TrackingState == SkeletonTrackingState.Tracked) {
                            foreach(Joint joint in sd.Joints) {
                                DepthImagePoint depthPoint;
                                depthPoint = depthImageFrame.MapFromSkeletonPoint(joint.Position);
                                switch(joint.JointType) {
                                    case JointType.Head:
                                        pHead.X = (int)(rootColorImage.Width * depthPoint.X / depthImageFrame.Width);
                                        pHead.Y = (int)(rootColorImage.Height * depthPoint.Y / depthImageFrame.Height);
                                        fHeadY = (float)depthPoint.Y / depthImageFrame.Height;
                                        fHeadZ = (float)joint.Position.Z;
                                        break;
                                    case JointType.FootLeft:
                                        pLeft.X = (int)(rootColorImage.Width * depthPoint.X / depthImageFrame.Width);
                                        pLeft.Y = (int)(rootColorImage.Height * depthPoint.Y / depthImageFrame.Height);
                                        fLeftY = (float)depthPoint.Y / depthImageFrame.Height;
                                        break;
                                    case JointType.FootRight:
                                        pRight.X = (int)(rootColorImage.Width * depthPoint.X / depthImageFrame.Width);
                                        pRight.Y = (int)(rootColorImage.Height * depthPoint.Y / depthImageFrame.Height);
                                        fRightY = (float)depthPoint.Y / depthImageFrame.Height;
                                        break;
                                }
                            }
                            CalcHeight();
                        }
                    }
                }
            }
        }

        private void btnInsert_Click(object sender, RoutedEventArgs e) {

            // ----------------------------------------
            try {
                double dist = double.Parse(txtDist.Text);
                if(dist < 1.8 || dist > 2.2) {
                    MessageBox.Show("1.9 ~ 2.1m 거리에 위치해 주십시오.");
                    return;
                }
            } catch {
                MessageBox.Show("1.9 ~ 2.1m 거리");
                return;
            }
            // ----------------------------------------

            string RRN = txtRootRRN.Text;
            string NAME = txtRootName.Text;
            string AGE = txtRootAge.Text;
            string HEIGHT = txtRootHeight.Text;
            string WEIGHT = txtRootWeight.Text;
            string BMI = txtBMI.Text;
            string CREATED = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            /*int RRN = int.Parse(txtRootRRN.Text);
            string NAME = txtRootName.Text;
            int AGE = int.Parse(txtRootAge.Text);
            int HEIGHT = int.Parse(txtRootHeight.Text);
            int WEIGHT = int.Parse(txtRootWeight.Text);
            double BMI = double.Parse(txtBMI.Text);
            string CREATED = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");*/

            try {
                lock(DBConn.getDBConn) {
                    if(!DBConn.IsDBConnected) {
                        MessageBox.Show("DB 연결 실패");
                        return;
                    } else {
                        try {
                            string sql = @"INSERT INTO CLIENT VALUES('#RRN','#NAME', '#AGE', '#HEIGHT', '#WEIGHT', '#BMI', '#CREATED')";

                            sql = sql.Replace("#RRN", RRN);
                            sql = sql.Replace("#NAME", NAME);
                            sql = sql.Replace("#AGE", AGE);
                            sql = sql.Replace("#HEIGHT", HEIGHT);
                            sql = sql.Replace("#WEIGHT", WEIGHT);
                            sql = sql.Replace("#BMI", BMI);
                            sql = sql.Replace("#CREATED", CREATED);

                            MessageBox.Show(sql);
                            /*RRN NUMBER(13,0)
                            NAME VARCHAR2(20 BYTE)
                            AGE NUMBER
                            HEIGHT NUMBER
                            WEIGHT NUMBER
                            BMI NUMBER
                            CREATED VARCHAR2(20 BYTE)*/
                            OracleConnection conn = DBConn.getDBConn;
                            OracleCommand cmd = new OracleCommand();
                            cmd.Connection = conn;
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("CLIENT 데이터 저장 성공", "성공");
                        } catch(Exception ex) {
                            MessageBox.Show(ex.Message, "CLIENT 데이터 저장 실패");
                        } finally {
                            DBConn.DBClose();
                        }
                    }
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.Message, "CLIENT 데이터 저장 실패");
            } finally {
                DBConn.DBClose();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e) {
            SWProject.ShowList showListWindow = new SWProject.ShowList();
            showListWindow.ShowDialog();
        }
    }
}

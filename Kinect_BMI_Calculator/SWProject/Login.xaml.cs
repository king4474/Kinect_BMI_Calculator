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

using SWProject;
using Oracle.DataAccess.Client;

namespace SWProject {
    /// <summary>
    /// Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login : Window {
        public Login() {
            InitializeComponent();
        }

        string result = "exit"; // exit : 프로그램 종료, retry : 로그인 재시도, success : 로그인 성공

        private void btnLogin_Click(object sender, RoutedEventArgs e) {
            string ID = txtID.Text;
            string PASSWORD = txtPWD.Password;
            if(ID.Equals("") || PASSWORD.Equals("")) {
                MessageBox.Show("아이디 비밀번호를 입력해 주십시오.", "경고");
                return;
            }
            
            try {
                lock(DBConn.getDBConn) {
                    if(!DBConn.IsDBConnected) {
                        MessageBox.Show("DB 연결 실패");
                        return;
                    } else {
                        try {
                            string sql = "SELECT * FROM ROOTUSERS WHERE ID='#ID' AND PASSWORD='#PASSWORD'";

                            sql = sql.Replace("#ID", ID);
                            sql = sql.Replace("#PASSWORD", PASSWORD);

                            OracleConnection conn = DBConn.getDBConn;
                            OracleCommand cmd = new OracleCommand();
                            cmd.Connection = conn;
                            cmd.CommandText = sql;
                            // cmd.ExecuteNonQuery();
                            OracleDataReader rdr = cmd.ExecuteReader();
                            if(rdr.Read()) {
                                MessageBox.Show("로그인 성공", "성공");
                                result = "success";
                            } else {
                                MessageBox.Show("아이디/비밀번호를 확인해 주십시오.", "경고");
                                txtID.Text = "";
                                txtPWD.Password = "";
                            }
                        } catch(Exception ex) {
                            MessageBox.Show(ex.Message, "실패");
                        } finally {
                            DBConn.DBClose();
                        }
                    }
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.Message, "회원가입 실패");
            } finally {
                DBConn.DBClose();
            }
            if (result.Equals("success")) {
                this.Close();
                MainWindow.LoginCheck(result);
            }
        }

        private void btnJoinAction_Click(object sender, RoutedEventArgs e) {
            SWProject.Join joinWindow = new SWProject.Join();
            joinWindow.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e) {
            if(result.Equals("success")) {
                MainWindow.LoginCheck(result);
            } else {
                Environment.Exit(0);
            }
        }
    }
}

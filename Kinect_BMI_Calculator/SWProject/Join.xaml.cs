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

using Oracle.DataAccess.Client;

namespace SWProject {
    /// <summary>
    /// Join.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Join : Window {
        public Join() {
            InitializeComponent();
        }

        Boolean idCheck = false;

        private void btnIDCheck_Click(object sender, RoutedEventArgs e) {
            string userID = txtID.Text;
            if(userID.Equals("")) {
                MessageBox.Show("아이디를 입력해 주십시오.", "경고");
            } else {
                try {
                    lock(DBConn.getDBConn) {
                        if(!DBConn.IsDBConnected) {
                            MessageBox.Show("DB 연결 실패");
                            return;
                        } else {
                            string sql = "SELECT ID FROM ROOTUSERS WHERE ID='#ID'";

                            sql = sql.Replace("#ID", userID);

                            OracleConnection conn = DBConn.getDBConn;
                            OracleCommand cmd = new OracleCommand();
                            cmd.Connection = conn;
                            cmd.CommandText = sql;
                            //cmd.ExecuteNonQuery();
                            OracleDataReader rdr = cmd.ExecuteReader();
                            if(rdr.Read()) {
                                MessageBox.Show("사용 불가능한 아이디입니다.");
                                idCheck = false;
                                return;
                            } else {
                                MessageBox.Show("사용 가능한 아이디입니다.");
                                idCheck = true;
                                return;
                            }

                        }
                    }
                } catch(Exception ex) {
                    MessageBox.Show(ex.Message, "중복체크 실패");
                } finally {
                    DBConn.DBClose();
                }
            }
        }

        private void btnJoin_Click(object sender, RoutedEventArgs e) {
            string userName = txtName.Text;
            string userID = txtID.Text;
            string userPWD = txtPWD.Password;
            string userPWDCheck = txtPWDCheck.Password;
            string userCreated = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            if(userName.Equals("") ||
                userID.Equals("") ||
                userPWD.Equals("") ||
                userPWDCheck.Equals("")) {
                MessageBox.Show("모두 입력해 주십시오.", "경고");
                return;
            }

            if(idCheck == false) {
                MessageBox.Show("아이디 중복체크 먼저 해 주십시오.", "경고");
                return;
            }

            if(userPWD.Equals(userPWDCheck) == false) {
                MessageBox.Show("비밀번호 불일치", "경고");
                return;
            }
            Boolean joinCheck = false;
            try {
                lock(DBConn.getDBConn) {
                    if(!DBConn.IsDBConnected) {
                        MessageBox.Show("DB 연결 실패");
                        return;
                    } else {
                        try {
                            string sql = @"INSERT INTO ROOTUSERS VALUES('#ID','#PASSWORD', '#Name', '#Created')";

                            sql = sql.Replace("#ID", userID);
                            sql = sql.Replace("#PASSWORD", userPWD);
                            sql = sql.Replace("#Name", userName);
                            sql = sql.Replace("#Created", userCreated);
                            MessageBox.Show(sql);

                            OracleConnection conn = DBConn.getDBConn;
                            OracleCommand cmd = new OracleCommand();
                            cmd.Connection = conn;
                            cmd.CommandText = sql;
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("회원가입 성공", "성공");
                            joinCheck = true;
                        } catch(Exception ex) {
                            MessageBox.Show(ex.Message, "JOIN 실패");
                        } finally {
                            DBConn.DBClose();
                            if(joinCheck == true) {
                                Window.GetWindow(this).Close();
                            }
                        }

                    }
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.Message, "회원가입 실패");
            } finally {
                DBConn.DBClose();
            }
        }
    }
}

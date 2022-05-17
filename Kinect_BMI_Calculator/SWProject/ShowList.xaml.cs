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
    /// ShowList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ShowList : Window {
        public ShowList() {
            InitializeComponent();
        }

        private void btnShowListSearch_Click(object sender, RoutedEventArgs e) {
            string sql = "SELECT * FROM CLIENT WHERE NAME='#NAME'";

            try {
                lock(DBConn.getDBConn) {
                    if(!DBConn.IsDBConnected) {
                        MessageBox.Show("DB 연결 실패");
                        return;
                    } else {
                        sql = sql.Replace("#NAME", txtShowListSearch.Text);
                        OracleConnection conn = DBConn.getDBConn;
                        OracleCommand cmd = new OracleCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        OracleDataReader rdr = cmd.ExecuteReader();

                        string RRN = "";
                        string name = "";
                        string age = "";
                        string height = "";
                        string weight = "";
                        string BMI = "";
                        string created = "";

                        if(rdr.Read()) {
                            RRN = rdr["RRN"].ToString();
                            name = rdr["NAME"].ToString();
                            age = rdr["AGE"].ToString();
                            height = rdr["HEIGHT"].ToString();
                            weight = rdr["WEIGHT"].ToString();
                            BMI = rdr["BMI"].ToString();
                            created = rdr["CREATED"].ToString();

                            txtShowListRRN.Text = RRN;
                            txtShowListName.Text = name;
                            txtShowListAge.Text = age;
                            txtShowListHeight.Text = height;
                            txtShowListWeight.Text = weight;
                            txtShowListBMI.Text = BMI;
                            txtShowListCreated.Text = created;
                        } else {
                            txtShowListSearch.Text = "검색 정보가 없습니다.";
                            txtShowListRRN.Text = "";
                            txtShowListName.Text = "";
                            txtShowListAge.Text = "";
                            txtShowListHeight.Text = "";
                            txtShowListWeight.Text = "";
                            txtShowListBMI.Text = "";
                            txtShowListCreated.Text = "";
                        }
                    }
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.Message, "검색 실패");
            } finally {
                DBConn.DBClose();
            }
        }

        private void btnShowListDelete_Click(object sender, RoutedEventArgs e) {
            try {
                lock(DBConn.getDBConn) {
                    if(!DBConn.IsDBConnected) {
                        MessageBox.Show("DB 연결 실패");
                        return;
                    } else {
                        string sql = "DELETE FROM CLIENT WHERE NAME='#NAME'";
                        sql = sql.Replace("#NAME", txtShowListSearch.Text);
                        OracleConnection conn = DBConn.getDBConn;
                        OracleCommand cmd = new OracleCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = sql;
                        cmd.ExecuteNonQuery();

                        txtShowListRRN.Text = "";
                        txtShowListName.Text = "";
                        txtShowListAge.Text = "";
                        txtShowListHeight.Text = "";
                        txtShowListWeight.Text = "";
                        txtShowListBMI.Text = "";
                        txtShowListCreated.Text = "";
                        MessageBox.Show("삭제 완료");
                    }
                }
            } catch(Exception ex) {
                MessageBox.Show(ex.Message, "삭제 실패");
            } finally {
                DBConn.DBClose();
            }
        }
    }
}

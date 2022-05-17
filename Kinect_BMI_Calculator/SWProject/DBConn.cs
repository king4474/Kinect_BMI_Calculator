using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Oracle.DataAccess.Client;
using System.Windows;

namespace SWProject {
    class DBConn {
        private static OracleConnection conn = null;
        public static string DBConnString {
            get;
            private set;
        }

        public static bool check = false;
        private static int errorCount = 0;

        //생성자
        public DBConn() { }

        public static OracleConnection getDBConn {
            get {
                if(!ConnectToDB()) {
                    return null;
                }
                return conn;
            }
        }

        // DB Connect
        public static bool ConnectToDB() {
            if(conn == null) {
                DBConnString = String.Format("Data Source=(DESCRIPTION="
                        + "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=LOCALHOST)(PORT=1521)))"
                        + "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XE)));"
                        + "User Id=swproject;Password=rootpw");

                conn = new OracleConnection(DBConnString);
                // MessageBox.Show("DB 연결 성공");
                // PROTOCOL=TCP -> 사용 프로토콜로, 오라클은 TCP를 사용.
                // HOST=LOCALHOST -> 접속할 DB의 IP
                // PORT=1521 -> TCP 를 사용하는 오라클 포트(1521)
                // SERVER=DEICATED -> 웹 서버, 소프트웨어 등을 임대하여 전용으로 사용하는 서버
                // SERVICE_NAME=ORCL -> 여러 개의 인스턴스를 모아 하나의 서버를 구성한 것.
            }

            try {
                if(!IsDBConnected) {
                    conn.Open();
                    if(conn.State == System.Data.ConnectionState.Open) {
                        check = true;
                    } else {
                        check = false;
                    }
                }
            } catch(Exception e) {
                errorCount++;
                if(errorCount == 1) {
                    MessageBox.Show(e.Message, "DBConn - ConnectToDB()");
                }
                return false;
            }
            return true;
        }

        // Database Open 여부 확인
        public static bool IsDBConnected {
            get {
                if(conn.State != System.Data.ConnectionState.Open) {
                    return false;
                }
                return true;
            }
        }

        // Database 연결 해제
        public static void DBClose() {
            if(IsDBConnected)
                getDBConn.Close();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class DBConnect
    {
        static string server = null;
        static string database = null;
        static string uid = null;
        static string password = null;
        public const int PRI_KEY_EXISTS = -1062;
        private const string DB_CONF_FILE = "Database.txt";
        static List<MySqlConnection> OpenedConnections = new List<MySqlConnection>();

        public DBConnect() { }

        //Initialize values
        public static MySqlConnection OpenNewConnection()
        {
            if (OpenedConnections.Count > 0)
                System.Windows.MessageBox.Show("Already have " +
                    OpenedConnections.Count + " opened connections");
            if (server == null)
            {
                string[] conf = null;
                if (System.IO.File.Exists(DB_CONF_FILE))
                    conf = System.IO.File.ReadAllLines(DB_CONF_FILE);
                if (conf != null && conf.Length == 4)
                {
                    server = conf[0];
                    database = conf[1];
                    uid = conf[2];
                    password = conf[3];
                }
                if (server == null)
                {
                    server = "localhost";
                    database = "sQzEN";
                    uid = "root";
                    password = "1234";
                }
            }
            string connStr = "SERVER=" + server + ";" + "DATABASE=" +
                database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";charset=utf8";
            MySqlConnection conn = new MySqlConnection(connStr);
            if (Open(ref conn))
            {
                OpenedConnections.Add(conn);
                return conn;
            }
            else
                return null;
        }

        //open connection to database
        public static bool Open(ref MySqlConnection conn)
        {
            try
            {
                conn.Open();
                return true;
            }
            catch (MySqlException e)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (e.Number)
                {
                    case 0:
                        System.Windows.MessageBox.Show("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        System.Windows.MessageBox.Show("Invalid username/password, please try again");
                        break;
                    default:
                        System.Windows.MessageBox.Show(e.ToString());
                        break;
                }
                return false;
            }
        }

        public static void Close(ref MySqlConnection conn)
        {
            if (conn == null)
                return;
            try
            {
                conn.Close();
                if(OpenedConnections.Count > 0 && OpenedConnections.Contains(conn))
                    OpenedConnections.Remove(conn);
            }
            catch (MySqlException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
            }
        }

        //Insert statement
        public static int Ins(MySqlConnection conn, string tb,
            string attbs, string vals, out string eMsg)
        {
            if (attbs == null || vals == null)
            {
                eMsg = Txt.s._((int)TxI.DB_DAT_NOK);
                return 0;
            }
            StringBuilder qry = new StringBuilder();
            qry.Append("INSERT INTO " + tb + "(" + attbs + ")VALUES");
            qry.Append(vals);
            
            MySqlCommand cmd = new MySqlCommand(qry.ToString(), conn);
            int n;
            try
            {
                n = cmd.ExecuteNonQuery();
                eMsg = null;
            } catch(MySqlException e) {
                if (e.Number == -PRI_KEY_EXISTS)
                {
                    eMsg = e.ToString();
                    n = PRI_KEY_EXISTS;
                }
                else
                {
                    eMsg = Txt.s._((int)TxI.DB_EXCPT) + e.ToString();
                    n = -1;
                }
            }
            return n;
        }

        public static int Update(MySqlConnection conn, string tb, string vals, string cond,
            out string eMsg)
        {
            StringBuilder qry = new StringBuilder();
            qry.Append("UPDATE " + tb + " SET " + vals);
            if (cond != null)
                qry.Append(" WHERE " + cond);
            MySqlCommand cmd = new MySqlCommand(qry.ToString(), conn);
            int n;
            try
            {
                n = cmd.ExecuteNonQuery();
                eMsg = null;
            }
            catch (MySqlException e)
            {
                n = -1;
                eMsg = e.ToString();
            }
            return n;
        }

        //Delete statement
        public static int Delete(MySqlConnection conn, string tb, string cond, out string eMsg)
        {
            StringBuilder qry = new StringBuilder();
            qry.Append("DELETE FROM " + tb);
            if(cond != null)
                qry.Append(" WHERE " + cond);
            MySqlCommand cmd = new MySqlCommand(qry.ToString(), conn);
            int n;
            try {
                n = cmd.ExecuteNonQuery();
                eMsg = null;
            }
            catch (MySqlException e) {
                n = -1;
                eMsg = e.ToString();
            }
            return n;
        }

        public static int Count(MySqlConnection conn, string tb, string attbs,
            string cond, out string eMsg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT COUNT(");
            if (attbs == null)
                sb.Append("*) FROM " + tb);
            else
                sb.Append(attbs + ") FROM " + tb);
            if(cond != null)
                sb.Append(" WHERE " + cond);

            int n;
            MySqlCommand cmd = new MySqlCommand(sb.ToString(), conn);
            try {
                if (int.TryParse(cmd.ExecuteScalar().ToString(), out n))
                    eMsg = null;
                else
                {
                    n = -1;
                    eMsg = Txt.s._((int)TxI.DB_COUNT_NOK);
                }
            }
            catch (MySqlException e) {
                eMsg = e.ToString();
                n = -1;
            }

            return n;
        }

        public static bool NExist(MySqlConnection conn, string tb, string cond, out string eMsg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT COUNT(*) FROM(SELECT 1 FROM " + tb);
            if (cond != null)
                sb.Append(" WHERE " + cond);
            sb.Append(" LIMIT 1) as tb");
            int n;
            MySqlCommand cmd = new MySqlCommand(sb.ToString(), conn);
            try
            {
                if (int.TryParse(cmd.ExecuteScalar().ToString(), out n))
                    eMsg = null;
                else
                {
                    n = -1;
                    eMsg = Txt.s._((int)TxI.DB_COUNT_NOK);
                }
            }
            catch (MySqlException e)
            {
                eMsg = e.ToString();
                n = -1;
            }

            if (0 < n)
                return false;
            return true;
        }

        //Max statement
        public static int MaxInt(MySqlConnection conn, string tb, string attb, string cond)
        {
            string query = "SELECT MAX(" + attb + ") FROM " + tb;
            if(cond != null)
                query = query + " WHERE " + cond;
            int n;
            MySqlCommand cmd = new MySqlCommand(query, conn);
            try {
                object i = cmd.ExecuteScalar();
                if (i is DBNull)
                    n = 0;
                else if (!int.TryParse(i.ToString(), out n))
                    n = -1;
            }
            catch (MySqlException) { n = -1; }

            return n;
        }

        public static string SafeSQL_Text(string unsafeText)
        {
            return unsafeText.Replace("\\", "\\\\").Replace("'", "\\'");
        }

        public static string mkQrySelect(string tb, string attbs, string cond)
        {
            string query = "SELECT ";
            if (attbs == null)
                query += "*";
            else
                query += attbs;
            query += " FROM " + tb;
            if (cond != null && cond.Length > 0)
                query += " WHERE " + cond;

            return query;
        }

        public static MySqlDataReader exeQrySelect(MySqlConnection conn, string query, out string eMsg) {
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader d = null;
            try {
                d = cmd.ExecuteReader();
                eMsg = null;
            }
            catch(MySqlException e) {
                d = null;
                eMsg = Txt.s._((int)TxI.DB_EXCPT) + e.ToString();
            }
            return d;
        }
    }
}

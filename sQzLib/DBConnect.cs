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
        //public MySqlConnection connection;
        static string server = null;
        static string database = null;
        static string uid = null;
        static string password = null;
        //static bool bConnected;

        //Constructor
        public DBConnect() { }

        //Initialize values
        public static MySqlConnection Init()
        {
            if (server == null)
            {
                string s = Utils.ReadFile("Database.txt");
                if (s != null)
                {
                    string[] vs = s.Split('\n');
                    if(vs.Length == 4)
                    {
                        server = vs[0];
                        database = vs[1];
                        uid = vs[2];
                        password = vs[3];
                    }
                }
                if (server == null)
                {
                    server = "localhost";
                    database = "sQz";
                    uid = "root";
                    password = "1234";
                }
            }
            string connStr = "SERVER=" + server + ";" + "DATABASE=" +
                database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";charset=utf8";
            //bConnected = false;
            MySqlConnection conn = new MySqlConnection(connStr);
            if (Open(ref conn))
                return conn;
            else
                return null;
        }

        //open connection to database
        public static bool Open(ref MySqlConnection conn)
        {
            //if (bConnected)
            //    return true;
            try
            {
                conn.Open();
                //bConnected = true;
                return true;
            }
            catch (MySqlException)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                //switch (ex.Number)
                //{
                //    case 0:
                //        Console.Write("Cannot connect to server.  Contact administrator");
                //        break;

                //    case 1045:
                //        Console.Write("Invalid username/password, please try again");
                //        break;
                //}
                return false;
            }
        }

        //Close connection
        public static bool Close(ref MySqlConnection conn)
        {
            try
            {
                //if (bConnected)
                {
                    conn.Close();
                    //bConnected = false;
                }
                return true;
            }
            catch (MySqlException ex)
            {
                Console.Write(ex.Message);
                return false;
            }
        }

        //Insert statement
        public static int Ins(MySqlConnection conn, string tb,
            string attbs, string vals, out string eMsg)
        {
            if (attbs == null || vals == null)
            {
                eMsg = Txt.s._[(int)TxI.DB_DAT_NOK];
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
                if (e.Number == 1062)
                {
                    eMsg = null;
                    n = -1062;
                }
                else
                {
                    eMsg = Txt.s._[(int)TxI.DB_EXCPT] + e.ToString();
                    n = -1;
                }
            }
            return n;
        }

        //Update statement
        public static void Update(MySqlConnection conn, string qry)
        {
			MySqlCommand cmd = new MySqlCommand(qry, conn);

            //Execute command
            cmd.ExecuteNonQuery();
        }

        //Delete statement
        public static int Delete(MySqlConnection conn, string tb, string cond)
        {
            string query = "DELETE FROM " + tb;
            if(cond != null)
                query += " WHERE " + cond;
            MySqlCommand cmd = new MySqlCommand(query, conn);
            int n;
            try { n = cmd.ExecuteNonQuery(); }
            catch (MySqlException) { n = -1; }
            return n;
        }

        //Count statement
        public static int Count(MySqlConnection conn, string tb)
        {
            string query = "SELECT COUNT(*) FROM " + tb;
            int n;

            MySqlCommand cmd = new MySqlCommand(query, conn);
            try { if (!int.TryParse(cmd.ExecuteScalar().ToString(), out n)) n = 0; }
            catch (MySqlException) { n = 0; }

            return n;
        }

        public static int Count(MySqlConnection conn, string tb, string attbs, string cond)
        {
            string query = "SELECT COUNT(" + attbs + ") FROM " + tb + " WHERE " + cond;
            int n;

            MySqlCommand cmd = new MySqlCommand(query, conn);
            try { if (!int.TryParse(cmd.ExecuteScalar().ToString(), out n)) n = 0; }
            catch (MySqlException) { n = -1; }

            return n;
        }

        //Max statement
        public static int MaxUShort(MySqlConnection conn, string tb, string attb, string cond)
        {
            string query = "SELECT MAX(" + attb + ") FROM " + tb + " WHERE " + cond;
            int uShort;

            MySqlCommand cmd = new MySqlCommand(query, conn);
            try {
                var o = cmd.ExecuteScalar();
                if (o == null)
                    uShort = 0;
                else if (o.ToString().Length == 0)
                    uShort = 0;
                else if (!int.TryParse(o.ToString(), out uShort))
                    uShort = -1;
            }
            catch (MySqlException) { uShort = -1; }

            return uShort;
        }

        //Min statement
        //public static int Min(MySqlConnection conn, string tb, string attb, string cond)
        //{
        //    string query = "SELECT MIN(" + attb + ") FROM " + tb + " WHERE " + cond;
        //    int n;

        //    MySqlCommand cmd = new MySqlCommand(query, conn);
        //    try { if (!int.TryParse(cmd.ExecuteScalar().ToString(), out n)) n = 0; }
        //    catch (MySqlException) { n = 0; }

        //    return n;
        //}

        public static string mkQrySelect(string tb, string attbs, string cond,
            string gpAttbs)
        {
            string query = "SELECT ";
            if (attbs == null)
                query += "*";
            else
                query += attbs;
            query += " FROM " + tb;
            if (cond != null)
                query += " WHERE " + cond;
            if (gpAttbs != null)
                query += " GROUP BY " + gpAttbs;

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
                eMsg = Txt.s._[(int)TxI.DB_EXCPT] + e.ToString();
            }
            return d;
        }
    }
}

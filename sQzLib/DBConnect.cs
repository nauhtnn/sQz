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

        public static MySqlConnection Conn
        {
            get
            {
                if (server == null)
                {
                    if (System.IO.File.Exists("Database.txt"))
                    {
                        string[] s = System.IO.File.ReadAllLines("Database.txt");
                        if (s != null && s.Length == 4)
                        {
                            server = s[0];
                            database = s[1];
                            uid = s[2];
                            password = s[3];
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
                MySqlConnection conn = new MySqlConnection(connStr);
                conn.Open();
                return conn;
            }
        }

        //Insert statement
        public static int Ins(string tb, string attbs, string vals)
        {
            StringBuilder qry = new StringBuilder();
            qry.Append("INSERT INTO " + tb + "(" + attbs + ")VALUES");
            qry.Append(vals);
            
            MySqlCommand cmd = new MySqlCommand(qry.ToString(), Conn);
            return cmd.ExecuteNonQuery();
        }

        public static int Update(string tb, string vals, string cond)
        {
            StringBuilder qry = new StringBuilder();
            qry.Append("UPDATE " + tb + " SET " + vals);
            if (cond != null)
                qry.Append(" WHERE " + cond);
            MySqlCommand cmd = new MySqlCommand(qry.ToString(), Conn);
            return cmd.ExecuteNonQuery();
        }

        //Delete statement
        public static int Delete(string tb, string cond)
        {
            StringBuilder qry = new StringBuilder();
            qry.Append("DELETE FROM " + tb);
            if(cond != null)
                qry.Append(" WHERE " + cond);
            MySqlCommand cmd = new MySqlCommand(qry.ToString(), Conn);
            return cmd.ExecuteNonQuery();
        }

        public static int Count(string tb, string attbs, string cond)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT COUNT(");
            if (attbs == null)
                sb.Append("*) FROM " + tb);
            else
                sb.Append(attbs + ") FROM " + tb);
            if(cond != null)
                sb.Append(" WHERE " + cond);

            MySqlCommand cmd = new MySqlCommand(sb.ToString(), Conn);
            return int.Parse(cmd.ExecuteScalar().ToString());
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
                    eMsg = Txt.s._[(int)TxI.DB_COUNT_NOK];
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
            string query = "SELECT MAX(" + attb + ") FROM " + tb + " WHERE " + cond;
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

        public static MySqlDataReader exeQrySelect(string tb, string attbs, string cond)
        {
            string query = "SELECT ";
            if (attbs == null)
                query += "*";
            else
                query += attbs;
            query += " FROM " + tb;
            if (cond != null)
                query += " WHERE " + cond;

            MySqlCommand cmd = new MySqlCommand(query, Conn);
            return cmd.ExecuteReader();
        }
    }
}

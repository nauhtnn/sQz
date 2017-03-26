using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    class DBConnect
    {
        //public MySqlConnection connection;
        static string server;
        static string database;
        static string uid;
        static string password;
        //static bool bConnected;

        //Constructor
        public DBConnect()
        {
            //Initialize();
        }

        //Initialize values
        public static MySqlConnection Init()
        {
            string connStr = null;
            string s = Utils.ReadFile("Database.txt");
            if (s == null)
            {
                server = "localhost";
                database = "sQz";
                uid = "root";
                password = "1234";
            }
            else
            {
                string[] vs = s.Split('\n');
                server = vs[0];
                database = vs[0];
                uid = vs[0];
                password = vs[0];
            }
            connStr = "SERVER=" + server + ";" + "DATABASE=" +
                database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password;//todo + ";charset=utf32";
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
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.Write("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.Write("Invalid username/password, please try again");
                        break;
                }
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
        public static void Ins(MySqlConnection conn, string tb, string[] vAttb, string[] vVal)
        {
            if (vAttb == null || vVal == null)
                return;
            if (vAttb.Length != vVal.Length)
                return;
            int lastIdx = vAttb.Length - 1;
            //string query = "INSERT INTO tableinfo (name, age) VALUES('John Smith', '33')";
            string query = "INSERT INTO " + tb + "(";
            for (int i = 0; i < lastIdx; ++i)
                query += vAttb[i] + ",";
            query += vAttb[lastIdx] + ")VALUES(";
            for (int i = 0; i < lastIdx; ++i)
                query += vVal[i] + ",";
            query += vVal[lastIdx] + ")";
            
            //create command and assign the query and connection from the constructor
            MySqlCommand cmd = new MySqlCommand(query, conn);

            //Execute command
            cmd.ExecuteNonQuery();
        }

        public static void Ins(MySqlConnection conn, string tb, string attb, string val)
        {
            if (attb == null || val == null)
                return;
            string query = "INSERT INTO " + tb + "(" + attb + ")VALUES(" + val + ")";

            //open connection
            //if (this.OpenConnection() == true)
            //{
            //create command and assign the query and connection from the constructor
            MySqlCommand cmd = new MySqlCommand(query, conn);

            //Execute command
            cmd.ExecuteNonQuery();
            //}
        }

        //Update statement
        public void Update()
        {
            throw new NotImplementedException();
            //string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

            //Open connection
            //if (this.OpenConnection() == true)
            //{
            //    //create mysql command
            //    MySqlCommand cmd = new MySqlCommand();
            //    //Assign the query using CommandText
            //    cmd.CommandText = query;
            //    //Assign the connection using Connection
            //    cmd.Connection = connection;

            //    //Execute query
            //    cmd.ExecuteNonQuery();
            //}
        }

        //Delete statement
        public void Delete()
        {
            throw new NotImplementedException();
            //string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            //if (this.OpenConnection() == true)
            //{
            //    MySqlCommand cmd = new MySqlCommand(query, connection);
            //    cmd.ExecuteNonQuery();
            //}
        }



        //Count statement
        public static int Count(MySqlConnection conn, string tb)
        {
            string query = "SELECT COUNT(*) FROM " + tb;
            int n;

            MySqlCommand cmd = new MySqlCommand(query, conn);
            //Create a data reader and Execute the command
            try { if (!int.TryParse(cmd.ExecuteScalar() + "", out n)) n = 0; }
            catch (MySqlException) { n = 0; }

            return n;
        }

        public static string mkQrySelect(string tb, string[] vAttb, string cdAttb,
            string cdAttbVal, string[] vGpAttb)
        {
            string query = "SELECT";
            int lastIdx = 0;
            if (vAttb == null)
                query += " * ";
            else
            {
                lastIdx = vAttb.Length - 1;
                for (int i = 0; i < lastIdx; ++i)
                    query += vAttb[i] + ",";
                query += vAttb[lastIdx] + " ";
            }
            query += " FROM " + tb;
            if (cdAttb != null && cdAttbVal != null)
                query += " WHERE " + cdAttb + "=" + cdAttbVal;
            if (vGpAttb != null)
            {
                query += " GROUP BY ";
                lastIdx = vGpAttb.Length - 1;
                for (int i = 0; i < lastIdx; ++i)
                    query += vGpAttb[i] + ",";
                query += vGpAttb[lastIdx];
            }

            return query;
        }

        public static MySqlDataReader exeQrySelect(MySqlConnection conn, string query) {
            MySqlCommand cmd = new MySqlCommand(query, conn);
            //Create a data reader and Execute the command
            MySqlDataReader d = null;
            try { d = cmd.ExecuteReader(); }
            catch(MySqlException) { d = null; }
            return d;
        }
    }
}

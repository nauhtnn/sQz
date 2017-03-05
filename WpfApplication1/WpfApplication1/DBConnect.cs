using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace WpfApplication1
{
    class DBConnect
    {
        public MySqlConnection connection;
        public string server;
        public string database;
        public string uid;
        public string password;
        bool bConnected;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        public void Initialize()
        {
            string connStr = null;
            string s = sQzCS.Utils.ReadFile("Database.txt");
            if (s == null)
            {
                server = "localhost";
                database = "connectcsharptomysql";
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
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connStr);
            bConnected = false;
        }

        //open connection to database
        public bool OpenConnection()
        {
            if (bConnected)
                return true;
            try
            {
                connection.Open();
                bConnected = true;
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
        public bool CloseConnection()
        {
            try
            {
                if (bConnected)
                {
                    connection.Close();
                    bConnected = false;
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
        public void Insert(string tb, string[] vAttb, string[] vValue)
        {
            if (vAttb == null || vValue == null)
                return;
            if (vAttb.Length != vValue.Length)
                return;
            int lastIdx = vAttb.Length - 1;
            //string query = "INSERT INTO tableinfo (name, age) VALUES('John Smith', '33')";
            string query = "INSERT INTO " + tb + "(";
            for (int i = 0; i < lastIdx; ++i)
                query += vAttb[i] + ",";
            query += vAttb[lastIdx] + ")VALUES(";
            for (int i = 0; i < vValue.Length; ++i)
                query += "'" + vValue[i] + "',";
            query += "'" + vValue[lastIdx] + "')";

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();
            }
        }

        //Update statement
        public void Update()
        {
            throw new NotImplementedException();
            string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();
            }
        }

        //Delete statement
        public void Delete()
        {
            throw new NotImplementedException();
            string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
        }

        //Select statement
        public List<byte[]>[] Select(string tb, string[] vAttb, string[] vCdAttb,
            string[] vCdAttbVal, string[] vGpAttb)
        {
            if (vCdAttb != null && vCdAttbVal != null &&
                vCdAttb.Length != vCdAttbVal.Length)
                return null;
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
            if(vCdAttb != null && vCdAttbVal != null)
            {
                query += " WHERE ";
                lastIdx = vCdAttb.Length - 1;
                for (int i = 0; i < lastIdx; ++i)
                    query += vCdAttb[i] + "=" + vCdAttbVal[i] + ",";
                query += vCdAttb + "=" + vCdAttbVal;
            }
            if(vGpAttb != null)
            {
                query += " GROUP BY ";
                lastIdx = vGpAttb.Length - 1;
                for (int i = 0; i < lastIdx; ++i)
                    query += vGpAttb[i] + ",";
                query += vGpAttb[lastIdx];
            }

            //Create a list to store the result
            List<byte[]>[] vRs = new List<byte[]>[vAttb.Length];
            for (int i = 0; i < vAttb.Length; ++i)
                vRs[i] = new List<byte[]>();

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader datRdr = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (datRdr.Read())
                {
                    for (int i = 0; i < vAttb.Length; ++i)
                        vRs[i].Add((byte[])datRdr.GetValue(i));
                }

                //close Data Reader
                datRdr.Close();

                //return list to be displayed
                return vRs;
            }
            else
            {
                return null;
            }
        }

        //Count statement
        public int Count()
        {
            throw new NotImplementedException();
            string query = "SELECT Count(*) FROM tableinfo";
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                return Count;
            }
            else
            {
                return Count;
            }
        }
    }
}

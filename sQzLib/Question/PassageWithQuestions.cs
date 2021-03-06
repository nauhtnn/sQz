using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class PassageWithQuestions
    {
        public static int globalMaxID = -1;
        List<string> MagicKeywords;
        public string Requirements;
        public string Passage;
        protected int _ID;
        public int ID { get { return _ID; } }
        public List<Question> Questions = new List<Question>();

        public PassageWithQuestions()
        {
            _ID = -1;
        }

        public PassageWithQuestions(int id)
        {
            _ID = id;
        }

        public bool AccquireGlobalMaxID()
        {
            if (-1 < globalMaxID)
            {
                _ID = ++globalMaxID;
                foreach (Question q in Questions)
                    q.PassageID = _ID;
                return false;
            }
            return true;
        }

        public static bool GetMaxID_inDB()
        {
            MySqlConnection conn = DBConnect.Init();
            if (conn == null)
                return true;
            int uid = DBConnect.MaxInt(conn, "sqz_passage", "id", null);
            if (uid < 0)
            {
                DBConnect.Close(ref conn);
                return true;
            }
            globalMaxID = uid;

            return false;
        }
    }
}

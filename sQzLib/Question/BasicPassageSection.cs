using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace sQzLib
{
    public class BasicPassageSection: QSheetSection
    {
        public static int globalMaxID = -1;
        public string Passage;
        protected int _ID;
        public int ID { get { return _ID; } }

        public BasicPassageSection()
        {
            _ID = -1;
        }

        public BasicPassageSection(int id)
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

        public override bool Parse(Queue<BasicRich_PlainText> tokens)
        {
            if (tokens.Count < 4 + Question.NUMBER_OF_OPTIONS)
            {
                System.Windows.MessageBox.Show("BasicPassageSection: From the end, line " +
                    tokens.Count + " doesn't have 1 requirement 1 passage 1 stem 4 options 1 answer!");
                return false;
            }

            Requirements = tokens.Dequeue().ToString();

            Passage = tokens.Dequeue().ToString();

            return ParseQuestions(tokens);
        }

        public override void DBAppendQryIns(string prefx, ref int idx, StringBuilder vals)
        {
            foreach (Question q in Questions)
            {
                vals.Append(prefx +
                    ID + "," + q.uId + ",'");
                foreach (int i in q.vAnsSort)
                    vals.Append(i.ToString());
                vals.Append("'," + ++idx + "),");
            }
        }
    }
}

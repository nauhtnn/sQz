using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace sQzLib
{
    public class Settings
    {
        public static int MAX_N_QUESTS = 999;
        public static int DEFT_N_ANS = 4;
        public int nQuest;
        public int nAns;
        public bool bChoiceSort;
        public bool bQuestSort;
        public bool bDIV;

        public Settings() {
            nQuest = MAX_N_QUESTS;
            nAns = DEFT_N_ANS;
            bQuestSort = true;
            bChoiceSort = true;
            bDIV = true;
        }
    }
}

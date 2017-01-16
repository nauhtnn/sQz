using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace sQzCS
{
    class Settings
    {
        public static UInt16 MAX_N_QUESTS = 999;
        public static UInt16 DEFT_N_CHOICES = 4;
        public UInt16 nQuest;
        public UInt16 nChoices;
        public bool bChoiceSort;
        public bool bQuestSort;
        public bool bDIV;

        public Settings() {
            nQuest = MAX_N_QUESTS;
            nChoices = DEFT_N_CHOICES;
            bQuestSort = true;
            bChoiceSort = true;
            bDIV = true;
        }
    }
}

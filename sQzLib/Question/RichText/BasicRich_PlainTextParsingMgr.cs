using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace sQzLib
{
    class BasicRich_PlainTextParsingMgr
    {
        public BasicRich_PlainTextParsingMgr()
        {
            if (!QSheetSection.LoadSectionMagicKeywords())
                QSheetSection.InitDefaultMagicKeywords();
        }

        public List<QSheetSection> ParseTokens(Queue<BasicRich_PlainText> tokens)
        {
            List<QSheetSection> sections = new List<QSheetSection>();
            QSheetSection.TrimToFirstSection(tokens);
            while(tokens.Count > 0)
            {
                if(QSheetSection.SECTION_MAGIC_PREFIX.Length > 0 &&
                    !tokens.Dequeue().StartsWith(QSheetSection.SECTION_MAGIC_PREFIX))
                {
                    System.Windows.MessageBox.Show("BasicPassageSection: From the end, line " +
                    tokens.Count + " doesn't have section magic prefix " + QSheetSection.SECTION_MAGIC_PREFIX);
                    return sections;
                }
                QSheetSection section = SelectSection(tokens.Peek().GetInnerText());
                if(!section.Parse(tokens))
                {
                    sections.Add(section);
                    break;
                }
                sections.Add(section);
            }
            return sections;
        }

        private QSheetSection SelectSection(string text)
        {
            if(RegexIsMatch(text, QSheetSection.SectionMagicKeywords[SectionID.PassageWithBlanks]))
                return new BasicPassageSection();
            if (RegexIsMatch(text, QSheetSection.SectionMagicKeywords[SectionID.BasicPassage]))
                return new BasicPassageSection();
            return new IndependentQSection();
        }

        private bool RegexIsMatch(string text, List<string> patterns)
        {
            bool matching = true;
            foreach (string pattern in patterns)
            {
                if (!Regex.IsMatch(text, pattern))
                {
                    matching = false;
                    break;
                }
            }
            return matching;
        }
    }

    public enum SectionID
    {
        DefaultIndependentQuestions = 0,
        BasicPassage,
        PassageWithBlanks
    }
}

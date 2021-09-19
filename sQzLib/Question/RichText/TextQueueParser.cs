using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace sQzLib
{
    class TextQueueParser
    {
        public TextQueueParser()
        {
            if (!QSheetSection.LoadSectionMagicKeywords())
                QSheetSection.InitDefaultMagicKeywords();
        }

        public List<QSheetSection> ParseTokens(IEnumerable<IText> tokens)
        {
            List<QSheetSection> sections = new List<QSheetSection>();
            var itor = tokens.GetEnumerator();
            QSheetSection.TrimToFirstSection(itor);
            while(itor.Current != null)
            {
                if(QSheetSection.SECTION_MAGIC_PREFIX.Length > 0 &&
                    !itor.Current.StartsWith(QSheetSection.SECTION_MAGIC_PREFIX))
                {
                    System.Windows.MessageBox.Show("ParseTokens: From the end, line " +
                        Utils.CountEnumerator(itor) + " doesn't have section magic prefix " + QSheetSection.SECTION_MAGIC_PREFIX);
                    return sections;
                }
                QSheetSection section = SelectSection(itor.Current.GetInnerText());
                if(!section.Parse(itor))
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
            if(RegexIsMatch(text, QSheetSection.SectionMagicKeywords[SectionTypeID.PassageWithBlanks]))
                return new PassageWithBlanks();
            if (RegexIsMatch(text, QSheetSection.SectionMagicKeywords[SectionTypeID.BasicPassage]))
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

    public enum SectionTypeID
    {
        DefaultIndependentQuestions = 0,
        BasicPassage,
        PassageWithBlanks
    }
}

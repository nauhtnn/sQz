using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Controls;

namespace sQzLib
{
    public class QSheet2Docx: DocxExporter
    {
        private void WriteSingleQuestion(Question question, int questionIdx)
        {
            if(Utils.IsRichText(question.Stem))
            {
                foreach(Paragraph p in
                    Utils.CreateDocxUnderlineParagraphs((questionIdx + 1).ToString() + ") ", question.Stem))
                    mDocxBody.AppendChild(p);
            }
            else
            {
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(
                    (questionIdx + 1).ToString() + ") " + question.Stem))));
            }

            char optionLabel = 'A';
            int entireAnswerSheet_optionIdx = questionIdx * Question.NUMBER_OF_OPTIONS;

            for(int optionIdx = 0; optionIdx < Question.NUMBER_OF_OPTIONS;
                ++optionIdx, ++optionLabel, ++entireAnswerSheet_optionIdx)
            {
                StringBuilder option = new StringBuilder();
                option.Append(optionLabel + ") " + question.vAns[optionIdx]);
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(option.ToString()))));
            }
        }

        public void WriteAnswerKeys(char[] answerKey)
        {
            int entireAnswerSheet_optionIdx = 0;
            int questionIdx = -1;
            while(entireAnswerSheet_optionIdx < answerKey.Length - Question.NUMBER_OF_OPTIONS)
            {
                char corrected_label = 'A';
                bool notReachCorrectAnswer = true;
                for (int optionIdx = 0; optionIdx < Question.NUMBER_OF_OPTIONS;
                    ++optionIdx, ++entireAnswerSheet_optionIdx)
                    if (notReachCorrectAnswer)
                    {
                        if (answerKey[entireAnswerSheet_optionIdx] == '0')
                            ++corrected_label;
                        else
                            notReachCorrectAnswer = false;
                    }
                mDocxBody.AppendChild(new Paragraph(new Run(
                    new Text((++questionIdx).ToString() + ") " +
                    Txt.s._((int)TxI.PRINT_CORRECT_LABEL) + corrected_label))));
            }
        }

        public void WriteQSheetInfo(string qSheetID)
        {
            WriteDocxTitle();
            mDocxBody.AppendChild(new Paragraph(new Run(
                new Text(Txt.s._((int)TxI.PRINT_PAPER_ID) + qSheetID))));
        }

        public void WriteQsheet(QuestSheet qsheet, char[] answerKey)
        {
            WriteQSheetInfo(qsheet.GetGlobalID_withTestType());
            int questionIdx = -1;
            char partLabel = '@';
            foreach (QSheetSection s in qsheet.Sections)
            {
                mDocxBody.AppendChild(new Paragraph(CreateBoldItalicRun(
                    Txt.s._((int)TxI.PART) + ++partLabel + "\n" + s.Requirements)));
                BasicPassageSection passage_section = s as BasicPassageSection;
                if (passage_section != null)
                    mDocxBody.AppendChild(new Paragraph(new Run(new Text(passage_section.Passage))));
                foreach (Question q in s.Questions)
                    WriteSingleQuestion(q, ++questionIdx);
            }
            WriteAnswerKeys(answerKey);
            WritePageBreak();
        }
    }
}

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
        private void WriteSingleQuestion(Question question, int questionIdx, char[] answerKey)
        {
            if(Utils.IsRichText(question.Stem))
            {
                mDocxBody.AppendChild(new Paragraph(new Run(new Text((questionIdx + 1).ToString() + ")"))));
                Run underlined = new Run(new Text("underlined underlined underlined"));
                underlined.RunProperties = new RunProperties();
                underlined.RunProperties.Underline = new Underline();
                underlined.RunProperties.Underline.Val = UnderlineValues.Single;
                Paragraph p = new Paragraph(underlined);
                p.ParagraphProperties = new ParagraphProperties();
                p.ParagraphProperties.NumberingProperties = new NumberingProperties();
                p.ParagraphProperties.NumberingProperties.NumberingId = new NumberingId();
                p.ParagraphProperties.NumberingProperties.NumberingId.Val = 0;
                mDocxBody.AppendChild(p);
            }
            else
            {
                StringBuilder stem = new StringBuilder();
                stem.Append((questionIdx + 1).ToString() + ") ");
                stem.Append(question.Stem);
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(stem.ToString()))));
            }

            char optionLabel = 'A';
            char corrected_label = 'A';
            int entireAnswerSheet_optionIdx = questionIdx * Question.NUMBER_OF_OPTIONS;
            bool notReachCorrectAnswer = true;

            for(int optionIdx = 0; optionIdx < Question.NUMBER_OF_OPTIONS;
                ++optionIdx, ++optionLabel, ++entireAnswerSheet_optionIdx)
            {
                StringBuilder option = new StringBuilder();
                option.Append(optionLabel + ") " + question.vAns[optionIdx]);
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(option.ToString()))));
                if(notReachCorrectAnswer)
                {
                    if (answerKey[entireAnswerSheet_optionIdx] == '0')
                        ++corrected_label;
                    else
                        notReachCorrectAnswer = false;
                }
            }

            mDocxBody.AppendChild(new Paragraph(new Run(
                new Text(Txt.s._((int)TxI.PRINT_CORRECT_LABEL) + corrected_label))));
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
            foreach (QSheetSection s in qsheet.Sections)
            {
                Run run = new Run(new Text(s.Requirements));
                run.RunProperties = new RunProperties();
                run.RunProperties.Bold = new Bold();
                run.RunProperties.Bold.Val = true;
                run.RunProperties.Italic = new Italic();
                run.RunProperties.Italic.Val = true;
                mDocxBody.AppendChild(new Paragraph(run));
                BasicPassageSection passage_section = s as BasicPassageSection;
                if (passage_section != null)
                    mDocxBody.AppendChild(new Paragraph(new Run(new Text(passage_section.Passage))));
                foreach (Question q in s.Questions)
                    WriteSingleQuestion(q, ++questionIdx, answerKey);
            }
            WritePageBreak();
        }
    }
}

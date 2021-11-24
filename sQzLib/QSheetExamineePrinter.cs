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
    public class QSheetExamineePrinter: DocxExporter
    {
        public string GetInnerTextOfRichTextSpan(RichTextBox richText)
        {
            StringBuilder text = new StringBuilder();
            foreach (var p in
                richText.Document.Blocks.OfType<System.Windows.Documents.Paragraph>())
            {
                foreach (var span in p.Inlines.OfType<System.Windows.Documents.Span>())
                    foreach(var run in span.Inlines.OfType< System.Windows.Documents.Run>())
                    text.Append(run.Text);
            }
            return text.ToString();
        }

        private void WriteSingleQuestionWithSeletedLabel(Question question, int questionIdx, byte[] optionStatusArray,
            char[] answerKey)
        {
            if(Utils.IsRichText(question.Stem))
            {
                mDocxBody.AppendChild(new Paragraph(new Run(new Text((questionIdx + 1).ToString() + ")"))));
                Paragraph p = LookupUnderlinedParagraph(
                    GetInnerTextOfRichTextSpan(Utils.GetRichText(question.Stem)));
                if (p == null)
                    System.Windows.MessageBox.Show("Cannot find underlined paragraph: " + question.Stem);
                else
                {
                    var p2 = p.Clone() as Paragraph;
                    p2.ParagraphProperties.NumberingProperties.NumberingId.Val = 0;
                    mDocxBody.AppendChild(p2);
                }
            }
            else
            {
                StringBuilder stem = new StringBuilder();
                stem.Append((questionIdx + 1).ToString() + ") ");
                stem.Append(question.Stem);
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(stem.ToString()))));
            }

            char optionLabel = 'A';
            char selectedLabel = 'A';
            char corrected_label = 'A';
            int entireAnswerSheet_optionIdx = questionIdx * Question.NUMBER_OF_OPTIONS;
            bool noSelection = true;
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
                if (noSelection)
                {
                    if (optionStatusArray[entireAnswerSheet_optionIdx] == 0)
                        ++selectedLabel;
                    else
                        noSelection = false;
                }
            }

            StringBuilder sb = new StringBuilder();
            if (noSelection)
                sb.Append(Txt.s._((int)TxI.PRINT_NO_SELECTED));
            else
                sb.Append(Txt.s._((int)TxI.PRINT_SELECTED) + selectedLabel);
            sb.Append("    " + Txt.s._((int)TxI.PRINT_CORRECT_LABEL) + corrected_label);
            mDocxBody.AppendChild(new Paragraph(new Run(
                    new Text(sb.ToString()))));
        }

        public void WriteThisExaminee(QuestSheet qsheet, ExamineeS0 examinee, char[] answerKey)
        {
            WriteExamineeInfo(examinee, qsheet.GetGlobalID_withTestType());
            WriteQsheetWithSelectedLabel(qsheet, examinee.AnswerSheet.BytesOfAnswer, answerKey);
            WriteExamineeResult(examinee);
        }

        public void WriteExamineeInfo(ExamineeS0 examinee, string qSheetID)
        {
            WriteDocxTitle();
            StringBuilder info = new StringBuilder();
            info.Append(Txt.s._((int)TxI.PRINT_NAME) + examinee.Name +
                "    " + Txt.s._((int)TxI.PRINT_ID) + examinee.ID +
                "    " + Txt.s._((int)TxI.PRINT_PAPER_ID) + qSheetID);
            mDocxBody.AppendChild(new Paragraph(new Run(new Text(info.ToString()))));
        }

        public void WriteExamineeResult(ExamineeS0 examinee)
        {
            mDocxBody.AppendChild(new Paragraph(new Run(
                new Text(Txt.s._((int)TxI.PRINT_CORRECT_COUNT) + examinee.CorrectCount))));
        }

        public void WriteQsheetWithSelectedLabel(QuestSheet qsheet, byte[] bytesOfAnswer, char[] answerKey)
        {
            int questionIdx = -1;
            foreach (QSheetSection s in qsheet.Sections)
            {
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(s.Requirements))));
                BasicPassageSection passage_section = s as BasicPassageSection;
                if (passage_section != null)
                    mDocxBody.AppendChild(new Paragraph(new Run(new Text(passage_section.Passage))));
                foreach (Question q in s.Questions)
                    WriteSingleQuestionWithSeletedLabel(q, ++questionIdx, bytesOfAnswer, answerKey);
            }
        }

        
    }
}

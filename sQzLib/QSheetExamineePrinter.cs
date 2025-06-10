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

        private void WriteSeletedLabel(Question question, int questionIdx, byte[] optionStatusArray, char[] answerKeys)
        {
            char selectedLabel = 'A';
            int entireAnswerSheet_optionIdx = questionIdx * Question.NUMBER_OF_OPTIONS;
            bool noSelection = true;
            char correctLabel = 'A';
            bool notFoundCorrect = true;

            for(int optionIdx = 0; optionIdx < Question.NUMBER_OF_OPTIONS;
                ++optionIdx, ++entireAnswerSheet_optionIdx)
            {
                if (noSelection)
                {
                    if (optionStatusArray[entireAnswerSheet_optionIdx] == 0)
                        ++selectedLabel;
                    else
                        noSelection = false;
                }
                if(notFoundCorrect)
                {
                    if (answerKeys[entireAnswerSheet_optionIdx] == '0')
                        ++correctLabel;
                    else
                        notFoundCorrect = false;
                }
            }

            StringBuilder selection = new StringBuilder();
            selection.Append((questionIdx + 1).ToString() + ") ");

            if (noSelection)
                selection.Append(Txt.s._((int)TxI.PRINT_NO_SELECTED));
            else
                selection.Append(Txt.s._((int)TxI.PRINT_SELECTED) + selectedLabel + ". ");
            selection.Append(Txt.s._((int)TxI.PRINT_CORRECT_LABEL) + correctLabel + ".");

            mDocxBody.AppendChild(new Paragraph(new Run(new Text(selection.ToString()))));
        }

        public void WriteThisExaminee(QuestSheet qsheet, ExamineeS0 examinee, char[] answerKey)
        {
            WriteExamineeInfo(examinee, qsheet.GetGlobalID_withTestType());
            WriteExamineeResult(examinee);
            WriteSelectedLabels(qsheet, examinee.AnswerSheet.BytesOfAnswer, answerKey);
        }

        public void WriteExamineeInfo(ExamineeS0 examinee, string qSheetID)
        {
            WriteDocxTitle();
            StringBuilder info = new StringBuilder();
            info.Append(Txt.s._((int)TxI.PRINT_NAME) + examinee.Name +
                "    " + Txt.s._((int)TxI.PRINT_ID) + examinee.ID +
                "    " + Txt.s._((int)TxI.PRINT_PAPER_ID) + qSheetID);
            mDocxBody.AppendChild(new Paragraph(CreateBoldItalicRun(info.ToString())));
        }

        public void WriteExamineeResult(ExamineeS0 examinee)
        {
            mDocxBody.AppendChild(new Paragraph(CreateBoldItalicRun(
                Txt.s._((int)TxI.PRINT_CORRECT_COUNT) + examinee.CorrectCount)));
        }

        public void WriteSelectedLabels(QuestSheet qsheet, byte[] bytesOfAnswer, char[] answerKey)
        {
            int questionIdx = -1;
            foreach (QSheetSection s in qsheet.Sections)
                foreach (Question q in s.Questions)
                    WriteSeletedLabel(q, ++questionIdx, bytesOfAnswer, answerKey);
        }

        
    }
}

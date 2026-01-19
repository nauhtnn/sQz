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
            char currentUserLabel = 'A';
            StringBuilder userSelectedLabels = new StringBuilder();
            int entireAnswerSheet_optionIdx = questionIdx * OptionSelectAnswer.OPTION_COUNT;
            bool noSelection = true;
            char currentKeyLabel = 'A';
            StringBuilder keyLabels = new StringBuilder();
            bool notFoundCorrect = true;

            for(int optionIdx = 0; optionIdx < OptionSelectAnswer.OPTION_COUNT;
                ++optionIdx, ++entireAnswerSheet_optionIdx)
            {
                if(question.QuestionType == AnswerType.SingleAnswer)
                {
                    if (noSelection)
                    {
                        if (optionStatusArray[entireAnswerSheet_optionIdx] != QuestionAnswer.TRUE)
                            ++currentUserLabel;
                        else
                        {
                            noSelection = false;
                            userSelectedLabels.Append(currentUserLabel + " ");
                        }   
                    }
                    if (notFoundCorrect)
                    {
                        if (answerKeys[entireAnswerSheet_optionIdx] != QuestionAnswer.TRUE)
                            ++currentKeyLabel;
                        else
                        {
                            notFoundCorrect = false;
                            keyLabels.Append(currentKeyLabel + " ");
                        }
                    }
                }
                else
                {
                    if (optionStatusArray[entireAnswerSheet_optionIdx] == QuestionAnswer.FALSE)
                    {
                        noSelection = false;
                        userSelectedLabels.Append("S ");
                    }
                    else if (optionStatusArray[entireAnswerSheet_optionIdx] == QuestionAnswer.TRUE)
                    {
                        noSelection = false;
                        userSelectedLabels.Append("Đ ");
                    }
                    else
                        userSelectedLabels.Append("_ ");

                    if (answerKeys[entireAnswerSheet_optionIdx] != QuestionAnswer.TRUE)
                        keyLabels.Append("S ");
                    else
                        keyLabels.Append("Đ ");
                }
            }

            if (userSelectedLabels.Length > 1)
                userSelectedLabels.Remove(userSelectedLabels.Length - 1, 1);
            if (keyLabels.Length > 1)
                keyLabels.Remove(keyLabels.Length - 1, 1);

            StringBuilder selection = new StringBuilder();
            selection.Append((questionIdx + 1).ToString() + ") ");

            if (noSelection)
                selection.Append(Txt.s._((int)TxI.PRINT_NO_SELECTED));
            else
                selection.Append(Txt.s._((int)TxI.PRINT_SELECTED) + userSelectedLabels.ToString() + ". ");
            selection.Append(Txt.s._((int)TxI.PRINT_CORRECT_LABEL) + keyLabels.ToString() + ".");

            mDocxBody.AppendChild(new Paragraph(new Run(new Text(selection.ToString()))));
        }

        public void WriteThisExaminee(QuestSheet qsheet, ExamineeS0 examinee, char[] answerKey)
        {
            WriteExamineeInfo(examinee, qsheet.GetGlobalID_withSubject());
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
                Txt.s._((int)TxI.PRINT_CORRECT_COUNT) + examinee.Grade2Decimal)));
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

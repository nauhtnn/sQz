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
    public class QSheetExamineePrinter: IDisposable
    {
        WordprocessingDocument mDocx;
        Body mDocxBody;
        public bool CreateDocx(string fpath)
        {
            try
            {
                mDocx = WordprocessingDocument.Create(fpath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
                MainDocumentPart mainPart = mDocx.AddMainDocumentPart();
                mainPart.Document = new Document();
                mDocxBody = mainPart.Document.AppendChild(new Body());
            }
            catch (OpenXmlPackageException e)
            {
                mDocx = null;
                mDocxBody = null;
                System.Windows.MessageBox.Show(e.ToString());
                return false;
            }
            catch (System.IO.IOException e)
            {
                mDocx = null;
                mDocxBody = null;
                System.Windows.MessageBox.Show(e.ToString());
                return false;
            }
            return true;
        }

        private void WriteSingleQuestionWithSeletedLabel(Question question, int questionIdx, byte[] optionStatusArray)
        {
            StringBuilder stem = new StringBuilder();
            stem.Append((questionIdx + 1).ToString() + ") ");
            stem.Append(question.Stem);
            mDocxBody.AppendChild(new Paragraph(new Run(new Text(stem.ToString()))));

            char optionLabel = 'A';
            char selectedLabel = 'A';
            int entireAnswerSheet_optionIdx = questionIdx * Question.NUMBER_OF_OPTIONS;
            bool noSelection = true;

            for(int optionIdx = 0; optionIdx < Question.NUMBER_OF_OPTIONS;
                ++optionIdx, ++optionLabel, ++entireAnswerSheet_optionIdx)
            {
                StringBuilder option = new StringBuilder();
                option.Append(optionLabel + ") " + question.vAns[optionIdx]);
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(option.ToString()))));
                if (noSelection)
                {
                    if (optionStatusArray[entireAnswerSheet_optionIdx] == 0)
                        ++selectedLabel;
                    else
                        noSelection = false;
                }
            }
            
            if(noSelection)
                mDocxBody.AppendChild(new Paragraph(new Run(new Text("No selection."))));
            else
                mDocxBody.AppendChild(new Paragraph(new Run(new Text("Selected: " + selectedLabel))));
        }

        public void WriteThisExaminee(QuestSheet qsheet, ExamineeS0 examinee)
        {
            WriteExamineeInfo(examinee);
            WriteQsheetWithSelectedLabel(qsheet, examinee.AnswerSheet.BytesOfAnswer);
            WriteExamineeResult(examinee);
        }

        public void WriteExamineeInfo(ExamineeS0 examinee)
        {
            mDocxBody.AppendChild(new Paragraph(new Run(new Text("<ID> " + examinee.ID))));
            mDocxBody.AppendChild(new Paragraph(new Run(new Text("<Name> " + examinee.Name))));
            mDocxBody.AppendChild(new Paragraph(new Run(new Text("<Birthdate> " + examinee.Birthdate))));
        }

        public void WriteExamineeResult(ExamineeS0 examinee)
        {
            mDocxBody.AppendChild(new Paragraph(new Run(new Text("<CorrectCount> " + examinee.CorrectCount))));
        }

        public void WriteQsheetWithSelectedLabel(QuestSheet qsheet, byte[] bytesOfAnswer)
        {
            int questionIdx = -1;
            foreach (QSheetSection s in qsheet.Sections)
            {
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(s.Requirements))));
                BasicPassageSection passage_section = s as BasicPassageSection;
                if (passage_section != null)
                    mDocxBody.AppendChild(new Paragraph(new Run(new Text(passage_section.Passage))));
                foreach (Question q in s.Questions)
                    WriteSingleQuestionWithSeletedLabel(q, ++questionIdx, bytesOfAnswer);
            }
        }

        private static bool IsUnderlined(Paragraph para)
        {
            foreach (Run run in para.ChildElements.OfType<Run>())
            {
                if (IsBoldItalicUnderline(run))
                    return true;
            }
            return false;
        }

        private static bool IsBoldItalicUnderline(Run run)
        {
            if (run.RunProperties == null ||
                run.RunProperties.Underline == null || run.RunProperties.Underline.Val == UnderlineValues.None)
                return false;
            return true;
        }

        public void Dispose()
        {
            if (mDocx != null)
                mDocx.Close();
        }

        private Paragraph WriteRichText(string text)
        {
            RichTextBox richText = new RichTextBox();
            byte[] bytes = new byte[text.Length];
            char[] chars = text.ToCharArray();
            for (int i = 0; i < text.Length; ++i)
                bytes[i] = (byte)chars[i];
            System.IO.MemoryStream stream = new System.IO.MemoryStream(bytes);
            var range = new System.Windows.Documents.TextRange(richText.Document.ContentStart, richText.Document.ContentEnd);
            range.Load(stream, System.Windows.DataFormats.Rtf);
            foreach (System.Windows.Documents.Paragraph p in
                richText.Document.Blocks.OfType<System.Windows.Documents.Paragraph>())
            {
                foreach (System.Windows.Documents.Run run
                    in p.Inlines.OfType<System.Windows.Documents.Run>())
                    foreach(System.Windows.TextDecoration decor in run.TextDecorations)
                        if(decor.Equals(System.Windows.TextDecorations.Underline))
                        {

                        }
                
            }
        }
    }
}

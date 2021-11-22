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
    public class QSheet2Docx: IDisposable
    {
        static WordprocessingDocument mDocx;
        static Body mDocxBody;
        static List<Paragraph> UnderlinedParagraphs;
        static Table DocxTitle;

        public static void CloseDocx()
        {
            if (mDocx != null)
                mDocx.Close();
        }

        static bool DocxCreated = false;

        public static bool CreateDocx(string fpath)
        {
            if (DocxCreated)
                return true;//suppose ok
            DocxCreated = true;

            try
            {
                mDocx = WordprocessingDocument.Create(fpath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
                MainDocumentPart mainPart;
                if (mDocx.MainDocumentPart == null)
                    mainPart = mDocx.AddMainDocumentPart();
                else
                    mainPart = mDocx.MainDocumentPart;
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
            LoadDocxTitle("Title_3.docx");
            return true;
        }

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

        static bool TitleLoaded = false;

        public static void LoadDocxTitle(string filePath)
        {
            if (TitleLoaded)
                return;
            TitleLoaded = true;
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Open(filePath, false);
            }
            catch (OpenXmlPackageException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return;
            }
            catch (System.IO.IOException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return;
            }
            Body body = doc.MainDocumentPart.Document.Body;
            foreach (Table p in body.ChildElements.OfType<Table>())
            {
                DocxTitle = p.Clone() as Table;
            }
            doc.Close();
        }

        public void WriteDocxTitle()
        {
            mDocxBody.AppendChild(DocxTitle.Clone() as Table);
        }

        public void WriteQSheetInfo(ExamineeS0 examinee, string qSheetID)
        {
            WriteDocxTitle();
            StringBuilder info = new StringBuilder();
            info.Append(Txt.s._((int)TxI.PRINT_NAME) + examinee.Name +
                "    " + Txt.s._((int)TxI.PRINT_ID) + examinee.ID +
                "    " + Txt.s._((int)TxI.PRINT_PAPER_ID) + qSheetID);
            mDocxBody.AppendChild(new Paragraph(new Run(new Text(info.ToString()))));
        }

        public void WriteQsheet(QuestSheet qsheet, char[] answerKey)
        {
            int questionIdx = -1;
            foreach (QSheetSection s in qsheet.Sections)
            {
                mDocxBody.AppendChild(new Paragraph(new Run(new Text(s.Requirements))));
                BasicPassageSection passage_section = s as BasicPassageSection;
                if (passage_section != null)
                    mDocxBody.AppendChild(new Paragraph(new Run(new Text(passage_section.Passage))));
                foreach (Question q in s.Questions)
                    WriteSingleQuestion(q, ++questionIdx, answerKey);
            }
        }

        public void Dispose()
        {
            if (mDocx != null)
                mDocx.Close();
        }

        private bool IsUnderlined(System.Windows.Documents.Paragraph para)
        {
            foreach (System.Windows.Documents.Run run in para.Inlines)
            {
                if (IsUnderlined(run))
                    return true;
            }
            return false;
        }

        private bool IsUnderlined(System.Windows.Documents.Run run)
        {
            foreach (System.Windows.TextDecoration decor in run.TextDecorations)
                if (decor.Equals(System.Windows.TextDecorations.Underline))
                    return true;
            return false;
        }

        //private List<Paragraph> WriteRichText(string text)
        //{
        //    RichTextBox richText = Utils.GetRichText(text);
        //    List<Paragraph> paras = new List<Paragraph>();
        //    foreach (System.Windows.Documents.Paragraph p in
        //        richText.Document.Blocks.OfType<System.Windows.Documents.Paragraph>())
        //    {
        //        Paragraph para = new Paragraph();
        //        foreach(var x in p.Inlines)
        //        {
        //            if (x.GetType() != typeof(System.Windows.Documents.Span))
        //            {
        //                int a = 0;
        //                ++a;
        //            }
        //            var s = x as System.Windows.Documents.Span;
        //            foreach(var r in s.Inlines.OfType<System.Windows.Documents.Run>())
        //            {
        //                string t = r.Text;
        //                foreach (System.Windows.Textlo decor in r.TextDecorations)
        //                    if (decor.Equals(System.Windows.TextDecorations.Underline))
        //                    {
        //                        int b = 0;
        //                        ++b;
        //                    }
        //            }
        //        }
        //        foreach (System.Windows.Documents.Run run
        //            in p.Inlines.OfType<System.Windows.Documents.Run>())
        //        {
        //            Run docx_run = new Run(run.Text);
        //            foreach (System.Windows.TextDecoration decor in run.TextDecorations)
        //                if (decor.Equals(System.Windows.TextDecorations.Underline))
        //                    docx_run.AppendChild(new RunProperties(new Underline()));
        //            para.AppendChild(docx_run);
        //        }
        //        paras.Add(para);
        //    }
        //    return paras;
        //}

        static bool UnderlinedLoaded = false;

        public static void LoadUnderlinedParagraphs(string fpath)
        {
            if (UnderlinedLoaded)
                return;
            UnderlinedLoaded = true;
            UnderlinedParagraphs = new List<Paragraph>();
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Open(fpath, false);
            }
            catch (OpenXmlPackageException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return;
            }
            catch (System.IO.IOException e)
            {
                System.Windows.MessageBox.Show(e.ToString());
                return;
            }
            Body body = doc.MainDocumentPart.Document.Body;
            foreach (Paragraph p in body.ChildElements.OfType<Paragraph>())
            {
                DocumentFormat.OpenXml.Drawing.Blip bl =
                    p.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault();
                if (bl == null)
                {
                    if (IsUnderlinedDocx(p))
                        UnderlinedParagraphs.Add(p);
                }
            }
            doc.Close();
        }

        public static bool IsUnderlinedDocx(Paragraph para)
        {
            foreach (Run run in para.ChildElements.OfType<Run>())
            {
                if (IsBoldItalicUnderline(run))
                    return true;
            }
            return false;
        }

        public static bool IsBoldItalicUnderline(Run run)
        {
            if (run.RunProperties == null ||
                run.RunProperties.Underline == null || run.RunProperties.Underline.Val == UnderlineValues.None)
                return false;
            return true;
        }
    }
}

﻿using System;
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
                bool exist = System.IO.File.Exists(fpath);
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
            LoadUnderlinedParagraphs("underlined.docx");
            LoadDocxTitle("Title_3.docx");
            return true;
        }

        public Paragraph LookupUnderlinedParagraph(string text)
        {
            foreach (Paragraph p in UnderlinedParagraphs)
                if (text.Trim().Equals(p.InnerText.Trim()))
                    return p;
            return null;
        }

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

        public void WritePageBreak()
        {
            var PageBreakParagraph = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
            mDocxBody.AppendChild(PageBreakParagraph);
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
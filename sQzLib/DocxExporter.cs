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
    public class DocxExporter
    {
        protected static WordprocessingDocument mDocx;
        protected static Body mDocxBody;
        protected static List<Paragraph> UnderlinedParagraphs;
        protected static Table DocxTitle;
        public static void CloseDocx()
        {
            if (mDocx != null)
            {
                try
                {
                    mDocx.Close();
                }
                catch (ObjectDisposedException e)
                {
                    System.Windows.MessageBox.Show(e.ToString());
                }
            }
            mDocx = null;
            DocxCreated = false;
        }

        static bool DocxCreated = false;

        public static bool ForceCreateDocx(string fpath)
        {
            if (DocxCreated)
            {
                System.Windows.MessageBox.Show("Force create an opening docx!");
                CloseDocx();
                DocxCreated = false;
            }
            return CreateDocx(fpath);
        }

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

        public Paragraph LookupUnderlinedParagraph(string text)
        {
            foreach (Paragraph p in UnderlinedParagraphs)
                if (text.Trim().Equals(p.InnerText.Trim()))
                    return p;
            return null;
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

        public void WritePageBreak()
        {
            var PageBreakParagraph = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
            mDocxBody.AppendChild(PageBreakParagraph);
        }
    }
}

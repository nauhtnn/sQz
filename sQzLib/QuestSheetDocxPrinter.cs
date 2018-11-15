using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;

namespace sQzLib
{
    class QuestSheetDocxPrinter
    {
        private static QuestSheetDocxPrinter sPrinter = null;
        private string testDate;
        private string questSheetId;

        public static QuestSheetDocxPrinter GetInstance()
        {
            if (sPrinter == null)
                sPrinter = new QuestSheetDocxPrinter();
            return sPrinter;
        }

        public void Print(string name, IEnumerable<string> data, string testDate, string qsId)
        {
            WordprocessingDocument doc = null;
            try
            {
                doc = WordprocessingDocument.Create(name, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
            }
            catch (OpenXmlPackageException)
            {
                return;
            }
            catch (System.IO.IOException)
            {
                return;
            }
            this.testDate = testDate;
            questSheetId = qsId;
            MainDocumentPart mainPart = doc.AddMainDocumentPart();

            NumberingDefinitionsPart numberingDefinitionsPart = mainPart.AddNewPart<NumberingDefinitionsPart>("rId1");
            CreateNumberingDefinitionsPartContent(numberingDefinitionsPart);

            FooterPart footerPart1 = mainPart.AddNewPart<FooterPart>("rId7");
            GenerateFooterPartContent(footerPart1);

            mainPart.Document = new Document();
            Body body = doc.MainDocumentPart.Document.AppendChild(new Body());
            body.Append(Create1stSection());
            body.Append(Create2ndSection());
            int i = 3;
            foreach (string s in data)
            {
                ++i;
                if (i < 4)
                    body.AppendChild(CreateParagraph(s, 1));
                else
                {
                    body.AppendChild(CreateParagraph(s, 0));
                    i = -1;
                }
            }
            body.Append(new Paragraph());
            body.Append(CreateLastSection());
            body.Append(CreateSectionProperties());
            doc.Close();
        }

        public Paragraph CreateParagraph(string s, int indent)
        {
            Paragraph paragraph1 = new Paragraph();

            paragraph1.Append(CreateParagraphProperties(indent));
            paragraph1.Append(new Run(new Text(s)));

            return paragraph1;
        }

        public ParagraphProperties CreateParagraphProperties(int lvReference)
        {
            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            ParagraphStyleId paragraphStyleId1 = new ParagraphStyleId() { Val = "ListParagraph" };

            NumberingProperties numberingProperties1 = new NumberingProperties();
            NumberingLevelReference numberingLevelReference1 = new NumberingLevelReference() { Val = lvReference };
            NumberingId numberingId1 = new NumberingId() { Val = 1 };//hardcode

            numberingProperties1.Append(numberingLevelReference1);
            numberingProperties1.Append(numberingId1);

            paragraphProperties1.Append(paragraphStyleId1);
            paragraphProperties1.Append(numberingProperties1);
            paragraphProperties1.SpacingBetweenLines = new SpacingBetweenLines();
            paragraphProperties1.SpacingBetweenLines.Before = "1";
            paragraphProperties1.SpacingBetweenLines.After = "1";

            return paragraphProperties1;
        }

        public SectionProperties CreateSectionProperties()
        {
            SectionProperties sectionProperties1 = new SectionProperties();
            FooterReference footerReference1 = new FooterReference() { Type = HeaderFooterValues.Default, Id = "rId7" };
            PageSize pageSize1 = new PageSize() { Width = (UInt32Value)12240U, Height = (UInt32Value)15840U };
            PageMargin pageMargin1 = new PageMargin() { Top = 1134, Right = (UInt32Value)1080U, Bottom = 1134, Left = (UInt32Value)1080U, Header = (UInt32Value)720U, Footer = (UInt32Value)720U, Gutter = (UInt32Value)0U };
            Columns columns1 = new Columns() { Space = "720" };
            DocGrid docGrid1 = new DocGrid() { LinePitch = 360 };

            sectionProperties1.Append(footerReference1);
            sectionProperties1.Append(pageSize1);
            sectionProperties1.Append(pageMargin1);
            sectionProperties1.Append(columns1);
            sectionProperties1.Append(docGrid1);
            return sectionProperties1;
        }

        // Create content of numberingDefinitionsPart.
        private static void CreateNumberingDefinitionsPartContent(NumberingDefinitionsPart numberingDefinitionsPart)
        {
            Numbering numbering = new Numbering();

            AbstractNum abstractNum = new AbstractNum() { AbstractNumberId = 0 };
            MultiLevelType multiLevelType1 = new MultiLevelType() { Val = MultiLevelValues.HybridMultilevel };

            Level level1 = new Level() { LevelIndex = 0 };
            StartNumberingValue startNumberingValue1 = new StartNumberingValue() { Val = 1 };
            NumberingFormat numberingFormat1 = new NumberingFormat() { Val = NumberFormatValues.Decimal };
            LevelText levelText1 = new LevelText() { Val = "%1." };
            LevelJustification levelJustification1 = new LevelJustification() { Val = LevelJustificationValues.Left };

            PreviousParagraphProperties previousParagraphProperties1 = new PreviousParagraphProperties();
            Indentation indentation1 = new Indentation() { Left = "720", Hanging = "360" };

            previousParagraphProperties1.Append(indentation1);

            level1.Append(startNumberingValue1);
            level1.Append(numberingFormat1);
            level1.Append(levelText1);
            level1.Append(levelJustification1);
            level1.Append(previousParagraphProperties1);

            Level level2 = new Level() { LevelIndex = 1 };
            StartNumberingValue startNumberingValue2 = new StartNumberingValue() { Val = 1 };
            NumberingFormat numberingFormat2 = new NumberingFormat() { Val = NumberFormatValues.LowerLetter };
            LevelText levelText2 = new LevelText() { Val = "(%2)" };
            LevelJustification levelJustification2 = new LevelJustification() { Val = LevelJustificationValues.Left };

            PreviousParagraphProperties previousParagraphProperties2 = new PreviousParagraphProperties();
            Indentation indentation2 = new Indentation() { Left = "1440", Hanging = "360" };

            previousParagraphProperties2.Append(indentation2);

            level2.Append(startNumberingValue2);
            level2.Append(numberingFormat2);
            level2.Append(levelText2);
            level2.Append(levelJustification2);
            level2.Append(previousParagraphProperties2);

            abstractNum.Append(multiLevelType1);
            abstractNum.Append(level1);
            abstractNum.Append(level2);

            NumberingInstance numberingInstance1 = new NumberingInstance() { NumberID = 1 };
            AbstractNumId abstractNumId1 = new AbstractNumId() { Val = 0 };

            numberingInstance1.Append(abstractNumId1);

            numbering.Append(abstractNum);
            numbering.Append(numberingInstance1);

            numberingDefinitionsPart.Numbering = numbering;
        }

        public Paragraph Create1stSection()
        {
            Paragraph paragraph1 = new Paragraph();

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();

            Tabs tabs1 = new Tabs();
            TabStop tabStop1 = new TabStop() { Val = TabStopValues.Center, Position = 2410 };
            TabStop tabStop2 = new TabStop() { Val = TabStopValues.Center, Position = 7655 };

            tabs1.Append(tabStop1);
            tabs1.Append(tabStop2);

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            paragraphMarkRunProperties1.Append(runFonts1);

            paragraphProperties1.Append(tabs1);
            paragraphProperties1.Append(paragraphMarkRunProperties1);

            Run run1 = new Run();

            RunProperties runProperties1 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            runProperties1.Append(runFonts2);
            TabChar tabChar1 = new TabChar();
            Text text1 = new Text();
            text1.Text = "SỞ GIÁO DỤC VÀ ĐÀO TẠO LONG AN";

            run1.Append(runProperties1);
            run1.Append(tabChar1);
            run1.Append(text1);

            Run run2 = new Run();

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts3 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            runProperties2.Append(runFonts3);
            TabChar tabChar2 = new TabChar();

            run2.Append(runProperties2);
            run2.Append(tabChar2);

            Run run3 = new Run();

            RunProperties runProperties3 = new RunProperties();
            RunFonts runFonts4 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold1 = new Bold();

            runProperties3.Append(runFonts4);
            runProperties3.Append(bold1);
            Text text2 = new Text();
            text2.Text = "KÌ THI CHỨNG CHỈ ỨNG DỤNG CNTT";

            run3.Append(runProperties3);
            run3.Append(text2);

            Run run4 = new Run();

            RunProperties runProperties4 = new RunProperties();
            RunFonts runFonts5 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold2 = new Bold();

            runProperties4.Append(runFonts5);
            runProperties4.Append(bold2);
            Break break1 = new Break();

            run4.Append(runProperties4);
            run4.Append(break1);

            Run run5 = new Run();

            RunProperties runProperties5 = new RunProperties();
            RunFonts runFonts6 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold3 = new Bold();

            runProperties5.Append(runFonts6);
            runProperties5.Append(bold3);
            TabChar tabChar3 = new TabChar();
            Text text3 = new Text();
            text3.Text = "TRUNG TÂM NGOẠI NGỮ - TIN HỌC LONG AN";

            run5.Append(runProperties5);
            run5.Append(tabChar3);
            run5.Append(text3);

            Run run6 = new Run();

            RunProperties runProperties6 = new RunProperties();
            RunFonts runFonts7 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold4 = new Bold();
            FontSize fontSize1 = new FontSize() { Val = "20" };

            runProperties6.Append(runFonts7);
            runProperties6.Append(bold4);
            runProperties6.Append(fontSize1);
            TabChar tabChar4 = new TabChar();

            run6.Append(runProperties6);
            run6.Append(tabChar4);

            Run run7 = new Run();

            RunProperties runProperties7 = new RunProperties();
            RunFonts runFonts8 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold5 = new Bold();

            runProperties7.Append(runFonts8);
            runProperties7.Append(bold5);
            Text text4 = new Text();
            text4.Text = "KHÓA NGÀY " + testDate;

            run7.Append(runProperties7);
            run7.Append(text4);

            Run run8 = new Run();

            RunProperties runProperties8 = new RunProperties();
            RunFonts runFonts9 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold6 = new Bold();

            runProperties8.Append(runFonts9);
            runProperties8.Append(bold6);
            Break break2 = new Break();

            run8.Append(runProperties8);
            run8.Append(break2);

            Run run9 = new Run();

            RunProperties runProperties9 = new RunProperties();
            RunFonts runFonts10 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Italic italic1 = new Italic();

            runProperties9.Append(runFonts10);
            runProperties9.Append(italic1);
            TabChar tabChar5 = new TabChar();

            run9.Append(runProperties9);
            run9.Append(tabChar5);

            Run run10 = new Run();

            RunProperties runProperties10 = new RunProperties();
            RunFonts runFonts11 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            runProperties10.Append(runFonts11);
            Text text5 = new Text();
            text5.Text = "——————————";

            run10.Append(runProperties10);
            run10.Append(text5);

            Run run11 = new Run();

            RunProperties runProperties11 = new RunProperties();
            RunFonts runFonts12 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Italic italic2 = new Italic();

            runProperties11.Append(runFonts12);
            runProperties11.Append(italic2);
            TabChar tabChar6 = new TabChar();
            Text text6 = new Text();
            text6.Text = "Thời gian làm bài 30 phút";

            run11.Append(runProperties11);
            run11.Append(tabChar6);
            run11.Append(text6);

            Run run12 = new Run();

            RunProperties runProperties12 = new RunProperties();
            RunFonts runFonts13 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            runProperties12.Append(runFonts13);
            Break break3 = new Break();

            run12.Append(runProperties12);
            run12.Append(break3);

            Run run13 = new Run();

            RunProperties runProperties13 = new RunProperties();
            RunFonts runFonts14 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            runProperties13.Append(runFonts14);
            TabChar tabChar7 = new TabChar();

            run13.Append(runProperties13);
            run13.Append(tabChar7);

            Run run14 = new Run();

            RunProperties runProperties14 = new RunProperties();
            RunFonts runFonts15 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            runProperties14.Append(runFonts15);
            TabChar tabChar8 = new TabChar();
            Text text7 = new Text();
            text7.Text = "—";

            run14.Append(runProperties14);
            run14.Append(tabChar8);
            run14.Append(text7);

            Run run15 = new Run();

            RunProperties runProperties15 = new RunProperties();
            RunFonts runFonts16 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };

            runProperties15.Append(runFonts16);
            Text text8 = new Text();
            text8.Text = "—————";

            run15.Append(runProperties15);
            run15.Append(text8);

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            paragraph1.Append(run2);
            paragraph1.Append(run3);
            paragraph1.Append(run4);
            paragraph1.Append(run5);
            paragraph1.Append(run6);
            paragraph1.Append(run7);
            paragraph1.Append(run8);
            paragraph1.Append(run9);
            paragraph1.Append(run10);
            paragraph1.Append(run11);
            paragraph1.Append(run12);
            paragraph1.Append(run13);
            paragraph1.Append(run14);
            paragraph1.Append(run15);

            return paragraph1;
        }

        private Paragraph Create2ndSection()
        {
            Paragraph paragraph1 = new Paragraph();

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold1 = new Bold();

            paragraphMarkRunProperties1.Append(runFonts1);
            paragraphMarkRunProperties1.Append(bold1);

            paragraphProperties1.Append(justification1);
            paragraphProperties1.Append(paragraphMarkRunProperties1);

            Run run1 = new Run();

            RunProperties runProperties1 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold2 = new Bold();

            runProperties1.Append(runFonts2);
            runProperties1.Append(bold2);
            Text text1 = new Text();
            text1.Text = "MÃ ĐỀ THI " + questSheetId;

            run1.Append(runProperties1);
            run1.Append(text1);

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            return paragraph1;
        }

        private Paragraph CreateLastSection()
        {
            Paragraph paragraph1 = new Paragraph();

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            ParagraphMarkRunProperties paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();
            RunFonts runFonts1 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold1 = new Bold();

            paragraphMarkRunProperties1.Append(runFonts1);
            paragraphMarkRunProperties1.Append(bold1);

            paragraphProperties1.Append(justification1);
            paragraphProperties1.Append(paragraphMarkRunProperties1);

            Run run1 = new Run();

            RunProperties runProperties1 = new RunProperties();
            RunFonts runFonts2 = new RunFonts() { Ascii = "Times New Roman", HighAnsi = "Times New Roman", ComplexScript = "Times New Roman" };
            Bold bold2 = new Bold();

            runProperties1.Append(runFonts2);
            runProperties1.Append(bold2);
            Text text1 = new Text();
            text1.Text = "HẾT";

            run1.Append(runProperties1);
            run1.Append(text1);

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            return paragraph1;
        }

        private void GenerateFooterPartContent(FooterPart footerPart1)
        {
            Footer footer1 = new Footer();

            SdtBlock sdtBlock1 = new SdtBlock();

            SdtProperties sdtProperties1 = new SdtProperties();
            SdtId sdtId1 = new SdtId() { Val = 712706661 };

            SdtContentDocPartObject sdtContentDocPartObject1 = new SdtContentDocPartObject();
            DocPartGallery docPartGallery1 = new DocPartGallery() { Val = "Page Numbers (Bottom of Page)" };
            DocPartUnique docPartUnique1 = new DocPartUnique();

            sdtContentDocPartObject1.Append(docPartGallery1);
            sdtContentDocPartObject1.Append(docPartUnique1);

            sdtProperties1.Append(sdtId1);
            sdtProperties1.Append(sdtContentDocPartObject1);

            SdtEndCharProperties sdtEndCharProperties1 = new SdtEndCharProperties();

            RunProperties runProperties1 = new RunProperties();
            NoProof noProof1 = new NoProof();

            runProperties1.Append(noProof1);

            sdtEndCharProperties1.Append(runProperties1);

            SdtContentBlock sdtContentBlock1 = new SdtContentBlock();

            Paragraph paragraph1 = new Paragraph();

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            ParagraphStyleId paragraphStyleId1 = new ParagraphStyleId() { Val = "Footer" };
            Justification justification1 = new Justification() { Val = JustificationValues.Center };

            paragraphProperties1.Append(paragraphStyleId1);
            paragraphProperties1.Append(justification1);

            Run run1 = new Run();
            FieldChar fieldChar1 = new FieldChar() { FieldCharType = FieldCharValues.Begin };

            run1.Append(fieldChar1);

            Run run2 = new Run();
            FieldCode fieldCode1 = new FieldCode() { Space = SpaceProcessingModeValues.Preserve };
            fieldCode1.Text = " PAGE   \\* MERGEFORMAT ";

            run2.Append(fieldCode1);

            Run run3 = new Run();
            FieldChar fieldChar2 = new FieldChar() { FieldCharType = FieldCharValues.Separate };

            run3.Append(fieldChar2);

            Run run4 = new Run();

            RunProperties runProperties2 = new RunProperties();
            NoProof noProof2 = new NoProof();

            runProperties2.Append(noProof2);
            Text text1 = new Text();
            text1.Text = "1";

            run4.Append(runProperties2);
            run4.Append(text1);

            Run run5 = new Run();

            RunProperties runProperties3 = new RunProperties();
            NoProof noProof3 = new NoProof();

            runProperties3.Append(noProof3);
            FieldChar fieldChar3 = new FieldChar() { FieldCharType = FieldCharValues.End };

            run5.Append(runProperties3);
            run5.Append(fieldChar3);

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            paragraph1.Append(run2);
            paragraph1.Append(run3);
            paragraph1.Append(run4);
            paragraph1.Append(run5);

            sdtContentBlock1.Append(paragraph1);

            sdtBlock1.Append(sdtProperties1);
            sdtBlock1.Append(sdtEndCharProperties1);
            sdtBlock1.Append(sdtContentBlock1);

            Paragraph paragraph2 = new Paragraph();

            ParagraphProperties paragraphProperties2 = new ParagraphProperties();
            ParagraphStyleId paragraphStyleId2 = new ParagraphStyleId() { Val = "Footer" };

            paragraphProperties2.Append(paragraphStyleId2);

            paragraph2.Append(paragraphProperties2);

            footer1.Append(sdtBlock1);
            footer1.Append(paragraph2);

            footerPart1.Footer = footer1;
        }
    }
}

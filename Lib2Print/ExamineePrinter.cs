using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace sQzLib
{
    class ExamineePrinter
    {
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "sqz_template.docx";

            if (!System.IO.File.Exists(filePath))
            {
                MessageBox.Show("No template!");
                return;
            }

            int i;
            for (i = 0; i < 100; ++i)
                if (!System.IO.File.Exists(filePath + i + ".docx"))
                    break;
            if (i == 100)
            {
                MessageBox.Show("Out of index to print. 99 slots have been taken!");
                return;
            }
            var word = new Microsoft.Office.Interop.Word.Application { Visible = true };
            var doc = word.Documents.Open(filePath, ReadOnly: true, Visible: true);
            doc.SaveAs2(filePath + i + ".docx");
            //DocxAddExaminee(doc);
        }

        private void DocxAddExaminee(Microsoft.Office.Interop.Word.Document doc, QuestSheet qsheet, ExamineeS0 nee)
        {
            if (doc.Tables.Count == 0)
            {
                System.Windows.MessageBox.Show("The docx file has no table!");
                return;
            }
            if (doc.Tables[1].Columns.Count < 6)
            {
                System.Windows.MessageBox.Show("The number of column < 6 (Idx, ID, name, birthdate, office, grade)");
                return;
            }
            foreach(Question q in nee.)
        }
    }
}

using MySqlX.XDevAPI.CRUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace sQzLib
{
    class MultiChoiceItemView
    {
        public static UIElement CreateContent(MultiChoiceItem question)
        {
            StackPanel Content = new StackPanel();
            Content.Children.Add(CreateStem(question));

            return Content;
        }
        static UIElement CreateStem(MultiChoiceItem question)
        {
            throw new NotImplementedException();
        }
    }
}

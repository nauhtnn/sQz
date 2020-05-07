using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using sQzLib;

namespace sQzClient
{
    class AnswerSheetView
    {
        public void FirstRenderTableToView(int rowCount, Grid view)
        {
            RenderTableHeaderToView(view);
            RenderTableMiddleRowsToView(rowCount, view);
            RenderTableBottomToView(rowCount, view);
        }

        void RenderTableHeaderToView(Grid view)
        {
            view.RowDefinitions.Add(new RowDefinition());
            Label cell = new Label();
            Grid.SetRow(cell, 0);
            Grid.SetColumn(cell, 0);
            view.Children.Add(cell);
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
            {
                cell = new Label();
                cell.Content = (char)('@' + i);
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleTop];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                cell.FontWeight = FontWeights.Bold;
                Grid.SetRow(cell, 0);
                Grid.SetColumn(cell, i);
                view.Children.Add(cell);
            }
            cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.RightTop];
        }

        void RenderTableBottomToView(int rowCount, Grid view)
        {
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            //bottom lines
            view.RowDefinitions.Add(new RowDefinition());
            Label cell = new Label();
            int lastRowIdx = rowCount;
            cell.Content = lastRowIdx;
            cell.BorderBrush = black;
            cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.LeftBottom];
            cell.HorizontalContentAlignment = HorizontalAlignment.Center;
            cell.FontWeight = FontWeights.Bold;
            Grid.SetRow(cell, lastRowIdx);
            Grid.SetColumn(cell, 0);
            view.Children.Add(cell);
            for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
            {
                cell = new Label(); cell.Content = "x";// mExaminee.mAnsSheet.vAnsItem[lastRowIdx - 1][i - 1].lbl;
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleBottom];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(cell, lastRowIdx);
                Grid.SetColumn(cell, i);
                view.Children.Add(cell);
            }
            cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.RightBottom];
        }

        void RenderTableMiddleRowsToView(int rowCount, Grid view)
        {
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            Label cell = new Label();
            for (int j = 1, n = rowCount; j < n; ++j)
            {
                view.RowDefinitions.Add(new RowDefinition());
                cell = new Label();
                cell.Content = j;
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleTop];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                cell.FontWeight = FontWeights.Bold;
                Grid.SetRow(cell, j);
                Grid.SetColumn(cell, 0);
                view.Children.Add(cell);
                for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
                {
                    cell = new Label(); cell.Content = "x";// mExaminee.mAnsSheet.vAnsItem[j - 1][i - 1].lbl;
                    cell.BorderBrush = black;
                    cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.MiddleTop];
                    cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                    cell.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetRow(cell, j);
                    Grid.SetColumn(cell, i);
                    view.Children.Add(cell);
                }
                cell.BorderThickness = Theme.Singleton.CellThick[(int)ThicknessId.RightTop];
            }


            //for (j = Question.svQuest[0].Count; -1 < j; --j)
            //    cells.RowDefinitions[j].Height = new GridLength(32, GridUnitType.Pixel);
        }
    }
}

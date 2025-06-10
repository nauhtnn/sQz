using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Collections.Generic;
using sQzLib;

namespace sQzLib
{
    public class AnswerGridView
    {
        const int GRID_ROW_HEIGHT = 28;

        Grid UI_Container;

        public static AnswerGridView NewWith(Grid UI_container)
        {
            AnswerGridView grid = new AnswerGridView();
            grid.UI_Container = UI_container;
            return grid;
        }

        public void FirstRenderTableToView(int rowCount, Grid view)
        {
            RenderTableMiddleRowsToView(rowCount, view);
            RenderTableBottomToView(rowCount, view);
        }

        //TODO: already archived, double thought before removing pernamently
        //void RenderTableHeaderToView(Grid view)
        //{
        //    view.RowDefinitions.Add(new RowDefinition());
        //    Label cell = new Label();
        //    Grid.SetRow(cell, 0);
        //    Grid.SetColumn(cell, 0);
        //    view.Children.Add(cell);
        //    SolidColorBrush black = new SolidColorBrush(Colors.Black);
        //    for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
        //    {
        //        cell = new Label();
        //        cell.Content = (char)('@' + i);
        //        cell.BorderBrush = black;
        //        cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.MiddleTop];
        //        cell.HorizontalContentAlignment = HorizontalAlignment.Center;
        //        cell.FontWeight = FontWeights.Bold;
        //        Grid.SetRow(cell, 0);
        //        Grid.SetColumn(cell, i);
        //        view.Children.Add(cell);
        //    }
        //    cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.RightTop];
        //}

        RowDefinition NewAnswerGridRow()
        {
            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength(GRID_ROW_HEIGHT, GridUnitType.Pixel);
            return rd;
        }

        void RenderTableBottomToView(int rowCount, Grid view)
        {
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            //bottom lines
            view.RowDefinitions.Add(NewAnswerGridRow());
            Label cell = new Label();
            int lastRowIdx = rowCount;
            cell.Content = lastRowIdx;
            cell.BorderBrush = black;
            cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.LeftBottom];
            cell.HorizontalContentAlignment = HorizontalAlignment.Center;
            cell.FontWeight = FontWeights.Bold;
            Grid.SetRow(cell, lastRowIdx);
            Grid.SetColumn(cell, 0);
            view.Children.Add(cell);
            for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
            {
                cell = new Label(); cell.Content = "x";// mExaminee.mAnsSheet.vAnsItem[lastRowIdx - 1][i - 1].lbl;
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.MiddleBottom];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                Grid.SetRow(cell, lastRowIdx);
                Grid.SetColumn(cell, i);
                view.Children.Add(cell);
            }
            cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.RightBottom];
        }

        void RenderTableMiddleRowsToView(int rowCount, Grid view)
        {
            SolidColorBrush black = new SolidColorBrush(Colors.Black);
            Label cell = new Label();
            for (int j = 1, n = rowCount; j < n; ++j)
            {
                view.RowDefinitions.Add(NewAnswerGridRow());
                cell = new Label();
                cell.Content = j;
                cell.BorderBrush = black;
                cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.MiddleTop];
                cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                cell.FontWeight = FontWeights.Bold;
                Grid.SetRow(cell, j);
                Grid.SetColumn(cell, 0);
                view.Children.Add(cell);
                for (int i = 1; i <= MultiChoiceItem.N_OPTIONS; ++i)
                {
                    cell = new Label(); cell.Content = "x";// mExaminee.mAnsSheet.vAnsItem[j - 1][i - 1].lbl;
                    cell.BorderBrush = black;
                    cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.MiddleTop];
                    cell.HorizontalContentAlignment = HorizontalAlignment.Center;
                    cell.VerticalContentAlignment = VerticalAlignment.Top;
                    Grid.SetRow(cell, j);
                    Grid.SetColumn(cell, i);
                    view.Children.Add(cell);
                }
                cell.BorderThickness = Theme.Singleton.BorderVisibility[(int)SelectedEdge.RightTop];
            }


            //for (j = Question.svQuest[0].Count; -1 < j; --j)
            //    cells.RowDefinitions[j].Height = new GridLength(32, GridUnitType.Pixel);
        }
    }
}

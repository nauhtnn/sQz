using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace sQzLib
{
    public delegate void PopupCallBack();

    class DialogData
    {
        public string Message;
        public Visibility ButtonsVisibility;
        public string buttonOK_text;
        public string buttonCancel_text;
        public PopupCallBack OK_CallBack;
        public PopupCallBack Cancel_CallBack;
        public string Password;
        public bool IsButtonOK_Clicked;

        private void _Init(string message, string ok_text, string cancel_text,
            string password, PopupCallBack OK_callBack, PopupCallBack cancel_CallBack)
        {
            Message = message;
            if ((ok_text == null || ok_text.Length == 0) &&
                (cancel_text == null || cancel_text.Length == 0))
                ButtonsVisibility = Visibility.Collapsed;
            else
                ButtonsVisibility = Visibility.Visible;
            OK_CallBack = OK_callBack;
            Cancel_CallBack = cancel_CallBack;
            buttonOK_text = ok_text;
            buttonCancel_text = cancel_text;
            Password = password;
            IsButtonOK_Clicked = false;
        }

        public DialogData(string message)
        {
            _Init(message, string.Empty, string.Empty, string.Empty, null, null);
        }

        public DialogData(string message, PopupCallBack cancel_CallBack)
        {
            _Init(message, string.Empty, string.Empty, string.Empty, null, cancel_CallBack);
        }

        public DialogData(string message, string ok_text, string cancel_text,
            string password, PopupCallBack OK_callBack, PopupCallBack cancel_CallBack)
        {
            _Init(message, ok_text, cancel_text, password, OK_callBack, cancel_CallBack);
        }
    }

    public class WPopup
    {
        Window mW;
        TextBlock MessageView;
        TextBox PasswordTextBox;
        static WPopup _s;
        Button mBtnOk;
        Button mBtnCncl;
        Grid mG;
        Queue<string> Messages;
        Queue<DialogData> dialogDataQueue;
        public bool OwnerClosing;
        WPopup()
        {
            IsClosingRequestPending = false;
            mW = new Window();
            mW.Title = Txt.s._((int)TxI.POPUP_TIT);
            mW.Closing += wPopup_Closing;
            mW.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mW.ResizeMode = ResizeMode.NoResize;

            mG = new Grid();
            RowDefinition rd = new RowDefinition();
            mG.RowDefinitions.Add(rd);
            rd = new RowDefinition();
            mG.RowDefinitions.Add(rd);
            rd = new RowDefinition();
            mG.RowDefinitions.Add(rd);
            ColumnDefinition cd = new ColumnDefinition();
            mG.ColumnDefinitions.Add(cd);
            cd = new ColumnDefinition();
            mG.ColumnDefinitions.Add(cd);

            MessageView = new TextBlock();
            MessageView.VerticalAlignment = VerticalAlignment.Center;
            MessageView.TextWrapping = TextWrapping.Wrap;
            MessageView.TextAlignment = TextAlignment.Center;
            Color c = new Color();
            c.R = 0x58;
            c.G = 0xa9;
            c.B = 0xb4;
            MessageView.Background = new SolidColorBrush(c);
            Grid.SetColumnSpan(MessageView, 2);
            mG.Children.Add(MessageView);

            PasswordTextBox = new TextBox();
            PasswordTextBox.TextAlignment = TextAlignment.Center;
            Grid.SetRow(PasswordTextBox, 1);
            Grid.SetColumnSpan(PasswordTextBox, 2);
            mG.Children.Add(PasswordTextBox);

            mBtnOk = new Button();
            mBtnOk.Click += BtnOk_Click;
            Grid.SetRow(mBtnOk, 2);
            mG.Children.Add(mBtnOk);

            mBtnCncl = new Button();
            mBtnCncl.Click += BtnCncl_Click;
            Grid.SetRow(mBtnCncl, 2);
            Grid.SetColumn(mBtnCncl, 1);
            mG.Children.Add(mBtnCncl);

            mW.Content = mG;

            dialogDataQueue = new Queue<DialogData>();

            Messages = new Queue<string>();
            OwnerClosing = false;
        }

        private void BtnCncl_Click(object sender, RoutedEventArgs e)
        {
            if (IsClosingRequestPending)
                return;
            IsClosingRequestPending = true;
            mW.Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            string password = dialogDataQueue.Peek().Password;
            if (password == null || password.Length == 0 ||
                password == PasswordTextBox.Text)
            {
                PasswordTextBox.Text = string.Empty;
                dialogDataQueue.Peek().IsButtonOK_Clicked = true;
                if (IsClosingRequestPending)
                    return;
                IsClosingRequestPending = true;
                mW.Close();
            }
        }

        public static WPopup s
        {
            get
            {
                if (_s == null)
                    _s = new WPopup();
                return _s;
            }
        }

        public static void nwIns(Window oner)
        {
            _s = new WPopup();
            _s.owner = oner;
        }

        public Window owner
        {
            set
            {
                if (value != null)
                {
                    mW.Owner = value;
                    mW.Width = ((0.1 < value.RenderSize.Width) ? value.RenderSize.Width :
                        SystemParameters.PrimaryScreenWidth) / 2;
                    mW.Height = ((0.1 < value.RenderSize.Height) ? value.RenderSize.Height :
                        SystemParameters.PrimaryScreenHeight) / 2;
                    mG.RowDefinitions[2].Height = mG.RowDefinitions[1].Height = new GridLength(mW.Height / 6);
                    if (0 < value.FontSize)
                        MessageView.FontSize = value.FontSize * 1.2;
                }
            }
        }

        private bool IsClosingRequestPending;

        public void Exit()
        {
            OwnerClosing = true;
            if (IsClosingRequestPending)
                return;
            IsClosingRequestPending = true;
            mW.Close();
        }

        public void ShowDialog(string msg)
        {
            dialogDataQueue.Enqueue(new DialogData(msg));
            ShowDialog();
        }

        public void ShowDialog(string msg, PopupCallBack cancel_CallBack)
        {
            dialogDataQueue.Enqueue(new DialogData(msg, cancel_CallBack));
            ShowDialog();
        }

        public void ShowDialog(string msg, string ok, string cncl, string password,
            PopupCallBack OK_CallBack, PopupCallBack cancel_CallBack)
        {
            dialogDataQueue.Enqueue(new DialogData(msg, ok, cncl, password, OK_CallBack,
                cancel_CallBack));
            ShowDialog();
        }

        private void ShowDialog()
        {
            if (dialogDataQueue.Count > 0)
            {
                DialogData peakData = dialogDataQueue.Peek();
                MessageView.Text = peakData.Message;
                mBtnOk.Visibility = mBtnCncl.Visibility =
                    peakData.ButtonsVisibility;
                if (peakData.ButtonsVisibility ==
                    Visibility.Visible)
                {
                    mBtnOk.Content = peakData.buttonOK_text;
                    mBtnCncl.Content = peakData.buttonCancel_text;
                }
                if (!mW.IsVisible)
                    mW.ShowDialog();
            }
            else
                mW.Close();
        }

        private void wPopup_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IsClosingRequestPending = false;
            if (!OwnerClosing)
                e.Cancel = true;
            if(dialogDataQueue.Count > 0)
            {
                if (dialogDataQueue.Peek().IsButtonOK_Clicked)
                    dialogDataQueue.Peek().OK_CallBack?.Invoke();
                else
                    dialogDataQueue.Peek().Cancel_CallBack?.Invoke();
                dialogDataQueue.Dequeue();
            }
            if (dialogDataQueue.Count == 0)
            {
                Window s = sender as Window;
                s.Visibility = Visibility.Collapsed;
            }
            else
                ShowDialog();
        }
    }
}

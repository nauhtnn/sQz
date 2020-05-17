using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace sQzLib
{
    public class Theme
    {
        public SolidColorBrush[] DefinedColors;
        public Thickness[] BorderVisibility;
        static Theme _s;
        public static Theme Singleton
        {
            get {
                if (_s == null)
                    _s = new Theme();
                return _s;
            }
        }

        public Theme()
        {
            DefinedColors = new SolidColorBrush[(int)BrushId.Count];
            if(System.IO.File.Exists("theme.txt"))
            {
                string[] hexColors = System.IO.File.ReadAllLines("theme.txt");
                int j = -1;
                foreach(string hexColor in hexColors)
                {
                    Color color = new Color();
                    color.A = 0xff;
                    int i;
                    if (int.TryParse(hexColor.Substring(0, 2), NumberStyles.AllowHexSpecifier, null, out i))
                        color.R = (byte)i;
                    if (int.TryParse(hexColor.Substring(2, 2), NumberStyles.AllowHexSpecifier, null, out i))
                        color.G = (byte)i;
                    if (int.TryParse(hexColor.Substring(4, 2), NumberStyles.AllowHexSpecifier, null, out i))
                        color.B = (byte)i;
                    SolidColorBrush brush = new SolidColorBrush(color);
                    DefinedColors[++j] = brush;
                }
            }
            
            BorderVisibility = new Thickness[(int)SelectedEdge.Count];
            BorderVisibility[(int)SelectedEdge.LeftTop] = new Thickness(0, 0, 0, 0);
            BorderVisibility[(int)SelectedEdge.MiddleTop] = new Thickness(1, 1, 0, 0);
            BorderVisibility[(int)SelectedEdge.RightTop] = new Thickness(1, 1, 1, 0);
            BorderVisibility[(int)SelectedEdge.LeftBottom] = new Thickness(1, 1, 0, 1);
            BorderVisibility[(int)SelectedEdge.MiddleBottom] = new Thickness(1, 1, 0, 1);
            BorderVisibility[(int)SelectedEdge.RightBottom] = new Thickness(1, 1, 1, 1);
        }
    }

    //from left to right
    public enum BrushId
    {
        BG = 0,
        AnswerSheet_BG,
        Sheet_BG,
        Button_Hover,
        Q_BG,
        QID_BG,
        QID_Color,
        Ans_TopLine,
        Ans_Highlight,
        FG_Gray,
        BG_Gray,
        FG,
        mConn,
        mBackup,
        mReconn,
        mExit,
        mSubmit,
        mBlack,
        Count
    }

    public enum SelectedEdge
    {
        LeftTop = 0,
        MiddleTop,
        RightTop,
        LeftBottom,
        MiddleBottom,
        RightBottom,
        Count
    }
}

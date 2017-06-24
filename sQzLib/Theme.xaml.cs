using System.Windows;
using System.Windows.Media;
using System.Globalization;

namespace sQzLib
{
    public class Theme
    {
        public SolidColorBrush[] _;
        public Thickness[] l;
        static Theme _s;
        public static Theme s
        {
            get {
                if (_s == null)
                    _s = new Theme();
                return _s;
            }
        }

        public Theme()
        {
            _ = new SolidColorBrush[(int)BrushId.Count];
            if(System.IO.File.Exists("theme.txt"))
            {
                string[] cls = System.IO.File.ReadAllLines("theme.txt");
                int j = -1;
                foreach(string cl in cls)
                {
                    Color c = new Color();
                    c.A = 0xff;
                    int i;
                    if (int.TryParse(cl.Substring(0, 2), NumberStyles.AllowHexSpecifier, null, out i))
                        c.R = (byte)i;
                    if (int.TryParse(cl.Substring(2, 2), NumberStyles.AllowHexSpecifier, null, out i))
                        c.G = (byte)i;
                    if (int.TryParse(cl.Substring(4, 2), NumberStyles.AllowHexSpecifier, null, out i))
                        c.B = (byte)i;
                    SolidColorBrush br = new SolidColorBrush(c);
                    _[++j] = br;
                }
            }
            
            l = new Thickness[(int)ThicknessId.Count];
            l[(int)ThicknessId.LT] = new Thickness(0, 0, 0, 0);
            l[(int)ThicknessId.MT] = new Thickness(1, 1, 0, 0);
            l[(int)ThicknessId.RT] = new Thickness(1, 1, 1, 0);
            l[(int)ThicknessId.LB] = new Thickness(1, 1, 0, 1);
            l[(int)ThicknessId.MB] = new Thickness(1, 1, 0, 1);
            l[(int)ThicknessId.RB] = new Thickness(1, 1, 1, 1);
        }
    }

    //from left to right
    public enum BrushId
    {
        BG = 0,
        LeftPanel_BG,
        Sheet_BG,
        Button_Hover,
        Q_BG,
        QID_BG,
        QID_Color,
        Ans_TopLine,
        Ans_Highlight,
        Count
    }

    public enum ThicknessId
    {
        LT = 0,
        MT,
        RT,
        LB,
        MB,
        RB,
        Count
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.iOS.Helpers
{
    public class DialData
    {
        public static List<DialItem> GetDialItems()
        {
            int x1 = 30, x2 = 130, x3 = 225;
            int y1 = 170, y2 = 250, y3 = 330, y4 = 410, y5 = 490;

            return new List<DialItem>()
            {                
                new DialItem() { Text = "1", X=x1, Y=y1 },
                new DialItem() { Text = "2", X=x2, Y=y1, DetailedText="ABC" },
                new DialItem() { Text = "3", X=x3, Y=y1 , DetailedText="DEF"},
                new DialItem() { Text = "4", X=x1, Y=y2, DetailedText="GHI" },
                new DialItem() { Text = "5", X=x2, Y=y2, DetailedText="JKL" },
                new DialItem() { Text = "6", X=x3, Y=y2, DetailedText="MNO"},
                new DialItem() { Text = "7", X=x1, Y=y3, DetailedText="PQRS" },
                new DialItem() { Text = "8", X=x2, Y=y3, DetailedText="TUV" },
                new DialItem() { Text = "9", X=x3, Y=y3, DetailedText="WXYZ" },
                new DialItem() { Text = "*", X=x1, Y=y4 },
                new DialItem() { Text = "0", X=x2, Y=y4, DetailedText="+" },
                new DialItem() { Text = "#", X=x3, Y=y4 }
            };

        }
    }

    public class DialItem {
        public nfloat X { get; set; }
        public nfloat Y { get; set; }
        public String Text { get; set; }
        public String DetailedText { get; set; }        
        public nfloat Width { get; set; } = 70;
        public nfloat Height { get; set; } = 70;         
    }
}

using System;
using System.Collections.Generic;

namespace FreedomVoice.iOS.Helpers
{
    public static class DialData
    {
        private const int X1 = 50, X2 = 130, X3 = 205;
        private const int Y1 = 160, Y2 = 230, Y3 = 300, Y4 = 370;

        public static List<DialItem> Items => new List<DialItem>
        {
            new DialItem { Text = "1", X = X1, Y = Y1 },
            new DialItem { Text = "2", X = X2, Y = Y1, DetailedText="ABC" },
            new DialItem { Text = "3", X = X3, Y = Y1, DetailedText="DEF"},
            new DialItem { Text = "4", X = X1, Y = Y2, DetailedText="GHI" },
            new DialItem { Text = "5", X = X2, Y = Y2, DetailedText="JKL" },
            new DialItem { Text = "6", X = X3, Y = Y2, DetailedText="MNO"},
            new DialItem { Text = "7", X = X1, Y = Y3, DetailedText="PQRS" },
            new DialItem { Text = "8", X = X2, Y = Y3, DetailedText="TUV" },
            new DialItem { Text = "9", X = X3, Y = Y3, DetailedText="WXYZ" },
            new DialItem { Text = "*", X = X1, Y = Y4 },
            new DialItem { Text = "0", X = X2, Y = Y4, DetailedText="+" },
            new DialItem { Text = "#", X = X3, Y = Y4 }
        };
    }

    public class DialItem
    {
        public nfloat X { get; set; }
        public nfloat Y { get; set; }
        public string Text { get; set; }
        public string DetailedText { get; set; }
        public nfloat Width { get; } = 60;
        public nfloat Height { get; } = 60;
    }
}
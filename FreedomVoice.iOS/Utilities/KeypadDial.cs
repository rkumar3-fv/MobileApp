using System;
using System.Collections.Generic;

namespace FreedomVoice.iOS.Utilities
{
    public class KeypadDial
    {
        private readonly nfloat _x1, _x2, _x3;
        private readonly nfloat _y1, _y2, _y3, _y4;

        public const string Plus = "+";

        public KeypadDial(nfloat startPositionX, nfloat startPositionY, nfloat diameter, nfloat distanceX, nfloat distanceY)
        {
            _x1 = startPositionX;
            _x2 = _x1 + distanceX + diameter;
            _x3 = _x2 + distanceX + diameter;

            _y1 = startPositionY;
            _y2 = _y1 + distanceY + diameter;
            _y3 = _y2 + distanceY + diameter;
            _y4 = _y3 + distanceY + diameter;
        }

        public List<DialItem> Items => new List<DialItem>
        {
            new DialItem { Text = "1", X = _x1, Y = _y1 },
            new DialItem { Text = "2", X = _x2, Y = _y1, DetailedText = "A B C" },
            new DialItem { Text = "3", X = _x3, Y = _y1, DetailedText = "D E F"},
            new DialItem { Text = "4", X = _x1, Y = _y2, DetailedText = "G H I" },
            new DialItem { Text = "5", X = _x2, Y = _y2, DetailedText = "J K L" },
            new DialItem { Text = "6", X = _x3, Y = _y2, DetailedText = "M N O"},
            new DialItem { Text = "7", X = _x1, Y = _y3, DetailedText = "P Q R S" },
            new DialItem { Text = "8", X = _x2, Y = _y3, DetailedText = "T U V" },
            new DialItem { Text = "9", X = _x3, Y = _y3, DetailedText = "W X Y Z" },
            new DialItem { Text = "*", X = _x1, Y = _y4, Image = "keypad_asterisk.png" },
            new DialItem { Text = "0", X = _x2, Y = _y4, DetailedText = Plus },
            new DialItem { Text = "#", X = _x3, Y = _y4, Image = "keypad_sharp.png" }
        };
    }

    public class DialItem
    {
        public nfloat X { get; set; }
        public nfloat Y { get; set; }
        public string Text { get; set; }
        public string DetailedText { get; set; }

        public string Image { get; set; }
    }
}
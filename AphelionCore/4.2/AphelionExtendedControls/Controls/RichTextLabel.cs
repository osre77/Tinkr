using System;
using System.Collections;

using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

using Skewworks.NETMF.Resources;

namespace Skewworks.NETMF.Controls
{
    [Serializable]
    public class RichTextLabel : Control
    {

        #region Structures

        [Serializable]
        private struct RichText
        {
            public Font Font;
            public bool Italic;
            public bool Bold;
            public bool Underline;
            public string Text;
            public Color Color;
            public RichText(string Text, Font Font, bool Bold, bool Italic, bool Underline, Color Color)
            {
                this.Text = Text;
                this.Font = Font;
                this.Bold = Bold;
                this.Italic = Italic;
                this.Underline = Underline;
                this.Color = Color;
            }
        }

        #endregion

        #region Variables

        private string _text;
        private bool _autosize;
        private Color _color;
        private ArrayList _parts;
        private Font _font;

        #endregion

        #region Constructors

        public RichTextLabel(string name, string text, Font font, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
            _font = font;
            _text = text;
            ParseText();
        }

        public RichTextLabel(string name, string text, Font font, int x, int y, int width, int height)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _autosize = false;
            _font = font;
            _text = text;
            ParseText();
        }

        public RichTextLabel(string name, string text, Font font, Color foreColor, int x, int y, int width, int height)
        {
            _text = text;
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            _font = font;
            _color = foreColor;
            _autosize = false;
            ParseText();
        }

        #endregion

        #region Properties

        public override bool CanFocus
        {
            get { return false; }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;
                _text = value;
                ParseText();
                Invalidate();
            }
        }

        #endregion

        #region GUI

        protected override void OnRender(int x, int y, int w, int h)
        {
            RichText rt;
            int lw, ly;

            for (int i = 0; i < _parts.Count; i++)
            {
                rt = (RichText)_parts[i];


                // Break text into pieces
                string[] bits = rt.Text.Split(' ');
                int e;
                for (int j = 0; j < bits.Length; j++)
                {
                    if (j < bits.Length - 1)
                        bits[j] += " ";

                    lw = FontManager.ComputeExtentEx(rt.Font, bits[j]).Width;
                    e = x + lw;
                    if (e > Left + Width)
                    {
                        x = Left;
                        y += rt.Font.Height;
                        if (y > Top + Height) break;
                    }

                    if (rt.Underline)
                    {
                        ly = rt.Font.Height + y - rt.Font.Descent + 1;
                        Core.Screen.DrawLine(_color, 1, x, ly, x + lw, ly);
                    }

                    Core.Screen.DrawText(bits[j], rt.Font, rt.Color, x, y);
                    x += FontManager.ComputeExtentEx(rt.Font, bits[j]).Width;

                }
            }
        }

        #endregion

        #region Text Parsing

        private Font FontByName(string name, bool bold, bool italic)
        {
            switch (name.ToLower())
            {
                case "Droid8":
                    if (bold && italic) return Fonts.Droid8BoldItalic;
                    if (bold) return Fonts.Droid8Bold;
                    if (italic) return Fonts.Droid8Italic;
                    return Fonts.Droid8;
                case "Droid9":
                    if (bold && italic) return Fonts.Droid9BoldItalic;
                    if (bold) return Fonts.Droid9Bold;
                    if (italic) return Fonts.Droid9Italic;
                    return Fonts.Droid9;
                case "Droid11":
                    if (bold && italic) return Fonts.Droid11BoldItalic;
                    if (bold) return Fonts.Droid11Bold;
                    if (italic) return Fonts.Droid11Italic;
                    return Fonts.Droid11;
                case "Droid12":
                    if (bold && italic) return Fonts.Droid12BoldItalic;
                    if (bold) return Fonts.Droid12Bold;
                    if (italic) return Fonts.Droid12Italic;
                    return Fonts.Droid12;
                default:
                    if (bold && italic) return Fonts.Droid16BoldItalic;
                    if (bold) return Fonts.Droid16Bold;
                    if (italic) return Fonts.Droid16Italic;
                    return Fonts.Droid16;
            }
        }

        private void ParseText()
        {
            Color col = _color;
            Font fnt = _font;
            bool bBold = false;
            bool bItalic = false;
            bool bUnderline = false;
            int i;
            int iStart = 0;
            string txt = _text;
            ArrayList fonts = new ArrayList();
            ArrayList cols = new ArrayList();


            // Reset Array
            _parts = new ArrayList();

            while (true)
            {
                // Get the next index of a Less Than
                i = txt.IndexOf("<", iStart);
                if (i < 0) break;

                // See if we care about it
                if (txt.Substring(i + 1, 1) == "/")
                {
                    // We're looking for a close here
                    if (txt.Substring(i, 4).ToLower() == "</b>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));

                        bBold = false;
                        txt = txt.Substring(i + 4);
                        iStart = 0;
                    }
                    else if (txt.Substring(i, 4).ToLower() == "</i>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));
                        bItalic = false;
                        txt = txt.Substring(i + 4);
                        iStart = 0;
                    }
                    else if (txt.Substring(i, 4).ToLower() == "</u>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));
                        bUnderline = false;
                        txt = txt.Substring(i + 4);
                        iStart = 0;
                    }
                    else if (txt.Substring(i, 7) == "</font>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));

                        fonts.RemoveAt(fonts.Count - 1);
                        if (fonts.Count > 0)
                            fnt = (Font)fonts[fonts.Count - 1];
                        else
                            fnt = _font;

                        txt = txt.Substring(i + 7);
                        iStart = 0;
                    }
                    else if (txt.Substring(i, 8) == "</color>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));

                        cols.RemoveAt(cols.Count - 1);
                        if (cols.Count > 0)
                            col = (Color)cols[cols.Count - 1];
                        else
                            col = _color;

                        txt = txt.Substring(i + 8);
                        iStart = 0;
                    }
                    else
                        iStart = i + 1;
                }
                else
                {

                    // We're looking for a start here
                    if (txt.Substring(i, 3).ToLower() == "<b>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));
                        bBold = true;
                        iStart = 0;
                        txt = txt.Substring(i + 3);
                    }
                    else if (txt.Substring(i, 3).ToLower() == "<i>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));
                        bItalic = true;
                        txt = txt.Substring(i + 3);
                        iStart = 0;
                    }
                    else if (txt.Substring(i, 3).ToLower() == "<u>")
                    {
                        if (i > 0)
                            _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));
                        bUnderline = true;
                        txt = txt.Substring(i + 3);
                        iStart = 0;
                    }
                    else if (txt.Substring(i, 6).ToLower() == "<font ")
                    {
                        int end = txt.IndexOf(">", i);   // Find the end of the tag

                        if (end > 0)
                        {
                            // Get the segment and replace double quotes with single quotes
                            string strName = Strings.Replace(txt.Substring(i + 6, end - i - 6), "\"", "'");

                            int e = strName.IndexOf("'");
                            if (e >= 0)
                            {
                                strName = strName.Substring(e + 1);
                                e = strName.IndexOf("'");
                                if (e > 0)
                                {
                                    strName = strName.Substring(0, e);

                                    // Valid Tag; add and trim
                                    if (i > 0)
                                        _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));
                                    txt = txt.Substring(end + 1);
                                    iStart = 0;

                                    // Add Font
                                    fnt = FontByName(strName, bBold, bItalic);
                                    fonts.Add(strName);

                                }
                                else
                                    iStart = i + 1; // Invalid tag
                            }
                            else
                                iStart = i + 1; // Invalid tag

                        }
                        else
                            iStart = i + 1; // Invalid tag
                    }
                    else if (txt.Substring(i, 7).ToLower() == "<color ")
                    {
                        int end = txt.IndexOf(">", i);   // Find the end of the tag

                        if (end > 0)
                        {
                            // Get the segment and replace double quotes with single quotes
                            string strName = Strings.Replace(txt.Substring(i + 7, end - i - 7), "\"", "'");

                            int e = strName.IndexOf("'");
                            if (e >= 0)
                            {
                                strName = strName.Substring(e + 1);
                                e = strName.IndexOf("'");
                                if (e >= 0)
                                {
                                    strName = strName.Substring(0, e);

                                    // Valid Tag; add and trim
                                    if (i > 0)
                                        _parts.Add(new RichText(txt.Substring(0, i), fnt, bBold, bItalic, bUnderline, col));
                                    txt = txt.Substring(end + 1);
                                    iStart = 0;

                                    // Add Color
                                    if (strName.IndexOf(',') > 0)
                                    {
                                        col = Colors.FromString(strName);
                                        cols.Add(col);
                                    }
                                    else
                                    {
                                        try
                                        {
                                            col = Colors.FromValue(strName);
                                            cols.Add(col);
                                        }
                                        catch (Exception) { }
                                    }

                                }
                                else
                                    iStart = i + 1; // Invalid tag
                            }
                            else
                                iStart = i + 1; // Invalid tag
                        }
                        else
                            iStart = i + 1; // Invalid tag
                    }
                    else
                        iStart = i + 1;
                }
            }

            if (txt.Length > 0)
                _parts.Add(new RichText(txt, fnt, bBold, bItalic, bUnderline, col));
        }

        #endregion

    }
}

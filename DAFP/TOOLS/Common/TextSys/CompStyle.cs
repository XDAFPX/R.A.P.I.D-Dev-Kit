using System.Linq;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.Common.TextSys
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public abstract class CompStyle : IOwnedBy<TextSpan>, IPriority<CompStyle>
    {
        private class Empty : CompStyle
        {
            public Empty() : base(0)
            {
            }

            public override string Apply()
            {
                return Owner.Text;
            }
        }

        public static CompStyle EMPTY = new Empty();
        protected TextSpan Owner;

        // Suggested default priorities (lower runs earlier -> outer tags)
        protected const int PRIORITY_LINK = 10;
        protected const int PRIORITY_FONT = 20;
        protected const int PRIORITY_SIZE = 30;
        protected const int PRIORITY_COLOR = 40;
        protected const int PRIORITY_EFFECT = 50; // b, i, u, s, smallcaps
        protected const int PRIORITY_SUB_SUP = 60;
        protected const int PRIORITY_MARK = 70;

        protected CompStyle(int priority)
        {
            Priority = priority;
        }

        public abstract string Apply();

        public TextSpan GetCurrentOwner()
        {
            return Owner;
        }

        public void ChangeOwner(TextSpan newOwner)
        {
            Owner = newOwner;
        }

        public int Priority { get; set; }

        // Helpers
        protected static string EscapeAttr(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        protected static void Wrap(ref string text, string openTag, string closeName)
        {
            var _sb = new StringBuilder(text.Length + openTag.Length + closeName.Length + 5);
            _sb.Append('<').Append(openTag).Append('>').Append(text).Append("</").Append(closeName).Append('>');
            text = _sb.ToString();
        }

        // Default rich-text CompStyles
        public sealed class RichTextBold : CompStyle
        {
            public RichTextBold(int priority = PRIORITY_EFFECT) : base(priority)
            {
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, "b", "b");
                Owner.Text = _t;
                return _t;
            }
        }

        public sealed class RichTextItalic : CompStyle
        {
            public RichTextItalic(int priority = PRIORITY_EFFECT) : base(priority)
            {
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, "i", "i");
                Owner.Text = _t;
                return _t;
            }
        }

        public sealed class RichTextUnderline : CompStyle
        {
            public RichTextUnderline(int priority = PRIORITY_EFFECT) : base(priority)
            {
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, "u", "u");
                Owner.Text = _t;
                return _t;
            }
        }

        public sealed class RichTextStrikethrough : CompStyle
        {
            public RichTextStrikethrough(int priority = PRIORITY_EFFECT) : base(priority)
            {
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, "s", "s");
                Owner.Text = _t;
                return _t;
            }
        }

        public sealed class RichTextColorHex : CompStyle
        {
            public readonly string Hex; // e.g. FF0000 or FF0000FF

            public RichTextColorHex(string hex, int priority = PRIORITY_COLOR) : base(priority)
            {
                Hex = NormalizeHex(hex);
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, $"color=#{Hex}", "color");
                Owner.Text = _t;
                return _t;
            }

            public static string NormalizeHex(string hex)
            {
                if (string.IsNullOrEmpty(hex)) return "FFFFFF";
                if (hex[0] == '#') hex = hex.Substring(1);
                hex = hex.ToUpperInvariant();
                if (hex.Length == 3) // short rgb -> rrggbb
                    hex = new string(new[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });

                if (hex.Length is 6 or 8) return hex;
                // best-effort fallback
                return hex.Length < 6 ? hex.PadRight(6, '0') : hex.Substring(0, 8);
            }
        }

        public sealed class RichTextSize : CompStyle
        {
            public readonly float Value;

            public RichTextSize(float size, int priority = PRIORITY_SIZE) : base(priority)
            {
                Value = size;
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                // Unity rich text: <size=VALUE>
                Wrap(ref _t, $"size={Value.ToString(CultureInfo.InvariantCulture)}", "size");
                Owner.Text = _t;
                return _t;
            }
        }

        // TMP link support: <link=ID>...</link>
        public sealed class RichTextLink : CompStyle
        {
            public readonly string Target;

            public RichTextLink(string target, int priority = PRIORITY_LINK) : base(priority)
            {
                Target = target ?? string.Empty;
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, $"link={EscapeAttr(Target)}", "link");
                Owner.Text = _t;
                return _t;
            }
        }

        // TMP font tag: <font=NAME>...</font>
        public sealed class RichTextFont : CompStyle
        {
            public readonly string Name;

            public RichTextFont(string name, int priority = PRIORITY_FONT) : base(priority)
            {
                Name = name ?? string.Empty;
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, $"font={EscapeAttr(Name)}", "font");
                Owner.Text = _t;
                return _t;
            }
        }

        // TMP smallcaps: <smallcaps>...</smallcaps>
        public sealed class RichTextSmallCaps : CompStyle
        {
            public RichTextSmallCaps(int priority = PRIORITY_EFFECT) : base(priority)
            {
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, "smallcaps", "smallcaps");
                Owner.Text = _t;
                return _t;
            }
        }

        public sealed class RichTextSubscript : CompStyle
        {
            public RichTextSubscript(int priority = PRIORITY_SUB_SUP) : base(priority)
            {
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, "sub", "sub");
                Owner.Text = _t;
                return _t;
            }
        }

        public sealed class RichTextSuperscript : CompStyle
        {
            public RichTextSuperscript(int priority = PRIORITY_SUB_SUP) : base(priority)
            {
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, "sup", "sup");
                Owner.Text = _t;
                return _t;
            }
        }

        // TMP mark background color: <mark=#RRGGBB>...</mark>
        public sealed class RichTextMark : CompStyle
        {
            public readonly string Hex;

            public RichTextMark(string hex, int priority = PRIORITY_MARK) : base(priority)
            {
                Hex = RichTextColorHex.NormalizeHex(hex);
            }

            public override string Apply()
            {
                var _t = Owner.Text;
                Wrap(ref _t, $"mark=#{Hex}", "mark");
                Owner.Text = _t;
                return _t;
            }
        }

        // Convenience factories
    }


    public struct TextSpan : IOwnerOf<IOwnedBy<TextSpan>>, IEquatable<TextSpan>, IMessage
    {
        public TextSpan(string text, ISet<IOwnedBy<TextSpan>> pets = default, string separator = " ")
        {
            Text = text;
            Children = new();
            if (pets != null)
                Children = pets.ToList();
            Separator = separator;


            foreach (var _child in Children)
            {
                _child.ChangeOwner(this);
            }
        }

        public string Text { get; set; }
        public string Separator { get; set; }

        public string Eval()
        {
            if (Children == null)
                return Text;
            var _l = Children.OfType<CompStyle>().ToList();
            _l.Sort();
            foreach (var _ownable in _l)
            {
                _ownable.ChangeOwner(this);
                Text = _ownable.Apply();
            }

            return Text;
        }

        public string Print()
        {
            return Eval();
        }

        public override string ToString()
        {
            return Print();
        }

        public bool Equals(TextSpan other)
        {
            return Text == other.Text && Separator == other.Separator && Equals(Children, other.Children);
        }

        public override bool Equals(object obj)
        {
            return obj is TextSpan _other && Equals(_other);
        }

        public static bool operator ==(TextSpan a, TextSpan b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(TextSpan a, TextSpan b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text, Separator, Children);
        }

        public TextSpan AddPass(CompStyle style)
        {
            if (style == null)
                return this;
            Children?.Add(style);
            return this;
        }

        public TextSpan RemovePass(CompStyle style)
        {
            if (style == null)
                return this;
            Children?.Remove(style);
            return this;
        }

        public TextSpan B_RT()
        {
            return AddPass(new CompStyle.RichTextBold());
        }

        public TextSpan I_RT()
        {
            return AddPass(new CompStyle.RichTextItalic());
        }

        public TextSpan U_RT()
        {
            return AddPass(new CompStyle.RichTextUnderline());
        }

        public TextSpan S_RT()
        {
            return AddPass(new CompStyle.RichTextStrikethrough());
        }

        public TextSpan Color_RT(string hex)
        {
            return AddPass(new CompStyle.RichTextColorHex(hex));
        }

        public TextSpan Sz_RT(float v)
        {
            return AddPass(new CompStyle.RichTextSize(v));
        }

        public TextSpan Lnk_RT(string target)
        {
            return AddPass(new CompStyle.RichTextLink(target));
        }

        public TextSpan Fnt_RT(string name)
        {
            return AddPass(new CompStyle.RichTextFont(name));
        }

        public TextSpan Sc_RT()
        {
            return AddPass(new CompStyle.RichTextSmallCaps());
        }

        public TextSpan Sub_RT()
        {
            return AddPass(new CompStyle.RichTextSubscript());
        }

        public TextSpan Sup_RT()
        {
            return AddPass(new CompStyle.RichTextSuperscript());
        }

        public TextSpan Mk_RT(string hex)
        {
            return AddPass(new CompStyle.RichTextMark(hex));
        }

        private List<IOwnedBy<TextSpan>> Children { get; }
        public IEnumerable<IOwnedBy<TextSpan>> Pets => Children;

        public void AddPet(IOwnedBy<TextSpan> pet)
        {
            if (pet == null) return;
            if (Children.Contains(pet)) return;
            Children.Add(pet);
        }

        public bool RemovePet(IOwnedBy<TextSpan> pet)
        {
            if (pet == null) return false;
            if (!Children.Contains(pet)) return false;
            Children.Remove(pet);
            return true;
        }
    }
}
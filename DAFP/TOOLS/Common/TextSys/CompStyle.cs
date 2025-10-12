using System.Linq;

namespace DAFP.TOOLS.Common.TextSys
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public abstract class CompStyle : IOwnable<Span>, IPriority<CompStyle>
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
        protected Span Owner;

        // Suggested default priorities (lower runs earlier -> outer tags)
        protected const int Priority_Link = 10;
        protected const int Priority_Font = 20;
        protected const int Priority_Size = 30;
        protected const int Priority_Color = 40;
        protected const int Priority_Effect = 50; // b, i, u, s, smallcaps
        protected const int Priority_SubSup = 60;
        protected const int Priority_Mark = 70;

        protected CompStyle(int priority)
        {
            Priority = priority;
        }

        public abstract string Apply();

        public Span GetCurrentOwner()
        {
            return Owner;
        }

        public void ChangeOwner(Span newOwner)
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
            var sb = new StringBuilder(text.Length + openTag.Length + closeName.Length + 5);
            sb.Append('<').Append(openTag).Append('>').Append(text).Append("</").Append(closeName).Append('>');
            text = sb.ToString();
        }

        // Default rich-text CompStyles
        public sealed class RichTextBold : CompStyle
        {
            public RichTextBold(int priority = Priority_Effect) : base(priority)
            {
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, "b", "b");
                Owner.Text = t;
                return t;
            }
        }

        public sealed class RichTextItalic : CompStyle
        {
            public RichTextItalic(int priority = Priority_Effect) : base(priority)
            {
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, "i", "i");
                Owner.Text = t;
                return t;
            }
        }

        public sealed class RichTextUnderline : CompStyle
        {
            public RichTextUnderline(int priority = Priority_Effect) : base(priority)
            {
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, "u", "u");
                Owner.Text = t;
                return t;
            }
        }

        public sealed class RichTextStrikethrough : CompStyle
        {
            public RichTextStrikethrough(int priority = Priority_Effect) : base(priority)
            {
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, "s", "s");
                Owner.Text = t;
                return t;
            }
        }

        public sealed class RichTextColorHex : CompStyle
        {
            public readonly string Hex; // e.g. FF0000 or FF0000FF

            public RichTextColorHex(string hex, int priority = Priority_Color) : base(priority)
            {
                Hex = NormalizeHex(hex);
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, $"color=#{Hex}", "color");
                Owner.Text = t;
                return t;
            }

            public static string NormalizeHex(string hex)
            {
                if (string.IsNullOrEmpty(hex)) return "FFFFFF";
                if (hex[0] == '#') hex = hex.Substring(1);
                hex = hex.ToUpperInvariant();
                if (hex.Length == 3) // short rgb -> rrggbb
                {
                    hex = new string(new[] { hex[0], hex[0], hex[1], hex[1], hex[2], hex[2] });
                }

                if (hex.Length is 6 or 8) return hex;
                // best-effort fallback
                return hex.Length < 6 ? hex.PadRight(6, '0') : hex.Substring(0, 8);
            }
        }

        public sealed class RichTextSize : CompStyle
        {
            public readonly float Value;

            public RichTextSize(float size, int priority = Priority_Size) : base(priority)
            {
                Value = size;
            }

            public override string Apply()
            {
                string t = Owner.Text;
                // Unity rich text: <size=VALUE>
                Wrap(ref t, $"size={Value.ToString(CultureInfo.InvariantCulture)}", "size");
                Owner.Text = t;
                return t;
            }
        }

        // TMP link support: <link=ID>...</link>
        public sealed class RichTextLink : CompStyle
        {
            public readonly string Target;

            public RichTextLink(string target, int priority = Priority_Link) : base(priority)
            {
                Target = target ?? string.Empty;
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, $"link={EscapeAttr(Target)}", "link");
                Owner.Text = t;
                return t;
            }
        }

        // TMP font tag: <font=NAME>...</font>
        public sealed class RichTextFont : CompStyle
        {
            public readonly string Name;

            public RichTextFont(string name, int priority = Priority_Font) : base(priority)
            {
                Name = name ?? string.Empty;
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, $"font={EscapeAttr(Name)}", "font");
                Owner.Text = t;
                return t;
            }
        }

        // TMP smallcaps: <smallcaps>...</smallcaps>
        public sealed class RichTextSmallCaps : CompStyle
        {
            public RichTextSmallCaps(int priority = Priority_Effect) : base(priority)
            {
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, "smallcaps", "smallcaps");
                Owner.Text = t;
                return t;
            }
        }

        public sealed class RichTextSubscript : CompStyle
        {
            public RichTextSubscript(int priority = Priority_SubSup) : base(priority)
            {
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, "sub", "sub");
                Owner.Text = t;
                return t;
            }
        }

        public sealed class RichTextSuperscript : CompStyle
        {
            public RichTextSuperscript(int priority = Priority_SubSup) : base(priority)
            {
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, "sup", "sup");
                Owner.Text = t;
                return t;
            }
        }

        // TMP mark background color: <mark=#RRGGBB>...</mark>
        public sealed class RichTextMark : CompStyle
        {
            public readonly string Hex;

            public RichTextMark(string hex, int priority = Priority_Mark) : base(priority)
            {
                Hex = RichTextColorHex.NormalizeHex(hex);
            }

            public override string Apply()
            {
                string t = Owner.Text;
                Wrap(ref t, $"mark=#{Hex}", "mark");
                Owner.Text = t;
                return t;
            }
        }

        // Convenience factories
    }


    public struct Span : IOwner<Span>, IEquatable<Span>
    {
        public Span(string text, ISet<IOwnable<Span>> pets = default, string separator = " ")
        {
            Text = text;
            Pets = pets;
            Separator = separator;
        }

        public string Text { get; set; }
        public string Separator { get; set; }

        public string Eval()
        {
            if (Pets == null)
                return Text;
            var l = Pets.OfType<CompStyle>().ToList();
            l.Sort();
            foreach (var _ownable in l)
            {
                _ownable.ChangeOwner(this);
                Text = _ownable.Apply();
            }

            return Text;
        }

        public ISet<IOwnable<Span>> Pets { get; }

        public bool Equals(Span other)
        {
            return Text == other.Text && Separator == other.Separator && Equals(Pets, other.Pets);
        }

        public override bool Equals(object obj)
        {
            return obj is Span other && Equals(other);
        }

        public static bool operator ==(Span a, Span b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Span a, Span b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Text, Separator, Pets);
        }

        public Span AddPass(CompStyle style)
        {
            if (style == null)
                return this;
            Pets?.Add(style);
            return this;
        }

        public Span RemovePass(CompStyle style)
        {
            if (style == null)
                return this;
            Pets?.Remove(style);
            return this;
        }

        public Span B_RT() => AddPass(new CompStyle.RichTextBold());
        public Span I_RT() => AddPass(new CompStyle.RichTextItalic());
        public Span U_RT() => AddPass(new CompStyle.RichTextUnderline());
        public Span S_RT() => AddPass(new CompStyle.RichTextStrikethrough());
        public Span Color_RT(string hex) => AddPass(new CompStyle.RichTextColorHex(hex));
        public Span Sz_RT(float v) => AddPass(new CompStyle.RichTextSize(v));
        public Span Lnk_RT(string target) => AddPass(new CompStyle.RichTextLink(target));
        public Span Fnt_RT(string name) => AddPass(new CompStyle.RichTextFont(name));
        public Span Sc_RT() => AddPass(new CompStyle.RichTextSmallCaps());
        public Span Sub_RT() => AddPass(new CompStyle.RichTextSubscript());
        public Span Sup_RT() => AddPass(new CompStyle.RichTextSuperscript());
        public Span Mk_RT(string hex) => AddPass(new CompStyle.RichTextMark(hex));
    }
}
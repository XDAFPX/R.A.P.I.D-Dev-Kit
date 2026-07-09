using System;
using System.Collections.Generic;
using System.Linq;

namespace DAFP.TOOLS.Common.TextSys
{
    public record CompText : IMessage
    {
        private readonly List<TextSpan> _stack = new();


        public CompText(TextSpan root)
        {
            _stack.Add(root);
        }

        public CompText(string text, params CompStyle[] styles)
        {
            _stack.Add(new TextSpan(text, styles.Cast<IOwnedBy<TextSpan>>().ToHashSet()));
        }

        public CompText Add(TextSpan textSpan)
        {
            _stack.Add(textSpan);
            return this;
        }

        public CompText Remove(TextSpan textSpan)
        {
            _stack.Remove(textSpan);
            return this;
        }

        public CompText Remove(string span)
        {
            var f = _stack.FirstOrDefault(span1 => span1.Text == span);
            if (f != default) _stack.Remove(f);

            return this;
        }

        public string Build()
        {
            return string.Join(
                "",
                _stack.Select((item, i) => i < _stack.Count - 1
                    ? item.Eval() + item.Separator
                    : item.Eval())
            );
        }

        public static CompText Literal(string txt)
        {
            return new CompText(txt);
        }

        public string Print()
        {
            return Build();
        }
    }
}
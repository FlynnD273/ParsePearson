using System;
using System.Text.RegularExpressions;

namespace ParseMastering.Parsers
{
    public static class ExpressionParser
    {
        private static Regex _textOpRegex = new Regex(@"^[0-9+\-*.\(\)a-zA-Z]+$", RegexOptions.Compiled);
        private static Regex _symbolOpRegex = new Regex(@"^&[a-zA-Z]+;$", RegexOptions.Compiled);
        //@DIV{(68*4-67*2.5);68+67}@Sup{2}&pi;

        /* 
         * start => paren | at | sym | text | series
         * paren => (start)
         * series => startstart
         * at => DIV | Sup | rt
         * sym => &[a-zA-Z]+;
         * DIV => @DIV{start;start}
         * Sup => @Sup{start}
         * rt => @RT{start}
         * text => ^[0-9+-*.()]+$
         */

        public static Operation Parse(string text)
        {
            text = text.Replace(" ", "");
            var op = ParseStart(text);
            op?.Flatten();
            return op;
        }

        private static (Operation, Operation) ParseHelper(string text, Func<string, Operation> A, Func<string, Operation> B)
        {
            int lBracket = 0, rBracket = 0, lParen = 0, rParen = 0;
            bool trackBracket = false;
            int startIndex = 1;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                switch (c)
                {
                    case '{':
                        if (i == 0) trackBracket = true;
                        lBracket++;
                        break;

                    case '}':
                        rBracket++;
                        if (trackBracket && rBracket == lBracket)
                        {
                            startIndex = i;
                            trackBracket = false;
                        }
                        break;

                    case '(':
                        lParen++;
                        break;

                    case ')':
                        rParen++;
                        break;

                    default:
                        break;
                }
                if (rBracket > lBracket || rParen > lParen) return (null, null);
            }
            if (lBracket != rBracket || lParen != rParen) return (null, null);

            for (int i = startIndex; i < text.Length - 1; i++)
            {
                Operation aOp = A.Invoke(text.Substring(0, i));
                if (aOp == null) continue;
                Operation bOp = B.Invoke(text.Substring(i));
                if (bOp == null) continue;

                return (aOp, bOp);
            }

            return (null, null);
        }

        private static (Operation, Operation) ParseHelper(string text, char separator, Func<string, Operation> A, Func<string, Operation> B)
        {
            for (int i = 1; i < text.Length - 1; i++)
            {
                if (text[i] != separator) continue;
                Operation aOp = A.Invoke(text.Substring(0, i));
                if (aOp == null) continue;
                Operation bOp = B.Invoke(text.Substring(i + 1));
                if (bOp == null) continue;

                return (aOp, bOp);
            }

            return (null, null);
        }

        // start => at | sym | text | series
        private static Operation ParseStart(string text)
        {
            var par = ParseParen(text);
            if (par != null)
            {
                return par;
            }

            var p = ParseAt(text);
            if (p != null)
            {
                return p;
            }

            var s = ParseSymbol(text);
            if (s != null)
            {
                return s;
            }

            var t = ParseText(text);
            if (t != null)
            {
                return t;
            }

            var subExp = ParseHelper(text, ParseStart, ParseStart);
            if (subExp.Item1 != null)
            {
                return new SeriesOperation(subExp.Item1, subExp.Item2);
            }

            return null;
        }

        // paren => (start)
        private static Operation ParseParen(string text)
        {
            if (!text.StartsWith("(") || !text.EndsWith(")")) return null;

            var subExp = ParseStart(text.Substring(1, text.Length - 1 - 1));
            if (subExp != null)
            {
                return new ParenOperation(subExp);
            }

            return null;
        }

        // at => DIV | Sup
        private static Operation ParseAt(string text)
        {
            if (string.IsNullOrEmpty(text) || text[0] != '@') return null;

            var d = ParseDiv(text);
            if (d != null)
            {
                return d;
            }

            var b = ParseSup(text);
            if (b != null)
            {
                return b;
            }

            var r = ParseRoot(text);
            if (r != null)
            {
                return r;
            }

            return null;
        }

        // sym => &[a-zA-Z];
        private static Operation ParseSymbol(string text)
        {
            if (!text.StartsWith("&") || !text.EndsWith(";")) return null;

            if (_symbolOpRegex.IsMatch(text))
            {
                return new SymbolOperation(text.Substring(1, text.Length - 1 - 1));
            }

            return null;
        }

        // DIV => @DIV{start;start}
        private static Operation ParseDiv(string text)
        {
            if (!text.StartsWith("@DIV{") || !text.EndsWith("}")) return null;

            var subExp = ParseHelper(text.Substring(5, text.Length - 5 - 1), ';', ParseStart, ParseStart);
            if (subExp.Item1 != null)
            {
                return new DivOperation(subExp.Item1, subExp.Item2);
            }

            return null;
        }

        // Sup => @Sup{start}
        private static Operation ParseSup(string text)
        {
            if (!text.StartsWith("@Sup{") || !text.EndsWith("}")) return null;

            var subExp = ParseStart(text.Substring(5, text.Length - 5 - 1));
            if (subExp != null)
            {
                return new SupOperation(subExp);
            }

            return null;
        }

        // rt => @RT{start}
        private static Operation ParseRoot(string text)
        {
            if (!text.StartsWith("@RT{") || !text.EndsWith("}")) return null;

            var subExp = ParseStart(text.Substring(4, text.Length - 4 - 1));
            if (subExp != null)
            {
                return new RootOperation(subExp);
            }

            return null;
        }

        // text => ^[0-9+-*.()]+$
        private static Operation ParseText(string text)
        {
            if (_textOpRegex.IsMatch(text))
            {
                return new TextOperation(text);
            }

            return null;
        }
    }
}
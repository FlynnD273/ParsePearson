using System.Collections.Generic;
using System.Linq;

namespace ParseMastering.Parsers
{
    public abstract class Operation
    {
        public abstract void Flatten();
    }

    public class SeriesOperation : Operation
    {
        public List<Operation> Children { get; private set; }

        public SeriesOperation() : base()
        { }

        public SeriesOperation(params Operation[] operations)
        {
            Children = new(operations);
        }

        public override void Flatten()
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];

                child.Flatten();

                if (child is SeriesOperation s)
                {
                    Children.RemoveAt(i);

                    foreach (Operation o in s.Children)
                    {
                        Children.Insert(i++, o);
                    }
                }
            }
        }
        public override string ToString() => $"{string.Join("", Children)}";
    }

    public class DivOperation : Operation
    {
        public Operation Numerator { get; set; }
        public Operation Denominator { get; set; }

        public DivOperation() : base()
        { }

        public DivOperation(Operation num, Operation den)
        {
            Numerator = num;
            Denominator = den;
        }

        public override void Flatten()
        {
            Numerator.Flatten();
            Denominator.Flatten();
        }

        public override string ToString() => $"({Numerator})/({Denominator})";
    }

    public class RootOperation : Operation
    {
        public Operation Argument { get; set; }

        public RootOperation() : base()
        { }

        public RootOperation(Operation arg)
        {
            Argument = arg;
        }

        public override void Flatten()
        {
            Argument.Flatten();
        }

        public override string ToString() => $"sqrt({Argument})";
    }

    public class ParenOperation : Operation
    {
        public Operation Argument { get; set; }

        public ParenOperation() : base()
        { }

        public ParenOperation(Operation arg)
        {
            Argument = arg;
        }

        public override void Flatten()
        {
            Argument.Flatten();
        }

        public override string ToString() => $"({Argument})";
    }

    public class SupOperation : Operation
    {
        public Operation Exponent { get; set; }

        public SupOperation() : base()
        { }

        public SupOperation(Operation exp)
        {
            Exponent = exp;
        }

        public override void Flatten()
        {
            Exponent.Flatten();
        }

        public override string ToString() => $"^{Exponent}";
    }

    public class SymbolOperation : Operation
    {
        public string Symbol { get; set; }

        public SymbolOperation() : base()
        { }

        public SymbolOperation(string symbol)
        {
            Symbol = symbol;
        }

        public override void Flatten() { }

        public override string ToString() => Symbol;
    }

    public class TextOperation : Operation
    {
        public string Text { get; set; }
        public List<string> Operands { get; set; }

        public TextOperation(string text)
        {
            Text = text;
        }

        public override string ToString() => Text;

        public override void Flatten()
        { }
    }
}
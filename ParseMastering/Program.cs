using org.mariuszgromada.math.mxparser;
using ParseMastering.Parsers;
using System.Windows.Shapes;

internal class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        mXparser.setDegreesMode();
        while (true)
        {
            Console.Write("Enter expression: ");
            string line = Console.ReadLine();
            string? parsed = null;

            Expression e = new(line);
            double val = e.calculate();

            if (double.IsNaN(val))
            {
                parsed = ParseEquation(line);

                if (parsed == null)
                {
                    Console.WriteLine("Failed to parse.");
                }
                else
                {
                    Console.WriteLine(parsed);
                    e = new(parsed);
                    val = e.calculate();
                    Console.WriteLine(val);
                    Clipboard.SetText(val.ToString().Replace("E", "*10^"));
                }
            }
            else
            {
                Console.WriteLine("Standard expression detected.");
                Console.WriteLine(val);
                Clipboard.SetText(val.ToString().Replace("E", "*10^"));
            }
        }


        string? ParseEquation(string line)
        {
            Operation op = ExpressionParser.Parse(line);
            if (op == null) return null;

            return op.ToString();
        }
    }
}
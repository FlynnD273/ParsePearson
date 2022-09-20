using ParseMastering.Parsers;

while (true)
{
    Console.Write("Enter equation: ");
    string line = Console.ReadLine();
    string parsed = "";

    parsed = ParseEquation(line);

    Console.WriteLine(parsed);
}


string ParseEquation(string line)
{
    Operation op = ExpressionParser.Parse(line);
    if (op == null) return "Failed to parse";

    return op.ToString();
}
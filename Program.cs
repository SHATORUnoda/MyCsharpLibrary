using System.Text;
using System.Text.RegularExpressions;

if (args.Length != 3)
{
    Console.WriteLine(
        "usage: Bundler <input.cs> <submit.cs> <libdir>");
    return;
}

string entry = Path.GetFullPath(args[0]);
string output = Path.GetFullPath(args[1]);
string libDir = Path.GetFullPath(args[2]);

if (!Directory.Exists(libDir))
{
    Console.WriteLine($"lib not found : {libDir}");
    return;
}

Dictionary<string, string> libs =
    new(StringComparer.OrdinalIgnoreCase);

foreach (var f in Directory.GetFiles(
             libDir,
             "*.cs"))
{
    libs[
        Path.GetFileNameWithoutExtension(f)
    ] = f;
}

HashSet<string> used = new();

List<string> order = new();

void Expand(string name)
{
    if (used.Contains(name))
        return;

    if (!libs.ContainsKey(name))
    {
        Console.WriteLine(
            $"library not found : {name}");
        return;
    }

    used.Add(name);

    string src =
        File.ReadAllText(
            libs[name]);

    Match m =
        Regex.Match(
            src,
            @"//\s*deps:\s*(.*)");

    if (m.Success)
    {
        foreach (var dep in
                 m.Groups[1].Value.Split(
                     ',',
                     StringSplitOptions.RemoveEmptyEntries))
        {
            Expand(dep.Trim());
        }
    }

    order.Add(name);
}

string program =
    File.ReadAllText(entry);

foreach (Match m in Regex.Matches(
             program,
             @"//\s*require:\s*(\w+)"))
{
    Expand(
        m.Groups[1].Value);
}

HashSet<string> usings = new();

void AddStaticUsing(string name)
{
    if (!libs.TryGetValue(name, out var path))
        return;

    string src = File.ReadAllText(path);

    Match nsMatch = Regex.Match(
        src,
        @"namespace\s+([A-Za-z0-9_.]+)");

    Match classMatch = Regex.Match(
        src,
        @"(?:public\s+)?static\s+class\s+([A-Za-z0-9_]+)");

    if (!nsMatch.Success || !classMatch.Success)
        return;

    usings.Add(
        $"using static {nsMatch.Groups[1].Value}.{classMatch.Groups[1].Value};");
}

StringBuilder body = new();

void AppendCode(string code)
{
    foreach (string line in code.Split('\n'))
    {
        string s = line.Trim();

        if (s.StartsWith("using "))
        {
            usings.Add(s);
            continue;
        }

        if (s.StartsWith("#nullable"))
            continue;

        body.AppendLine(line);
    }
}

foreach (var name in order)
{
    body.AppendLine(
        $"// ===== {name} =====");

    AddStaticUsing(name);

    AppendCode(
        File.ReadAllText(
            libs[name]));

    body.AppendLine();
}

body.AppendLine(
    "// ===== Main =====");

AppendCode(program);

StringBuilder sb = new();

sb.AppendLine("#nullable disable");
sb.AppendLine();

//
// using / using static を自動追加
//

foreach (var u in usings.OrderBy(x => x))
{
    sb.AppendLine(u);
}

sb.AppendLine();

sb.Append(body);

File.WriteAllText(
    output,
    sb.ToString());

Console.WriteLine(
    $"generated : {output}");

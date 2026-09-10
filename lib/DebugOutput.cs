using System;
using System.Text;

public class DebugOutput : TextWriter
{
    public readonly TextWriter console;
    public readonly TextWriter file;

    public DebugOutput(TextWriter console, TextWriter file)
    {
        this.console = console;
        this.file = file;
    }

    public override Encoding Encoding => console.Encoding;

    public override void Write(char value)
    {
        console.Write(value);
        file.Write(value);
    }

    public override void Write(string value)
    {
        console.Write(value);
        file.Write(value);
    }

    public override void Flush()
    {
        console.Flush();
        file.Flush();
    }

    public static void Init()
    {
        var file = new StreamWriter("../out.txt", false)
        {
            AutoFlush = true
        };

        Console.SetOut(
            new DebugOutput(Console.Out, file)
        );
    }
}
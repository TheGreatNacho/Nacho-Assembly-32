using System;

namespace na32
{
    class Program
    {
        static void Main(string[] args)
        {
            // Command line requires 1 argument, program location. test_program1.na works for a hello world example
            if (args.Length == 0)
            {
                Console.Write("File Location: ");
                args = new string[1];
                args[0] = Console.ReadLine();
            }
            // Program Location
            string programLocation = args[0];
            // Create a new lexer, reading the file location
            Na32Lexer lexer = new Na32Lexer(programLocation);

            DateTime curTime = DateTime.Now;
            // Execute the program and get the result
            byte result = lexer.Execute();
            TimeSpan diffTime = DateTime.Now - curTime;
            // Close the lexer
            lexer.Close();

            // Print the result to console
            Console.WriteLine($"Program executed in {diffTime.TotalSeconds}s with result: {result}");

        }
    }
}

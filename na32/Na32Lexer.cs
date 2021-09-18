using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace na32
{
    public class Na32Lexer
    {
        private readonly Dictionary<string, Operation> _opsString = new Dictionary<string, Operation>();
        private readonly Dictionary<byte, Operation> _opsID = new Dictionary<byte, Operation>();
        public List<ProgramFunction> Program;
        private byte _flags = 0;
        /* _flag Structure
         *      1. ADDR
         *      2. MEM
         */

        // Registers
        private int registerA;
        private int registerB;
        private int[] memory;
        private Stack stack;
        private int programPointer;


        // Config
        public int MemorySize = 65535;
        public int StackSize = 20;


        public Na32Lexer()
        {
            LoadOperations();
        }
        public Na32Lexer(string fileLocation)
        {
            LoadOperations();
            try
            {
                using (FileStream _stream = new FileStream(fileLocation, FileMode.Open))
                using (BinaryReader _reader = new BinaryReader(_stream))
                    Parse(_reader.ReadBytes((int)_stream.Length));
            }
            catch
            {
                Console.WriteLine($"File location \"{fileLocation}\" does not exist.");
            }
        }


        // Flags
        public bool MemFlag()
        {
            return (_flags & 2) == 2;
        }
        public void MemFlag(bool value)
        {
            if (value)
                _flags = (byte)(_flags | 0x02); // Add the Mem flag
            else
                _flags = (byte)(_flags & 0xFD); // Remove the Mem flag
        }
        public bool RegFlag()
        {
            return (_flags & 1) == 1;
        }
        public void RegFlag(bool value)
        {
            if (value)
                _flags = (byte)(_flags | 0x01); // Add the Register flag
            else
                _flags = (byte)(_flags & 0xFE); // Remove the Register flag
        }
        // Operations
        private void AddOperation(string name, byte id, Action<int> action, int argument = 0)
        {
            Operation op = new Operation(name, id, argument, action);
            _opsString[name] = op;
            _opsID[id] = op;
        }
        private void LoadOperations()
        {
            AddOperation("NOOP", 0x00, a => { programPointer++; });     // No Operation
            // Math/Logic
            AddOperation("ADD", 0x01, a => { registerA += GetValue(a); programPointer++; }, 1);   // Add the following argument to the A register
            AddOperation("SUB", 0x02, a => { registerA -= GetValue(a); programPointer++; }, 1);   // Subtract the following argument from the A register
            AddOperation("MUL", 0x03, a => { registerA *= GetValue(a); programPointer++; }, 1);   // Multipy the following argument with the A register
            AddOperation("DIV", 0x04, a => { registerA /= GetValue(a); programPointer++; }, 1);   // Divide the A register with the following argument
            AddOperation("INC", 0x05, a => { registerA++; programPointer++; });      // Increment the A register
            AddOperation("DEC", 0x06, a => { registerA--; programPointer++; });      // Decrement the B register
            AddOperation("AND", 0x07, a => { registerA &= GetValue(a); programPointer++; }, 1);    // Logical AND Operator
            AddOperation("OR", 0x08, a => { registerA |= GetValue(a); programPointer++; }, 1);    // Logical OR Operator
            AddOperation("BSL", 0x09, a => { registerA <<= GetValue(a); programPointer++; }, 1);    // Bit Shift the A register left by the following argument
            AddOperation("BSR", 0x10, a => { registerA >>= GetValue(a); programPointer++; }, 1);    // Bit Shift the A register right by the following argument
            // Register Manipulation
            AddOperation("LDA", 0x30, a => { registerA = GetValue(a); programPointer++; }, 1);   // Load the following argument to the A register
            AddOperation("LDB", 0x31, a => { registerB = GetValue(a); programPointer++; }, 1);   // Load the following argument to the B register
            AddOperation("MVA", 0x32, a => { memory[GetValue(a)] = registerA; programPointer++; }, 1);   // Set's the memory at the argument location equal to the A register
            AddOperation("MVB", 0x33, a => { memory[GetValue(a)] = registerB; programPointer++; }, 1);   // Set's the memory at the argument location equal to the B register
            // Program Movement
            AddOperation("JMP", 0x40, a => { programPointer = GetValue(a); }, 1);   // Jump to the location specified in the following argument
            AddOperation("JEZ", 0x41, a => { if (registerA == 0) programPointer = GetValue(a); else programPointer++; }, 1);   // Jump to the location specified in the following argument if the A register is equal to zero
            AddOperation("JNZ", 0x42, a => { if (registerA != 0) programPointer = GetValue(a); else programPointer++; }, 1);   // Jump to the location specified in the following argument if the A register is not equal to zero
            AddOperation("JEQ", 0x43, a => { if (registerA == registerB) programPointer = GetValue(a); else programPointer++; }, 1);   // Jump to the location specified in the following argument if the A register equals the B register
            AddOperation("CALL", 0x44, a => { stack.Push(programPointer); programPointer = GetValue(a); }, 1); // Add's the current program pointer to the stack and moves the program pointer to the following argument
            AddOperation("RET", 0x45, a => { programPointer = (int)stack.Pop() + 1; });      // Move's the program pointer to the last pointer on the stack
            // Output/Flags
            AddOperation("OUT", 0xF0, a => { Console.Write((char)GetValue(a)); programPointer++; }, 1);   // Outputs the following argument to console 
            AddOperation("OUTI", 0xF1, a => { Console.WriteLine(GetValue(a)); programPointer++; }, 1); // Outputs the following argument as an integer to console 
            AddOperation("MEM", 0xF2, a => { MemFlag(true); programPointer++; });      // Sets the MEM flag
            AddOperation("REG", 0xF3, a => { RegFlag(true); programPointer++; });      // Sets the REG flag
        }
        // End Operations
        public override string ToString()
        {
            return ParseToString();
        }
        public string ParseToString()
        {
            string programString = "";
            bool memFlag = false;
            bool regFlag = false;
            for (programPointer = 0; programPointer < Program.Count; programPointer++)
            {
                ProgramFunction function = Program[programPointer];
                if (function.Instruction.Equals(_opsString["MEM"]))
                {
                    memFlag = true;
                    continue;
                }
                if (function.Instruction.Equals(_opsString["REG"]))
                {
                    regFlag = true;
                    continue;
                }
                string instruction = function.Instruction.Instruction.ToString();
                string argument = function.Argument0.ToString();

                if (regFlag)
                {
                    argument = ((Na32Lexer.Register)function.Argument0).ToString();
                    regFlag = false;
                }
                if (memFlag)
                {
                    argument = $"#{argument}";
                    memFlag = false;
                }
                programString += $"{instruction}";
                if (function.Instruction.Arguments > 0)
                {
                    programString += $" {argument}";
                }
                programString += "\r\n";
            }
            return programString;
        }

        public void Parse(string program)
        {
            this.Program = new List<ProgramFunction>();
            this.Reset();
            // Remove newlines and spaces and replace them with a single space, for seperation
            program = Regex.Replace(program.ToUpper(), @"\s+", " ");
            // Seperate the string into a string array, seperated by spacebars
            string[] splitString = program.Split(' ');
            Dictionary<string, int> labelPointers = new Dictionary<string, int>();
            Dictionary<string, int> unresolved = new Dictionary<string, int>();

            // We set the key outside the try and for loop, so we can catch the data for error handling
            string key = "";
            try
            {
                for (int i = 0; i < splitString.Length;)
                {
                    // The key is the key to the _opsstring. We increment i in the assignment, as we're running multiple searches in one loop
                    // The key might also be a label, like :test
                    key = splitString[i++];
                    if (key.StartsWith(":"))
                    {
                        // Set a labelPointer equal to the position it would occupy in the program
                        string labelName = key.Substring(1);
                        labelPointers[labelName] = this.Program.Count;
                        // If this label would resolve some previous conflicts, resolve them and add them to the program
                        if (unresolved.ContainsKey(labelName))
                        {
                            this.Program[unresolved[labelName]].Argument0 = this.Program.Count;
                            unresolved.Remove(labelName);
                        }
                        continue;
                    }
                    Operation operation = _opsString[key];
                    int argument = 0;
                    if (operation.Arguments > 0)
                    {
                        string strArg = splitString[i++];
                        // If the argument is in the label dictionary, convert the argument to the program pointer
                        if (labelPointers.ContainsKey(strArg))
                        {
                            strArg = labelPointers[strArg].ToString();
                        }
                        // Convert argument from #argument to memory[argument]
                        if (strArg.StartsWith("#"))
                        {
                            this.Program.Add(new ProgramFunction(_opsString["MEM"]));
                            strArg = strArg.Substring(1);
                        }
                        // Convert argument from hexadecimal to decimal
                        if (strArg.StartsWith("0x"))
                        {
                            strArg = int.Parse(strArg.Substring(2), System.Globalization.NumberStyles.HexNumber).ToString();
                        }
                        if (Enum.IsDefined(typeof(Register), strArg.ToUpper()))
                        {
                            this.Program.Add(new ProgramFunction(_opsString["REG"]));
                            strArg = ((int)Enum.Parse(typeof(Register), strArg)).ToString();
                        }
                        // Check if the string contains any letter in the uppercase alphabet (lowercase is filtered out), or the numbers 0-9
                        if (Regex.IsMatch(strArg, "^[A-Z_][A-Z0-9_]*"))
                        {
                            unresolved[strArg] = this.Program.Count;
                            strArg = this.Program.Count.ToString();
                        }
                        argument = int.Parse(strArg);
                    }
                    this.Program.Add(new ProgramFunction(operation, argument));

                }
                if (unresolved.Count > 0)
                {
                    throw new Exception($"Unresolved string keys: {unresolved}");
                }
            }
            catch (KeyNotFoundException e)
            {
                // The key to _opsstring given is not valid, so there's a syntax error
                Console.WriteLine($"Syntax Error: {key} is not an operation or argument.");
                this.Program.Clear();
            }
            catch (Exception e)
            {
                // An unexpected exception
                Console.WriteLine(e);
                this.Program.Clear();
            }
        }
        public byte[] ParseToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                foreach (ProgramFunction function in Program)
                {
                    writer.Write(function.Instruction.InstructionByte);
                    if (function.Instruction.Arguments > 0)
                    {
                        writer.Write(function.Argument0);
                    }
                }
                return ms.ToArray();
            }
        }
        public void Parse(byte[] bytes)
        {
            this.Program = new List<ProgramFunction>();
            this.Reset();
            using (MemoryStream stream = new MemoryStream(bytes))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < stream.Length)
                {
                    Operation instruction = _opsID[reader.ReadByte()];
                    if (instruction.Arguments > 0)
                        Program.Add(new ProgramFunction(instruction, reader.ReadInt32()));
                    else
                        Program.Add(new ProgramFunction(instruction));
                }
            }

        }
        // Enums and structs

        public struct Operation
        {
            public Operation(string instruction, byte instructionByte, Action<int> action)
            {
                Instruction = instruction;
                InstructionByte = instructionByte;
                Arguments = 0;
                InstructionAction = action;
            }
            public Operation(string instruction, byte instructionByte, int arguments, Action<int> action)
            {
                Instruction = instruction;
                InstructionByte = instructionByte;
                Arguments = arguments;
                InstructionAction = action;
            }

            public override string ToString()
            {
                return $"{Instruction} [{base.ToString()}]";
            }
            public string Instruction;
            public byte InstructionByte;
            public int Arguments;
            public Action<int> InstructionAction;
        }
        public enum Register
        {
            EAX = 0x00,
            EBX = 0x01,
            PP = 0x02
        }
        public class ProgramFunction
        {
            public Operation Instruction;
            public int Argument0;
            public ProgramFunction(Operation type)
            {
                Instruction = type;
            }
            public ProgramFunction(Operation type, int argument0)
            {
                Instruction = type;
                Argument0 = argument0;
            }

            public override string ToString()
            {
                string output = Instruction.Instruction;
                for (int i = 0; i < Instruction.Arguments; i++)
                {
                    output += $" {Argument0}";
                }
                return output;
            }
        }


        // Class Functions
        private int GetRegister(Register register)
        {
            switch (register)
            {
                case Register.EAX:
                    return registerA;
                case Register.EBX:
                    return registerB;
                case Register.PP:
                    return programPointer;
            }
            return 0;
        }
        private int GetValue(int value)
        {
            int returnValue = value;
            if (RegFlag())
            {
                returnValue = GetRegister((Register)returnValue);
                RegFlag(false);
            }
            if (MemFlag())
            {
                returnValue = memory[returnValue];
                MemFlag(false);
            }
            return returnValue;
        }

        public void Reset()
        {
            registerA = 0;
            registerB = 0;
            programPointer = 0;
            _flags = 0;
            memory = new int[MemorySize];
            stack = new Stack(StackSize);

        }
        public byte Execute()
        {
            // Return value from program, defaults to 0 if not modified
            byte returnValue = 0;

            // Clear there registers and memory
            List<ProgramFunction> program = this.Program;
            Reset();
            try
            {
                for (programPointer = 0; programPointer < program.Count;)
                {
                    ProgramFunction function = (ProgramFunction)program[programPointer];
                    function.Instruction.InstructionAction(function.Argument0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                returnValue = 0xff;
            }
            return returnValue;
        }
    }
}

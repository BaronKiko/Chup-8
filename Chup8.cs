using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chup_8
{
    public class Chup8
    {
        Random rnd = new Random();

        private ushort PC;
        private byte[] Memory;
        private byte[] V;
        private ushort I;
        private ushort SP;

        public Chup8(string FilePath)
        {
            // Init variables
            PC     = 0x200;
            Memory = new byte[4096];
            V      = new byte[16];

            //Load the ROM
            byte[] FileData = File.ReadAllBytes(FilePath);

            for (int Index = 0; Index < FileData.Length; ++Index)
                Memory[Index + 0x200] = FileData[Index];
        }


        public void Run()
        {
            while (true)
                Step();
        }


        private void Step()
        {
            ushort instruction = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            // Complete instructions
            switch ((OpCode)instruction)
            {
                
            }

            // Opcodes
            switch ((OpCode)(instruction & 0xF000))
            {
                case OpCode.Annn:
                    I = (ushort)(instruction & 0x0FFF);
                    PC += 2;
                    Console.WriteLine("Executed Annn");
                    break;

                case OpCode.RND:
                    V[(instruction & 0x0F00) >> 8] = (byte)(rnd.Next() & instruction);
                    PC += 2;
                    Console.WriteLine("Executed RND");
                    break;

                case OpCode.JP:
                    PC = (byte)(instruction & 0xFFF);
                    break;

                case OpCode.Clear:
                    //TODO: Clear graphics
                    PC += 2;
                    break;

                default: throw new Exception($"Instruction [0x{instruction:X4}] is unknown!");
            }
        }
    }
}

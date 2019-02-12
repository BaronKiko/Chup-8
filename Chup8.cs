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

        private ushort   PC;
        private byte[]   Memory;
        private byte[]   V;
        private ushort   I;
        private ushort   StackPointer;
        private ushort[] Stack;

        public Chup8(string FilePath)
        {
            // Init variables
            PC           = 0x200;
            Memory       = new byte[4096];
            V            = new byte[16];
            StackPointer = 0;
            Stack        = new ushort[16];

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


        private void Push(ushort value)
        {
            Stack[StackPointer++] = value;
        }


        public ushort Pop()
        {
            return Stack[StackPointer--];
        }


        private void Step()
        {
            ushort instruction = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            // Complete instructions
            switch ((OpCode)instruction)
            {
                case OpCode.Clear:
                    //TODO: Clear graphics
                    PC += 2;
                    break;
            }

            // Opcodes
            switch ((OpCode)(instruction & 0xF000))
            {
                // 1nnn
                case OpCode.JP:
                    PC = (ushort)(instruction & 0xFFF);
                    break;

                // 2nnn
                case OpCode.CALL:
                    Push((ushort)(PC + 2));
                    PC = (ushort)(instruction & 0xFFF);
                    break;

                //Cxkk
                case OpCode.RND:
                    V[(instruction & 0x0F00) >> 8] = (byte)(rnd.Next() & instruction);
                    PC += 2;
                    break;

                // Annn
                case OpCode.LDI:
                    I = (ushort)(instruction & 0x0FFF);
                    PC += 2;
                    break;

                default:
                    throw new Exception($"Instruction [0x{instruction:X4}] is unknown!");
            }
        }
    }
}

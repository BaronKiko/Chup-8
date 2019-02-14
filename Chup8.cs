using Chup_OGL;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Chup_8
{
    public class Chup8
    {
        Random    rnd = new Random();
        Stopwatch sw  = new Stopwatch();

        private ushort   PC;
        private byte[]   Memory;
        private byte[]   V;
        private ushort   I;
        private ushort   StackPointer;
        private ushort[] Stack;

        public ulong[] Display;
        private int    DisplayMode;
        private int    DisplayWidth, DisplayHeight;

        private bool[] keys;

        private byte DelayTimer;
        private byte SoundTimer;
        private long lastTime, lastStep;

        private WindowManager wm;

        private bool redrawNeeded = true;
        private bool RendorToConsole = false;

        public Chup8(string FilePath, int displayMode)
        {
            if (RendorToConsole)
                Console.OutputEncoding = Encoding.UTF8;
            else
                wm = new WindowManager();

            // Init variables
            PC           = 0x200;
            Memory       = new byte[4096];
            V            = new byte[16];

            // Stack
            StackPointer = 0;
            Stack        = new ushort[16];

            // Init display
            DisplayMode = displayMode;
            if (displayMode == 0)
            {
                DisplayWidth  = 64;
                DisplayHeight = 32;

                Display = new ulong[DisplayHeight];
            }
            else
                throw new Exception($"Unsupported Display Mode {displayMode}");

            // Init Timers
            DelayTimer = 0;
            SoundTimer = 0;
            lastTime   = 0;
            lastStep   = 0;
            sw.Start();

            //Load the ROM
            byte[] FileData = File.ReadAllBytes(FilePath);

            for (int Index = 0; Index < FileData.Length; ++Index)
                Memory[Index + 0x200] = FileData[Index];

            // Load fonts
            Fonts.LoadFonts(ref Memory);
        }


        public void Run(int frequency)
        {
            while (true)
            {
                if (sw.ElapsedMilliseconds - lastStep > 1000.0 / frequency)
                {
                    keys = wm.GetKeys();
                    Step();
                    lastStep = sw.ElapsedMilliseconds;
                }

                if (sw.ElapsedMilliseconds - lastTime > 1000.0 / 60)
                {
                    if (DelayTimer > 0)
                        DelayTimer--;

                    if (SoundTimer > 0)
                        SoundTimer--;

                    lastTime = sw.ElapsedMilliseconds;
                }

                if (redrawNeeded)
                {
                    if (RendorToConsole)
                        DrawDisplay();
                    else
                        wm.DrawFrame(Display);
                    redrawNeeded = false;
                }
            }
        }


        private void Push(ushort value)
        {
            Stack[StackPointer++] = value;
        }


        public ushort Pop()
        {
            return Stack[--StackPointer];
        }


        private void DrawDisplay()
        {
            string output = "";

            for (int i = 0; i < 32; i++)
            {
                ulong mask = (ulong)1 << 63;

                for (int x = 0; x < 64; x++)
                {
                    output += ((Display[i] & mask) == 0) ? " " : "\u2588";
                    mask >>= 1;
                }

                output += '\n';
            }

            Console.SetCursorPosition(0, 0);
            Console.Clear();
            Console.Write(output);
        }


        private bool DrawByte(int x, int y, byte draw)
        {
            bool collision = false;

            for (int i = 0; i < 8; i++)
            {
                if ((draw & (1 << (8 - i - 1))) == 0)
                    continue;

                int destX     = (x + i) % DisplayWidth;
                ulong bitmask = ((ulong)1) << (DisplayWidth - destX - 1);

                // Get current displayed pixel
                if ((Display[y] & bitmask) != 0)
                    collision = true;

                Display[y] ^= bitmask;
            }

            redrawNeeded = true;

            return collision;
        }

        public void Step()
        {
            ushort instruction = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);
            PC += 2;

            // Complete instructions
            switch ((OpCode)instruction)
            {
                case OpCode.Clear:
                    for (int i = 0; i < Display.Length; i++)
                        Display[i] = 0;
                    return;

                case OpCode.RET:
                    PC = Pop();
                    return;
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
                    Push((ushort)(PC));
                    PC = (ushort)(instruction & 0xFFF);
                    break;

                // 3xkk
                case OpCode.SKIP:
                    if (V[(instruction & 0x0F00) >> 8] == (byte)instruction)
                        PC += 2;
                    break;

                // 4xkk
                case OpCode.NSKIP:
                    if (V[(instruction & 0x0F00) >> 8] != (byte)instruction)
                        PC += 2;
                    break;

                // 5xy0
                case OpCode.SKIPR:
                    if (V[(instruction & 0x0F00) >> 8] == V[(instruction & 0x00F0) >> 4])
                        PC += 2;
                    break;

                // 6xkk
                case OpCode.LDRC:
                    V[(instruction & 0x0F00) >> 8] = (byte)instruction;
                    break;

                // 7xkk
                case OpCode.ADDRC:
                    V[(instruction & 0x0F00) >> 8] += (byte)instruction;
                    break;

                case OpCode.HEX8:
                    Run8(instruction);
                    break;

                // 9xy0
                case OpCode.NSKIPR:
                    if (V[(instruction & 0x0F00) >> 8] != V[(instruction & 0x0F0) >> 4])
                        PC += 2;
                    break;

                // Annn
                case OpCode.LDI:
                    I = (ushort)(instruction & 0x0FFF);
                    break;

                // Bnnn
                case OpCode.JUMPO:
                    PC = (ushort)((ushort)(instruction & 0xFFF) + V[0]);
                    break;

                // Cxkk
                case OpCode.RND:
                    V[(instruction & 0x0F00) >> 8] = (byte)(rnd.Next() & instruction);
                    break;

                // Dxyn
                case OpCode.DRAW:
                    byte x   = V[(instruction & 0xF00) >> 8];
                    byte y   = V[(instruction & 0x0F0) >> 4];
                    byte len = (byte)(instruction & 0x00F);
                    int  end = I + len;

                    // Set collision to 0
                    byte vf = 0;

                    for (int pos = I; pos < end; pos++)
                    {
                        if (DrawByte(x, y++ % DisplayHeight, Memory[pos]))
                            vf = 1;
                    }

                    V[0xF] = vf;

                    break;

                // Ex9E
                // ExA1 NOT skip
                case OpCode.SKIPKEY:
                    bool state = keys[V[(instruction & 0x0F00) >> 8]];

                    if ((byte)instruction == 0xA1)
                        state = !state;

                    if (state)
                        PC += 2;

                    break;

                case OpCode.F:
                    RunF(instruction);
                    break;

                default:
                    throw new Exception($"Instruction [0x{instruction:X4}] is unknown!");
            }
        }

        private void Run8(ushort instruction)
        {
            byte vf;

            int x = (instruction & 0x0F00) >> 8;
            int y = (instruction & 0x00F0) >> 4;

            switch ((OpCode)(instruction & 0xF))
            {
                // 8xy0
                case OpCode.LDRR:
                    V[x] = V[y];
                    break;

                // 8xy1
                case OpCode.ORRR:
                    V[x] |= V[y];
                    break;

                // 8xy2
                case OpCode.ANDRR:
                    V[x] &= V[y];
                    break;

                // 8xy3
                case OpCode.XORRR:
                    V[x] ^= V[y];
                    break;

                // 8xy4
                case OpCode.ADDRR:
                    int temp = V[x] + V[y];
                    vf     = (byte)((temp >> 8) & 1);
                    V[x]   = (byte)temp;
                    V[0xF] = vf;
                    break;

                // 8xy5
                case OpCode.SUBRR:
                    vf     = (byte)((V[x] > V[y]) ? 1 : 0);
                    V[x]  -= V[y];
                    V[0xF] = vf;
                    break;

                // 8x06
                case OpCode.SHR:
                    vf     = (byte)(V[x] & 1);
                    V[x] >>= 1;
                    V[0xF] = vf;
                    break;

                // 8xy7
                case OpCode.SUBRRN:
                    vf = (byte)((V[y] > V[x]) ? 1 : 0);
                    V[x] -= V[y];
                    V[0xF] = vf;
                    break;

                // 8x0E
                case OpCode.SHL:
                    vf     = (byte)((V[x] >> 7) & 1);
                    V[x] <<= 1;
                    V[0xF] = vf;
                    break;

                default:
                    throw new Exception($"Instruction [0x{instruction:X4}] is unknown!");
            }
        }

        private void RunF(ushort instruction)
        {
            int count;

            switch ((OpCode) (instruction & 0xFF))
            {
                // 0xFx07
                case OpCode.GETDELAY:
                    V[(instruction & 0x0F00) >> 8] = DelayTimer;
                    break;

                // 0xFx0A
                case OpCode.WAITKEY:
                    byte key = 0xFF;
                    while (key == 0xFF)
                    {
                        bool[] keys = wm.GetKeys();
                        for (byte i = 0; i < 16; i++)
                        {
                            if (keys[i])
                            {
                                key = i;
                                break;
                            }
                        }

                        if (key == 0xFF)
                            Thread.Sleep(1);
                    }

                    V[(instruction & 0x0F00) >> 8] = key;

                    break;

                // Fx15
                case OpCode.SETDELAY:
                    DelayTimer = V[(instruction & 0x0F00) >> 8];
                    break;

                // Fx18
                case OpCode.SETSOUND:
                    SoundTimer = V[(instruction & 0x0F00) >> 8];
                    break;

                // Fx1E
                case OpCode.ADDIR:
                    I += V[(instruction & 0x0F00) >> 8];
                    break;

                // Fx29
                case OpCode.LDFT:
                    I = (ushort)(V[(instruction & 0x0F00) >> 8] * 5);
                    break;

                // Fx33
                case OpCode.LDBCD:
                    byte value = V[(instruction & 0x0F00) >> 8];

                    Memory[I]     = (byte)((value / 100) % 10);
                    Memory[I + 1] = (byte)((value / 10)  % 10);
                    Memory[I + 2] = (byte)(value         % 10);
                    break;

                // Fx55
                case OpCode.LDRANGE:
                    count = ((instruction & 0x0F00) >> 8) + 1;

                    for (int i = 0; i < count; i++)
                        Memory[I + i] = V[i];

                    break;

                // Fx65
                case OpCode.RDRANGE:
                    count = ((instruction & 0x0F00) >> 8) + 1;

                    for (int i = 0; i < count; i++)
                        V[i] = Memory[I + i];

                    break;

                default:
                    throw new Exception($"Instruction [0x{instruction:X4}] is unknown!");
            }
        }
    }
}

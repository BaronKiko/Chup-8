namespace Chup_8
{
    public enum OpCode
    {
        Clear    = 0x00E0,
        RET      = 0x00EE,
        JP       = 0x1000,
        CALL     = 0x2000,
        SKIP     = 0x3000,
        NSKIP    = 0x4000,
        SKIPR    = 0x5000,
        LDRC     = 0x6000,
        ADDRC    = 0x7000,
        NSKIPR   = 0x9000,
        LDI      = 0xA000,
        JUMPO    = 0xB000,
        RND      = 0xC000,
        DRAW     = 0xD000,
        SKIPKEY  = 0xE000,

        // 8
        HEX8     = 0x8000,
        LDRR     = 0x0000,
        ORRR     = 0x0001,
        ANDRR    = 0x0002,
        XORRR    = 0x0003,
        ADDRR    = 0x0004,
        SUBRR    = 0x0005,
        SHR      = 0x0006,
        SUBRRN   = 0x0007,
        SHL      = 0x000E,


        // F
        F        = 0xF000,
        GETDELAY = 0x0007,
        WAITKEY  = 0x000A,
        SETDELAY = 0x0015,
        SETSOUND = 0x0018,
        ADDIR    = 0x001E,
        LDFT     = 0x0029,
        LDBCD    = 0x0033,
        LDRANGE  = 0x0055,
        RDRANGE  = 0x0065,
    }
}

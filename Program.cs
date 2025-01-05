using JHelper.Common.ProcessInterop;

namespace ASRT_No_AI;

internal static partial class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        ProcessMemory? process = ProcessMemory.HookProcess("ASN_App_PcDx9_Final.exe");
        if (process is null)
        {
            Messages.GameNotFoundError();
            return;
        }

        if (process.MainModule.ModuleMemorySize != 0xC7C000 && process.MainModule.ModuleMemorySize != 0xD06000)
        {
            Messages.VersionMismatchError();
            return;
        }

        IntPtr injectionpoint = process.MainModule.BaseAddress + 0x4C3BEC;

        if (!process.Read(injectionpoint, out byte status))
        {
            Messages.CannotAccessMemoryError();
            return;
        }

        if (status != 0xE8)
        {
            Span<byte> injectedcode = stackalloc byte[] { 0x8B, 0x0D, 0x30, 0x74, 0xBC, 0x00, 0x43, 0xEB, 0x04, 0x3B, 0x59, 0x28, 0xC3, 0x50, 0xA1, 0x88, 0x1A,
                0xEC, 0x00, 0x83, 0xB8, 0x25, 0x05, 0x00, 0x00, 0x00, 0x58, 0x75, 0xEC, 0x50, 0x53, 0x51, 0x52, 0x31, 0xC0, 0x31, 0xD2, 0xB9, 0x3F, 0x0F,
                0xBD, 0x00, 0x41, 0x8B, 0x19, 0x00, 0xD8, 0xFE, 0xC2, 0x80, 0xFA, 0x04, 0x75, 0xF4, 0x5A, 0x59, 0x5B, 0x39, 0xC3, 0x58, 0xEB, 0xCE };

            IntPtr injectionaddress = process.Allocate(injectedcode.Length);

            if (injectionaddress == IntPtr.Zero || !process.WriteArray<byte>(injectionaddress, injectedcode))
            {
                Messages.CannotAccessMemoryError();
                return;
            }

            Span<byte> injectedcodecall = stackalloc byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90 };

            nint offset = injectionaddress - (injectionpoint + 5);
            unsafe
            {
                Span<byte> ptr = new(&offset, sizeof(int));
                ptr.CopyTo(injectedcodecall.Slice(1, sizeof(int)));
            }

            if (!process.WriteArray<byte>(injectionpoint, injectedcodecall))
            {
                Messages.CannotAccessMemoryError();
                return;
            }

            Messages.Success();
        }
        else
        {
            if (Messages.GameAlreadyPatched() == DialogResult.Yes)
            {
                Span<byte> originalCode = stackalloc byte[] { 0x8B, 0x0D, 0x30, 0x74, 0xBC, 0x00, 0x43, 0x3B, 0x59, 0x28 };

                if (!process.Read<int>(injectionpoint + 1, out int offset)
                    || !process.WriteArray<byte>(injectionpoint, originalCode)
                    || !process.Deallocate(injectionpoint + 5 + offset))
                {
                    Messages.CannotAccessMemoryError();
                    return;
                }

                Messages.GameUnpatched();
            }
        }
    }
}
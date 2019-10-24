using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASRT_No_AI_Tool
{
    public class Program
    {
        [DllImport("kernel32")]
        private static extern int OpenProcess(int dwDesiredAccess, int bInheritHandle, int dwProcessId);

        [DllImport("kernel32")]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpbuffer, int nSize, int lpNumberOfBytesWritten);

        [DllImport("kernel32")]
        private static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int nSize, int lpNumberOfBytesRead);

        [DllImport("kernel32")]
        private static extern int VirtualAllocEx(int hProcess, IntPtr lpAddress, int nSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32")]
        private static extern int VirtualFreeEx(int hProcess, int lpAddress, int nSize, uint dwFreeType);

        private static int processHandle;

        public static void Main()
        {
            Application.EnableVisualStyles();
            Process[] processList = Process.GetProcessesByName("ASN_App_PcDx9_Final");
            if (processList.Length == 0)
            {
                MessageBox.Show("Please start the game first!", "ASRT No AI Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            foreach (Process process in processList)
            {
                processHandle = OpenProcess(0x38, 0, process.Id);

                if (processHandle == 0)
                {
                    MessageBox.Show("Could not access the game, please run as administrator!", "ASRT No AI Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(2);
                }

                if (process.MainModule.ModuleMemorySize != 0xC7C000 && process.MainModule.ModuleMemorySize != 0xD06000)
                {
                    MessageBox.Show("Cannot apply patch. Please ensure you are\n" +
                                    "running the correct version of the game!", "ASRT No AI Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(2);
                }
                byte[] status = new byte[4];
                int bytesRead = 0;
                int injectionpoint = 0x8C3BEC;
                ReadProcessMemory(processHandle, injectionpoint, status, 1, bytesRead);
                if (BitConverter.ToInt32(status, 0) != 0xE8)
                {
                    byte[] injectedcode = new byte[] {0x8B, 0x0D, 0x30, 0x74, 0xBC, 0x00, 0x43, 0xEB, 0x04, 0x3B, 0x59, 0x28, 0xC3, 0x50, 0xA1, 0x88, 0x1A,
                    0xEC, 0x00, 0x83, 0xB8, 0x25, 0x05, 0x00, 0x00, 0x00, 0x58, 0x75, 0xEC, 0x50, 0x53, 0x51, 0x52, 0x31, 0xC0, 0x31, 0xD2, 0xB9, 0x3F, 0x0F,
                    0xBD, 0x00, 0x41, 0x8B, 0x19, 0x00, 0xD8, 0xFE, 0xC2, 0x80, 0xFA, 0x04, 0x75, 0xF4, 0x5A, 0x59, 0x5B, 0x39, 0xC3, 0x58, 0xEB, 0xCE};
                    int injectionaddress = VirtualAllocEx(processHandle, IntPtr.Zero, injectedcode.Length, 0x00001000, 0x40);
                    WriteProcessMemory(processHandle, injectionaddress, injectedcode, injectedcode.Length, 0);
                    byte[] injectedcodecall = new byte[] { 0xE8, 0x00, 0x00, 0x00, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90 };
                    BitConverter.GetBytes(injectionaddress - (injectionpoint + 5)).CopyTo(injectedcodecall, 1);
                    WriteProcessMemory(processHandle, injectionpoint, injectedcodecall, injectedcodecall.Length, 0);
                    MessageBox.Show("You are now free from the AI in Single Race mode!", "ASRT No AI Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    if (MessageBox.Show("It appears your game is already patched\n" +
                                       "Do you want to remove the patch and restore stock settings?", "ASRT No AI Tool", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        byte[] offset = new byte[4];
                        ReadProcessMemory(processHandle, injectionpoint + 1, offset, offset.Length, bytesRead);
                        WriteProcessMemory(processHandle, injectionpoint, new byte[] { 0x8B, 0x0D, 0x30, 0x74, 0xBC, 0x00, 0x43, 0x3B, 0x59, 0x28 }, 10, 0);
                        VirtualFreeEx(processHandle, injectionpoint + 5 + BitConverter.ToInt32(offset, 0), 10, 0x4000);
                        MessageBox.Show("No AI mod has been disabled!\n" +
                        "Enjoy your stock experience!", "ASRT No AI Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
    }
}
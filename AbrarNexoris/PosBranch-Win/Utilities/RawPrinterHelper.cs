using System;
using System.Runtime.InteropServices;

namespace PosBranch_Win.Utilities
{
    public class RawPrinterHelper
    {
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true)]
        static extern bool OpenPrinter(string printerName, out IntPtr hPrinter, IntPtr pDefault);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter")]
        static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA")]
        static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] ref DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter")]
        static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter")]
        static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter")]
        static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter")]
        static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int count, out int written);

        [StructLayout(LayoutKind.Sequential)]
        public struct DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }

        public static bool SendBytesToPrinter(string printerName, byte[] bytes, string docName)
        {
            IntPtr hPrinter = IntPtr.Zero;
            bool docStarted = false;
            bool pageStarted = false;
            
            try
            {
                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Attempting to print to '{printerName}' with document '{docName}'");
                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Data size: {bytes.Length} bytes");
                
                DOCINFOA di = new DOCINFOA
                {
                    pDocName = docName,
                    pDataType = "RAW"
                };

                if (!OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Failed to open printer '{printerName}'");
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Successfully opened printer");

                if (!StartDocPrinter(hPrinter, 1, ref di))
                {
                    System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Failed to start document");
                    return false;
                }
                docStarted = true;
                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Successfully started document");

                if (!StartPagePrinter(hPrinter))
                {
                    System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Failed to start page");
                    return false;
                }
                pageStarted = true;
                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Successfully started page");

                IntPtr pUnmanagedBytes = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, pUnmanagedBytes, bytes.Length);
                bool success = WritePrinter(hPrinter, pUnmanagedBytes, bytes.Length, out int written);
                Marshal.FreeCoTaskMem(pUnmanagedBytes);

                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: WritePrinter result: {success}, bytes written: {written}");

                if (pageStarted)
                {
                    EndPagePrinter(hPrinter);
                    System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Ended page");
                }

                if (docStarted)
                {
                    EndDocPrinter(hPrinter);
                    System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Ended document");
                }

                if (hPrinter != IntPtr.Zero)
                {
                    ClosePrinter(hPrinter);
                    System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Closed printer");
                }

                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Print operation completed successfully: {success}");
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RawPrinterHelper: Exception occurred: {ex.Message}");
                
                // Cleanup in case of exception
                try
                {
                    if (pageStarted && hPrinter != IntPtr.Zero)
                        EndPagePrinter(hPrinter);
                    if (docStarted && hPrinter != IntPtr.Zero)
                        EndDocPrinter(hPrinter);
                    if (hPrinter != IntPtr.Zero)
                        ClosePrinter(hPrinter);
                }
                catch
                {
                    // Ignore cleanup errors
                }
                
                return false;
            }
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PosBranch_Win.Utilities
{
    /// <summary>
    /// Utility class to load custom embedded fonts for use in the application.
    /// Ensures fonts work on any PC without requiring font installation in Windows\Fonts.
    /// </summary>
    public static class CustomFontLoader
    {
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pFileView, uint cjSize, IntPtr pReserved, [In] ref uint pNumFonts);

        [DllImport("gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int AddFontResourceEx(string lpszFilename, uint fl, IntPtr pdv);

        private const uint FR_PRIVATE = 0x10;

        private static PrivateFontCollection privateFontCollection;
        private static IntPtr fontBufferPtr = IntPtr.Zero;
        private static bool isInitialized = false;
        private static readonly object lockObj = new object();

        /// <summary>
        /// Initializes and registers the DS-Digital font globally for the process.
        /// Call this once in Program.Main() before any forms are instantiated.
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
                return;

            lock (lockObj)
            {
                if (isInitialized)
                    return;

                try
                {
                    byte[] fontData = null;

                    // 1. Try reading from embedded resource "PosBranch_Win.Font.DS-DIGI.TTF"
                    Assembly assembly = Assembly.GetExecutingAssembly();
                    string resourceName = "PosBranch_Win.Font.DS-DIGI.TTF";

                    using (Stream fontStream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (fontStream != null)
                        {
                            fontData = new byte[fontStream.Length];
                            fontStream.Read(fontData, 0, (int)fontStream.Length);
                        }
                    }

                    // Fallback: Try reading from disk ("Font\DS-DIGI.TTF" or "DS-DIGI.TTF" in startup folder)
                    string fontDiskPath = Path.Combine(Application.StartupPath, "Font", "DS-DIGI.TTF");
                    if (!File.Exists(fontDiskPath))
                    {
                        fontDiskPath = Path.Combine(Application.StartupPath, "DS-DIGI.TTF");
                    }

                    if (fontData == null && File.Exists(fontDiskPath))
                    {
                        fontData = File.ReadAllBytes(fontDiskPath);
                    }

                    // 2. Register with Windows GDI via AddFontResourceEx
                    if (File.Exists(fontDiskPath))
                    {
                        try
                        {
                            AddFontResourceEx(fontDiskPath, FR_PRIVATE, IntPtr.Zero);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"AddFontResourceEx warning: {ex.Message}");
                        }
                    }
                    else if (fontData != null)
                    {
                        // Save embedded font bytes to a temp font file and register with GDI
                        try
                        {
                            string tempDir = Path.Combine(Path.GetTempPath(), "NexorisFonts");
                            Directory.CreateDirectory(tempDir);
                            string tempFontPath = Path.Combine(tempDir, "DS-DIGI.TTF");
                            if (!File.Exists(tempFontPath) || new FileInfo(tempFontPath).Length != fontData.Length)
                            {
                                File.WriteAllBytes(tempFontPath, fontData);
                            }
                            AddFontResourceEx(tempFontPath, FR_PRIVATE, IntPtr.Zero);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Temp font save warning: {ex.Message}");
                        }
                    }

                    // 3. Register font bytes into process GDI via AddFontMemResourceEx and GDI+ via PrivateFontCollection
                    if (fontData != null && fontData.Length > 0)
                    {
                        privateFontCollection = new PrivateFontCollection();

                        // Allocate persistent memory (DO NOT free immediately as GDI+ needs it active)
                        fontBufferPtr = Marshal.AllocCoTaskMem(fontData.Length);
                        Marshal.Copy(fontData, 0, fontBufferPtr, fontData.Length);

                        // Register with GDI+
                        privateFontCollection.AddMemoryFont(fontBufferPtr, fontData.Length);

                        // Register with GDI system table for WinForms controls
                        uint dummy = 0;
                        AddFontMemResourceEx(fontBufferPtr, (uint)fontData.Length, IntPtr.Zero, ref dummy);
                    }

                    isInitialized = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in CustomFontLoader.Initialize: {ex.Message}");
                    isInitialized = true;
                }
            }
        }

        /// <summary>
        /// Gets the DS-Digital font with the specified size and style.
        /// </summary>
        public static Font GetDSDigitalFont(float size, FontStyle style = FontStyle.Bold)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            // 1. Try using PrivateFontCollection
            try
            {
                if (privateFontCollection != null && privateFontCollection.Families.Length > 0)
                {
                    return new Font(privateFontCollection.Families[0], size, style);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetDSDigitalFont PrivateFontCollection exception: {ex.Message}");
            }

            // 2. Try creating font by family name "DS-Digital" (registered by AddFontMemResourceEx / AddFontResourceEx)
            try
            {
                Font fontByName = new Font("DS-Digital", size, style);
                if (string.Equals(fontByName.FontFamily.Name, "DS-Digital", StringComparison.OrdinalIgnoreCase))
                {
                    return fontByName;
                }
            }
            catch { }

            // 3. Final fallback to default system font
            return new Font(FontFamily.GenericSansSerif, size, style);
        }

        /// <summary>
        /// Gets the DS-Digital font family if available.
        /// </summary>
        public static FontFamily GetDSDigitalFontFamily()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (privateFontCollection != null && privateFontCollection.Families.Length > 0)
            {
                return privateFontCollection.Families[0];
            }

            try
            {
                return new FontFamily("DS-Digital");
            }
            catch
            {
                return FontFamily.GenericSansSerif;
            }
        }

        /// <summary>
        /// Disposes font resources when application exits.
        /// </summary>
        public static void Dispose()
        {
            if (privateFontCollection != null)
            {
                privateFontCollection.Dispose();
                privateFontCollection = null;
            }

            if (fontBufferPtr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(fontBufferPtr);
                fontBufferPtr = IntPtr.Zero;
            }

            isInitialized = false;
        }
    }
}

using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace PosBranch_Win.Utilities
{
    /// <summary>
    /// Utility class to load custom embedded fonts for use in the application
    /// This ensures fonts work on any PC without requiring font installation
    /// </summary>
    public static class CustomFontLoader
    {
        private static PrivateFontCollection privateFontCollection;
        private static bool isInitialized = false;

        /// <summary>
        /// Initializes and loads the DS-Digital font from embedded resources
        /// Call this once during application startup or form initialization
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized)
                return;

            try
            {
                privateFontCollection = new PrivateFontCollection();

                // Load DS-DIGI.TTF from embedded resources
                // Resource name format: PosBranch_Win.Font.DS-DIGI.TTF
                string resourceName = "PosBranch_Win.Font.DS-DIGI.TTF";

                // Get the embedded resource stream
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                using (Stream fontStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (fontStream == null)
                    {
                        // Try to list all available resources for debugging
                        string[] resources = assembly.GetManifestResourceNames();
                        string availableResources = string.Join(", ", resources);
                        throw new Exception($"Font resource '{resourceName}' not found. Available resources: {availableResources}");
                    }

                    // Read the font data into a byte array
                    byte[] fontData = new byte[fontStream.Length];
                    fontStream.Read(fontData, 0, (int)fontStream.Length);

                    // Allocate memory for the font data
                    IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
                    Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

                    // Add the font to the private font collection
                    privateFontCollection.AddMemoryFont(fontPtr, fontData.Length);

                    // Free the allocated memory
                    Marshal.FreeCoTaskMem(fontPtr);
                }

                isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading custom font: {ex.Message}");
                throw new Exception($"Failed to load DS-Digital font: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets the DS-Digital font with the specified size and style
        /// </summary>
        /// <param name="size">Font size in points</param>
        /// <param name="style">Font style (Bold, Regular, etc.)</param>
        /// <returns>Font object using the embedded DS-Digital font</returns>
        public static Font GetDSDigitalFont(float size, FontStyle style = FontStyle.Bold)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (privateFontCollection == null || privateFontCollection.Families.Length == 0)
            {
                throw new Exception("DS-Digital font not loaded. Call Initialize() first.");
            }

            // Create and return the font using the first family in the collection
            // DO NOT use "DS-Digital" string - use the FontFamily from the collection
            return new Font(privateFontCollection.Families[0], size, style);
        }

        /// <summary>
        /// Gets the DS-Digital font family
        /// </summary>
        /// <returns>FontFamily object for DS-Digital</returns>
        public static FontFamily GetDSDigitalFontFamily()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (privateFontCollection == null || privateFontCollection.Families.Length == 0)
            {
                throw new Exception("DS-Digital font not loaded. Call Initialize() first.");
            }

            return privateFontCollection.Families[0];
        }

        /// <summary>
        /// Disposes of the font collection resources
        /// Call this when the application is closing
        /// </summary>
        public static void Dispose()
        {
            if (privateFontCollection != null)
            {
                privateFontCollection.Dispose();
                privateFontCollection = null;
                isInitialized = false;
            }
        }
    }
}

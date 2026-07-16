using System.Drawing;

namespace PosBranch_Win
{
    /// <summary>
    /// Single source of truth for sidebar/navigator colors, so the Report Navigator
    /// and Favourites panel always stay visually consistent.
    /// </summary>
    internal static class SidebarTheme
    {
        // Core accent (used for headers, active state, hover borders)
        public static readonly Color AccentPrimary = Color.FromArgb(25, 118, 210);     // #1976D2 Flat Blue
        public static readonly Color AccentDark = Color.FromArgb(21, 101, 192);        // #1565C0
        public static readonly Color AccentDarker = Color.FromArgb(13, 71, 161);       // #0D47A1

        // Backgrounds
        public static readonly Color PanelBackground = Color.FromArgb(245, 247, 250);   // #F5F7FA Soft Grey-Blue
        public static readonly Color PanelBorder = Color.FromArgb(217, 225, 234);       // #D9E1EA
        public static readonly Color HeaderTop = Color.FromArgb(255, 255, 255);         // Clean modern top pane gradient
        public static readonly Color HeaderBottom = Color.FromArgb(226, 231, 236);      // Clean modern bottom pane gradient
        public static readonly Color HeaderText = Color.FromArgb(44, 62, 80);           // #2C3E50 Dark Slate Text
        public static readonly Color ItemAreaBackground = Color.FromArgb(255, 255, 255); // Pure White to contrast with background
        public static readonly Color ItemAreaBorder = Color.FromArgb(217, 225, 234);     // #D9E1EA

        // Hover
        public static readonly Color HoverBackground = Color.FromArgb(234, 244, 255);   // #EAF4FF Soft Blue Highlight
        public static readonly Color HoverText = Color.FromArgb(21, 101, 192);          // #1565C0 Darker Blue Text

        // Text
        public static readonly Color TextPrimary = Color.FromArgb(44, 62, 80);          // #2C3E50 Dark Slate Text
        public static readonly Color TextMuted = Color.FromArgb(120, 130, 140);
        public static readonly Color TextOnAccent = Color.White;

        // Typography
        public const string FontFamily = "Segoe UI";
        public const float CaptionFontSize = 9f;
        public const float HeaderFontSize = 9.5f;
        public const float ItemFontSize = 9f;

        // Spacing
        public const int ItemHeight = 30;
        public const int ItemIndent = 18;
        public const int GroupSpacing = 8;

        // Spacer Image for sub-report alignment (pushes text right to align with group header text)
        public static readonly Image TransparentSpacer = new Bitmap(20, 16);
    }
}

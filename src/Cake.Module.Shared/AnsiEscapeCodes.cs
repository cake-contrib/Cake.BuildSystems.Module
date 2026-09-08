using System;
using System.Collections.Generic;

using Cake.Core;

namespace Cake.Module.Shared
{
    /// <summary>
    /// ANSI SGR (Select Graphic Rendition) escape sequences.
    /// </summary>
    /// <remarks>
    /// Most hosted build systems render their logs in a web UI that has no notion of a console
    /// buffer, so assigning <see cref="IConsole.ForegroundColor"/> has no effect there. Those build
    /// systems do however interpret ANSI escape sequences, which is what
    /// <see cref="IConsole.SupportAnsiEscapeCodes"/> reports.
    /// </remarks>
    public static class AnsiEscapeCodes
    {
        /// <summary>
        /// Resets all previously applied graphic renditions.
        /// </summary>
        public static readonly string Reset = Sgr(0);

        /// <summary>
        /// Renders the following text in white.
        /// </summary>
        public static readonly string ForegroundWhite = Sgr(97);

        /// <summary>
        /// Renders the following text in yellow.
        /// </summary>
        public static readonly string ForegroundYellow = Sgr(33);

        /// <summary>
        /// Renders the following text in light gray.
        /// </summary>
        public static readonly string ForegroundLightGray = Sgr(37);

        /// <summary>
        /// Renders the following text in dark gray.
        /// </summary>
        public static readonly string ForegroundDarkGray = Sgr(90);

        /// <summary>
        /// Renders the following text in blue.
        /// </summary>
        public static readonly string ForegroundBlue = Sgr(34);

        /// <summary>
        /// Renders the following text on a magenta background.
        /// </summary>
        public static readonly string BackgroundMagenta = Sgr(45);

        /// <summary>
        /// Renders the following text on a red background.
        /// </summary>
        public static readonly string BackgroundRed = Sgr(41);

        /// <summary>
        /// Erases the remainder of the current line. Used by GitLab CI to delimit collapsible sections.
        /// </summary>
        public static readonly string SectionMarker = "\u001B[0K";

        // Mirrors the ConsoleColor to SGR parameter mapping of Cake's own AnsiConsoleRenderer, so that
        // a report rendered through escape sequences is colored like one rendered to a real terminal.
        private static readonly IDictionary<ConsoleColor, string> ForegroundCodes = new Dictionary<ConsoleColor, string>
        {
            { ConsoleColor.Black, Sgr(30) },
            { ConsoleColor.DarkRed, Sgr(31) },
            { ConsoleColor.DarkGreen, Sgr(32) },
            { ConsoleColor.DarkYellow, Sgr(33) },
            { ConsoleColor.DarkBlue, Sgr(34) },
            { ConsoleColor.DarkMagenta, Sgr(35) },
            { ConsoleColor.DarkCyan, Sgr(36) },
            { ConsoleColor.Gray, Sgr(37) },
            { ConsoleColor.DarkGray, Sgr("30;1") },
            { ConsoleColor.Red, Sgr("31;1") },
            { ConsoleColor.Green, Sgr("32;1") },
            { ConsoleColor.Yellow, Sgr("33;1") },
            { ConsoleColor.Blue, Sgr("34;1") },
            { ConsoleColor.Magenta, Sgr("35;1") },
            { ConsoleColor.Cyan, Sgr("36;1") },
            { ConsoleColor.White, Sgr("37;1") },
        };

        /// <summary>
        /// Gets the escape sequence that renders the following text in the given <see cref="ConsoleColor"/>.
        /// </summary>
        /// <param name="color">The <see cref="ConsoleColor"/> to translate.</param>
        /// <returns>The escape sequence, or an empty string if the color has no ANSI equivalent.</returns>
        public static string GetForeground(ConsoleColor color)
        {
            return ForegroundCodes.TryGetValue(color, out var code) ? code : string.Empty;
        }

        private static string Sgr(object parameters) => "\u001B[" + parameters + "m";
    }
}

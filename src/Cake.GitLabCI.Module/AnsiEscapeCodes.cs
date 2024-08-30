namespace Cake.GitLabCI.Module
{
    internal static class AnsiEscapeCodes
    {
        public static readonly string Reset = string.Format(FORMAT, 0);
        public static readonly string ForegroundWhite = string.Format(FORMAT, 97);
        public static readonly string ForegroundYellow = string.Format(FORMAT, 33);
        public static readonly string ForegroundLightGray = string.Format(FORMAT, 37);
        public static readonly string ForegroundDarkGray = string.Format(FORMAT, 90);
        public static readonly string BackgroundMagenta = string.Format(FORMAT, 45);
        public static readonly string BackgroundRed = string.Format(FORMAT, 41);
        public static readonly string SectionMarker = "\u001B[0K";

        private const string FORMAT = "\u001B[{0}m";
    }
}

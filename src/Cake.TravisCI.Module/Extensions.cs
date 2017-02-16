namespace Cake.TravisCI.Module
{
    public static class Extensions
    {
        public static string ToFoldMessage(this string s)
        {
            return s.Trim().Replace(" ", string.Empty);
        }
    }
}
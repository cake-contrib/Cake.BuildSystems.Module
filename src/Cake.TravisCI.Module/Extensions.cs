namespace Cake.TravisCI.Module
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Removes all spaces from the string.
        /// </summary>
        /// <param name="s">The string to modify.</param>
        /// <returns>The modified string.</returns>
        public static string ToFoldMessage(this string s)
        {
            return s.Trim().Replace(" ", string.Empty);
        }
    }
}

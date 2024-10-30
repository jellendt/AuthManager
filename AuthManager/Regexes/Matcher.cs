using System.Text.RegularExpressions;

namespace AuthManager.Regexes
{
    public static partial class Matcher
    {
        [GeneratedRegex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$")]
        public static partial Regex EMail();
    }
}

namespace AFVC
{
    static class StringExtensions
    {
        public static string RemoveLast(this string s, int characters)
        {
            return s.Remove(s.Length - characters);
        }

        public static string RemoveFirst(this string s, int characters)
        {
            return s.Remove(0, characters);
        }
    }
}

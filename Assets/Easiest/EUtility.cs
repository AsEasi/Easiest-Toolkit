using System.Text;
using UnityEngine.UIElements;

namespace Easiest
{

    public class EUtility
    {
        public static readonly string[] alphabet = 
            {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
             "a", "b", "c", "d", "e", "f", "g", "h", "i", "h", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "Z"};

        public static string GenerateRandomeAlphabet(int _length)
        {
            System.Random _random = new();
            StringBuilder _builder = new StringBuilder();

            for (int i = 0; i < _length; i++)
            {
                _builder.Append(alphabet[_random.Next(0, alphabet.Length)]);
            }

            return _builder.ToString();
        }
    }
}

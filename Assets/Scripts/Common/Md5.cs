using System.Security.Cryptography;
using System.Text;

namespace Assets.Scripts.Common
{
	public static class Md5
    {
        public static string Encode(string value)
        {
            var md5 = MD5.Create();
            var bytes = Encoding.ASCII.GetBytes(value);
            var hash = md5.ComputeHash(bytes);
            var stringBuilder = new StringBuilder();

            foreach (var b in hash)
            {
                stringBuilder.Append(b.ToString("X2"));
            }

            return stringBuilder.ToString();
        }

        public static ProtectedValue Encode(ProtectedValue value)
	    {
            return new ProtectedValue(Encode(value.String));
	    }
    }
}
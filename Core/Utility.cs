using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
	public class Utility {
        public static T[] ConcatArrays<T>(params T[][] list) {
            var result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++) {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }

        public static void SaveECDsaKey(ECDsaKey key, string? path = null, string? filename = null) {
            if (path == null) {
                path = Environment.CurrentDirectory;
            }
            if (filename == null) {
                filename = "ecdsa.key";
            }
            string fullpath = Path.Combine(path, filename);
            File.WriteAllText(fullpath, key.GetPrivateKeyAsString());
		}

        public static ECDsaKey LoadECDsaKey(string? path = null, string? filename = null) {
            if (path == null) {
                path = Environment.CurrentDirectory;
            }
            if (filename == null) {
                filename = "ecdsa.key";
            }
            string fullpath = Path.Combine(path, filename);
            string input = File.ReadAllText(fullpath);

            return ECDsaKey.FromPrivateKey(input);
        }
    }
}

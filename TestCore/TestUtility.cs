using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using System.IO;

namespace TestCore {
	[TestClass]
	public class TestUtility {

		public const string ECDsaKeyBase64 = "SYL708ieHD8g4JAqrWexCL3wBgZOpLtCBjJV1Ten6FU=";

		[TestMethod]
		public void TestConcatArray() {
			int[] int1 = new int[] { 1, 2, };
			int[] int2 = new int[] { 3, 4, };
			int[] intExpected = new int[] { 1, 2, 3, 4, };

			CollectionAssert.AreEqual(intExpected, Utility.ConcatArrays(int1, int2));
		}

		[TestMethod]
		public void TestSaveECDsaKey() {
			ECDsaKey key = ECDsaKey.FromPrivateKey(ECDsaKeyBase64);

			string path = Environment.CurrentDirectory;
			string filename = "testutility.key";

			Utility.SaveECDsaKey(key, path, filename);

			string content = File.ReadAllText(Path.Combine(path, filename));

			Assert.AreEqual(ECDsaKeyBase64, content);
		}

		[TestMethod]
		public void TestLoadECDsaKey() {
			string path = Environment.CurrentDirectory;
			string filename = "testutility.key";

			ECDsaKey key = Utility.LoadECDsaKey(path, filename);

			Assert.AreEqual(ECDsaKeyBase64, key.GetPrivateKeyAsString());
		}
	}
}

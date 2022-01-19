using System;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCore {
	[TestClass]
	public class TestTransaction {
		private ECDsaKey inputKey = new();
		private ECDsaKey outputKey = new();
		private static byte[] baseMerkleHash = new byte[] { 0x0, 0x0 };

		[TestInitialize()]
		public void Init() {
			inputKey = new ECDsaKey();
			outputKey = new ECDsaKey();
		}

		[TestMethod]
		public void TestTransactionSignatureUsingValidKey() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);

			Assert.IsNotNull(t.Signature);
		}


		[TestMethod]
		public void TestTransactionSignatureUsingInvalidKey() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);

			Assert.ThrowsException<ArgumentException>(() => { t.Sign(outputKey); });
		}

		[TestMethod]
		public void TestTransactionSignatureVerification() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);
			Assert.IsTrue(t.VerifySignature());
		}

		[TestMethod]
		public void TestTransactionSignatureVerificationUsingTamperedSignature() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);
			if (t.Signature != null) {
				byte[] tamperedSignature = t.Signature;
				tamperedSignature[tamperedSignature.Length - 1] = (byte)~(tamperedSignature[tamperedSignature.Length - 1]);
				t.Signature = tamperedSignature;
			} else {
				Assert.Fail();
			}

			Assert.IsFalse(t.VerifySignature());
		}

		[TestMethod]
		public void TestTransactionSignatureVerificationUsingTamperedData() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);
			Transaction t2 = new Transaction(t.Id, t.Version, t.CreationTime, baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 1100, false, t.Signature);
			Assert.IsFalse(t2.VerifySignature());
		}
	}
}

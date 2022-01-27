using System;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCore {
	[TestClass]
	public class TestTransaction {
		private ECDsaKey inputKey = new();
		private ECDsaKey outputKey = new();
		private static byte[] baseMerkleHash = Utility.GetEmptyByteArray(64);

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

		[TestMethod]
		public void TestTransactionFromBytes() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);

			Transaction t2 = Transaction.FromByteArray(t.GetByteArray());

			Assert.AreEqual(t.Version, t2.Version);
			Assert.AreEqual(t.CreationTime.ToUnixTimeSeconds(), t2.CreationTime.ToUnixTimeSeconds());
			CollectionAssert.AreEqual(t.MerkleHash, t2.MerkleHash);
			CollectionAssert.AreEqual(t.Input, t2.Input);
			CollectionAssert.AreEqual(t.Output, t2.Output);
			Assert.AreEqual(t.Amount, t2.Amount);
			Assert.AreEqual(t.IsDelegating, t2.IsDelegating);
			CollectionAssert.AreEqual(t.Signature, t2.Signature);
		}

		[TestMethod]
		public void TestTransactionFromHex() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);

			string hex = Convert.ToHexString(t.GetByteArray());

			Transaction t2 = Transaction.FromHexString(hex);

			Assert.AreEqual(t.Version, t2.Version);
			Assert.AreEqual(t.CreationTime.ToUnixTimeSeconds(), t2.CreationTime.ToUnixTimeSeconds());
			CollectionAssert.AreEqual(t.MerkleHash, t2.MerkleHash);
			CollectionAssert.AreEqual(t.Input, t2.Input);
			CollectionAssert.AreEqual(t.Output, t2.Output);
			Assert.AreEqual(t.Amount, t2.Amount);
			Assert.AreEqual(t.IsDelegating, t2.IsDelegating);
			CollectionAssert.AreEqual(t.Signature, t2.Signature);
		}
	}
}

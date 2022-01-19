using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCore {
	[TestClass]
	public class TestBlock {
		private ECDsaKey inputKey = new();
		private ECDsaKey outputKey = new();
		private ECDsaKey verifierKey = new();
		private static byte[] baseMerkleHash = new byte[] { 0x0, 0x0 };

		[TestInitialize()]
		public void Init() {
			inputKey = new ECDsaKey();
			outputKey = new ECDsaKey();
			verifierKey = new ECDsaKey();
		}

		[TestMethod]
		public void TestBlockSignatureUsingValidKey() {
			Transaction t = new Transaction( baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false );
			t.Sign(inputKey);

			Block b = new Block(baseMerkleHash, new List<Transaction>() { t }, verifierKey.GetPublicKey());
			b.Sign(verifierKey);

			Assert.IsNotNull(t.Signature);
		}

		[TestMethod]
		public void TestBlockSignatureVerification() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);

			Block b = new Block(baseMerkleHash, new List<Transaction>() { t }, verifierKey.GetPublicKey());
			b.Sign(verifierKey);

			Assert.IsTrue(b.VerifySignature());
		}

		[TestMethod]
		public void TestBlockSignatureVerificationUsingTamperedSignature() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);

			Block b = new Block(baseMerkleHash, new List<Transaction>() { t }, verifierKey.GetPublicKey());
			b.Sign(verifierKey);

			if (b.Signature != null) {
				byte[] tamperedSignature = b.Signature;
				tamperedSignature[tamperedSignature.Length - 1] = (byte)~(tamperedSignature[tamperedSignature.Length - 1]);
				b.Signature = tamperedSignature;
			} else {
				Assert.Fail();
			}

			Assert.IsFalse(b.VerifySignature());
		}

		[TestMethod]
		public void TestBlockSignatureVerificationUsingTamperedData() {
			Transaction t = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 100, false);
			t.Sign(inputKey);

			Transaction t2 = new Transaction(baseMerkleHash, inputKey.GetPublicKey(), outputKey.GetPublicKey(), 1, false);
			t2.Sign(inputKey);

			Block b = new Block(baseMerkleHash, new List<Transaction>() { t }, verifierKey.GetPublicKey());
			b.Sign(verifierKey);

			Block tamperedB = new Block(b.Id,b.Version, b.CreationTime, baseMerkleHash, new List<Transaction>() { t2 }, verifierKey.GetPublicKey(), b.Signature);

			Assert.IsFalse(tamperedB.VerifySignature());
		}
	}
}

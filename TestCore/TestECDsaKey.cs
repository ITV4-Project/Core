using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCore {
	[TestClass]
	public class TestECDsaKey {

		public const string ECDsaKeyBase64PrivateKey = @"RGFZ8WlrHB1uDl6FonCTcrqp2PE+dUyL/RYM7mgtOes=";
		public const string ECDsaKeyHeyPublicKey = "04F3388C279340D3F1CF0A8495987D3BA2517D296D5FFC96033E4CFC560D02F6E6A25004E59ED8F08ED7BD8BB982F3DECE65C367C502FEAFF7ADD3557684093A6B";

		[TestMethod]
		public void TestGetPrivateKey() {
			ECDsaKey key = new ECDsaKey();
			byte[] privateKey = key.GetPrivateKey();

			Assert.AreEqual(32, privateKey.Length);
		}

		[TestMethod]
		public void TestGetPrivateKeyAsString() {
			ECDsaKey key = new ECDsaKey();
			string privateKey = key.GetPrivateKeyAsString();

			Assert.AreEqual(44, privateKey.Length);
		}

		[TestMethod]
		public void TestGetPublicKey() {
			ECDsaKey key = new ECDsaKey();
			byte[] publickey = key.GetPublicKey();

			Assert.AreEqual(65, publickey.Length);
			Assert.AreEqual((byte)0x04, publickey[0]);
		}

		[TestMethod]
		public void TestGetPublicKeyAsString() {
			ECDsaKey key = new ECDsaKey();
			string publickey = key.GetPublicKeyAsString();

			Assert.AreEqual(130, publickey.Length);
			Assert.AreEqual('0', publickey[0]);
			Assert.AreEqual('4', publickey[1]);
		}

		[TestMethod]
		public void TestFromPrivateKey() {
			ECDsaKey key = ECDsaKey.FromPrivateKey(ECDsaKeyBase64PrivateKey);
			Assert.AreEqual(ECDsaKeyBase64PrivateKey, key.GetPrivateKeyAsString());
		}

		[TestMethod]
		public void TestFromPublicKey() {
			ECDsaKey key = ECDsaKey.FromPublicKey(ECDsaKeyHeyPublicKey);
			Assert.AreEqual(ECDsaKeyHeyPublicKey, key.GetPublicKeyAsString());
		}

		[TestMethod]
		public void TestSignAndVerify() {
			ECDsaKey key = ECDsaKey.FromPrivateKey(ECDsaKeyBase64PrivateKey);
			byte[] data = new byte[] { 0x00, 0x01, 0x02, 0x03 };

			byte[] signature = key.Sign(data);

			Assert.IsTrue(key.Verify(data, signature));
		}

	}
}

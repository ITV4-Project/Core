﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Core
{
    public class ECDsaKey
    {

        private readonly ECDsa key;

        /// <summary>
        /// Create a ECDsaKey with a new public and private key
        /// </summary>
        public ECDsaKey()
        {
            key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        }
        /// <summary>
        /// Create a ECDsaKey based on exisiting public or private key
        /// </summary>
        /// <param name="parameters">Exisitng ECParameters</param>
        public ECDsaKey(ECParameters parameters)
        {
            key = ECDsa.Create(parameters);
        }
        /// <summary>
        /// Create a ECDsaKey based on exisiting public or private key
        /// </summary>
        /// <param name="exisitingKey">Existing key as hexidecimal string</param>
        /// <param name="isPrivateKey">Is the key private or public</param>
        public ECDsaKey(String exisitingKey, bool isPrivateKey = false) : this(Convert.FromHexString(exisitingKey), isPrivateKey) { }

        /// <summary>
        /// Create a ECDsaKey based on exisiting public or private key
        /// </summary>
        /// <param name="exisitingKey">Existing key</param>
        /// <param name="isPrivateKey">Is the key private or public</param>
        public ECDsaKey(byte[] exisitingKey, bool isPrivateKey)
        {
            if (isPrivateKey)
            {
                ECParameters parameters = new ECParameters();
                parameters.Curve = ECCurve.NamedCurves.nistP256;
                parameters.D = exisitingKey;

                key = ECDsa.Create(parameters);
            }
            else
            {
                ECParameters parameters = new ECParameters();
                parameters.Curve = ECCurve.NamedCurves.nistP256;

                parameters.Q = new ECPoint
                {
                    X = exisitingKey.Skip(1).Take(16).ToArray(),
                    Y = exisitingKey.Skip(17).ToArray()
                };

                key = ECDsa.Create(parameters);
            }
        }

        /// <summary>
        /// Get private key as string
        /// </summary>
        /// <returns>The private key as a hexadecimal string</returns>
        public string GetPrivateKey()
        {
            if (key == null) {
                throw new NullReferenceException("ECDsaKey has not been initialized");
			}
            ECParameters p = key.ExportParameters(true);
            var privateKey = p.D;
            if (privateKey == null) {
                throw new NullReferenceException("ECDsaKey has no private key");
            }
            return Convert.ToHexString(privateKey);
        }

        /// <summary>
        /// Get public key as string
        /// </summary>
        /// <returns>The public key as a hexadecimal string</returns>
        public string GetPublicKey() {
            ECParameters p = key.ExportParameters(false);
            byte[] prefix = { 0x04 };
            byte[]? x = p.Q.X;
            byte[]? y = p.Q.Y;
            if (x == null || y == null) {
                throw new NullReferenceException("ECDsaKey has no public key");
            }
            byte[] publicKey = prefix.Concat(x).Concat(y).ToArray();
            return Convert.ToHexString(publicKey);
        }

        /// <summary>
        /// Use the ECDsaKey to sign a byte array
        /// </summary>
        /// <param name="data">The byte array to sign</param>
        /// <returns>The signature of the byte array</returns>
        public byte[] Sign(byte[] data)
        {
            return key.SignData(data, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// Use the ECDsaKey to sign a string
        /// </summary>
        /// <param name="data">The string to sign</param>
        /// <returns>The signature of the string</returns>
        public string Sign(String data)
        {
            byte[] dataBytes = Encoding.Unicode.GetBytes(data);
            byte[] signature = key.SignData(dataBytes, HashAlgorithmName.SHA256);
            return Convert.ToBase64String(signature);
        }

        /// <summary>
        /// Veirfy the signature of a byte array
        /// </summary>
        /// <param name="data">The byte array to verify</param>
        /// <param name="signature">The signature to verify as a byte array</param>
        /// <returns>True if genuine</returns>
        public bool Verify(byte[] data, byte[] signature)
        {
            return key.VerifyData(data, signature, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// Veirfy the signature of a string
        /// </summary>
        /// <param name="data">The string to verify</param>
        /// <param name="signature">The signature to verify as a string</param>
        /// <returns>True if genuine</returns>
        public bool Verify(String data, string signature)
        {
            byte[] dataBytes = Encoding.Unicode.GetBytes(data);
            byte[] signatureByteArray = Convert.FromBase64String(signature);
            return Verify(dataBytes, signatureByteArray);
        }
    }
}
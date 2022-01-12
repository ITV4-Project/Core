using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    public record Transaction {

        public const int CURRENT_VERSION = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="merkleHash">Signature/Hash of the previous transaction</param>
        /// <param name="input">The public key of the  input/sender of the transaction</param>
        /// <param name="output">The public key of the  output/reciever of the transaction</param>
        /// <param name="amount">The amount to send in the transaction</param>
        /// <param name="isDelegating">If the transaction is also delegating input</param>
        /// <param name="signature">The signature/hash of the transaction</param>
        public Transaction(byte[] merkleHash, byte[] input, byte[] output, int amount, bool isDelegating, byte[]? signature = null) :
           this(DateTimeOffset.Now, merkleHash, input, output, amount, isDelegating, signature) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="creationTime">Exact time the Transaction was created</param>
        /// <param name="merkleHash">Signature/Hash of the previous transaction</param>
        /// <param name="input">The public key of the  input/sender of the transaction</param>
        /// <param name="output">The public key of the  output/reciever of the transaction</param>
        /// <param name="amount">The amount to send in the transaction</param>
        /// <param name="isDelegating">If the transaction is also delegating input</param>
        /// <param name="signature">The signature/hash of the transaction</param>
        public Transaction(DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, int amount, bool isDelegating, byte[]? signature = null) :
           this(CURRENT_VERSION, creationTime, merkleHash, input, output, amount, isDelegating, signature) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Optional GUID of the Transaction</param>
        /// <param name="creationTime">Exact time the Transaction was created</param>
        /// <param name="merkleHash">Signature/Hash of the previous transaction</param>
        /// <param name="input">The public key of the  input/sender of the transaction</param>
        /// <param name="output">The public key of the  output/reciever of the transaction</param>
        /// <param name="amount">The amount to send in the transaction</param>
        /// <param name="isDelegating">If the transaction is also delegating input</param>
        /// <param name="signature">The signature/hash of the transaction</param>
        public Transaction(Guid id, DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, int amount, bool isDelegating, byte[]? signature = null) :
            this(id, CURRENT_VERSION, creationTime, merkleHash, input, output, amount, isDelegating, signature) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="version">Version of the Transaction</param>
        /// <param name="creationTime">Exact time the Transaction was created</param>
        /// <param name="merkleHash">Signature/Hash of the previous transaction</param>
        /// <param name="input">The public key of the  input/sender of the transaction</param>
        /// <param name="output">The public key of the  output/reciever of the transaction</param>
        /// <param name="amount">The amount to send in the transaction</param>
        /// <param name="isDelegating">If the transaction is also delegating input</param>
        /// <param name="signature">The signature/hash of the transaction</param>
        public Transaction(int version, DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, int amount, bool isDelegating, byte[]? signature = null) :
            this(new Guid(), version, creationTime, merkleHash, input, output, amount, isDelegating, signature) {}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id">Optional GUID of the Transaction</param>
        /// <param name="version">Version of the Transaction</param>
        /// <param name="creationTime">Exact time the Transaction was created</param>
        /// <param name="merkleHash">Signature/Hash of the previous transaction</param>
        /// <param name="input">The public key of the input/sender of the transaction</param>
        /// <param name="output">The public key of the  output/reciever of the transaction</param>
        /// <param name="amount">The amount to send in the transaction</param>
        /// <param name="isDelegating">If the transaction is also delegating input</param>
        /// <param name="signature">The signature/hash of the transaction</param>
        public Transaction(Guid id, int version, DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, int amount, bool isDelegating, byte[]? signature = null) {
            Id = id;
            Version = version;
            CreationTime = creationTime;
            MerkleHash = merkleHash;
            Input = input;
            Output = output;
            Amount = amount;
            IsDelegating = isDelegating;
            Signature = signature;
		}

        /// <summary>
        /// Optional GUID, Does not influence verification
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Version of the Transaction (format)
        /// </summary>
        public int Version { get; init; }
        /// <summary>
        /// Exact creation time of the Transaction
        /// </summary>
        public DateTimeOffset CreationTime { get; init; }
        /// <summary>
        /// The signature/hash of the previous transaction
        /// </summary>
        public byte[] MerkleHash { get; init; }
        /// <summary>
        /// The public key of the input/sender of the transaction
        /// </summary>
        public byte[] Input { get; init; }
        /// <summary>
        /// The public key of the output/reciever of the transaction
        /// </summary>
        public byte[] Output { get; init; }
        /// <summary>
        /// The amount to be send in the Transaction
        /// </summary>
        public int Amount { get; init; }
        /// <summary>
        /// If the Transaction is delegating
        /// </summary>
        public bool IsDelegating { get; init; }
        /// <summary>
        /// The signature/hash of the Transaction
        /// </summary>
        public byte[]? Signature { get; set; }

        private byte[]? byteArrayCache;

        /// <summary>
        /// Convert all data in the Transaction to a ByteArray. This should be Unique for each transaction
        /// </summary>
        /// <returns>a ByteArray based on the data in the transaction</returns>
        public byte[] GetSignatureByteArray() {
            if (byteArrayCache == null) {
                byte[] versionByteArray = BitConverter.GetBytes(Version);
                byte[] timeByteArray = BitConverter.GetBytes(CreationTime.ToUnixTimeSeconds());
                byte[] amountByteArray = BitConverter.GetBytes(Amount);
                byte[] delegateByteArray = BitConverter.GetBytes(IsDelegating);

                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(versionByteArray);
                    Array.Reverse(timeByteArray);
                    Array.Reverse(amountByteArray);
                    Array.Reverse(delegateByteArray);
                }

                byteArrayCache = Utility.ConcatArrays(versionByteArray, timeByteArray, MerkleHash, Input, Output, amountByteArray, delegateByteArray);
            }

            return byteArrayCache;
        }

        /// <summary>
        /// Sign the Transaction
        /// </summary>
        /// <param name="key">The key used to sign the Transaction</param>
        /// <exception cref="ArgumentException">Fired if the wrong key is used</exception>
        public void Sign(ECDsaKey key) {
            if (!key.GetPublicKey().SequenceEqual(Input)) {
                throw new ArgumentException("Transaction must be signed using the private key belonging to the input");
			}

            bool foundSignature = false;
            while (!foundSignature) {
                byte[] possibleSignatureBytes = key.Sign(GetSignatureByteArray());
                if (possibleSignatureBytes[0] == 0b00000000 && possibleSignatureBytes[1] < 0b00010000) {
                    foundSignature = true;
                    Signature = possibleSignatureBytes;
                }
            }
        }

        /// <summary>
        /// Verify the validity of the signature
        /// </summary>
        /// <returns>True if the transaction signature is valid, false otherwise</returns>
        public bool VerifySignature() {
            return VerifySignature(ECDsaKey.FromPublicKey(Input));
        }

        /// <summary>
        /// Verify the validity of the signature
        /// </summary>
        /// <param name="key">The key used to verify the Transaction</param>
        /// <returns>True if the transaction signature is valid, false otherwise</returns>
        public bool VerifySignature(ECDsaKey key) {
            if (Signature != null) {
                return key.Verify(GetSignatureByteArray(), Signature);
            }
            return false;
        }
    }
}

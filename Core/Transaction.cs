using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    public record Transaction {

        public const int CURRENT_VERSION = 1;

		/// <summary>
		/// Empty Contructor
		/// </summary>
		public Transaction() {
            MerkleHash = Array.Empty<byte>();
            Input = Array.Empty<byte>();
            Output = Array.Empty<byte>();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="merkleHash">Signature/Hash of the previous transaction</param>
        /// <param name="input">The public key of the  input/sender of the transaction</param>
        /// <param name="output">The public key of the  output/reciever of the transaction</param>
        /// <param name="amount">The amount to send in the transaction</param>
        /// <param name="isDelegating">If the transaction is also delegating input</param>
        /// <param name="signature">The signature/hash of the transaction</param>
        public Transaction(byte[] merkleHash, byte[] input, byte[] output, long amount, bool isDelegating, byte[]? signature = null) :
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
        public Transaction(DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, long amount, bool isDelegating, byte[]? signature = null) :
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
        public Transaction(Guid id, DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, long amount, bool isDelegating, byte[]? signature = null) :
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
        public Transaction(int version, DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, long amount, bool isDelegating, byte[]? signature = null) :
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
        public Transaction(Guid id, int version, DateTimeOffset creationTime, byte[] merkleHash, byte[] input, byte[] output, long amount, bool isDelegating, byte[]? signature = null) {
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
        public long Amount { get; init; }
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
        /// Convert all data and the signature in the Transaction to a ByteArray. This should be Unique for each transaction
        /// </summary>
        /// <returns>a ByteArray based on the data and the signature in the transaction</returns>
        public byte[] GetByteArray() {
            if (Signature == null) {
                return Array.Empty<byte>(); 
            } else {
                return Utility.ConcatArrays(GetSignatureByteArray(), Signature);
            }
        }

        /// <summary>
        /// Initialize a Transaction from a hex string
        /// </summary>
        /// <param name="hex">The hex string containing the transaction</param>
        /// <returns>Transaction</returns>
        public static Transaction FromHexString(string hex) {
            return Transaction.FromByteArray(Convert.FromHexString(hex));
		}

        /// <summary>
        /// Initialize a Transaction from a byte array
        /// </summary>
        /// <param name="bytes">The byte array containting the transaction</param>
        /// <returns>Transaction</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the byte array is invalid</exception>
		public static Transaction FromByteArray(byte[] bytes) {
            int bytesLength = bytes.Length;
            int remainingLength = bytes.Length - 149; // 149 is the amount of bytes already taken
            int inputOutputLength = (remainingLength / 2);

            if ((remainingLength % 2) > 0) {
                throw new ArgumentOutOfRangeException("Input and Output bytes length is not equal");
            }

            byte[] versionBytes = new byte[4];
			byte[] timeByteArray = new byte[8];
			byte[] merkleHash = new byte[64];
            byte[] input = new byte[inputOutputLength];
            byte[] output = new byte[inputOutputLength];
            byte[] amountByteArray = new byte[8];
			byte delegateByte = 0;
            byte[] signature = new byte[64];

            Array.Copy(bytes, 0, versionBytes, 0, 4);
            Array.Copy(bytes, 4, timeByteArray, 0, 8);
            Array.Copy(bytes, 12, merkleHash, 0, 64);

            Array.Copy(bytes, 76, input, 0, inputOutputLength);
            Array.Copy(bytes, 76 + inputOutputLength, output, 0, inputOutputLength);
            
            Array.Copy(bytes, bytesLength - 73, amountByteArray, 0, 8);
            delegateByte = bytes[bytesLength - 65];
            Array.Copy(bytes, bytesLength - 64, signature, 0, 64);

            return new Transaction() {
                Version = BitConverter.ToInt32(versionBytes),
                CreationTime = DateTimeOffset.FromUnixTimeSeconds(BitConverter.ToInt64(timeByteArray)),
                MerkleHash = merkleHash,
                Input = input,
                Output = output,
                Amount = BitConverter.ToInt64(amountByteArray),
                IsDelegating = Convert.ToBoolean(delegateByte),
                Signature = signature
			};
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

        internal void SignGenesis(ECDsaKey key) {
            Signature = key.Sign(GetSignatureByteArray());
        }

        /// <summary>
        /// Verify the validity of the signature
        /// </summary>
        /// <returns>True if the transaction signature is valid, false otherwise</returns>
        public bool VerifySignature() {
            if (Signature != null) {
                return ECDsaKey.FromPublicKey(Input).Verify(GetSignatureByteArray(), Signature);
            }
            return false;
        }
    }
}

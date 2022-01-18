using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    public record Block {

        public const int CURRENT_VERSION = 1;

        public Block() { }

        /// <summary>
        /// Block Constructor
        /// </summary>
        /// <param name="merkleHash">Signature/Hash of the previous Block</param>
        /// <param name="transactions">Array of transactions</param>
        /// <param name="verifier">The public key of the verifier of the block</param>
        /// <param name="signature"The signature/hash of the block></param>
        public Block(byte[] merkleHash, List<Transaction> transactions, byte[] verifier, byte[]? signature = null)
            : this(DateTimeOffset.Now, merkleHash, transactions, verifier, signature) { }

        /// <summary>
        /// Block Constructor
        /// </summary>
        /// <param name="creationTime">Exact time the Block was created</param>
        /// <param name="merkleHash">Signature/Hash of the previous Block</param>
        /// <param name="transactions">Array of transactions</param>
        /// <param name="verifier">The public key of the verifier of the block</param>
        /// <param name="signature"The signature/hash of the block></param>
        public Block(DateTimeOffset creationTime, byte[] merkleHash, List<Transaction> transactions, byte[] verifier, byte[]? signature = null)
            : this(CURRENT_VERSION, creationTime, merkleHash, transactions, verifier, signature) { }

        /// <summary>
        /// Block Constructor
        /// </summary>
        /// <param name="creationTime">Exact time the Block was created</param>
        /// <param name="version">Version of the Block</param>
        /// <param name="creationTime">Exact time the Block was created</param>
        /// <param name="merkleHash">Signature/Hash of the previous Block</param>
        /// <param name="transactions">Array of transactions</param>
        /// <param name="verifier">The public key of the verifier of the block</param>
        /// <param name="signature"The signature/hash of the block></param>
        public Block(int version, DateTimeOffset creationTime, byte[] merkleHash, List<Transaction> transactions, byte[] verifier, byte[]? signature = null)
            : this(new Guid(), version, creationTime, merkleHash, transactions, verifier, signature) { }

        /// <summary>
        /// Block Constructor
        /// </summary>
        /// <param name="id">Optional GUID of the Block</param>
        /// <param name="version">Version of the Block</param>
        /// <param name="creationTime">Exact time the Block was created</param>
        /// <param name="merkleHash">Signature/Hash of the previous Block</param>
        /// <param name="transactions">Array of transactions</param>
        /// <param name="verifier">The public key of the verifier of the block</param>
        /// <param name="signature"The signature/hash of the block></param>
        public Block(Guid id, int version, DateTimeOffset creationTime, byte[] merkleHash, List<Transaction> transactions, byte[] verifier, byte[]? signature = null) {
            Id = id;
            Version = version;
            CreationTime = creationTime;
            MerkleHash = merkleHash;
            TransactionCount = transactions.Count;
            Transactions = transactions;
            Verifier = verifier;
            Signature = signature;
		}

        /// <summary>
        /// Optional GUID, Does not influence verification
        /// </summary>
        public Guid Id { get; init; }
        /// <summary>
        /// Version of the Block (format)
        /// </summary>
        public int Version { get; init; }
        /// <summary>
        /// Exact creation time of the Block
        /// </summary>
        public DateTimeOffset CreationTime { get; init; }
        /// <summary>
        /// The signature/hash of the previous block
        /// </summary>
        public byte[] MerkleHash { get; init; }
        /// <summary>
        /// The amount of transactions in the block
        /// </summary>
        public int TransactionCount { get; init; }
        /// <summary>
        /// The public key of the input/sender of the block
        /// </summary>
        public List<Transaction> Transactions { get; init; }
        /// <summary>
        /// The public key of the verifier of the block
        /// </summary>
        public byte[] Verifier { get; init; }
        /// <summary>
        /// The signature/hash of the Block
        /// </summary>
        public byte[]? Signature { get; set; }

        private byte[]? byteArrayCache;

        /// <summary>
        /// Convert all data in the Block to a ByteArray. This should be Unique for each block
        /// </summary>
        /// <returns>a ByteArray based on the data in the block</returns>
        public byte[] GetSignatureByteArray() {
            if (byteArrayCache == null) {
                byte[] versionByteArray = BitConverter.GetBytes(Version);
                byte[] timeByteArray = BitConverter.GetBytes(CreationTime.ToUnixTimeSeconds());
                byte[] transactionByteArray = Array.Empty<byte>();
                byte[] transactionCountByteArray = BitConverter.GetBytes(TransactionCount);

                foreach (Transaction transaction in Transactions) {
                    Utility.ConcatArrays(transaction.GetByteArray());
                }

                if (!BitConverter.IsLittleEndian) {
                    Array.Reverse(versionByteArray);
                    Array.Reverse(timeByteArray);
                    Array.Reverse(transactionCountByteArray);
                }

                byteArrayCache = Utility.ConcatArrays(versionByteArray, timeByteArray, MerkleHash, transactionCountByteArray, transactionByteArray, Verifier);
            }

            return byteArrayCache;
        }

        /// <summary>
        /// Sign the Transaction
        /// </summary>
        /// <param name="key">The key used to sign the Block</param>
        /// <exception cref="ArgumentException">Fired if the wrong key is used</exception>
        public void Sign(ECDsaKey key) {
            if (!key.GetPublicKey().SequenceEqual(Verifier)) {
                throw new ArgumentException("Block must be signed using the private key belonging to the verifier");
			}

            Signature = key.Sign(GetSignatureByteArray());
        }

        /// <summary>
        /// Verify the validity of the signature
        /// </summary>
        /// <returns>True if the block signature is valid, false otherwise</returns>
        public bool VerifySignature() {
            return VerifySignature(ECDsaKey.FromPublicKey(Verifier));
        }

        /// <summary>
        /// Verify the validity of the signature
        /// </summary>
        /// <param name="key">The key used to verify the Block</param>
        /// <returns>True if the block signature is valid, false otherwise</returns>
        public bool VerifySignature(ECDsaKey key) {
            if (Signature != null) {
                return key.Verify(GetSignatureByteArray(), Signature);
            }
            return false;
        }
    }
}

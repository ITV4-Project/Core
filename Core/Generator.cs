using Core.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
    public class Generator {
        private ECDsaKey genesisKey;
        private List<ECDsaKey> keys;
        private Dictionary<ECDsaKey, byte[]> lastSignatureDictionary;
        private Random random;

        private Ledger ledger;

        public Generator(Ledger ledger, int initialKeys = 20) {
            if (initialKeys < 2) initialKeys = 2;

            genesisKey = new ECDsaKey();

            keys = new List<ECDsaKey>();
			for (int i = 0; i < initialKeys; i++) {
				keys.Add(new ECDsaKey());
			}

            lastSignatureDictionary = new Dictionary<ECDsaKey, byte[]>();

            random = new Random();

            this.ledger = ledger;
        }

        public ECDsaKey GetRandomKey() {
            return keys[random.Next(0, keys.Count)];
		}

        public Block CreateRandomBlock(int transactionCount = 10) {
            if (transactionCount < 1) transactionCount = 1;

            List<Transaction> transactions = new List<Transaction>();
			for (int i = 0; i < transactionCount; i++) {
                transactions.Add(CreateRandomTransaction());
			}

            Block lastBlock = ledger.GetLastBlock();
            if (lastBlock.Signature == null) {
                throw new Exception("Last confirmed block must have a signature");
			}

            ECDsaKey verifier = GetRandomKey();

            Block b = new Block() {
                MerkleHash = lastBlock.Signature,
                Transactions = transactions,
                Verifier = verifier.GetPublicKey()
            };
            b.Sign(verifier);
            return b;
        }

        public Block CreateDistributionBlock(int amount) {
            List<Transaction> transactions = new List<Transaction>();
            foreach(ECDsaKey key in keys) {
                transactions.Add(CreateDistributionTransaction(key, amount));
			}

            Block lastBlock = ledger.GetLastBlock();
            if (lastBlock.Signature == null) {
                throw new Exception("Last confirmed block must have a signature");
            }

            ECDsaKey verifier = GetRandomKey();

            Block b = new Block() {
                MerkleHash = lastBlock.Signature,
                Transactions = transactions,
                Verifier = verifier.GetPublicKey()
            };
            b.Sign(verifier);
            return b;
        }

        public Transaction CreateRandomTransaction() {
            ECDsaKey inputKey = GetRandomKey();
            ECDsaKey outputKey = GetRandomKey();

            while(inputKey == outputKey) {
                outputKey = GetRandomKey();
            }

            byte[] merkleHash;
            Block lastBlock = ledger.GetLastBlock();
            if (lastBlock.Signature != null) {
                merkleHash = lastBlock.Signature;
            } else {
                merkleHash = Utility.GetEmptyByteArray(64);
			}

            Transaction t = new Transaction() {
                MerkleHash = merkleHash,
                Input = inputKey.GetPublicKey(),
                Output = outputKey.GetPublicKey(),
                Amount = (random.Next(0, 100) * (long)Math.Pow(10, 8))
            };
            t.Sign(inputKey);
            if (t.Signature == null) {
                throw new NullReferenceException("Signed signature should not be null");
			}
            lastSignatureDictionary[inputKey] = t.Signature;
            return t;
		}

        public Transaction CreateDistributionTransaction(ECDsaKey output, int amount) {
            byte[] merkleHash;

            if (lastSignatureDictionary.ContainsKey(genesisKey)) {
                merkleHash = lastSignatureDictionary[genesisKey];
            } else {
                Transaction lastTransaction = ledger.GetLastTransaction(genesisKey);
                if (lastTransaction.Signature != null) {
                    merkleHash = lastTransaction.Signature;
                } else {
                    merkleHash = Utility.GetEmptyByteArray(64);
                }
            }

            Transaction t = new Transaction() {
                MerkleHash = merkleHash,
                Input = genesisKey.GetPublicKey(),
                Output = output.GetPublicKey(),
                Amount = amount * (long)Math.Pow(10, 8)
            };
            t.Sign(genesisKey);
            if (t.Signature == null) {
                throw new NullReferenceException("Signed signature should not be null");
            }
            lastSignatureDictionary[genesisKey] = t.Signature;
            return t;
        }

        public ECDsaKey GetGenesisKey() {
            return genesisKey;
		}

        public List<ECDsaKey> GetKeys() {
            return keys;
		}

        public Block GetGenesisBlock() {
            Transaction t = new Transaction() {
                MerkleHash = Utility.GetEmptyByteArray(64),
                Amount = 10000000 * (long)Math.Pow(10, 8),
                Input = Utility.ConcatArrays(new byte[] { 0x04 }, Utility.GetEmptyByteArray(64)),
                Output = genesisKey.GetPublicKey(),
                IsDelegating = false
            };
            t.SignGenesis(genesisKey);


            Block b = new Block() {
                MerkleHash = Utility.GetEmptyByteArray(64),
                TransactionCount = 1,
                Transactions = new List<Transaction>() {
                    t
                },
                Verifier = genesisKey.GetPublicKey()
            };
            b.Sign(genesisKey);
            return b;
        }
    }
}

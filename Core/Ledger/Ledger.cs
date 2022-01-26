using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Core.Database {
	public class Ledger {

		private string dbPath;

		public Ledger() {
			dbPath = CoreDbContext.GetLocalAppDataDatabase();
		}

		public void SaveBlock(Block block) {
			Block lastBlock = GetLastBlock();
			if (lastBlock.Signature != null && block.MerkleHash.SequenceEqual(lastBlock.Signature)) {
				if (VerifyBlock(block)) {
					using (var db = new CoreDbContext(dbPath)) {
						if (db.Blocks != null) {
							db.Blocks.Add(block);
							db.SaveChanges();
						} else {
							throw new Exception("An error occured saving the block to the database");
						}
					}
				} else {
					throw new Exception("Block is invalid");
				}
			} else {
				throw new Exception("Block is not a successor to the previous block");
			}
		}

		public void SaveGenesisBlock(Block block) {
			using (var db = new CoreDbContext(dbPath)) {
				if (db.Blocks != null) {
					int blockCount = db.Blocks.Count();
					if (blockCount == 0) {
						db.Blocks.Add(block);
						db.SaveChanges();
					} else {
						throw new Exception("A Genesis block already exists");
					}
				} else {
					throw new Exception("An error occured saving the block to the database");
				}
			}
		}

		public void ClearLedger() {
			using (var db = new CoreDbContext(dbPath)) {
				if (db.Blocks != null) {
					db.Database.ExecuteSqlRaw("DELETE FROM Transactions;");
					db.Database.ExecuteSqlRaw("DELETE FROM Blocks;");
					db.SaveChanges();
				}
			}
		}

		public Block GetLastBlock() {
			using (var db = new CoreDbContext(dbPath)) {
				if (db.Blocks != null ) {

					Block? block = db.Blocks.OrderByDescending(b => b.CreationTime).FirstOrDefault();
					if (block != null) {
						return block;
					}
				}
			}

			return new Block() { 
				TransactionCount = 0,
				Transactions = new List<Transaction>(),
				Verifier = Utility.ConcatArrays(new byte[] { 0x04 }, Utility.GetEmptyByteArray(32)),
				MerkleHash = Utility.GetEmptyByteArray(64),
				Signature = Utility.GetEmptyByteArray(64)
			};

		}

		public long GetBalance(ECDsaKey key) {
			return GetBalance(key.GetPublicKey());
		}
		public long GetBalance(byte[] publicKey) {
			using (var db = new CoreDbContext(dbPath)) {
				if (db.Transactions != null) {

					long send = db.Transactions.Where(t => t.Input == publicKey).Select(t => t.Amount).Sum();
					long recieved = db.Transactions.Where(t => t.Output == publicKey).Select(t => t.Amount).Sum();

					return recieved - send;
				}
			}

			return 0;
		}

		public Transaction GetLastTransaction(ECDsaKey key) {
			byte[] publicKey = key.GetPublicKey();

			using (var db = new CoreDbContext(dbPath)) {
				if (db.Transactions != null) {

					Transaction? transaction = db.Transactions.Where(t => t.Input == publicKey).OrderByDescending(t => t.CreationTime).FirstOrDefault();
					if (transaction != null) {
						return transaction;
					}
				}
			}

			return new Transaction() {
				Amount = 0,
				Input = Utility.ConcatArrays(new byte[] { 0x04 }, Utility.GetEmptyByteArray(32)),
				Output = Utility.ConcatArrays(new byte[] { 0x04 }, Utility.GetEmptyByteArray(32)),
				MerkleHash = Utility.GetEmptyByteArray(64),
				Signature = Utility.GetEmptyByteArray(64)
			};
		}

		public bool VerifyBlock(Block block) {

			if (block.VerifySignature() == false) return false;

			foreach (Transaction t in block.Transactions) {
				if (t.VerifySignature() == false) return false;

				if (GetBalance(t.Input) < t.Amount) return false;
			}

			return true;
		}
	}
}

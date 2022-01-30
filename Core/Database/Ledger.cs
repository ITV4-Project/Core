using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Database.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Core.Database {
	public class Ledger : ILedger {

		private string dbPath;

		public Ledger() {
			dbPath = CoreDbContext.GetLocalAppDataDatabase();
		}

		/// <summary>
		/// Save a new Block to the ledger
		/// </summary>
		/// <param name="block">Block to be saved to the ledger</param>
		/// <exception cref="InvalidBlockException">Thrown when the block is invalid</exception>
		/// <exception cref="InvalidSequenceException">Thrown if the block does not succeed the current newest block</exception>
		public void SaveNewBlock(Block block) {
			Block lastBlock = GetLatestBlock();
			if (lastBlock.Signature != null && block.MerkleHash.SequenceEqual(lastBlock.Signature)) {
				if (VerifyBlock(block)) {
					using (var db = new CoreDbContext(dbPath)) {
						db.Blocks.Add(block);
						db.SaveChanges();
					}
				} else {
					throw new InvalidBlockException();
				}
			} else {
				throw new InvalidSequenceException();
			}
		}

		/// <summary>
		/// Save a missing block to the ledger
		/// </summary>
		/// <param name="block">Block to be saved to the ledger</param>
		/// <exception cref="InvalidBlockException">Thrown if block is invalid</exception>
		/// <exception cref="DuplicateException"></exception>
		public void SaveMissingBlock(Block block) {
			if (!VerifyBlock(block)) throw new InvalidBlockException();
			if (IsBlockInLedger(block)) throw new DuplicateException();
			
			bool hasNextBlock = true;
			bool hasPreviousBlock = true;

			try {
				GetNextBlock(block);
			} catch (NotFoundException) {
				hasNextBlock = false;
			}

			try {
				GetPreviousBlock(block);
			} catch (NotFoundException) {
				hasPreviousBlock = false;
			}

			if (hasNextBlock || hasPreviousBlock) {
				using (var db = new CoreDbContext(dbPath)) {
					db.Blocks.Add(block);
					db.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Save a Genesis block to the ledger. (The first block of the Ledger)
		/// </summary>
		/// <param name="block">Genesis block</param>
		public void SaveGenesisBlock(Block block) {
			using (var db = new CoreDbContext(dbPath)) {
				int blockCount = db.Blocks.Count();
				if (blockCount == 0) {
					db.Blocks.Add(block);
					db.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Remove all blocks/transactions from the ledger
		/// </summary>
		public void ClearLedger() {
			using (var db = new CoreDbContext(dbPath)) {
				if (db.Blocks != null) {
					db.Database.ExecuteSqlRaw("DELETE FROM Transactions;");
					db.Database.ExecuteSqlRaw("DELETE FROM Blocks;");
					db.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Check if a Block is saved in the ledger
		/// </summary>
		/// <param name="block">Block to check</param>
		/// <returns>True if the block is saved in the ledger</returns>
		/// <exception cref="InvalidBlockException">Thrown if the block is invalid</exception>
		public bool IsBlockInLedger(Block block) {
			try {
				if (block.Signature != null) {
					GetBlock(block.Signature);
					return true;
				} else {
					throw new InvalidBlockException();
				}
			} catch (NotFoundException) {
				return false;
			}
		}

		/// <summary>
		/// Get a block from the ledger, from its signature
		/// </summary>
		/// <param name="signature">The signature of the block to get</param>
		/// <returns>The block</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetBlock(byte[] signature) {
			using (var db = new CoreDbContext()) {
				Block? result = db.Blocks.Where(x => x.Signature == signature).FirstOrDefault();
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get the next block in the ledger
		/// </summary>
		/// <param name="block">The block from which the next block is requested</param>
		/// <returns>The next block in the ledger</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetNextBlock(Block block) {
			using (var db = new CoreDbContext()) {
				Block? result = db.Blocks.Where(x => x.MerkleHash == block.Signature).FirstOrDefault();
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get the previous block in the ledger
		/// </summary>
		/// <param name="block">The block from which the previous block is requested</param>
		/// <returns>The previous block in the ledger</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetPreviousBlock(Block block) {
			using (var db = new CoreDbContext()) {
				Block? result = db.Blocks.Where(x => x.Signature == block.MerkleHash).FirstOrDefault();
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get a transaction from the ledger based on its signature
		/// </summary>
		/// <param name="signature">The signature of the transaction</param>
		/// <returns>The transaction</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetTransaction(byte[] signature) {
			using (var db = new CoreDbContext()) {
				Transaction? result = db.Transactions.Where(x => x.Signature == signature).FirstOrDefault();
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get all transactions from the ledger
		/// </summary>
		/// <returns>The transactions</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public IQueryable<Transaction> GetAllTransactions() {
			using (var db = new CoreDbContext()) {
				IQueryable<Transaction> result = db.Transactions;
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get all blocks from the ledger
		/// </summary>
		/// <returns>The blocks</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public IQueryable<Block> GetAllBlocks() {
			using (var db = new CoreDbContext()) {
				IQueryable<Block> result = db.Blocks;
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get the next transaction from the ledger
		/// </summary>
		/// <param name="transaction">The transaction from which the next will be found</param>
		/// <returns>The next transaction</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetNextTransaction(Transaction transaction) {
			using (var db = new CoreDbContext()) {
				Transaction? result = db.Transactions.Where(x => x.MerkleHash == transaction.Signature).FirstOrDefault();
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get the previous transaction from the ledger
		/// </summary>
		/// <param name="transaction">The transaction from which the previous will be found</param>
		/// <returns>The previous transaction</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetPreviousTransaction(Transaction transaction) {
			using (var db = new CoreDbContext()) {
				Transaction? result = db.Transactions.Where(x => x.Signature == transaction.MerkleHash).FirstOrDefault();
				if (result == null) throw new NotFoundException();
				return result;
			}
		}

		/// <summary>
		/// Get the latetst block based on the creationtime
		/// </summary>
		/// <returns>The latetst block</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetLatestBlock() {
			using (var db = new CoreDbContext(dbPath)) {
				Block? block = db.Blocks.OrderByDescending(b => b.CreationTime).FirstOrDefault();
				if (block == null) throw new NotFoundException();
				return block;
			}
		}

		/// <summary>
		/// Get the balance of a key
		/// </summary>
		/// <param name="key">The key for which the balance is to be found</param>
		/// <returns>The balance of the key</returns>
		public long GetBalance(ECDsaKey key) {
			return GetBalance(key.GetPublicKey());
		}

		/// <summary>
		/// Get the balance of a key
		/// </summary>
		/// <param name="key">The key for which the balance is to be found</param>
		/// <returns>The balance of the key</returns>
		public long GetBalance(byte[] publicKey) {
			using (var db = new CoreDbContext(dbPath)) {

				long send = db.Transactions.Where(t => t.Input == publicKey).Select(t => t.Amount).Sum();
				long recieved = db.Transactions.Where(t => t.Output == publicKey).Select(t => t.Amount).Sum();

				return recieved - send;
			}
		}

		/// <summary>
		/// Get the last transaction for a key
		/// </summary>
		/// <param name="key">The key for which the last transaction has to be found</param>
		/// <returns>The last transaction for a key</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetLastTransaction(ECDsaKey key) {
			byte[] publicKey = key.GetPublicKey();

			using (var db = new CoreDbContext(dbPath)) {

				Transaction? transaction = db.Transactions.Where(t => t.Input == publicKey).OrderByDescending(t => t.CreationTime).FirstOrDefault();
				if (transaction == null) {
					throw new NotFoundException();
				} else {
					return transaction;
				}
			}
		}

		/// <summary>
		/// Verify a block signature and its transactions (including signatures)
		/// </summary>
		/// <param name="block">The block to verify</param>
		/// <returns>True if valid</returns>
		public bool VerifyBlock(Block block) {

			if (block.VerifySignature() == false) return false;

			foreach (Transaction t in block.Transactions) {
				if (t.VerifySignature() == false) return false;

				if (GetBalance(t.Input) < t.Amount) return false;
			}

			return true;
		}

		/// <summary>
		/// Verify the validity of the Ledger
		/// </summary>
		/// <returns>Returns true if valid, false if invalid (blocks are missing)</returns>
		public bool VerifyLedgerIntegrity() {
			try {
				byte[]? missingBelow = FindMissingBlockBelow(GetLatestBlock());

				if (missingBelow == null) return true; else return true;
			} catch (NotFoundException) {
				return false;
			}
		}

		/// <summary>
		/// Find a Missing Block below a Block
		/// </summary>
		/// <returns>Returns the signature of the first missing block, null if there are no blocks missing</returns>
		public byte[]? FindMissingBlockBelow(Block block) {
			return FindMissingBlock(block, true);
		}

		/// <summary>
		/// Find a Missing Block above a Block
		/// </summary>
		/// <returns>Returns the signature of the first missing block, null if there are no blocks missing</returns>
		public byte[]? FindMissingBlockAbove(Block block) {
			return FindMissingBlock(block, false);
		}

		private byte[]? FindMissingBlock(Block block, bool below) {
			bool foundPrevious = false;

			try {
				if (below) {
					block = GetPreviousBlock(block);
				} else {
					block = GetNextBlock(block);
				}
				
				foundPrevious = true;
			} catch (NotFoundException) {
				return block.MerkleHash;
			}

			while (foundPrevious) {
				try {
					if (below) {
						block = GetPreviousBlock(block);
					} else {
						block = GetNextBlock(block);
					}
				} catch (NotFoundException) {
					foundPrevious = false;
				}
			}

			if (Enumerable.SequenceEqual(block.MerkleHash, Utility.GetEmptyByteArray(128))) {
				return null;
			} else {
				return block.MerkleHash;
			}
		}
	}
}

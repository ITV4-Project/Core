using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Database.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Core.Database {
	public interface ILedger {

		/// <summary>
		/// Save a new Block to the ledger
		/// </summary>
		/// <param name="block">Block to be saved to the ledger</param>
		/// <exception cref="InvalidBlockException">Thrown when the block is invalid</exception>
		/// <exception cref="InvalidSequenceException">Thrown if the block does not succeed the current newest block</exception>
		public void SaveNewBlock(Block block);

		/// <summary>
		/// Save a missing block to the ledger
		/// </summary>
		/// <param name="block">Block to be saved to the ledger</param>
		/// <exception cref="InvalidBlockException">Thrown if block is invalid</exception>
		/// <exception cref="DuplicateException"></exception>
		public void SaveMissingBlock(Block block);

		/// <summary>
		/// Save a Genesis block to the ledger. (The first block of the Ledger)
		/// </summary>
		/// <param name="block">Genesis block</param>
		public void SaveGenesisBlock(Block block);

		/// <summary>
		/// Remove all blocks/transactions from the ledger
		/// </summary>
		public void ClearLedger();

		/// <summary>
		/// Check if a Block is saved in the ledger
		/// </summary>
		/// <param name="block">Block to check</param>
		/// <returns>True if the block is saved in the ledger</returns>
		/// <exception cref="InvalidBlockException">Thrown if the block is invalid</exception>
		public bool IsBlockInLedger(Block block);

		/// <summary>
		/// Get a block from the ledger, from its signature
		/// </summary>
		/// <param name="signature">The signature of the block to get</param>
		/// <returns>The block</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetBlock(byte[] signature);

		/// <summary>
		/// Get the next block in the ledger
		/// </summary>
		/// <param name="block">The block from which the next block is requested</param>
		/// <returns>The next block in the ledger</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetNextBlock(Block block);

		/// <summary>
		/// Get the previous block in the ledger
		/// </summary>
		/// <param name="block">The block from which the previous block is requested</param>
		/// <returns>The previous block in the ledger</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetPreviousBlock(Block block);

		/// <summary>
		/// Get a transaction from the ledger based on its signature
		/// </summary>
		/// <param name="signature">The signature of the transaction</param>
		/// <returns>The transaction</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetTransaction(byte[] signature);

		/// <summary>
		/// Get all transactions from the ledger
		/// </summary>
		/// <returns>The transactions</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public IQueryable<Transaction> GetAllTransactions();

		/// <summary>
		/// Get all blocks from the ledger
		/// </summary>
		/// <returns>The blocks</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public IQueryable<Block> GetAllBlocks();

		/// <summary>
		/// Get the next transaction from the ledger
		/// </summary>
		/// <param name="transaction">The transaction from which the next will be found</param>
		/// <returns>The next transaction</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetNextTransaction(Transaction transaction);

		/// <summary>
		/// Get the previous transaction from the ledger
		/// </summary>
		/// <param name="transaction">The transaction from which the previous will be found</param>
		/// <returns>The previous transaction</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetPreviousTransaction(Transaction transaction);

		/// <summary>
		/// Get the latetst block based on the creationtime
		/// </summary>
		/// <returns>The latetst block</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Block GetLatestBlock();

		/// <summary>
		/// Get the balance of a key
		/// </summary>
		/// <param name="key">The key for which the balance is to be found</param>
		/// <returns>The balance of the key</returns>
		public long GetBalance(ECDsaKey key);

		/// <summary>
		/// Get the balance of a key
		/// </summary>
		/// <param name="key">The key for which the balance is to be found</param>
		/// <returns>The balance of the key</returns>
		public long GetBalance(byte[] publicKey);

		/// <summary>
		/// Get the last transaction for a key
		/// </summary>
		/// <param name="key">The key for which the last transaction has to be found</param>
		/// <returns>The last transaction for a key</returns>
		/// <exception cref="NotFoundException">Thrown if not found</exception>
		public Transaction GetLastTransaction(ECDsaKey key);

		/// <summary>
		/// Verify a block signature and its transactions (including signatures)
		/// </summary>
		/// <param name="block">The block to verify</param>
		/// <returns>True if valid</returns>
		public bool VerifyBlock(Block block);

		/// <summary>
		/// Verify the validity of the Ledger
		/// </summary>
		/// <returns>Returns true if valid, false if invalid (blocks are missing)</returns>
		public bool VerifyLedgerIntegrity();

		/// <summary>
		/// Find a Missing Block below a Block
		/// </summary>
		/// <returns>Returns the signature of the first missing block, null if there are no blocks missing</returns>
		public byte[]? FindMissingBlockBelow(Block block);

		/// <summary>
		/// Find a Missing Block above a Block
		/// </summary>
		/// <returns>Returns the signature of the first missing block, null if there are no blocks missing</returns>
		public byte[]? FindMissingBlockAbove(Block block);
	}
}

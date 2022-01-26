using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Database;
using Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestCore {
	[TestClass]
	public class Testledger {

		[TestMethod]
		public void Test() {
			Ledger ledger = new Ledger();
			Generator generator = new Generator(ledger);

			ledger.ClearLedger();
			ledger.SaveGenesisBlock(generator.GetGenesisBlock());

			Console.WriteLine("Created genesis block");
			Console.WriteLine("Genesis balance: {0}", ledger.GetBalance(generator.GetGenesisKey()));

			ledger.SaveBlock(generator.CreateDistributionBlock(1000));

			Console.WriteLine("Distributed funds");
			foreach (ECDsaKey key in generator.GetKeys()) {
				Console.WriteLine(ledger.GetBalance(key));
			}
			Console.WriteLine("Genesis balance: {0}", ledger.GetBalance(generator.GetGenesisKey()));

			
			ledger.SaveBlock(generator.CreateRandomBlock());

			Console.WriteLine("Generated random transactions");
			foreach (ECDsaKey key in generator.GetKeys()) {
				Console.WriteLine(ledger.GetBalance(key));
			}
			Console.WriteLine("Genesis balance: {0}", ledger.GetBalance(generator.GetGenesisKey()));
		}
	}
}

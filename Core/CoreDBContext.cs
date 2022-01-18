using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {
	public class CoreDbContext : DbContext {

		public readonly string DbPath;

		public DbSet<Transaction>? Transactions { get; set; }
		public DbSet<Block>? Blocks { get; set; }

		public CoreDbContext() : this(CoreDbContext.GetCoreDbPath()) { }

		public CoreDbContext(string dbPath) {
			this.DbPath = dbPath;

			Database.EnsureCreated();
		}

		// The following configures EF to create a Sqlite database file in the
		// special "local" folder for your platform.
		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseSqlite($"Data Source={DbPath}");

		public static string GetCoreDbPath() {
			var folder = Environment.SpecialFolder.LocalApplicationData;
			var path = Environment.GetFolderPath(folder);
			return Path.Join(path, "core.db");
		}


	}
}

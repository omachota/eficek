using System.Security.Cryptography;
using Eficek.Database.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Eficek.Database;

public class EficekDbContext(DbContextOptions<EficekDbContext> options) : DbContext(options)
{
	public DbSet<UserInternal> Users { get; init; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		var sqliteConnection = new SqliteConnectionStringBuilder { DataSource = "eficek.sqlite" };
		var connection = new SqliteConnection(sqliteConnection.ToString());

		optionsBuilder.UseSqlite(connection);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<UserInternal>(entity =>
		{
			entity.HasKey(user => user.Id);
			entity.Property(user => user.Hash).HasMaxLength(SHA256.HashSizeInBytes).IsRequired();
			entity.Property(user => user.Salt).HasMaxLength(PasswordHasher.SaltByteLength).IsRequired();
			entity.Property(user => user.UserName).HasMaxLength(64).IsRequired();
			entity.Property(user => user.Email).HasMaxLength(128).IsRequired();
		});
	}
}
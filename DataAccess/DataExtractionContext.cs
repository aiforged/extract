using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess;

public class DataExtractionContext : DbContext
{
    private readonly string _connectionString;
    public DataExtractionContext(string connectionString) : base()
    {
        _connectionString = connectionString;
    }

    // Tables
    public DbSet<Document> Document { get; set; }
    public DbSet<Definition> Defintion { get; set; }
    public DbSet<Metadata> Metadata { get; set; }
    public DbSet<Detail> Detail { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {        
        optionsBuilder.UseSqlServer(_connectionString);
    }
}



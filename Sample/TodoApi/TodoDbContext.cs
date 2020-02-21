using Microsoft.EntityFrameworkCore;

public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> Todos { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("Todos");
    }
}
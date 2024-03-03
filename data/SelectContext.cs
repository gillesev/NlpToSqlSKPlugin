using Microsoft.EntityFrameworkCore;

namespace MinimalApi.Data;

public class SelectContext : DbContext
{
    public SelectContext(DbContextOptions<SelectContext> options) : base(options)
    {
    }
}
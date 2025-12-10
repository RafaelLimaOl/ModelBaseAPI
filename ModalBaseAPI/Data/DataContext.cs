using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ModelBaseAPI.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext(options) 
{
}

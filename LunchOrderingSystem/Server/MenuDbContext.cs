using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LunchOrderingSystem.Server
{
    public class MenuDbContext : DbContext
    {
        public DbSet<OrderInfo> OrderInfo { get; set; }

        public string DbPath { get; private set; }

        public MenuDbContext(DbContextOptions<MenuDbContext> options)
           : base(options)
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}app.db";
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }

    public class OrderInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderId { get; set; }
        public int MenuId { get; set; }
        public string UserIP { get; set; }
        public DateTime OrderTime { get; set; }
    }
}

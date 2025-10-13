using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdminPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Data
{
    public class FloriaDbContext : DbContext
    {
        public FloriaDbContext(DbContextOptions<FloriaDbContext> options) : base(options) { }

        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<QuestionSet> QuestionSets => Set<QuestionSet>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ===== QuestionSet: default values & mapping =====
            mb.Entity<QuestionSet>(e =>
            {
                // Nếu trong DB đã DEFAULT GETDATE() thì vẫn để lại để đảm bảo
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.UpdatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.IsActive).HasDefaultValue(true);   // true = hiển thị
                e.Property(x => x.IsLocked).HasDefaultValue(false);  // false = không khóa

                // (Optional) Nếu muốn mặc định chỉ lấy các set đang active:
                // e.HasQueryFilter(s => s.IsActive);
            });

            // ===== Quan hệ Questions -> QuestionSets =====
            mb.Entity<Question>(e =>
            {
                e.HasOne(q => q.QuestionSet)
                 .WithMany(s => s.Questions)
                 .HasForeignKey(q => q.QuestionSetId)
                 .OnDelete(DeleteBehavior.Cascade); // Xóa set sẽ xóa luôn question thuộc set
            });

            // ===== Quan hệ Answers -> Questions =====
            mb.Entity<Answer>(e =>
            {
                e.HasOne(a => a.Question)
                 .WithMany(q => q.Answers)
                 .HasForeignKey(a => a.QuestionId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Index gợi ý cho sort và query =====
            mb.Entity<Question>()
              .HasIndex(q => new { q.QuestionSetId, q.OrderInSet });

            mb.Entity<Answer>()
              .HasIndex(a => new { a.QuestionId, a.OrderInQuestion });

            mb.Entity<Question>()
                .HasOne(q => q.QuestionSet)
                .WithMany(s => s.Questions)
                .HasForeignKey(q => q.QuestionSetId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);


            base.OnModelCreating(mb);
        }

        // Auto update UpdatedAt khi sửa QuestionSet
        public override int SaveChanges()
        {
            TouchTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            TouchTimestamps();
            return base.SaveChangesAsync(ct);
        }

        private void TouchTimestamps()
        {
            var now = DateTime.Now; // hoặc DateTime.UtcNow nếu bạn chuẩn UTC
            foreach (var entry in ChangeTracker.Entries<QuestionSet>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
        }
    }
}

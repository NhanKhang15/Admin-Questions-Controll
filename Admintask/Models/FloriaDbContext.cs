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

        // Questions
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<Answer> Answers => Set<Answer>();
        public DbSet<QuestionSet> QuestionSets => Set<QuestionSet>();

        // Content Management
        public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Expert> Experts => Set<Expert>();

        // Videos
        public DbSet<Video> Videos => Set<Video>();
        public DbSet<VideoCategory> VideoCategories => Set<VideoCategory>();
        public DbSet<VideoTag> VideoTags => Set<VideoTag>();
        public DbSet<VideoStats> VideoStats => Set<VideoStats>();

        // Posts
        public DbSet<Post> Posts => Set<Post>();
        public DbSet<PostCategory> PostCategories => Set<PostCategory>();
        public DbSet<PostTag> PostTags => Set<PostTag>();
        public DbSet<PostStats> PostStats => Set<PostStats>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ===== QuestionSet: default values & mapping =====
            mb.Entity<QuestionSet>(e =>
            {
                e.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.UpdatedAt).HasDefaultValueSql("GETDATE()");
                e.Property(x => x.IsActive).HasDefaultValue(true);
                e.Property(x => x.IsLocked).HasDefaultValue(false);
            });

            // ===== Quan hệ Questions -> QuestionSets =====
            mb.Entity<Question>(e =>
            {
                e.HasOne(q => q.QuestionSet)
                 .WithMany(s => s.Questions)
                 .HasForeignKey(q => q.QuestionSetId)
                 .OnDelete(DeleteBehavior.Cascade);
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

            // ===== Video Relationships =====
            mb.Entity<Video>(e =>
            {
                e.HasOne(v => v.Expert)
                 .WithMany(ex => ex.Videos)
                 .HasForeignKey(v => v.ExpertId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(v => v.Stats)
                 .WithOne(s => s.Video)
                 .HasForeignKey<VideoStats>(s => s.VideoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // VideoCategory composite key
            mb.Entity<VideoCategory>(e =>
            {
                e.HasKey(vc => new { vc.VideoId, vc.CategoryId });
                e.HasOne(vc => vc.Video)
                 .WithMany(v => v.VideoCategories)
                 .HasForeignKey(vc => vc.VideoId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(vc => vc.Category)
                 .WithMany(c => c.VideoCategories)
                 .HasForeignKey(vc => vc.CategoryId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // VideoTag composite key
            mb.Entity<VideoTag>(e =>
            {
                e.HasKey(vt => new { vt.VideoId, vt.TagId });
                e.HasOne(vt => vt.Video)
                 .WithMany(v => v.VideoTags)
                 .HasForeignKey(vt => vt.VideoId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(vt => vt.Tag)
                 .WithMany(t => t.VideoTags)
                 .HasForeignKey(vt => vt.TagId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Post Relationships =====
            mb.Entity<Post>(e =>
            {
                e.HasOne(p => p.Expert)
                 .WithMany(ex => ex.Posts)
                 .HasForeignKey(p => p.ExpertId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasOne(p => p.Stats)
                 .WithOne(s => s.Post)
                 .HasForeignKey<PostStats>(s => s.PostId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // PostCategory composite key
            mb.Entity<PostCategory>(e =>
            {
                e.HasKey(pc => new { pc.PostId, pc.CategoryId });
                e.HasOne(pc => pc.Post)
                 .WithMany(p => p.PostCategories)
                 .HasForeignKey(pc => pc.PostId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(pc => pc.Category)
                 .WithMany(c => c.PostCategories)
                 .HasForeignKey(pc => pc.CategoryId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // PostTag composite key
            mb.Entity<PostTag>(e =>
            {
                e.HasKey(pt => new { pt.PostId, pt.TagId });
                e.HasOne(pt => pt.Post)
                 .WithMany(p => p.PostTags)
                 .HasForeignKey(pt => pt.PostId)
                 .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(pt => pt.Tag)
                 .WithMany(t => t.PostTags)
                 .HasForeignKey(pt => pt.TagId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

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
            var now = DateTime.Now;
            foreach (var entry in ChangeTracker.Entries<QuestionSet>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
            }
            foreach (var entry in ChangeTracker.Entries<Video>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
            }
            foreach (var entry in ChangeTracker.Entries<Post>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
            }
        }
    }
}

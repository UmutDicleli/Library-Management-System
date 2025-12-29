using Microsoft.EntityFrameworkCore;

namespace MyLibrary
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        public DbSet<YayinEvi> YayinEvis { get; set; }
        public DbSet<BookPublish> BookPublishes { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<RentBook> RentBooks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Member>()
                .HasIndex(u => u.MemberPhoneNumber)
                .IsUnique();

            modelBuilder.Entity<Member>()
                .HasIndex(u => u.MemberEmail)
                .IsUnique();

            modelBuilder.Entity<Member>()
                .HasIndex(u => u.MemberLastName)
                .IsUnique();

            modelBuilder.Entity<Author>()
                .HasIndex(u => u.AuthorName)
                .IsUnique();

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Kitaplar)
                .HasForeignKey(b => b.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookPublish>()
                .HasOne(bp => bp.YayinEvi)
                .WithMany(ye => ye.BookPublishes)
                .HasForeignKey(bp => bp.YayinEviFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookPublish>()
                .HasOne(bp => bp.Book)
                .WithMany(b => b.Publishes)
                .HasForeignKey(bp => bp.BookFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RentBook>()
                .HasKey(rb => rb.BookPublishFK);

            modelBuilder.Entity<RentBook>()
                .HasOne(rb => rb.Member)
                .WithMany(m => m.RentedBooks)
                .HasForeignKey(rb => rb.MemberFK)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RentBook>()
                .HasOne(rb => rb.BookPublish)
                .WithOne(bp => bp.RentBook)
                .HasForeignKey<RentBook>(rb => rb.BookPublishFK)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

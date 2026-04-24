using Microsoft.EntityFrameworkCore;
using System;

namespace MyRoomii
{
    // ============================================
    // EF Core Database Context
    // ============================================
    
    public class MyRoomiiContext : DbContext
    {
        // DbSets matching all tables from group59_p2.sql
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserPreference> UserPreferences { get; set; } = null!;
        public DbSet<Verification> Verifications { get; set; } = null!;
        public DbSet<Landlord> Landlords { get; set; } = null!;
        public DbSet<RoomListing> RoomListings { get; set; } = null!;
        public DbSet<RoomAmenities> RoomAmenities { get; set; } = null!;
        public DbSet<RoommateMatch> RoommateMatches { get; set; } = null!;
        public DbSet<Chat> Chats { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;

        public MyRoomiiContext() { }

        public MyRoomiiContext(DbContextOptions<MyRoomiiContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Default connection string - can be overridden
                optionsBuilder.UseSqlServer("Server=localhost,1433;Database=myRoomii;User Id=sa;Password=Test123!@#;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map table names exactly as in SQL
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserPreference>().ToTable("UserPreference");
            modelBuilder.Entity<Verification>().ToTable("Verification");
            modelBuilder.Entity<Landlord>().ToTable("Landlord");
            modelBuilder.Entity<RoomListing>().ToTable("RoomListing");
            modelBuilder.Entity<RoomAmenities>().ToTable("RoomAmenities");
            modelBuilder.Entity<RoommateMatch>().ToTable("RoommateMatch");
            modelBuilder.Entity<Chat>().ToTable("Chat");
            modelBuilder.Entity<Message>().ToTable("Message");
            modelBuilder.Entity<Review>().ToTable("Review");

            // Configure primary keys
            modelBuilder.Entity<User>().HasKey(u => u.UserID);
            modelBuilder.Entity<UserPreference>().HasKey(up => up.UserID);
            modelBuilder.Entity<Verification>().HasKey(v => v.UserID);
            modelBuilder.Entity<Landlord>().HasKey(l => l.UserID);
            modelBuilder.Entity<RoomListing>().HasKey(rl => rl.ListingID);
            modelBuilder.Entity<RoomAmenities>().HasKey(ra => ra.ListingID);
            modelBuilder.Entity<RoommateMatch>().HasKey(rm => rm.MatchID);
            modelBuilder.Entity<Chat>().HasKey(c => c.ChatID);
            modelBuilder.Entity<Message>().HasKey(m => m.MessageID);
            modelBuilder.Entity<Review>().HasKey(r => r.ReviewID);

            // Configure relationships (matching foreign keys from group59_p2.sql)
            
            // UserPreference -> User (CASCADE DELETE)
            modelBuilder.Entity<UserPreference>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<UserPreference>(up => up.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Verification -> User (CASCADE DELETE)
            modelBuilder.Entity<Verification>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Verification>(v => v.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Landlord -> User (CASCADE DELETE)
            modelBuilder.Entity<Landlord>()
                .HasOne<User>()
                .WithOne()
                .HasForeignKey<Landlord>(l => l.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomListing -> Landlord (NO ACTION)
            modelBuilder.Entity<RoomListing>()
                .HasOne<Landlord>()
                .WithMany()
                .HasForeignKey(rl => rl.LandlordUserID)
                .OnDelete(DeleteBehavior.NoAction);

            // RoomAmenities -> RoomListing (CASCADE DELETE)
            modelBuilder.Entity<RoomAmenities>()
                .HasOne<RoomListing>()
                .WithOne()
                .HasForeignKey<RoomAmenities>(ra => ra.ListingID)
                .OnDelete(DeleteBehavior.Cascade);

            // RoommateMatch -> User (NO ACTION for both)
            modelBuilder.Entity<RoommateMatch>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(rm => rm.UserID1)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RoommateMatch>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(rm => rm.UserID2)
                .OnDelete(DeleteBehavior.NoAction);

            // RoommateMatch -> RoomListing (NO ACTION)
            modelBuilder.Entity<RoommateMatch>()
                .HasOne<RoomListing>()
                .WithMany()
                .HasForeignKey(rm => rm.ListingID)
                .OnDelete(DeleteBehavior.NoAction);

            // Chat -> User (NO ACTION for both)
            modelBuilder.Entity<Chat>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.User1ID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chat>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.User2ID)
                .OnDelete(DeleteBehavior.NoAction);

            // Message -> Chat (CASCADE DELETE)
            modelBuilder.Entity<Message>()
                .HasOne<Chat>()
                .WithMany()
                .HasForeignKey(m => m.ChatID)
                .OnDelete(DeleteBehavior.Cascade);

            // Message -> User (NO ACTION)
            modelBuilder.Entity<Message>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.SenderID)
                .OnDelete(DeleteBehavior.NoAction);

            // Review -> User (NO ACTION for Author)
            modelBuilder.Entity<Review>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.AuthorUserID)
                .OnDelete(DeleteBehavior.NoAction);

            // Review -> Landlord (NO ACTION)
            modelBuilder.Entity<Review>()
                .HasOne<Landlord>()
                .WithMany()
                .HasForeignKey(r => r.LandlordUserID)
                .OnDelete(DeleteBehavior.NoAction);

            // Review -> RoomListing (SET NULL)
            modelBuilder.Entity<Review>()
                .HasOne<RoomListing>()
                .WithMany()
                .HasForeignKey(r => r.ListingID)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}


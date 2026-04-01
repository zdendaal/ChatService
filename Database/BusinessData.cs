using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using VideoStreamingService.Models;


namespace VideoStreamingService.Database
{
    public class BusinessData : DbContext
    {
        public BusinessData(DbContextOptions<BusinessData> options) : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Chat> Chats { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<ChatMember> ChatMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // user table configuration
            modelBuilder.Entity<User>().HasKey(x => x.Id);
            modelBuilder.Entity<User>().HasMany(x => x.Chats).WithOne(x => x.User);
            modelBuilder.Entity<User>().Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique(true);
            modelBuilder.Entity<User>().HasIndex(x => x.Nickname).IsUnique(true);
            modelBuilder.Entity<User>().Property(x => x.Email).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.Nickname).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.Country).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.passwordHash).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.ProfilePictureUrl).IsRequired(true);

            // chat table configuratoin
            modelBuilder.Entity<Chat>().HasKey(x => x.Id);
            modelBuilder.Entity<Chat>().HasMany(x => x.Messages).WithOne(x => x.Chat);
            modelBuilder.Entity<Chat>().HasMany(x => x.Members).WithOne(x => x.Chat);
            modelBuilder.Entity<Chat>().Property(x => x.Name).IsRequired(true).HasMaxLength(BusinessSettings.s_maxChatNameLength);
            modelBuilder.Entity<Chat>().HasMany(x => x.Messages).WithOne(x => x.Chat).HasForeignKey(x => x.Chat).OnDelete(DeleteBehavior.Cascade);

            // message table configuration
            modelBuilder.Entity<Message>().Property(x => x.Content).IsRequired(true).HasMaxLength(BusinessSettings.s_maxMessageContentLength);
            modelBuilder.Entity<Message>().Property(x => x.Timestamp).HasDefaultValueSql("CURRENT_TIMESTAMP");
            modelBuilder.Entity<Message>().HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.Sender).OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}

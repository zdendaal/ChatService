using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ChatService.Models;


namespace ChatService.Database
{
    public class BusinessData : DbContext
    {
        public BusinessData(DbContextOptions<BusinessData> options) : base(options)
        {
            Database.EnsureCreated();
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
            modelBuilder.Entity<User>().Property(x => x.CreatedAt);
            modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique(true);
            modelBuilder.Entity<User>().HasIndex(x => x.Nickname).IsUnique(true);
            modelBuilder.Entity<User>().Property(x => x.Email).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.Nickname).IsRequired(true).HasMaxLength(BusinessSettings.nicknameMaxLength);
            modelBuilder.Entity<User>().Property(x => x.Country).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.passwordHash).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.ProfilePictureUrl).IsRequired(true);
            modelBuilder.Entity<User>().Property(x => x.Role).IsRequired(true);
            modelBuilder.Entity<User>().HasMany(x => x.Chats).WithOne(x => x.User).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

            // chat table configuratoin
            modelBuilder.Entity<Chat>().HasKey(x => x.Id);
            modelBuilder.Entity<Chat>().HasMany(x => x.Members).WithOne(x => x.Chat).HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Chat>().Property(x => x.Name).IsRequired(true).HasMaxLength(BusinessSettings.chatNameMaxLength);
            modelBuilder.Entity<Chat>().HasMany(x => x.Messages).WithOne(x => x.Chat).HasForeignKey(x => x.ChatId).OnDelete(DeleteBehavior.Cascade);

            // message table configuration
            modelBuilder.Entity<Message>().HasKey(x => x.Id);
            modelBuilder.Entity<Message>().Property(x => x.Content).IsRequired(true).HasMaxLength(BusinessSettings.messageNameMaxLength);
            modelBuilder.Entity<Message>().Property(x => x.Timestamp);
            modelBuilder.Entity<Message>().HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);

            // chat member table configuration
            modelBuilder.Entity<ChatMember>().HasKey(x => new { x.UserId, x.ChatId });
            modelBuilder.Entity<ChatMember>().Property(x => x.Role).IsRequired(true);
            modelBuilder.Entity<ChatMember>().Property(x => x.JoinedAt).IsRequired(true);
            modelBuilder.Entity<ChatMember>().Property(x => x.LastSeen).IsRequired(true);

            base.OnModelCreating(modelBuilder);
        }
    }
}

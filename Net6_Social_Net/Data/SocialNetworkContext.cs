using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Net6_Social_Net.Data
{
    public partial class SocialNetworkContext : DbContext
    {
        public SocialNetworkContext()
        {
        }

        public SocialNetworkContext(DbContextOptions<SocialNetworkContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comments { get; set; } = null!;
        public virtual DbSet<Event> Events { get; set; } = null!;
        public virtual DbSet<Friend> Friends { get; set; } = null!;
        public virtual DbSet<Message> Messages { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Post> Posts { get; set; } = null!;
        public virtual DbSet<SearchHistory> SearchHistories { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserSetting> UserSettings { get; set; } = null!;
        public virtual DbSet<VerifyCode> VerifyCodes { get; set; } = null!;

        /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
         {
             if (!optionsBuilder.IsConfigured)
             {
 #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                 optionsBuilder.UseSqlServer("Data Source=MSI;Initial Catalog=SocialNetwork;Persist Security Info=True;User ID=Tuan;Password=123;Encrypt=True;Trust Server Certificate=True");
             }
         }*/
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Kiểm tra nếu DbContext chưa được cấu hình thông qua Dependency Injection
            if (!optionsBuilder.IsConfigured)
            {
                // Tạo cấu hình để đọc file appsettings.json
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();

                // Lấy chuỗi kết nối từ appsettings.json và cấu hình DbContext
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.CommentId).HasColumnName("CommentID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.PostId)
                    .HasConstraintName("FK__Comments__PostID__534D60F1");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Comments__UserID__5441852A");
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.Property(e => e.EventId).HasColumnName("EventID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.EventDate).HasColumnType("date");

                entity.Property(e => e.EventTitle).HasMaxLength(100);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Events)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Events__UserID__797309D9");
            });

            modelBuilder.Entity<Friend>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.FriendId })
                    .HasName("PK__Friends__3DA43AFA29965EC1");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.FriendId).HasColumnName("FriendID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .HasDefaultValueSql("('Pending')");

                entity.HasOne(d => d.FriendNavigation)
                    .WithMany(p => p.FriendFriendNavigations)
                    .HasForeignKey(d => d.FriendId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Friends__FriendI__04E4BC85");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.FriendUsers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Friends__UserID__03F0984C");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.Property(e => e.MessageId).HasColumnName("MessageID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ReceiverId).HasColumnName("ReceiverID");

                entity.Property(e => e.SenderId).HasColumnName("SenderID");

                entity.HasOne(d => d.Receiver)
                    .WithMany(p => p.MessageReceivers)
                    .HasForeignKey(d => d.ReceiverId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Messages__Receiv__75A278F5");

                entity.HasOne(d => d.Sender)
                    .WithMany(p => p.MessageSenders)
                    .HasForeignKey(d => d.SenderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Messages__Sender__74AE54BC");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.NotificationId).HasColumnName("NotificationID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsRead).HasDefaultValueSql("((0))");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Notificat__UserI__59063A47");
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(e => e.PostId).HasColumnName("PostID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(255)
                    .HasColumnName("ImageURL");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("status")
                    .HasDefaultValueSql("('Public')");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Posts)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Posts__UserID__4F7CD00D");
            });

            modelBuilder.Entity<SearchHistory>(entity =>
            {
                entity.HasKey(e => e.SearchId)
                    .HasName("PK__SearchHi__21C53514FB2DD8C8");

                entity.ToTable("SearchHistory");

                entity.Property(e => e.SearchId).HasColumnName("SearchID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Query).HasMaxLength(255);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SearchHistories)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__SearchHis__UserI__08B54D69");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username, "UQ__Users__536C85E4731EF1D4")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "UQ__Users__A9D105348242AAC4")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.PasswordHash).HasMaxLength(255);

                entity.Property(e => e.PhoneNumber).HasMaxLength(15);

                entity.Property(e => e.ProfilePicture).HasMaxLength(255);

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("('Users')");

                entity.Property(e => e.Username).HasMaxLength(50);
            });

            modelBuilder.Entity<UserSetting>(entity =>
            {
                entity.HasKey(e => e.SettingId)
                    .HasName("PK__UserSett__54372AFD77266FE3");

                entity.Property(e => e.SettingId).HasColumnName("SettingID");

                entity.Property(e => e.NotificationSetting).HasDefaultValueSql("((1))");

                entity.Property(e => e.PrivacySetting)
                    .HasMaxLength(15)
                    .HasDefaultValueSql("('Public')");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserSettings)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__UserSetti__UserI__114A936A");
            });

            modelBuilder.Entity<VerifyCode>(entity =>
            {
                entity.HasKey(e => e.VerifyId)
                    .HasName("PK__VerifyCo__0A2710A90C9A5EF9");

                entity.Property(e => e.VerifyId).HasColumnName("VerifyID");

                entity.Property(e => e.Gmail).HasMaxLength(100);

                entity.Property(e => e.IsVerify).HasDefaultValueSql("((0))");

                entity.Property(e => e.SendDate).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.VerifyCode1)
                    .HasMaxLength(255)
                    .HasColumnName("VerifyCode");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

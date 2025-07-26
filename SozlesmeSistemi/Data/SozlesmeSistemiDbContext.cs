using Microsoft.EntityFrameworkCore;
using SozlesmeSistemi.Models; // Models klasöründeki entity


namespace SozlesmeSistemi.Data
{
    public class SozlesmeSistemiDbContext : DbContext
    {

        public SozlesmeSistemiDbContext(DbContextOptions<SozlesmeSistemiDbContext> options) : base(options)
        {
        }

        // Tablolar 
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentStatusHistory> DocumentStatusHistories { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Backup> Backups { get; set; }

        public DbSet<ContractRequest> ContractRequests { get; set; }

        public DbSet<Unit> Units { get; set; } // Doğru yazım ve isimlendirme
        public DbSet<UserHierarchy> UserHierarchies { get; set; }
        public DbSet<ContractStatusHistory> ContractStatusHistories { get; set; }

        public DbSet<Notification> Notifications { get; set; } // Bildirimler tablosu

        public DbSet<ContractSignature> ContractSignatures { get; set; } // Added

        public DbSet<ContractSigners> ContractSigners { get; set; } // Bu satır mutlaka olmalı
        public DbSet<ContractParaf> ContractParafs { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=EBRU\\SQLEXPRESS;Database=SozlesmeSistemiDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Contract>()
                .Property(c => c.SozlesmeTutari)
                .HasPrecision(18, 2);

            // Composite Key Tanımlama (UserId + RoleId)
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // User ile UserRole ilişkisi
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            // Role ile UserRole ilişkisi
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // DocumentStatusHistory için kaskad ayarını değiştir
            modelBuilder.Entity<DocumentStatusHistory>()
                .HasOne(d => d.ChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);


          

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.ContractRequest)
                .WithMany() // Eğer ContractRequest içinde ICollection<Contract> eklemeyeceksen WithMany() boş kalabilir
                .HasForeignKey(c => c.ContractRequestId)
                .OnDelete(DeleteBehavior.SetNull); // veya Restrict// Document ile DocumentStatusHistory ilişkisi     

            //edanın kısımları
            modelBuilder.Entity<Document>()
                .Property(d => d.Description)
                .HasMaxLength(1000); // Örnek

            modelBuilder.Entity<DocumentStatusHistory>()
                .Property(h => h.Description)
                .HasMaxLength(1000); // Örnek

            //ebrunun kısımları

            // Reminder -> Contract (Hatırlatmaların sözleşme ile ilişkisi)
            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.Contract)  // Her hatırlatma bir sözleşme ile ilişkilidir
                .WithMany()                // Bir sözleşme birden fazla hatırlatmaya sahip olabilir
                .HasForeignKey(r => r.ContractId) // Hatırlatmanın ContractId'si ile ilişkili
                .OnDelete(DeleteBehavior.Cascade); // Hatırlatma silindiğinde, ilişkili sözleşme silinmesin


            //revize
            //contractrequest
            modelBuilder.Entity<ContractRequest>()
              .HasOne(cr => cr.RequestedByUser)
              .WithMany()
              .HasForeignKey(cr => cr.RequestedById)
              .OnDelete(DeleteBehavior.Restrict);  // veya NoAction

            modelBuilder.Entity<ContractRequest>()
                .HasOne(cr => cr.RequestedToUser)
                .WithMany()
                .HasForeignKey(cr => cr.RequestedToId)
                .OnDelete(DeleteBehavior.Restrict);  // veya NoAction


            modelBuilder.Entity<ContractRequest>()
                .HasOne(cr => cr.AssignedToUser)
                .WithMany() // ya da User sınıfında ICollection<ContractRequest> AssignedRequests varsa onu yazabilirsin
                .HasForeignKey(cr => cr.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);


            //unit
            // Unit ile ilişkiler
            modelBuilder.Entity<User>()
                .HasOne(u => u.Unit)
                .WithMany(u => u.Users)
                .HasForeignKey(u => u.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.OurUnit)
                .WithMany(u => u.OurContracts)
                .HasForeignKey(c => c.OurUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasOne(c => c.CounterUnit)
                .WithMany(u => u.CounterContracts)
                .HasForeignKey(c => c.CounterUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            //userhierarcy
            // UserHierarchy ilişkilerini şöyle güncelleyin:
            modelBuilder.Entity<UserHierarchy>()
                .HasOne(uh => uh.Manager)
                .WithMany(u => u.ManagedHierarchies)
                .HasForeignKey(uh => uh.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserHierarchy>()
                .HasOne(uh => uh.Subordinate)
                .WithMany(u => u.SubordinateHierarchies)
                .HasForeignKey(uh => uh.SubordinateId)
                .OnDelete(DeleteBehavior.Restrict);

            // ContractStatusHistory ilişkisini şöyle güncelleyin:
            // ContractStatusHistory ilişkileri
            modelBuilder.Entity<ContractStatusHistory>()
                .HasOne(d => d.Contract)
                .WithMany()
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractStatusHistory>()
                .HasOne(d => d.ChangedByUser)
                .WithMany()
                .HasForeignKey(d => d.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction); // Kaskad silmeyi kapat

            // Notification ilişkileri
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany(u => u.SentNotifications)
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(u => u.ReceivedNotifications)
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            //DÖNENİN EKLEDİĞİ KOD BİTİŞ

            modelBuilder.Entity<ContractRequest>()
                 .HasOne(cr => cr.Unit)
                 .WithMany(u => u.ContractRequests)
                 .HasForeignKey(cr => cr.UnitId)
                 .OnDelete(DeleteBehavior.Restrict); // Opsiyonel: Birimi silindiğinde ilgili taleplerin ne olacağı




            // ContractSigners ilişkileri
            modelBuilder.Entity<ContractSigners>()
                .HasOne(cs => cs.Contract)
                .WithMany(c => c.ContractSigners)
                .HasForeignKey(cs => cs.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractSigners>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContractSigners>()
                .HasOne(cs => cs.Unit)
                .WithMany()
                .HasForeignKey(cs => cs.UnitId)
                .OnDelete(DeleteBehavior.Restrict);





            modelBuilder.Entity<Contract>()
    .HasOne(c => c.Manager)
    .WithMany()
    .HasForeignKey(c => c.ManagerId)
    .OnDelete(DeleteBehavior.Restrict)
    .IsRequired(false); // ManagerId nullable, zorunlu değil





            // ContractSignature ilişkisi (Hata düzeltmesi için eklenen/güncellenen kısım)
            modelBuilder.Entity<ContractSignature>()
                .HasOne(cs => cs.Contract)
                .WithMany(c => c.ContractSignatures) // Contract modelindeki ContractSignatures koleksiyonuyla eşleşiyor
                .HasForeignKey(cs => cs.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractSignature>()
                .HasOne(cs => cs.User)
                .WithMany()
                .HasForeignKey(cs => cs.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ContractParaf ilişkileri
            modelBuilder.Entity<ContractParaf>()
                .HasOne(p => p.Contract)
                .WithMany(c => c.ContractParafs)
                .HasForeignKey(p => p.ContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContractParaf>()
                .HasOne(p => p.ParaflayanUser)
                .WithMany()
                .HasForeignKey(p => p.ParaflayanUserId)
                .OnDelete(DeleteBehavior.Restrict);




            //BEN EKLEDİĞİM KOD BAŞLANGIÇ 1
            modelBuilder.Entity<Notification>()
                 .HasOne(n => n.Contract)
                 .WithMany()
                 .HasForeignKey(n => n.ContractId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.ContractRequest)
                .WithMany()
                .HasForeignKey(n => n.ContractRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            //BEN EKLEDİĞİM KOD BİTİŞ 1
        }
    }
}

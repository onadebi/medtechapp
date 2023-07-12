using MedTechAPI.Domain.Entities;
using MedTechAPI.Domain.Entities.PatientEntitites;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Entities.ProfileManagement.ProfileManagement;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Persistence.ModelBuilders;
using Microsoft.EntityFrameworkCore;

namespace MedTechAPI.Persistence
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.SeedBuilder();
        }

        public DbSet<EmploymentStatus> EmploymentStatus { get; set; }
        public DbSet<AppActivityLog> AppActivityLogs { get; set; }
        public DbSet<BranchDetail> BranchDetails { get; set; }
        public DbSet<MedicCompanyDetail> MedicCompanyDetails { get; set; }
        public DbSet<CountryDetail> CountryDetails { get; set; }
        public DbSet<CurrencyDetail> CurrencyDetails { get; set; }
        public DbSet<EmailConfig> EmailConfigs { get; set; }
        public DbSet<EmploymentSector> EmploymentSectors { get; set; }
        public DbSet<GenderCategory> GenderCategories { get; set; }
        public DbSet<IdentificationType> IdentificationTypes { get; set; }
        public DbSet<MaritalStatusDetail> MaritalStatusDetails { get; set; }
        public DbSet<MessageBox> MessageBox { get; set; }
        public DbSet<ResidentAccomodationDetail> ResidentAccomodationDetails { get; set; }
        public DbSet<Salutation> Salutations { get; set; }
        public DbSet<StateDetail> StateDetails { get; set; }
        //public DbSet<UserAddress> UserAddress { get; set; }
        //public DbSet<UserNextOfKin> UserNextOfKins { get; set; }
        public DbSet<UserDocument> UserDocuments { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserProfileGroup> UserProfileGroups { get; set; }
        public DbSet<UserGroup> UserGroup { get; set; }



        #region MENU PERSMISSIONS
        public DbSet<MenuController> MenuController { get; set; }
        public DbSet<MenuControllerActions> MenuControllerActions { get; set; }
        //public DbSet<UserGroupMenuControllerActionPermissions> UserGroupMenuControllerActionPermissions { get; set; }
        #endregion

        #region PATIENTS
        public DbSet<PatientCategory> PatientCategory { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        #endregion

    }
}

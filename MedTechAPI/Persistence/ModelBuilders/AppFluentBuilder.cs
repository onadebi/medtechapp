using MedTechAPI.Domain.Entities.PatientEntitites;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using Microsoft.EntityFrameworkCore;

namespace MedTechAPI.Persistence.ModelBuilders
{
    public static class AppFluentBuilder
    {
        public static ModelBuilder SeedBuilder(this ModelBuilder model)
        {

            model.Entity<PatientProfile>(prop =>
            {
                prop.HasMany<PatientNextOfKin>(u => u.PatientNextOfKins).WithOne(p => p.PatientProfile).HasForeignKey(f => f.PatienId).OnDelete(DeleteBehavior.Restrict);
                prop.HasOne<BranchDetail>(u => u.BranchDetail).WithMany(p => p.PatientProfile).HasForeignKey(f => f.MedicBranchId).OnDelete(DeleteBehavior.Restrict);
                prop.HasIndex(i => i.PatientUserGuid, name: "ix_PatientProfile_PatientUserGuid").IsUnique();
            });

            model.Entity<PatientCategory>(prop =>
            {
                prop.HasIndex(u => u.CategoryName, name: "ix_PatientCategory_CategoryName").IsUnique();
            });           

            model.Entity<GenderCategory>(prop =>
            {
                prop.HasMany<UserProfile>(u => u.UserProfile).WithOne(p => p.GenderCategory).HasForeignKey(f => f.GenderCategoryId).OnDelete(DeleteBehavior.Restrict);
            });


            model.Entity<UserProfile>(prop =>
            {
                prop.HasMany<UserProfileGroup>(u => u.UserProfileGroups).WithOne(p => p.UserProfile).HasForeignKey(f => f.UserProfileId).OnDelete(DeleteBehavior.Restrict);
                prop.HasOne<BranchDetail>(u=> u.BranchDetail).WithMany(p=> p.UserProfile).HasForeignKey(f=> f.MedicBranchId).OnDelete(DeleteBehavior.Restrict);
                prop.HasOne<Salutation>(u=> u.Salutation).WithMany(p=> p.UserProfile).HasForeignKey(f=> f.SalutationId).OnDelete(DeleteBehavior.Restrict);
                prop.HasMany<UserNextOfKin>(u => u.UserNextOfKin).WithOne(p => p.UserProfile).HasForeignKey(f => f.UserProfileId).OnDelete(DeleteBehavior.Restrict);
            });

             model.Entity<UserGroup>(prop =>
            {
                prop.HasIndex(u => new { u.GroupName, u.CompanyId }, name: "ix_UserGroup_RoleName_UniqueIndex").IsUnique();
                //prop.HasMany<UserGroupMenuControllerActionPermissions>(u => u.UserGroupMenuControllerActionPermissions).WithOne(p => p.UserGroup).HasForeignKey(f => f.UserGroupId).OnDelete(DeleteBehavior.Restrict);
                prop.HasMany<UserProfileGroup>(u => u.UserProfileGroups).WithOne(p => p.UserGroups).HasForeignKey(f => f.UserGroupId).OnDelete(DeleteBehavior.Restrict);
            });
            model.Entity<UserProfileGroup>(prop =>
            {
                prop.HasIndex(u => new { u.UserProfileId, u.UserGroupId,  u.CompanyId }, name: "ix_UserProfileGroup_UserProfileIdUserGroupId_CompositeUniqueIndex").IsUnique();                
            });
            model.Entity<MenuController>(prop =>
            {
                prop.HasIndex(u => new { u.ControllerCode, u.CompanyId }, name: "ix_MenuController_ControllerCode").IsUnique();
                prop.HasMany<MenuControllerActions>(u => u.MenuControllerActions).WithOne(p => p.MenuController).HasForeignKey(f => f.MenuControllerId).OnDelete(DeleteBehavior.Restrict);
                //prop.HasMany<UserGroupMenuControllerActionPermissions>(u => u.UserMenuControllerActionPermissions).WithOne(p => p.MenuControllerPermission).HasForeignKey(f => f.MenuControllerId).OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<MenuControllerActions>(prop =>
            {
                prop.HasIndex(u => new { u.ActionName, u.UrlPath, u.CompanyId }, name: "ix_MenuControllerActions_ActionName_UrlPath_CompanyId").IsUnique();
                prop.HasIndex(u => u.ActionName, name: "ix_MenuControllerActions_ActionName").IsUnique();
                //prop.HasMany<UserGroupMenuControllerActionPermissions>(u => u.UserMenuControllerActionPermissions).WithOne(p => p.MenuControllerActionPermission).HasForeignKey(f => f.MenuControllerActionId).OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<CountryDetail>(prop =>
            {
                prop.HasMany<StateDetail>(u => u.StateDetails).WithOne(p => p.CountryDetail).HasForeignKey(f => f.CountryDetailId).OnDelete(DeleteBehavior.Restrict);
                prop.HasMany<MedicCompanyDetail>(u => u.MedicCompanyDetail).WithOne(p => p.CountryDetail).HasForeignKey(f => f.CountryId).OnDelete(DeleteBehavior.Restrict);
                prop.HasIndex(u =>new { u.CountryCode, u.CountryName  },name: "ix_CountryDetail_CountryCode_CountryName_UniqueIndex").IsUnique();
            });

            model.Entity<MedicCompanyDetail>(prop =>
            {
                prop.HasMany<BranchDetail>(u => u.BranchDetail).WithOne(p => p.CompanyDetail).HasForeignKey(f => f.CompanyId).OnDelete(DeleteBehavior.Restrict);
                prop.HasMany<MenuController>(u => u.MenuController).WithOne(p => p.MedicCompanyDetail).HasForeignKey(f => f.CompanyId).OnDelete(DeleteBehavior.Restrict);
                prop.HasMany<MenuControllerActions>(u => u.MenuControllerActions).WithOne(p => p.MedicCompanyDetail).HasForeignKey(f => f.CompanyId).OnDelete(DeleteBehavior.Restrict);
                prop.HasMany<UserProfile>(u => u.UserProfile).WithOne(p => p.MedicCompanyDetail).HasForeignKey(f => f.MedicCompanyId).OnDelete(DeleteBehavior.Restrict);
                prop.HasIndex(u => new { u.CompanyName, u.CountryId, u.StateId }, name: "ix_MedicCompDetail_MedicCompCode_MedicCompName_UniqueIndex").IsUnique();
            });

            model.Entity<BranchDetail>(prop =>
            {
                prop.HasIndex(u => new { u.BranchName, u.CompanyId, u.BranchAddress }, name: "ix_BranchDetail_BranchName_CompanyId_BranchAddress").IsUnique();
            });

            model.Entity<StateDetail>(prop =>
            {
                prop.HasMany<MedicCompanyDetail>(u => u.MedicCompanyDetail).WithOne(p => p.StateDetail).HasForeignKey(f => f.StateId).OnDelete(DeleteBehavior.Restrict);
                prop.HasIndex(u => new { u.StateCode, u.StateName }, name: "ix_StateDetail_StateCode_StateName_UniqueIndex").IsUnique();
            });

            model.Entity<AppDocuments>(prop =>
            {
                prop.HasMany<UserDocument>(u => u.UserAppDocument).WithOne(p => p.AppUserDocuments).HasForeignKey(f => f.AppDocumentId).OnDelete(DeleteBehavior.Restrict);
            });
            model.Entity<UserDocument>(prop =>
            {
                prop.HasIndex(u => new { u.UserProfileId, u.AppDocumentId }, name: "ix_UserDocument_UserProfileIdAppDocumentId_CompositeUniqueIndex").IsUnique();
            });

            return model;
        }
    }
}

namespace MedTechAPI.Persistence.ModelBuilders
{
    public static class DbInitializer
    {
        public static async Task SeedDefaultsData(this IHost host)
        {
            var serviceProvider = host.Services.CreateScope().ServiceProvider;
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            //var cache = serviceProvider.GetRequiredService<ICacheService>();

            if (!context.EmploymentStatus.Any())
            {
                await context.EmploymentStatus.AddRangeAsync(SeedData.GetEmploymentStatuses());
            }

            if (!context.GenderCategories.Any())
            {
                await context.GenderCategories.AddRangeAsync(SeedData.GetGenderCategoryList());
            }

            if (!context.MenuController.Any())
            {
                var menuData = SeedData.GetMenuControllers(context);
                await context.MenuController.AddRangeAsync(menuData);
                await context.SaveChangesAsync();
            }
            if (!context.MenuControllerActions.Any())
            {
                await context.MenuControllerActions.AddRangeAsync(SeedData.GetMenuControllerActions(context));
            }

            if (!context.UserGroup.Any())
            {
                await context.UserGroup.AddRangeAsync(SeedData.GetUserRoles(context));
            }

            if (!context.Salutations.Any())
            {
                await context.Salutations.AddRangeAsync(SeedData.GetSalutations());
            }

            if (!context.PatientCategory.Any())
            {
                await context.PatientCategory.AddRangeAsync(SeedData.GetPatientCategory());
            }

            if (!context.CountryDetails.Any())
            {
                var allCountries = SeedData.GetCountries();
                await context.CountryDetails.AddRangeAsync(allCountries);
                await context.SaveChangesAsync();
                if (!context.StateDetails.Any())
                {
                    var allStates = SeedData.GetStateDetails();
                    allStates.ForEach(m => m.CountryDetailId = allCountries.FirstOrDefault().Id);
                    await context.StateDetails.AddRangeAsync(allStates);
                    await context.SaveChangesAsync();
                }
            }

            if (context.ChangeTracker.HasChanges())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}

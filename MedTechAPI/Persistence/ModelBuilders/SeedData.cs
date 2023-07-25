using System.Diagnostics.Metrics;
using System.Reflection;
using MedTechAPI.Domain.Config;
using MedTechAPI.Domain.Entities.PatientEntitites;
using MedTechAPI.Domain.Entities.ProfileManagement;
using MedTechAPI.Domain.Entities.SetupConfigurations;
using MedTechAPI.Domain.Enums;
using MedTechAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp.Extensions;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace MedTechAPI.Persistence.ModelBuilders
{
    public static class SeedData
    {
        public static List<AppDocuments> GetApplicationDocuments()
        {
            return new List<AppDocuments>()
                {
                    new AppDocuments() { DocumentName = "Photo", DocumentDescription="Profile picture of the user.", DocumentAllowedFormats = new string[]
                    {
                        AppDocumentExtensionTypesEnum.JPG.ToString().ToLower(),AppDocumentExtensionTypesEnum.PNG.ToString().ToLower(),AppDocumentExtensionTypesEnum.JPEG.ToString().ToLower()
                    }, MaxMbFileSize = 1.5M
                    }
                };
        }

        public static List<EmploymentStatus> GetEmploymentStatuses()
        {
            return new List<EmploymentStatus> {
                new EmploymentStatus { Status  = "Employed"},
                new EmploymentStatus { Status  = "Unemployed"},
                new EmploymentStatus { Status  = "Self Employed"},
                new EmploymentStatus { Status  = "Business Owner"},
            };
        }

        public static List<MenuController> GetMenuControllers(AppDbContext _context)
        {
            List<MenuController> objResp = new();
            var companyDetail = _context.MedicCompanyDetails.FirstOrDefault();
            objResp.AddRange(GetAllControllerMenu(_context));
            #region MainMenu
            // Addition of Custom Menu
            Dictionary<string, int> mainMenu = new() { { "Configurations", 99 }, { "Patients", 98 }, { "Staffs", 97 }, { "Pharmacy", 97 } };
            int menuDisplayedCounter = 90;
            foreach (KeyValuePair<string, int> item in mainMenu)
            {
                string CtrCode = $"MDC{menuDisplayedCounter++}{DateTime.Now:yyyyMMddhhMMss}";
                objResp.Add(new MenuController { CompanyId = companyDetail.Id, OrderPriority = item.Value, ControllerCode = CtrCode, ControllerName = item.Key, ControllerDescription = "", DisplayName = item.Key, UrlPath = $"/{item.Key}", IsMenuDisplayed = true });
            }
            #endregion
            return objResp;
        }

        public static List<MenuControllerActions> GetMenuControllerActions(AppDbContext _context)
        {
            List<MenuControllerActions> objResp = new();
            var menuCtr = _context.MenuController.ToList();
            objResp.AddRange(GetAllControllerMenuActions(_context));

            #region MainMenu
            //List<MenuControllerActions> displaySubMenus = new();
            var menuForConfig = menuCtr.Where(m => m.DisplayName == "Configurations" && m.IsMenuDisplayed == true).FirstOrDefault();
            if (menuForConfig != null)
            {
                Dictionary<string, int> subMenus = new() { { "Salutations", 1 }, { "User groups", 2 }, { "Patients config", 3 }, { "Company and branches", 4 }
            , { "Allowed Identifications", 5 }, { "Marital status", 6 }, { "Country", 7 }, { "State/Province", 8 }, { "Relationships", 9 }, { "Banks", 10 }};
                int counter = 90;
                foreach (KeyValuePair<string, int> item in subMenus)
                {
                    string CtrCode = $"MDC{counter++}{DateTime.Now:yyyyMMddhhMMss}";
                    objResp.Add(new MenuControllerActions { CompanyId = menuForConfig.CompanyId, OrderPriority = item.Value, MenuControllerId = menuForConfig.Id, ActionName = item.Key, DisplayName = item.Key, UrlPath = $"{menuForConfig.UrlPath}/{item.Key}", AllowAnonymous = false, IsMenuDisplayed = true });
                }
            }
            #endregion
            return objResp;
        }

        public static List<CountryDetail> GetCountries()
        {
            return new List<CountryDetail> {
                new CountryDetail { CountryName ="Nigeria", CountryCode = "NGN"},
                new CountryDetail { CountryName ="Ghana", CountryCode = "GH"},
                new CountryDetail { CountryName ="Unspecified", CountryCode = "Unspecified"},
            };
        }

        public static List<Salutation> GetSalutations()
        {
            return new List<Salutation> {
                new Salutation { SalutationName ="Mr"},
                new Salutation { SalutationName ="Miss" },
                new Salutation { SalutationName ="Mrs" },
                new Salutation { SalutationName ="Sherif" },
                new Salutation { SalutationName ="Chief" },
                new Salutation { SalutationName ="Doctor" },
                new Salutation { SalutationName ="Professor" },
            };
        }

        public static List<StateDetail> GetStateDetails()
        {
            return new List<StateDetail> {
                new StateDetail { StateName ="Lagos", StateCode = "LA"},
                new StateDetail { StateName ="Delta", StateCode = "DH"},
                new StateDetail { StateName ="Unspecified", StateCode = "Unspecified"},
            };
        }

        public static List<PatientCategory> GetPatientCategory()
        {
            return new List<PatientCategory> {
                new PatientCategory { CategoryName ="In-Patient"},
                new PatientCategory { CategoryName ="Out-Patient"},
            };
        }

        public static List<GenderCategory> GetGenderCategoryList()
        {
            return new List<GenderCategory> {
                new GenderCategory { ActiveStatus = true, ApprovedBy= AppConstants.AppSystem, GenderName ="Male"},
                new GenderCategory { ActiveStatus = true, ApprovedBy= AppConstants.AppSystem, GenderName ="Female"},
                new GenderCategory { ActiveStatus = true, ApprovedBy= AppConstants.AppSystem, GenderName ="Not specified"},
            };
        }

        public static List<MedicCompanyDetail> GetMedicCompanyListList(AppDbContext _context)
        {
            var countryDetail = _context.CountryDetails.AsNoTracking().FirstOrDefault();
            if (countryDetail != null)
            {
                var stateDetail = _context.StateDetails.AsNoTracking().FirstOrDefault(m => m.CountryDetailId == countryDetail.Id);
                return new List<MedicCompanyDetail>
            {
                 new MedicCompanyDetail { CompanyName = "Demo", CountryId = countryDetail.Id, StateId = stateDetail.Id,CompanyAddress = "------" }
            };
            }
            return new List<MedicCompanyDetail>();
        }

        public static List<MainMenu> GetMainMenuList(AppDbContext _context)
        {
            var companyDetail = _context.MedicCompanyDetails.AsNoTracking().Select(c => new MedicCompanyDetail
            {
                Id = c.Id
            }).FirstOrDefault();
            return new List<MainMenu>
            {
                //new MainMenu { TitleDisplay = "Settings", OrderPriority = 100, CompanyId = companyDetail.Id },
                 new MainMenu { TitleDisplay = "Configurations", OrderPriority = 99, CompanyId = companyDetail.Id },
                 new MainMenu { TitleDisplay = "Patients", OrderPriority = 98, CompanyId = companyDetail.Id },
                 new MainMenu { TitleDisplay = "Staffs", OrderPriority = 97, CompanyId = companyDetail.Id },
                 new MainMenu { TitleDisplay = "Pharmacy", OrderPriority = 97, CompanyId = companyDetail.Id },
            };
        }

        public static List<SubMenu> GetSubMenuList(AppDbContext _context)
        {
            List<SubMenu> objResp = new();
            var mainMenu = _context.MainMenus.AsNoTracking();

            var menuForConfig = mainMenu.Where(m => m.TitleDisplay.Contains("Configurations")).FirstOrDefault();
            if (menuForConfig != null)
            {
                objResp.AddRange(new List<SubMenu>
                    {
                         new SubMenu { TitleDisplay = "Salutations", OrderPriority = 1, MenuID = menuForConfig.MenuID, CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "User groups", OrderPriority = 2, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "Patients config", OrderPriority = 3, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "Company and branches", OrderPriority = 4, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "Allowed Identifications", OrderPriority = 5, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "Marital status", OrderPriority = 6, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "Country", OrderPriority = 7, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "State/Province", OrderPriority = 8, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "Relationships", OrderPriority = 10, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                         new SubMenu { TitleDisplay = "Banks", OrderPriority = 12, MenuID = menuForConfig.MenuID , CompanyId = menuForConfig.CompanyId },
                    });
            }

            //var menuForSettings = mainMenu.Where(m => m.TitleDisplay.Contains("Settings", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            //if (menuForConfig != null)
            //{
            //    objResp.AddRange(new List<SubMenu>
            //        {
            //             new SubMenu { TitleDisplay = "User groups", OrderPriority = 1, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Users", OrderPriority = 2, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Gender", OrderPriority = 3, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Education", OrderPriority = 4, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Allowed Identifications", OrderPriority = 5, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Marital status", OrderPriority = 6, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Country", OrderPriority = 7, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "State/Province", OrderPriority = 8, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Work sector", OrderPriority = 9, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Relationships", OrderPriority = 10, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Residence types", OrderPriority = 11, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Banks", OrderPriority = 12, MenuID = menuForSettings.MenuID },
            //             new SubMenu { TitleDisplay = "Advert channel", OrderPriority = 97, MenuID = menuForSettings.MenuID },
            //        });
            //}
            return objResp;
        }

        public static List<UserGroup> GetUserRoles(AppDbContext context)
        {
            var dbContext = context.MenuControllerActions.Select(m => new string[] { m.Id.ToString() }).AsNoTracking().ToList();
            var companyDetail = context.MedicCompanyDetails.FirstOrDefault();
            System.Text.StringBuilder sb = new();
            dbContext.ForEach(m => { sb.Append(m.FirstOrDefault() + ","); });
            string adminGroupRights = sb.ToString().Trim(',');
            return new List<UserGroup> {
                new UserGroup { GroupName = UserRolesEnum.Admin.ToString(), GroupDescription = "This is the Adminstrative role."
                , GroupRight = adminGroupRights, AllowApprove = true, AllowdDelete = true, AllowNew = true, AllowEdit = true, CompanyId = companyDetail.Id},
                new UserGroup { GroupName = UserRolesEnum.Viewer.ToString(), GroupDescription = "", GroupRight ="", CompanyId = companyDetail.Id},
                new UserGroup { GroupName = UserRolesEnum.Approver.ToString(), GroupDescription = "", GroupRight ="", AllowApprove = true, CompanyId = companyDetail.Id},
                new UserGroup { GroupName = UserRolesEnum.Member.ToString(), GroupDescription = "", GroupRight ="", CompanyId = companyDetail.Id},
                new UserGroup {GroupName = UserRolesEnum.Contributor.ToString(), GroupDescription = "This is the default role for all users", GroupRight ="", AllowNew = true, CompanyId = companyDetail.Id},
            };
        }

        public static void GetAllControllerMenuAnMenuActionsDifference(AppDbContext _context)
        {
            List<MenuController> allDbControllerMenu = (from menu in GetAllControllerMenu(_context) select menu).ToList();
            List<MenuController> allAppControllerMenu = (from appMenu in _context.MenuController select appMenu).ToList();

            #region Populate missing MenuControllers

            var temp = (from dbMenu in allDbControllerMenu
                        select new 
                        {
                            ControllerName = (
                               dbMenu.ControllerName = dbMenu.ControllerName.Trim().EndsWith("Controller") ? dbMenu.ControllerName.Replace("Controller", "").Trim() : dbMenu.ControllerName.Trim()
                           )
                        }
               );
            //var diff = (from appmenu in allAppControllerMenu select appmenu.ControllerName.Trim()).Except(
            //   (from dbMenu in allDbControllerMenu
            //    select new
            //    {
            //        CtrName = dbMenu.ControllerName,
            //        Text = (
            //           dbMenu.ControllerName = dbMenu.ControllerName.Trim().EndsWith("Controller") ? dbMenu.ControllerName.Replace("Controller", "").Trim() : dbMenu.ControllerName.Trim()
            //       )
            //    }
            //   ));

            IEnumerable<string> diff = (from appmenu in allAppControllerMenu select appmenu.ControllerName.Trim()).Except((from dbMenu in allDbControllerMenu select dbMenu.ControllerName.Trim()));
            List<MenuController> diffMenuController = new();
            foreach (var item in diff)
            {
                var itemExists = allAppControllerMenu.FirstOrDefault(m => m.ControllerName.Equals(item, StringComparison.OrdinalIgnoreCase));
                if (itemExists != null)
                {
                    diffMenuController.Add(itemExists);
                }
            }
            if (diffMenuController.Any())
            {
                _context.MenuController.AddRange(diffMenuController);
            }
            #endregion

            #region Populate MenuController Actions

            #endregion
        }

        #region HELPERS
        private static List<MenuController> GetAllControllerMenu(AppDbContext _context)
        {
            List<MenuController> objResp = new();
            var companyDetail = _context.MedicCompanyDetails.FirstOrDefault();
            if (companyDetail != null)
            {
                var ctrResults = Assembly.GetExecutingAssembly()
            .GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
            .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
            .GroupBy(x => x.DeclaringType.Name)
            .Select(x => new { ControllerBase = x.Key, Actions = x.Select(s => s.Name).ToList() })
            .ToList();
                int counter = 1;
                foreach (var ctr in ctrResults)
                {
                    var ctrName = ctr.ControllerBase.ToString();
                    var displayName = !string.IsNullOrWhiteSpace(ctrName) ? ctrName.Replace("Controller", "") : throw new Exception("Invalid Controller name");
                    string CtrCode = $"MDC{counter++}{DateTime.Now:yyyyMMddhhMMss}";

                    var menuCtr = new MenuController { CompanyId = companyDetail.Id, OrderPriority = 1, ControllerCode = CtrCode, ControllerName = ctrName, ControllerDescription = "", DisplayName = displayName.SplitCamelCase(), UrlPath = $"/{displayName}", IsMenuDisplayed = false };
                    objResp.Add(menuCtr);
                    #region For Inspection purposes only
                    Console.WriteLine($"========{ctrName}========");
                    ctr.Actions.ForEach(a =>
                    {
                        Console.WriteLine($"Action name is::: {a.Trim()}\n");
                    });
                    #endregion
                }
            }
            return objResp;
        }
        private static List<MenuControllerActions> GetAllControllerMenuActions(AppDbContext _context)
        {
            List<MenuControllerActions> objResp = new();
            var menuCtr = _context.MenuController.ToList();

            if (menuCtr != null && menuCtr.Any())
            {
                var ctrResults = Assembly.GetExecutingAssembly()
            .GetTypes().Where(type => typeof(ControllerBase).IsAssignableFrom(type))
            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
            .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
            .GroupBy(x => x.DeclaringType.Name)
            .Select(x => new { ControllerBase = x.Key, Actions = x.Select(s => s.Name).ToList(), IsAnonymousAction = x.Select(s => s.GetAttribute<AllowAnonymousAttribute>()).ToList() })
            .ToList();
                foreach (var ctr in ctrResults)
                {
                    var ctrName = ctr.ControllerBase.ToString();
                    var displayName = !string.IsNullOrWhiteSpace(ctrName) ? ctrName.Replace("Controller", "") : throw new Exception("Invalid Controller name");
                    string CtrCode = $"MDC{DateTime.Now:yyyyMMddhhMMss}";

                    var companyDetail = menuCtr.FirstOrDefault(m => m.ControllerName == ctrName.Trim());

                    Console.WriteLine($"========{ctrName}========");
                    ctr.Actions.ForEach(a =>
                    {
                        objResp.Add(new MenuControllerActions { CompanyId = companyDetail.CompanyId, OrderPriority = 1, MenuControllerId = companyDetail.Id, ActionName = a.ToString(), DisplayName = a.ToString().SplitCamelCase(), UrlPath = $"{companyDetail.UrlPath}/{a.Trim()}", AllowAnonymous = false, IsMenuDisplayed = false });
                        Console.WriteLine($"Action name is::: {a.ToString()}\n");
                    });
                }
            }
            return objResp;
        }
        #endregion
    }
}

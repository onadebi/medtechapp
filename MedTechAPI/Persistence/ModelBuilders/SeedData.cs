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

                    var menuCtr = new MenuController { CompanyId = companyDetail.Id, OrderPriority = 1, ControllerCode = CtrCode, ControllerName = ctrName, ControllerDescription = "", DisplayName = displayName.SplitCamelCase(), UrlPath = $"/{displayName}" };
                    objResp.Add(menuCtr);
                    #region For Inspection purposes only
                    Console.WriteLine($"========{ctrName}========");
                    ctr.Actions.ForEach(a =>
                    {
                        Console.WriteLine($"Action name is::: {a.ToString()}\n");
                    });
                    #endregion
                }
            }
            return objResp;
        }

        public static List<MenuControllerActions> GetMenuControllerActions(AppDbContext _context)
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
                        objResp.Add(new MenuControllerActions { CompanyId = companyDetail.CompanyId, OrderPriority = 1, MenuControllerId = companyDetail.Id, ActionName = a.ToString(), DisplayName = a.ToString().SplitCamelCase(), UrlPath = $"{companyDetail.UrlPath}/{a.ToString()}", AllowAnonymous = false }); ;
                        Console.WriteLine($"Action name is::: {a.ToString()}\n");
                    });
                }
            }
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
            var dbContext = context.SubMenus.Select(m => new string[] { m.TitleDisplay }).AsNoTracking().ToList();
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
    }
}

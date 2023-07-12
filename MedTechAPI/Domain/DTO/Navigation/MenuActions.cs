namespace MedTechAPI.Domain.DTO.Navigation
{
    public class MenuActions
    {
        public Guid Id { get; set; }
        public int OrderPriority { get; set; }
        public int CompanyId { get; set; }

        public string ActionName { get; set; }

        public string UrlPath { get; set; }

        public string DisplayName { get; set; }
        public string IconClass { get; set; } = "<span class='glyphicon glyphicon-barcode'></span>";
        public string ActionDescription { get; set; }
        public bool? AllowAnonymous { get; set; } = false;
        public Guid MenuControllerId { get; set; }
        public MenuModules MenuModules { get; set; }
    }
}

namespace MedTechAPI.Domain.DTO.Navigation
{
    public class MenuModules
    {
        public Guid Id { get; set; }
        public int OrderPriority { get; set; }
        public string ControllerName { get; set; }

        public string ControllerCode { get; set; }

        public string UrlPath { get; set; }
        public string DisplayName { get; set; }
        public string IconClass { get; set; } = "<span class='glyphicon glyphicon-barcode'></span>";
        public string ControllerDescription { get; set; }

        public ICollection<MenuActions> MenuActions { get; set; }= new List<MenuActions>();

    }
}

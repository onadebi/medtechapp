namespace MedTechAPI.Domain.DTO.Navigation
{
    public class UserGroupMenuAccess
    {
        public int UserGroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public string MenuActionId { get; set; }
        public string ActionName { get; set; }
        public Nullable<Guid> MenuControllerId { get; set; }
        public bool? IsMenuPath { get; set; }
        public string ControllerName { get; set; }
    }

    public class AllMenuActions
    {
        public Nullable<Guid> Id { get; set; }
        public string ControllerDisplayName { get; set; }
        public string ActionName { get; set; }
        public string ActionDisplayName { get; set; }
        public string ActionDescription { get; set; }
        public Nullable<Guid> MenuControllerId { get; set; }
        public bool? IsMenuPath { get; set; }
    }

    public class UserGroupActionsGroupedByGroupName
    {
        public string GroupName { get; set; }
        public string GroupDescription { get; set; }
        public string MenuControllerName { get; set; }
        public List<AllMenuActions> MenuActions { get; set; }
        public List<AllMenuActions> MenuUtilities{ get; set; }
        //public bool AllowApprove { get; set; }
        //public bool AllowEdit { get; set; }
        //public bool AllowNew { get; set; }
        //public bool AllowView { get; set; }
        //public bool AllowDelete { get; set;}
    }
}

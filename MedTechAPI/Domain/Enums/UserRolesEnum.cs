namespace MedTechAPI.Domain.Enums
{
    public enum UserRolesEnum
    {
        Admin = 1,
        Member= 2, // Can view and comment
        Viewer = 3, //Can only view. Cannot comment
        Contributor = 4, // Can view, comment and contribute
        Approver = 5,
    }
}

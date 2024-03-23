namespace SupplyManagement.Utilities
{
    using SupplyManagement.Models.Enums;
    public static class TopMenu
    {
        public static class User
        {
            public const string PageName = "User";
            public const string RoleName = "User";
            public const string Path = "/User/Index";
            public const string ControllerName = "User";
            public const string ActionName = "Index";
        }
        public static class Manager
        {
            public const string PageName = "Manager";
            public const string RoleName = "Manager";
            public const string Path = "/Manager/Index";
            public const string ControllerName = "Manager";
            public const string ActionName = "Index";
        }

        public static class Courier
        {
            public const string PageName = "Courier";
            public const string RoleName = "Courier";
            public const string Path = "/Courier/Index";
            public const string ControllerName = "Courier";
            public const string ActionName = "Index";
        }


        public static class Admin
        {
            public const string PageName = "Admin";
            public const string RoleName = "Admin";
            public const string Path = "/Admin/Index";
            public const string ControllerName = "Admin";
            public const string ActionName = "Index";
        }
    }
}

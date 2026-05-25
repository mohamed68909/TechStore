
namespace TechStore.Utilities
{
    public static class SD
    {
        // Roles
        public const string AdminRole = "Admin";
        public const string EditorRole = "Editor";
        public const string CustomerRole = "Customer";

        // Order statuses
        public const string Pending = "Pending";
        public const string Approve = "Approved";

        // FIX 11: Corrected spelling from "Proccessing" → "Processing"
        public const string Processing = "Processing";

        public const string Cancelled = "Cancelled";
        public const string Shipped = "Shipped";
        public const string Refund = "Refund";
        public const string Rejected = "Rejected";

        // Session
        public const string SessionKey = "ShoppingCartSession";

        // FIX (M-13): Stripe currency as a constant — change here to support multi-region
        public const string StripeCurrency = "usd";
    }
}

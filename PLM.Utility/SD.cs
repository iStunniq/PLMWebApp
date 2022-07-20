using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLM.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Sales = "Sales";
        public const string Role_Marketing = "Marketing";
        public const string Role_Logistics = "Logistics";
        public const string Role_Courier = "Courier";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusInProcess = "Processing";
        public const string StatusApproval = "Approval";
        public const string StatusShipped = "Shipped";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRefunded = "Refunded";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";
    }
}

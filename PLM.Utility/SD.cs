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
        public const string Role_Operation = "Operations";

        public const string StatusPending = "Pending";
        public const string StatusInProcess = "Processing";
        public const string StatusApproval = "Approval";
        public const string StatusShipped = "To Ship";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRefunded = "Refunded";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Cancelled";

        public const int Mid = 10;
        public const int High = 20;

        public const string StockZero = "0. Out of Stock";
        public const string StockLow = "1. Low Stock";
        public const string StockMid = "2. Medium Stock";
        public const string StockHigh = "3. High Stock";

        public const int MinuteTimeout = 10;
    
        public const string Contact = "#09219370070 - Gabriel";
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace RetalSystemAPI.Models.Enums;

public enum SupplierTransactionType
{

    PurchaseInvoice = 1,  // فاتورة شراء (دين على المحل)
    Payment = 2,          // سداد للمورد
    PurchaseReturn = 3,   // مرتجع مشتريات
    OpeningBalance = 4,
    Adjustment = 5
}

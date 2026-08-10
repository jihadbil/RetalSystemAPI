using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;



namespace RetalSystemAPI.Models.Suppliers;

public class SupplierPhone:BaseEntity
{
    public Guid SupplierId { get; set; }

    public Supplier Supplier { get; set; } 


    public string PhoneNumber { get; set; } = null!;

    public string Name { get; set; }




}

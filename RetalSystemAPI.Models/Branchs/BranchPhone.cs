using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RetalSystemAPI.Models.Branchs;
/// <summary>
/// يمثل الكيان الخاص بأرقام هواتف الفروع في النظام، ويحتوي على الخصائص المتعلقة برقم الهاتف مثل الاسم، رقم الهاتف، ومعرف الفرع المرتبط به.
/// </summary>
public class BranchPhone:BaseEntity    
{
   
    /// <summary>
    /// اسم الشخص أو الجهة المرتبطة برقم الهاتف.
    /// </summary>
    public string? Name { get; set; }
    /// <summary>
    /// رقم الهاتف.
    /// </summary>
    /// 
    [Required]
    [RegularExpression(@"^(09\d{8}|\+2189\d{8})$", ErrorMessage = "رقم الهاتف غير صحيح.")]
    public required string PhoneNumber { get; set; } 
    /// <summary>
    /// معرف الفرع المرتبط برقم الهاتف، حيث يشير إلى الفرع الذي ينتمي إليه هذا الرقم.
    /// </summary>
    public Guid BranchId { get; set; }
    public Branch? Branch { get; set; }


    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }


}

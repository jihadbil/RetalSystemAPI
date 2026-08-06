using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Entities.Branchs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetalSystemAPI.Models.Branchs;
/// <summary>
/// يمثل الكيان الخاص بأرقام هواتف الفروع في النظام، ويحتوي على الخصائص المتعلقة برقم الهاتف مثل الاسم، رقم الهاتف، ومعرف الفرع المرتبط به.
/// </summary>
public class BranchPhones:BaseEntity    
{
   
    /// <summary>
    /// اسم الشخص أو الجهة المرتبطة برقم الهاتف.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// رقم الهاتف.
    /// </summary>
    public string PhoneNumber { get; set; } = null!;
    /// <summary>
    /// معرف الفرع المرتبط برقم الهاتف، حيث يشير إلى الفرع الذي ينتمي إليه هذا الرقم.
    /// </summary>
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; }



}

using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.Services.Branch.Specifications;

/// <summary>
/// تخصيص استعلامات الفروع مع العلاقات (BranchPhones, ApplicationUser).
/// </summary>
public class BranchWithPhonesSpec : BaseSpecification<RetalSystemAPI.Models.Branchs.Branch>
{
    public BranchWithPhonesSpec()
    {
        AddInclude(b => b.BranchPhones);
        ApplyOrderBy(b => b.Name);
    }

    public BranchWithPhonesSpec(Guid id) : base(b => b.Id == id)
    {
        AddInclude(b => b.BranchPhones);
    }
}

public class BranchWithPhonesAndUsersSpec : BaseSpecification<RetalSystemAPI.Models.Branchs.Branch>
{
    public BranchWithPhonesAndUsersSpec(Guid id) : base(b => b.Id == id)
    {
        AddInclude(b => b.BranchPhones);
        AddInclude(b => b.ApplicationUsers);
    }
}

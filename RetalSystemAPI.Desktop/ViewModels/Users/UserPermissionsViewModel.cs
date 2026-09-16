using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Users;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Users;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Users;

public partial class UserPermissionsViewModel : BaseViewModel
{
    private readonly IUserApiService _userApiService;
    private readonly IToastService _toastService;

    private List<PermissionDto> _allSystemPermissions = new();
    private List<string> _roleDefaultPermissions = new();

    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _userRolesDisplay = string.Empty;

    [ObservableProperty]
    private bool _isCustomized;

    [ObservableProperty]
    private bool _isAdminUser;

    [ObservableProperty]
    private ObservableCollection<PermissionGroupViewModel> _permissionGroups = new();

    public Action? RequestClose { get; set; }

    public UserPermissionsViewModel(IUserApiService userApiService, IToastService toastService)
    {
        _userApiService = userApiService;
        _toastService = toastService;
    }

    public async Task InitializeAsync(string userId)
    {
        UserId = userId;
        await ExecuteAsync(async () =>
        {
            // 1. جلب قائمة صلاحيات النظام الكاملة
            var permRes = await _userApiService.GetAllPermissionsAsync();
            if (permRes.Success && permRes.Data != null)
            {
                _allSystemPermissions = permRes.Data;
            }

            // 2. جلب بيانات وصلاحيات المستخدم المحددة
            var userPermRes = await _userApiService.GetUserPermissionsAsync(userId);
            if (userPermRes.Success && userPermRes.Data != null)
            {
                var data = userPermRes.Data;
                UserName = data.UserName;
                UserRolesDisplay = data.Roles.Count > 0 ? string.Join(", ", data.Roles) : "مستخدم عام";
                IsCustomized = data.IsCustomized;
                _roleDefaultPermissions = data.RolePermissions;

                IsAdminUser = data.Roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                                                 r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));

                BuildPermissionGroups(data.DirectPermissions, data.RolePermissions);
            }
            else
            {
                _toastService.ShowError(userPermRes.Message ?? "فشل تحميل صلاحيات المستخدم", "خطأ");
            }
        });
    }

    private void BuildPermissionGroups(List<string> directPerms, List<string> rolePerms)
    {
        var directSet = new HashSet<string>(directPerms, StringComparer.OrdinalIgnoreCase);
        var roleSet = new HashSet<string>(rolePerms, StringComparer.OrdinalIgnoreCase);

        var groups = _allSystemPermissions
            .GroupBy(p => p.Category)
            .Select(g =>
            {
                var groupVm = new PermissionGroupViewModel
                {
                    CategoryName = g.Key
                };

                foreach (var p in g)
                {
                    bool isGranted;
                    if (IsAdminUser)
                    {
                        isGranted = true;
                    }
                    else if (IsCustomized)
                    {
                        // إذا كان مخصصاً، نعتمد الصلاحيات المباشرة المحفوظة للمستخدم
                        isGranted = directSet.Contains(p.Code);
                    }
                    else
                    {
                        // إذا كان يرث من الدور، نعتمد الصلاحيات الافتراضية للدور
                        isGranted = roleSet.Contains(p.Code);
                    }

                    var itemVm = new PermissionItemViewModel
                    {
                        Code = p.Code,
                        Name = p.Name,
                        Description = p.Description,
                        Category = p.Category,
                        IsScreenAccess = p.IsScreenAccess,
                        IsGranted = isGranted
                    };

                    itemVm.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(PermissionItemViewModel.IsGranted))
                        {
                            groupVm.NotifyCountChanged();
                        }
                    };

                    groupVm.Permissions.Add(itemVm);
                }

                return groupVm;
            })
            .ToList();

        PermissionGroups = new ObservableCollection<PermissionGroupViewModel>(groups);
    }

    partial void OnIsCustomizedChanged(bool value)
    {
        // عند التحويل إلى وراثة من الدور (false)، نُعيد تعيين المؤشرات لصلاحيات الدور
        if (!value && !IsAdminUser)
        {
            var roleSet = new HashSet<string>(_roleDefaultPermissions, StringComparer.OrdinalIgnoreCase);
            foreach (var grp in PermissionGroups)
            {
                foreach (var item in grp.Permissions)
                {
                    item.IsGranted = roleSet.Contains(item.Code);
                }
                grp.NotifyCountChanged();
            }
        }
    }

    [RelayCommand]
    private void CopyRolePermissions()
    {
        if (IsAdminUser) return;

        IsCustomized = true;
        var roleSet = new HashSet<string>(_roleDefaultPermissions, StringComparer.OrdinalIgnoreCase);

        foreach (var grp in PermissionGroups)
        {
            foreach (var item in grp.Permissions)
            {
                item.IsGranted = roleSet.Contains(item.Code);
            }
            grp.NotifyCountChanged();
        }

        _toastService.ShowInfo("تم استيراد الصلاحيات الافتراضية للدور كقالب. يمكنك الآن تفعيل أو تعطيل أي صلاحية بحرية لهذا المستخدم.", "تم نسخ القالب");
    }

    [RelayCommand]
    private void SelectAll()
    {
        if (IsAdminUser) return;

        IsCustomized = true;
        foreach (var grp in PermissionGroups)
        {
            foreach (var item in grp.Permissions)
            {
                item.IsGranted = true;
            }
            grp.NotifyCountChanged();
        }
    }

    [RelayCommand]
    private void DeselectAll()
    {
        if (IsAdminUser) return;

        IsCustomized = true;
        foreach (var grp in PermissionGroups)
        {
            foreach (var item in grp.Permissions)
            {
                item.IsGranted = false;
            }
            grp.NotifyCountChanged();
        }
    }

    [RelayCommand]
    private async Task SavePermissionsAsync()
    {
        if (string.IsNullOrEmpty(UserId)) return;

        if (IsAdminUser)
        {
            _toastService.ShowInfo("حساب المسؤول الرئيسي (Admin) يمتلك صلاحيات شاملة دائمة لكافة الشاشات والإجراءات.", "تنبيه");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var selectedCodes = PermissionGroups
                .SelectMany(g => g.Permissions)
                .Where(p => p.IsGranted)
                .Select(p => p.Code)
                .Distinct()
                .ToList();

            var req = new UpdateUserPermissionsDto
            {
                UserId = UserId,
                IsCustomized = IsCustomized,
                Permissions = IsCustomized ? selectedCodes : new List<string>()
            };

            var res = await _userApiService.UpdateUserPermissionsAsync(UserId, req);
            if (res.Success)
            {
                var modeDesc = IsCustomized ? "بصلاحيات مخصصة ومستقلة" : "بالوراثة التلقائية من الدور";
                _toastService.ShowSuccess($"تم حفظ صلاحيات المستخدم '{UserName}' {modeDesc} بنجاح", "تم الحفظ بنجاح");
                RequestClose?.Invoke();
            }
            else
            {
                _toastService.ShowError(res.Message ?? "فشل حفظ صلاحيات المستخدم", "خطأ");
            }
        });
    }
}

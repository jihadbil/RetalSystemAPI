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

public partial class PermissionItemViewModel : ObservableObject
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsScreenAccess { get; set; }

    [ObservableProperty]
    private bool _isGranted;
}

public partial class PermissionGroupViewModel : ObservableObject
{
    public string CategoryName { get; set; } = string.Empty;
    public ObservableCollection<PermissionItemViewModel> Permissions { get; set; } = new();

    public int GrantedCount => Permissions.Count(p => p.IsGranted);
    public int TotalCount => Permissions.Count;
    public string SummaryDisplay => $"{GrantedCount} من {TotalCount} صلاحية مفعلة";

    public void NotifyCountChanged()
    {
        OnPropertyChanged(nameof(GrantedCount));
        OnPropertyChanged(nameof(SummaryDisplay));
    }
}

public partial class RolePermissionsViewModel : BaseViewModel
{
    private readonly IUserApiService _userApiService;
    private readonly IToastService _toastService;

    private List<PermissionDto> _allSystemPermissions = new();

    [ObservableProperty]
    private ObservableCollection<RoleDto> _roles = new();

    [ObservableProperty]
    private RoleDto? _selectedRole;

    [ObservableProperty]
    private ObservableCollection<PermissionGroupViewModel> _permissionGroups = new();

    [ObservableProperty]
    private string? _newRoleName;

    [ObservableProperty]
    private bool _isAdminRole;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public Action? RequestClose { get; set; }

    public RolePermissionsViewModel(IUserApiService userApiService, IToastService toastService)
    {
        _userApiService = userApiService;
        _toastService = toastService;
        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        await ExecuteAsync(async () =>
        {
            // 1. تحميل قائمة الصلاحيات الكاملة من النظام
            var permRes = await _userApiService.GetAllPermissionsAsync();
            if (permRes.Success && permRes.Data != null)
            {
                _allSystemPermissions = permRes.Data;
            }

            // 2. تحميل الأدوار
            await LoadRolesAsync();
        });
    }

    [RelayCommand]
    public async Task LoadRolesAsync()
    {
        var res = await _userApiService.GetRolesAsync();
        if (res.Success && res.Data != null)
        {
            Roles = new ObservableCollection<RoleDto>(res.Data);
            if (SelectedRole == null || !Roles.Any(r => r.Id == SelectedRole.Id))
            {
                SelectedRole = Roles.FirstOrDefault();
            }
            else
            {
                SelectedRole = Roles.First(r => r.Id == SelectedRole.Id);
            }
        }
    }

    partial void OnSelectedRoleChanged(RoleDto? value)
    {
        if (value == null)
        {
            PermissionGroups.Clear();
            IsAdminRole = false;
            return;
        }

        IsAdminRole = value.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                      value.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

        _ = LoadRolePermissionsAsync(value);
    }

    private async Task LoadRolePermissionsAsync(RoleDto role)
    {
        await ExecuteAsync(async () =>
        {
            var rolePermRes = await _userApiService.GetRolePermissionsAsync(role.Id);
            var grantedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (rolePermRes.Success && rolePermRes.Data?.Permissions != null)
            {
                foreach (var p in rolePermRes.Data.Permissions)
                {
                    grantedCodes.Add(p);
                }
            }

            // تجميع الصلاحيات حسب التصنيف Category
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
                        var itemVm = new PermissionItemViewModel
                        {
                            Code = p.Code,
                            Name = p.Name,
                            Description = p.Description,
                            Category = p.Category,
                            IsScreenAccess = p.IsScreenAccess,
                            IsGranted = IsAdminRole || grantedCodes.Contains(p.Code)
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
        });
    }

    [RelayCommand]
    private void SelectAll()
    {
        if (IsAdminRole) return;

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
        if (IsAdminRole) return;

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
        if (SelectedRole == null) return;

        if (IsAdminRole)
        {
            _toastService.ShowInfo("دور مدير النظام يمتلك صلاحيات كاملة وتلقائية لكافة الشاشات والإجراءات.", "تنبيه");
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

            var req = new UpdateRolePermissionsDto
            {
                Permissions = selectedCodes
            };

            var res = await _userApiService.UpdateRolePermissionsAsync(SelectedRole.Id, req);
            if (res.Success)
            {
                _toastService.ShowSuccess($"تم حفظ وتحديث صلاحيات الدور '{SelectedRole.Name}' بنجاح", "نجاح العملية");
            }
            else
            {
                _toastService.ShowError(res.Message ?? "فشل حفظ صلاحيات الدور", "خطأ");
            }
        });
    }

    [RelayCommand]
    private async Task CreateRoleAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRoleName))
        {
            _toastService.ShowWarning("يرجى إدخال اسم الدور الجديد", "بيانات غير مكتملة");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new CreateRoleRequest { Name = NewRoleName.Trim() };
            var res = await _userApiService.CreateRoleAsync(req);
            if (res.Success && res.Data != null)
            {
                _toastService.ShowSuccess($"تم إنشاء الدور الجديد '{req.Name}' بنجاح", "تمت الإضافة");
                NewRoleName = string.Empty;
                await LoadRolesAsync();
                SelectedRole = Roles.FirstOrDefault(r => r.Id == res.Data.Id);
            }
            else
            {
                _toastService.ShowError(res.Message ?? "فشل إنشاء الدور", "خطأ");
            }
        });
    }

    [RelayCommand]
    private async Task DeleteRoleAsync()
    {
        if (SelectedRole == null) return;

        if (IsAdminRole || SelectedRole.Name.Equals("User", StringComparison.OrdinalIgnoreCase))
        {
            _toastService.ShowWarning("لا يمكن حذف الأدوار الأساسية للنظام.", "غير مسموح");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _userApiService.DeleteRoleAsync(SelectedRole.Id);
            if (res.Success)
            {
                _toastService.ShowSuccess($"تم حذف الدور '{SelectedRole.Name}' بنجاح", "تم الحذف");
                await LoadRolesAsync();
            }
            else
            {
                _toastService.ShowError(res.Message ?? "فشل حذف الدور", "خطأ");
            }
        });
    }
}

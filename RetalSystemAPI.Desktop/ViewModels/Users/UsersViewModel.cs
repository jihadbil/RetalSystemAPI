using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetalSystemAPI.Desktop.Models.Branch;
using RetalSystemAPI.Desktop.Models.Users;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.Services.Users;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Users;

public partial class UsersViewModel : BaseViewModel
{
    private readonly IUserApiService _userApiService;
    private readonly IBranchApiService _branchApiService;

    [ObservableProperty]
    private ObservableCollection<UserSummaryDto> _users = new();

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranchFilter;

    [ObservableProperty]
    private string? _searchQuery;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    public Func<UserSummaryDto?, Task>? OpenFormDialogHandler { get; set; }
    public Func<UserSummaryDto, Task>? OpenResetPasswordDialogHandler { get; set; }
    public Func<string, string, Task<bool>>? ConfirmDeleteHandler { get; set; }

    public UsersViewModel(IUserApiService userApiService, IBranchApiService branchApiService)
    {
        _userApiService = userApiService;
        _branchApiService = branchApiService;
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadBranchesAsync();
        await LoadUsersAsync();
    }

    private async Task LoadBranchesAsync()
    {
        var res = await _branchApiService.GetAllAsync();
        if (res.Success && res.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(res.Data);
        }
    }

    partial void OnSearchQueryChanged(string? value)
    {
        CurrentPage = 1;
        _ = LoadUsersAsync();
    }

    partial void OnSelectedBranchFilterChanged(BranchDto? value)
    {
        CurrentPage = 1;
        _ = LoadUsersAsync();
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        await ExecuteAsync(async () =>
        {
            Guid? branchId = SelectedBranchFilter?.Id;
            var response = await _userApiService.GetPagedAsync(CurrentPage, PageSize, SearchQuery, branchId);
            if (response.Success && response.Data != null)
            {
                Users = new ObservableCollection<UserSummaryDto>(response.Data.Items);
                TotalPages = response.Data.TotalPages > 0 ? response.Data.TotalPages : 1;
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحميل قائمة المستخدمين";
            }
        });
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!IsLoading && CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task PrevPageAsync()
    {
        if (!IsLoading && CurrentPage > 1)
        {
            CurrentPage--;
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task CreateUserAsync()
    {
        if (OpenFormDialogHandler != null)
        {
            await OpenFormDialogHandler(null);
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task EditUserAsync(object? parameter)
    {
        if (parameter is UserSummaryDto user && OpenFormDialogHandler != null)
        {
            await OpenFormDialogHandler(user);
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync(object? parameter)
    {
        if (parameter is UserSummaryDto user && OpenResetPasswordDialogHandler != null)
        {
            await OpenResetPasswordDialogHandler(user);
        }
    }

    [RelayCommand]
    private async Task DeleteUserAsync(object? parameter)
    {
        if (parameter is not UserSummaryDto user) return;
        if (ConfirmDeleteHandler != null)
        {
            var confirmed = await ConfirmDeleteHandler("تأكيد حذف المستخدم", $"هل أنت متاكد من حذف حساب المستخدم '{user.UserName}'؟");
            if (!confirmed) return;
        }

        await ExecuteAsync(async () =>
        {
            var res = await _userApiService.DeleteAsync(user.Id);
            if (res.Success) await LoadUsersAsync();
            else ErrorMessage = res.Message ?? "فشل حذف المستخدم";
        });
    }
}

public partial class UserFormViewModel : BaseViewModel
{
    private readonly IUserApiService _userApiService;
    private readonly IBranchApiService _branchApiService;

    [ObservableProperty]
    private string? _userId;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string? _phoneNumber;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BranchDto> _branches = new();

    [ObservableProperty]
    private BranchDto? _selectedBranch;

    [ObservableProperty]
    private ObservableCollection<RoleDto> _roles = new();

    [ObservableProperty]
    private RoleDto? _selectedRole;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private bool _isEditMode;

    public Action? CloseWindowHandler { get; set; }

    public UserFormViewModel(IUserApiService userApiService, IBranchApiService branchApiService)
    {
        _userApiService = userApiService;
        _branchApiService = branchApiService;
    }

    public async Task InitializeAsync(string? userId)
    {
        await LoadBranchesAndRolesAsync();

        if (!string.IsNullOrEmpty(userId))
        {
            IsEditMode = true;
            UserId = userId;
            await LoadUserDetailsAsync(userId);
        }
        else
        {
            IsEditMode = false;
            UserId = null;
            UserName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Password = string.Empty;
            IsActive = true;
            if (Branches.Count > 0) SelectedBranch = Branches.First();
            if (Roles.Count > 0) SelectedRole = Roles.First();
        }
    }

    private async Task LoadBranchesAndRolesAsync()
    {
        var branchesRes = await _branchApiService.GetAllAsync();
        if (branchesRes.Success && branchesRes.Data != null)
        {
            Branches = new ObservableCollection<BranchDto>(branchesRes.Data);
        }

        var rolesRes = await _userApiService.GetRolesAsync();
        if (rolesRes.Success && rolesRes.Data != null)
        {
            Roles = new ObservableCollection<RoleDto>(rolesRes.Data);
        }
    }

    private async Task LoadUserDetailsAsync(string id)
    {
        await ExecuteAsync(async () =>
        {
            var res = await _userApiService.GetByIdAsync(id);
            if (res.Success && res.Data != null)
            {
                UserName = res.Data.UserName;
                Email = res.Data.Email;
                PhoneNumber = res.Data.PhoneNumber;
                IsActive = res.Data.IsActive;

                SelectedBranch = Branches.FirstOrDefault(b => b.Id == res.Data.BranchId);
                var userRoleName = res.Data.Roles.FirstOrDefault();
                if (!string.IsNullOrEmpty(userRoleName))
                {
                    SelectedRole = Roles.FirstOrDefault(r => r.Name == userRoleName);
                }
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تحميل تفاصيل المستخدم";
            }
        });
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!IsEditMode && string.IsNullOrWhiteSpace(UserName))
        {
            ErrorMessage = "اسم المستخدم مطلوب";
            return;
        }

        if (!IsEditMode && string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "كلمة المرور مطلوبة للمستخدم الجديد";
            return;
        }

        if (SelectedBranch == null)
        {
            ErrorMessage = "يرجى اختيار الفرع التابع للمستخدم";
            return;
        }

        await ExecuteAsync(async () =>
        {
            if (IsEditMode && !string.IsNullOrEmpty(UserId))
            {
                var req = new UpdateUserRequest
                {
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    BranchId = SelectedBranch.Id,
                    Role = SelectedRole?.Name,
                    IsActive = IsActive
                };
                var res = await _userApiService.UpdateAsync(UserId, req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل حفظ بيانات المستخدم";
            }
            else
            {
                var req = new CreateUserRequest
                {
                    UserName = UserName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    Password = Password,
                    BranchId = SelectedBranch.Id,
                    Role = SelectedRole?.Name
                };
                var res = await _userApiService.CreateAsync(req);
                if (res.Success) CloseWindowHandler?.Invoke();
                else ErrorMessage = res.Message ?? "فشل إضافة المستخدم";
            }
        });
    }
}

public partial class ResetPasswordViewModel : BaseViewModel
{
    private readonly IUserApiService _userApiService;

    [ObservableProperty]
    private string _userId = string.Empty;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    public Action? CloseWindowHandler { get; set; }

    public ResetPasswordViewModel(IUserApiService userApiService)
    {
        _userApiService = userApiService;
    }

    public void Initialize(string userId, string userName)
    {
        UserId = userId;
        UserName = userName;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "كلمة المرور الجديدة مطلوبة";
            return;
        }

        if (NewPassword.Length < 6)
        {
            ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var req = new ResetPasswordRequest { NewPassword = NewPassword };
            var res = await _userApiService.ResetPasswordAsync(UserId, req);
            if (res.Success)
            {
                CloseWindowHandler?.Invoke();
            }
            else
            {
                ErrorMessage = res.Message ?? "فشل تغيير كلمة المرور";
            }
        });
    }
}

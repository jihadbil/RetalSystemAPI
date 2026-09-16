using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RetalSystemAPI.Desktop.Models.Tenant;
using RetalSystemAPI.Desktop.Services;
using RetalSystemAPI.Desktop.ViewModels.Base;

namespace RetalSystemAPI.Desktop.ViewModels.Tenants;

public partial class TenantsViewModel : BaseViewModel
{
    private readonly ITenantApiService _tenantApiService;

    [ObservableProperty]
    private TenantDto? _currentTenant;

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private bool _isUploadingLogo;

    // ── حقول التعديل ───────────────────────────
    [ObservableProperty]
    private string _tenantName = string.Empty;

    [ObservableProperty]
    private string _commercialName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _businessType = string.Empty;

    [ObservableProperty]
    private string _taxNumber = string.Empty;

    [ObservableProperty]
    private string _commercialRegistrationNumber = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private string _additionalPhone = string.Empty;

    [ObservableProperty]
    private string _contactEmail = string.Empty;

    [ObservableProperty]
    private string _websiteUrl = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _country = "ليبيا";

    [ObservableProperty]
    private string _postalCode = string.Empty;

    [ObservableProperty]
    private string _logoUrl = string.Empty;

    [ObservableProperty]
    private string _invoiceHeaderNote = string.Empty;

    [ObservableProperty]
    private string _invoiceFooterNote = string.Empty;

    [ObservableProperty]
    private string _bankDetails = string.Empty;

    [ObservableProperty]
    private string _defaultCurrency = "دينار ليبي";

    [ObservableProperty]
    private string _defaultCurrencySymbol = "د.ل";

    [ObservableProperty]
    private decimal _defaultTaxRate;

    [ObservableProperty]
    private bool _isTaxIncludedInPrices;

    [ObservableProperty]
    private string _timezone = "Africa/Tripoli";

    [ObservableProperty]
    private string _successMessage = string.Empty;

    public string DisplayLogoUrl => string.IsNullOrWhiteSpace(LogoUrl)
        ? string.Empty
        : (LogoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? LogoUrl
            : $"https://localhost:7226/{LogoUrl.TrimStart('/')}");

    partial void OnLogoUrlChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayLogoUrl));
    }

    public TenantsViewModel(ITenantApiService tenantApiService)
    {
        _tenantApiService = tenantApiService;
        _ = LoadProfileAsync();
    }

    [RelayCommand]
    public void SetTab(object? parameter)
    {
        if (parameter is int index)
        {
            SelectedTabIndex = index;
        }
        else if (parameter != null && int.TryParse(parameter.ToString(), out int parsed))
        {
            SelectedTabIndex = parsed;
        }
    }

    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;

        await ExecuteAsync(async () =>
        {
            var response = await _tenantApiService.GetMyProfileAsync();
            if (response.Success && response.Data != null)
            {
                CurrentTenant = response.Data;
                PopulateFields(response.Data);
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحميل بيانات الملف الشخصي للمؤسسة";
            }
        });
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(TenantName))
        {
            ErrorMessage = "اسم المنشأة / المؤسسة مطلوب";
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber))
        {
            ErrorMessage = "رقم الهاتف الرئيسي مطلوب";
            return;
        }

        await ExecuteAsync(async () =>
        {
            var request = new UpdateTenantRequest
            {
                Id = CurrentTenant?.Id ?? Guid.Empty,
                Name = TenantName.Trim(),
                CommercialName = string.IsNullOrWhiteSpace(CommercialName) ? null : CommercialName.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                BusinessType = string.IsNullOrWhiteSpace(BusinessType) ? null : BusinessType.Trim(),
                TaxNumber = string.IsNullOrWhiteSpace(TaxNumber) ? null : TaxNumber.Trim(),
                CommercialRegistrationNumber = string.IsNullOrWhiteSpace(CommercialRegistrationNumber) ? null : CommercialRegistrationNumber.Trim(),
                PhoneNumber = PhoneNumber.Trim(),
                AdditionalPhone = string.IsNullOrWhiteSpace(AdditionalPhone) ? null : AdditionalPhone.Trim(),
                ContactEmail = string.IsNullOrWhiteSpace(ContactEmail) ? null : ContactEmail.Trim(),
                WebsiteUrl = string.IsNullOrWhiteSpace(WebsiteUrl) ? null : WebsiteUrl.Trim(),
                Address = Address?.Trim() ?? string.Empty,
                City = string.IsNullOrWhiteSpace(City) ? null : City.Trim(),
                Country = string.IsNullOrWhiteSpace(Country) ? "ليبيا" : Country.Trim(),
                PostalCode = string.IsNullOrWhiteSpace(PostalCode) ? null : PostalCode.Trim(),
                LogoUrl = LogoUrl ?? string.Empty,
                InvoiceHeaderNote = string.IsNullOrWhiteSpace(InvoiceHeaderNote) ? null : InvoiceHeaderNote.Trim(),
                InvoiceFooterNote = string.IsNullOrWhiteSpace(InvoiceFooterNote) ? null : InvoiceFooterNote.Trim(),
                BankDetails = string.IsNullOrWhiteSpace(BankDetails) ? null : BankDetails.Trim(),
                DefaultCurrency = string.IsNullOrWhiteSpace(DefaultCurrency) ? "دينار ليبي" : DefaultCurrency.Trim(),
                DefaultCurrencySymbol = string.IsNullOrWhiteSpace(DefaultCurrencySymbol) ? "د.ل" : DefaultCurrencySymbol.Trim(),
                DefaultTaxRate = DefaultTaxRate,
                IsTaxIncludedInPrices = IsTaxIncludedInPrices,
                Timezone = string.IsNullOrWhiteSpace(Timezone) ? "Africa/Tripoli" : Timezone.Trim(),
                IsActive = CurrentTenant?.IsActive ?? true
            };

            var response = await _tenantApiService.UpdateMyProfileAsync(request);
            if (response.Success && response.Data != null)
            {
                CurrentTenant = response.Data;
                PopulateFields(response.Data);
                SuccessMessage = "تم حفظ وتحديث بيانات ملف المؤسسة بنجاح";
            }
            else
            {
                ErrorMessage = response.Message ?? "فشل تحديث البيانات";
            }
        });
    }

    [RelayCommand]
    private async Task SelectAndUploadLogoAsync()
    {
        SuccessMessage = string.Empty;
        ErrorMessage = string.Empty;

        var openFileDialog = new OpenFileDialog
        {
            Title = "اختر صورة شعار المؤسسة",
            Filter = "صور الشعار (*.png;*.jpg;*.jpeg;*.webp)|*.png;*.jpg;*.jpeg;*.webp|جميع الملفات (*.*)|*.*",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var filePath = openFileDialog.FileName;
            if (!File.Exists(filePath)) return;

            IsUploadingLogo = true;
            try
            {
                var response = await _tenantApiService.UploadLogoAsync(filePath);
                if (response.Success && response.Data != null)
                {
                    CurrentTenant = response.Data;
                    LogoUrl = response.Data.LogoUrl;
                    SuccessMessage = "تم رفع وتحديث شعار المؤسسة بنجاح";
                }
                else
                {
                    ErrorMessage = response.Message ?? "فشل رفع الشعار";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"حدث خطأ أثناء رفع الشعار: {ex.Message}";
            }
            finally
            {
                IsUploadingLogo = false;
            }
        }
    }

    [RelayCommand]
    private void RemoveLogo()
    {
        LogoUrl = string.Empty;
    }

    private void PopulateFields(TenantDto dto)
    {
        TenantName = dto.Name;
        CommercialName = dto.CommercialName ?? string.Empty;
        Description = dto.Description ?? string.Empty;
        BusinessType = dto.BusinessType ?? string.Empty;
        TaxNumber = dto.TaxNumber ?? string.Empty;
        CommercialRegistrationNumber = dto.CommercialRegistrationNumber ?? string.Empty;
        PhoneNumber = dto.PhoneNumber;
        AdditionalPhone = dto.AdditionalPhone ?? string.Empty;
        ContactEmail = dto.ContactEmail ?? string.Empty;
        WebsiteUrl = dto.WebsiteUrl ?? string.Empty;
        Address = dto.Address;
        City = dto.City ?? string.Empty;
        Country = string.IsNullOrWhiteSpace(dto.Country) ? "ليبيا" : dto.Country;
        PostalCode = dto.PostalCode ?? string.Empty;
        LogoUrl = dto.LogoUrl;
        InvoiceHeaderNote = dto.InvoiceHeaderNote ?? string.Empty;
        InvoiceFooterNote = dto.InvoiceFooterNote ?? string.Empty;
        BankDetails = dto.BankDetails ?? string.Empty;
        DefaultCurrency = string.IsNullOrWhiteSpace(dto.DefaultCurrency) ? "دينار ليبي" : dto.DefaultCurrency;
        DefaultCurrencySymbol = string.IsNullOrWhiteSpace(dto.DefaultCurrencySymbol) ? "د.ل" : dto.DefaultCurrencySymbol;
        DefaultTaxRate = dto.DefaultTaxRate;
        IsTaxIncludedInPrices = dto.IsTaxIncludedInPrices;
        Timezone = string.IsNullOrWhiteSpace(dto.Timezone) ? "Africa/Tripoli" : dto.Timezone;
    }
}

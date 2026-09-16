using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Desktop.Core.Http;
using RetalSystemAPI.Desktop.Models.Catalog;
using RetalSystemAPI.Desktop.Models.Common;

namespace RetalSystemAPI.Desktop.Services.Catalog;

public interface ICategoryApiService
{
    Task<ApiResponse<List<CategoryDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<List<CategoryDto>>> GetRootsAsync(CancellationToken ct = default);
    Task<ApiResponse<List<CategoryDto>>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);
    Task<ApiResponse<PagedResult<CategoryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<CategoryDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default);
}

public class CategoryApiService : ICategoryApiService
{
    private readonly ApiClient _apiClient;

    public CategoryApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<CategoryDto>>> GetAllAsync(CancellationToken ct = default) =>
        _apiClient.GetAsync<List<CategoryDto>>("catalog/categories", ct);

    public Task<ApiResponse<List<CategoryDto>>> GetRootsAsync(CancellationToken ct = default) =>
        _apiClient.GetAsync<List<CategoryDto>>("catalog/categories/roots", ct);

    public Task<ApiResponse<List<CategoryDto>>> GetChildrenAsync(Guid parentId, CancellationToken ct = default) =>
        _apiClient.GetAsync<List<CategoryDto>>($"catalog/categories/{parentId}/children", ct);

    public Task<ApiResponse<PagedResult<CategoryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, CancellationToken ct = default) =>
        _apiClient.GetAsync<PagedResult<CategoryDto>>($"catalog/categories/paged?pageNumber={pageNumber}&pageSize={pageSize}", ct);

    public Task<ApiResponse<CategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.GetAsync<CategoryDto>($"catalog/categories/{id}", ct);

    public Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default) =>
        _apiClient.PostAsync<CategoryDto>("catalog/categories", request, ct);

    public Task<ApiResponse<CategoryDto>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default) =>
        _apiClient.PutAsync<CategoryDto>($"catalog/categories/{id}", request, ct);

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"catalog/categories/{id}", ct);

    public Task<ApiResponse<CategoryDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.PatchAsync<CategoryDto>($"catalog/categories/{id}/toggle-active", ct);
}

public interface IUnitApiService
{
    Task<ApiResponse<List<UnitDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<UnitDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<UnitDto>> CreateAsync(CreateUnitRequest request, CancellationToken ct = default);
    Task<ApiResponse<UnitDto>> UpdateAsync(Guid id, UpdateUnitRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class UnitApiService : IUnitApiService
{
    private readonly ApiClient _apiClient;

    public UnitApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<UnitDto>>> GetAllAsync(CancellationToken ct = default) =>
        _apiClient.GetAsync<List<UnitDto>>("catalog/units", ct);

    public Task<ApiResponse<UnitDto>> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.GetAsync<UnitDto>($"catalog/units/{id}", ct);

    public Task<ApiResponse<UnitDto>> CreateAsync(CreateUnitRequest request, CancellationToken ct = default) =>
        _apiClient.PostAsync<UnitDto>("catalog/units", request, ct);

    public Task<ApiResponse<UnitDto>> UpdateAsync(Guid id, UpdateUnitRequest request, CancellationToken ct = default) =>
        _apiClient.PutAsync<UnitDto>($"catalog/units/{id}", request, ct);

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"catalog/units/{id}", ct);
}

public interface IProductApiService
{
    Task<ApiResponse<List<ProductDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ApiResponse<PagedResult<ProductDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, CancellationToken ct = default);
    Task<ApiResponse<List<ProductDto>>> SearchAsync(string query, CancellationToken ct = default);
    Task<ApiResponse<ProductDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default);
    Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<byte[]?> ExportExcelAsync(bool singleSheet = true, CancellationToken ct = default);
    Task<byte[]?> DownloadTemplateAsync(bool singleSheet = true, CancellationToken ct = default);
    Task<ApiResponse<ProductExcelValidationResultDto>> ValidateExcelAsync(string filePath, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default);
    Task<ApiResponse<ProductImportResultDto>> ImportExcelAsync(string filePath, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default);
    Task<byte[]?> ExportRejectedExcelAsync(List<FailedRowDetailsDto> failedRows, CancellationToken ct = default);
}

public class ProductApiService : IProductApiService
{
    private readonly ApiClient _apiClient;

    public ProductApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<ProductDto>>> GetAllAsync(CancellationToken ct = default) =>
        _apiClient.GetAsync<List<ProductDto>>("catalog/products", ct);

    public Task<ApiResponse<PagedResult<ProductDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, CancellationToken ct = default)
    {
        var url = $"catalog/products/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (categoryId.HasValue) url += $"&categoryId={categoryId.Value}";
        return _apiClient.GetAsync<PagedResult<ProductDto>>(url, ct);
    }

    public Task<ApiResponse<List<ProductDto>>> SearchAsync(string query, CancellationToken ct = default) =>
        _apiClient.GetAsync<List<ProductDto>>($"catalog/products/search?q={Uri.EscapeDataString(query)}", ct);

    public Task<ApiResponse<ProductDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default) =>
        _apiClient.GetAsync<ProductDto>($"catalog/products/by-barcode/{Uri.EscapeDataString(barCode)}", ct);

    public Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.GetAsync<ProductDto>($"catalog/products/{id}", ct);

    public Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken ct = default) =>
        _apiClient.PostAsync<ProductDto>("catalog/products", request, ct);

    public Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default) =>
        _apiClient.PutAsync<ProductDto>($"catalog/products/{id}", request, ct);

    public Task<ApiResponse> DeleteAsync(Guid id, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"catalog/products/{id}", ct);

    public Task<byte[]?> ExportExcelAsync(bool singleSheet = true, CancellationToken ct = default) =>
        _apiClient.DownloadFileAsync($"catalog/products/export-excel?singleSheet={singleSheet}", ct);

    public Task<byte[]?> DownloadTemplateAsync(bool singleSheet = true, CancellationToken ct = default) =>
        _apiClient.DownloadFileAsync($"catalog/products/excel-template?singleSheet={singleSheet}", ct);

    public Task<ApiResponse<ProductExcelValidationResultDto>> ValidateExcelAsync(string filePath, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default)
    {
        options ??= new ProductExcelImportOptionsDto();
        string url = $"catalog/products/validate-excel?importMode={(int)options.ImportMode}&autoCreateCategories={options.AutoCreateCategories}&autoCreateUnits={options.AutoCreateUnits}&autoGenerateMissingBarcodes={options.AutoGenerateMissingBarcodes}";
        if (options.DefaultShowroomWarehouseId.HasValue) url += $"&defaultShowroomWarehouseId={options.DefaultShowroomWarehouseId.Value}";
        if (options.DefaultStorageWarehouseId.HasValue) url += $"&defaultStorageWarehouseId={options.DefaultStorageWarehouseId.Value}";
        return _apiClient.PostFileAsync<ProductExcelValidationResultDto>(url, filePath, ct);
    }

    public Task<ApiResponse<ProductImportResultDto>> ImportExcelAsync(string filePath, ProductExcelImportOptionsDto? options = null, CancellationToken ct = default)
    {
        options ??= new ProductExcelImportOptionsDto();
        string url = $"catalog/products/import-excel?importMode={(int)options.ImportMode}&autoCreateCategories={options.AutoCreateCategories}&autoCreateUnits={options.AutoCreateUnits}&autoGenerateMissingBarcodes={options.AutoGenerateMissingBarcodes}";
        if (options.DefaultShowroomWarehouseId.HasValue) url += $"&defaultShowroomWarehouseId={options.DefaultShowroomWarehouseId.Value}";
        if (options.DefaultStorageWarehouseId.HasValue) url += $"&defaultStorageWarehouseId={options.DefaultStorageWarehouseId.Value}";
        return _apiClient.PostFileAsync<ProductImportResultDto>(url, filePath, ct);
    }

    public async Task<byte[]?> ExportRejectedExcelAsync(List<FailedRowDetailsDto> failedRows, CancellationToken ct = default)
    {
        var res = await _apiClient.PostAsync<object>("catalog/products/export-rejected-excel", failedRows, ct);
        // Note: For binary downloads, we can download from server or directly save bytes if returned
        return null;
    }
}

public interface IProductUnitApiService
{
    Task<ApiResponse<List<ProductUnitDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ApiResponse<ProductUnitDto>> AddAsync(Guid productId, CreateProductUnitRequest request, CancellationToken ct = default);
    Task<ApiResponse> RemoveAsync(Guid productId, Guid productUnitId, CancellationToken ct = default);
    Task<ApiResponse<ProductUnitDto>> SetDefaultAsync(Guid productId, Guid productUnitId, CancellationToken ct = default);
}

public class ProductUnitApiService : IProductUnitApiService
{
    private readonly ApiClient _apiClient;

    public ProductUnitApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<ProductUnitDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default) =>
        _apiClient.GetAsync<List<ProductUnitDto>>($"catalog/products/{productId}/units", ct);

    public Task<ApiResponse<ProductUnitDto>> AddAsync(Guid productId, CreateProductUnitRequest request, CancellationToken ct = default) =>
        _apiClient.PostAsync<ProductUnitDto>($"catalog/products/{productId}/units", request, ct);

    public Task<ApiResponse> RemoveAsync(Guid productId, Guid productUnitId, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"catalog/products/{productId}/units/{productUnitId}", ct);

    public Task<ApiResponse<ProductUnitDto>> SetDefaultAsync(Guid productId, Guid productUnitId, CancellationToken ct = default) =>
        _apiClient.PatchAsync<ProductUnitDto>($"catalog/products/{productId}/units/{productUnitId}/set-default", ct);
}

public interface IProductBarCodeApiService
{
    Task<ApiResponse<List<ProductBarCodeDto>>> GetAllAsync(string? search = null, CancellationToken ct = default);
    Task<ApiResponse<List<ProductBarCodeDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ApiResponse<ProductBarCodeDto>> AddAsync(Guid productId, CreateProductBarCodeRequest request, CancellationToken ct = default);
    Task<ApiResponse<ProductBarCodeDto>> UpdateAsync(Guid productId, Guid barCodeId, UpdateProductBarCodeRequest request, CancellationToken ct = default);
    Task<ApiResponse> RemoveAsync(Guid productId, Guid barCodeId, CancellationToken ct = default);
}

public class ProductBarCodeApiService : IProductBarCodeApiService
{
    private readonly ApiClient _apiClient;

    public ProductBarCodeApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<ProductBarCodeDto>>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        var url = "catalog/barcodes";
        if (!string.IsNullOrWhiteSpace(search)) url += $"?search={Uri.EscapeDataString(search)}";
        return _apiClient.GetAsync<List<ProductBarCodeDto>>(url, ct);
    }

    public Task<ApiResponse<List<ProductBarCodeDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default) =>
        _apiClient.GetAsync<List<ProductBarCodeDto>>($"catalog/products/{productId}/barcodes", ct);

    public Task<ApiResponse<ProductBarCodeDto>> AddAsync(Guid productId, CreateProductBarCodeRequest request, CancellationToken ct = default) =>
        _apiClient.PostAsync<ProductBarCodeDto>($"catalog/products/{productId}/barcodes", request, ct);

    public Task<ApiResponse<ProductBarCodeDto>> UpdateAsync(Guid productId, Guid barCodeId, UpdateProductBarCodeRequest request, CancellationToken ct = default) =>
        _apiClient.PutAsync<ProductBarCodeDto>($"catalog/products/{productId}/barcodes/{barCodeId}", request, ct);

    public Task<ApiResponse> RemoveAsync(Guid productId, Guid barCodeId, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"catalog/products/{productId}/barcodes/{barCodeId}", ct);
}

public interface IProductImageApiService
{
    Task<ApiResponse<List<ProductImageDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ApiResponse<ProductImageDto>> UploadAsync(Guid productId, string filePath, bool isDefault = false, Guid? barcodeId = null, CancellationToken ct = default);
    Task<ApiResponse<ProductImageDto>> UploadAsync(Guid productId, string filePath, bool isDefault, Guid? barcodeId, IProgress<int>? progress, CancellationToken ct = default);
    Task<ApiResponse> RemoveAsync(Guid productId, Guid imageId, CancellationToken ct = default);
    Task<ApiResponse<ProductImageDto>> SetDefaultAsync(Guid productId, Guid imageId, CancellationToken ct = default);
}

/// <summary>
/// محتوى بايتات يُبلّغ عن تقدم الإرسال (البايتات المرسلة من الإجمالي) أثناء رفع الملف للخادم.
/// </summary>
public class ProgressableStreamContent : HttpContent
{
    private readonly Stream _stream;
    private readonly IProgress<int>? _progress;
    private readonly int _bufferSize = 81920;

    public ProgressableStreamContent(Stream stream, IProgress<int>? progress)
    {
        _stream = stream;
        _progress = progress;
    }

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context)
    {
        long total = _stream.CanSeek ? _stream.Length : -1;
        var buffer = new byte[_bufferSize];
        long sent = 0;
        int read;
        int lastReported = -1;
        while ((read = await _stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, read));
            sent += read;
            if (total > 0 && _progress != null)
            {
                int percent = (int)(sent * 100 / total);
                if (percent > lastReported)
                {
                    lastReported = percent;
                    _progress.Report(percent);
                }
            }
        }
        _progress?.Report(100);
    }

    protected override bool TryComputeLength(out long length)
    {
        if (_stream.CanSeek)
        {
            length = _stream.Length;
            return true;
        }
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stream.Dispose();
        }
        base.Dispose(disposing);
    }
}

public class ProductImageApiService : IProductImageApiService
{
    private readonly ApiClient _apiClient;

    public ProductImageApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<List<ProductImageDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default) =>
        _apiClient.GetAsync<List<ProductImageDto>>($"catalog/products/{productId}/images", ct);

    public Task<ApiResponse<ProductImageDto>> UploadAsync(Guid productId, string filePath, bool isDefault = false, Guid? barcodeId = null, CancellationToken ct = default) =>
        UploadAsync(productId, filePath, isDefault, barcodeId, null, ct);

    public async Task<ApiResponse<ProductImageDto>> UploadAsync(Guid productId, string filePath, bool isDefault, Guid? barcodeId, IProgress<int>? progress, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        var fileStream = File.OpenRead(filePath);
        var streamContent = new ProgressableStreamContent(fileStream, progress);
        content.Add(streamContent, "file", Path.GetFileName(filePath));
        content.Add(new StringContent(isDefault.ToString()), "isDefault");
        if (barcodeId.HasValue)
        {
            content.Add(new StringContent(barcodeId.Value.ToString()), "barcodeId");
        }

        return await _apiClient.PostMultipartAsync<ProductImageDto>($"catalog/products/{productId}/images", content, ct);
    }

    public Task<ApiResponse> RemoveAsync(Guid productId, Guid imageId, CancellationToken ct = default) =>
        _apiClient.DeleteAsync($"catalog/products/{productId}/images/{imageId}", ct);

    public Task<ApiResponse<ProductImageDto>> SetDefaultAsync(Guid productId, Guid imageId, CancellationToken ct = default) =>
        _apiClient.PatchAsync<ProductImageDto>($"catalog/products/{productId}/images/{imageId}/set-default", ct);
}

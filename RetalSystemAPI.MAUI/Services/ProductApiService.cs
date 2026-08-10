using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.MAUI.Core.Http;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.MAUI.Services;

public interface IProductApiService
{
    Task<ApiResponse<PagedResult<ProductResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, CancellationToken ct = default);
    Task<ApiResponse<List<ProductResponseDto>>> SearchAsync(string query, CancellationToken ct = default);
    Task<ApiResponse<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default);
    Task<ApiResponse<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ApiResponse<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ApiResponse<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default);

    // Excel Operations
    Task<byte[]?> ExportExcelAsync(CancellationToken ct = default);
    Task<byte[]?> DownloadTemplateAsync(CancellationToken ct = default);
    Task<ApiResponse<bool>> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default);

    // Product Units
    Task<ApiResponse<List<ProductUnitResponseDto>>> GetProductUnitsAsync(Guid productId, CancellationToken ct = default);
    Task<ApiResponse<ProductUnitResponseDto>> AddProductUnitAsync(Guid productId, CreateProductUnitDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoveProductUnitAsync(Guid productId, Guid productUnitId, CancellationToken ct = default);
    Task<ApiResponse<bool>> SetDefaultProductUnitAsync(Guid productId, Guid productUnitId, CancellationToken ct = default);

    // Product BarCodes
    Task<ApiResponse<List<ProductBarCodeResponseDto>>> GetProductBarCodesAsync(Guid productId, CancellationToken ct = default);
    Task<ApiResponse<ProductBarCodeResponseDto>> AddProductBarCodeAsync(Guid productId, CreateProductBarCodeDto dto, CancellationToken ct = default);
    Task<ApiResponse<ProductBarCodeResponseDto>> UpdateProductBarCodeAsync(Guid productId, Guid barCodeId, UpdateProductBarCodeDto dto, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoveProductBarCodeAsync(Guid productId, Guid barCodeId, CancellationToken ct = default);

    // Product Images
    Task<ApiResponse<List<ProductImageResponseDto>>> GetProductImagesAsync(Guid productId, CancellationToken ct = default);
    Task<ApiResponse<ProductImageResponseDto>> UploadProductImageAsync(Guid productId, Stream fileStream, string fileName, bool isDefault = false, Guid? barcodeId = null, CancellationToken ct = default);
    Task<ApiResponse<bool>> RemoveProductImageAsync(Guid productId, Guid imageId, CancellationToken ct = default);
    Task<ApiResponse<bool>> SetDefaultProductImageAsync(Guid productId, Guid imageId, CancellationToken ct = default);
}

public class ProductApiService : IProductApiService
{
    private readonly ApiClient _apiClient;

    public ProductApiService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResponse<PagedResult<ProductResponseDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? categoryId = null, CancellationToken ct = default)
    {
        var url = $"api/catalog/products/paged?pageNumber={pageNumber}&pageSize={pageSize}";
        if (categoryId.HasValue && categoryId.Value != Guid.Empty)
        {
            url += $"&categoryId={categoryId.Value}";
        }
        return _apiClient.GetAsync<PagedResult<ProductResponseDto>>(url, ct);
    }

    public Task<ApiResponse<List<ProductResponseDto>>> SearchAsync(string query, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<ProductResponseDto>>($"api/catalog/products/search?q={Uri.EscapeDataString(query)}", ct);
    }

    public Task<ApiResponse<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<ProductResponseDto>($"api/catalog/products/by-barcode/{Uri.EscapeDataString(barCode)}", ct);
    }

    public Task<ApiResponse<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<ProductResponseDto>($"api/catalog/products/{id}", ct);
    }

    public Task<ApiResponse<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<ProductResponseDto>("api/catalog/products", dto, ct);
    }

    public Task<ApiResponse<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<ProductResponseDto>($"api/catalog/products/{id}", dto, ct);
    }

    public Task<ApiResponse<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/catalog/products/{id}", ct);
    }

    public Task<byte[]?> ExportExcelAsync(CancellationToken ct = default)
    {
        return _apiClient.GetByteArrayAsync("api/catalog/products/export-excel", ct);
    }

    public Task<byte[]?> DownloadTemplateAsync(CancellationToken ct = default)
    {
        return _apiClient.GetByteArrayAsync("api/catalog/products/excel-template", ct);
    }

    public Task<ApiResponse<bool>> ImportExcelAsync(Stream fileStream, string fileName, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);

        return _apiClient.PostMultipartAsync<bool>("api/catalog/products/import-excel", content, ct);
    }

    // Units
    public Task<ApiResponse<List<ProductUnitResponseDto>>> GetProductUnitsAsync(Guid productId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<ProductUnitResponseDto>>($"api/catalog/products/{productId}/units", ct);
    }

    public Task<ApiResponse<ProductUnitResponseDto>> AddProductUnitAsync(Guid productId, CreateProductUnitDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<ProductUnitResponseDto>($"api/catalog/products/{productId}/units", dto, ct);
    }

    public Task<ApiResponse<bool>> RemoveProductUnitAsync(Guid productId, Guid productUnitId, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/catalog/products/{productId}/units/{productUnitId}", ct);
    }

    public Task<ApiResponse<bool>> SetDefaultProductUnitAsync(Guid productId, Guid productUnitId, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<bool>($"api/catalog/products/{productId}/units/{productUnitId}/set-default", null, ct);
    }

    // BarCodes
    public Task<ApiResponse<List<ProductBarCodeResponseDto>>> GetProductBarCodesAsync(Guid productId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<ProductBarCodeResponseDto>>($"api/catalog/products/{productId}/barcodes", ct);
    }

    public Task<ApiResponse<ProductBarCodeResponseDto>> AddProductBarCodeAsync(Guid productId, CreateProductBarCodeDto dto, CancellationToken ct = default)
    {
        return _apiClient.PostAsync<ProductBarCodeResponseDto>($"api/catalog/products/{productId}/barcodes", dto, ct);
    }

    public Task<ApiResponse<ProductBarCodeResponseDto>> UpdateProductBarCodeAsync(Guid productId, Guid barCodeId, UpdateProductBarCodeDto dto, CancellationToken ct = default)
    {
        return _apiClient.PutAsync<ProductBarCodeResponseDto>($"api/catalog/products/{productId}/barcodes/{barCodeId}", dto, ct);
    }

    public Task<ApiResponse<bool>> RemoveProductBarCodeAsync(Guid productId, Guid barCodeId, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/catalog/products/{productId}/barcodes/{barCodeId}", ct);
    }

    // Images
    public Task<ApiResponse<List<ProductImageResponseDto>>> GetProductImagesAsync(Guid productId, CancellationToken ct = default)
    {
        return _apiClient.GetAsync<List<ProductImageResponseDto>>($"api/catalog/products/{productId}/images", ct);
    }

    public Task<ApiResponse<ProductImageResponseDto>> UploadProductImageAsync(Guid productId, Stream fileStream, string fileName, bool isDefault = false, Guid? barcodeId = null, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(isDefault.ToString().ToLower()), "isDefault");
        if (barcodeId.HasValue)
        {
            content.Add(new StringContent(barcodeId.Value.ToString()), "barcodeId");
        }

        return _apiClient.PostMultipartAsync<ProductImageResponseDto>($"api/catalog/products/{productId}/images", content, ct);
    }

    public Task<ApiResponse<bool>> RemoveProductImageAsync(Guid productId, Guid imageId, CancellationToken ct = default)
    {
        return _apiClient.DeleteAsync<bool>($"api/catalog/products/{productId}/images/{imageId}", ct);
    }

    public Task<ApiResponse<bool>> SetDefaultProductImageAsync(Guid productId, Guid imageId, CancellationToken ct = default)
    {
        return _apiClient.PatchAsync<bool>($"api/catalog/products/{productId}/images/{imageId}/set-default", null, ct);
    }
}

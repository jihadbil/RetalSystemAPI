using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Controllers.Base;

/// <summary>
/// المتحكم الأساسي لجميع المتحكمات في النظام، يوفر معالجة موحدة للنتائج وإرجاع أكواد HTTP المناسبة.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse<T>.Ok(result.Data!));

        int statusCode = ResolveStatusCode(result.ErrorCode);
        return StatusCode(statusCode, ApiResponse<T>.Fail(result.ErrorMessage!, result.ErrorCode));
    }

    protected IActionResult ToActionResult(ServiceResult result)
    {
        if (result.IsSuccess)
            return Ok(ApiResponse.Ok());

        int statusCode = ResolveStatusCode(result.ErrorCode);
        return StatusCode(statusCode, ApiResponse.Fail(result.ErrorMessage!, result.ErrorCode));
    }

    private static int ResolveStatusCode(string? errorCode) => errorCode switch
    {
        ErrorCodes.NotFound
        or ErrorCodes.TenantNotFound
        or ErrorCodes.BranchNotFound
        or ErrorCodes.CategoryNotFound
        or ErrorCodes.ProductNotFound
        or ErrorCodes.UnitNotFound
        or ErrorCodes.ProductUnitNotFound
        or ErrorCodes.BarCodeNotFound
        or ErrorCodes.ImageNotFound
        or ErrorCodes.SupplierNotFound
        or ErrorCodes.WarehouseNotFound
        or ErrorCodes.StockNotFound
        or ErrorCodes.PurchaseOrderNotFound
        or ErrorCodes.CustomerNotFound
        or ErrorCodes.SalesInvoiceNotFound
        or ErrorCodes.SalesReturnNotFound
        or ErrorCodes.StockTransferNotFound
        or ErrorCodes.StockAdjustmentNotFound => 404,

        ErrorCodes.Unauthorized
        or ErrorCodes.InvalidCredentials => 401,

        ErrorCodes.UserAlreadyExists
        or ErrorCodes.TenantNameExists
        or ErrorCodes.BranchNameExists
        or ErrorCodes.UnitNameExists
        or ErrorCodes.ProductUnitDuplicate
        or ErrorCodes.BarCodeDuplicate
        or ErrorCodes.CategoryNameExists
        or ErrorCodes.SupplierNameExists
        or ErrorCodes.WarehouseNameExists
        or ErrorCodes.PurchaseOrderNumberExists
        or ErrorCodes.CustomerCodeExists
        or ErrorCodes.SalesInvoiceNumberExists
        or ErrorCodes.SalesReturnNumberExists
        or ErrorCodes.StockTransferNumberExists
        or ErrorCodes.StockAdjustmentNumberExists
        or ErrorCodes.ConcurrencyError => 409,

        ErrorCodes.BranchHasUsers
        or ErrorCodes.CategoryHasProducts
        or ErrorCodes.UnitInUse
        or ErrorCodes.CategoryCircularRef
        or ErrorCodes.WarehouseHasStock => 422,

        ErrorCodes.InvalidFileType
        or ErrorCodes.FileTooLarge
        or ErrorCodes.ValidationError
        or ErrorCodes.InsufficientStock
        or ErrorCodes.InsufficientShowroomStock
        or ErrorCodes.CustomerCreditExceeded
        or ErrorCodes.PurchaseOrderInvalidStatus
        or ErrorCodes.SalesInvoiceInvalidStatus
        or ErrorCodes.StockTransferInvalidStatus
        or ErrorCodes.StockTransferSameWarehouse => 400,

        _ => 400
    };
}

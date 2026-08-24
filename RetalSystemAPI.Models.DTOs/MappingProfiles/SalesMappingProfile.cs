using AutoMapper;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class SalesMappingProfile : Profile
{
    public SalesMappingProfile()
    {
        // ── SalesInvoice ──────────────────────────────────────────
        CreateMap<SalesInvoice, SalesInvoiceResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == InvoiceStatus.Draft ? "مسودة" :
                src.Status == InvoiceStatus.Pending ? "معلقة" :
                src.Status == InvoiceStatus.Paid ? "مدفوعة" :
                src.Status == InvoiceStatus.PartiallyPaid ? "مدفوعة جزئياً" :
                src.Status == InvoiceStatus.Cancelled ? "ملغاة" :
                src.Status == InvoiceStatus.Voided ? "باطلة" : src.Status.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<SalesInvoice, SalesInvoiceSummaryDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == InvoiceStatus.Draft ? "مسودة" :
                src.Status == InvoiceStatus.Pending ? "معلقة" :
                src.Status == InvoiceStatus.Paid ? "مدفوعة" :
                src.Status == InvoiceStatus.PartiallyPaid ? "مدفوعة جزئياً" :
                src.Status == InvoiceStatus.Cancelled ? "ملغاة" :
                src.Status == InvoiceStatus.Voided ? "باطلة" : src.Status.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<CreateSalesInvoiceDto, SalesInvoice>()
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.TotalAmount - src.PaidAmount))
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<UpdateSalesInvoiceDto, SalesInvoice>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.RemainingAmount, opt => opt.Ignore());

        CreateMap<SalesInvoiceItem, SalesInvoiceItemResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        CreateMap<SalesInvoiceItemDto, SalesInvoiceItem>()
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => (src.Quantity * src.UnitPrice) - src.DiscountAmount));

        // ── SalesReturn ───────────────────────────────────────────
        CreateMap<SalesReturn, SalesReturnResponseDto>()
            .ForMember(dest => dest.OriginalInvoiceNumber, opt => opt.MapFrom(src => src.OriginalInvoice != null ? src.OriginalInvoice.InvoiceNumber : null))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == SalesReturnReason.Defective ? "بضاعة معيبة أو تالفة" :
                src.Reason == SalesReturnReason.WrongItem ? "صنف خاطئ" :
                src.Reason == SalesReturnReason.CustomerChange ? "تغيير رأي العميل" :
                src.Reason == SalesReturnReason.Expired ? "منتهي الصلاحية" :
                src.Reason == SalesReturnReason.Other ? "سبب آخر" : src.Reason.ToString()))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<SalesReturn, SalesReturnSummaryDto>()
            .ForMember(dest => dest.OriginalInvoiceNumber, opt => opt.MapFrom(src => src.OriginalInvoice != null ? src.OriginalInvoice.InvoiceNumber : null))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == SalesReturnReason.Defective ? "بضاعة معيبة أو تالفة" :
                src.Reason == SalesReturnReason.WrongItem ? "صنف خاطئ" :
                src.Reason == SalesReturnReason.CustomerChange ? "تغيير رأي العميل" :
                src.Reason == SalesReturnReason.Expired ? "منتهي الصلاحية" :
                src.Reason == SalesReturnReason.Other ? "سبب آخر" : src.Reason.ToString()))
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        CreateMap<CreateSalesReturnDto, SalesReturn>()
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        CreateMap<SalesReturnItem, SalesReturnItemResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        CreateMap<SalesReturnItemDto, SalesReturnItem>()
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
    }
}

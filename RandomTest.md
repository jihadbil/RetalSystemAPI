خطا نجاح حدف صنف رغم انه توجد له مبيعات و مشتريات
عند اضافة منتج جديد و التوجه الي الصور و بعد ادخال نكهات الأكواد يقول و التوجه للصور يتم اختيار الكود و الصورة لكنه يقول تم ربط الكود بالصورة في الداكرة و سيتم رفع البيانات بمجرد الحفظ ولا يتم عرض هدا بواجه الداتا و هدا يربك المستخدم ولا يظيف الا اخر صورة تم ارفاقها ولا يتم ارفاق صورة لباقي الأكواد
عند رفع الصور لا يظهر اي شريط تحميل و هدا خطا 
عند الدخول الي شاشة العرض و المخزون لا يتم تحديث البيانات في حالة اضافة مخزن جديد او صالة جديدة لا يتم جلب البيانات الجديدة
قم بتعديل بيانات صنف و حدفت 2 باركود من صنف لكن لم يتم حفظ البيانات و لا يظحدف الباركودات و هدا الكود في الخلفية لا يوجد اي خطا info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (10ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [p].[Id], [p].[AveragePrice], [p].[CategoryId], [p].[CostPrice], [p].[CreatedAt], [p].[CreatedByUserId], [p].[Description], [p].[IsDeleted], [p].[Name], [p].[RowVersion], [p].[SalePrice], [p].[TenantId], [p].[UpdatedAt], [p].[UpdatedByUserId], [c0].[Id], [c0].[CreatedAt], [c0].[CreatedByUserId], [c0].[IsActive], [c0].[IsDeleted], [c0].[Name], [c0].[ParentCategoryId], [c0].[RowVersion], [c0].[SortOrder], [c0].[TenantId], [c0].[UpdatedAt], [c0].[UpdatedByUserId]
      FROM [Products] AS [p]
      INNER JOIN (
          SELECT [c].[Id], [c].[CreatedAt], [c].[CreatedByUserId], [c].[IsActive], [c].[IsDeleted], [c].[Name], [c].[ParentCategoryId], [c].[RowVersion], [c].[SortOrder], [c].[TenantId], [c].[UpdatedAt], [c].[UpdatedByUserId]
          FROM [Categories] AS [c]
          WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
      WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
      ORDER BY [p].[Id], [c0].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (9ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [p4].[Id], [p4].[BarcodeId], [p4].[CreatedAt], [p4].[CreatedByUserId], [p4].[ImageUrl], [p4].[IsDefault], [p4].[IsDeleted], [p4].[ProductId], [p4].[RowVersion], [p4].[TenantId], [p4].[UpdatedAt], [p4].[UpdatedByUserId], [s].[Id], [s].[Id0]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s]
      INNER JOIN (
          SELECT [p0].[Id], [p0].[BarcodeId], [p0].[CreatedAt], [p0].[CreatedByUserId], [p0].[ImageUrl], [p0].[IsDefault], [p0].[IsDeleted], [p0].[ProductId], [p0].[RowVersion], [p0].[TenantId], [p0].[UpdatedAt], [p0].[UpdatedByUserId]
          FROM [ProductImages] AS [p0]
          WHERE [p0].[IsDeleted] = CAST(0 AS bit) AND [p0].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p4] ON [s].[Id] = [p4].[ProductId]
      ORDER BY [s].[Id], [s].[Id0]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (11ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [s1].[Id], [s1].[ConversionFactor], [s1].[CreatedAt], [s1].[CreatedByUserId], [s1].[IsDefault], [s1].[IsDeleted], [s1].[ProductId], [s1].[RowVersion], [s1].[TenantId], [s1].[UnitId], [s1].[UpdatedAt], [s1].[UpdatedByUserId], [s1].[Id0], [s1].[CreatedAt0], [s1].[CreatedByUserId0], [s1].[Description], [s1].[IsDeleted0], [s1].[Name], [s1].[RowVersion0], [s1].[TenantId0], [s1].[UnitPackage], [s1].[UpdatedAt0], [s1].[UpdatedByUserId0], [s0].[Id], [s0].[Id0]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s0]
      INNER JOIN (
          SELECT [p1].[Id], [p1].[ConversionFactor], [p1].[CreatedAt], [p1].[CreatedByUserId], [p1].[IsDefault], [p1].[IsDeleted], [p1].[ProductId], [p1].[RowVersion], [p1].[TenantId], [p1].[UnitId], [p1].[UpdatedAt], [p1].[UpdatedByUserId], [u0].[Id] AS [Id0], [u0].[CreatedAt] AS [CreatedAt0], [u0].[CreatedByUserId] AS [CreatedByUserId0], [u0].[Description], [u0].[IsDeleted] AS [IsDeleted0], [u0].[Name], [u0].[RowVersion] AS [RowVersion0], [u0].[TenantId] AS [TenantId0], [u0].[UnitPackage], [u0].[UpdatedAt] AS [UpdatedAt0], [u0].[UpdatedByUserId] AS [UpdatedByUserId0]
          FROM [ProductUnits] AS [p1]
          INNER JOIN (
              SELECT [u].[Id], [u].[CreatedAt], [u].[CreatedByUserId], [u].[Description], [u].[IsDeleted], [u].[Name], [u].[RowVersion], [u].[TenantId], [u].[UnitPackage], [u].[UpdatedAt], [u].[UpdatedByUserId]
              FROM [Units] AS [u]
              WHERE [u].[IsDeleted] = CAST(0 AS bit) AND [u].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [u0] ON [p1].[UnitId] = [u0].[Id]
          WHERE [p1].[IsDeleted] = CAST(0 AS bit) AND [p1].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [s1] ON [s0].[Id] = [s1].[ProductId]
      ORDER BY [s0].[Id], [s0].[Id0]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (5ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [p5].[Id], [p5].[BarCode], [p5].[CreatedAt], [p5].[CreatedByUserId], [p5].[Description], [p5].[IsDeleted], [p5].[ProductId], [p5].[RowVersion], [p5].[TenantId], [p5].[Title], [p5].[UpdatedAt], [p5].[UpdatedByUserId], [s2].[Id], [s2].[Id0]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s2]
      INNER JOIN (
          SELECT [p2].[Id], [p2].[BarCode], [p2].[CreatedAt], [p2].[CreatedByUserId], [p2].[Description], [p2].[IsDeleted], [p2].[ProductId], [p2].[RowVersion], [p2].[TenantId], [p2].[Title], [p2].[UpdatedAt], [p2].[UpdatedByUserId]
          FROM [ProductBarCodes] AS [p2]
          WHERE [p2].[IsDeleted] = CAST(0 AS bit) AND [p2].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p5] ON [s2].[Id] = [p5].[ProductId]
      ORDER BY [s2].[Id], [s2].[Id0], [p5].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (13ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [p6].[Id], [p6].[BarcodeId], [p6].[CreatedAt], [p6].[CreatedByUserId], [p6].[ImageUrl], [p6].[IsDefault], [p6].[IsDeleted], [p6].[ProductId], [p6].[RowVersion], [p6].[TenantId], [p6].[UpdatedAt], [p6].[UpdatedByUserId], [s2].[Id], [s2].[Id0], [p5].[Id]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s2]
      INNER JOIN (
          SELECT [p2].[Id], [p2].[ProductId]
          FROM [ProductBarCodes] AS [p2]
          WHERE [p2].[IsDeleted] = CAST(0 AS bit) AND [p2].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p5] ON [s2].[Id] = [p5].[ProductId]
      INNER JOIN (
          SELECT [p3].[Id], [p3].[BarcodeId], [p3].[CreatedAt], [p3].[CreatedByUserId], [p3].[ImageUrl], [p3].[IsDefault], [p3].[IsDeleted], [p3].[ProductId], [p3].[RowVersion], [p3].[TenantId], [p3].[UpdatedAt], [p3].[UpdatedByUserId]
          FROM [ProductImages] AS [p3]
          WHERE [p3].[IsDeleted] = CAST(0 AS bit) AND [p3].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p6] ON [p5].[Id] = [p6].[BarcodeId]
      ORDER BY [s2].[Id], [s2].[Id0], [p5].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @barCodeStrings1='?' (Size = 100), @barCodeStrings2='?' (Size = 100), @barCodeStrings3='?' (Size = 100), @_8__locals1_id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT CASE
          WHEN EXISTS (
              SELECT 1
              FROM [ProductBarCodes] AS [p]
              WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[BarCode] IN (@barCodeStrings1, @barCodeStrings2, @barCodeStrings3) AND [p].[ProductId] <> @_8__locals1_id) THEN CAST(1 AS bit)
          ELSE CAST(0 AS bit)
      END
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (12ms) [Parameters=[@p9='?' (DbType = Guid), @p0='?' (Precision = 18) (Scale = 4) (DbType = Decimal), @p1='?' (DbType = Guid), @p2='?' (Precision = 18) (Scale = 4) (DbType = Decimal), @p3='?' (Size = 2000), @p4='?' (DbType = Boolean), @p5='?' (Size = 200), @p10='?' (Size = 8) (DbType = Binary), @p6='?' (Precision = 18) (Scale = 4) (DbType = Decimal), @p7='?' (DbType = DateTime2), @p8='?' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SET IMPLICIT_TRANSACTIONS OFF;
      SET NOCOUNT ON;
      UPDATE [Products] SET [AveragePrice] = @p0, [CategoryId] = @p1, [CostPrice] = @p2, [Description] = @p3, [IsDeleted] = @p4, [Name] = @p5, [SalePrice] = @p6, [UpdatedAt] = @p7, [UpdatedByUserId] = @p8
      OUTPUT INSERTED.[RowVersion]
      WHERE [Id] = @p9 AND [RowVersion] = @p10;
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (6ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [p].[Id], [p].[AveragePrice], [p].[CategoryId], [p].[CostPrice], [p].[CreatedAt], [p].[CreatedByUserId], [p].[Description], [p].[IsDeleted], [p].[Name], [p].[RowVersion], [p].[SalePrice], [p].[TenantId], [p].[UpdatedAt], [p].[UpdatedByUserId], [c0].[Id], [c0].[CreatedAt], [c0].[CreatedByUserId], [c0].[IsActive], [c0].[IsDeleted], [c0].[Name], [c0].[ParentCategoryId], [c0].[RowVersion], [c0].[SortOrder], [c0].[TenantId], [c0].[UpdatedAt], [c0].[UpdatedByUserId]
      FROM [Products] AS [p]
      INNER JOIN (
          SELECT [c].[Id], [c].[CreatedAt], [c].[CreatedByUserId], [c].[IsActive], [c].[IsDeleted], [c].[Name], [c].[ParentCategoryId], [c].[RowVersion], [c].[SortOrder], [c].[TenantId], [c].[UpdatedAt], [c].[UpdatedByUserId]
          FROM [Categories] AS [c]
          WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
      WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
      ORDER BY [p].[Id], [c0].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [p4].[Id], [p4].[BarcodeId], [p4].[CreatedAt], [p4].[CreatedByUserId], [p4].[ImageUrl], [p4].[IsDefault], [p4].[IsDeleted], [p4].[ProductId], [p4].[RowVersion], [p4].[TenantId], [p4].[UpdatedAt], [p4].[UpdatedByUserId], [s].[Id], [s].[Id0]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s]
      INNER JOIN (
          SELECT [p0].[Id], [p0].[BarcodeId], [p0].[CreatedAt], [p0].[CreatedByUserId], [p0].[ImageUrl], [p0].[IsDefault], [p0].[IsDeleted], [p0].[ProductId], [p0].[RowVersion], [p0].[TenantId], [p0].[UpdatedAt], [p0].[UpdatedByUserId]
          FROM [ProductImages] AS [p0]
          WHERE [p0].[IsDeleted] = CAST(0 AS bit) AND [p0].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p4] ON [s].[Id] = [p4].[ProductId]
      ORDER BY [s].[Id], [s].[Id0]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [s1].[Id], [s1].[ConversionFactor], [s1].[CreatedAt], [s1].[CreatedByUserId], [s1].[IsDefault], [s1].[IsDeleted], [s1].[ProductId], [s1].[RowVersion], [s1].[TenantId], [s1].[UnitId], [s1].[UpdatedAt], [s1].[UpdatedByUserId], [s1].[Id0], [s1].[CreatedAt0], [s1].[CreatedByUserId0], [s1].[Description], [s1].[IsDeleted0], [s1].[Name], [s1].[RowVersion0], [s1].[TenantId0], [s1].[UnitPackage], [s1].[UpdatedAt0], [s1].[UpdatedByUserId0], [s0].[Id], [s0].[Id0]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s0]
      INNER JOIN (
          SELECT [p1].[Id], [p1].[ConversionFactor], [p1].[CreatedAt], [p1].[CreatedByUserId], [p1].[IsDefault], [p1].[IsDeleted], [p1].[ProductId], [p1].[RowVersion], [p1].[TenantId], [p1].[UnitId], [p1].[UpdatedAt], [p1].[UpdatedByUserId], [u0].[Id] AS [Id0], [u0].[CreatedAt] AS [CreatedAt0], [u0].[CreatedByUserId] AS [CreatedByUserId0], [u0].[Description], [u0].[IsDeleted] AS [IsDeleted0], [u0].[Name], [u0].[RowVersion] AS [RowVersion0], [u0].[TenantId] AS [TenantId0], [u0].[UnitPackage], [u0].[UpdatedAt] AS [UpdatedAt0], [u0].[UpdatedByUserId] AS [UpdatedByUserId0]
          FROM [ProductUnits] AS [p1]
          INNER JOIN (
              SELECT [u].[Id], [u].[CreatedAt], [u].[CreatedByUserId], [u].[Description], [u].[IsDeleted], [u].[Name], [u].[RowVersion], [u].[TenantId], [u].[UnitPackage], [u].[UpdatedAt], [u].[UpdatedByUserId]
              FROM [Units] AS [u]
              WHERE [u].[IsDeleted] = CAST(0 AS bit) AND [u].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [u0] ON [p1].[UnitId] = [u0].[Id]
          WHERE [p1].[IsDeleted] = CAST(0 AS bit) AND [p1].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [s1] ON [s0].[Id] = [s1].[ProductId]
      ORDER BY [s0].[Id], [s0].[Id0]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [p5].[Id], [p5].[BarCode], [p5].[CreatedAt], [p5].[CreatedByUserId], [p5].[Description], [p5].[IsDeleted], [p5].[ProductId], [p5].[RowVersion], [p5].[TenantId], [p5].[Title], [p5].[UpdatedAt], [p5].[UpdatedByUserId], [s2].[Id], [s2].[Id0]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s2]
      INNER JOIN (
          SELECT [p2].[Id], [p2].[BarCode], [p2].[CreatedAt], [p2].[CreatedByUserId], [p2].[Description], [p2].[IsDeleted], [p2].[ProductId], [p2].[RowVersion], [p2].[TenantId], [p2].[Title], [p2].[UpdatedAt], [p2].[UpdatedByUserId]
          FROM [ProductBarCodes] AS [p2]
          WHERE [p2].[IsDeleted] = CAST(0 AS bit) AND [p2].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p5] ON [s2].[Id] = [p5].[ProductId]
      ORDER BY [s2].[Id], [s2].[Id0], [p5].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [p6].[Id], [p6].[BarcodeId], [p6].[CreatedAt], [p6].[CreatedByUserId], [p6].[ImageUrl], [p6].[IsDefault], [p6].[IsDeleted], [p6].[ProductId], [p6].[RowVersion], [p6].[TenantId], [p6].[UpdatedAt], [p6].[UpdatedByUserId], [s2].[Id], [s2].[Id0], [p5].[Id]
      FROM (
          SELECT TOP(1) [p].[Id], [c0].[Id] AS [Id0]
          FROM [Products] AS [p]
          INNER JOIN (
              SELECT [c].[Id]
              FROM [Categories] AS [c]
              WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [c0] ON [p].[CategoryId] = [c0].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [c0].[Id]
      ) AS [s2]
      INNER JOIN (
          SELECT [p2].[Id], [p2].[ProductId]
          FROM [ProductBarCodes] AS [p2]
          WHERE [p2].[IsDeleted] = CAST(0 AS bit) AND [p2].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p5] ON [s2].[Id] = [p5].[ProductId]
      INNER JOIN (
          SELECT [p3].[Id], [p3].[BarcodeId], [p3].[CreatedAt], [p3].[CreatedByUserId], [p3].[ImageUrl], [p3].[IsDefault], [p3].[IsDeleted], [p3].[ProductId], [p3].[RowVersion], [p3].[TenantId], [p3].[UpdatedAt], [p3].[UpdatedByUserId]
          FROM [ProductImages] AS [p3]
          WHERE [p3].[IsDeleted] = CAST(0 AS bit) AND [p3].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p6] ON [p5].[Id] = [p6].[BarcodeId]
      ORDER BY [s2].[Id], [s2].[Id0], [p5].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (4ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT COUNT(*)
      FROM [Products] AS [p]
      WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (9ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Int32), @p1='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT [p0].[Id], [p0].[AveragePrice], [p0].[CategoryId], [p0].[CostPrice], [p0].[CreatedAt], [p0].[CreatedByUserId], [p0].[Description], [p0].[IsDeleted], [p0].[Name], [p0].[RowVersion], [p0].[SalePrice], [p0].[TenantId], [p0].[UpdatedAt], [p0].[UpdatedByUserId], [c0].[Id], [c0].[CreatedAt], [c0].[CreatedByUserId], [c0].[IsActive], [c0].[IsDeleted], [c0].[Name], [c0].[ParentCategoryId], [c0].[RowVersion], [c0].[SortOrder], [c0].[TenantId], [c0].[UpdatedAt], [c0].[UpdatedByUserId]
      FROM (
          SELECT [p].[Id], [p].[AveragePrice], [p].[CategoryId], [p].[CostPrice], [p].[CreatedAt], [p].[CreatedByUserId], [p].[Description], [p].[IsDeleted], [p].[Name], [p].[RowVersion], [p].[SalePrice], [p].[TenantId], [p].[UpdatedAt], [p].[UpdatedByUserId]
          FROM [Products] AS [p]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId
          ORDER BY [p].[Name]
          OFFSET @p ROWS FETCH NEXT @p1 ROWS ONLY
      ) AS [p0]
      INNER JOIN (
          SELECT [c].[Id], [c].[CreatedAt], [c].[CreatedByUserId], [c].[IsActive], [c].[IsDeleted], [c].[Name], [c].[ParentCategoryId], [c].[RowVersion], [c].[SortOrder], [c].[TenantId], [c].[UpdatedAt], [c].[UpdatedByUserId]
          FROM [Categories] AS [c]
          WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [c0] ON [p0].[CategoryId] = [c0].[Id]
      ORDER BY [p0].[Name], [p0].[Id], [c0].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (10ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Int32), @p1='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT [p4].[Id], [p4].[BarCode], [p4].[CreatedAt], [p4].[CreatedByUserId], [p4].[Description], [p4].[IsDeleted], [p4].[ProductId], [p4].[RowVersion], [p4].[TenantId], [p4].[Title], [p4].[UpdatedAt], [p4].[UpdatedByUserId], [p0].[Id], [c0].[Id]
      FROM (
          SELECT [p].[Id], [p].[CategoryId], [p].[Name]
          FROM [Products] AS [p]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId
          ORDER BY [p].[Name]
          OFFSET @p ROWS FETCH NEXT @p1 ROWS ONLY
      ) AS [p0]
      INNER JOIN (
          SELECT [c].[Id]
          FROM [Categories] AS [c]
          WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [c0] ON [p0].[CategoryId] = [c0].[Id]
      INNER JOIN (
          SELECT [p1].[Id], [p1].[BarCode], [p1].[CreatedAt], [p1].[CreatedByUserId], [p1].[Description], [p1].[IsDeleted], [p1].[ProductId], [p1].[RowVersion], [p1].[TenantId], [p1].[Title], [p1].[UpdatedAt], [p1].[UpdatedByUserId]
          FROM [ProductBarCodes] AS [p1]
          WHERE [p1].[IsDeleted] = CAST(0 AS bit) AND [p1].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p4] ON [p0].[Id] = [p4].[ProductId]
      ORDER BY [p0].[Name], [p0].[Id], [c0].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (9ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Int32), @p1='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT [p5].[Id], [p5].[BarcodeId], [p5].[CreatedAt], [p5].[CreatedByUserId], [p5].[ImageUrl], [p5].[IsDefault], [p5].[IsDeleted], [p5].[ProductId], [p5].[RowVersion], [p5].[TenantId], [p5].[UpdatedAt], [p5].[UpdatedByUserId], [p0].[Id], [c0].[Id]
      FROM (
          SELECT [p].[Id], [p].[CategoryId], [p].[Name]
          FROM [Products] AS [p]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId
          ORDER BY [p].[Name]
          OFFSET @p ROWS FETCH NEXT @p1 ROWS ONLY
      ) AS [p0]
      INNER JOIN (
          SELECT [c].[Id]
          FROM [Categories] AS [c]
          WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [c0] ON [p0].[CategoryId] = [c0].[Id]
      INNER JOIN (
          SELECT [p2].[Id], [p2].[BarcodeId], [p2].[CreatedAt], [p2].[CreatedByUserId], [p2].[ImageUrl], [p2].[IsDefault], [p2].[IsDeleted], [p2].[ProductId], [p2].[RowVersion], [p2].[TenantId], [p2].[UpdatedAt], [p2].[UpdatedByUserId]
          FROM [ProductImages] AS [p2]
          WHERE [p2].[IsDeleted] = CAST(0 AS bit) AND [p2].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p5] ON [p0].[Id] = [p5].[ProductId]
      ORDER BY [p0].[Name], [p0].[Id], [c0].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (14ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Int32), @p1='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
      SELECT [s].[Id], [s].[ConversionFactor], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDefault], [s].[IsDeleted], [s].[ProductId], [s].[RowVersion], [s].[TenantId], [s].[UnitId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[Id0], [s].[CreatedAt0], [s].[CreatedByUserId0], [s].[Description], [s].[IsDeleted0], [s].[Name], [s].[RowVersion0], [s].[TenantId0], [s].[UnitPackage], [s].[UpdatedAt0], [s].[UpdatedByUserId0], [p0].[Id], [c0].[Id]
      FROM (
          SELECT [p].[Id], [p].[CategoryId], [p].[Name]
          FROM [Products] AS [p]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId
          ORDER BY [p].[Name]
          OFFSET @p ROWS FETCH NEXT @p1 ROWS ONLY
      ) AS [p0]
      INNER JOIN (
          SELECT [c].[Id]
          FROM [Categories] AS [c]
          WHERE [c].[IsDeleted] = CAST(0 AS bit) AND [c].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [c0] ON [p0].[CategoryId] = [c0].[Id]
      INNER JOIN (
          SELECT [p3].[Id], [p3].[ConversionFactor], [p3].[CreatedAt], [p3].[CreatedByUserId], [p3].[IsDefault], [p3].[IsDeleted], [p3].[ProductId], [p3].[RowVersion], [p3].[TenantId], [p3].[UnitId], [p3].[UpdatedAt], [p3].[UpdatedByUserId], [u0].[Id] AS [Id0], [u0].[CreatedAt] AS [CreatedAt0], [u0].[CreatedByUserId] AS [CreatedByUserId0], [u0].[Description], [u0].[IsDeleted] AS [IsDeleted0], [u0].[Name], [u0].[RowVersion] AS [RowVersion0], [u0].[TenantId] AS [TenantId0], [u0].[UnitPackage], [u0].[UpdatedAt] AS [UpdatedAt0], [u0].[UpdatedByUserId] AS [UpdatedByUserId0]
          FROM [ProductUnits] AS [p3]
          INNER JOIN (
              SELECT [u].[Id], [u].[CreatedAt], [u].[CreatedByUserId], [u].[Description], [u].[IsDeleted], [u].[Name], [u].[RowVersion], [u].[TenantId], [u].[UnitPackage], [u].[UpdatedAt], [u].[UpdatedByUserId]
              FROM [Units] AS [u]
              WHERE [u].[IsDeleted] = CAST(0 AS bit) AND [u].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [u0] ON [p3].[UnitId] = [u0].[Id]
          WHERE [p3].[IsDeleted] = CAST(0 AS bit) AND [p3].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [s] ON [p0].[Id] = [s].[ProductId]
      ORDER BY [p0].[Name], [p0].[Id], [c0].[Id]

حاولت الحدف يدويا لكن يبدو ان السبب كان بسبب وجود رابط و مفاتيح خاريجية مع جداول اخري متل جدول مخزون المخز و الصور لدلك تحقق من الامر

في شاشة ادارة المخزون عند التنقل بين تبويب المخز و الصالة لا يتم تطبيق فلترة على كومبو بوكس المخزن او لاصالة المفروض في تبويب المخز يعرض المخازن فقط و في تبويب الصالة يعرض الصلات فقط 
في التسويات الجردية لا يتم تخصيص السبب الصحيح هناك خطا ما لقد سجلت فاتورة و السبب كان رصيد افتيتاحي لكن النظام سجلها جرد دوري
في شاشة ادخال فاتورة جديدة الكمبو بوكس الخاص بطريقة التنزيل يعرض كل لاصناديق المتاحةو صناديق اخرى غير موجودة و انا لا اريد هدا يجب ان يعرض فقط الصناديق المتاحة للصنف
في شاشة امر التحويل في اضافة تحويل مخزني جديد يتم جلب كل الاصناف و هدا طا في المنتجات الكتيرة يحدث بطء في النظام لدلك يجب استبدال هدا بالبحث بالكود او اسم الصنف 
في شاشة  مرتجع المشتريا عن انشاء مرتجع مشتريات جديد يتم جلب كل الاصناف و هدا طا في المنتجات الكتيرة يحدث بطء في النظام لدلك يجب استبدال هدا بالبحث بالكود او اسم الصنف 

عند حدف مرتجع مشتريات info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (25ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [p].[Id], [p].[BranchId], [p].[CreatedAt], [p].[CreatedByUserId], [p].[IsDeleted], [p].[Notes], [p].[PaymentMethod], [p].[PurchaseInvoiceId], [p].[Reason], [p].[ReturnDate], [p].[ReturnNumber], [p].[RowVersion], [p].[SupplierId], [p].[TenantId], [p].[TotalAmount], [p].[UpdatedAt], [p].[UpdatedByUserId], [p].[WarehouseId], [b0].[Id], [b0].[Address], [b0].[CreatedAt], [b0].[CreatedByUserId], [b0].[IsActive], [b0].[IsDeleted], [b0].[Name], [b0].[RowVersion], [b0].[TenantId], [b0].[UpdatedAt], [b0].[UpdatedByUserId], [w0].[Id], [w0].[BranchId], [w0].[CreatedAt], [w0].[CreatedByUserId], [w0].[IsActive], [w0].[IsDeleted], [w0].[Name], [w0].[RowVersion], [w0].[TenantId], [w0].[Type], [w0].[UpdatedAt], [w0].[UpdatedByUserId], [s0].[Id], [s0].[Address], [s0].[CreatedAt], [s0].[CreatedByUserId], [s0].[IsActive], [s0].[IsDeleted], [s0].[Name], [s0].[OpeningBalance], [s0].[RowVersion], [s0].[TenantId], [s0].[UpdatedAt], [s0].[UpdatedByUserId], [p1].[Id], [p1].[BranchId], [p1].[CreatedAt], [p1].[CreatedByUserId], [p1].[DiscountAmount], [p1].[InvoiceDate], [p1].[InvoiceNumber], [p1].[IsDeleted], [p1].[Notes], [p1].[PaidAmount], [p1].[PaymentMethod], [p1].[PurchaseOrderId], [p1].[RemainingAmount], [p1].[RowVersion], [p1].[Status], [p1].[SubTotal], [p1].[SupplierId], [p1].[TaxAmount], [p1].[TenantId], [p1].[TotalAmount], [p1].[UpdatedAt], [p1].[UpdatedByUserId], [p1].[WarehouseId]
      FROM [PurchaseReturns] AS [p]
      INNER JOIN (
          SELECT [b].[Id], [b].[Address], [b].[CreatedAt], [b].[CreatedByUserId], [b].[IsActive], [b].[IsDeleted], [b].[Name], [b].[RowVersion], [b].[TenantId], [b].[UpdatedAt], [b].[UpdatedByUserId]
          FROM [Branches] AS [b]
          WHERE [b].[IsDeleted] = CAST(0 AS bit) AND [b].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [b0] ON [p].[BranchId] = [b0].[Id]
      INNER JOIN (
          SELECT [w].[Id], [w].[BranchId], [w].[CreatedAt], [w].[CreatedByUserId], [w].[IsActive], [w].[IsDeleted], [w].[Name], [w].[RowVersion], [w].[TenantId], [w].[Type], [w].[UpdatedAt], [w].[UpdatedByUserId]
          FROM [Warehouses] AS [w]
          WHERE [w].[IsDeleted] = CAST(0 AS bit) AND [w].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [w0] ON [p].[WarehouseId] = [w0].[Id]
      INNER JOIN (
          SELECT [s].[Id], [s].[Address], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsActive], [s].[IsDeleted], [s].[Name], [s].[OpeningBalance], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId]
          FROM [Suppliers] AS [s]
          WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [s0] ON [p].[SupplierId] = [s0].[Id]
      LEFT JOIN (
          SELECT [p0].[Id], [p0].[BranchId], [p0].[CreatedAt], [p0].[CreatedByUserId], [p0].[DiscountAmount], [p0].[InvoiceDate], [p0].[InvoiceNumber], [p0].[IsDeleted], [p0].[Notes], [p0].[PaidAmount], [p0].[PaymentMethod], [p0].[PurchaseOrderId], [p0].[RemainingAmount], [p0].[RowVersion], [p0].[Status], [p0].[SubTotal], [p0].[SupplierId], [p0].[TaxAmount], [p0].[TenantId], [p0].[TotalAmount], [p0].[UpdatedAt], [p0].[UpdatedByUserId], [p0].[WarehouseId]
          FROM [PurchaseInvoices] AS [p0]
          WHERE [p0].[IsDeleted] = CAST(0 AS bit) AND [p0].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [p1] ON [p].[PurchaseInvoiceId] = [p1].[Id]
      WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
      ORDER BY [p].[Id], [b0].[Id], [w0].[Id], [s0].[Id], [p1].[Id]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (24ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @id='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT [s2].[Id], [s2].[CreatedAt], [s2].[CreatedByUserId], [s2].[IsDeleted], [s2].[LineTotal], [s2].[Notes], [s2].[ProductBarCodeId], [s2].[ProductId], [s2].[PurchaseReturnId], [s2].[Quantity], [s2].[RowVersion], [s2].[TenantId], [s2].[UnitPrice], [s2].[UpdatedAt], [s2].[UpdatedByUserId], [s2].[Id0], [s2].[AveragePrice], [s2].[CategoryId], [s2].[CostPrice], [s2].[CreatedAt0], [s2].[CreatedByUserId0], [s2].[Description], [s2].[IsDeleted0], [s2].[Name], [s2].[RowVersion0], [s2].[SalePrice], [s2].[TenantId0], [s2].[UpdatedAt0], [s2].[UpdatedByUserId0], [s2].[Id1], [s2].[BarCode], [s2].[CreatedAt1], [s2].[CreatedByUserId1], [s2].[Description0], [s2].[IsDeleted1], [s2].[ProductId0], [s2].[RowVersion1], [s2].[TenantId1], [s2].[Title], [s2].[UpdatedAt1], [s2].[UpdatedByUserId1], [s1].[Id], [s1].[Id0], [s1].[Id1], [s1].[Id2], [s1].[Id3]
      FROM (
          SELECT TOP(1) [p].[Id], [b0].[Id] AS [Id0], [w0].[Id] AS [Id1], [s0].[Id] AS [Id2], [p1].[Id] AS [Id3]
          FROM [PurchaseReturns] AS [p]
          INNER JOIN (
              SELECT [b].[Id]
              FROM [Branches] AS [b]
              WHERE [b].[IsDeleted] = CAST(0 AS bit) AND [b].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [b0] ON [p].[BranchId] = [b0].[Id]
          INNER JOIN (
              SELECT [w].[Id]
              FROM [Warehouses] AS [w]
              WHERE [w].[IsDeleted] = CAST(0 AS bit) AND [w].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [w0] ON [p].[WarehouseId] = [w0].[Id]
          INNER JOIN (
              SELECT [s].[Id]
              FROM [Suppliers] AS [s]
              WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [s0] ON [p].[SupplierId] = [s0].[Id]
          LEFT JOIN (
              SELECT [p0].[Id]
              FROM [PurchaseInvoices] AS [p0]
              WHERE [p0].[IsDeleted] = CAST(0 AS bit) AND [p0].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [p1] ON [p].[PurchaseInvoiceId] = [p1].[Id]
          WHERE [p].[IsDeleted] = CAST(0 AS bit) AND [p].[TenantId] = @ef_filter__CurrentTenantId AND [p].[Id] = @id
          ORDER BY [p].[Id], [b0].[Id], [w0].[Id], [s0].[Id], [p1].[Id]
      ) AS [s1]
      INNER JOIN (
          SELECT [p2].[Id], [p2].[CreatedAt], [p2].[CreatedByUserId], [p2].[IsDeleted], [p2].[LineTotal], [p2].[Notes], [p2].[ProductBarCodeId], [p2].[ProductId], [p2].[PurchaseReturnId], [p2].[Quantity], [p2].[RowVersion], [p2].[TenantId], [p2].[UnitPrice], [p2].[UpdatedAt], [p2].[UpdatedByUserId], [p4].[Id] AS [Id0], [p4].[AveragePrice], [p4].[CategoryId], [p4].[CostPrice], [p4].[CreatedAt] AS [CreatedAt0], [p4].[CreatedByUserId] AS [CreatedByUserId0], [p4].[Description], [p4].[IsDeleted] AS [IsDeleted0], [p4].[Name], [p4].[RowVersion] AS [RowVersion0], [p4].[SalePrice], [p4].[TenantId] AS [TenantId0], [p4].[UpdatedAt] AS [UpdatedAt0], [p4].[UpdatedByUserId] AS [UpdatedByUserId0], [p6].[Id] AS [Id1], [p6].[BarCode], [p6].[CreatedAt] AS [CreatedAt1], [p6].[CreatedByUserId] AS [CreatedByUserId1], [p6].[Description] AS [Description0], [p6].[IsDeleted] AS [IsDeleted1], [p6].[ProductId] AS [ProductId0], [p6].[RowVersion] AS [RowVersion1], [p6].[TenantId] AS [TenantId1], [p6].[Title], [p6].[UpdatedAt] AS [UpdatedAt1], [p6].[UpdatedByUserId] AS [UpdatedByUserId1]
          FROM [PurchaseReturnItems] AS [p2]
          INNER JOIN (
              SELECT [p3].[Id], [p3].[AveragePrice], [p3].[CategoryId], [p3].[CostPrice], [p3].[CreatedAt], [p3].[CreatedByUserId], [p3].[Description], [p3].[IsDeleted], [p3].[Name], [p3].[RowVersion], [p3].[SalePrice], [p3].[TenantId], [p3].[UpdatedAt], [p3].[UpdatedByUserId]
              FROM [Products] AS [p3]
              WHERE [p3].[IsDeleted] = CAST(0 AS bit) AND [p3].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [p4] ON [p2].[ProductId] = [p4].[Id]
          LEFT JOIN (
              SELECT [p5].[Id], [p5].[BarCode], [p5].[CreatedAt], [p5].[CreatedByUserId], [p5].[Description], [p5].[IsDeleted], [p5].[ProductId], [p5].[RowVersion], [p5].[TenantId], [p5].[Title], [p5].[UpdatedAt], [p5].[UpdatedByUserId]
              FROM [ProductBarCodes] AS [p5]
              WHERE [p5].[IsDeleted] = CAST(0 AS bit) AND [p5].[TenantId] = @ef_filter__CurrentTenantId
          ) AS [p6] ON [p2].[ProductBarCodeId] = [p6].[Id]
          WHERE [p2].[IsDeleted] = CAST(0 AS bit) AND [p2].[TenantId] = @ef_filter__CurrentTenantId
      ) AS [s2] ON [s1].[Id] = [s2].[PurchaseReturnId]
      ORDER BY [s1].[Id], [s1].[Id0], [s1].[Id1], [s1].[Id2], [s1].[Id3]
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (4ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @_8__locals1_purchaseReturn_WarehouseId='?' (DbType = Guid), @item_ProductBarCodeId_Value='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[WarehouseId] = @_8__locals1_purchaseReturn_WarehouseId AND [s].[ProductBarcodeId] = @item_ProductBarCodeId_Value
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[Id] = @p
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @_8__locals1_purchaseReturn_WarehouseId='?' (DbType = Guid), @item_ProductBarCodeId_Value='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[WarehouseId] = @_8__locals1_purchaseReturn_WarehouseId AND [s].[ProductBarcodeId] = @item_ProductBarCodeId_Value
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[Id] = @p
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @_8__locals1_purchaseReturn_WarehouseId='?' (DbType = Guid), @item_ProductBarCodeId_Value='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[WarehouseId] = @_8__locals1_purchaseReturn_WarehouseId AND [s].[ProductBarcodeId] = @item_ProductBarCodeId_Value
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[Id] = @p
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @_8__locals1_purchaseReturn_WarehouseId='?' (DbType = Guid), @item_ProductBarCodeId_Value='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[WarehouseId] = @_8__locals1_purchaseReturn_WarehouseId AND [s].[ProductBarcodeId] = @item_ProductBarCodeId_Value
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[Id] = @p
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @_8__locals1_purchaseReturn_WarehouseId='?' (DbType = Guid), @item_ProductBarCodeId_Value='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[WarehouseId] = @_8__locals1_purchaseReturn_WarehouseId AND [s].[ProductBarcodeId] = @item_ProductBarCodeId_Value
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[Id] = @p
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @_8__locals1_purchaseReturn_WarehouseId='?' (DbType = Guid), @item_ProductBarCodeId_Value='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[WarehouseId] = @_8__locals1_purchaseReturn_WarehouseId AND [s].[ProductBarcodeId] = @item_ProductBarCodeId_Value
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[Id] = @p
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @_8__locals1_purchaseReturn_WarehouseId='?' (DbType = Guid), @item_ProductBarCodeId_Value='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[WarehouseId] = @_8__locals1_purchaseReturn_WarehouseId AND [s].[ProductBarcodeId] = @item_ProductBarCodeId_Value
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[@ef_filter__CurrentTenantId='?' (DbType = Guid), @p='?' (DbType = Guid)], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[CreatedAt], [s].[CreatedByUserId], [s].[IsDeleted], [s].[MinStockLevel], [s].[ProductBarcodeId], [s].[Quantity], [s].[RowVersion], [s].[TenantId], [s].[UpdatedAt], [s].[UpdatedByUserId], [s].[WarehouseId]
      FROM [StorgeStocks] AS [s]
      WHERE [s].[IsDeleted] = CAST(0 AS bit) AND [s].[TenantId] = @ef_filter__CurrentTenantId AND [s].[Id] = @p
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: The instance of entity type 'Product' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.ThrowIdentityConflict(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.StartTracking(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.InternalEntryBase.SetEntityState(EntityState oldState, EntityState newState, Boolean acceptChanges, Boolean modifyProperties)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityGraphAttacher.PaintAction(EntityEntryGraphNode`1 node)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityEntryGraphIterator.TraverseGraph[TState](EntityEntryGraphNode`1 node, Func`2 handleNode)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityEntryGraphIterator.TraverseGraph[TState](EntityEntryGraphNode`1 node, Func`2 handleNode)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityEntryGraphIterator.TraverseGraph[TState](EntityEntryGraphNode`1 node, Func`2 handleNode)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityGraphAttacher.AttachGraph(InternalEntityEntry rootEntry, EntityState targetState, EntityState storeGeneratedWithKeySetTargetState, Boolean forceStateWhenUnknownKey)
         at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.SetEntityState(InternalEntityEntry entry, EntityState entityState)
         at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.Attach(TEntity entity)
         at RetalSystemAPI.DataAccess.Repositories.Implementations.Repository`1.SoftDelete(T entity) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.DataAccess\Repositories\Implementations\Repository.cs:line 271
         at RetalSystemAPI.Services.Purchase.Implementations.PurchaseReturnService.DeleteAsync(Guid id, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.Services\Purchase\Implementations\PurchaseReturnService.cs:line 278
         at RetalSystemAPI.Controllers.Purchase.PurchaseReturnsController.Delete(Guid id, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI\Controllers\Purchase\PurchaseReturnsController.cs:line 109
         at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
         at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

مرتجع المبيعات يجب ان يتم ارجاعه الي الصالة لان الصالة تتعامل مع الصنف كل النكهات تحت بند واحد لكن في المخزن بتم التعامل مع كل نكهة على حدى اما توفير دعم الألية او اجبار الأرجاع الي الصالة 

تقييد الترجيع بفاتورة الا بصلاحية لتجازل هدا التقييد حتى لا يتم ارجاع الكميات بشكل خاطا او زيادة غير مبررة او خداع او اي شيء
عند حدف صنف من فاتروة مبيعات يتم حدفه لكن لا يتم اعادة الكمية و هدا كله خطا فواتير المبيعات يجب ان يكون لها حالات متل فواتير المشتريات بعد اعلاقها لا يتم تعديلها ابدا و الفواتير التي تاتي من نقطة البيع تعتبر فواتير مغلقة الحالة ومنتهية و الفواتير التي تاتي من العداد اليدوي لها كلام تاني يجب مراجعة هدا الأمر الامرو يجب ضافة زر بجانب كل صنف يقوم بانشاء فاتروة مرتجعات و يدخل بيناتها من الفاتورة المبيعات المفتوحة حاليا بدل الدهاب الي واجه المرتجعات و اعداداها من هناك

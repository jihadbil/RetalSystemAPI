namespace RetalSystemAPI.Services.Common.Models;

/// <summary>
/// فئة مساعدة داخلية لحساب وتوزيع قيم المرتجعات والخصومات والضرائب بنظام التقريب التراكمي لمنع فروقات الكسور.
/// </summary>
internal static class ReturnValuation
{
    /// <summary>
    /// توزيع مبلغ إجمالي على مجموعة أوزان أو مبالغ جزئية بنظام التقريب التراكمي، مما يضمن تطابق مجموع الأجزاء مع الإجمالي دون فقدان كسور.
    /// </summary>
    /// <param name="weights">قائمة الأوزان أو المبالغ النسبية لكل بند</param>
    /// <param name="total">المبلغ الإجمالي المراد توزيعه بدقة</param>
    /// <returns>مصفوفة بالمبالغ المخصصة لكل بند مقربة لأربع خانات عشرية</returns>
    public static decimal[] Allocate(IReadOnlyList<decimal> weights, decimal total)
    {
        // حساب مجموع كافة الأوزان الموزعة
        var sum = weights.Sum();

        // إنشاء مصفوفة لتخزين المبالغ الناتجة لكل وزن
        var amounts = new decimal[weights.Count];

        // متغير لتتبع مجموع الأوزان التراكمي حتى البند الحالي
        decimal weightSoFar = 0;

        // متغير لتتبع مجموع المبالغ الموزعة التراكمية حتى البند الحالي
        decimal amountSoFar = 0;

        // المرور على كافة الأوزان لحساب نصيب كل بند بدقة تراكمية
        for (var i = 0; i < weights.Count; i++)
        {
            // إضافة وزن البند الحالي إلى الوزن التراكمي
            weightSoFar += weights[i];

            // حساب المبلغ التراكمي المستحق حتى هذا البند وتقريبه لأربع خانات عشرية
            var cumulative = sum > 0 ? decimal.Round(total * weightSoFar / sum, 4) : 0;

            // تحديد نصيب هذا البند بطرح التراكمي السابق من التراكمي الحالي لمنع تراكم أخطاء التقريب
            amounts[i] = cumulative - amountSoFar;

            // تحديث المبلغ التراكمي السابق ليصبح مساوياً للمبلغ التراكمي الحالي
            amountSoFar = cumulative;
        }

        // إرجاع مصفوفة المبالغ الموزعة
        return amounts;
    }

    /// <summary>
    /// حساب القيمة التناسبية لمبلغ معين (كالخصم أو الضريبة) للكمية المرتجعة حالياً مع مراعاة الكميات المرتجعة سابقاً.
    /// </summary>
    /// <param name="amount">المبلغ الإجمالي المراد حساب النسبة منه (مثلاً إجمالي خصم السطر)</param>
    /// <param name="sold">إجمالي الكمية المباعة الأصلية في الفاتورة</param>
    /// <param name="previous">إجمالي الكمية المرتجعة سابقاً من هذا البند</param>
    /// <param name="requested">الكمية المطلوب إرجاعها في الحركة الحالية</param>
    /// <returns>المبلغ المستحق لهذه الكمية المرتجعة مع حفظ التناسب الدقيق</returns>
    public static decimal ForQuantity(decimal amount, decimal sold, decimal previous, decimal requested) =>
        // حساب الفرق بين القيمة التراكمية بعد إضافة الكمية الحالية والقيمة التراكمية للكميات السابقة فقط
        decimal.Round(amount * (previous + requested) / sold, 4) - decimal.Round(amount * previous / sold, 4);
}

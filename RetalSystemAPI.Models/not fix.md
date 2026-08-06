12. ⚠️ Tenant.PhoneNumber — Validation يتعارض مع BranchPhones.PhoneNumber
الملفات: 
Tenant.cs
 · 
BranchPhones.cs

كلاهما يحتوي على نفس Regex لرقم الهاتف ولكنه مكرر في مكانين. يجب استخراجه في:

ثابت const string PhonePattern، أو
ValidationConstants.cs مشتركة


1. ⚠️ اسم المجلد Branchs — خطأ إملائي
المجلد يجب أن يسمى Branches وليس Branchs.


9. ⚠️ BaseEntity لا تمتلك TenantId
الملف: 
BaseEntity.cs

كل نموذج يضيف TenantId يدوياً بشكل مكرر (Product، Category، Unit، ProductUnit، ProductBarCode، ProductImage، Branch، BranchPhones). هذا تكرار (DRY Violation).
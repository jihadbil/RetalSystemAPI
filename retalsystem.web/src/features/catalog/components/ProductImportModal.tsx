import React, { useState } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Badge } from '../../../shared/components/ui/Badge';
import { catalogService } from '../services/catalogService';
import type { ImportPreviewRow } from '../types/catalog.types';
import {
  IconFileUpload,
  IconFileSpreadsheet,
  IconCheck,
  IconAlertCircle,
  IconDownload,
} from '@tabler/icons-react';
import { useToast } from '../../../shared/context/ToastContext';

interface ProductImportModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const ProductImportModal: React.FC<ProductImportModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
}) => {
  const { showSuccess, showError } = useToast();
  const [step, setStep] = useState<1 | 2 | 3 | 4>(1);
  const [file, setFile] = useState<File | null>(null);
  const [previewRows, setPreviewRows] = useState<ImportPreviewRow[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isDownloadingTemplate, setIsDownloadingTemplate] = useState(false);
  const [importResult, setImportResult] = useState<{ imported: number; rejected: number } | null>(null);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0];
    if (!selected) return;
    setFile(selected);
    setIsLoading(true);
    try {
      const rows = await catalogService.previewImportFile(selected);
      setPreviewRows(rows);
      setStep(3); // الانتقال للمعاينة
    } catch {
      showError('خطأ في قراءة الملف', 'تعذر تحليل ملف Excel، تأكد من مطابقة الأعمدة للقالب.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleStartImport = async () => {
    if (!file) {
      showError('تعذر الاستيراد', 'اختر ملف Excel أولاً');
      return;
    }
    setIsLoading(true);
    setStep(4);
    try {
      const res = await catalogService.executeImport(file);
      setImportResult({ imported: res.productsImported, rejected: res.failedRowsCount });
      showSuccess('اكتمل الاستيراد', `تم استيراد ${res.productsImported} صنف بنجاح`);
      onSuccess();
    } catch {
      showError('فشل الاستيراد', 'حدث خطأ أثناء حفظ الأصناف');
      setStep(3);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDownloadTemplate = async () => {
    setIsDownloadingTemplate(true);
    try {
      await catalogService.downloadImportTemplate();
      showSuccess('تنزيل القالب', 'تم تنزيل قالب استيراد الأصناف');
    } catch (err: unknown) {
      showError('تعذر تنزيل القالب', err instanceof Error ? err.message : 'حدث خطأ أثناء تنزيل القالب');
    } finally {
      setIsDownloadingTemplate(false);
    }
  };

  const handleReset = () => {
    setStep(1);
    setFile(null);
    setPreviewRows([]);
    setImportResult(null);
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={() => {
        handleReset();
        onClose();
      }}
      title="معالج استيراد الأصناف عبر Excel"
      subtitle="استيراد وتحديث كتالوج المنتجات دفعة واحدة بملف إكسل"
      size="xl"
    >
      <div className="flex flex-col gap-5 py-1">
        {/* شريط خطوات المعالج الأربعة */}
        <div className="flex items-center justify-between border-b border-slate-100 pb-3 text-xs font-semibold">
          {[
            { s: 1, title: 'اختيار الملف' },
            { s: 2, title: 'مطابقة الأعمدة' },
            { s: 3, title: 'معاينة وفحص البيانات' },
            { s: 4, title: 'تنفيذ الاستيراد' },
          ].map((item) => (
            <div
              key={item.s}
              className={`flex items-center gap-1.5 ${
                step >= item.s ? 'text-emerald-700 font-bold' : 'text-slate-400'
              }`}
            >
              <span
                className={`w-6 h-6 rounded-full flex items-center justify-center text-xs ${
                  step >= item.s ? 'bg-emerald-600 text-white' : 'bg-slate-100 text-slate-500'
                }`}
              >
                {item.s}
              </span>
              <span className="hidden sm:inline">{item.title}</span>
            </div>
          ))}
        </div>

        {/* الخطوة 1: رفع الملف وتنزيل القالب */}
        {step === 1 && (
          <div className="flex flex-col items-center gap-4 py-6">
            <div className="w-full border-2 border-dashed border-slate-200 hover:border-emerald-400 rounded-2xl p-8 bg-slate-50/50 flex flex-col items-center text-center transition-colors">
              <IconFileSpreadsheet className="w-12 h-12 text-emerald-600 mb-3" />
              <h4 className="text-sm font-bold text-slate-800">اختر ملف Excel (.xlsx / .csv)</h4>
              <p className="text-xs text-slate-400 mt-1">يجب أن يحتوي الملف على أعمدة: الاسم، الكود، سعر البيع، التكلفة، والباركود</p>

              <label className="mt-4 cursor-pointer">
                <input
                  type="file"
                  accept=".xlsx, .xls, .csv"
                  onChange={handleFileChange}
                  className="hidden"
                />
                <span className="inline-flex items-center gap-2 h-10 px-5 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-bold transition-colors shadow-xs">
                  <IconFileUpload className="w-4 h-4" />
                  استعراض الملفات
                </span>
              </label>
            </div>

            <button
              type="button"
              onClick={handleDownloadTemplate}
              disabled={isDownloadingTemplate}
              className="flex items-center gap-1.5 text-xs text-emerald-700 hover:text-emerald-800 underline font-semibold cursor-pointer"
            >
              <IconDownload className="w-4 h-4" />
              {isDownloadingTemplate ? 'جاري تنزيل القالب...' : 'تنزيل قالب إكسل فارغ نموذجي (.xlsx)'}
            </button>
          </div>
        )}

        {/* الخطوة 3: فحص ومعاينة البيانات */}
        {step === 3 && (
          <div className="flex flex-col gap-3">
            <div className="flex items-center justify-between text-xs font-medium">
              <span className="text-slate-600">
                الملف المحدد: <strong className="text-slate-800">{file?.name}</strong> (
                {previewRows.length} سجل تم العثور عليه)
              </span>
              <div className="flex items-center gap-2">
                <Badge variant="success" size="sm">
                  {previewRows.filter((r) => r.isValid).length} صالح للاستيراد
                </Badge>
                {previewRows.some((r) => !r.isValid) && (
                  <Badge variant="danger" size="sm">
                    {previewRows.filter((r) => !r.isValid).length} أخطاء تحتاج تصحيح
                  </Badge>
                )}
              </div>
            </div>

            <div className="border border-slate-200 rounded-xl overflow-x-auto max-h-64">
              <table className="w-full text-right text-xs">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-semibold sticky top-0">
                  <tr>
                    <th className="py-2.5 px-3">#</th>
                    <th className="py-2.5 px-3">الاسم</th>
                    <th className="py-2.5 px-3">الكود</th>
                    <th className="py-2.5 px-3">الباركود</th>
                    <th className="py-2.5 px-3">السعر</th>
                    <th className="py-2.5 px-3">التكلفة</th>
                    <th className="py-2.5 px-3">الحالة</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {previewRows.map((row) => (
                    <tr
                      key={row.rowNumber}
                      className={row.isValid ? 'hover:bg-slate-50' : 'bg-rose-50/50'}
                    >
                      <td className="py-2.5 px-3 text-slate-400 font-mono">{row.rowNumber}</td>
                      <td className="py-2.5 px-3 font-bold text-slate-800">{row.name}</td>
                      <td className="py-2.5 px-3 text-slate-600 font-mono">{row.code}</td>
                      <td className="py-2.5 px-3 text-slate-500 font-mono num-ltr">{row.barCode || '-'}</td>
                      <td className="py-2.5 px-3 font-bold text-emerald-700 num-ltr">{row.salePrice.toFixed(2)}</td>
                      <td className="py-2.5 px-3 text-slate-500 num-ltr">{row.costPrice.toFixed(2)}</td>
                      <td className="py-2.5 px-3">
                        {row.isValid ? (
                          <span className="text-emerald-600 flex items-center gap-1 font-bold text-[11px]">
                            <IconCheck className="w-3.5 h-3.5" /> سليم
                          </span>
                        ) : (
                          <span className="text-rose-600 flex items-center gap-1 font-bold text-[11px]" title={row.error}>
                            <IconAlertCircle className="w-3.5 h-3.5" /> {row.error}
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div className="flex items-center justify-between pt-3 border-t border-slate-100">
              <Button variant="secondary" size="md" onClick={() => setStep(1)}>
                اختيار ملف آخر
              </Button>
              <Button
                variant="primary"
                size="md"
                onClick={handleStartImport}
                disabled={previewRows.filter((r) => r.isValid).length === 0}
              >
                تأكيد واستيراد ({previewRows.filter((r) => r.isValid).length}) صنف
              </Button>
            </div>
          </div>
        )}

        {/* الخطوة 4: نتيجة الاستيراد */}
        {step === 4 && (
          <div className="flex flex-col items-center gap-4 py-8 text-center">
            {isLoading ? (
              <>
                <svg className="animate-spin h-10 w-10 text-emerald-600 mb-2" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                <p className="text-sm font-bold text-slate-800">جاري معالجة الأصناف وإدخالها لقاعدة البيانات...</p>
              </>
            ) : (
              <>
                <div className="w-14 h-14 rounded-full bg-emerald-50 text-emerald-600 flex items-center justify-center mb-1">
                  <IconCheck className="w-8 h-8" />
                </div>
                <h4 className="text-base font-extrabold text-slate-900">تم إكمال عملية الاستيراد</h4>
                <p className="text-xs text-slate-600">
                  تم استيراد <strong>{importResult?.imported}</strong> صنف بنجاح إلى كتالوج الأصناف.
                </p>
                <Button
                  variant="primary"
                  size="md"
                  onClick={() => {
                    handleReset();
                    onClose();
                  }}
                  className="mt-3"
                >
                  إغلاق ومتابعة الكتالوج
                </Button>
              </>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};

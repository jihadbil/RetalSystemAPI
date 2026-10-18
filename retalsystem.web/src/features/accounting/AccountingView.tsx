import React, { useState, useEffect, useCallback } from 'react';
import {
  SafeDto,
  SafeType,
  SafeTypeLabels,
  ExpenseDto,
  ExpenseCategoryDto,
  SafeTransactionDto,
  TransactionDirection,
  PosTerminalDto,
} from './types/accounting.types';
import { accountingService } from './services/accountingService';
import { SafeFormModal } from './components/SafeFormModal';
import { SafeTransferModal } from './components/SafeTransferModal';
import { SafeTransactionModal } from './components/SafeTransactionModal';
import { ExpenseFormModal } from './components/ExpenseFormModal';
import { Button } from '../../shared/components/ui/Button';
import { Input } from '../../shared/components/ui/Input';
import { Select } from '../../shared/components/ui/Select';
import { Badge } from '../../shared/components/ui/Badge';
import { Tabs, TabItem } from '../../shared/components/ui/Tabs';
import { LoadingOverlay } from '../../shared/components/ui/LoadingOverlay';
import { EmptyState } from '../../shared/components/ui/EmptyState';
import { useDebounce } from '../../shared/hooks/useDebounce';
import { useToast } from '../../shared/context/ToastContext';
import { useAuth } from '../../shared/context/AuthContext';
import { Permissions } from '../../shared/auth/permissions';
import {
  IconVault,
  IconBuildingBank,
  IconArrowsExchange,
  IconArrowDownLeft,
  IconReceipt,
  IconPlus,
  IconSearch,
  IconTrash,
  IconEdit,
  IconWallet,
  IconDeviceDesktop,
} from '@tabler/icons-react';

export const AccountingView: React.FC = () => {
  const { showSuccess, showError } = useToast();
  const { currentBranchId, hasPermission } = useAuth();
  const canViewSafes = hasPermission(Permissions.Safes.View);
  const canCreateSafe = hasPermission(Permissions.Safes.Create);
  const canEditSafe = hasPermission(Permissions.Safes.Edit);
  const canTransfer = hasPermission(Permissions.Safes.Transfer);
  const canCreateTransaction = hasPermission(Permissions.Safes.Transactions);
  const canViewExpenses = hasPermission(Permissions.Expenses.View);
  const canCreateExpense = hasPermission(Permissions.Expenses.Create);
  const canDeleteExpense = hasPermission(Permissions.Expenses.Delete);
  const [selectedTab, setActiveTab] = useState('safes');
  const activeTab = (selectedTab === 'expenses' ? canViewExpenses : canViewSafes) ? selectedTab : canViewSafes ? 'safes' : 'expenses';

  // بيانات الخزائن
  const [safes, setSafes] = useState<SafeDto[]>([]);
  const [isLoadingSafes, setIsLoadingSafes] = useState(false);

  // بيانات المصروفات
  const [expenses, setExpenses] = useState<ExpenseDto[]>([]);
  const [categories, setCategories] = useState<ExpenseCategoryDto[]>([]);
  const [isLoadingExpenses, setIsLoadingExpenses] = useState(false);
  const [expenseSearch, setExpenseSearch] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('');
  const debouncedExpenseSearch = useDebounce(expenseSearch, 300);

  // حركات الخزينة
  const [selectedSafeForTrx, setSelectedSafeForTrx] = useState<string>('');
  const [transactions, setTransactions] = useState<SafeTransactionDto[]>([]);
  const [isLoadingTrx, setIsLoadingTrx] = useState(false);
  const [terminals, setTerminals] = useState<PosTerminalDto[]>([]);
  const [isLoadingTerminals, setIsLoadingTerminals] = useState(false);

  // النوافذ المنبثقة
  const [isSafeFormOpen, setIsSafeFormOpen] = useState(false);
  const [safeToEdit, setSafeToEdit] = useState<SafeDto | null>(null);

  const [isTransferOpen, setIsTransferOpen] = useState(false);

  const [isTransactionOpen, setIsTransactionOpen] = useState(false);
  const [safeForTransaction, setSafeForTransaction] = useState<SafeDto | null>(null);

  const [isExpenseFormOpen, setIsExpenseFormOpen] = useState(false);

  const [accountingError, setAccountingError] = useState<string | null>(null);

  const loadSafes = useCallback(async () => {
    try {
      setIsLoadingSafes(true);
      setAccountingError(null);
      const data = await accountingService.getSafes(currentBranchId || undefined);
      setSafes(data);
      if (!data.some((safe) => safe.id === selectedSafeForTrx)) setSelectedSafeForTrx(data[0]?.id || '');
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل بيانات الخزائن';
      setAccountingError(msg);
      showError('خطأ في تحميل الخزائن', msg);
    } finally {
      setIsLoadingSafes(false);
    }
  }, [selectedSafeForTrx, showError, currentBranchId]);

  const loadExpenses = useCallback(async () => {
    try {
      setIsLoadingExpenses(true);
      const [expResult, cats] = await Promise.all([
        accountingService.getExpenses(1, 20, selectedCategory || undefined, undefined, debouncedExpenseSearch, currentBranchId || undefined),
        accountingService.getExpenseCategories(),
      ]);
      setExpenses(expResult.items);
      setCategories(cats);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل المصروفات والتصنيفات';
      setAccountingError(msg);
      showError('خطأ في تحميل المصروفات', msg);
    } finally {
      setIsLoadingExpenses(false);
    }
  }, [selectedCategory, debouncedExpenseSearch, showError, currentBranchId]);

  const loadTransactions = useCallback(async () => {
    if (!selectedSafeForTrx) return;
    try {
      setIsLoadingTrx(true);
      const res = await accountingService.getSafeTransactions(selectedSafeForTrx);
      setTransactions(res.items);
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'فشل تحميل حركات الخزينة';
      showError('خطأ في كشف حركات الخزينة', msg);
    } finally {
      setIsLoadingTrx(false);
    }
  }, [selectedSafeForTrx, showError]);

  const loadTerminals = useCallback(async () => {
    try { setIsLoadingTerminals(true); setTerminals((await accountingService.getPosTerminals()).filter((terminal) => !currentBranchId || terminal.branchId === currentBranchId)); }
    catch (err) { showError('خطأ في نقاط البيع', err instanceof Error ? err.message : 'تعذر تحميل نقاط البيع'); }
    finally { setIsLoadingTerminals(false); }
  }, [showError, currentBranchId]);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      if (canViewSafes) void loadSafes();
      if (canViewExpenses) void loadExpenses();
    }, 0);
    return () => window.clearTimeout(timer);
  }, [canViewSafes, canViewExpenses, loadSafes, loadExpenses]);

  useEffect(() => {
    if (activeTab === 'transactions' && selectedSafeForTrx) {
      const timer = window.setTimeout(() => { void loadTransactions(); }, 0);
      return () => window.clearTimeout(timer);
    }
  }, [activeTab, selectedSafeForTrx, loadTransactions]);

  useEffect(() => {
    if (activeTab !== 'terminals') return;
    const timer = window.setTimeout(() => { void loadTerminals(); }, 0);
    return () => window.clearTimeout(timer);
  }, [activeTab, loadTerminals]);

  // إجمالي السيولة النقدية في النظام
  const totalLiquidity = safes.reduce((acc, s) => acc + (s.currentBalance || 0), 0);
  const totalExpensesAmount = expenses.reduce((acc, e) => acc + (e.amount || 0), 0);

  const tabs: TabItem[] = [
    ...(canViewSafes ? [{ id: 'safes', label: 'الخزائن والحسابات المصرفية', icon: <IconVault className="w-4 h-4" /> }] : []),
    ...(canViewExpenses
      ? [{ id: 'expenses', label: 'سجل المصروفات والنفقات', icon: <IconReceipt className="w-4 h-4" /> }]
      : []),
    ...(canViewSafes ? [
      { id: 'transactions', label: 'كشف حركات الصندوق', icon: <IconArrowsExchange className="w-4 h-4" /> },
      { id: 'terminals', label: 'نقاط البيع والأدراج', icon: <IconDeviceDesktop className="w-4 h-4" /> },
    ] : []),
  ];

  return (
    <div className="flex flex-col gap-5">
      {/* الترويسة الرئيسية */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-black text-slate-900 tracking-tight">الخزائن والإدارة المالية</h1>
          <p className="text-xs text-slate-500 font-medium mt-0.5">
            إدارة أرصدة الصناديق، الحسابات البنكية، التحويلات، وقيد المصروفات
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {canTransfer && <Button
            variant="outline"
            size="sm"
            onClick={() => setIsTransferOpen(true)}
            className="text-emerald-700 border-emerald-300 hover:bg-emerald-50"
          >
            <IconArrowsExchange className="w-4 h-4" />
            تحويل مالي
          </Button>}

          {canCreateTransaction && <Button
            variant="outline"
            size="sm"
            onClick={() => {
              setSafeForTransaction(safes[0] || null);
              setIsTransactionOpen(true);
            }}
          >
            <IconArrowDownLeft className="w-4 h-4" />
            إيداع / سحب مباشر
          </Button>}

          {canCreateExpense && <Button
            variant="primary"
            size="sm"
            onClick={() => setIsExpenseFormOpen(true)}
          >
            <IconPlus className="w-4 h-4" />
            سند مصروف جديد
          </Button>}
        </div>
      </div>

      {/* بطاقة إجمالي السيولة */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="p-4 rounded-2xl bg-gradient-to-br from-emerald-800 to-emerald-950 text-white shadow-xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-emerald-200">إجمالي السيولة النقدية والبنكية</span>
            <IconWallet className="w-5 h-5 text-emerald-300" />
          </div>
          <div className="text-2xl font-black mt-2 num-ltr text-right">
            {totalLiquidity.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
            <span className="text-xs font-medium text-emerald-200">د.ل</span>
          </div>
          <div className="text-[11px] text-emerald-300/80 mt-1">
            موزعة على {safes.length} حسابات وصناديق نقدية
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-slate-400">إجمالي المصروفات المسجلة</span>
            <div className="text-2xl font-black text-slate-900 num-ltr text-right mt-1">
              {totalExpensesAmount.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
              <span className="text-xs font-normal">د.ل</span>
            </div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-slate-50 text-slate-700 flex items-center justify-center">
            <IconReceipt className="w-6 h-6" />
          </div>
        </div>

        <div className="p-4 rounded-2xl bg-white border border-slate-200/80 shadow-2xs flex items-center justify-between">
          <div>
            <span className="text-xs font-bold text-slate-400">عدد بنود المصروفات النشطة</span>
            <div className="text-2xl font-black text-emerald-700 mt-1">{categories.length}</div>
          </div>
          <div className="w-12 h-12 rounded-xl bg-emerald-50 text-emerald-600 flex items-center justify-center">
            <IconVault className="w-6 h-6" />
          </div>
        </div>
      </div>

      {/* تبويبات الشاشة */}
      <Tabs tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />

      {accountingError && (
        <div className="p-4 rounded-xl bg-rose-50 border border-rose-200 text-rose-800 text-sm flex items-center justify-between gap-3 animate-fade-in">
          <div>
            <p className="font-bold text-rose-900">تعذر تحميل البيانات من الخادم:</p>
            <p className="text-xs text-rose-700 mt-0.5">{accountingError}</p>
          </div>
          <Button
            variant="secondary"
            size="sm"
            onClick={() => {
              loadSafes();
              loadExpenses();
            }}
          >
            إعادة المحاولة
          </Button>
        </div>
      )}

      {/* المحتوى حسب التبويب النشط */}
      {activeTab === 'safes' && (
        <div className="flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <span className="text-sm font-bold text-slate-800">قائمة الصناديق والحسابات البنكية</span>
            {canCreateSafe && <Button
              variant="secondary"
              size="sm"
              onClick={() => {
                setSafeToEdit(null);
                setIsSafeFormOpen(true);
              }}
            >
              <IconPlus className="w-4 h-4" />
              إضافة خزينة / حساب
            </Button>}
          </div>

          <div className="relative grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <LoadingOverlay isVisible={isLoadingSafes} />
            {safes.map((safe) => {
              const isBank = safe.type === SafeType.Bank;
              const isDrawer = safe.type === SafeType.CashDrawer;

              return (
                <div
                  key={safe.id}
                  className="p-5 rounded-2xl bg-white border border-slate-200/80 shadow-2xs flex flex-col justify-between gap-4 hover:border-emerald-300 transition-all"
                >
                  <div>
                    <div className="flex items-start justify-between gap-2">
                      <div className="flex items-center gap-2.5">
                        <div
                          className={`w-10 h-10 rounded-xl flex items-center justify-center ${
                            isBank
                              ? 'bg-blue-50 text-blue-700'
                              : isDrawer
                              ? 'bg-amber-50 text-amber-700'
                              : 'bg-emerald-50 text-emerald-700'
                          }`}
                        >
                          {isBank ? <IconBuildingBank className="w-5 h-5" /> : <IconVault className="w-5 h-5" />}
                        </div>
                        <div>
                          <h3 className="font-extrabold text-sm text-slate-900 leading-tight">
                            {safe.name}
                          </h3>
                          <span className="text-[11px] text-slate-400 font-medium">
                            {SafeTypeLabels[safe.type]}
                          </span>
                        </div>
                      </div>

                      <Badge variant={safe.isActive ? 'success' : 'neutral'}>
                        {safe.isActive ? 'نشط' : 'معطل'}
                      </Badge>
                    </div>

                    {isBank && safe.bankAccountNumber && (
                      <div className="mt-3 p-2 bg-slate-50 rounded-xl font-mono text-[11px] text-slate-600 num-ltr text-right truncate">
                        {safe.bankAccountNumber}
                      </div>
                    )}
                  </div>

                  <div className="pt-3 border-t border-slate-100 flex items-end justify-between">
                    <div>
                      <span className="text-[11px] font-bold text-slate-400">الرصيد المتاح</span>
                      <div className="text-xl font-black text-slate-900 num-ltr text-right">
                        {safe.currentBalance.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}{' '}
                        <span className="text-xs font-normal">د.ل</span>
                      </div>
                    </div>

                    <div className="flex items-center gap-1">
                      {canCreateTransaction && <button
                        type="button"
                        title="إيداع / سحب"
                        onClick={() => {
                          setSafeForTransaction(safe);
                          setIsTransactionOpen(true);
                        }}
                        className="p-1.5 rounded-lg text-emerald-700 hover:bg-emerald-50 transition-colors"
                      >
                        <IconArrowDownLeft className="w-4 h-4" />
                      </button>}

                      {canEditSafe && <button
                        type="button"
                        title="تعديل الحساب"
                        onClick={() => {
                          setSafeToEdit(safe);
                          setIsSafeFormOpen(true);
                        }}
                        className="p-1.5 rounded-lg text-slate-500 hover:bg-slate-100 transition-colors"
                      >
                        <IconEdit className="w-4 h-4" />
                      </button>}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {activeTab === 'terminals' && (
        <div className="flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-sm font-bold text-slate-800">نقاط البيع المرتبطة بأدراج النقود الفعلية</h2>
              <p className="text-xs text-slate-500 mt-1">أنشئ نقطة بيع ثم ولّد رمزاً صالحاً لخمس دقائق لربط المتصفح أو برنامج سطح المكتب.</p>
            </div>
            {canEditSafe && <Button variant="primary" size="sm" onClick={async () => {
              const drawers = safes.filter((s) => s.type === SafeType.CashDrawer && s.isActive && (!currentBranchId || s.branchId === currentBranchId));
              if (!currentBranchId || drawers.length === 0) { showError('تعذر الإنشاء', 'اختر فرعاً وأنشئ درج نقود نشطاً فيه أولاً'); return; }
              const code = window.prompt('رمز نقطة البيع (مثال POS-01)');
              if (!code?.trim()) return;
              const name = window.prompt('اسم نقطة البيع');
              if (!name?.trim()) return;
              const drawerLabel = drawers.map((d, i) => `${i + 1}: ${d.name}`).join('\n');
              const selection = Number(window.prompt(`اختر رقم درج النقود:\n${drawerLabel}`, '1')) - 1;
              if (!drawers[selection]) return;
              try {
                await accountingService.createPosTerminal({ code: code.trim(), name: name.trim(), branchId: currentBranchId, defaultCashDrawerId: drawers[selection].id });
                showSuccess('تم إنشاء نقطة البيع'); await loadTerminals();
              } catch (err) { showError('فشل إنشاء نقطة البيع', err instanceof Error ? err.message : 'خطأ غير متوقع'); }
            }}>
              <IconPlus className="w-4 h-4" /> إضافة نقطة بيع
            </Button>}
          </div>
          <div className="relative grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            <LoadingOverlay isVisible={isLoadingTerminals} />
            {terminals.map((terminal) => (
              <div key={terminal.id} className="p-4 rounded-2xl bg-white border border-slate-200 shadow-2xs">
                <div className="flex items-start justify-between gap-2">
                  <div><h3 className="font-extrabold text-slate-900">{terminal.name}</h3><p className="text-xs font-mono text-slate-500">{terminal.code}</p></div>
                  <Badge variant={terminal.isActive ? 'success' : 'neutral'}>{terminal.isActive ? 'نشطة' : 'معطلة'}</Badge>
                </div>
                <div className="text-xs text-slate-600 mt-3 space-y-1">
                  <p>الفرع: {terminal.branchName}</p><p>درج النقود: {terminal.defaultCashDrawerName}</p>
                </div>
                {canEditSafe && <Button className="mt-4 w-full" variant="secondary" size="sm" onClick={async () => {
                  try {
                    const pairing = await accountingService.generatePairingCode(terminal.id);
                    window.alert(`رمز ربط ${terminal.name}: ${pairing.pairingCode}\nصالح حتى: ${new Date(pairing.expiresAt).toLocaleTimeString('ar-LY')}`);
                  } catch (err) { showError('فشل توليد رمز الربط', err instanceof Error ? err.message : 'خطأ غير متوقع'); }
                }}>توليد رمز ربط الجهاز</Button>}
              </div>
            ))}
          </div>
        </div>
      )}

      {activeTab === 'expenses' && (
        <div className="flex flex-col gap-4">
          {/* شريط البحث وفلترة المصروفات */}
          <div className="p-3 bg-white rounded-2xl border border-slate-200/80 shadow-2xs flex flex-wrap items-center justify-between gap-3">
            <div className="flex items-center gap-3 w-full sm:w-auto flex-1 max-w-lg">
              <Input
                placeholder="بحث برقم السند أو البيان..."
                value={expenseSearch}
                onChange={(e) => setExpenseSearch(e.target.value)}
                leftIcon={<IconSearch className="w-4 h-4 text-slate-400" />}
              />

              <div className="w-48">
                <Select
                  value={selectedCategory}
                  onChange={(e) => setSelectedCategory(e.target.value)}
                  options={[
                    { value: '', label: 'جميع بنود المصروفات' },
                    ...categories.map((c) => ({ value: c.id, label: c.name })),
                  ]}
                />
              </div>
            </div>

            {canCreateExpense && <Button variant="primary" size="sm" onClick={() => setIsExpenseFormOpen(true)}>
              <IconPlus className="w-4 h-4" />
              إضافة سند مصروف
            </Button>}
          </div>

          {/* جدول سندات المصروفات */}
          <div className="relative bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
            <LoadingOverlay isVisible={isLoadingExpenses} />

            {expenses.length === 0 && !isLoadingExpenses ? (
              <EmptyState
                icon={<IconReceipt className="w-10 h-10 text-slate-300" />}
                title="لا توجد سندات مصروفات مسجلة"
                description="لم يتم قيد أي مصروفات تطابق معايير البحث المحددة."
                actionLabel={canCreateExpense ? 'سند مصروف جديد' : undefined}
                onAction={canCreateExpense ? () => setIsExpenseFormOpen(true) : undefined}
              />
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-right text-xs min-w-[650px]">
                  <thead className="bg-slate-50/90 border-b border-slate-200 text-slate-500 font-bold">
                    <tr>
                      <th className="py-3 px-4">رقم السند</th>
                      <th className="py-3 px-4">التاريخ</th>
                      <th className="py-3 px-4">بند المصروف</th>
                      <th className="py-3 px-4">الخزينة المسدد منها</th>
                      <th className="py-3 px-4">طريقة الدفع</th>
                      <th className="py-3 px-4">البيان / الوصف</th>
                      <th className="py-3 px-4 text-left">المبلغ (د.ل)</th>
                      <th className="py-3 px-4 text-center">إجراء</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 font-medium">
                    {expenses.map((exp) => (
                      <tr key={exp.id} className="hover:bg-slate-50/70 transition-colors">
                        <td className="py-3 px-4 font-mono font-bold text-slate-800 num-ltr text-right">
                          {exp.expenseNumber}
                        </td>
                        <td className="py-3 px-4 text-slate-500 num-ltr text-right">{exp.expenseDate}</td>
                        <td className="py-3 px-4 font-bold text-slate-900">{exp.expenseCategoryName}</td>
                        <td className="py-3 px-4 text-slate-600">{exp.safeName || '-'}</td>
                        <td className="py-3 px-4 text-slate-500">{exp.paymentMethodName}</td>
                        <td className="py-3 px-4 text-slate-600 max-w-xs truncate">{exp.description}</td>
                        <td className="py-3 px-4 text-left font-black num-ltr text-rose-700 text-sm">
                          {exp.amount.toLocaleString('ar-SA', { minimumFractionDigits: 2 })}
                        </td>
                        <td className="py-3 px-4 text-center">
                          {canDeleteExpense && <button
                            type="button"
                            title="حذف سند المصروف"
                            onClick={async () => {
                              await accountingService.deleteExpense(exp.id);
                              showSuccess('تم حذف سند المصروف');
                              loadExpenses();
                            }}
                            className="p-1.5 rounded-lg text-rose-600 hover:bg-rose-50 transition-colors"
                          >
                            <IconTrash className="w-4 h-4" />
                          </button>}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {activeTab === 'transactions' && (
        <div className="flex flex-col gap-4">
          <div className="p-3.5 bg-white rounded-2xl border border-slate-200/80 shadow-2xs flex items-center justify-between">
            <div className="flex items-center gap-3">
              <span className="text-xs font-bold text-slate-700">اختر الخزينة / الحساب:</span>
              <div className="w-72">
                <Select
                  value={selectedSafeForTrx}
                  onChange={(e) => setSelectedSafeForTrx(e.target.value)}
                  options={safes.map((s) => ({
                    value: s.id,
                    label: `${s.name} (${s.currentBalance.toLocaleString('ar-SA')} د.ل)`,
                  }))}
                />
              </div>
            </div>

            <Button variant="secondary" size="sm" onClick={loadTransactions}>
              تحديث الكشف
            </Button>
          </div>

          <div className="relative bg-white rounded-2xl border border-slate-200/80 shadow-2xs overflow-hidden">
            <LoadingOverlay isVisible={isLoadingTrx} />
            <div className="overflow-x-auto">
              <table className="w-full text-right text-xs min-w-[700px]">
                <thead className="bg-slate-50 border-b border-slate-200 text-slate-500 font-bold">
                  <tr>
                    <th className="py-3 px-4">التاريخ</th>
                    <th className="py-3 px-4">نوع المستند</th>
                    <th className="py-3 px-4">المرجع</th>
                    <th className="py-3 px-4">البيان</th>
                    <th className="py-3 px-4 text-left">وارد / إيداع (+)</th>
                    <th className="py-3 px-4 text-left">صادر / سحب (-)</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 font-medium">
                  {transactions.map((tr) => (
                    <tr key={tr.id} className="hover:bg-slate-50/70 transition-colors">
                      <td className="py-3 px-4 text-slate-500 num-ltr text-right">{tr.transactionDate}</td>
                      <td className="py-3 px-4 font-bold text-slate-800">{tr.documentTypeName || 'حركة نقدية'}</td>
                      <td className="py-3 px-4 font-mono text-slate-600 num-ltr text-right">
                        {tr.transactionNumber || '-'}
                      </td>
                      <td className="py-3 px-4 text-slate-700">{tr.description}</td>
                      <td className="py-3 px-4 text-left num-ltr font-black text-emerald-600">
                        {tr.direction === TransactionDirection.Inbound
                          ? tr.amount.toLocaleString('ar-SA', { minimumFractionDigits: 2 })
                          : '-'}
                      </td>
                      <td className="py-3 px-4 text-left num-ltr font-black text-rose-600">
                        {tr.direction === TransactionDirection.Outbound
                          ? tr.amount.toLocaleString('ar-SA', { minimumFractionDigits: 2 })
                          : '-'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* النوافذ المنبثقة */}
      <SafeFormModal
        isOpen={isSafeFormOpen}
        onClose={() => setIsSafeFormOpen(false)}
        safeToEdit={safeToEdit}
        onSaved={loadSafes}
      />

      <SafeTransferModal
        isOpen={isTransferOpen}
        onClose={() => setIsTransferOpen(false)}
        safes={safes}
        onTransferSuccess={loadSafes}
      />

      <SafeTransactionModal
        isOpen={isTransactionOpen}
        onClose={() => setIsTransactionOpen(false)}
        safe={safeForTransaction}
        onTransactionSuccess={loadSafes}
      />

      <ExpenseFormModal
        isOpen={isExpenseFormOpen}
        onClose={() => setIsExpenseFormOpen(false)}
        categories={categories}
        safes={safes}
        onSaved={() => {
          loadExpenses();
          loadSafes();
        }}
      />
    </div>
  );
};

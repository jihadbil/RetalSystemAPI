import React, { useState, useEffect } from 'react';
import { Modal } from '../../../shared/components/ui/Modal';
import { Button } from '../../../shared/components/ui/Button';
import { Input } from '../../../shared/components/ui/Input';
import { Select } from '../../../shared/components/ui/Select';
import { Tabs } from '../../../shared/components/ui/Tabs';
import type { ProductDto, CategoryDto, UnitDto, CreateProductRequest } from '../types/catalog.types';
import { stockService } from '../../stock/services/stockService';
import type { WarehouseDto } from '../../stock/types/stock.types';
import {
  IconInfoCircle,
  IconCoin,
  IconBarcode,
  IconBuildingWarehouse,
  IconPlus,
  IconTrash,
} from '@tabler/icons-react';

interface ProductFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  productToEdit?: ProductDto | null;
  categories: CategoryDto[];
  units: UnitDto[];
  onSave: (data: CreateProductRequest) => Promise<void>;
}

export const ProductFormModal: React.FC<ProductFormModalProps> = ({
  isOpen,
  onClose,
  productToEdit,
  categories,
  units,
  onSave,
}) => {
  const [activeTab, setActiveTab] = useState<string>('basic');
  const [isLoading, setIsLoading] = useState(false);

  // المستودعات الحقيقية
  const [warehouses, setWarehouses] = useState<WarehouseDto[]>([]);

  // حقول النموذج
  const [name, setName] = useState('');
  const [code, setCode] = useState('');
  const [description, setDescription] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [defaultUnitId, setDefaultUnitId] = useState('');
  const [costPrice, setCostPrice] = useState<number>(0);
  const [salePrice, setSalePrice] = useState<number>(0);

  // باركودات إضافية
  const [barcodes, setBarcodes] = useState<{ barCode: string; description: string }[]>([
    { barCode: '', description: 'الباركود الرئيسي' },
  ]);

  // أرصدة افتتاحية
  const [openingBalances, setOpeningBalances] = useState<
    { warehouseId: string; warehouseType?: number; quantity: number }[]
  >([]);

  useEffect(() => {
    if (isOpen) {
      stockService
        .getWarehouses()
        .then((whs) => {
          const activeWhs = (whs || []).filter((w) => w.isActive);
          setWarehouses(activeWhs);
        })
        .catch((err) => {
          console.error('Failed to load warehouses:', err);
        });
    }
  }, [isOpen]);

  useEffect(() => {
    if (isOpen) {
      if (productToEdit) {
        setName(productToEdit.name);
        setCode(productToEdit.code);
        setDescription(productToEdit.description || '');
        setCategoryId(productToEdit.categoryId);
        setDefaultUnitId(productToEdit.defaultUnitId || '');
        setCostPrice(productToEdit.costPrice);
        setSalePrice(productToEdit.salePrice);
        setBarcodes(
          productToEdit.barCodes && productToEdit.barCodes.length > 0
            ? productToEdit.barCodes.map((b) => ({ barCode: b.barCode, description: b.description || '' }))
            : [{ barCode: '', description: 'الباركود الرئيسي' }]
        );
      } else {
        setName('');
        setCode(`PRD-${Math.floor(1000 + Math.random() * 9000)}`);
        setDescription('');
        setCategoryId(categories[0]?.id || '');
        setDefaultUnitId(units[0]?.id || '');
        setCostPrice(0);
        setSalePrice(0);
        setBarcodes([{ barCode: '', description: 'الباركود الرئيسي' }]);
        setOpeningBalances([]);
      }
      setActiveTab('basic');
    }
  }, [isOpen, productToEdit, categories, units]);

  const handleAddBarcode = () => {
    setBarcodes((prev) => [...prev, { barCode: '', description: `باركود ${prev.length + 1}` }]);
  };

  const handleRemoveBarcode = (index: number) => {
    setBarcodes((prev) => prev.filter((_, idx) => idx !== index));
  };

  const handleBarcodeChange = (index: number, field: 'barCode' | 'description', value: string) => {
    setBarcodes((prev) => {
      const updated = [...prev];
      updated[index][field] = value;
      return updated;
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;

    setIsLoading(true);
    try {
      await onSave({
        name: name.trim(),
        code: code.trim(),
        description: description.trim(),
        categoryId: categoryId || categories[0]?.id,
        defaultUnitId: defaultUnitId || units[0]?.id,
        costPrice: Number(costPrice) || 0,
        salePrice: Number(salePrice) || 0,
        barCodes: barcodes.filter((b) => b.barCode.trim().length > 0),
        units: defaultUnitId
          ? [{ unitId: defaultUnitId, conversionFactor: 1, isDefault: true }]
          : [],
        openingBalances: openingBalances.filter((o) => o.quantity > 0),
      });
      onClose();
    } finally {
      setIsLoading(false);
    }
  };

  const tabs = [
    { id: 'basic', label: 'البيانات الأساسية', icon: <IconInfoCircle className="w-4 h-4" /> },
    { id: 'pricing', label: 'الأسعار والوحدات', icon: <IconCoin className="w-4 h-4" /> },
    { id: 'barcodes', label: 'الباركودات المتعددة', icon: <IconBarcode className="w-4 h-4" />, badge: barcodes.filter((b) => b.barCode).length },
    ...(!productToEdit
      ? [{ id: 'opening', label: 'الأرصدة الافتتاحية (اختياري)', icon: <IconBuildingWarehouse className="w-4 h-4" /> }]
      : []),
  ];

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={productToEdit ? 'تعديل بيانات الصنف' : 'إضافة صنف جديد'}
      subtitle="أدخل بيانات الصنف بدقة، يمكنك إضافة باركودات وأسعار ووحدات متعددة"
      size="xl"
      footer={
        <>
          <Button variant="secondary" size="md" onClick={onClose} disabled={isLoading}>
            إلغاء
          </Button>
          <Button variant="primary" size="md" onClick={handleSubmit} isLoading={isLoading}>
            {productToEdit ? 'حفظ التعديلات' : 'إضافة الصنف'}
          </Button>
        </>
      }
    >
      <div className="flex flex-col gap-4">
        {/* شريط التبويبات الخمسة */}
        <Tabs tabs={tabs} activeTab={activeTab} onChange={setActiveTab} />

        {/* محتوى التبويب 1: البيانات الأساسية */}
        {activeTab === 'basic' && (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2">
            <div className="sm:col-span-2">
              <Input
                label="اسم الصنف التجاري *"
                placeholder="مثال: قهوة اسبريسو فاخرة 1 كجم"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
                autoFocus
              />
            </div>

            <Input
              label="كود الصنف (SKU) *"
              placeholder="PRD-1001"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              isLtr
              required
            />

            <Select
              label="التصنيف الأساسي"
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
            >
              {categories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.parentCategoryName ? `${c.parentCategoryName} ← ${c.name}` : c.name}
                </option>
              ))}
            </Select>

            <Select
              label="الوحدة الافتراضية للبيع"
              value={defaultUnitId}
              onChange={(e) => setDefaultUnitId(e.target.value)}
            >
              {units.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.name} ({u.symbol})
                </option>
              ))}
            </Select>

            <div className="sm:col-span-2">
              <label className="text-xs font-semibold text-slate-700 block mb-1">وصف الصنف وملاحظاته</label>
              <textarea
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="أضف وصفاً توضيحياً لمكونات أو خصائص الصنف..."
                className="w-full text-xs p-3 rounded-lg border border-slate-200 focus:ring-2 focus:ring-emerald-500 focus:outline-none"
                rows={2}
              />
            </div>

          </div>
        )}

        {/* محتوى التبويب 2: الأسعار والوحدات */}
        {activeTab === 'pricing' && (
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 py-2">
            <Input
              label="سعر التكلفة (الشراء) د.ل *"
              type="number"
              step="0.1"
              min="0"
              value={costPrice}
              onChange={(e) => setCostPrice(Number(e.target.value))}
              isLtr
              required
            />

            <Input
              label="سعر البيع الافتراضي د.ل *"
              type="number"
              step="0.1"
              min="0"
              value={salePrice}
              onChange={(e) => setSalePrice(Number(e.target.value))}
              isLtr
              required
            />

            <div className="sm:col-span-2 p-3 rounded-xl bg-emerald-50/50 border border-emerald-100 flex items-center justify-between text-xs">
              <span className="font-semibold text-slate-700">هامش الربح التقديري:</span>
              <span className="font-bold text-emerald-800 num-ltr">
                {salePrice > costPrice
                  ? `${(salePrice - costPrice).toFixed(2)} د.ل (${(((salePrice - costPrice) / Math.max(1, costPrice)) * 100).toFixed(0)}%)`
                  : '0.00 د.ل (0%)'}
              </span>
            </div>
          </div>
        )}

        {/* محتوى التبويب 3: الباركودات المتعددة */}
        {activeTab === 'barcodes' && (
          <div className="flex flex-col gap-3 py-2">
            <div className="flex items-center justify-between">
              <p className="text-xs text-slate-500">
                يمكنك ربط عدة باركودات لنفس الصنف (مثلاً: باركود القطعة، باركود الصندوق، أو أشكال العبوة المختلفة)
              </p>
              <Button variant="secondary" size="sm" leftIcon={<IconPlus className="w-3.5 h-3.5 text-emerald-600" />} onClick={handleAddBarcode}>
                + إضافة باركود
              </Button>
            </div>

            <div className="flex flex-col gap-2 max-h-60 overflow-y-auto">
              {barcodes.map((item, idx) => (
                <div key={idx} className="flex items-center gap-2 p-2 rounded-lg bg-slate-50 border border-slate-200">
                  <div className="flex-1">
                    <Input
                      placeholder="امسح أو اكتب الباركود..."
                      value={item.barCode}
                      onChange={(e) => handleBarcodeChange(idx, 'barCode', e.target.value)}
                      isLtr
                      className="h-9 font-mono"
                    />
                  </div>
                  <div className="flex-1">
                    <Input
                      placeholder="وصف الباركود (مثال: نكهة توت / كرتونة)"
                      value={item.description}
                      onChange={(e) => handleBarcodeChange(idx, 'description', e.target.value)}
                      className="h-9"
                    />
                  </div>
                  {barcodes.length > 1 && (
                    <button
                      type="button"
                      onClick={() => handleRemoveBarcode(idx)}
                      className="p-2 text-slate-400 hover:text-rose-600 transition-colors"
                      title="حذف الباركود"
                    >
                      <IconTrash className="w-4 h-4" />
                    </button>
                  )}
                </div>
              ))}
            </div>
          </div>
        )}

        {/* الأرصدة الافتتاحية متاحة عند إنشاء الصنف فقط لأن API التعديل لا يغير الرصيد. */}
        {activeTab === 'opening' && (
          <div className="flex flex-col gap-3 py-2">
            <p className="text-xs text-slate-500">
              قسم اختياري: يمكنك تسجيل كميات المخزون الأولية المتوفرة حالياً في مستودعاتك لهذا الصنف:
            </p>
            {warehouses.length > 0 ? (
              <div className="flex flex-col gap-2">
                {warehouses.map((wh) => {
                  const currentBalance = openingBalances.find((b) => b.warehouseId === wh.id)?.quantity || 0;
                  return (
                    <div
                      key={wh.id}
                      className="p-3 rounded-lg border border-slate-200 bg-white flex items-center justify-between"
                    >
                      <div>
                        <span className="text-xs font-bold text-slate-800">{wh.name}</span>
                        <span className="text-[11px] text-slate-400 block">{wh.typeName || (wh.type === 2 ? 'صالة عرض' : 'مستودع تخزيني')}</span>
                      </div>
                      <div className="w-32">
                        <Input
                          type="number"
                          min="0"
                          placeholder="0"
                          value={currentBalance}
                          onChange={(e) => {
                            const val = Number(e.target.value);
                            setOpeningBalances((prev) => {
                              const filtered = prev.filter((b) => b.warehouseId !== wh.id);
                              if (val > 0) {
                                return [...filtered, { warehouseId: wh.id, warehouseType: wh.type, quantity: val }];
                              }
                              return filtered;
                            });
                          }}
                          isLtr
                          className="h-9 text-center font-bold"
                        />
                      </div>
                    </div>
                  );
                })}
              </div>
            ) : (
              <div className="p-4 rounded-lg bg-slate-50 text-slate-500 text-xs text-center border border-slate-200">
                لا توجد مستودعات نشطة مسجلة في المنشأة حالياً
              </div>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
};

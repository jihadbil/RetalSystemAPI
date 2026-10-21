import React, { forwardRef, useEffect, useState } from 'react';
import { lookupService, type LookupOption, type LookupResource } from '../../services/lookupService';
import { useDebounce } from '../../hooks/useDebounce';

interface SelectOption {
  value: string | number;
  label: string;
}

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  helperText?: string;
  options?: SelectOption[];
  lookup?: { resource: LookupResource; branchId?: string };
  onLookupSelect?: (option: LookupOption) => void;
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(({
  label,
  error,
  helperText,
  options,
  lookup,
  onLookupSelect,
  children,
  className = '',
  id,
  ...props
}, ref) => {
  const selectId = id || (label ? `select-${label.replace(/\s+/g, '-')}` : undefined);
  const [search, setSearch] = useState('');
  const debouncedSearch = useDebounce(search, 250);
  const [page, setPage] = useState(1);
  const [rows, setRows] = useState<LookupOption[]>([]);
  const [selected, setSelected] = useState<LookupOption | null>(null);
  const [hasNext, setHasNext] = useState(false);
  const [loadError, setLoadError] = useState('');
  const [loading, setLoading] = useState(false);
  const resource = lookup?.resource;
  const branchId = lookup?.branchId;
  const value = props.value;
  useEffect(() => { setPage(1); }, [debouncedSearch, resource, branchId]);
  useEffect(() => {
    if (!resource) return;
    let cancelled = false;
    setRows([]); setLoading(true); setLoadError('');
    void lookupService.page(resource, { branchId, search: debouncedSearch, pageNumber: page }).then((result) => {
      if (cancelled) return;
      setRows(result.items); setHasNext(result.hasNextPage);
    }).catch((err) => { if (!cancelled) setLoadError(err instanceof Error ? err.message : 'تعذر تحميل الخيارات'); })
      .finally(() => { if (!cancelled) setLoading(false); });
    return () => { cancelled = true; };
  }, [resource, branchId, debouncedSearch, page]);
  useEffect(() => {
    setSelected(null);
    if (!resource || typeof value !== 'string' || !value) return;
    let cancelled = false;
    void lookupService.page(resource, { branchId, id: value, pageSize: 1 }).then((result) => {
      if (!cancelled) setSelected(result.items[0] || null);
    }).catch(() => {});
    return () => { cancelled = true; };
  }, [resource, branchId, value]);
  const remoteOptions = [
    { value: '', label: loading ? 'جاري التحميل...' : 'اختر...' },
    ...rows.map((row) => ({ value: row.id, label: row.name })),
    ...(selected && !rows.some((row) => row.id === selected.id) ? [{ value: selected.id, label: selected.name }] : []),
  ];
  const effectiveOptions = resource ? remoteOptions : options;

  return (
    <div className="w-full flex flex-col gap-1.5">
      {label && (
        <label htmlFor={selectId} className="text-xs font-semibold text-slate-700 select-none">
          {label}
        </label>
      )}

      {resource && <input type="search" aria-label={`بحث ${label || 'الخيارات'}`} value={search}
        onChange={(event) => setSearch(event.target.value)} disabled={props.disabled}
        placeholder="ابحث في جميع الخيارات..." className="h-9 px-3 border border-slate-200 rounded-lg text-sm" />}
      <select
        ref={ref}
        id={selectId}
        className={`w-full h-10 px-3 text-sm rounded-lg border bg-white text-slate-900 transition-colors duration-150 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 disabled:bg-slate-50 disabled:text-slate-400 disabled:cursor-not-allowed ${
          error ? 'border-rose-300 focus:ring-rose-500 focus:border-rose-500' : 'border-slate-200 hover:border-slate-300'
        } ${className}`}
        {...props}
        onChange={(event) => {
          const chosen = [...rows, ...(selected ? [selected] : [])].find((row) => row.id === event.target.value);
          if (chosen && resource) onLookupSelect?.(chosen);
          props.onChange?.(event);
        }}
      >
        {effectiveOptions
          ? effectiveOptions.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))
          : children}
      </select>
      {resource && <div className="flex items-center gap-3 text-xs">
        <button type="button" onClick={() => setPage((p) => p - 1)} disabled={loading || page === 1 || props.disabled}>السابق</button>
        <span>صفحة {page}</span>
        <button type="button" onClick={() => setPage((p) => p + 1)} disabled={loading || !hasNext || props.disabled}>التالي</button>
        {loadError && <span role="alert" className="text-rose-600">{loadError}</span>}
      </div>}

      {error && <span className="text-xs text-rose-600 font-medium">{error}</span>}
      {!error && helperText && <span className="text-xs text-slate-500">{helperText}</span>}
    </div>
  );
});

Select.displayName = 'Select';

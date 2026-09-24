import React, { forwardRef } from 'react';

interface SelectOption {
  value: string | number;
  label: string;
}

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label?: string;
  error?: string;
  helperText?: string;
  options?: SelectOption[];
}

export const Select = forwardRef<HTMLSelectElement, SelectProps>(({
  label,
  error,
  helperText,
  options,
  children,
  className = '',
  id,
  ...props
}, ref) => {
  const selectId = id || (label ? `select-${label.replace(/\s+/g, '-')}` : undefined);

  return (
    <div className="w-full flex flex-col gap-1.5">
      {label && (
        <label htmlFor={selectId} className="text-xs font-semibold text-slate-700 select-none">
          {label}
        </label>
      )}

      <select
        ref={ref}
        id={selectId}
        className={`w-full h-10 px-3 text-sm rounded-lg border bg-white text-slate-900 transition-colors duration-150 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 disabled:bg-slate-50 disabled:text-slate-400 disabled:cursor-not-allowed ${
          error ? 'border-rose-300 focus:ring-rose-500 focus:border-rose-500' : 'border-slate-200 hover:border-slate-300'
        } ${className}`}
        {...props}
      >
        {options
          ? options.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))
          : children}
      </select>

      {error && <span className="text-xs text-rose-600 font-medium">{error}</span>}
      {!error && helperText && <span className="text-xs text-slate-500">{helperText}</span>}
    </div>
  );
});

Select.displayName = 'Select';

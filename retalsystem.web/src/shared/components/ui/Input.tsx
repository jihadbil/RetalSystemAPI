import React, { forwardRef } from 'react';

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
  leftIcon?: React.ReactNode;
  rightIcon?: React.ReactNode;
  isLtr?: boolean;
}

export const Input = forwardRef<HTMLInputElement, InputProps>(({
  label,
  error,
  helperText,
  leftIcon,
  rightIcon,
  isLtr = false,
  className = '',
  id,
  type = 'text',
  ...props
}, ref) => {
  const inputId = id || (label ? `input-${label.replace(/\s+/g, '-')}` : undefined);

  return (
    <div className="w-full flex flex-col gap-1.5">
      {label && (
        <label htmlFor={inputId} className="text-xs font-semibold text-slate-700 select-none">
          {label}
        </label>
      )}

      <div className="relative flex items-center">
        {leftIcon && (
          <div className="absolute right-3 text-slate-400 pointer-events-none flex items-center">
            {leftIcon}
          </div>
        )}

        <input
          ref={ref}
          id={inputId}
          type={type}
          className={`w-full h-10 px-3 text-sm rounded-lg border bg-white text-slate-900 transition-colors duration-150 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 disabled:bg-slate-50 disabled:text-slate-400 disabled:cursor-not-allowed ${
            error ? 'border-rose-300 focus:ring-rose-500 focus:border-rose-500' : 'border-slate-200 hover:border-slate-300'
          } ${leftIcon ? 'pr-9' : ''} ${rightIcon ? 'pl-9' : ''} ${isLtr ? 'num-ltr text-left' : ''} ${className}`}
          {...props}
        />

        {rightIcon && (
          <div className="absolute left-3 text-slate-400 pointer-events-none flex items-center">
            {rightIcon}
          </div>
        )}
      </div>

      {error && <span className="text-xs text-rose-600 font-medium">{error}</span>}
      {!error && helperText && <span className="text-xs text-slate-500">{helperText}</span>}
    </div>
  );
});

Input.displayName = 'Input';

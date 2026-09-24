import React from 'react';

interface CardProps extends Omit<React.HTMLAttributes<HTMLDivElement>, 'title'> {
  children: React.ReactNode;
  title?: React.ReactNode;
  subtitle?: string;
  action?: React.ReactNode;
  footer?: React.ReactNode;
  noPadding?: boolean;
}

export const Card: React.FC<CardProps> = ({
  children,
  title,
  subtitle,
  action,
  footer,
  noPadding = false,
  className = '',
  ...props
}) => {
  return (
    <div
      className={`bg-white rounded-xl border border-slate-200/90 shadow-xs flex flex-col transition-shadow ${className}`}
      {...props}
    >
      {(title || action) && (
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-slate-100 bg-slate-50/40 rounded-t-xl">
          <div>
            {typeof title === 'string' ? (
              <h3 className="text-sm font-bold text-slate-900 leading-snug">{title}</h3>
            ) : (
              title
            )}
            {subtitle && <p className="text-xs text-slate-500 mt-0.5">{subtitle}</p>}
          </div>
          {action && <div className="flex items-center gap-2">{action}</div>}
        </div>
      )}

      <div className={`flex-1 ${noPadding ? '' : 'p-5'}`}>{children}</div>

      {footer && (
        <div className="px-5 py-3 border-t border-slate-100 bg-slate-50/40 rounded-b-xl text-xs text-slate-600">
          {footer}
        </div>
      )}
    </div>
  );
};

/** Escape CSV delimiters and keep user-provided text from becoming a spreadsheet formula. */
export function csvCell(value: string | number): string {
  let text = String(value);
  let first = 0;
  while (first < text.length && (text.charCodeAt(first) <= 32 || /\s/.test(text[first]))) first++;
  if (typeof value === 'string' && /^[=+\-@]/.test(text.slice(first))) text = `'${text}`;
  return `"${text.replace(/"/g, '""')}"`;
}

export function buildCsv(rows: (string | number)[][]): string {
  return '\uFEFF' + rows.map((row) => row.map(csvCell).join(',')).join('\r\n');
}

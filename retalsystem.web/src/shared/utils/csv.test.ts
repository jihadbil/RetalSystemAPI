import { describe, expect, it } from 'vitest';
import { buildCsv, csvCell } from './csv';

describe('CSV exports', () => {
  it('preserves quotes, delimiters, newlines and URL-sensitive characters inside a cell', () => {
    expect(csvCell('A "quoted", #value\nnext')).toBe('"A ""quoted"", #value\nnext"');
    expect(buildCsv([['الاسم', 'الكود'], ['صنف #1', 'A,B']])).toBe('\uFEFF"الاسم","الكود"\r\n"صنف #1","A,B"');
  });

  it.each(['=1+1', '+SUM(A1)', '-1+2', '@SUM(A1)', '\t=1+1', '  =1+1', '\u0000 =1+1'])('keeps %j as text', (value) => {
    expect(csvCell(value)).toBe(`"'${value}"`);
  });

  it('retains numeric values as numbers', () => {
    expect(csvCell(-2.5)).toBe('"-2.5"');
  });
});

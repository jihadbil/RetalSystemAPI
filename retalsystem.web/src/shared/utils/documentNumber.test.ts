import { afterEach, expect, it, vi } from 'vitest';
import { createDocumentNumber } from './documentNumber';

afterEach(() => vi.useRealTimers());

it('fits the document contract and creates distinct numbers in the same millisecond', () => {
  vi.useFakeTimers(); vi.setSystemTime(new Date('2026-10-21T00:00:00Z'));
  const numbers = Array.from({ length: 1024 }, () => createDocumentNumber('SRTN'));
  expect(new Set(numbers).size).toBe(numbers.length);
  expect(numbers.every((number) => /^SRTN-20261021-[0-9a-f]{32}$/.test(number))).toBe(true);
  expect(createDocumentNumber('ABCDEFGH')).toHaveLength(50);
  expect(() => createDocumentNumber('PREFIXTOOLONG')).toThrow();
});

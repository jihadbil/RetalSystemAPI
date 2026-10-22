import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from './apiClient';

describe('structured HTTP errors', () => {
  beforeEach(() => {
    vi.stubGlobal('localStorage', { getItem: () => null });
    vi.stubGlobal('sessionStorage', { getItem: () => null });
  });
  afterEach(() => vi.unstubAllGlobals());
  it.each(['application/problem+json', 'application/problem+json; charset=utf-8', 'application/vnd.retal+json'])('reads validation and code from %s', async (contentType) => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({
      title: 'Validation failed', errorCode: 'INVALID_QUANTITY', errors: { quantity: ['الكمية غير صالحة'] },
    }), { status: 422, headers: { 'Content-Type': contentType } })));
    await expect(apiClient.get('engineering-test')).rejects.toMatchObject({ statusCode: 422, message: 'الكمية غير صالحة', errorCode: 'INVALID_QUANTITY', errors: ['الكمية غير صالحة'] });
  });
  it('uses problem detail when no field errors exist', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ title: 'Conflict', detail: 'البيانات تغيرت' }),
      { status: 409, headers: { 'Content-Type': 'application/problem+json' } })));
    await expect(apiClient.get('engineering-test')).rejects.toMatchObject({ message: 'البيانات تغيرت' });
  });
  it('handles problem JSON for failed file downloads too', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ title: 'Workbook invalid', errors: { file: ['عنوان العمود غير صحيح'] } }),
      { status: 400, headers: { 'Content-Type': 'application/problem+json' } })));
    await expect(apiClient.download('engineering-test.xlsx')).rejects.toMatchObject({ message: 'عنوان العمود غير صحيح', errors: ['عنوان العمود غير صحيح'] });
  });
});

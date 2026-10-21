/** Fits the API's 50-character limit, including documents created simultaneously. */
export function createDocumentNumber(prefix: string): string {
  if (!/^[A-Z]{1,8}$/.test(prefix)) throw new Error('Invalid document prefix');
  const entropy = crypto.getRandomValues(new Uint8Array(16));
  const suffix = Array.from(entropy, (byte) => byte.toString(16).padStart(2, '0')).join('');
  return `${prefix}-${new Date().toISOString().slice(0, 10).replaceAll('-', '')}-${suffix}`;
}

import { describe, expect, it } from 'vitest';
import { isPrivilegedUser, userHasPermission } from './authorization';

describe('authorization policy', () => {
  it('denies missing permissions by default', () => {
    expect(userHasPermission({ roles: ['Cashier'], permissions: [] }, 'Permissions.Products.Edit')).toBe(false);
    expect(userHasPermission(null, 'Permissions.Products.View')).toBe(false);
  });

  it('allows an explicitly assigned permission', () => {
    expect(userHasPermission(
      { roles: ['Cashier'], permissions: ['Permissions.Pos.CreateSale'] },
      'Permissions.Pos.CreateSale'
    )).toBe(true);
  });

  it('allows privileged roles case-insensitively', () => {
    expect(isPrivilegedUser({ roles: ['ADMIN'] })).toBe(true);
    expect(userHasPermission({ roles: ['Owner'] }, 'Permissions.Users.Delete')).toBe(true);
  });
});

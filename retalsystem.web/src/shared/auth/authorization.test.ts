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

  it('reserves the permission bypass for SuperAdmin as the backend does', () => {
    expect(isPrivilegedUser({ roles: ['SuperAdmin'] })).toBe(true);
    expect(userHasPermission({ roles: ['SuperAdmin'] }, 'Permissions.Users.Delete')).toBe(true);
  });

  it.each(['Admin', 'Owner'])('requires explicit claims for %s', (role) => {
    expect(isPrivilegedUser({ roles: [role] })).toBe(false);
    expect(userHasPermission({ roles: [role], permissions: [] }, 'Permissions.Users.Delete')).toBe(false);
    expect(userHasPermission({ roles: [role], permissions: ['Permissions.Users.View'] }, 'Permissions.Users.View')).toBe(true);
  });

  it('matches permission claims case-insensitively as the backend does', () => {
    expect(userHasPermission({ permissions: ['permissions.products.view'] }, 'Permissions.Products.View')).toBe(true);
  });
});

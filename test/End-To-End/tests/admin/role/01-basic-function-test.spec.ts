import { test as base, expect } from '@playwright/test';
import { RoleFormPage } from '@admin/RoleFormPage';

const test = base.extend<{ roleFormPage: RoleFormPage }>({
    roleFormPage: async ({ page }, use) => {
        const roleFormPage = new RoleFormPage(page);
        await roleFormPage.login();
        await roleFormPage.navigateTo();
        await use(roleFormPage);
    }
});

test('should be able to create a role with required fields', async ({ page, roleFormPage }) => {
    const timestamp = new Date().getTime();

    await roleFormPage.fillRoleForm({
        title: 'Test Role ' + timestamp,
        status: '1',
        description: 'This is a test role description.',
        permissions: [
            {
                key: 'Page_View',
                checked: true
            },
            {
                key: 'Page_Manage',
                checked: true
            }
        ]
    });

    await roleFormPage.save();

    await expect(page).toHaveURL(/\/admin\/roles\/edit\/\d+/);
});

test('should keep selected permissions checked after save', async ({ page, roleFormPage }) => {
    const timestamp = new Date().getTime();

    await roleFormPage.fillRoleForm({
        title: 'Test Role Permission ' + timestamp,
        status: '1',
        description: 'This is a test role description.',
        permissions: [
            {
                key: 'Page_View',
                checked: true
            },
            {
                key: 'Page_Manage',
                checked: true
            }
        ]
    });

    await roleFormPage.save();

    await expect(page).toHaveURL(/\/admin\/roles\/edit\/\d+/);
    await roleFormPage.expectPermissionChecked('Page_View');
    await roleFormPage.expectPermissionChecked('Page_Manage');
});

test('should be able to save and exit from the role form', async ({ page, roleFormPage }) => {
    const timestamp = new Date().getTime();

    await roleFormPage.fillRoleForm({
        title: 'Test Role Save Exit ' + timestamp,
        status: '1',
        description: 'This is a test role description.'
    });

    await roleFormPage.saveAndExit();

    await expect(page).toHaveURL(/\/admin\/roles/);
});

test('should cancel and return to the role list page', async ({ page, roleFormPage }) => {
    await roleFormPage.cancel();

    await expect(page).toHaveURL(/\/admin\/roles/);
});
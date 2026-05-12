import { expect, Locator, Page } from '@playwright/test';
import { AdminPageBase } from '@models/AdminPageBase';

export interface RoleFormData {
    title?: string;
    status?: string;
    description?: string;
    permissions?: Array<{
        key?: string;
        checked?: boolean;
        title?: string;
    }>;
}

export class RoleFormPage extends AdminPageBase {
    readonly titleField = this.page.locator('#Title');
    readonly statusField = this.page.locator('#Status');
    readonly descriptionField = this.page.locator('#Description');
    readonly saveButton = this.page.locator('input[type="submit"][value="保存"]');
    readonly saveAndExitButton = this.page.locator('input[type="submit"][value="保存并退出"]');
    readonly cancelButton = this.page.getByRole('link', { name: '取消' });

    constructor(page: Page) {
        super(page);
    }

    async navigateTo(): Promise<void> {
        await this.page.goto('/admin/roles/create');
    }

    async fillRoleForm(roleData: RoleFormData): Promise<void> {
        await this.fill(this.titleField, roleData.title);
        await this.fill(this.statusField, roleData.status);
        await this.fill(this.descriptionField, roleData.description);

        for (const permission of roleData.permissions ?? []) {
            await this.setPermission(permission.key, permission.checked, permission.title);
        }
    }

    async setPermission(permissionKey?: string, checked?: boolean, permissionTitle?: string): Promise<void> {
        if (permissionKey == null && permissionTitle == null) {
            return;
        }

        let permissionLocator: Locator | undefined;

        if (permissionKey) {
            const keyInput = this.page.locator(`input[type="hidden"][name$=".Key"][value="${permissionKey}"]`).first();
            const permissionLabel = keyInput.locator('xpath=ancestor::label[contains(@class, "checkbox-inline")]').first();
            if (await permissionLabel.count() > 0) {
                permissionLocator = permissionLabel.locator('input[type="checkbox"]').first();
            }
        }

        if ((!permissionLocator || await permissionLocator.count() === 0) && permissionTitle) {
            const permissionLabel = this.page.locator('label.checkbox-inline', { hasText: permissionTitle }).first();
            if (await permissionLabel.count() > 0) {
                permissionLocator = permissionLabel.locator('input[type="checkbox"]').first();
            }
        }

        if (!permissionLocator) {
            return;
        }

        const shouldCheck = checked ?? true;
        if (shouldCheck) {
            await permissionLocator.check();
        } else {
            await permissionLocator.uncheck();
        }
    }

    async expectPermissionChecked(permissionKey?: string, permissionTitle?: string): Promise<void> {
        const permissionLocator = await this.getPermissionLocator(permissionKey, permissionTitle);
        if (!permissionLocator) {
            throw new Error('Permission locator was not found.');
        }

        await expect(permissionLocator).toBeChecked();
    }

    private async getPermissionLocator(permissionKey?: string, permissionTitle?: string): Promise<Locator | undefined> {
        let permissionLocator: Locator | undefined;

        if (permissionKey) {
            const keyInput = this.page.locator(`input[type="hidden"][name$=".Key"][value="${permissionKey}"]`).first();
            const permissionLabel = keyInput.locator('xpath=ancestor::label[contains(@class, "checkbox-inline")]').first();
            if (await permissionLabel.count() > 0) {
                permissionLocator = permissionLabel.locator('input[type="checkbox"]').first();
            }
        }

        if ((!permissionLocator || await permissionLocator.count() === 0) && permissionTitle) {
            const permissionLabel = this.page.locator('label.checkbox-inline', { hasText: permissionTitle }).first();
            if (await permissionLabel.count() > 0) {
                permissionLocator = permissionLabel.locator('input[type="checkbox"]').first();
            }
        }

        return permissionLocator;
    }

    async save(): Promise<void> {
        await this.saveButton.click();
    }

    async saveAndExit(): Promise<void> {
        await this.saveAndExitButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}
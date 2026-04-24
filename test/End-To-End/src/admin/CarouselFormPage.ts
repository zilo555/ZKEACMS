import { Page } from '@playwright/test';
import { AdminPageBase } from '@models/AdminPageBase';

export interface CarouselItemFormData {
    title?: string;
    targetLink?: string;
    imageUrl?: string;
    status?: string;
}

export interface CarouselFormData {
    title?: string;
    status?: string;
    description?: string;
    items?: CarouselItemFormData[];
}

export class CarouselFormPage extends AdminPageBase {
    readonly titleField = this.page.locator('#Title');
    readonly statusField = this.page.locator('#Status');
    readonly descriptionField = this.page.locator('#Description');
    readonly itemsContainer = this.page.locator('.input-group-collection');
    readonly addItemButton = this.page.locator('.input-group-collection .add');
    readonly saveButton = this.page.locator('input[type="submit"][data-value="Create"]');
    readonly saveAndExitButton = this.page.locator('input[type="submit"][data-value="CreateAndExit"]');
    readonly cancelButton = this.page.getByRole('link', { name: '取消' });

    constructor(page: Page) {
        super(page);
    }

    async navigateTo(): Promise<void> {
        await this.page.goto('/admin/carousel/create');
    }

    async fillCarouselForm(formData: CarouselFormData): Promise<void> {
        await this.fill(this.titleField, formData.title);

        if (formData.items?.length) {
            await this.ensureCarouselItemCount(formData.items.length);
            for (let index = 0; index < formData.items.length; index++) {
                await this.fillCarouselItem(index, formData.items[index]);
            }
        }

        await this.fill(this.statusField, formData.status);
        await this.fill(this.descriptionField, formData.description);
    }

    async ensureCarouselItemCount(count: number): Promise<void> {
        const currentCount = await this.itemsContainer.locator('.items>.item').count();
        while (currentCount < count) {
            await this.addItemButton.click();
            await this.itemsContainer.locator('.items>.item').nth(currentCount).waitFor();
            return this.ensureCarouselItemCount(count);
        }
    }

    async fillCarouselItem(index: number, itemData: CarouselItemFormData): Promise<void> {
        const item = this.itemsContainer.locator('.items>.item').nth(index);
        await this.fill(item.locator(`input[id$="__Title"]`), itemData.title);
        await this.fill(item.locator(`input[id$="__TargetLink"]`), itemData.targetLink);
        await this.fill(item.locator(`input[id$="__ImageUrl"]`), itemData.imageUrl);
        await this.fill(item.locator(`select[id$="__Status"]`), itemData.status);
    }

    async save(): Promise<void> {
        await this.saveButton.click();
    }

    async createCarousel(formData: CarouselFormData, saveAndExit: boolean = false): Promise<void> {
        await this.fillCarouselForm(formData);

        if (saveAndExit) {
            await this.saveAndExit();
            return;
        }

        await this.save();
    }

    async saveAndExit(): Promise<void> {
        await this.saveAndExitButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}
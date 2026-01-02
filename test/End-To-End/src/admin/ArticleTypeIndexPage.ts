import { AdminPageBase } from "@models/AdminPageBase";
import { Page } from "@playwright/test";

export class ArticleTypeIndexPage extends AdminPageBase {

    readonly createArticleTypeLink = this.page.locator('a[href="/admin/articletype/create"]');

    constructor(page: Page) {
        super(page);
    }

    async navigateTo(): Promise<void> {
        await this.page.goto('/admin/articletype');
    }

    async goToCreateArticleTypePage(): Promise<void> {
        await this.createArticleTypeLink.click();
    }

    async createSubArticleType(parentTypeName: string): Promise<void> {
        const parentTypeItem = await this.page.getByRole('treeitem', { name: parentTypeName, exact: true });
        await parentTypeItem.click({ button: 'right' });
        await this.page.locator('.jstree-contextmenu').locator('a[rel="0"]').click();
    }
}
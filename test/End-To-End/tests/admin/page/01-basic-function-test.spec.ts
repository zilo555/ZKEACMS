import { test, expect, Page } from '@playwright/test';
import { PageFormData, PageFormPage } from '@admin/PageFormPage';

async function createPage(pageForm: PageFormPage): Promise<PageFormData> {
    const timeStamp = new Date().getTime();
    await pageForm.login();
    await pageForm.navigateTo();
    const formData = {
        name: 'Test Page' + timeStamp,
        title: 'This is a Test Page',
        path: 'test-page-' + timeStamp,
        layout: '默认',
        keywords: 'test, page',
        description: 'This is a description for the test page.',
        status: '1'
    } as PageFormData
    await pageForm.fillForm(formData);
    await pageForm.submitForm();
    formData.id = await pageForm.expectPageCreated();
    return formData;
}
test('Should create page successfully with primary fields', async ({ page }) => {
    var pageForm = new PageFormPage(page);
    await createPage(pageForm);
});

test('Should fail to create page with missing required fields', async ({ page }) => {
    var pageForm = new PageFormPage(page);
    await pageForm.login();
    await pageForm.navigateTo();
    pageForm.submitForm();
    await expect(page.locator('#PageName-error')).toBeVisible();
    await expect(page.locator('#PageUrl-error')).toBeVisible();
});

test('Should publish page successfully without any widgets', async ({ page }) => {
    var pageForm = new PageFormPage(page);
    const formData = await createPage(pageForm);
    await pageForm.publishPageInDesignView();
    await expect(page).toHaveURL(`/${formData.path}`);
});

test('Should publish page successfully with widgets', async ({ page }) => {
    var pageForm = new PageFormPage(page);
    await pageForm.login();
    const formData = await createPage(pageForm);

    await page.locator(".templates").evaluate((element) => {
        if (!element.classList.contains("active")) {
            element.classList.add("active");
        }
    });

    const main = page.getByRole('link', { name: '+ 添加内容 | 主内容' });
    
    await page.getByRole('img', { name: '巨幕' }).dragTo(main);
    await page.getByRole('img', { name: '页头' }).dragTo(main);
    await page.getByRole('img', { name: '图片', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '段落' }).dragTo(main);
    await page.getByRole('img', { name: '分隔符' }).dragTo(main);
    await page.getByRole('img', { name: '间距' }).dragTo(main);
    await page.getByRole('img', { name: '文字二列' }).dragTo(main);
    await page.getByRole('img', { name: '文字三列' }).dragTo(main);
    await page.getByRole('img', { name: '图片右', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图片右（圆）' }).dragTo(main);
    await page.getByRole('img', { name: '图片左', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图片左（圆）' }).dragTo(main);
    await page.getByRole('img', { name: '图片左（平分）' }).dragTo(main);
    await page.getByRole('img', { name: '图例二', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图例二（圆）' }).dragTo(main);
    await page.getByRole('img', { name: '图例三', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图例三（圆）' }).dragTo(main);

    await page.locator(".templates").evaluate((element) => {
        if (element.classList.contains("active")) {
            element.classList.remove("active");
        }
    });

    await pageForm.publishPageInDesignView();
    await expect(page).toHaveURL(`/${formData.path}`);
});
test('Should edit page successfully', async ({ page }) => {
    var pageForm = new PageFormPage(page);
    const formData = await createPage(pageForm);
    const newTitle = 'This is an updated Test Page';
    formData.title = newTitle;
    await pageForm.navigateToEditPage(formData.id!);
    await pageForm.fillForm(formData);
    await pageForm.submitForm();
    
    await expect(page.getByRole('textbox', { name: '标题' })).toHaveValue(newTitle);
});
test('Should delete page successfully', async ({ page }) => {
    var pageForm = new PageFormPage(page);
    const formData = await createPage(pageForm);
    await page.locator(".templates").evaluate((element) => {
        if (!element.classList.contains("active")) {
            element.classList.add("active");
        }
    });

    const main = page.getByRole('link', { name: '+ 添加内容 | 主内容' });
    
    await page.getByRole('img', { name: '巨幕' }).dragTo(main);
    await page.getByRole('img', { name: '页头' }).dragTo(main);
    await page.getByRole('img', { name: '图片', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '段落' }).dragTo(main);
    await page.getByRole('img', { name: '分隔符' }).dragTo(main);
    await page.getByRole('img', { name: '间距' }).dragTo(main);
    await page.getByRole('img', { name: '文字二列' }).dragTo(main);
    await page.getByRole('img', { name: '文字三列' }).dragTo(main);
    await page.getByRole('img', { name: '图片右', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图片右（圆）' }).dragTo(main);
    await page.getByRole('img', { name: '图片左', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图片左（圆）' }).dragTo(main);
    await page.getByRole('img', { name: '图片左（平分）' }).dragTo(main);
    await page.getByRole('img', { name: '图例二', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图例二（圆）' }).dragTo(main);
    await page.getByRole('img', { name: '图例三', exact: true }).dragTo(main);
    await page.getByRole('img', { name: '图例三（圆）' }).dragTo(main);

    await page.locator(".templates").evaluate((element) => {
        if (element.classList.contains("active")) {
            element.classList.remove("active");
        }
    });

    await pageForm.publishPageInDesignView();
    await expect(page).toHaveURL(`/${formData.path}`);
    
    await pageForm.navigateToEditPage(formData.id!);
    
    page.once('dialog', dialog => {
      console.log(`Dialog message: ${dialog.message()}`);
      dialog.accept().catch(() => {});
    });
    await page.getByRole('button', { name: '删除' }).click();
    await expect(page).toHaveURL(/\/admin\/page#/);
});
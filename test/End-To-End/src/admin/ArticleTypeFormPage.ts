import { Page } from '@playwright/test';
import { AdminPageBase } from '@models/AdminPageBase';

export interface ArticleTypeData {
  title: string;
  url?: string;
  seoTitle?: string;
  seoKeywords?: string;
  seoDescription?: string;
  status?: '有效' | '无效';
  description?: string;
}

export class ArticleTypeFormPage extends AdminPageBase {
  // Form fields
  readonly titleField = this.page.locator('#Title');
  readonly urlField = this.page.locator('#Url');
  readonly seoTitleField = this.page.locator('#SEOTitle');
  readonly seoKeywordsField = this.page.locator('#SEOKeyWord');
  readonly seoDescriptionField = this.page.locator('#SEODescription');
  readonly statusField = this.page.locator('#Status');
  readonly descriptionField = this.page.locator('#Description');
  
  // Buttons
  readonly saveButton = this.page.locator('input[data-value="Create"]');
  readonly saveAndExitButton = this.page.locator('input[data-value="CreateAndExit"]');
  readonly cancelButton = this.page.locator('a:has-text("取消")');
  readonly randomUrlButton = this.page.locator('.random');
  
  // Form validation
  readonly titleValidation = this.page.locator('[data-valmsg-for="Title"]');
  readonly urlValidation = this.page.locator('[data-valmsg-for="Url"]');
  
  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to the article type creation page
   */
  async navigateTo(): Promise<void> {
    await this.page.goto('/admin/articletype/create');
    await this.waitForData();
  }

  /**
   * Fill the article type form with provided data
   */
  async fillArticleTypeForm(articleTypeData: ArticleTypeData): Promise<void> {
    // Fill required fields
    await this.titleField.fill(articleTypeData.title);
    
    // Fill optional fields
    if (articleTypeData.url) {
      await this.urlField.fill(articleTypeData.url);
    } else {
      // Click the random URL button to generate one
      await this.randomUrlButton.click();
    }
    await this.fill(this.seoTitleField, articleTypeData.seoTitle);
    await this.fill(this.seoKeywordsField, articleTypeData.seoKeywords);
    await this.fill(this.seoDescriptionField, articleTypeData.seoDescription);
    await this.fill(this.statusField, articleTypeData.status);
    await this.fill(this.descriptionField, articleTypeData.description);
  }

  /**
   * Submit the form and stay on the page
   */
  async save(): Promise<void> {
    await this.saveButton.click();
  }

  /**
   * Submit the form and return to the article type list
   */
  async saveAndExit(): Promise<void> {
    await this.saveAndExitButton.click();
  }

  /**
   * Cancel and return to the article type list
   */
  async cancel(): Promise<void> {
    await this.cancelButton.click();
  }

  /**
   * Create a new article type with the provided data
   */
  async createArticleType(articleTypeData: ArticleTypeData): Promise<void> {
    await this.fillArticleTypeForm(articleTypeData);
    await this.save();
  }

  /**
   * Wait for the page to be fully loaded
   */
  async waitForData(): Promise<void> {
    await this.page.waitForSelector('#Title', { state: 'visible' });
  }

  /**
   * Check if the form has validation errors
   */
  async hasValidationErrors(): Promise<boolean> {
    const titleError = await this.titleValidation.isVisible();
    const urlError = await this.urlValidation.isVisible();
    return titleError || urlError;
  }

  /**
   * Get validation error message for title field
   */
  async getTitleErrorMessage(): Promise<string | null> {
    const isVisible = await this.titleValidation.isVisible();
    if (isVisible) {
      return await this.titleValidation.textContent();
    }
    return null;
  }

  /**
   * Get validation error message for URL field
   */
  async getUrlErrorMessage(): Promise<string | null> {
    const isVisible = await this.urlValidation.isVisible();
    if (isVisible) {
      return await this.urlValidation.textContent();
    }
    return null;
  }
}
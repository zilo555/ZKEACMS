import { Page } from '@playwright/test';
import { AdminPageBase } from '@models/AdminPageBase';

export interface RuleItemData {
  condition?: string;
  property?: string;
  functionName?: string;
  value?: string;
}

export interface RuleFormData {
  title?: string;
  zoneName?: string;
  status?: string;
  description?: string;
}

export class RuleFormPage extends AdminPageBase {
  readonly titleField = this.page.locator('#Title');
  readonly zoneNameField = this.page.locator('#ZoneName');
  readonly addRuleItemButton = this.page.locator('input.add[data-value="Create"]');
  readonly statusField = this.page.locator('#Status');
  readonly descriptionField = this.page.locator('#Description');
  readonly saveButton = this.page.locator('input[data-value="Create"]:nth-child(1)');
  readonly saveAndExitButton = this.page.locator('input[data-value="CreateAndExit"]');
  readonly cancelButton = this.page.getByRole('link', { name: '取消' });

  constructor(page: Page) {
    super(page);
  }

  async navigateTo(): Promise<void> {
    await this.page.goto('/admin/rule/create');
    await this.waitForPageLoad();
  }

  async waitForPageLoad(): Promise<void> {
    await this.page.waitForURL('**/admin/rule/create');
    await this.titleField.waitFor({ state: 'visible' });
  }

  async fillRuleForm(ruleData: RuleFormData): Promise<void> {
    await this.fill(this.titleField, ruleData.title);
    await this.fill(this.zoneNameField, ruleData.zoneName);
    await this.fill(this.statusField, ruleData.status);
    await this.fill(this.descriptionField, ruleData.description);
  }

  async addRuleItem(ruleItemData: RuleItemData = {}): Promise<void> {
    await this.addRuleItemButton.click();
    const ruleItem = this.page.locator('.items .row.item').last();

    if (ruleItemData.condition) {
      await ruleItem.locator('select[id$="__Condition"]').waitFor({ state: 'visible' });
    }
    await this.fill(ruleItem.locator('select[id$="__Condition"]'), ruleItemData.condition);

    await this.fill(ruleItem.locator('select[id$="__Property"]'), ruleItemData.property);
    await this.fill(ruleItem.locator('select[id$="__FunctionName"]'), ruleItemData.functionName);
    await this.fill(ruleItem.locator('input[id$="__Value"]'), ruleItemData.value);
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

  async createRule(ruleData: RuleFormData, ruleItems: RuleItemData[] = [], saveAndExit = false): Promise<void> {
    await this.fillRuleForm(ruleData);

    for (const ruleItem of ruleItems) {
      await this.addRuleItem(ruleItem);
    }

    if (saveAndExit) {
      await this.saveAndExit();
      return;
    }

    await this.save();
  }
}
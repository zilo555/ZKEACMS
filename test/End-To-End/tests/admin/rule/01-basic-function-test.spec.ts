import { test as base, expect } from '@playwright/test';
import { RuleFormPage } from '@admin/RuleFormPage';

const test = base.extend<{ ruleFormPage: RuleFormPage }>({
    ruleFormPage: async ({ page }, use) => {
        const ruleFormPage = new RuleFormPage(page);
        await ruleFormPage.login();
        await ruleFormPage.navigateTo();
        await use(ruleFormPage);
    }
});

test('should be able to create a rule with required fields', async ({ page, ruleFormPage }) => {
    const timestamp = new Date().getTime();

    await ruleFormPage.fillRuleForm({
        title: 'Test Rule ' + timestamp,
        zoneName: 'Test Zone ' + timestamp,
        status: '1',
        description: 'This is a test rule description.'
    });

    await ruleFormPage.addRuleItem({
        property: "ValueOf('Now')",
        functionName: 'GreaterThan',
        value: '2026-01-01'
    });

    await ruleFormPage.save();

    await expect(page).toHaveURL(/\/admin\/rule\/edit\/\d+/);
});

test('should be able to save and exit from the rule form', async ({ page, ruleFormPage }) => {
    const timestamp = new Date().getTime();

    await ruleFormPage.fillRuleForm({
        title: 'Test Rule Save Exit ' + timestamp,
        zoneName: 'Test Zone ' + timestamp,
        description: 'This is a test rule description.'
    });

    await ruleFormPage.saveAndExit();

    await expect(page).toHaveURL(/\/admin\/rule/);
});

test('should cancel and return to the rule list page', async ({ page, ruleFormPage }) => {
    await ruleFormPage.cancel();

    await expect(page).toHaveURL(/\/admin\/rule/);
});
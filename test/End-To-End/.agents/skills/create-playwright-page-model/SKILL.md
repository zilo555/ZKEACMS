---
name: create-playwright-page-model
description: "Use when creating a Playwright page object model for a ZKEACMS page from a URL or live page inspection."
---

# Create Playwright Page Model

## Purpose

Create a complete TypeScript page object model for a ZKEACMS page by inspecting the target page, determining whether it is an admin or frontend page, and writing a stable model in the correct project folder.

## Declaration

Plesease use playwright/* tools (NOT @playwright/test framework) to opoen/inspect the page.

## When To Use

Use this skill when the task is to:

- inspect a URL in **playwright MCP tool** and generate a page object model
- decide whether the page belongs in `src/admin/` or `src/models/`
- create or update locators and interaction methods for a ZKEACMS page
- reuse the existing `PageBase` or `AdminPageBase` conventions

## Inputs

- A target URL or page to inspect
- Optional login credentials from `.env` when the target is behind authentication
- The HTML structure of the page, ideally captured from `document.body.innerHTML`

## Workflow

1. Open the target page in **playwright MCP tool** and inspect the visible UI and DOM structure.
2. If the page requires authentication, read credentials from `.env` and sign in using the existing admin flow.
3. Classify the page:
   - admin pages under `/admin/` use `AdminPageBase`
   - public pages use `PageBase`
4. Identify the key controls, inputs, navigation elements, dialogs, and summary areas.
5. Prefer stable locators in this order:
   - `data-testid`
   - ARIA roles and accessible names
   - meaningful text selectors
   - CSS selectors only when necessary
   - `nth()` only when the page structure is intentionally indexed
6. Generate a TypeScript class with:
   - a descriptive class name
   - a constructor accepting `Page`
   - locator properties for stable elements
   - public methods for the page's main actions
   - navigation helpers when the page is reached via a route
7. Save the file to the correct location:
   - `src/admin/` for admin pages
   - `src/models/` or a relevant subdirectory for frontend pages

## Design Rules

- Keep the page object focused on behavior, not test assertions.
- Use async methods for all Playwright operations.
- Keep method names descriptive and aligned with the existing project style.
- Return useful values from navigation or save actions when it improves downstream tests.
- Handle optional fields defensively so the model can be reused across related forms.

## Locator Strategy

Prefer the following patterns in order:

- `page.locator('[data-testid="..."]')`
- `page.getByRole('button', { name: 'Save' })`
- `page.getByText('Title')`
- `page.locator('#Id')`
- `page.locator('.class')`
- `page.locator('.item').nth(0)`
- `page.locator('.item', { hasText: 'Item 1' })`
- `page.locator('.item', { has: page.locator('img') })`

Do not use `[ref="..."]` selectors.

## Admin Page Conventions

When the page is administrative:

- extend `AdminPageBase`
- keep login and logout flows consistent with the shared base class
- use the common helper methods for filling inputs and TinyMCE editors
- favor route-based navigation helpers such as `navigateTo()`

## Frontend Page Conventions

When the page is public-facing:

- extend `PageBase`
- keep the API small and focused on user-facing actions
- map the route clearly in `navigateTo()` when the page has a stable path

## Completion Check

The skill is complete when the generated page object model:

- matches the page type and correct base class
- compiles as TypeScript in the existing project structure
- uses stable locators
- exposes the page's meaningful actions
- is saved in the correct directory

## Example Output Shape

```ts
import { Page } from '@playwright/test';
import { AdminPageBase } from '@models/AdminPageBase';

export class ArticleManagementPage extends AdminPageBase {
  readonly titleField = this.page.locator('#Title');

  constructor(page: Page) {
    super(page);
  }

  async navigateTo(): Promise<void> {
    await this.page.goto('/admin/article');
  }
}
```
import { test as base, expect } from '@playwright/test';
import { CarouselFormPage, CarouselFormData } from '@admin/CarouselFormPage';

const test = base.extend<{ carouselFormPage: CarouselFormPage }>({
  carouselFormPage: async ({ page }, use) => {
    const carouselFormPage = new CarouselFormPage(page);
    await carouselFormPage.login();
    await carouselFormPage.navigateTo();
    await use(carouselFormPage);
  },
});

test.describe('CarouselFormPage Tests', () => {
  test('should load the carousel creation page', async ({ page, carouselFormPage }) => {
    await expect(page).toHaveURL(/.*\/admin\/carousel\/create/);
    await expect(carouselFormPage.titleField).toBeVisible();
    await expect(carouselFormPage.statusField).toBeVisible();
    await expect(carouselFormPage.descriptionField).toBeVisible();
    await expect(carouselFormPage.addItemButton).toBeVisible();
    await expect(carouselFormPage.saveButton).toBeVisible();
    await expect(carouselFormPage.saveAndExitButton).toBeVisible();
    await expect(carouselFormPage.cancelButton).toBeVisible();
  });

  test('should fill the carousel form with valid data', async ({ carouselFormPage }) => {
    const formData: CarouselFormData = {
      title: 'Test Carousel',
      status: '1',
      description: 'This is a test carousel description.',
      items: [
        {
          title: 'Carousel Item 1',
          targetLink: '/test-link-1',
          imageUrl: '/images/test-1.jpg',
          status: '1',
        },
      ],
    };

    await carouselFormPage.fillCarouselForm(formData);

    await expect(carouselFormPage.titleField).toHaveValue(formData.title || '');
    await expect(carouselFormPage.statusField).toHaveValue(formData.status || '');
    await expect(carouselFormPage.descriptionField).toHaveValue(formData.description || '');
    await expect(carouselFormPage.itemsContainer.locator('.items>.item').nth(0).locator('input[id$="__Title"]')).toHaveValue(formData.items?.[0].title || '');
    await expect(carouselFormPage.itemsContainer.locator('.items>.item').nth(0).locator('input[id$="__TargetLink"]')).toHaveValue(formData.items?.[0].targetLink || '');
    await expect(carouselFormPage.itemsContainer.locator('.items>.item').nth(0).locator('input[id$="__ImageUrl"]')).toHaveValue(formData.items?.[0].imageUrl || '');
    await expect(carouselFormPage.itemsContainer.locator('.items>.item').nth(0).locator('select[id$="__Status"]')).toHaveValue(formData.items?.[0].status || '');
  });

  test('should show validation errors when required fields are missing', async ({ page, carouselFormPage }) => {
    await carouselFormPage.save();

    await expect(page.locator('[data-valmsg-for="Title"]')).toBeVisible();
  });

  test('should successfully save a carousel', async ({ page, carouselFormPage }) => {
    const timestamp = new Date().getTime();
    const formData: CarouselFormData = {
      title: 'Test Carousel ' + timestamp,
      status: '1',
      description: 'This is a test carousel description.',
      items: [
        {
          title: 'Carousel Item ' + timestamp,
          targetLink: '/test-carousel-link-' + timestamp,
          imageUrl: '/images/test-carousel-' + timestamp + '.jpg',
          status: '1',
        },
      ],
    };

    await carouselFormPage.createCarousel(formData);

    await expect(page).toHaveURL(/.*\/admin\/carousel\/edit\/\d+/);
  });

  test('should navigate back to carousel list when cancel is clicked', async ({ page, carouselFormPage }) => {
    await carouselFormPage.cancel();
    await expect(page).toHaveURL(/.*\/admin\/carousel$/);
  });
});
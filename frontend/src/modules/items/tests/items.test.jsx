import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { MemoryRouter, Routes, Route } from 'react-router';
import { CreateItemPage } from '../pages/CreateItemPage';
import { ItemDetailsPage } from '../pages/ItemDetailsPage';
import { EditItemPage } from '../pages/EditItemPage';
import { itemsApi } from '../services/itemsApi';

// Mock the items API
vi.mock('../services/itemsApi', () => ({
  itemsApi: {
    createItem: vi.fn(),
    getItem: vi.fn(),
    updateItem: vi.fn(),
    submitItemForAssessment: vi.fn(),
    submitConditionAnswers: vi.fn(),
    confirmAssessment: vi.fn(),
    answerClarification: vi.fn(),
  }
}));

describe('Member 1 Items React Frontend', () => {
  
  beforeEach(() => {
    vi.clearAllMocks();
  });

  // 1. Create item form
  it('submits create item form successfully', async () => {
    itemsApi.createItem.mockResolvedValue({ id: '123' });
    
    render(
      <MemoryRouter initialEntries={['/items/new']}>
        <Routes>
          <Route path="/items/new" element={<CreateItemPage />} />
          <Route path="/items/:id" element={<div>Item Details Page</div>} />
        </Routes>
      </MemoryRouter>
    );

    fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: 'Test Item' } });
    fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
    fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Shelf A' } });
    
    fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));

    await waitFor(() => {
      expect(itemsApi.createItem).toHaveBeenCalledWith(expect.objectContaining({
        title: 'Test Item',
        category: 'Electronics',
        locationArea: 'Shelf A'
      }));
    });

    // Expect navigation to item details page
    await screen.findByText('Item Details Page');
  });

  // 2. Item rendering & Conditional UI (Submit button disabled/hidden if not draft)
  it('hides Submit button if item status is not Draft', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Confirmed Item',
      status: 'InAssessment', // Not draft
      conditionAnswers: [{ questionText: 'Q1', answer: 'A1' }]
    });

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    // Wait for load
    await screen.findByText('Confirmed Item');
    
    // Submit button should NOT be in the document
    expect(screen.queryByRole('button', { name: /Submit for Assessment/i })).not.toBeInTheDocument();
  });

  it('shows Submit button if item status is Draft and has condition answers', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Draft Item',
      status: 'Draft',
      conditionAnswers: [{ questionText: 'Q1', answer: 'A1' }]
    });

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('Draft Item');
    expect(screen.getByRole('button', { name: /Submit for Assessment/i })).toBeInTheDocument();
  });

  // 3. API Error handling (400, 403, 404, 500)
  it('displays validation errors (400) on create', async () => {
    const error = new Error('Bad Request');
    error.response = { status: 400, data: { detail: 'Title is required' } };
    itemsApi.createItem.mockRejectedValue(error);

    render(
      <MemoryRouter initialEntries={['/items/new']}>
        <Routes>
          <Route path="/items/new" element={<CreateItemPage />} />
        </Routes>
      </MemoryRouter>
    );

    // Fill required fields to pass HTML5 validation
    fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: 'Test' } });
    fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Cat' } });
    fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Loc' } });

    fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));

    await screen.findByText('Title is required');
  });

  it('displays 403 permission error on item details', async () => {
    const error = new Error('Forbidden');
    error.response = { status: 403, data: { detail: 'Forbidden' } };
    itemsApi.getItem.mockRejectedValue(error);

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('You do not have permission to view this item.');
  });

  it('displays 404 not found on item details', async () => {
    const error = new Error('Not found');
    error.response = { status: 404, data: { detail: 'Not found' } };
    itemsApi.getItem.mockRejectedValue(error);

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('Item not found.');
  });

  // 4. 409 concurrency handling
  it('displays concurrency alert (409) when updating an item with outdated version', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Draft Item',
      category: 'Cat',
      locationArea: 'Loc',
      status: 'Draft',
      version: 1
    });

    const error = new Error('Conflict');
    error.response = { status: 409, data: { detail: 'Concurrency conflict occurred.' } };
    itemsApi.updateItem.mockRejectedValue(error);

    render(
      <MemoryRouter initialEntries={['/items/123/edit']}>
        <Routes>
          <Route path="/items/:id/edit" element={<EditItemPage />} />
        </Routes>
      </MemoryRouter>
    );

    // Wait for the form to load
    await screen.findByDisplayValue('Draft Item');

    // Submit the edit form
    fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));

    // Verify concurrency alert is shown
    await screen.findByText('Concurrency conflict occurred.');
    expect(screen.getByRole('button', { name: /Reload Latest Version/i })).toBeInTheDocument();
  });

  // 5. Condition answer submission
  it('submits condition answers successfully', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Draft Item',
      status: 'Draft',
    });
    
    itemsApi.submitConditionAnswers.mockResolvedValue({});

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('Draft Item');

    // Fill in the static questions
    const textareas = screen.getAllByRole('textbox');
    fireEvent.change(textareas[textareas.length - 2], { target: { value: 'Yes, powers on' } });
    fireEvent.change(textareas[textareas.length - 1], { target: { value: 'No damage' } });

    fireEvent.click(screen.getByRole('button', { name: /Save Answers/i }));

    await waitFor(() => {
      expect(itemsApi.submitConditionAnswers).toHaveBeenCalled();
    });
  });

  // 6. Assessment confirmation
  it('confirms an assessment after explicit confirmation', async () => {
    window.confirm = vi.fn().mockReturnValue(true); // Mock explicit user confirm

    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Assessed Item',
      status: 'InAssessment',
      assessments: [
        { id: 'a1', status: 'PendingConfirmation', version: 1 }
      ]
    });

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('Assessed Item');

    fireEvent.click(screen.getByRole('button', { name: /Confirm Assessment/i }));

    await waitFor(() => {
      expect(itemsApi.confirmAssessment).toHaveBeenCalledWith('123', 'a1', { ownerConfirmation: true });
    });
  });

  // 7. Clarification answer
  it('submits an answer to a pending clarification', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Assessed Item',
      status: 'InAssessment',
      assessments: [
        { 
          id: 'a1', 
          status: 'AwaitingInformation', 
          version: 1,
          clarifications: [
            { id: 'c1', questionText: 'Is the power cable included?' }
          ]
        }
      ]
    });

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('Assessed Item');
    
    // Find the clarification input
    const input = screen.getByPlaceholderText('Type your answer here...');
    fireEvent.change(input, { target: { value: 'Yes, included.' } });

    fireEvent.click(screen.getByRole('button', { name: /Submit Answer/i }));

    await waitFor(() => {
      expect(itemsApi.answerClarification).toHaveBeenCalledWith('123', 'c1', { answer: 'Yes, included.' });
    });
  });

});

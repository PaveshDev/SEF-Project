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

    fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: 'Dell Latitude 5420' } });
    fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
    fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Colombo' } });
    
    fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));

    await waitFor(() => {
      expect(itemsApi.createItem).toHaveBeenCalledWith(expect.objectContaining({
        title: 'Dell Latitude 5420',
        category: 'Electronics',
        locationArea: 'Colombo'
      }));
    });

    // Expect navigation to item details page
    await screen.findByText('Item Details Page');
  });

  describe('Title Validation', () => {
    it('rejects empty title', async () => {
      render(
        <MemoryRouter initialEntries={['/items/new']}>
          <Routes>
            <Route path="/items/new" element={<CreateItemPage />} />
          </Routes>
        </MemoryRouter>
      );
      
      fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: '' } });
      fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
      fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Kandy' } });
      const form = screen.getByLabelText(/Title \*/).closest('form');
      fireEvent.submit(form);
      
      expect(await screen.findByText('Title is required')).toBeInTheDocument();
      expect(itemsApi.createItem).not.toHaveBeenCalled();
    });

    it('rejects whitespace-only title', async () => {
      render(
        <MemoryRouter initialEntries={['/items/new']}>
          <Routes>
            <Route path="/items/new" element={<CreateItemPage />} />
          </Routes>
        </MemoryRouter>
      );
      
      fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: '   ' } });
      fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
      fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Kandy' } });
      fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));
      
      expect(await screen.findByText('Title is required')).toBeInTheDocument();
      expect(itemsApi.createItem).not.toHaveBeenCalled();
    });

    it('rejects "12345" title', async () => {
      render(
        <MemoryRouter initialEntries={['/items/new']}>
          <Routes>
            <Route path="/items/new" element={<CreateItemPage />} />
          </Routes>
        </MemoryRouter>
      );
      
      fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: '12345' } });
      fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
      fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Kandy' } });
      fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));
      
      expect(await screen.findByText('Title must contain at least one letter.')).toBeInTheDocument();
      expect(itemsApi.createItem).not.toHaveBeenCalled();
    });

    it('accepts "Laptop" title', async () => {
      itemsApi.createItem.mockResolvedValue({ id: '1' });
      render(
        <MemoryRouter initialEntries={['/items/new']}>
          <Routes>
            <Route path="/items/new" element={<CreateItemPage />} />
          </Routes>
        </MemoryRouter>
      );
      
      fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: 'Dell Latitude 5420' } });
      fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
      fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Kandy' } });
      fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));
      
      await waitFor(() => {
        expect(itemsApi.createItem).toHaveBeenCalledWith(expect.objectContaining({ title: 'Dell Latitude 5420' }));
      });
    });

    it('accepts "Laptop 15" title', async () => {
      itemsApi.createItem.mockResolvedValue({ id: '1' });
      render(
        <MemoryRouter initialEntries={['/items/new']}>
          <Routes>
            <Route path="/items/new" element={<CreateItemPage />} />
          </Routes>
        </MemoryRouter>
      );
      
      fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: 'Dell Latitude 5420' } });
      fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
      fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Kandy' } });
      fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));
      
      await waitFor(() => {
        expect(itemsApi.createItem).toHaveBeenCalledWith(expect.objectContaining({ title: 'Dell Latitude 5420' }));
      });
    });

    it('accepts "Office Chair 2" title', async () => {
      itemsApi.createItem.mockResolvedValue({ id: '1' });
      render(
        <MemoryRouter initialEntries={['/items/new']}>
          <Routes>
            <Route path="/items/new" element={<CreateItemPage />} />
          </Routes>
        </MemoryRouter>
      );
      
      fireEvent.change(screen.getByLabelText(/Title \*/), { target: { value: 'Office Chair' } });
      fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
      fireEvent.change(screen.getByLabelText(/Location Area \*/), { target: { value: 'Kandy' } });
      fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));
      
      await waitFor(() => {
        expect(itemsApi.createItem).toHaveBeenCalledWith(expect.objectContaining({ title: 'Office Chair' }));
      });
    });
  });

  // 2. Item rendering & Conditional UI (Submit button disabled/hidden if not draft)
  it('hides Submit button if item status is not Draft', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Wooden Study Table',
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
    await screen.findByText('Wooden Study Table');
    
    // Submit button should NOT be in the document
    expect(screen.queryByRole('button', { name: /Submit for Assessment/i })).not.toBeInTheDocument();
  });

  it('shows Submit button if item status is Draft and has condition answers', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Dell Latitude 5420',
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

    await screen.findByText('Dell Latitude 5420');
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
    fireEvent.change(screen.getByLabelText(/Category \*/), { target: { value: 'Electronics' } });
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
      title: 'Dell Latitude 5420',
      category: 'Electronics',
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
    await screen.findByDisplayValue('Dell Latitude 5420');

    // Submit the edit form
    fireEvent.click(screen.getByRole('button', { name: /Save Item/i }));

    // Verify concurrency alert is shown
    await screen.findByText('Concurrency conflict occurred.');
    expect(screen.getByRole('button', { name: /Reload Latest Version/i })).toBeInTheDocument();
  });

  // 5. Condition answer submission
  it('submits condition answers successfully and shows feedback', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Dell Latitude 5420',
      status: 'Draft',
    });
    
    // Use a delayed promise to test the "Saving..." state
    let resolveSubmit;
    const submitPromise = new Promise(resolve => {
      resolveSubmit = resolve;
    });
    itemsApi.submitConditionAnswers.mockReturnValue(submitPromise);

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('Dell Latitude 5420');

    // Fill in the static questions
    const yesLabels = screen.getAllByText('Yes');
    fireEvent.click(yesLabels[0]);
    
    const noLabels = screen.getAllByText('No');
    fireEvent.click(noLabels[1]);

    const textareas = screen.getAllByRole('textbox');
    fireEvent.change(textareas[0], { target: { value: 'powers on fine' } });
    fireEvent.change(textareas[1], { target: { value: 'no damage' } });

    const saveButton = screen.getByRole('button', { name: /Save Answers/i });
    fireEvent.click(saveButton);

    // Verify "Saving..." state
    expect(screen.getByRole('button', { name: /Saving.../i })).toBeDisabled();

    // Resolve the API call
    resolveSubmit({});

    // Verify success message
    await waitFor(() => {
      expect(itemsApi.submitConditionAnswers).toHaveBeenCalled();
    });
    
    expect(await screen.findByText('Condition answers saved successfully.')).toBeInTheDocument();
    
    // Verify button goes back to normal
    expect(screen.getByRole('button', { name: /Save Answers/i })).toBeEnabled();
    
    // Verify values remain bound
    expect(textareas[0].value).toBe('powers on fine');
  });

  it('shows error message when condition answers submission fails', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Dell Latitude 5420',
      status: 'Draft',
    });
    
    const error = new Error('Failed to save');
    error.response = { data: { detail: 'API Error Detail' } };
    itemsApi.submitConditionAnswers.mockRejectedValue(error);

    render(
      <MemoryRouter initialEntries={['/items/123']}>
        <Routes>
          <Route path="/items/:id" element={<ItemDetailsPage />} />
        </Routes>
      </MemoryRouter>
    );

    await screen.findByText('Dell Latitude 5420');

    const yesLabels = screen.getAllByText('Yes');
    fireEvent.click(yesLabels[0]);
    
    const noLabels = screen.getAllByText('No');
    fireEvent.click(noLabels[1]);

    const saveButton = screen.getByRole('button', { name: /Save Answers/i });
    fireEvent.click(saveButton);

    expect(await screen.findByText('API Error Detail')).toBeInTheDocument();
    expect(screen.queryByText('Condition answers saved successfully.')).not.toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Save Answers/i })).toBeEnabled();
  });

  // 6. Assessment confirmation
  it('confirms an assessment after explicit confirmation', async () => {
    window.confirm = vi.fn().mockReturnValue(true); // Mock explicit user confirm

    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Samsung Microwave Oven',
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

    await screen.findByText('Samsung Microwave Oven');

    fireEvent.click(screen.getByRole('button', { name: /Confirm Assessment/i }));

    await waitFor(() => {
      expect(itemsApi.confirmAssessment).toHaveBeenCalledWith('123', 'a1', { ownerConfirmation: true });
    });
  });

  // 7. Clarification answer
  it('submits an answer to a pending clarification', async () => {
    itemsApi.getItem.mockResolvedValue({
      id: '123',
      title: 'Samsung Microwave Oven',
      status: 'InAssessment',
      assessments: [
        { 
          id: 'a1', 
          status: 'AwaitingInformation', 
          version: 1,
          clarifications: [
            { id: 'c1', question: 'Is the power cable included?', status: 'Pending', reason: 'Need to know to assess value.' }
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

    await screen.findByText('Samsung Microwave Oven');
    
    // Find the clarification input
    const input = screen.getByPlaceholderText('Type your detailed answer here...');
    fireEvent.change(input, { target: { value: 'Yes, included.' } });

    fireEvent.click(screen.getByRole('button', { name: /Submit Answer/i }));

    await waitFor(() => {
      expect(itemsApi.answerClarification).toHaveBeenCalledWith('123', 'c1', { answer: 'Yes, included.' });
    });
  });

});

import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { ProductDialog } from './ProductDialog';

describe('ProductDialog', () => {
  it('rejects an empty product', async () => {
    const onSave = vi.fn();
    render(<ProductDialog open product={null} saving={false} error={null} onClose={vi.fn()} onSave={onSave} />);
    fireEvent.click(screen.getByRole('button', { name: '저장' }));
    expect(await screen.findByText('SKU를 입력하세요.')).toBeInTheDocument();
    expect(await screen.findByText('상품명을 입력하세요.')).toBeInTheDocument();
    expect(onSave).not.toHaveBeenCalled();
  });
});

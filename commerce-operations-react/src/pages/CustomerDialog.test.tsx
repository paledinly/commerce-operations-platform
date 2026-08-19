import { fireEvent, render, screen } from '@testing-library/react';
import { describe, expect, it, vi } from 'vitest';
import { CustomerDialog } from './CustomerDialog';

describe('CustomerDialog', () => {
  it('rejects empty customer data', async () => {
    const onSave = vi.fn();
    render(<CustomerDialog open customer={null} saving={false} error={null} onClose={vi.fn()} onSave={onSave} />);
    fireEvent.click(screen.getByRole('button', { name: '저장' }));
    expect(await screen.findByText('올바른 이메일을 입력하세요.')).toBeInTheDocument();
    expect(await screen.findByText('이름을 입력하세요.')).toBeInTheDocument();
    expect(onSave).not.toHaveBeenCalled();
  });
});

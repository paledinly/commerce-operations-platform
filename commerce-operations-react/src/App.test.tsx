import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { beforeEach, describe, expect, it } from 'vitest';
import { App } from './App';
import { useAuthStore } from './store/auth';

describe('App', () => {
  beforeEach(() => useAuthStore.getState().logout());
  it('redirects unauthenticated users to login', async () => {
    render(<QueryClientProvider client={new QueryClient()}><MemoryRouter initialEntries={['/']}><App /></MemoryRouter></QueryClientProvider>);
    expect(await screen.findByRole('heading', { name: 'Commerce Operations' })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: '로그인' })).toBeInTheDocument();
  });
});

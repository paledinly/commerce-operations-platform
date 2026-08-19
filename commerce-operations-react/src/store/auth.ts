import { create } from 'zustand';
import type { OperatorProfile } from '../api/auth';

interface AuthState {
  token: string | null;
  operator: OperatorProfile | null;
  login: (token: string, operator: OperatorProfile) => void;
  logout: () => void;
}

const savedToken = sessionStorage.getItem('commerce.accessToken');
export const useAuthStore = create<AuthState>((set) => ({
  token: savedToken,
  operator: null,
  login: (token, operator) => {
    sessionStorage.setItem('commerce.accessToken', token);
    set({ token, operator });
  },
  logout: () => {
    sessionStorage.removeItem('commerce.accessToken');
    set({ token: null, operator: null });
  }
}));


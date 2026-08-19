import { api } from './client';

export interface OperatorProfile { id: number; email: string; displayName: string; role: string }
export interface LoginResponse { accessToken: string; expiresAtUtc: string; operator: OperatorProfile }

export async function login(email: string, password: string) {
  const response = await api.post<LoginResponse>('/auth/login', { email, password });
  return response.data;
}

export async function getMe() {
  const response = await api.get<OperatorProfile>('/auth/me');
  return response.data;
}


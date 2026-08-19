import { api } from './client';

export type CustomerStatus = 'ACTIVE' | 'SUSPENDED' | 'WITHDRAWN';
export interface Customer { id: number; email: string; name: string; phone: string; status: CustomerStatus; createdAt: string; updatedAt: string }
export interface CustomerInput { email: string; name: string; phone: string; status: CustomerStatus }
export interface CustomerList { items: Customer[]; page: number; pageSize: number; totalCount: number }
export interface CustomerSearch { search?: string; status?: CustomerStatus | ''; page: number; pageSize: number }

export async function getCustomers(params: CustomerSearch) { return (await api.get<CustomerList>('/customers', { params })).data; }
export async function createCustomer(input: CustomerInput) { return (await api.post<Customer>('/customers', input)).data; }
export async function updateCustomer(id: number, input: CustomerInput) { return (await api.put<Customer>(`/customers/${id}`, input)).data; }
export async function changeCustomerStatus(id: number, status: CustomerStatus) { return (await api.patch<Customer>(`/customers/${id}/status`, { status })).data; }


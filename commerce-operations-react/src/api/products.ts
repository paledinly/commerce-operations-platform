import { api } from './client';

export type ProductStatus = 'ACTIVE' | 'INACTIVE';
export interface Product { id: number; sku: string; name: string; price: number; status: ProductStatus; createdAt: string; updatedAt: string }
export interface ProductInput { sku: string; name: string; price: number; status: ProductStatus }
export interface ProductList { items: Product[]; page: number; pageSize: number; totalCount: number }
export interface ProductSearch { search?: string; status?: ProductStatus | ''; page: number; pageSize: number }

export async function getProducts(params: ProductSearch) {
  const response = await api.get<ProductList>('/products', { params });
  return response.data;
}
export async function createProduct(input: ProductInput) { return (await api.post<Product>('/products', input)).data; }
export async function updateProduct(id: number, input: ProductInput) { return (await api.put<Product>(`/products/${id}`, input)).data; }
export async function changeProductStatus(id: number, status: ProductStatus) { return (await api.patch<Product>(`/products/${id}/status`, { status })).data; }


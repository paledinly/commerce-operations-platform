import { api } from './client';

export interface Inventory { productId: number; sku?: string; productName?: string; availableQuantity: number; reservedQuantity: number; version: number; updatedAt: string }
export interface InventoryPage { items: Inventory[]; page: number; pageSize: number; totalCount: number }
export interface InventoryMovement { id: number; productId: number; movementType: 'INITIAL' | 'ADJUSTMENT'; quantityDelta: number; availableAfter: number; reason: string; createdAt: string }

export async function getInventories(page: number, pageSize: number) { return (await api.get<InventoryPage>('/inventories', { params: { page, pageSize } })).data; }
export async function createInventory(productId: number, initialQuantity: number) { return (await api.post<Inventory>('/inventories', { productId, initialQuantity })).data; }
export async function adjustInventory(productId: number, quantityDelta: number, reason: string) { return (await api.post<Inventory>(`/inventories/${productId}/adjustments`, { quantityDelta, reason })).data; }
export async function getInventoryMovements(productId: number) { return (await api.get<InventoryMovement[]>(`/inventories/${productId}/movements`)).data; }


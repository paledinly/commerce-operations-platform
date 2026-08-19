import { api } from './client';
export interface DashboardSummary{totalProducts:number;activeProducts:number;totalCustomers:number;activeCustomers:number;totalOrders:number;netRevenue:number;ordersByStatus:Record<string,number>;availableQuantity:number;reservedQuantity:number;lowStockProducts:number;pendingEvents:number;generatedAtUtc:string}
export async function getDashboard(){return(await api.get<DashboardSummary>('/dashboard')).data;}

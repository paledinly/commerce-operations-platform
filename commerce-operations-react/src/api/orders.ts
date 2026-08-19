import { api } from './client';
export interface OrderItem { id:number;productId:number;sku:string;productName:string;unitPrice:number;quantity:number;lineAmount:number }
export type OrderStatus='CREATED'|'PAID'|'SHIPPED'|'COMPLETED'|'CANCELLED'|'REFUNDED';
export interface Order { id:number;customerId:number;customerEmail:string;customerName:string;status:OrderStatus;totalAmount:number;createdAt:string;updatedAt:string;items:OrderItem[] }
export interface Payment {id:number;orderId:number;transactionType:'PAYMENT'|'REFUND';amount:number;status:'APPROVED';referenceNo:string;createdAt:string}
export type OrderSummary = Omit<Order,'items'>;
export interface OrderPage { items:OrderSummary[];page:number;pageSize:number;totalCount:number }
export interface CreateOrderInput { customerId:number;items:{productId:number;quantity:number}[] }
export async function getOrders(page:number,pageSize:number){return(await api.get<OrderPage>('/orders',{params:{page,pageSize}})).data;}
export async function getOrder(id:number){return(await api.get<Order>(`/orders/${id}`)).data;}
export async function createOrder(input:CreateOrderInput){return(await api.post<Order>('/orders',input)).data;}
export async function cancelOrder(id:number){return(await api.post<Order>(`/orders/${id}/cancel`)).data;}
export async function payOrder(id:number){return(await api.post<Payment>(`/orders/${id}/pay`)).data;}
export async function refundOrder(id:number){return(await api.post<Payment>(`/orders/${id}/refund`)).data;}
export interface Shipment {id:number;orderId:number;carrier:string;trackingNumber:string;status:'SHIPPED'|'DELIVERED';shippedAt:string;deliveredAt?:string}
export async function shipOrder(id:number,carrier:string,trackingNumber:string){return(await api.post<Shipment>(`/orders/${id}/ship`,{carrier,trackingNumber})).data;}
export async function deliverOrder(id:number){return(await api.post<Shipment>(`/orders/${id}/deliver`)).data;}

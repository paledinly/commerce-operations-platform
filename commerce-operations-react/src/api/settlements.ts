import { api } from './client';
export interface DailySettlement{settlementDate:string;paymentAmount:number;refundAmount:number;netAmount:number;paymentCount:number;refundCount:number;calculatedAt:string}
export interface SettlementPage{items:DailySettlement[];from:string;to:string}export interface RebuildResult{from:string;to:string;rebuiltDays:number}
export async function getSettlements(from:string,to:string){return(await api.get<SettlementPage>('/settlements',{params:{from,to}})).data;}export async function rebuildSettlements(from:string,to:string){return(await api.post<RebuildResult>('/settlements/rebuild',null,{params:{from,to}})).data;}

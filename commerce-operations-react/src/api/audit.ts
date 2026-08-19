import { api } from './client';
export interface AuditLog{id:number;operatorId?:number;operatorEmail:string;httpMethod:string;requestPath:string;resourceType:string;statusCode:number;durationMs:number;ipAddress?:string;userAgent?:string;createdAt:string}
export interface AuditPage{items:AuditLog[];page:number;pageSize:number;totalCount:number}
export async function getAuditLogs(params:{from?:string;to?:string;resourceType?:string;result?:string;page:number;pageSize:number}){return(await api.get<AuditPage>('/audit-logs',{params})).data;}

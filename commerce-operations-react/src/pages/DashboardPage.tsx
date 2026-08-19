import { useQuery } from '@tanstack/react-query';
import { AppBar, Button, Container, Tab, Tabs, Toolbar, Typography } from '@mui/material';
import { useState } from 'react';
import { getMe } from '../api/auth';
import { useAuthStore } from '../store/auth';
import { CustomersPage } from './CustomersPage';
import { ProductsPage } from './ProductsPage';
import { InventoriesPage } from './InventoriesPage';
import { OrdersPage } from './OrdersPage';
import { OverviewPage } from './OverviewPage';
import { SettlementsPage } from './SettlementsPage';
import { AuditLogsPage } from './AuditLogsPage';

export function DashboardPage() {
  const [section, setSection] = useState<'overview' | 'products' | 'customers' | 'inventories' | 'orders' | 'settlements' | 'audit'>('overview');
  const logout = useAuthStore((state) => state.logout);
  const stored = useAuthStore((state) => state.operator);
  const profile = useQuery({ queryKey: ['auth', 'me'], queryFn: getMe, retry: false });
  const operator = profile.data ?? stored;
  return <>
    <AppBar position="static"><Toolbar><Typography variant="h6" sx={{ flexGrow: 1 }}>Commerce Operations</Typography><Typography variant="body2" sx={{ mr: 2 }}>{operator?.displayName}</Typography><Button color="inherit" onClick={logout}>로그아웃</Button></Toolbar></AppBar>
    <Tabs value={section} onChange={(_, value: 'overview' | 'products' | 'customers' | 'inventories' | 'orders' | 'settlements' | 'audit') => setSection(value)} sx={{ px: 3, borderBottom: 1, borderColor: 'divider' }}><Tab value="overview" label="대시보드" /><Tab value="products" label="상품" /><Tab value="customers" label="회원" /><Tab value="inventories" label="재고" /><Tab value="orders" label="주문" /><Tab value="settlements" label="정산" /><Tab value="audit" label="감사 로그" /></Tabs>
    <Container maxWidth="xl" sx={{ py: 4 }}>{section === 'overview' ? <OverviewPage /> : section === 'products' ? <ProductsPage /> : section === 'customers' ? <CustomersPage /> : section === 'inventories' ? <InventoriesPage /> : section === 'orders' ? <OrdersPage /> : section === 'settlements' ? <SettlementsPage /> : <AuditLogsPage />}</Container>
  </>;
}

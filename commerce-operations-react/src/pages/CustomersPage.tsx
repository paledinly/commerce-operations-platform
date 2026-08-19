import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Box, Button, FormControl, InputLabel, MenuItem, Paper, Select, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, TextField, Typography } from '@mui/material';
import axios from 'axios';
import { useState } from 'react';
import { changeCustomerStatus, createCustomer, getCustomers, updateCustomer, type Customer, type CustomerInput, type CustomerStatus } from '../api/customers';
import { CustomerDialog } from './CustomerDialog';

const statusLabel: Record<CustomerStatus, string> = { ACTIVE: '정상', SUSPENDED: '정지', WITHDRAWN: '탈퇴' };

export function CustomersPage() {
  const client = useQueryClient();
  const [draftSearch, setDraftSearch] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<CustomerStatus | ''>('');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selected, setSelected] = useState<Customer | null>(null);
  const customers = useQuery({ queryKey: ['customers', { search, status, page, pageSize }], queryFn: () => getCustomers({ search: search || undefined, status, page: page + 1, pageSize }) });
  const refresh = () => client.invalidateQueries({ queryKey: ['customers'] });
  const save = useMutation({ mutationFn: (input: CustomerInput) => selected ? updateCustomer(selected.id, input) : createCustomer(input), onSuccess: () => { setDialogOpen(false); refresh(); } });
  const changeStatus = useMutation({ mutationFn: ({ customer, next }: { customer: Customer; next: CustomerStatus }) => changeCustomerStatus(customer.id, next), onSuccess: refresh });
  const errorMessage = save.isError ? axios.isAxiosError(save.error) && save.error.response?.status === 409 ? '이미 등록된 이메일입니다.' : '회원 정보를 저장하지 못했습니다.' : null;
  const submitSearch = () => { setPage(0); setSearch(draftSearch.trim()); };

  return <Box>
    <Box sx={{ display: 'flex', flexDirection: { xs: 'column', md: 'row' }, justifyContent: 'space-between', alignItems: { md: 'center' }, gap: 2, mb: 3 }}>
      <Box><Typography variant="h4" component="h1">회원 관리</Typography><Typography color="text.secondary">회원 정보와 이용 상태를 관리합니다.</Typography></Box>
      <Button variant="contained" onClick={() => { setSelected(null); save.reset(); setDialogOpen(true); }}>회원 등록</Button>
    </Box>
    <Paper sx={{ p: 2, mb: 2 }}><Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
      <TextField label="이메일, 이름 또는 전화번호" value={draftSearch} onChange={(event) => setDraftSearch(event.target.value)} onKeyDown={(event) => { if (event.key === 'Enter') submitSearch(); }} fullWidth />
      <FormControl sx={{ minWidth: 160 }}><InputLabel>상태</InputLabel><Select label="상태" value={status} onChange={(event) => { setPage(0); setStatus(event.target.value as CustomerStatus | ''); }}><MenuItem value="">전체</MenuItem><MenuItem value="ACTIVE">정상</MenuItem><MenuItem value="SUSPENDED">정지</MenuItem><MenuItem value="WITHDRAWN">탈퇴</MenuItem></Select></FormControl>
      <Button variant="outlined" onClick={submitSearch}>검색</Button>
    </Stack></Paper>
    {customers.isError && <Alert severity="error" sx={{ mb: 2 }}>회원 목록을 불러오지 못했습니다.</Alert>}
    <TableContainer component={Paper}><Table><TableHead><TableRow><TableCell>이메일</TableCell><TableCell>이름</TableCell><TableCell>전화번호</TableCell><TableCell>상태</TableCell><TableCell align="right">작업</TableCell></TableRow></TableHead>
      <TableBody>{customers.isLoading ? <TableRow><TableCell colSpan={5}>불러오는 중…</TableCell></TableRow> : customers.data?.items.length === 0 ? <TableRow><TableCell colSpan={5}>등록된 회원이 없습니다.</TableCell></TableRow> : customers.data?.items.map((customer) => <TableRow key={customer.id} hover><TableCell>{customer.email}</TableCell><TableCell>{customer.name}</TableCell><TableCell>{customer.phone}</TableCell><TableCell><Select size="small" value={customer.status} disabled={changeStatus.isPending} onChange={(event) => changeStatus.mutate({ customer, next: event.target.value as CustomerStatus })}>{Object.entries(statusLabel).map(([value, label]) => <MenuItem key={value} value={value}>{label}</MenuItem>)}</Select></TableCell><TableCell align="right"><Button size="small" onClick={() => { setSelected(customer); save.reset(); setDialogOpen(true); }}>수정</Button></TableCell></TableRow>)}</TableBody>
    </Table><TablePagination component="div" count={customers.data?.totalCount ?? 0} page={page} rowsPerPage={pageSize} onPageChange={(_, value) => setPage(value)} onRowsPerPageChange={(event) => { setPage(0); setPageSize(Number(event.target.value)); }} rowsPerPageOptions={[10, 20, 50, 100]} labelRowsPerPage="페이지당 행" /></TableContainer>
    <CustomerDialog open={dialogOpen} customer={selected} saving={save.isPending} error={errorMessage} onClose={() => setDialogOpen(false)} onSave={(input) => save.mutate(input)} />
  </Box>;
}


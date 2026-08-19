import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Box, Button, Chip, FormControl, InputLabel, MenuItem, Paper, Select, Stack, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, TextField, Typography } from '@mui/material';
import axios from 'axios';
import { useState } from 'react';
import { changeProductStatus, createProduct, getProducts, updateProduct, type Product, type ProductInput, type ProductStatus } from '../api/products';
import { ProductDialog } from './ProductDialog';

const money = new Intl.NumberFormat('ko-KR', { style: 'currency', currency: 'KRW', maximumFractionDigits: 2 });

export function ProductsPage() {
  const client = useQueryClient();
  const [draftSearch, setDraftSearch] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<ProductStatus | ''>('');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selected, setSelected] = useState<Product | null>(null);
  const products = useQuery({ queryKey: ['products', { search, status, page, pageSize }], queryFn: () => getProducts({ search: search || undefined, status, page: page + 1, pageSize }) });
  const refresh = () => client.invalidateQueries({ queryKey: ['products'] });
  const save = useMutation({ mutationFn: (input: ProductInput) => selected ? updateProduct(selected.id, input) : createProduct(input), onSuccess: () => { setDialogOpen(false); refresh(); } });
  const toggle = useMutation({ mutationFn: (product: Product) => changeProductStatus(product.id, product.status === 'ACTIVE' ? 'INACTIVE' : 'ACTIVE'), onSuccess: refresh });
  const errorMessage = save.isError ? axios.isAxiosError(save.error) && save.error.response?.status === 409 ? '이미 사용 중인 SKU입니다.' : '상품을 저장하지 못했습니다.' : null;
  const openCreate = () => { setSelected(null); save.reset(); setDialogOpen(true); };
  const openEdit = (product: Product) => { setSelected(product); save.reset(); setDialogOpen(true); };

  return <Box>
    <Box sx={{ display: 'flex', flexDirection: { xs: 'column', md: 'row' }, justifyContent: 'space-between', alignItems: { md: 'center' }, gap: 2, mb: 3 }}>
      <Box><Typography variant="h4" component="h1">상품 관리</Typography><Typography color="text.secondary">상품을 등록하고 판매 상태를 관리합니다.</Typography></Box>
      <Button variant="contained" onClick={openCreate}>상품 등록</Button>
    </Box>
    <Paper sx={{ p: 2, mb: 2 }}>
      <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
        <TextField label="SKU 또는 상품명" value={draftSearch} onChange={(event) => setDraftSearch(event.target.value)} onKeyDown={(event) => { if (event.key === 'Enter') { setPage(0); setSearch(draftSearch.trim()); } }} fullWidth />
        <FormControl sx={{ minWidth: 160 }}><InputLabel>상태</InputLabel><Select label="상태" value={status} onChange={(event) => { setPage(0); setStatus(event.target.value as ProductStatus | ''); }}><MenuItem value="">전체</MenuItem><MenuItem value="ACTIVE">활성</MenuItem><MenuItem value="INACTIVE">비활성</MenuItem></Select></FormControl>
        <Button variant="outlined" onClick={() => { setPage(0); setSearch(draftSearch.trim()); }}>검색</Button>
      </Stack>
    </Paper>
    {products.isError && <Alert severity="error" sx={{ mb: 2 }}>상품 목록을 불러오지 못했습니다.</Alert>}
    <TableContainer component={Paper}>
      <Table><TableHead><TableRow><TableCell>SKU</TableCell><TableCell>상품명</TableCell><TableCell align="right">판매 가격</TableCell><TableCell>상태</TableCell><TableCell align="right">작업</TableCell></TableRow></TableHead>
        <TableBody>{products.isLoading ? <TableRow><TableCell colSpan={5}>불러오는 중…</TableCell></TableRow> : products.data?.items.length === 0 ? <TableRow><TableCell colSpan={5}>등록된 상품이 없습니다.</TableCell></TableRow> : products.data?.items.map((product) => <TableRow key={product.id} hover><TableCell>{product.sku}</TableCell><TableCell>{product.name}</TableCell><TableCell align="right">{money.format(product.price)}</TableCell><TableCell><Chip size="small" color={product.status === 'ACTIVE' ? 'success' : 'default'} label={product.status === 'ACTIVE' ? '활성' : '비활성'} /></TableCell><TableCell align="right"><Button size="small" onClick={() => openEdit(product)}>수정</Button><Button size="small" color={product.status === 'ACTIVE' ? 'warning' : 'success'} disabled={toggle.isPending} onClick={() => toggle.mutate(product)}>{product.status === 'ACTIVE' ? '비활성화' : '활성화'}</Button></TableCell></TableRow>)}</TableBody>
      </Table>
      <TablePagination component="div" count={products.data?.totalCount ?? 0} page={page} rowsPerPage={pageSize} onPageChange={(_, value) => setPage(value)} onRowsPerPageChange={(event) => { setPage(0); setPageSize(Number(event.target.value)); }} rowsPerPageOptions={[10, 20, 50, 100]} labelRowsPerPage="페이지당 행" />
    </TableContainer>
    <ProductDialog open={dialogOpen} product={selected} saving={save.isPending} error={errorMessage} onClose={() => setDialogOpen(false)} onSave={(input) => save.mutate(input)} />
  </Box>;
}

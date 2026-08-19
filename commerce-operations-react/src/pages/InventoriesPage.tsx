import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Alert, Box, Button, Paper, Table, TableBody, TableCell, TableContainer, TableHead, TablePagination, TableRow, Typography } from '@mui/material';
import axios from 'axios';
import { useState } from 'react';
import { adjustInventory, createInventory, getInventories, getInventoryMovements, type Inventory } from '../api/inventories';
import { getProducts } from '../api/products';
import { InventoryAdjustDialog, type InventoryAdjustValues } from './InventoryAdjustDialog';
import { InventoryCreateDialog, type InventoryCreateValues } from './InventoryCreateDialog';
import { InventoryMovementsDialog } from './InventoryMovementsDialog';

export function InventoriesPage() {
  const client = useQueryClient(); const [page,setPage]=useState(0); const [pageSize,setPageSize]=useState(20); const [createOpen,setCreateOpen]=useState(false); const [selected,setSelected]=useState<Inventory|null>(null); const [adjustOpen,setAdjustOpen]=useState(false); const [historyOpen,setHistoryOpen]=useState(false);
  const inventories=useQuery({queryKey:['inventories',{page,pageSize}],queryFn:()=>getInventories(page+1,pageSize)});
  const products=useQuery({queryKey:['products','inventory-options'],queryFn:()=>getProducts({page:1,pageSize:100,status:'ACTIVE'})});
  const movements=useQuery({queryKey:['inventory-movements',selected?.productId],queryFn:()=>getInventoryMovements(selected!.productId),enabled:historyOpen&&!!selected});
  const refresh=()=>client.invalidateQueries({queryKey:['inventories']});
  const create=useMutation({mutationFn:(values:InventoryCreateValues)=>createInventory(values.productId,values.initialQuantity),onSuccess:()=>{setCreateOpen(false);refresh();}});
  const adjust=useMutation({mutationFn:(values:InventoryAdjustValues)=>adjustInventory(selected!.productId,values.quantityDelta,values.reason),onSuccess:()=>{setAdjustOpen(false);refresh();}});
  const message=(error:unknown,fallback:string)=>axios.isAxiosError(error)&&error.response?.status===409?'이미 재고가 존재하거나 가용 재고가 부족합니다.':fallback;
  return <Box><Box sx={{display:'flex',justifyContent:'space-between',alignItems:'center',mb:3}}><Box><Typography variant="h4">재고 관리</Typography><Typography color="text.secondary">상품별 가용·예약 재고와 조정 이력을 관리합니다.</Typography></Box><Button variant="contained" onClick={()=>{create.reset();setCreateOpen(true);}}>재고 생성</Button></Box>
    {inventories.isError&&<Alert severity="error" sx={{mb:2}}>재고 목록을 불러오지 못했습니다.</Alert>}<TableContainer component={Paper}><Table><TableHead><TableRow><TableCell>SKU</TableCell><TableCell>상품명</TableCell><TableCell align="right">가용</TableCell><TableCell align="right">예약</TableCell><TableCell align="right">버전</TableCell><TableCell align="right">작업</TableCell></TableRow></TableHead><TableBody>{inventories.isLoading?<TableRow><TableCell colSpan={6}>불러오는 중…</TableCell></TableRow>:inventories.data?.items.length===0?<TableRow><TableCell colSpan={6}>등록된 재고가 없습니다.</TableCell></TableRow>:inventories.data?.items.map((item)=><TableRow key={item.productId}><TableCell>{item.sku??`#${item.productId}`}</TableCell><TableCell>{item.productName??'삭제되거나 찾을 수 없는 상품'}</TableCell><TableCell align="right">{item.availableQuantity}</TableCell><TableCell align="right">{item.reservedQuantity}</TableCell><TableCell align="right">{item.version}</TableCell><TableCell align="right"><Button size="small" onClick={()=>{setSelected(item);adjust.reset();setAdjustOpen(true);}}>조정</Button><Button size="small" onClick={()=>{setSelected(item);setHistoryOpen(true);}}>이력</Button></TableCell></TableRow>)}</TableBody></Table><TablePagination component="div" count={inventories.data?.totalCount??0} page={page} rowsPerPage={pageSize} onPageChange={(_,value)=>setPage(value)} onRowsPerPageChange={(event)=>{setPage(0);setPageSize(Number(event.target.value));}} rowsPerPageOptions={[10,20,50,100]} labelRowsPerPage="페이지당 행" /></TableContainer>
    <InventoryCreateDialog open={createOpen} products={products.data?.items??[]} saving={create.isPending} error={create.isError?message(create.error,'재고를 생성하지 못했습니다.'):null} onClose={()=>setCreateOpen(false)} onSave={(values)=>create.mutate(values)} />
    <InventoryAdjustDialog open={adjustOpen} inventory={selected} saving={adjust.isPending} error={adjust.isError?message(adjust.error,'재고를 조정하지 못했습니다.'):null} onClose={()=>setAdjustOpen(false)} onSave={(values)=>adjust.mutate(values)} />
    <InventoryMovementsDialog open={historyOpen} inventory={selected} movements={movements.data??[]} onClose={()=>setHistoryOpen(false)} />
  </Box>;
}


import { zodResolver } from '@hookform/resolvers/zod';
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, TextField } from '@mui/material';
import { useEffect } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { z } from 'zod';
import type { Product, ProductInput } from '../api/products';

const schema = z.object({
  sku: z.string().trim().min(1, 'SKU를 입력하세요.').max(50).regex(/^[A-Za-z0-9][A-Za-z0-9._-]*$/, '영문, 숫자, 점, 밑줄, 하이픈만 사용할 수 있습니다.'),
  name: z.string().trim().min(1, '상품명을 입력하세요.').max(200),
  price: z.number().min(0, '가격은 0 이상이어야 합니다.').max(999999999999.99),
  status: z.enum(['ACTIVE', 'INACTIVE'])
});

interface Props { open: boolean; product: Product | null; saving: boolean; error: string | null; onClose: () => void; onSave: (input: ProductInput) => void }

export function ProductDialog({ open, product, saving, error, onClose, onSave }: Props) {
  const { register, control, handleSubmit, reset, formState: { errors } } = useForm<ProductInput>({ resolver: zodResolver(schema), defaultValues: { sku: '', name: '', price: 0, status: 'ACTIVE' } });
  useEffect(() => reset(product ? { sku: product.sku, name: product.name, price: product.price, status: product.status } : { sku: '', name: '', price: 0, status: 'ACTIVE' }), [product, open, reset]);
  return <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm">
    <DialogTitle>{product ? '상품 수정' : '상품 등록'}</DialogTitle>
    <DialogContent>
      <TextField label="SKU" fullWidth margin="normal" {...register('sku')} error={!!errors.sku} helperText={errors.sku?.message} />
      <TextField label="상품명" fullWidth margin="normal" {...register('name')} error={!!errors.name} helperText={errors.name?.message} />
      <TextField label="판매 가격" type="number" fullWidth margin="normal" slotProps={{ htmlInput: { min: 0, step: '0.01' } }} {...register('price', { valueAsNumber: true })} error={!!errors.price} helperText={errors.price?.message} />
      <Controller name="status" control={control} render={({ field }) => <TextField select label="상태" fullWidth margin="normal" {...field}><MenuItem value="ACTIVE">활성</MenuItem><MenuItem value="INACTIVE">비활성</MenuItem></TextField>} />
      {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
    </DialogContent>
    <DialogActions><Button onClick={onClose} disabled={saving}>취소</Button><Button variant="contained" onClick={handleSubmit(onSave)} disabled={saving}>{saving ? '저장 중…' : '저장'}</Button></DialogActions>
  </Dialog>;
}

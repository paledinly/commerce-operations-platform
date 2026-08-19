import { zodResolver } from '@hookform/resolvers/zod';
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, TextField } from '@mui/material';
import { Controller, useForm } from 'react-hook-form';
import { z } from 'zod';
import type { Product } from '../api/products';

const schema = z.object({ productId: z.number().int().positive('상품을 선택하세요.'), initialQuantity: z.number().int().min(0, '초기 재고는 0 이상이어야 합니다.') });
export interface InventoryCreateValues { productId: number; initialQuantity: number }
interface Props { open: boolean; products: Product[]; saving: boolean; error: string | null; onClose: () => void; onSave: (values: InventoryCreateValues) => void }

export function InventoryCreateDialog({ open, products, saving, error, onClose, onSave }: Props) {
  const { control, register, handleSubmit, reset, formState: { errors } } = useForm<InventoryCreateValues>({ resolver: zodResolver(schema), defaultValues: { productId: 0, initialQuantity: 0 } });
  const close = () => { reset(); onClose(); };
  return <Dialog open={open} onClose={saving ? undefined : close} fullWidth maxWidth="sm"><DialogTitle>재고 항목 생성</DialogTitle><DialogContent>
    <Controller name="productId" control={control} render={({ field }) => <TextField select label="상품" fullWidth margin="normal" {...field} onChange={(event) => field.onChange(Number(event.target.value))} error={!!errors.productId} helperText={errors.productId?.message}><MenuItem value={0}>상품 선택</MenuItem>{products.map((product) => <MenuItem key={product.id} value={product.id}>{product.sku} · {product.name}</MenuItem>)}</TextField>} />
    <TextField label="초기 가용 재고" type="number" fullWidth margin="normal" {...register('initialQuantity', { valueAsNumber: true })} error={!!errors.initialQuantity} helperText={errors.initialQuantity?.message} slotProps={{ htmlInput: { min: 0, step: 1 } }} />
    {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
  </DialogContent><DialogActions><Button onClick={close} disabled={saving}>취소</Button><Button variant="contained" onClick={handleSubmit(onSave)} disabled={saving}>생성</Button></DialogActions></Dialog>;
}


import { zodResolver } from '@hookform/resolvers/zod';
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField, Typography } from '@mui/material';
import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import type { Inventory } from '../api/inventories';

const schema = z.object({ quantityDelta: z.number().int().refine((value) => value !== 0, '0이 아닌 증감 수량을 입력하세요.'), reason: z.string().trim().min(1, '조정 사유를 입력하세요.').max(200) });
export interface InventoryAdjustValues { quantityDelta: number; reason: string }
interface Props { open: boolean; inventory: Inventory | null; saving: boolean; error: string | null; onClose: () => void; onSave: (values: InventoryAdjustValues) => void }

export function InventoryAdjustDialog({ open, inventory, saving, error, onClose, onSave }: Props) {
  const { register, handleSubmit, reset, formState: { errors } } = useForm<InventoryAdjustValues>({ resolver: zodResolver(schema), defaultValues: { quantityDelta: 0, reason: '' } });
  useEffect(() => reset({ quantityDelta: 0, reason: '' }), [inventory, open, reset]);
  return <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm"><DialogTitle>재고 조정</DialogTitle><DialogContent>
    <Typography color="text.secondary">{inventory?.sku} · 현재 가용 {inventory?.availableQuantity}</Typography>
    <TextField label="증감 수량" type="number" fullWidth margin="normal" {...register('quantityDelta', { valueAsNumber: true })} error={!!errors.quantityDelta} helperText={errors.quantityDelta?.message ?? '입고는 양수, 차감은 음수로 입력합니다.'} />
    <TextField label="조정 사유" fullWidth margin="normal" multiline minRows={2} {...register('reason')} error={!!errors.reason} helperText={errors.reason?.message} />
    {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
  </DialogContent><DialogActions><Button onClick={onClose} disabled={saving}>취소</Button><Button variant="contained" onClick={handleSubmit(onSave)} disabled={saving}>적용</Button></DialogActions></Dialog>;
}


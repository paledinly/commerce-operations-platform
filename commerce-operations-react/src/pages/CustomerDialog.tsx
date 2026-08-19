import { zodResolver } from '@hookform/resolvers/zod';
import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, TextField } from '@mui/material';
import { useEffect } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { z } from 'zod';
import type { Customer, CustomerInput } from '../api/customers';

const schema = z.object({
  email: z.email('올바른 이메일을 입력하세요.').max(255),
  name: z.string().trim().min(1, '이름을 입력하세요.').max(100),
  phone: z.string().trim().refine((value) => /^\+?[0-9]{8,15}$/.test(value.replace(/[\s()-]/g, '')), '전화번호는 8~15자리 숫자여야 합니다.'),
  status: z.enum(['ACTIVE', 'SUSPENDED', 'WITHDRAWN'])
});

interface Props { open: boolean; customer: Customer | null; saving: boolean; error: string | null; onClose: () => void; onSave: (input: CustomerInput) => void }

export function CustomerDialog({ open, customer, saving, error, onClose, onSave }: Props) {
  const { register, control, handleSubmit, reset, formState: { errors } } = useForm<CustomerInput>({ resolver: zodResolver(schema), defaultValues: { email: '', name: '', phone: '', status: 'ACTIVE' } });
  useEffect(() => reset(customer ? { email: customer.email, name: customer.name, phone: customer.phone, status: customer.status } : { email: '', name: '', phone: '', status: 'ACTIVE' }), [customer, open, reset]);
  return <Dialog open={open} onClose={saving ? undefined : onClose} fullWidth maxWidth="sm">
    <DialogTitle>{customer ? '회원 수정' : '회원 등록'}</DialogTitle>
    <DialogContent>
      <TextField label="이메일" type="email" fullWidth margin="normal" {...register('email')} error={!!errors.email} helperText={errors.email?.message} />
      <TextField label="이름" fullWidth margin="normal" {...register('name')} error={!!errors.name} helperText={errors.name?.message} />
      <TextField label="전화번호" fullWidth margin="normal" placeholder="01012345678" {...register('phone')} error={!!errors.phone} helperText={errors.phone?.message} />
      <Controller name="status" control={control} render={({ field }) => <TextField select label="상태" fullWidth margin="normal" {...field}><MenuItem value="ACTIVE">정상</MenuItem><MenuItem value="SUSPENDED">정지</MenuItem><MenuItem value="WITHDRAWN">탈퇴</MenuItem></TextField>} />
      {error && <Alert severity="error" sx={{ mt: 2 }}>{error}</Alert>}
    </DialogContent>
    <DialogActions><Button onClick={onClose} disabled={saving}>취소</Button><Button variant="contained" onClick={handleSubmit(onSave)} disabled={saving}>{saving ? '저장 중…' : '저장'}</Button></DialogActions>
  </Dialog>;
}


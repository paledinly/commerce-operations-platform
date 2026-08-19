import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { Alert, Box, Button, Card, CardContent, Container, TextField, Typography } from '@mui/material';
import axios from 'axios';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { login } from '../api/auth';
import { useAuthStore } from '../store/auth';

const schema = z.object({ email: z.email('올바른 이메일을 입력하세요.'), password: z.string().min(8, '비밀번호는 8자 이상이어야 합니다.') });
type FormValues = z.infer<typeof schema>;

export function LoginPage() {
  const saveLogin = useAuthStore((state) => state.login);
  const { register, handleSubmit, formState: { errors } } = useForm<FormValues>({ resolver: zodResolver(schema) });
  const mutation = useMutation({ mutationFn: ({ email, password }: FormValues) => login(email, password), onSuccess: (data) => saveLogin(data.accessToken, data.operator) });
  const message = mutation.isError
    ? axios.isAxiosError(mutation.error) && mutation.error.response?.status === 401 ? '이메일 또는 비밀번호가 올바르지 않습니다.' : '로그인 서버에 연결할 수 없습니다.'
    : null;

  return <Container maxWidth="sm">
    <Box sx={{ minHeight: '100vh', display: 'grid', placeItems: 'center' }}>
      <Card sx={{ width: '100%' }}><CardContent sx={{ p: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>Commerce Operations</Typography>
        <Typography color="text.secondary" sx={{ mb: 3 }}>운영자 계정으로 로그인하세요.</Typography>
        {message && <Alert severity="error" sx={{ mb: 2 }}>{message}</Alert>}
        <Box component="form" onSubmit={handleSubmit((values) => mutation.mutate(values))} noValidate>
          <TextField label="이메일" type="email" fullWidth margin="normal" autoComplete="username" {...register('email')} error={!!errors.email} helperText={errors.email?.message} />
          <TextField label="비밀번호" type="password" fullWidth margin="normal" autoComplete="current-password" {...register('password')} error={!!errors.password} helperText={errors.password?.message} />
          <Button type="submit" variant="contained" fullWidth size="large" disabled={mutation.isPending} sx={{ mt: 3 }}>{mutation.isPending ? '로그인 중…' : '로그인'}</Button>
        </Box>
      </CardContent></Card>
    </Box>
  </Container>;
}


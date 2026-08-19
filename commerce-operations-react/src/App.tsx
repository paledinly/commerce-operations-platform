import { Navigate, Route, Routes } from 'react-router-dom';
import { DashboardPage } from './pages/DashboardPage';
import { LoginPage } from './pages/LoginPage';
import { useAuthStore } from './store/auth';

function Protected({ children }: { children: React.ReactNode }) {
  const token = useAuthStore((state) => state.token);
  return token ? children : <Navigate to="/login" replace />;
}

export function App() {
  const token = useAuthStore((state) => state.token);
  return <Routes>
    <Route path="/login" element={token ? <Navigate to="/" replace /> : <LoginPage />} />
    <Route path="/" element={<Protected><DashboardPage /></Protected>} />
    <Route path="*" element={<Navigate to="/" replace />} />
  </Routes>;
}


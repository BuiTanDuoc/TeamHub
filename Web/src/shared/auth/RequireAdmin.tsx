import { Navigate, Outlet } from 'react-router-dom'
import { useAuthStore } from './authStore'

/** Chặn route chỉ dành cho SystemAdmin (backend vẫn kiểm tra, đây chỉ để không hiện trang lỗi 403). */
export function RequireAdmin() {
  const isAdmin = useAuthStore((s) => s.user?.isSystemAdmin ?? false)
  return isAdmin ? <Outlet /> : <Navigate to="/dashboard" replace />
}

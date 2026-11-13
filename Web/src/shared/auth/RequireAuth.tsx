import { CSpinner } from '@coreui/react'
import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuthStore } from './authStore'

/** Chặn các route cần đăng nhập; lưu lại đường dẫn hiện tại để quay về sau khi đăng nhập. */
export function RequireAuth() {
  const status = useAuthStore((s) => s.status)
  const location = useLocation()

  if (status === 'loading') {
    return (
      <div className="d-flex justify-content-center align-items-center min-vh-100">
        <CSpinner color="primary" variant="grow" />
      </div>
    )
  }
  if (status === 'anonymous') {
    return <Navigate to="/login" replace state={{ from: `${location.pathname}${location.search}` }} />
  }
  return <Outlet />
}

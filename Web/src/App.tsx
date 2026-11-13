import { CSpinner } from '@coreui/react'
import { lazy, Suspense, useEffect } from 'react'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import DefaultLayout from './layout/DefaultLayout'
import { useAuthStore } from './shared/auth/authStore'
import { RequireAuth } from './shared/auth/RequireAuth'
import { ToastHost } from './shared/components/ToastHost'

// Mỗi màn hình tách chunk riêng để tải ban đầu nhẹ
const LoginPage = lazy(() => import('./features/auth/LoginPage'))
const DashboardPage = lazy(() => import('./features/dashboard/DashboardPage'))
const TeamsPage = lazy(() => import('./features/teams/TeamsPage'))
const TeamDetailPage = lazy(() => import('./features/teams/TeamDetailPage'))
const ProjectPage = lazy(() => import('./features/projects/ProjectPage'))
const NotificationsPage = lazy(() => import('./features/notifications/NotificationsPage'))
const NotificationPreferencesPage = lazy(() => import('./features/notifications/NotificationPreferencesPage'))
const NotFoundPage = lazy(() => import('./pages/NotFoundPage'))

export default function App() {
  const init = useAuthStore((s) => s.init)
  useEffect(() => {
    void init()
  }, [init])

  return (
    <BrowserRouter>
      <Suspense
        fallback={
          <div className="pt-5 text-center">
            <CSpinner color="primary" variant="grow" />
          </div>
        }
      >
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<RequireAuth />}>
            <Route element={<DefaultLayout />}>
              <Route index element={<Navigate to="/dashboard" replace />} />
              <Route path="dashboard" element={<DashboardPage />} />
              <Route path="teams" element={<TeamsPage />} />
              <Route path="teams/:teamId" element={<TeamDetailPage />} />
              <Route path="teams/:teamId/projects/:projectId" element={<ProjectPage />} />
              <Route path="notifications" element={<NotificationsPage />} />
              <Route path="settings/notifications" element={<NotificationPreferencesPage />} />
              <Route path="*" element={<NotFoundPage />} />
            </Route>
          </Route>
        </Routes>
      </Suspense>
      <ToastHost />
    </BrowserRouter>
  )
}

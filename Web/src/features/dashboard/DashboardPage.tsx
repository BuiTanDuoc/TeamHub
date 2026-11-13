import { CCard, CCardBody, CCardHeader, CCol, CRow } from '@coreui/react'
import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { notificationsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { useAuthStore } from '@/shared/auth/authStore'
import { RoleBadge } from '@/shared/components/Badges'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { formatRelative } from '@/shared/utils/format'
import { notificationTarget } from '@/layout/NotificationBell'

export default function DashboardPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const user = useAuthStore((s) => s.user)
  const teams = useAuthStore((s) => s.teams)
  const unread = useQuery({ queryKey: qk.unreadCount, queryFn: notificationsApi.unreadCount })
  const latest = useQuery({ queryKey: qk.notificationList(true), queryFn: () => notificationsApi.list(true, 1, 8) })

  return (
    <>
      <PageHeader title={t('dashboard.welcome', { name: user?.fullName ?? '' })} subtitle={t('dashboard.subtitle')} />
      <CRow className="g-3">
        <CCol lg={6}>
          <CCard className="h-100">
            <CCardHeader className="fw-semibold">{t('dashboard.myTeams')}</CCardHeader>
            <CCardBody>
              {teams.length === 0 ? (
                <EmptyState title={t('dashboard.noTeams')} description={t('teams.emptyHint')} />
              ) : (
                <div className="d-flex flex-column gap-2">
                  {teams.map((m) => (
                    <Link key={m.teamId} to={`/teams/${m.teamId}`} className="d-flex justify-content-between align-items-center text-decoration-none border rounded p-2">
                      <span>
                        <span className="fw-semibold">{m.name}</span> <span className="small text-body-secondary">{m.code}</span>
                      </span>
                      <RoleBadge role={m.role} />
                    </Link>
                  ))}
                </div>
              )}
            </CCardBody>
          </CCard>
        </CCol>
        <CCol lg={6}>
          <CCard className="h-100">
            <CCardHeader className="d-flex justify-content-between">
              <span className="fw-semibold">{t('dashboard.unread', { count: unread.data?.count ?? 0 })}</span>
              <Link to="/notifications" className="small">{t('notifications.viewAll')}</Link>
            </CCardHeader>
            <CCardBody>
              {latest.data?.items.length === 0 && <EmptyState title={t('dashboard.allCaughtUp')} />}
              <div className="d-flex flex-column gap-2">
                {latest.data?.items.map((n) => (
                  <button key={n.id} type="button" className="btn btn-outline-secondary text-start" onClick={() => navigate(notificationTarget(n))}>
                    <div className="small">{n.message}</div>
                    <div className="small text-body-secondary">{formatRelative(n.createdAt)}</div>
                  </button>
                ))}
              </div>
            </CCardBody>
          </CCard>
        </CCol>
      </CRow>
    </>
  )
}

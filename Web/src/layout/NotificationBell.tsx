import { cilBell, cilCheckAlt } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CBadge, CButton, CDropdown, CDropdownDivider, CDropdownHeader, CDropdownMenu, CDropdownToggle, CSpinner } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { notificationsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { AppNotification } from '@/shared/api/types'
import { useApiError } from '@/shared/hooks/useApiError'
import { formatRelative } from '@/shared/utils/format'

export const notificationTarget = (n: AppNotification): string =>
  n.teamId && n.projectId
    ? `/teams/${n.teamId}/projects/${n.projectId}${n.workItemId ? `?task=${n.workItemId}` : ''}`
    : '/notifications'

export function NotificationBell() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showError } = useApiError()

  const unread = useQuery({ queryKey: qk.unreadCount, queryFn: notificationsApi.unreadCount, refetchInterval: 60_000 })
  const latest = useQuery({ queryKey: qk.notificationList(false), queryFn: () => notificationsApi.list(false, 1, 6) })

  const refresh = () => queryClient.invalidateQueries({ queryKey: qk.notifications })
  const markRead = useMutation({ mutationFn: notificationsApi.markRead, onSuccess: refresh, onError: showError })
  const markAll = useMutation({ mutationFn: notificationsApi.markAllRead, onSuccess: refresh, onError: showError })

  const count = unread.data?.count ?? 0

  const open = (n: AppNotification) => {
    if (!n.isRead) markRead.mutate(n.id)
    navigate(notificationTarget(n))
  }

  return (
    <CDropdown variant="nav-item" placement="bottom-end" autoClose="outside">
      <CDropdownToggle caret={false} className="position-relative" aria-label={t('nav.notifications')}>
        <CIcon icon={cilBell} size="lg" />
        {count > 0 && (
          <CBadge color="danger" position="top-end" shape="rounded-pill" className="mt-1">
            {count > 99 ? '99+' : count}
          </CBadge>
        )}
      </CDropdownToggle>
      <CDropdownMenu className="pt-0" style={{ minWidth: 340 }}>
        <CDropdownHeader className="bg-body-secondary fw-semibold py-2 d-flex align-items-center justify-content-between">
          {t('notifications.title')}
          <CButton color="link" size="sm" className="p-0" disabled={count === 0 || markAll.isPending} onClick={() => markAll.mutate()}>
            <CIcon icon={cilCheckAlt} className="me-1" />
            {t('notifications.markAllRead')}
          </CButton>
        </CDropdownHeader>

        {latest.isPending && (
          <div className="text-center py-3">
            <CSpinner size="sm" />
          </div>
        )}
        {latest.data?.items.length === 0 && <div className="text-center text-body-secondary py-3 small">{t('notifications.empty')}</div>}

        {latest.data?.items.map((n) => (
          <button
            key={n.id}
            type="button"
            className={`dropdown-item d-block text-wrap py-2 ${n.isRead ? '' : 'bg-primary-subtle'}`}
            onClick={() => open(n)}
          >
            <div className="small fw-semibold">{t(`enums.notificationType.${n.type}`)}</div>
            <div className="small">{n.message}</div>
            <div className="small text-body-secondary">{formatRelative(n.createdAt)}</div>
          </button>
        ))}

        <CDropdownDivider className="my-0" />
        <Link to="/notifications" className="dropdown-item text-center small py-2">
          {t('notifications.viewAll')}
        </Link>
      </CDropdownMenu>
    </CDropdown>
  )
}

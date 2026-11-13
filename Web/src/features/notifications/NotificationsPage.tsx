import { cilCheckAlt } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CBadge, CButton, CCard, CFormSwitch, CListGroup, CListGroupItem, CPagination, CPaginationItem } from '@coreui/react'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { notificationsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { AppNotification } from '@/shared/api/types'
import { NOTIFICATION_ICON_COLOR } from '@/shared/components/Badges'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { useApiError } from '@/shared/hooks/useApiError'
import { formatDateTime } from '@/shared/utils/format'
import { notificationTarget } from '@/layout/NotificationBell'

const PAGE_SIZE = 20

export default function NotificationsPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const [unreadOnly, setUnreadOnly] = useState(false)
  const [page, setPage] = useState(1)

  const query = useQuery({
    queryKey: [...qk.notificationList(unreadOnly), page],
    queryFn: () => notificationsApi.list(unreadOnly, page, PAGE_SIZE),
    placeholderData: keepPreviousData,
  })
  const refresh = () => queryClient.invalidateQueries({ queryKey: qk.notifications })
  const markRead = useMutation({ mutationFn: notificationsApi.markRead, onSuccess: refresh, onError: showError })
  const markAll = useMutation({ mutationFn: notificationsApi.markAllRead, onSuccess: refresh, onError: showError })

  const open = (n: AppNotification) => {
    if (!n.isRead) markRead.mutate(n.id)
    navigate(notificationTarget(n))
  }

  return (
    <>
      <PageHeader
        title={t('notifications.title')}
        actions={
          <>
            <CFormSwitch id="unread-only" label={t('notifications.unreadOnly')} checked={unreadOnly} onChange={(e) => { setUnreadOnly(e.target.checked); setPage(1) }} className="align-self-center" />
            <CButton color="secondary" variant="outline" disabled={markAll.isPending} onClick={() => markAll.mutate()}>
              <CIcon icon={cilCheckAlt} className="me-1" />
              {t('notifications.markAllRead')}
            </CButton>
          </>
        }
      />
      <QueryBoundary query={query}>
        {(data) => {
          const pages = Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE))
          return data.items.length === 0 ? (
            <EmptyState title={t('notifications.empty')} />
          ) : (
            <>
              <CCard>
                <CListGroup flush>
                  {data.items.map((n) => (
                    <CListGroupItem key={n.id} as="button" type="button" className={`d-flex gap-3 align-items-start ${n.isRead ? '' : 'bg-primary-subtle'}`} onClick={() => open(n)}>
                      <CBadge color={NOTIFICATION_ICON_COLOR[n.type]} className="mt-1">{t(`enums.notificationType.${n.type}`)}</CBadge>
                      <div className="flex-grow-1 text-start">
                        <div className={n.isRead ? '' : 'fw-semibold'}>{n.message}</div>
                        <div className="small text-body-secondary">{formatDateTime(n.createdAt)}</div>
                      </div>
                    </CListGroupItem>
                  ))}
                </CListGroup>
              </CCard>
              {pages > 1 && (
                <CPagination className="justify-content-center mt-3" aria-label="pagination">
                  <CPaginationItem as="button" disabled={page <= 1} onClick={() => setPage(page - 1)}>‹</CPaginationItem>
                  <CPaginationItem active>{page} / {pages}</CPaginationItem>
                  <CPaginationItem as="button" disabled={page >= pages} onClick={() => setPage(page + 1)}>›</CPaginationItem>
                </CPagination>
              )}
            </>
          )
        }}
      </QueryBoundary>
    </>
  )
}

import { cilCheckAlt, cilSettings } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CBadge, CButton, CCard, CFormSwitch, CListGroup, CListGroupItem, CSpinner } from '@coreui/react'
import { useInfiniteQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
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

  // Keyset pagination với cursor mờ: thông báo mới đẩy realtime lên đầu không làm trang sau bị lặp/sót
  const query = useInfiniteQuery({
    queryKey: qk.notificationList(unreadOnly),
    initialPageParam: undefined as string | undefined,
    queryFn: ({ pageParam }) => notificationsApi.list(unreadOnly, pageParam, PAGE_SIZE),
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  })
  const refresh = () => queryClient.invalidateQueries({ queryKey: qk.notifications })
  const markRead = useMutation({ mutationFn: notificationsApi.markRead, onSuccess: refresh, onError: showError })
  const markAll = useMutation({ mutationFn: notificationsApi.markAllRead, onSuccess: refresh, onError: showError })

  const items = query.data?.pages.flatMap((p) => p.items) ?? []

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
            <CFormSwitch id="unread-only" label={t('notifications.unreadOnly')} checked={unreadOnly} onChange={(e) => setUnreadOnly(e.target.checked)} className="align-self-center" />
            <CButton color="secondary" variant="outline" disabled={markAll.isPending} onClick={() => markAll.mutate()}>
              <CIcon icon={cilCheckAlt} className="me-1" />
              {t('notifications.markAllRead')}
            </CButton>
            <Link to="/settings/notifications">
              <CButton color="secondary" variant="outline">
                <CIcon icon={cilSettings} className="me-1" />
                {t('notifications.settings')}
              </CButton>
            </Link>
          </>
        }
      />
      <QueryBoundary query={query}>
        {() =>
          items.length === 0 ? (
            <EmptyState title={t('notifications.empty')} />
          ) : (
            <>
              <CCard>
                <CListGroup flush>
                  {items.map((n) => (
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
              {query.hasNextPage && (
                <div className="text-center mt-3">
                  <CButton color="secondary" variant="outline" disabled={query.isFetchingNextPage} onClick={() => void query.fetchNextPage()}>
                    {query.isFetchingNextPage && <CSpinner size="sm" className="me-2" />}
                    {t('common.loadMore')}
                  </CButton>
                </div>
              )}
            </>
          )
        }
      </QueryBoundary>
    </>
  )
}

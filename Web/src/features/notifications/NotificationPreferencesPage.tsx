import { CButton, CCard, CFormSwitch, CSpinner, CTable, CTableBody, CTableDataCell, CTableHead, CTableHeaderCell, CTableRow } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'
import { notificationsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { NotificationPreference } from '@/shared/api/types'
import { PageHeader } from '@/shared/components/PageHeader'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'

export default function NotificationPreferencesPage() {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const query = useQuery({ queryKey: qk.notificationPrefs, queryFn: notificationsApi.preferences })
  const [draft, setDraft] = useState<NotificationPreference[]>([])

  useEffect(() => {
    if (query.data) setDraft(query.data)
  }, [query.data])

  const save = useMutation({
    mutationFn: () => notificationsApi.savePreferences(draft),
    onSuccess: async () => {
      toast.success(t('common.saved'))
      await queryClient.invalidateQueries({ queryKey: qk.notificationPrefs })
    },
    onError: showError,
  })

  const toggle = (type: NotificationPreference['type'], field: 'inAppEnabled' | 'pushEnabled', value: boolean) =>
    setDraft((d) => d.map((p) => (p.type === type ? { ...p, [field]: value } : p)))

  const dirty = query.data ? JSON.stringify(query.data) !== JSON.stringify(draft) : false

  return (
    <>
      <div className="mb-2 small">
        <Link to="/notifications">← {t('notifications.title')}</Link>
      </div>
      <PageHeader title={t('notifications.settings')} subtitle={t('notifications.settingsHint')} />
      <QueryBoundary query={query}>
        {() => (
          <CCard>
            <CTable align="middle" responsive className="mb-0">
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>{t('notifications.eventType')}</CTableHeaderCell>
                  <CTableHeaderCell className="text-center">{t('notifications.inApp')}</CTableHeaderCell>
                  <CTableHeaderCell className="text-center">{t('notifications.push')}</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {draft.map((p) => (
                  <CTableRow key={p.type}>
                    <CTableDataCell>{t(`enums.notificationType.${p.type}`)}</CTableDataCell>
                    <CTableDataCell className="text-center">
                      <CFormSwitch className="d-inline-block" aria-label={`${p.type} ${t('notifications.inApp')}`} checked={p.inAppEnabled} onChange={(e) => toggle(p.type, 'inAppEnabled', e.target.checked)} />
                    </CTableDataCell>
                    <CTableDataCell className="text-center">
                      <CFormSwitch className="d-inline-block" aria-label={`${p.type} ${t('notifications.push')}`} checked={p.pushEnabled} onChange={(e) => toggle(p.type, 'pushEnabled', e.target.checked)} />
                    </CTableDataCell>
                  </CTableRow>
                ))}
              </CTableBody>
            </CTable>
            <div className="p-3 border-top d-flex justify-content-between align-items-center">
              <span className="small text-body-secondary">{t('notifications.bothOffHint')}</span>
              <CButton color="primary" disabled={!dirty || save.isPending} onClick={() => save.mutate()}>
                {save.isPending && <CSpinner size="sm" className="me-2" />}
                {t('common.save')}
              </CButton>
            </div>
          </CCard>
        )}
      </QueryBoundary>
    </>
  )
}

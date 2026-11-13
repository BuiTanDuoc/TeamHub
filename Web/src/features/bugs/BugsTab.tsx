import { cilPlus } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CButton, CCard, CCol, CFormInput, CFormSelect, CRow, CSpinner, CTable, CTableBody, CTableDataCell, CTableHead, CTableHeaderCell, CTableRow } from '@coreui/react'
import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { bugsApi, projectsApi, type BugFilter } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { BUG_SEVERITIES, BUG_STATUSES, type BugSeverity, type BugStatus } from '@/shared/api/types'
import { BugSeverityBadge, BugStatusBadge, PriorityBadge } from '@/shared/components/Badges'
import { EmptyState } from '@/shared/components/EmptyState'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useDebounce } from '@/shared/hooks/useDebounce'
import { formatDateTime } from '@/shared/utils/format'
import { isFinal } from './bugWorkflow'

const PAGE_SIZE = 25

interface Props {
  teamId: string
  projectId: string
  canContribute: boolean
  onOpen: (bugId: string) => void
  onCreate: () => void
}

export function BugsTab({ teamId, projectId, canContribute, onOpen, onCreate }: Props) {
  const { t } = useTranslation()
  const [status, setStatus] = useState<BugStatus | ''>('')
  const [severity, setSeverity] = useState<BugSeverity | ''>('')
  const [assigneeId, setAssigneeId] = useState('')
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebounce(search.trim(), 350)

  const filter: BugFilter = { pageSize: PAGE_SIZE }
  if (status) filter.status = status
  if (severity) filter.severity = severity
  if (assigneeId) filter.assigneeId = assigneeId
  if (debouncedSearch) filter.search = debouncedSearch

  const members = useQuery({ queryKey: qk.projectMembers(teamId, projectId), queryFn: () => projectsApi.members(teamId, projectId) })

  const query = useInfiniteQuery({
    queryKey: qk.bugs(teamId, projectId, filter),
    initialPageParam: undefined as number | undefined,
    queryFn: ({ pageParam }) => bugsApi.list(teamId, projectId, pageParam === undefined ? filter : { ...filter, cursor: pageParam }),
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  })
  const items = query.data?.pages.flatMap((p) => p.items) ?? []
  const now = Date.now()

  return (
    <>
      <CRow className="g-2 mb-3">
        <CCol md={4}>
          <CFormInput placeholder={t('bugs.searchPlaceholder')} value={search} maxLength={100} onChange={(e) => setSearch(e.target.value)} />
        </CCol>
        <CCol sm={4} md={2}>
          <CFormSelect value={status} aria-label={t('tasks.status')} onChange={(e) => setStatus(e.target.value as BugStatus | '')}>
            <option value="">{t('tasks.allStatuses')}</option>
            {BUG_STATUSES.map((s) => (
              <option key={s} value={s}>{t(`enums.bugStatus.${s}`)}</option>
            ))}
          </CFormSelect>
        </CCol>
        <CCol sm={4} md={2}>
          <CFormSelect value={severity} aria-label={t('bugs.severity')} onChange={(e) => setSeverity(e.target.value as BugSeverity | '')}>
            <option value="">{t('bugs.allSeverities')}</option>
            {BUG_SEVERITIES.map((s) => (
              <option key={s} value={s}>{t(`enums.bugSeverity.${s}`)}</option>
            ))}
          </CFormSelect>
        </CCol>
        <CCol sm={4} md={2}>
          <CFormSelect value={assigneeId} aria-label={t('tasks.assignee')} onChange={(e) => setAssigneeId(e.target.value)}>
            <option value="">{t('board.allAssignees')}</option>
            {members.data?.items.map((m) => (
              <option key={m.userId} value={m.userId}>{m.fullName}</option>
            ))}
          </CFormSelect>
        </CCol>
        {canContribute && (
          <CCol md={2} className="text-md-end">
            <CButton color="danger" onClick={onCreate}>
              <CIcon icon={cilPlus} className="me-1" />
              {t('bugs.create')}
            </CButton>
          </CCol>
        )}
      </CRow>

      <QueryBoundary query={query}>
        {() =>
          items.length === 0 ? (
            <EmptyState title={t('bugs.empty')} description={status || severity || assigneeId || debouncedSearch ? t('tasks.emptyFiltered') : undefined} />
          ) : (
            <CCard>
              <CTable align="middle" hover responsive className="mb-0">
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>{t('tasks.code')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.titleField')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('bugs.severity')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.priority')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.status')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.assignee')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('bugs.slaDue')}</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {items.map((b) => {
                    const overdue = !!b.slaDueAt && !isFinal(b.status) && new Date(b.slaDueAt).getTime() < now
                    return (
                      <CTableRow key={b.id} role="button" style={{ cursor: 'pointer' }} onClick={() => onOpen(b.id)}>
                        <CTableDataCell className="text-nowrap fw-semibold">{b.code}</CTableDataCell>
                        <CTableDataCell style={{ minWidth: 220 }}>{b.title}</CTableDataCell>
                        <CTableDataCell><BugSeverityBadge severity={b.severity} /></CTableDataCell>
                        <CTableDataCell><PriorityBadge priority={b.priority} /></CTableDataCell>
                        <CTableDataCell><BugStatusBadge status={b.status} /></CTableDataCell>
                        <CTableDataCell>
                          {b.assigneeName ? (
                            <span className="d-inline-flex align-items-center gap-2 text-nowrap">
                              <UserAvatar name={b.assigneeName} size="sm" />
                              {b.assigneeName}
                            </span>
                          ) : (
                            <span className="text-body-secondary">{t('tasks.unassigned')}</span>
                          )}
                        </CTableDataCell>
                        <CTableDataCell className={`text-nowrap ${overdue ? 'text-danger fw-semibold' : ''}`}>
                          {b.slaDueAt && !isFinal(b.status) ? formatDateTime(b.slaDueAt) : '—'}
                          {overdue && <div className="small">{t('bugs.overdue')}</div>}
                        </CTableDataCell>
                      </CTableRow>
                    )
                  })}
                </CTableBody>
              </CTable>
              {query.hasNextPage && (
                <div className="text-center p-3 border-top">
                  <CButton color="secondary" variant="outline" disabled={query.isFetchingNextPage} onClick={() => void query.fetchNextPage()}>
                    {query.isFetchingNextPage && <CSpinner size="sm" className="me-2" />}
                    {t('common.loadMore')}
                  </CButton>
                </div>
              )}
            </CCard>
          )
        }
      </QueryBoundary>
    </>
  )
}

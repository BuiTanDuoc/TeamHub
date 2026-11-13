import { cilPlus } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CButton, CCard, CCol, CFormInput, CFormSelect, CRow, CSpinner, CTable, CTableBody, CTableDataCell, CTableHead, CTableHeaderCell, CTableRow } from '@coreui/react'
import { useInfiniteQuery, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { projectsApi, tasksApi, type TaskFilter } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { WORK_ITEM_STATUSES, WORK_ITEM_TYPES, type WorkItemStatus, type WorkItemType } from '@/shared/api/types'
import { PriorityBadge, TaskStatusBadge, TypeBadge } from '@/shared/components/Badges'
import { EmptyState } from '@/shared/components/EmptyState'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useDebounce } from '@/shared/hooks/useDebounce'
import { formatDate } from '@/shared/utils/format'

const PAGE_SIZE = 25

interface Props {
  teamId: string
  projectId: string
  canContribute: boolean
  onOpen: (taskId: string) => void
  onCreate: () => void
}

export function TaskListView({ teamId, projectId, canContribute, onOpen, onCreate }: Props) {
  const { t } = useTranslation()
  const [status, setStatus] = useState<WorkItemStatus | ''>('')
  const [type, setType] = useState<WorkItemType | ''>('')
  const [assigneeId, setAssigneeId] = useState('')
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebounce(search.trim(), 350)

  const filter: TaskFilter = { pageSize: PAGE_SIZE }
  if (status) filter.status = status
  if (type) filter.type = type
  if (assigneeId) filter.assigneeId = assigneeId
  if (debouncedSearch) filter.search = debouncedSearch

  const members = useQuery({ queryKey: qk.projectMembers(teamId, projectId), queryFn: () => projectsApi.members(teamId, projectId) })

  // Keyset pagination theo Number: nextCursor của trang trước truyền vào ?cursor= của trang sau
  const query = useInfiniteQuery({
    queryKey: qk.tasks(teamId, projectId, filter),
    initialPageParam: undefined as number | undefined,
    queryFn: ({ pageParam }) => tasksApi.list(teamId, projectId, pageParam === undefined ? filter : { ...filter, cursor: pageParam }),
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  })

  const items = query.data?.pages.flatMap((p) => p.items) ?? []

  return (
    <>
      <CRow className="g-2 mb-3">
        <CCol md={4}>
          <CFormInput placeholder={t('tasks.searchPlaceholder')} value={search} maxLength={100} onChange={(e) => setSearch(e.target.value)} />
        </CCol>
        <CCol sm={4} md={2}>
          <CFormSelect value={status} aria-label={t('tasks.status')} onChange={(e) => setStatus(e.target.value as WorkItemStatus | '')}>
            <option value="">{t('tasks.allStatuses')}</option>
            {WORK_ITEM_STATUSES.map((s) => (
              <option key={s} value={s}>{t(`enums.taskStatus.${s}`)}</option>
            ))}
          </CFormSelect>
        </CCol>
        <CCol sm={4} md={2}>
          <CFormSelect value={type} aria-label={t('tasks.type')} onChange={(e) => setType(e.target.value as WorkItemType | '')}>
            <option value="">{t('tasks.allTypes')}</option>
            {WORK_ITEM_TYPES.map((s) => (
              <option key={s} value={s}>{t(`enums.taskType.${s}`)}</option>
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
            <CButton color="primary" onClick={onCreate}>
              <CIcon icon={cilPlus} className="me-1" />
              {t('tasks.create')}
            </CButton>
          </CCol>
        )}
      </CRow>

      <QueryBoundary query={query}>
        {() =>
          items.length === 0 ? (
            <EmptyState title={t('tasks.empty')} description={status || type || assigneeId || debouncedSearch ? t('tasks.emptyFiltered') : undefined} />
          ) : (
            <CCard>
              <CTable align="middle" hover responsive className="mb-0">
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>{t('tasks.code')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.titleField')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.type')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.status')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.priority')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.assignee')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('tasks.dueDate')}</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {items.map((task) => (
                    <CTableRow key={task.id} role="button" style={{ cursor: 'pointer' }} onClick={() => onOpen(task.id)}>
                      <CTableDataCell className="text-nowrap fw-semibold">{task.code}</CTableDataCell>
                      <CTableDataCell style={{ minWidth: 220 }}>{task.title}</CTableDataCell>
                      <CTableDataCell><TypeBadge type={task.type} /></CTableDataCell>
                      <CTableDataCell><TaskStatusBadge status={task.status} /></CTableDataCell>
                      <CTableDataCell><PriorityBadge priority={task.priority} /></CTableDataCell>
                      <CTableDataCell>
                        {task.assigneeName ? (
                          <span className="d-inline-flex align-items-center gap-2 text-nowrap">
                            <UserAvatar name={task.assigneeName} size="sm" />
                            {task.assigneeName}
                          </span>
                        ) : (
                          <span className="text-body-secondary">{t('tasks.unassigned')}</span>
                        )}
                      </CTableDataCell>
                      <CTableDataCell className="text-nowrap">{formatDate(task.dueDate)}</CTableDataCell>
                    </CTableRow>
                  ))}
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

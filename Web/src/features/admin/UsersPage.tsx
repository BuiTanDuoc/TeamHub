import { cilPlus } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CBadge, CButton, CCard, CFormInput, CFormSelect, CPagination, CPaginationItem, CTable, CTableBody, CTableDataCell, CTableHead, CTableHeaderCell, CTableRow } from '@coreui/react'
import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router-dom'
import { usersApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { useAuthStore } from '@/shared/auth/authStore'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useDebounce } from '@/shared/hooks/useDebounce'
import { UserDetailModal } from './UserDetailModal'
import { UserFormModal } from './UserFormModal'

const PAGE_SIZE = 20
type StatusFilter = '' | 'active' | 'inactive'

export default function UsersPage() {
  const { t } = useTranslation()
  const me = useAuthStore((s) => s.user)
  const [params, setParams] = useSearchParams()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<StatusFilter>('')
  const [page, setPage] = useState(1)
  const [creating, setCreating] = useState(false)
  const debounced = useDebounce(search.trim(), 350)
  const selectedId = params.get('user')

  const query = useQuery({
    queryKey: qk.adminUsers(debounced, status, page),
    queryFn: () => usersApi.search(debounced, { page, pageSize: PAGE_SIZE, ...(status ? { isActive: status === 'active' } : {}) }),
    placeholderData: keepPreviousData,
  })

  const open = (id: string | null) => {
    const next = new URLSearchParams(params)
    if (id) next.set('user', id)
    else next.delete('user')
    setParams(next, { replace: true })
  }

  return (
    <>
      <PageHeader
        title={t('users.title')}
        subtitle={t('users.subtitle')}
        actions={
          <CButton color="primary" onClick={() => setCreating(true)}>
            <CIcon icon={cilPlus} className="me-1" />
            {t('users.create')}
          </CButton>
        }
      />

      <div className="d-flex flex-wrap gap-2 mb-3">
        <CFormInput style={{ maxWidth: 320 }} placeholder={t('users.searchPlaceholder')} value={search} onChange={(e) => { setSearch(e.target.value); setPage(1) }} />
        <CFormSelect style={{ width: 200 }} value={status} aria-label={t('users.status')} onChange={(e) => { setStatus(e.target.value as StatusFilter); setPage(1) }}>
          <option value="">{t('users.allStatuses')}</option>
          <option value="active">{t('users.active')}</option>
          <option value="inactive">{t('users.inactive')}</option>
        </CFormSelect>
      </div>

      <QueryBoundary query={query}>
        {(data) => {
          const pages = Math.max(1, Math.ceil(data.totalCount / PAGE_SIZE))
          return data.items.length === 0 ? (
            <EmptyState title={t('users.empty')} />
          ) : (
            <>
              <CCard>
                <CTable align="middle" hover responsive className="mb-0">
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>{t('members.member')}</CTableHeaderCell>
                      <CTableHeaderCell>{t('users.status')}</CTableHeaderCell>
                      <CTableHeaderCell>{t('users.systemRole')}</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {data.items.map((u) => (
                      <CTableRow key={u.id} role="button" style={{ cursor: 'pointer' }} className={u.isActive ? '' : 'text-body-secondary'} onClick={() => open(u.id)}>
                        <CTableDataCell>
                          <div className="d-flex align-items-center gap-2">
                            <UserAvatar name={u.fullName} src={u.avatarUrl} size="sm" />
                            <div>
                              <div className="fw-semibold">{u.fullName} {u.id === me?.id && <span className="small text-body-secondary">({t('common.you')})</span>}</div>
                              <div className="small text-body-secondary">{u.email}</div>
                            </div>
                          </div>
                        </CTableDataCell>
                        <CTableDataCell>
                          <CBadge color={u.isActive ? 'success' : 'secondary'}>{u.isActive ? t('users.active') : t('users.inactive')}</CBadge>
                        </CTableDataCell>
                        <CTableDataCell>{u.isSystemAdmin ? <CBadge color="danger">{t('roles.systemAdmin')}</CBadge> : <span className="text-body-secondary">—</span>}</CTableDataCell>
                      </CTableRow>
                    ))}
                  </CTableBody>
                </CTable>
              </CCard>
              <div className="d-flex justify-content-between align-items-center mt-3">
                <span className="small text-body-secondary">{t('users.total', { count: data.totalCount })}</span>
                {pages > 1 && (
                  <CPagination className="mb-0" aria-label="pagination">
                    <CPaginationItem as="button" disabled={page <= 1} onClick={() => setPage(page - 1)}>‹</CPaginationItem>
                    <CPaginationItem active>{page} / {pages}</CPaginationItem>
                    <CPaginationItem as="button" disabled={page >= pages} onClick={() => setPage(page + 1)}>›</CPaginationItem>
                  </CPagination>
                )}
              </div>
            </>
          )
        }}
      </QueryBoundary>

      {selectedId && <UserDetailModal key={selectedId} userId={selectedId} onClose={() => open(null)} />}
      <UserFormModal visible={creating} onClose={() => setCreating(false)} onCreated={(id) => open(id)} />
    </>
  )
}

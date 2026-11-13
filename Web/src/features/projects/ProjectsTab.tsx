import { cilPlus } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CButton, CCard, CFormSelect, CTable, CTableBody, CTableDataCell, CTableHead, CTableHeaderCell, CTableRow } from '@coreui/react'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { projectsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { PROJECT_STATUSES, type ProjectStatus } from '@/shared/api/types'
import { ProjectStatusBadge } from '@/shared/components/Badges'
import { EmptyState } from '@/shared/components/EmptyState'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { formatDate } from '@/shared/utils/format'
import { ProjectFormModal } from './ProjectFormModal'

export function ProjectsTab({ teamId, canManage }: { teamId: string; canManage: boolean }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const [status, setStatus] = useState<ProjectStatus | ''>('')
  const [creating, setCreating] = useState(false)

  const query = useQuery({
    queryKey: [...qk.projects(teamId), 'list', status],
    queryFn: () => projectsApi.list(teamId, status || undefined),
  })

  return (
    <>
      <div className="d-flex flex-wrap justify-content-between gap-2 mb-3">
        <CFormSelect style={{ width: 200 }} value={status} aria-label={t('projects.status')} onChange={(e) => setStatus(e.target.value as ProjectStatus | '')}>
          <option value="">{t('projects.allStatuses')}</option>
          {PROJECT_STATUSES.map((s) => (
            <option key={s} value={s}>
              {t(`enums.projectStatus.${s}`)}
            </option>
          ))}
        </CFormSelect>
        {canManage && (
          <CButton color="primary" onClick={() => setCreating(true)}>
            <CIcon icon={cilPlus} className="me-1" />
            {t('projects.create')}
          </CButton>
        )}
      </div>

      <QueryBoundary query={query}>
        {(data) =>
          data.items.length === 0 ? (
            <EmptyState title={t('projects.empty')} description={canManage ? t('projects.emptyManageHint') : undefined} />
          ) : (
            <CCard>
              <CTable align="middle" hover responsive className="mb-0">
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>{t('projects.name')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('projects.key')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('projects.status')}</CTableHeaderCell>
                    <CTableHeaderCell>{t('projects.timeline')}</CTableHeaderCell>
                    <CTableHeaderCell className="text-end">{t('projects.members')}</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {data.items.map((p) => (
                    <CTableRow key={p.id} role="link" style={{ cursor: 'pointer' }} onClick={() => navigate(`/teams/${teamId}/projects/${p.id}`)}>
                      <CTableDataCell>
                        <Link to={`/teams/${teamId}/projects/${p.id}`} className="fw-semibold text-decoration-none" onClick={(e) => e.stopPropagation()}>
                          {p.name}
                        </Link>
                        {p.description && <div className="small text-body-secondary text-truncate" style={{ maxWidth: 420 }}>{p.description}</div>}
                      </CTableDataCell>
                      <CTableDataCell>
                        <code>{p.key}</code>
                      </CTableDataCell>
                      <CTableDataCell>
                        <ProjectStatusBadge status={p.status} />
                      </CTableDataCell>
                      <CTableDataCell className="text-body-secondary">
                        {p.startDate || p.endDate ? `${formatDate(p.startDate)} → ${formatDate(p.endDate)}` : '—'}
                      </CTableDataCell>
                      <CTableDataCell className="text-end">{p.memberCount}</CTableDataCell>
                    </CTableRow>
                  ))}
                </CTableBody>
              </CTable>
            </CCard>
          )
        }
      </QueryBoundary>

      <ProjectFormModal teamId={teamId} visible={creating} onClose={() => setCreating(false)} onCreated={(id) => navigate(`/teams/${teamId}/projects/${id}`)} />
    </>
  )
}

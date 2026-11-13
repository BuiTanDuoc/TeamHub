import { cilPlus } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CBadge, CButton, CCard, CCardBody, CCol, CRow } from '@coreui/react'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { teamsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { useAuthStore } from '@/shared/auth/authStore'
import { RoleBadge } from '@/shared/components/Badges'
import { EmptyState } from '@/shared/components/EmptyState'
import { PageHeader } from '@/shared/components/PageHeader'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { CreateTeamModal } from './TeamFormModals'

export default function TeamsPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const isAdmin = useAuthStore((s) => s.user?.isSystemAdmin ?? false)
  const myTeams = useAuthStore((s) => s.teams)
  const [creating, setCreating] = useState(false)
  const query = useQuery({ queryKey: qk.teams, queryFn: () => teamsApi.list(1, 100) })

  return (
    <>
      <PageHeader
        title={t('teams.title')}
        subtitle={t('teams.subtitle')}
        actions={
          isAdmin && (
            <CButton color="primary" onClick={() => setCreating(true)}>
              <CIcon icon={cilPlus} className="me-1" />
              {t('teams.create')}
            </CButton>
          )
        }
      />

      <QueryBoundary query={query}>
        {(data) =>
          data.items.length === 0 ? (
            <EmptyState title={t('teams.empty')} description={isAdmin ? t('teams.emptyAdminHint') : t('teams.emptyHint')} />
          ) : (
            <CRow className="g-3">
              {data.items.map((team) => {
                const role = myTeams.find((m) => m.teamId === team.id)?.role
                return (
                  <CCol key={team.id} md={6} xl={4}>
                    <CCard className="h-100">
                      <CCardBody className="d-flex flex-column">
                        <div className="d-flex justify-content-between align-items-start mb-2">
                          <div>
                            <Link to={`/teams/${team.id}`} className="h5 text-decoration-none stretched-link">
                              {team.name}
                            </Link>
                            <div className="small text-body-secondary">{team.code}</div>
                          </div>
                          <div className="d-flex flex-column align-items-end gap-1">
                            {role && <RoleBadge role={role} />}
                            {!team.isActive && <CBadge color="secondary">{t('teams.inactive')}</CBadge>}
                          </div>
                        </div>
                        <p className="text-body-secondary flex-grow-1 mb-2">{team.description || t('common.noDescription')}</p>
                        <div className="small text-body-secondary">{t('teams.memberCount', { count: team.memberCount })}</div>
                      </CCardBody>
                    </CCard>
                  </CCol>
                )
              })}
            </CRow>
          )
        }
      </QueryBoundary>

      <CreateTeamModal visible={creating} onClose={() => setCreating(false)} onCreated={(id) => navigate(`/teams/${id}`)} />
    </>
  )
}

import { cilPencil } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CBadge, CButton } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { teamsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { useAuthStore } from '@/shared/auth/authStore'
import { useTeamPermissions } from '@/shared/auth/permissions'
import { RoleBadge } from '@/shared/components/Badges'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { PageHeader } from '@/shared/components/PageHeader'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { TabNav } from '@/shared/components/TabNav'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { ProjectsTab } from '../projects/ProjectsTab'
import { EditTeamModal } from './TeamFormModals'
import { TeamMembersTab } from './TeamMembersTab'

type Tab = 'projects' | 'members'

export default function TeamDetailPage() {
  const { teamId = '' } = useParams()
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const reloadMe = useAuthStore((s) => s.reloadMe)
  const perms = useTeamPermissions(teamId)
  const [params, setParams] = useSearchParams()
  const [editing, setEditing] = useState(false)
  const [leaving, setLeaving] = useState(false)

  const tab: Tab = params.get('tab') === 'members' ? 'members' : 'projects'
  const query = useQuery({ queryKey: qk.team(teamId), queryFn: () => teamsApi.get(teamId) })

  const setActive = useMutation({
    mutationFn: (isActive: boolean) => teamsApi.setActive(teamId, isActive),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: qk.teams })
    },
    onError: showError,
  })

  const leave = useMutation({
    mutationFn: () => teamsApi.leave(teamId),
    onSuccess: async () => {
      toast.success(t('teams.left'))
      queryClient.removeQueries({ queryKey: qk.team(teamId) })
      await reloadMe()
      navigate('/teams', { replace: true })
    },
    onError: (e) => {
      setLeaving(false)
      showError(e)
    },
  })

  return (
    <QueryBoundary query={query}>
      {(team) => (
        <>
          <div className="mb-2 small">
            <Link to="/teams">← {t('teams.title')}</Link>
          </div>
          <PageHeader
            title={
              <span className="d-flex flex-wrap align-items-center gap-2">
                {team.name} <code className="fs-6">{team.code}</code>
                {perms.role && <RoleBadge role={perms.role} />}
                {!team.isActive && <CBadge color="secondary">{t('teams.inactive')}</CBadge>}
              </span>
            }
            subtitle={team.description || t('common.noDescription')}
            actions={
              <>
                {perms.canManage && (
                  <CButton color="secondary" variant="outline" onClick={() => setEditing(true)}>
                    <CIcon icon={cilPencil} className="me-1" />
                    {t('common.edit')}
                  </CButton>
                )}
                {perms.isSystemAdmin && (
                  <CButton color={team.isActive ? 'warning' : 'success'} variant="outline" disabled={setActive.isPending} onClick={() => setActive.mutate(!team.isActive)}>
                    {team.isActive ? t('teams.deactivate') : t('teams.activate')}
                  </CButton>
                )}
                {perms.role && (
                  <CButton color="danger" variant="outline" onClick={() => setLeaving(true)}>
                    {t('teams.leave')}
                  </CButton>
                )}
              </>
            }
          />

          <TabNav
            active={tab}
            onChange={(k) => setParams(k === 'projects' ? {} : { tab: k }, { replace: true })}
            tabs={[
              { key: 'projects', label: t('teams.tabProjects') },
              { key: 'members', label: `${t('teams.tabMembers')} (${team.memberCount})` },
            ]}
          />
          {tab === 'projects' ? <ProjectsTab teamId={teamId} canManage={perms.canManage} /> : <TeamMembersTab teamId={teamId} canManage={perms.canManage} />}

          <EditTeamModal team={team} visible={editing} onClose={() => setEditing(false)} />
          <ConfirmModal visible={leaving} title={t('teams.leave')} message={t('teams.leaveConfirm', { name: team.name })} confirmLabel={t('teams.leave')} busy={leave.isPending} onClose={() => setLeaving(false)} onConfirm={() => leave.mutate()} />
        </>
      )}
    </QueryBoundary>
  )
}

import { cilPencil, cilPlus, cilTrash } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CButton, CFormSelect } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { projectsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { PROJECT_STATUSES, type ProjectStatus } from '@/shared/api/types'
import { useTeamPermissions } from '@/shared/auth/permissions'
import { ProjectStatusBadge } from '@/shared/components/Badges'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { PageHeader } from '@/shared/components/PageHeader'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { TabNav } from '@/shared/components/TabNav'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { formatDate } from '@/shared/utils/format'
import { BoardView } from '../tasks/BoardView'
import { TaskDetailModal } from '../tasks/TaskDetailModal'
import { TaskFormModal } from '../tasks/TaskFormModal'
import { TaskListView } from '../tasks/TaskListView'
import { ProjectFormModal } from './ProjectFormModal'
import { ProjectMembersTab } from './ProjectMembersTab'

type Tab = 'board' | 'list' | 'members'

export default function ProjectPage() {
  const { teamId = '', projectId = '' } = useParams()
  const { t } = useTranslation()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const perms = useTeamPermissions(teamId)
  const [params, setParams] = useSearchParams()
  const [editing, setEditing] = useState(false)
  const [creatingTask, setCreatingTask] = useState(false)
  const [deleting, setDeleting] = useState(false)

  const tabParam = params.get('tab')
  const tab: Tab = tabParam === 'list' || tabParam === 'members' ? tabParam : 'board'
  const taskId = params.get('task')

  const query = useQuery({ queryKey: qk.project(teamId, projectId), queryFn: () => projectsApi.get(teamId, projectId) })

  const update = (changes: Record<string, string | null>) => {
    const next = new URLSearchParams(params)
    for (const [k, v] of Object.entries(changes)) {
      if (v === null) next.delete(k)
      else next.set(k, v)
    }
    setParams(next, { replace: true })
  }
  const openTask = (id: string) => update({ task: id })

  const changeStatus = useMutation({
    mutationFn: (status: ProjectStatus) => projectsApi.changeStatus(teamId, projectId, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: qk.projects(teamId) }),
    onError: showError,
  })

  const remove = useMutation({
    mutationFn: () => projectsApi.remove(teamId, projectId),
    onSuccess: async () => {
      toast.success(t('projects.deleted'))
      queryClient.removeQueries({ queryKey: qk.project(teamId, projectId) })
      await queryClient.invalidateQueries({ queryKey: qk.projects(teamId) })
      navigate(`/teams/${teamId}?tab=projects`, { replace: true })
    },
    onError: (e) => {
      setDeleting(false)
      showError(e)
    },
  })

  return (
    <QueryBoundary query={query}>
      {(project) => (
        <>
          <div className="mb-2 small">
            <Link to={`/teams/${teamId}`}>← {t('projects.backToTeam')}</Link>
          </div>
          <PageHeader
            title={
              <span className="d-flex flex-wrap align-items-center gap-2">
                {project.name} <code className="fs-6">{project.key}</code>
                <ProjectStatusBadge status={project.status} />
              </span>
            }
            subtitle={
              <>
                {project.description && <div>{project.description}</div>}
                {(project.startDate || project.endDate) && (
                  <div className="small">
                    {formatDate(project.startDate)} → {formatDate(project.endDate)}
                  </div>
                )}
              </>
            }
            actions={
              <>
                {perms.canContribute && (
                  <CButton color="primary" onClick={() => setCreatingTask(true)}>
                    <CIcon icon={cilPlus} className="me-1" />
                    {t('tasks.create')}
                  </CButton>
                )}
                {perms.canManage && (
                  <>
                    <CFormSelect style={{ width: 160 }} value={project.status} disabled={changeStatus.isPending} aria-label={t('projects.status')} onChange={(e) => changeStatus.mutate(e.target.value as ProjectStatus)}>
                      {PROJECT_STATUSES.map((s) => (
                        <option key={s} value={s}>{t(`enums.projectStatus.${s}`)}</option>
                      ))}
                    </CFormSelect>
                    <CButton color="secondary" variant="outline" onClick={() => setEditing(true)} title={t('common.edit')}>
                      <CIcon icon={cilPencil} />
                    </CButton>
                    <CButton color="danger" variant="outline" onClick={() => setDeleting(true)} title={t('common.delete')}>
                      <CIcon icon={cilTrash} />
                    </CButton>
                  </>
                )}
              </>
            }
          />

          <TabNav
            active={tab}
            onChange={(k) => update({ tab: k === 'board' ? null : k })}
            tabs={[
              { key: 'board', label: t('projects.tabBoard') },
              { key: 'list', label: t('projects.tabList') },
              { key: 'members', label: `${t('projects.tabMembers')} (${project.memberCount})` },
            ]}
          />

          {tab === 'board' && <BoardView teamId={teamId} projectId={projectId} canContribute={perms.canContribute} onOpen={openTask} />}
          {tab === 'list' && <TaskListView teamId={teamId} projectId={projectId} canContribute={perms.canContribute} onOpen={openTask} onCreate={() => setCreatingTask(true)} />}
          {tab === 'members' && <ProjectMembersTab teamId={teamId} projectId={projectId} canManage={perms.canManage} />}

          {taskId && (
            <TaskDetailModal key={taskId} teamId={teamId} projectId={projectId} taskId={taskId} canContribute={perms.canContribute} canManage={perms.canManage} onClose={() => update({ task: null })} />
          )}
          <TaskFormModal teamId={teamId} projectId={projectId} visible={creatingTask} onClose={() => setCreatingTask(false)} onCreated={openTask} />
          <ProjectFormModal teamId={teamId} project={project} visible={editing} onClose={() => setEditing(false)} />
          <ConfirmModal visible={deleting} title={t('projects.delete')} message={t('projects.deleteConfirm', { name: project.name })} confirmLabel={t('common.delete')} busy={remove.isPending} onClose={() => setDeleting(false)} onConfirm={() => remove.mutate()} />
        </>
      )}
    </QueryBoundary>
  )
}

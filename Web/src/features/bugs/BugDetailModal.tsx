import { cilPencil, cilTrash } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CButton, CCard, CCardBody, CCol, CFormSelect, CModal, CModalBody, CModalHeader, CModalTitle, CRow } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { bugsApi, projectsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { BugStatus } from '@/shared/api/types'
import { BUG_STATUS_COLOR, BugSeverityBadge, BugStatusBadge, PriorityBadge } from '@/shared/components/Badges'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { Field } from '@/shared/components/Field'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { formatDateTime } from '@/shared/utils/format'
import { BugFormModal } from './BugFormModal'
import { BugHistory } from './BugHistory'
import { BugStatusDialog } from './BugStatusDialog'
import { isFinal } from './bugWorkflow'

interface Props {
  teamId: string
  projectId: string
  bugId: string
  canContribute: boolean
  /** Developer trở lên được giao bug (BugWorkflow/Bug.Assign kiểm tra thêm ở backend). */
  canAssign: boolean
  canManage: boolean
  onClose: () => void
}

const Block = ({ title, children }: { title: string; children: string }) => (
  <div className="mb-3">
    <h4 className="h6 text-body-secondary">{title}</h4>
    <div className="task-description">{children}</div>
  </div>
)

export function BugDetailModal({ teamId, projectId, bugId, canContribute, canAssign, canManage, onClose }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const [editing, setEditing] = useState(false)
  const [target, setTarget] = useState<BugStatus | null>(null)
  const [confirmDelete, setConfirmDelete] = useState(false)

  const key = qk.bug(teamId, projectId, bugId)
  const query = useQuery({ queryKey: key, queryFn: () => bugsApi.get(teamId, projectId, bugId) })
  const members = useQuery({ queryKey: qk.projectMembers(teamId, projectId), queryFn: () => projectsApi.members(teamId, projectId) })
  const refresh = () => queryClient.invalidateQueries({ queryKey: qk.project(teamId, projectId) })

  const assign = useMutation({
    mutationFn: (assigneeId: string) => bugsApi.assign(teamId, projectId, bugId, assigneeId),
    onSuccess: refresh,
    onError: async (e) => {
      showError(e)
      await refresh()
    },
  })

  const remove = useMutation({
    mutationFn: () => bugsApi.remove(teamId, projectId, bugId),
    onSuccess: async () => {
      toast.success(t('bugs.deleted'))
      setConfirmDelete(false)
      queryClient.removeQueries({ queryKey: key })
      await refresh()
      onClose()
    },
    onError: (e) => {
      setConfirmDelete(false)
      showError(e)
    },
  })

  return (
    <CModal visible onClose={onClose} size="xl" scrollable alignment="top">
      <CModalHeader>
        <CModalTitle className="d-flex flex-wrap align-items-center gap-2">
          {query.data ? (
            <>
              <span className="text-body-secondary">{query.data.code}</span>
              <BugSeverityBadge severity={query.data.severity} />
              <BugStatusBadge status={query.data.status} />
            </>
          ) : (
            t('bugs.detail')
          )}
        </CModalTitle>
      </CModalHeader>
      <CModalBody>
        <QueryBoundary query={query}>
          {(bug) => {
            const overdue = !!bug.slaDueAt && !isFinal(bug.status) && new Date(bug.slaDueAt).getTime() < Date.now()
            return (
              <>
                <CRow className="g-4">
                  <CCol lg={8}>
                    <div className="d-flex justify-content-between align-items-start gap-2 mb-3">
                      <h3 className="h4 mb-0" style={{ wordBreak: 'break-word' }}>{bug.title}</h3>
                      {canContribute && (
                        <CButton color="secondary" variant="outline" size="sm" className="flex-shrink-0" onClick={() => setEditing(true)}>
                          <CIcon icon={cilPencil} className="me-1" />
                          {t('common.edit')}
                        </CButton>
                      )}
                    </div>
                    <Block title={t('bugs.steps')}>{bug.stepsToReproduce}</Block>
                    <CRow>
                      <CCol md={6}><Block title={t('bugs.expected')}>{bug.expectedResult}</Block></CCol>
                      <CCol md={6}><Block title={t('bugs.actual')}>{bug.actualResult}</Block></CCol>
                    </CRow>
                    {bug.deviceInfo && <Block title={t('bugs.deviceInfo')}>{bug.deviceInfo}</Block>}
                  </CCol>

                  <CCol lg={4}>
                    {/* Chỉ hiện các bước chuyển hợp lệ cho đúng vai trò của người xem (backend tính sẵn) */}
                    <CCard className="mb-3">
                      <CCardBody>
                        <h3 className="h6">{t('bugs.actions')}</h3>
                        {canContribute && bug.allowedTransitions.length > 0 ? (
                          <div className="d-flex flex-wrap gap-2">
                            {bug.allowedTransitions.map((s) => (
                              <CButton key={s} size="sm" color={BUG_STATUS_COLOR[s]} variant="outline" onClick={() => setTarget(s)}>
                                → {t(`enums.bugStatus.${s}`)}
                              </CButton>
                            ))}
                          </div>
                        ) : (
                          <p className="small text-body-secondary mb-0">{t('bugs.noActions')}</p>
                        )}
                      </CCardBody>
                    </CCard>

                    <CCard className="mb-3">
                      <CCardBody>
                        {canAssign && (
                          <Field label={t('tasks.assignee')} htmlFor="bug-assignee" className="mb-3">
                            <CFormSelect id="bug-assignee" value={bug.assigneeId ?? ''} disabled={assign.isPending} onChange={(e) => e.target.value && assign.mutate(e.target.value)}>
                              <option value="" disabled>{t('tasks.unassigned')}</option>
                              {bug.assigneeId && !members.data?.items.some((m) => m.userId === bug.assigneeId) && (
                                <option value={bug.assigneeId}>{bug.assigneeName}</option>
                              )}
                              {members.data?.items.map((m) => (
                                <option key={m.userId} value={m.userId}>{m.fullName}</option>
                              ))}
                            </CFormSelect>
                          </Field>
                        )}
                        <dl className="row small mb-0">
                          {!canAssign && (
                            <>
                              <dt className="col-5 text-body-secondary fw-normal">{t('tasks.assignee')}</dt>
                              <dd className="col-7">{bug.assigneeName ?? t('tasks.unassigned')}</dd>
                            </>
                          )}
                          <dt className="col-5 text-body-secondary fw-normal">{t('tasks.priority')}</dt>
                          <dd className="col-7"><PriorityBadge priority={bug.priority} /></dd>
                          <dt className="col-5 text-body-secondary fw-normal">{t('bugs.environment')}</dt>
                          <dd className="col-7">{t(`enums.bugEnvironment.${bug.environment}`)}</dd>
                          <dt className="col-5 text-body-secondary fw-normal">{t('bugs.affectedVersion')}</dt>
                          <dd className="col-7">{bug.affectedVersion ?? '—'}</dd>
                          <dt className="col-5 text-body-secondary fw-normal">{t('bugs.fixedInVersion')}</dt>
                          <dd className="col-7">{bug.fixedInVersion ?? '—'}</dd>
                          <dt className="col-5 text-body-secondary fw-normal">{t('bugs.slaDue')}</dt>
                          <dd className={`col-7 ${overdue ? 'text-danger fw-semibold' : ''}`}>
                            {bug.slaDueAt && !isFinal(bug.status) ? formatDateTime(bug.slaDueAt) : '—'}
                            {overdue && ` (${t('bugs.overdue')})`}
                          </dd>
                          <dt className="col-5 text-body-secondary fw-normal">{t('bugs.reopenCount')}</dt>
                          <dd className="col-7">{bug.reopenCount}</dd>
                          <dt className="col-5 text-body-secondary fw-normal">{t('tasks.reporter')}</dt>
                          <dd className="col-7">
                            <span className="d-inline-flex align-items-center gap-1">
                              <UserAvatar name={bug.reporterName} size="sm" />
                              {bug.reporterName}
                            </span>
                          </dd>
                          <dt className="col-5 text-body-secondary fw-normal">{t('tasks.createdAt')}</dt>
                          <dd className="col-7">{formatDateTime(bug.createdAt)}</dd>
                          {bug.resolvedAt && (
                            <>
                              <dt className="col-5 text-body-secondary fw-normal">{t('bugs.resolvedAt')}</dt>
                              <dd className="col-7">{formatDateTime(bug.resolvedAt)}</dd>
                            </>
                          )}
                          {bug.closedAt && (
                            <>
                              <dt className="col-5 text-body-secondary fw-normal">{t('bugs.closedAt')}</dt>
                              <dd className="col-7">{formatDateTime(bug.closedAt)}</dd>
                            </>
                          )}
                        </dl>
                      </CCardBody>
                    </CCard>

                    <CCard className="mb-3">
                      <CCardBody>
                        <h3 className="h6">{t('history.title')}</h3>
                        <BugHistory teamId={teamId} projectId={projectId} bugId={bugId} />
                      </CCardBody>
                    </CCard>

                    {canManage && (
                      <CButton color="danger" variant="outline" className="w-100" onClick={() => setConfirmDelete(true)}>
                        <CIcon icon={cilTrash} className="me-1" />
                        {t('bugs.delete')}
                      </CButton>
                    )}
                  </CCol>
                </CRow>

                <BugFormModal teamId={teamId} projectId={projectId} bug={bug} visible={editing} onClose={() => setEditing(false)} />
                <BugStatusDialog teamId={teamId} projectId={projectId} bug={bug} target={target} onClose={() => setTarget(null)} />
              </>
            )
          }}
        </QueryBoundary>
      </CModalBody>
      <ConfirmModal visible={confirmDelete} title={t('bugs.delete')} message={t('bugs.deleteConfirm')} confirmLabel={t('common.delete')} busy={remove.isPending} onClose={() => setConfirmDelete(false)} onConfirm={() => remove.mutate()} />
    </CModal>
  )
}

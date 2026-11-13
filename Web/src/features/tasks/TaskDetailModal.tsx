import { cilPencil, cilTrash } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import {
  CButton, CCard, CCardBody, CCol, CFormInput, CFormSelect, CFormTextarea, CModal, CModalBody, CModalHeader, CModalTitle, CRow, CSpinner,
} from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { projectsApi, tasksApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { PRIORITIES, type Priority, type TaskDetail, type WorkItemStatus } from '@/shared/api/types'
import { useAuthStore } from '@/shared/auth/authStore'
import { PriorityBadge, TaskStatusBadge, TypeBadge } from '@/shared/components/Badges'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { Field } from '@/shared/components/Field'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { formatDate, formatDateTime } from '@/shared/utils/format'
import { CommentsSection } from './CommentsSection'
import { TaskHistory } from './TaskHistory'

const editSchema = z.object({
  title: z.string().trim().min(1, 'validation.required').max(300, 'validation.tooLong'),
  description: z.string().max(20000, 'validation.tooLong'),
  priority: z.enum(['Low', 'Medium', 'High', 'Urgent']),
  estimate: z.string().trim().refine((v) => v === '' || (!Number.isNaN(Number(v)) && Number(v) >= 0 && Number(v) <= 99999), 'validation.estimate'),
  dueDate: z.string(),
})
type EditValues = z.infer<typeof editSchema>

const toForm = (t: TaskDetail): EditValues => ({
  title: t.title,
  description: t.description ?? '',
  priority: t.priority,
  estimate: t.estimateHours == null ? '' : String(t.estimateHours),
  dueDate: t.dueDate ?? '',
})

interface Props {
  teamId: string
  projectId: string
  taskId: string
  canContribute: boolean
  canManage: boolean
  onClose: () => void
}

export function TaskDetailModal({ teamId, projectId, taskId, canContribute, canManage, onClose }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError, applyFieldErrors } = useApiError()
  const me = useAuthStore((s) => s.user)
  const [editing, setEditing] = useState(false)
  const [confirmDelete, setConfirmDelete] = useState(false)

  const key = qk.task(teamId, projectId, taskId)
  const query = useQuery({ queryKey: key, queryFn: () => tasksApi.get(teamId, projectId, taskId) })
  const members = useQuery({ queryKey: qk.projectMembers(teamId, projectId), queryFn: () => projectsApi.members(teamId, projectId) })

  // Thay đổi task ảnh hưởng board, danh sách, lịch sử → làm mới cả nhánh dự án
  const refresh = () => queryClient.invalidateQueries({ queryKey: qk.project(teamId, projectId) })

  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<EditValues>({ resolver: zodResolver(editSchema) })

  useEffect(() => {
    if (query.data && !editing) reset(toForm(query.data))
  }, [query.data, editing, reset])

  const save = useMutation({
    mutationFn: (v: EditValues) =>
      tasksApi.update(teamId, projectId, taskId, {
        title: v.title,
        description: v.description.trim() || null,
        priority: v.priority as Priority,
        estimateHours: v.estimate === '' ? null : Number(v.estimate),
        dueDate: v.dueDate || null,
      }),
    onSuccess: async () => {
      toast.success(t('common.saved'))
      setEditing(false)
      await refresh()
    },
    onError: async (e) => {
      applyFieldErrors(e, setError, ['title', 'description', 'priority', 'estimateHours', 'dueDate'])
      await refresh() // 409 (RowVersion): có người sửa trước → tải lại bản mới nhất
    },
  })

  const changeStatus = useMutation({
    mutationFn: (status: WorkItemStatus) => tasksApi.changeStatus(teamId, projectId, taskId, status),
    onSuccess: refresh,
    onError: async (e) => {
      showError(e)
      await refresh()
    },
  })

  const assign = useMutation({
    mutationFn: (assigneeId: string | null) => tasksApi.assign(teamId, projectId, taskId, assigneeId),
    onSuccess: refresh,
    onError: async (e) => {
      showError(e)
      await refresh()
    },
  })

  const remove = useMutation({
    mutationFn: () => tasksApi.remove(teamId, projectId, taskId),
    onSuccess: async () => {
      toast.success(t('tasks.deleted'))
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
              <TypeBadge type={query.data.type} />
              <TaskStatusBadge status={query.data.status} />
            </>
          ) : (
            t('tasks.detail')
          )}
        </CModalTitle>
      </CModalHeader>
      <CModalBody>
        <QueryBoundary query={query}>
          {(task) => (
            <CRow className="g-4">
              {/* Cột trái: nội dung + bình luận */}
              <CCol lg={8}>
                {editing ? (
                  <form onSubmit={handleSubmit((v) => save.mutate(v))} noValidate>
                    <Field label={t('tasks.titleField')} required error={errors.title?.message} htmlFor="edit-title">
                      <CFormInput id="edit-title" invalid={!!errors.title} {...register('title')} />
                    </Field>
                    <Field label={t('tasks.description')} error={errors.description?.message} htmlFor="edit-desc">
                      <CFormTextarea id="edit-desc" rows={6} invalid={!!errors.description} {...register('description')} />
                    </Field>
                    <CRow>
                      <CCol sm={4}>
                        <Field label={t('tasks.priority')} htmlFor="edit-priority">
                          <CFormSelect id="edit-priority" {...register('priority')}>
                            {PRIORITIES.map((p) => (
                              <option key={p} value={p}>{t(`enums.priority.${p}`)}</option>
                            ))}
                          </CFormSelect>
                        </Field>
                      </CCol>
                      <CCol sm={4}>
                        <Field label={t('tasks.estimate')} error={errors.estimate?.message} htmlFor="edit-estimate">
                          <CFormInput id="edit-estimate" type="number" min={0} step="0.5" invalid={!!errors.estimate} {...register('estimate')} />
                        </Field>
                      </CCol>
                      <CCol sm={4}>
                        <Field label={t('tasks.dueDate')} htmlFor="edit-due">
                          <CFormInput id="edit-due" type="date" {...register('dueDate')} />
                        </Field>
                      </CCol>
                    </CRow>
                    <div className="d-flex gap-2">
                      <CButton type="submit" color="primary" disabled={save.isPending}>
                        {save.isPending && <CSpinner size="sm" className="me-2" />}
                        {t('common.save')}
                      </CButton>
                      <CButton color="secondary" variant="outline" disabled={save.isPending} onClick={() => { reset(toForm(task)); setEditing(false) }}>
                        {t('common.cancel')}
                      </CButton>
                    </div>
                  </form>
                ) : (
                  <div>
                    <div className="d-flex justify-content-between align-items-start gap-2 mb-3">
                      <h3 className="h4 mb-0" style={{ wordBreak: 'break-word' }}>{task.title}</h3>
                      {canContribute && (
                        <CButton color="secondary" variant="outline" size="sm" className="flex-shrink-0" onClick={() => setEditing(true)}>
                          <CIcon icon={cilPencil} className="me-1" />
                          {t('common.edit')}
                        </CButton>
                      )}
                    </div>
                    {task.description ? (
                      <div className="task-description mb-3">{task.description}</div>
                    ) : (
                      <p className="text-body-secondary">{t('common.noDescription')}</p>
                    )}
                  </div>
                )}

                <hr />
                <CommentsSection teamId={teamId} projectId={projectId} taskId={taskId} meId={me?.id} canContribute={canContribute} canManage={canManage} />
              </CCol>

              {/* Cột phải: thuộc tính + lịch sử */}
              <CCol lg={4}>
                <CCard className="mb-3">
                  <CCardBody>
                    <Field label={t('tasks.status')} htmlFor="detail-status" className="mb-3">
                      <CFormSelect
                        id="detail-status" value={task.status} disabled={!canContribute || changeStatus.isPending || task.allowedTransitions.length === 0}
                        onChange={(e) => changeStatus.mutate(e.target.value as WorkItemStatus)}
                      >
                        <option value={task.status}>{t(`enums.taskStatus.${task.status}`)}</option>
                        {task.allowedTransitions.map((s) => (
                          <option key={s} value={s}>→ {t(`enums.taskStatus.${s}`)}</option>
                        ))}
                      </CFormSelect>
                    </Field>

                    <Field label={t('tasks.assignee')} htmlFor="detail-assignee" className="mb-3">
                      <CFormSelect
                        id="detail-assignee" value={task.assigneeId ?? ''} disabled={!canContribute || assign.isPending}
                        onChange={(e) => assign.mutate(e.target.value || null)}
                      >
                        <option value="">{t('tasks.unassigned')}</option>
                        {/* Người đang được giao có thể đã bị gỡ khỏi dự án: vẫn hiển thị để select không lệch giá trị */}
                        {task.assigneeId && !members.data?.items.some((m) => m.userId === task.assigneeId) && (
                          <option value={task.assigneeId}>{task.assigneeName}</option>
                        )}
                        {members.data?.items.map((m) => (
                          <option key={m.userId} value={m.userId}>{m.fullName}</option>
                        ))}
                      </CFormSelect>
                    </Field>

                    <dl className="row small mb-0">
                      <dt className="col-5 text-body-secondary fw-normal">{t('tasks.priority')}</dt>
                      <dd className="col-7"><PriorityBadge priority={task.priority} /></dd>
                      <dt className="col-5 text-body-secondary fw-normal">{t('tasks.estimate')}</dt>
                      <dd className="col-7">{task.estimateHours == null ? '—' : `${task.estimateHours}h`}</dd>
                      <dt className="col-5 text-body-secondary fw-normal">{t('tasks.dueDate')}</dt>
                      <dd className="col-7">{formatDate(task.dueDate)}</dd>
                      <dt className="col-5 text-body-secondary fw-normal">{t('tasks.reporter')}</dt>
                      <dd className="col-7">
                        <span className="d-inline-flex align-items-center gap-1">
                          <UserAvatar name={task.reporterName} size="sm" />
                          {task.reporterName}
                        </span>
                      </dd>
                      <dt className="col-5 text-body-secondary fw-normal">{t('tasks.createdAt')}</dt>
                      <dd className="col-7">{formatDateTime(task.createdAt)}</dd>
                      {task.completedAt && (
                        <>
                          <dt className="col-5 text-body-secondary fw-normal">{t('tasks.completedAt')}</dt>
                          <dd className="col-7">{formatDateTime(task.completedAt)}</dd>
                        </>
                      )}
                    </dl>
                  </CCardBody>
                </CCard>

                <CCard className="mb-3">
                  <CCardBody>
                    <h3 className="h6">{t('history.title')}</h3>
                    <TaskHistory teamId={teamId} projectId={projectId} taskId={taskId} />
                  </CCardBody>
                </CCard>

                {canManage && (
                  <CButton color="danger" variant="outline" className="w-100" onClick={() => setConfirmDelete(true)}>
                    <CIcon icon={cilTrash} className="me-1" />
                    {t('tasks.delete')}
                  </CButton>
                )}
              </CCol>
            </CRow>
          )}
        </QueryBoundary>
      </CModalBody>

      <ConfirmModal
        visible={confirmDelete}
        title={t('tasks.delete')}
        message={t('tasks.deleteConfirm')}
        confirmLabel={t('common.delete')}
        busy={remove.isPending}
        onClose={() => setConfirmDelete(false)}
        onConfirm={() => remove.mutate()}
      />
    </CModal>
  )
}

import {
  CButton, CCol, CForm, CFormInput, CFormSelect, CFormTextarea, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CRow, CSpinner,
} from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { projectsApi, tasksApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { PRIORITIES, WORK_ITEM_TYPES, type Priority, type WorkItemType } from '@/shared/api/types'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'

// Quy tắc khớp CreateTaskValidator: SubTask bắt buộc có cha; Epic không có cha
const schema = z
  .object({
    type: z.enum(['Epic', 'Story', 'Task', 'SubTask']),
    title: z.string().trim().min(1, 'validation.required').max(300, 'validation.tooLong'),
    description: z.string().max(20000, 'validation.tooLong'),
    priority: z.enum(['Low', 'Medium', 'High', 'Urgent']),
    assigneeId: z.string(),
    parentId: z.string(),
    estimate: z.string().trim().refine((v) => v === '' || (!Number.isNaN(Number(v)) && Number(v) >= 0 && Number(v) <= 99999), 'validation.estimate'),
    dueDate: z.string(),
  })
  .refine((v) => v.type !== 'SubTask' || v.parentId !== '', { path: ['parentId'], message: 'validation.subTaskParent' })
type Values = z.infer<typeof schema>

const defaults: Values = { type: 'Task', title: '', description: '', priority: 'Medium', assigneeId: '', parentId: '', estimate: '', dueDate: '' }

interface Props {
  teamId: string
  projectId: string
  visible: boolean
  onClose: () => void
  onCreated?: (taskId: string) => void
}

export function TaskFormModal({ teamId, projectId, visible, onClose, onCreated }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { applyFieldErrors } = useApiError()

  const {
    register, handleSubmit, reset, setError, watch, setValue, formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: defaults })

  useEffect(() => {
    if (visible) reset(defaults)
  }, [visible, reset])

  const type = watch('type')
  useEffect(() => {
    if (type === 'Epic') setValue('parentId', '') // Epic không thể có item cha
  }, [type, setValue])

  const members = useQuery({ queryKey: qk.projectMembers(teamId, projectId), queryFn: () => projectsApi.members(teamId, projectId), enabled: visible })
  // Danh sách ứng viên làm cha: 100 task mới nhất, bỏ SubTask (Sub-task không thể làm cha)
  const parents = useQuery({
    queryKey: [...qk.project(teamId, projectId), 'parent-candidates'],
    queryFn: () => tasksApi.list(teamId, projectId, { pageSize: 100 }),
    enabled: visible,
    select: (d) => d.items.filter((x) => x.type !== 'SubTask'),
  })

  const mutation = useMutation({
    mutationFn: (v: Values) =>
      tasksApi.create(teamId, projectId, {
        type: v.type as WorkItemType,
        title: v.title,
        description: v.description.trim() || null,
        priority: v.priority as Priority,
        assigneeId: v.assigneeId || null,
        parentId: v.type === 'Epic' ? null : v.parentId || null,
        estimateHours: v.estimate === '' ? null : Number(v.estimate),
        dueDate: v.dueDate || null,
      }),
    onSuccess: async (created) => {
      toast.success(t('tasks.created', { code: created.code }))
      await queryClient.invalidateQueries({ queryKey: qk.project(teamId, projectId) })
      onClose()
      onCreated?.(created.id)
    },
    onError: (e) => applyFieldErrors(e, setError, ['type', 'title', 'description', 'priority', 'assigneeId', 'parentId', 'estimateHours', 'dueDate']),
  })

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static" size="lg">
      <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
        <CModalHeader>
          <CModalTitle>{t('tasks.create')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CRow>
            <CCol sm={4}>
              <Field label={t('tasks.type')} required error={errors.type?.message} htmlFor="task-type">
                <CFormSelect id="task-type" {...register('type')}>
                  {WORK_ITEM_TYPES.map((x) => (
                    <option key={x} value={x}>{t(`enums.taskType.${x}`)}</option>
                  ))}
                </CFormSelect>
              </Field>
            </CCol>
            <CCol sm={8}>
              <Field label={t('tasks.titleField')} required error={errors.title?.message} htmlFor="task-title">
                <CFormInput id="task-title" autoFocus invalid={!!errors.title} {...register('title')} />
              </Field>
            </CCol>
          </CRow>

          <Field label={t('tasks.description')} error={errors.description?.message} htmlFor="task-desc">
            <CFormTextarea id="task-desc" rows={4} invalid={!!errors.description} {...register('description')} />
          </Field>

          <CRow>
            <CCol sm={6}>
              <Field label={t('tasks.priority')} error={errors.priority?.message} htmlFor="task-priority">
                <CFormSelect id="task-priority" {...register('priority')}>
                  {PRIORITIES.map((x) => (
                    <option key={x} value={x}>{t(`enums.priority.${x}`)}</option>
                  ))}
                </CFormSelect>
              </Field>
            </CCol>
            <CCol sm={6}>
              <Field label={t('tasks.assignee')} error={errors.assigneeId?.message} htmlFor="task-assignee">
                <CFormSelect id="task-assignee" {...register('assigneeId')}>
                  <option value="">{t('tasks.unassigned')}</option>
                  {members.data?.items.map((m) => (
                    <option key={m.userId} value={m.userId}>{m.fullName}</option>
                  ))}
                </CFormSelect>
              </Field>
            </CCol>
          </CRow>

          {type !== 'Epic' && (
            <Field label={t('tasks.parent')} required={type === 'SubTask'} error={errors.parentId?.message} htmlFor="task-parent">
              <CFormSelect id="task-parent" invalid={!!errors.parentId} {...register('parentId')}>
                <option value="">{t('tasks.noParent')}</option>
                {parents.data?.map((p) => (
                  <option key={p.id} value={p.id}>{p.code} — {p.title}</option>
                ))}
              </CFormSelect>
            </Field>
          )}

          <CRow>
            <CCol sm={6}>
              <Field label={t('tasks.estimate')} error={errors.estimate?.message} htmlFor="task-estimate">
                <CFormInput id="task-estimate" type="number" min={0} step="0.5" invalid={!!errors.estimate} {...register('estimate')} />
              </Field>
            </CCol>
            <CCol sm={6}>
              <Field label={t('tasks.dueDate')} error={errors.dueDate?.message} htmlFor="task-due">
                <CFormInput id="task-due" type="date" {...register('dueDate')} />
              </Field>
            </CCol>
          </CRow>
          {members.data?.items.length === 0 && <p className="small text-body-secondary mb-0">{t('tasks.noProjectMembersHint')}</p>}
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" variant="outline" onClick={onClose} disabled={mutation.isPending}>
            {t('common.cancel')}
          </CButton>
          <CButton type="submit" color="primary" disabled={mutation.isPending}>
            {mutation.isPending && <CSpinner size="sm" className="me-2" />}
            {t('common.create')}
          </CButton>
        </CModalFooter>
      </CForm>
    </CModal>
  )
}

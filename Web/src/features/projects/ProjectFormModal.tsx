import {
  CButton, CCol, CForm, CFormInput, CFormSwitch, CFormTextarea, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CRow, CSpinner,
} from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { projectsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { Project } from '@/shared/api/types'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'

const schema = z
  .object({
    name: z.string().trim().min(1, 'validation.required').max(200, 'validation.tooLong'),
    // 2-10 ký tự chữ/số, bắt đầu bằng chữ (khớp CreateProjectValidator)
    key: z.string().trim(),
    description: z.string().max(4000, 'validation.tooLong'),
    startDate: z.string(),
    endDate: z.string(),
    useSprints: z.boolean(),
  })
  .refine((v) => !v.startDate || !v.endDate || v.endDate >= v.startDate, { path: ['endDate'], message: 'validation.endBeforeStart' })
type Values = z.infer<typeof schema>

const KEY_PATTERN = /^[A-Za-z][A-Za-z0-9]{1,9}$/

interface Props {
  teamId: string
  /** Có project → chế độ sửa (Key không đổi được). */
  project?: Project
  visible: boolean
  onClose: () => void
  onCreated?: (projectId: string) => void
}

const emptyValues: Values = { name: '', key: '', description: '', startDate: '', endDate: '', useSprints: false }

export function ProjectFormModal({ teamId, project, visible, onClose, onCreated }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { applyFieldErrors } = useApiError()
  const editing = !!project

  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: emptyValues })

  useEffect(() => {
    if (!visible) return
    reset(
      project
        ? {
            name: project.name, key: project.key, description: project.description ?? '',
            startDate: project.startDate ?? '', endDate: project.endDate ?? '', useSprints: project.useSprints,
          }
        : emptyValues,
    )
  }, [visible, project, reset])

  const mutation = useMutation({
    mutationFn: async (v: Values) => {
      const base = {
        name: v.name,
        description: v.description.trim() || null,
        startDate: v.startDate || null,
        endDate: v.endDate || null,
        useSprints: v.useSprints,
      }
      if (project) {
        await projectsApi.update(teamId, project.id, base)
        return project.id
      }
      return (await projectsApi.create(teamId, { ...base, key: v.key })).id
    },
    onSuccess: async (id) => {
      toast.success(editing ? t('common.saved') : t('projects.created'))
      await queryClient.invalidateQueries({ queryKey: qk.projects(teamId) })
      onClose()
      if (!editing) onCreated?.(id)
    },
    onError: (e) => applyFieldErrors(e, setError, ['name', 'key', 'description', 'startDate', 'endDate']),
  })

  const submit = (v: Values) => {
    if (!editing && !KEY_PATTERN.test(v.key)) {
      setError('key', { type: 'validate', message: 'validation.projectKey' })
      return
    }
    mutation.mutate(v)
  }

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static">
      <CForm onSubmit={handleSubmit(submit)} noValidate>
        <CModalHeader>
          <CModalTitle>{editing ? t('projects.edit') : t('projects.create')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CRow>
            <CCol sm={8}>
              <Field label={t('projects.name')} required error={errors.name?.message} htmlFor="project-name">
                <CFormInput id="project-name" invalid={!!errors.name} {...register('name')} />
              </Field>
            </CCol>
            <CCol sm={4}>
              <Field label={t('projects.key')} required={!editing} error={errors.key?.message} hint={editing ? t('projects.keyImmutable') : t('projects.keyHint')} htmlFor="project-key">
                <CFormInput id="project-key" className="text-uppercase" maxLength={10} disabled={editing} invalid={!!errors.key} {...register('key')} />
              </Field>
            </CCol>
          </CRow>
          <Field label={t('projects.description')} error={errors.description?.message} htmlFor="project-desc">
            <CFormTextarea id="project-desc" rows={3} invalid={!!errors.description} {...register('description')} />
          </Field>
          <CRow>
            <CCol sm={6}>
              <Field label={t('projects.startDate')} error={errors.startDate?.message} htmlFor="project-start">
                <CFormInput id="project-start" type="date" {...register('startDate')} />
              </Field>
            </CCol>
            <CCol sm={6}>
              <Field label={t('projects.endDate')} error={errors.endDate?.message} htmlFor="project-end">
                <CFormInput id="project-end" type="date" invalid={!!errors.endDate} {...register('endDate')} />
              </Field>
            </CCol>
          </CRow>
          <CFormSwitch id="project-sprints" label={t('projects.useSprints')} {...register('useSprints')} />
          <div className="form-text">{t('projects.useSprintsHint')}</div>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" variant="outline" onClick={onClose} disabled={mutation.isPending}>
            {t('common.cancel')}
          </CButton>
          <CButton type="submit" color="primary" disabled={mutation.isPending}>
            {mutation.isPending && <CSpinner size="sm" className="me-2" />}
            {editing ? t('common.save') : t('common.create')}
          </CButton>
        </CModalFooter>
      </CForm>
    </CModal>
  )
}

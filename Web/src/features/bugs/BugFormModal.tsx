import {
  CButton, CCol, CForm, CFormInput, CFormSelect, CFormTextarea, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CRow, CSpinner,
} from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { bugsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { BUG_ENVIRONMENTS, BUG_SEVERITIES, PRIORITIES, type BugDetail, type BugRequest } from '@/shared/api/types'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'

// Độ dài khớp CreateBugValidator/UpdateBugValidator
const schema = z.object({
  title: z.string().trim().min(1, 'validation.required').max(300, 'validation.tooLong'),
  severity: z.enum(['Trivial', 'Minor', 'Major', 'Critical']),
  priority: z.enum(['Low', 'Medium', 'High', 'Urgent']),
  environment: z.enum(['Dev', 'Staging', 'Production']),
  affectedVersion: z.string().trim().max(50, 'validation.tooLong'),
  stepsToReproduce: z.string().trim().min(1, 'validation.required').max(10000, 'validation.tooLong'),
  expectedResult: z.string().trim().min(1, 'validation.required').max(4000, 'validation.tooLong'),
  actualResult: z.string().trim().min(1, 'validation.required').max(4000, 'validation.tooLong'),
  deviceInfo: z.string().trim().max(500, 'validation.tooLong'),
})
type Values = z.infer<typeof schema>

const defaults: Values = {
  title: '', severity: 'Major', priority: 'Medium', environment: 'Dev', affectedVersion: '',
  stepsToReproduce: '', expectedResult: '', actualResult: '', deviceInfo: '',
}

const fromBug = (b: BugDetail): Values => ({
  title: b.title, severity: b.severity, priority: b.priority, environment: b.environment,
  affectedVersion: b.affectedVersion ?? '', stepsToReproduce: b.stepsToReproduce, expectedResult: b.expectedResult,
  actualResult: b.actualResult, deviceInfo: b.deviceInfo ?? '',
})

interface Props {
  teamId: string
  projectId: string
  /** Có bug → chế độ sửa. */
  bug?: BugDetail
  visible: boolean
  onClose: () => void
  onCreated?: (bugId: string) => void
}

export function BugFormModal({ teamId, projectId, bug, visible, onClose, onCreated }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { applyFieldErrors } = useApiError()
  const editing = !!bug

  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: defaults })

  useEffect(() => {
    if (visible) reset(bug ? fromBug(bug) : defaults)
  }, [visible, bug, reset])

  const mutation = useMutation({
    mutationFn: async (v: Values) => {
      const body: BugRequest = { ...v, affectedVersion: v.affectedVersion || null, deviceInfo: v.deviceInfo || null }
      if (bug) {
        await bugsApi.update(teamId, projectId, bug.id, body)
        return bug.id
      }
      return (await bugsApi.create(teamId, projectId, body)).id
    },
    onSuccess: async (id) => {
      toast.success(editing ? t('common.saved') : t('bugs.created'))
      await queryClient.invalidateQueries({ queryKey: qk.project(teamId, projectId) })
      onClose()
      if (!editing) onCreated?.(id)
    },
    onError: (e) =>
      applyFieldErrors(e, setError, ['title', 'severity', 'priority', 'environment', 'affectedVersion', 'stepsToReproduce', 'expectedResult', 'actualResult', 'deviceInfo']),
  })

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static" size="lg" scrollable>
      <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
        <CModalHeader>
          <CModalTitle>{editing ? t('bugs.edit') : t('bugs.create')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <Field label={t('tasks.titleField')} required error={errors.title?.message} htmlFor="bug-title">
            <CFormInput id="bug-title" autoFocus invalid={!!errors.title} {...register('title')} />
          </Field>
          <CRow>
            <CCol sm={4}>
              <Field label={t('bugs.severity')} htmlFor="bug-severity">
                <CFormSelect id="bug-severity" {...register('severity')}>
                  {BUG_SEVERITIES.map((s) => (
                    <option key={s} value={s}>{t(`enums.bugSeverity.${s}`)}</option>
                  ))}
                </CFormSelect>
              </Field>
            </CCol>
            <CCol sm={4}>
              <Field label={t('tasks.priority')} htmlFor="bug-priority">
                <CFormSelect id="bug-priority" {...register('priority')}>
                  {PRIORITIES.map((p) => (
                    <option key={p} value={p}>{t(`enums.priority.${p}`)}</option>
                  ))}
                </CFormSelect>
              </Field>
            </CCol>
            <CCol sm={4}>
              <Field label={t('bugs.environment')} htmlFor="bug-env">
                <CFormSelect id="bug-env" {...register('environment')}>
                  {BUG_ENVIRONMENTS.map((x) => (
                    <option key={x} value={x}>{t(`enums.bugEnvironment.${x}`)}</option>
                  ))}
                </CFormSelect>
              </Field>
            </CCol>
          </CRow>
          <CRow>
            <CCol sm={6}>
              <Field label={t('bugs.affectedVersion')} error={errors.affectedVersion?.message} htmlFor="bug-version">
                <CFormInput id="bug-version" invalid={!!errors.affectedVersion} {...register('affectedVersion')} />
              </Field>
            </CCol>
            <CCol sm={6}>
              <Field label={t('bugs.deviceInfo')} error={errors.deviceInfo?.message} htmlFor="bug-device">
                <CFormInput id="bug-device" invalid={!!errors.deviceInfo} {...register('deviceInfo')} />
              </Field>
            </CCol>
          </CRow>
          <Field label={t('bugs.steps')} required error={errors.stepsToReproduce?.message} htmlFor="bug-steps">
            <CFormTextarea id="bug-steps" rows={4} invalid={!!errors.stepsToReproduce} {...register('stepsToReproduce')} />
          </Field>
          <CRow>
            <CCol md={6}>
              <Field label={t('bugs.expected')} required error={errors.expectedResult?.message} htmlFor="bug-expected">
                <CFormTextarea id="bug-expected" rows={3} invalid={!!errors.expectedResult} {...register('expectedResult')} />
              </Field>
            </CCol>
            <CCol md={6}>
              <Field label={t('bugs.actual')} required error={errors.actualResult?.message} htmlFor="bug-actual">
                <CFormTextarea id="bug-actual" rows={3} invalid={!!errors.actualResult} {...register('actualResult')} />
              </Field>
            </CCol>
          </CRow>
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

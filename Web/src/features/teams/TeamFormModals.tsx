import { CButton, CForm, CFormInput, CFormTextarea, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CSpinner } from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { teamsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { Team, UserSummary } from '@/shared/api/types'
import { useAuthStore } from '@/shared/auth/authStore'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { UserPicker } from './UserPicker'

// ---------- Tạo team (chỉ SystemAdmin) ----------
const createSchema = z.object({
  name: z.string().trim().min(1, 'validation.required').max(200, 'validation.tooLong'),
  code: z.string().trim().min(1, 'validation.required').max(20, 'validation.tooLong').regex(/^[A-Za-z0-9_-]+$/, 'validation.teamCode'),
  description: z.string().max(2000, 'validation.tooLong'),
  leadUserId: z.string().min(1, 'validation.leadRequired'),
})
type CreateValues = z.infer<typeof createSchema>

interface CreateProps {
  visible: boolean
  onClose: () => void
  onCreated: (teamId: string) => void
}

export function CreateTeamModal({ visible, onClose, onCreated }: CreateProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const reloadMe = useAuthStore((s) => s.reloadMe)
  const { applyFieldErrors } = useApiError()
  const [lead, setLead] = useState<UserSummary | null>(null)

  const {
    register, handleSubmit, reset, setValue, setError, formState: { errors },
  } = useForm<CreateValues>({ resolver: zodResolver(createSchema), defaultValues: { name: '', code: '', description: '', leadUserId: '' } })

  useEffect(() => {
    if (visible) {
      reset()
      setLead(null)
    }
  }, [visible, reset])

  const mutation = useMutation({
    mutationFn: (v: CreateValues) =>
      teamsApi.create({ name: v.name, code: v.code, description: v.description.trim() || null, leadUserId: v.leadUserId }),
    onSuccess: async ({ id }) => {
      toast.success(t('teams.created'))
      await queryClient.invalidateQueries({ queryKey: qk.teams })
      await reloadMe()
      onCreated(id)
    },
    onError: (e) => applyFieldErrors(e, setError, ['name', 'code', 'description', 'leadUserId']),
  })

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static">
      <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
        <CModalHeader>
          <CModalTitle>{t('teams.create')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <Field label={t('teams.name')} required error={errors.name?.message} htmlFor="team-name">
            <CFormInput id="team-name" invalid={!!errors.name} {...register('name')} />
          </Field>
          <Field label={t('teams.code')} required error={errors.code?.message} hint={t('teams.codeHint')} htmlFor="team-code">
            <CFormInput id="team-code" invalid={!!errors.code} {...register('code')} />
          </Field>
          <Field label={t('teams.description')} error={errors.description?.message} htmlFor="team-desc">
            <CFormTextarea id="team-desc" rows={3} invalid={!!errors.description} {...register('description')} />
          </Field>
          <Field label={t('teams.firstLead')} required error={errors.leadUserId?.message} hint={t('teams.firstLeadHint')}>
            <UserPicker
              value={lead}
              invalid={!!errors.leadUserId}
              onChange={(u) => {
                setLead(u)
                setValue('leadUserId', u?.id ?? '', { shouldValidate: true })
              }}
            />
          </Field>
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

// ---------- Sửa team (TeamLead) ----------
const editSchema = z.object({
  name: z.string().trim().min(1, 'validation.required').max(200, 'validation.tooLong'),
  description: z.string().max(2000, 'validation.tooLong'),
})
type EditValues = z.infer<typeof editSchema>

interface EditProps {
  team: Team
  visible: boolean
  onClose: () => void
}

export function EditTeamModal({ team, visible, onClose }: EditProps) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const reloadMe = useAuthStore((s) => s.reloadMe)
  const { applyFieldErrors } = useApiError()

  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<EditValues>({ resolver: zodResolver(editSchema), defaultValues: { name: team.name, description: team.description ?? '' } })

  useEffect(() => {
    if (visible) reset({ name: team.name, description: team.description ?? '' })
  }, [visible, team, reset])

  const mutation = useMutation({
    mutationFn: (v: EditValues) => teamsApi.update(team.id, { name: v.name, description: v.description.trim() || null }),
    onSuccess: async () => {
      toast.success(t('common.saved'))
      await queryClient.invalidateQueries({ queryKey: qk.teams })
      await reloadMe()
      onClose()
    },
    onError: (e) => applyFieldErrors(e, setError, ['name', 'description']),
  })

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static">
      <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
        <CModalHeader>
          <CModalTitle>{t('teams.edit')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <Field label={t('teams.name')} required error={errors.name?.message} htmlFor="edit-team-name">
            <CFormInput id="edit-team-name" invalid={!!errors.name} {...register('name')} />
          </Field>
          <Field label={t('teams.description')} error={errors.description?.message} htmlFor="edit-team-desc">
            <CFormTextarea id="edit-team-desc" rows={3} invalid={!!errors.description} {...register('description')} />
          </Field>
          <p className="small text-body-secondary mb-0">{t('teams.codeImmutable', { code: team.code })}</p>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" variant="outline" onClick={onClose} disabled={mutation.isPending}>
            {t('common.cancel')}
          </CButton>
          <CButton type="submit" color="primary" disabled={mutation.isPending}>
            {mutation.isPending && <CSpinner size="sm" className="me-2" />}
            {t('common.save')}
          </CButton>
        </CModalFooter>
      </CForm>
    </CModal>
  )
}


import { CButton, CForm, CFormInput, CFormSelect, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CSpinner } from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { teamsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { TEAM_ROLES, type TeamRole } from '@/shared/api/types'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'

const schema = z.object({
  email: z.string().trim().min(1, 'validation.required').max(256, 'validation.tooLong').email('validation.email'),
  role: z.enum(['Viewer', 'Tester', 'Developer', 'TeamLead']),
  capacity: z
    .string()
    .trim()
    .refine((v) => v === '' || (!Number.isNaN(Number(v)) && Number(v) >= 0 && Number(v) <= 168), 'validation.capacity'),
})
type Values = z.infer<typeof schema>

interface Props {
  teamId: string
  visible: boolean
  onClose: () => void
}

export function AddMemberModal({ teamId, visible, onClose }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { applyFieldErrors } = useApiError()
  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { email: '', role: 'Developer', capacity: '' } })

  useEffect(() => {
    if (visible) reset({ email: '', role: 'Developer', capacity: '' })
  }, [visible, reset])

  const mutation = useMutation({
    mutationFn: (v: Values) =>
      teamsApi.addMember(teamId, { email: v.email, role: v.role as TeamRole, weeklyCapacityHours: v.capacity === '' ? null : Number(v.capacity) }),
    onSuccess: async () => {
      toast.success(t('members.added'))
      await queryClient.invalidateQueries({ queryKey: qk.team(teamId) })
      await queryClient.invalidateQueries({ queryKey: qk.teams })
      onClose()
    },
    onError: (e) => applyFieldErrors(e, setError, ['email', 'role', 'weeklyCapacityHours']),
  })

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static">
      <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
        <CModalHeader>
          <CModalTitle>{t('members.add')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <Field label={t('auth.email')} required error={errors.email?.message} hint={t('members.addHint')} htmlFor="member-email">
            <CFormInput id="member-email" type="email" invalid={!!errors.email} {...register('email')} />
          </Field>
          <Field label={t('members.role')} required error={errors.role?.message} htmlFor="member-role">
            <CFormSelect id="member-role" {...register('role')}>
              {TEAM_ROLES.map((r) => (
                <option key={r} value={r}>
                  {t(`enums.teamRole.${r}`)}
                </option>
              ))}
            </CFormSelect>
          </Field>
          <Field label={t('members.capacity')} error={errors.capacity?.message} hint={t('members.capacityHint')} htmlFor="member-capacity">
            <CFormInput id="member-capacity" type="number" min={0} max={168} step="0.5" invalid={!!errors.capacity} {...register('capacity')} />
          </Field>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" variant="outline" onClick={onClose} disabled={mutation.isPending}>
            {t('common.cancel')}
          </CButton>
          <CButton type="submit" color="primary" disabled={mutation.isPending}>
            {mutation.isPending && <CSpinner size="sm" className="me-2" />}
            {t('common.add')}
          </CButton>
        </CModalFooter>
      </CForm>
    </CModal>
  )
}

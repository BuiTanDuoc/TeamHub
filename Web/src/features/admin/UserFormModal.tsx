import { CButton, CForm, CFormInput, CFormSwitch, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CSpinner } from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { usersApi } from '@/shared/api/endpoints'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { PASSWORD_POLICY_HINT_KEY, passwordField } from './PasswordFields'

const schema = z.object({
  email: z.string().trim().min(1, 'validation.required').max(256, 'validation.tooLong').email('validation.email'),
  fullName: z.string().trim().min(1, 'validation.required').max(200, 'validation.tooLong'),
  password: passwordField,
  isSystemAdmin: z.boolean(),
})
type Values = z.infer<typeof schema>
const defaults: Values = { email: '', fullName: '', password: '', isSystemAdmin: false }

interface Props {
  visible: boolean
  onClose: () => void
  onCreated: (userId: string) => void
}

export function UserFormModal({ visible, onClose, onCreated }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { applyFieldErrors } = useApiError()
  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: defaults })

  useEffect(() => {
    if (visible) reset(defaults)
  }, [visible, reset])

  const mutation = useMutation({
    mutationFn: (v: Values) => usersApi.create(v),
    onSuccess: async ({ id }) => {
      toast.success(t('users.created'))
      await queryClient.invalidateQueries({ queryKey: ['users'] })
      onClose()
      onCreated(id)
    },
    onError: (e) => applyFieldErrors(e, setError, ['email', 'fullName', 'password', 'isSystemAdmin']),
  })

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static">
      <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
        <CModalHeader>
          <CModalTitle>{t('users.create')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <Field label={t('auth.email')} required error={errors.email?.message} htmlFor="user-email">
            <CFormInput id="user-email" type="email" autoComplete="off" autoFocus invalid={!!errors.email} {...register('email')} />
          </Field>
          <Field label={t('users.fullName')} required error={errors.fullName?.message} htmlFor="user-name">
            <CFormInput id="user-name" invalid={!!errors.fullName} {...register('fullName')} />
          </Field>
          <Field label={t('users.initialPassword')} required error={errors.password?.message} hint={t(PASSWORD_POLICY_HINT_KEY)} htmlFor="user-password">
            <CFormInput id="user-password" type="password" autoComplete="new-password" invalid={!!errors.password} {...register('password')} />
          </Field>
          <CFormSwitch id="user-admin" label={t('users.grantSystemAdmin')} {...register('isSystemAdmin')} />
          <div className="form-text">{t('users.createHint')}</div>
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

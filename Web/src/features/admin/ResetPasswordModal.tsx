import { CButton, CForm, CFormInput, CModal, CModalBody, CModalFooter, CModalHeader, CModalTitle, CSpinner } from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { z } from 'zod'
import { usersApi } from '@/shared/api/endpoints'
import { Field } from '@/shared/components/Field'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { PASSWORD_POLICY_HINT_KEY, passwordField, passwordsMatch } from './PasswordFields'

const schema = z
  .object({ newPassword: passwordField, confirmPassword: z.string() })
  .refine(passwordsMatch, { path: ['confirmPassword'], message: 'validation.passwordMismatch' })
type Values = z.infer<typeof schema>

interface Props {
  userId: string
  userName: string
  visible: boolean
  onClose: () => void
}

export function ResetPasswordModal({ userId, userName, visible, onClose }: Props) {
  const { t } = useTranslation()
  const { applyFieldErrors } = useApiError()
  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<Values>({ resolver: zodResolver(schema), defaultValues: { newPassword: '', confirmPassword: '' } })

  useEffect(() => {
    if (visible) reset({ newPassword: '', confirmPassword: '' })
  }, [visible, reset])

  const mutation = useMutation({
    mutationFn: (v: Values) => usersApi.resetPassword(userId, v.newPassword),
    onSuccess: () => {
      toast.success(t('users.passwordReset'))
      onClose()
    },
    onError: (e) => applyFieldErrors(e, setError, ['newPassword']),
  })

  return (
    <CModal visible={visible} onClose={onClose} backdrop="static" alignment="center">
      <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
        <CModalHeader>
          <CModalTitle>{t('users.resetPassword')}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          <p>{t('users.resetPasswordHint', { name: userName })}</p>
          <Field label={t('users.newPassword')} required error={errors.newPassword?.message} hint={t(PASSWORD_POLICY_HINT_KEY)} htmlFor="reset-new">
            <CFormInput id="reset-new" type="password" autoComplete="new-password" autoFocus invalid={!!errors.newPassword} {...register('newPassword')} />
          </Field>
          <Field label={t('users.confirmPassword')} required error={errors.confirmPassword?.message} htmlFor="reset-confirm" className="mb-0">
            <CFormInput id="reset-confirm" type="password" autoComplete="new-password" invalid={!!errors.confirmPassword} {...register('confirmPassword')} />
          </Field>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" variant="outline" onClick={onClose} disabled={mutation.isPending}>
            {t('common.cancel')}
          </CButton>
          <CButton type="submit" color="warning" disabled={mutation.isPending}>
            {mutation.isPending && <CSpinner size="sm" className="me-2" />}
            {t('users.resetPassword')}
          </CButton>
        </CModalFooter>
      </CForm>
    </CModal>
  )
}

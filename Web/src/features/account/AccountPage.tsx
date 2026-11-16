import { CButton, CCard, CCardBody, CCardHeader, CCol, CForm, CFormInput, CRow, CSpinner } from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { accountApi } from '@/shared/api/endpoints'
import { useAuthStore } from '@/shared/auth/authStore'
import { Field } from '@/shared/components/Field'
import { PageHeader } from '@/shared/components/PageHeader'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { PASSWORD_POLICY_HINT_KEY, passwordField, passwordsMatch } from '../admin/PasswordFields'

// ---------- Hồ sơ ----------
const isHttps = (v: string): boolean => {
  try {
    return new URL(v).protocol === 'https:'
  } catch {
    return false
  }
}
const profileSchema = z.object({
  fullName: z.string().trim().min(1, 'validation.required').max(200, 'validation.tooLong'),
  // Backend chỉ nhận https tuyệt đối (UpdateMyProfileValidator); để trống = xóa ảnh
  avatarUrl: z.string().trim().max(500, 'validation.tooLong').refine((v) => v === '' || isHttps(v), 'validation.httpsUrl'),
})
type ProfileValues = z.infer<typeof profileSchema>

function ProfileCard() {
  const { t } = useTranslation()
  const user = useAuthStore((s) => s.user)
  const reloadMe = useAuthStore((s) => s.reloadMe)
  const { applyFieldErrors } = useApiError()
  const {
    register, handleSubmit, reset, setError, watch, formState: { errors, isDirty },
  } = useForm<ProfileValues>({ resolver: zodResolver(profileSchema), defaultValues: { fullName: '', avatarUrl: '' } })

  useEffect(() => {
    if (user) reset({ fullName: user.fullName, avatarUrl: user.avatarUrl ?? '' })
  }, [user, reset])

  const mutation = useMutation({
    mutationFn: (v: ProfileValues) => accountApi.updateProfile(v.fullName, v.avatarUrl || null),
    onSuccess: async () => {
      await reloadMe()
      toast.success(t('common.saved'))
    },
    onError: (e) => applyFieldErrors(e, setError, ['fullName', 'avatarUrl']),
  })

  const avatarPreview = watch('avatarUrl')
  const previewUrl = isHttps(avatarPreview) ? avatarPreview : null

  return (
    <CCard className="h-100">
      <CCardHeader className="fw-semibold">{t('account.profile')}</CCardHeader>
      <CCardBody>
        <div className="d-flex align-items-center gap-3 mb-4">
          <UserAvatar name={watch('fullName') || user?.fullName || '?'} src={previewUrl} size="xl" />
          <div>
            <div className="text-body-secondary">{user?.email}</div>
            <div className="small text-body-secondary">{t('account.emailFixed')}</div>
          </div>
        </div>
        <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
          <Field label={t('users.fullName')} required error={errors.fullName?.message} htmlFor="profile-name">
            <CFormInput id="profile-name" invalid={!!errors.fullName} {...register('fullName')} />
          </Field>
          <Field label={t('account.avatarUrl')} error={errors.avatarUrl?.message} hint={t('account.avatarHint')} htmlFor="profile-avatar">
            <CFormInput id="profile-avatar" type="url" placeholder="https://" invalid={!!errors.avatarUrl} {...register('avatarUrl')} />
          </Field>
          <CButton type="submit" color="primary" disabled={!isDirty || mutation.isPending}>
            {mutation.isPending && <CSpinner size="sm" className="me-2" />}
            {t('common.save')}
          </CButton>
        </CForm>
      </CCardBody>
    </CCard>
  )
}

// ---------- Đổi mật khẩu ----------
const passwordSchema = z
  .object({ currentPassword: z.string().min(1, 'validation.required').max(128, 'validation.tooLong'), newPassword: passwordField, confirmPassword: z.string() })
  .refine(passwordsMatch, { path: ['confirmPassword'], message: 'validation.passwordMismatch' })
  .refine((v) => v.newPassword !== v.currentPassword, { path: ['newPassword'], message: 'validation.passwordSame' })
type PasswordValues = z.infer<typeof passwordSchema>

function PasswordCard() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const logout = useAuthStore((s) => s.logout)
  const { applyFieldErrors } = useApiError()
  const {
    register, handleSubmit, reset, setError, formState: { errors },
  } = useForm<PasswordValues>({ resolver: zodResolver(passwordSchema), defaultValues: { currentPassword: '', newPassword: '', confirmPassword: '' } })

  const mutation = useMutation({
    mutationFn: (v: PasswordValues) => accountApi.changePassword(v.currentPassword, v.newPassword),
    onSuccess: async () => {
      reset()
      toast.success(t('account.passwordChanged'))
      // Backend thu hồi mọi phiên (kể cả phiên này) → xóa token cục bộ và đăng nhập lại
      await logout()
      navigate('/login', { replace: true })
    },
    onError: (e) => applyFieldErrors(e, setError, ['currentPassword', 'newPassword']),
  })

  return (
    <CCard className="h-100">
      <CCardHeader className="fw-semibold">{t('account.changePassword')}</CCardHeader>
      <CCardBody>
        <p className="small text-body-secondary">{t('account.changePasswordNote')}</p>
        <CForm onSubmit={handleSubmit((v) => mutation.mutate(v))} noValidate>
          <Field label={t('account.currentPassword')} required error={errors.currentPassword?.message} htmlFor="pw-current">
            <CFormInput id="pw-current" type="password" autoComplete="current-password" invalid={!!errors.currentPassword} {...register('currentPassword')} />
          </Field>
          <Field label={t('users.newPassword')} required error={errors.newPassword?.message} hint={t(PASSWORD_POLICY_HINT_KEY)} htmlFor="pw-new">
            <CFormInput id="pw-new" type="password" autoComplete="new-password" invalid={!!errors.newPassword} {...register('newPassword')} />
          </Field>
          <Field label={t('users.confirmPassword')} required error={errors.confirmPassword?.message} htmlFor="pw-confirm">
            <CFormInput id="pw-confirm" type="password" autoComplete="new-password" invalid={!!errors.confirmPassword} {...register('confirmPassword')} />
          </Field>
          <CButton type="submit" color="warning" disabled={mutation.isPending}>
            {mutation.isPending && <CSpinner size="sm" className="me-2" />}
            {t('account.changePassword')}
          </CButton>
        </CForm>
      </CCardBody>
    </CCard>
  )
}

export default function AccountPage() {
  const { t } = useTranslation()
  return (
    <>
      <PageHeader title={t('account.title')} subtitle={t('account.subtitle')} />
      <CRow className="g-3">
        <CCol lg={6}><ProfileCard /></CCol>
        <CCol lg={6}><PasswordCard /></CCol>
      </CRow>
    </>
  )
}

import { cilLockLocked, cilUser } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import {
  CAlert, CButton, CCard, CCardBody, CCol, CContainer, CForm, CFormInput, CInputGroup, CInputGroupText, CRow, CSpinner,
} from '@coreui/react'
import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { z } from 'zod'
import { LANGUAGES } from '@/i18n'
import { ApiError } from '@/shared/api/client'
import { useAuthStore } from '@/shared/auth/authStore'
import { Field } from '@/shared/components/Field'
import { GoogleSignInButton, isGoogleConfigured } from './GoogleSignInButton'

const schema = z.object({
  email: z.string().min(1, 'validation.required').max(256, 'validation.tooLong').email('validation.email'),
  password: z.string().min(1, 'validation.required').max(128, 'validation.tooLong'),
})
type FormValues = z.infer<typeof schema>

export default function LoginPage() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const status = useAuthStore((s) => s.status)
  const login = useAuthStore((s) => s.login)
  const loginWithGoogle = useAuthStore((s) => s.loginWithGoogle)
  const [error, setError] = useState<string | null>(null)
  const [googleBusy, setGoogleBusy] = useState(false)

  const from = (location.state as { from?: string } | null)?.from ?? '/dashboard'

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { email: '', password: '' } })

  if (status === 'authenticated') return <Navigate to={from} replace />

  const explain = (e: unknown): string => {
    if (e instanceof ApiError) {
      if (e.status === 429) return t('errors.tooManyRequests')
      if (e.status === 401) return e.detail ?? t('auth.invalidCredentials')
      return e.displayMessage
    }
    return t('errors.network')
  }

  const onSubmit = async (values: FormValues) => {
    setError(null)
    try {
      await login(values.email.trim(), values.password)
      navigate(from, { replace: true })
    } catch (e) {
      setError(explain(e))
    }
  }

  const onGoogle = async (idToken: string) => {
    setError(null)
    setGoogleBusy(true)
    try {
      await loginWithGoogle(idToken)
      navigate(from, { replace: true })
    } catch (e) {
      setError(explain(e))
    } finally {
      setGoogleBusy(false)
    }
  }

  return (
    <div className="login-wrapper bg-body-tertiary">
      <CContainer>
        <CRow className="justify-content-center">
          <CCol md={8} lg={5} xl={4}>
            <div className="text-center mb-4">
              <svg width="48" height="48" viewBox="0 0 32 32" aria-hidden="true">
                <rect width="32" height="32" rx="7" fill="#5856d6" />
                <path d="M9 10h14v3h-5.5v10h-3V13H9z" fill="#fff" />
              </svg>
              <h1 className="h2 mt-2 mb-0">TeamHub</h1>
              <p className="text-body-secondary">{t('auth.tagline')}</p>
            </div>

            <CCard className="p-2">
              <CCardBody>
                <h2 className="h4">{t('auth.login')}</h2>
                <p className="text-body-secondary">{t('auth.loginHint')}</p>

                {error && (
                  <CAlert color="danger" dismissible onClose={() => setError(null)}>
                    {error}
                  </CAlert>
                )}

                <CForm onSubmit={handleSubmit(onSubmit)} noValidate>
                  <Field label={t('auth.email')} error={errors.email?.message} htmlFor="email" className="mb-3">
                    <CInputGroup>
                      <CInputGroupText>
                        <CIcon icon={cilUser} />
                      </CInputGroupText>
                      <CFormInput id="email" type="email" autoComplete="username" autoFocus invalid={!!errors.email} {...register('email')} />
                    </CInputGroup>
                  </Field>

                  <Field label={t('auth.password')} error={errors.password?.message} htmlFor="password" className="mb-4">
                    <CInputGroup>
                      <CInputGroupText>
                        <CIcon icon={cilLockLocked} />
                      </CInputGroupText>
                      <CFormInput id="password" type="password" autoComplete="current-password" invalid={!!errors.password} {...register('password')} />
                    </CInputGroup>
                  </Field>

                  <CButton type="submit" color="primary" className="w-100" disabled={isSubmitting || googleBusy}>
                    {isSubmitting && <CSpinner size="sm" className="me-2" />}
                    {t('auth.login')}
                  </CButton>
                </CForm>

                {isGoogleConfigured && (
                  <>
                    <div className="d-flex align-items-center my-3 text-body-secondary small">
                      <hr className="flex-grow-1" />
                      <span className="mx-2">{t('auth.or')}</span>
                      <hr className="flex-grow-1" />
                    </div>
                    <GoogleSignInButton onCredential={(token) => void onGoogle(token)} />
                  </>
                )}
              </CCardBody>
            </CCard>

            <div className="text-center mt-3 small">
              {LANGUAGES.map((lang, i) => (
                <span key={lang.code}>
                  {i > 0 && <span className="text-body-secondary"> · </span>}
                  <button
                    type="button"
                    className={`btn btn-link btn-sm p-0 ${i18n.language.startsWith(lang.code) ? 'fw-bold' : ''}`}
                    onClick={() => void i18n.changeLanguage(lang.code)}
                  >
                    {lang.flag} {lang.label}
                  </button>
                </span>
              ))}
            </div>
          </CCol>
        </CRow>
      </CContainer>
    </div>
  )
}

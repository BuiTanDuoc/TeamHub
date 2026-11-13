import { CButton } from '@coreui/react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

export default function NotFoundPage() {
  const { t } = useTranslation()
  return (
    <div className="text-center py-5">
      <h1 className="display-3 fw-bold">404</h1>
      <p className="lead text-body-secondary">{t('errors.notFound')}</p>
      <Link to="/dashboard">
        <CButton color="primary">{t('common.backToHome')}</CButton>
      </Link>
    </div>
  )
}

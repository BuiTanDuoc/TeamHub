import { CFooter } from '@coreui/react'
import { memo } from 'react'
import { useTranslation } from 'react-i18next'

function AppFooterComponent() {
  const { t } = useTranslation()
  return (
    <CFooter className="px-4">
      <div>
        <span className="fw-semibold">TeamHub</span>
        <span className="ms-1">&copy; {new Date().getFullYear()}</span>
      </div>
      <div className="ms-auto text-body-secondary small">{t('footer.poweredBy')}</div>
    </CFooter>
  )
}

export const AppFooter = memo(AppFooterComponent)

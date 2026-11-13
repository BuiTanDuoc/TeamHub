import { CAlert, CButton, CSpinner } from '@coreui/react'
import type { UseQueryResult } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { ApiError } from '../api/client'

interface Props<T> {
  query: UseQueryResult<T, Error>
  children: (data: T) => ReactNode
}

/** Gom 3 trạng thái tải/lỗi/dữ liệu của TanStack Query để màn hình chỉ lo phần hiển thị dữ liệu. */
export function QueryBoundary<T>({ query, children }: Props<T>) {
  const { t } = useTranslation()

  if (query.isPending) {
    return (
      <div className="text-center py-5">
        <CSpinner color="primary" />
      </div>
    )
  }

  if (query.isError) {
    const message = query.error instanceof ApiError ? query.error.displayMessage : t('errors.network')
    return (
      <CAlert color="danger" className="d-flex align-items-center justify-content-between">
        <span>{message}</span>
        <CButton color="danger" variant="outline" size="sm" onClick={() => void query.refetch()}>
          {t('common.retry')}
        </CButton>
      </CAlert>
    )
  }

  return <>{children(query.data)}</>
}

import { CFormInput, CListGroup, CListGroupItem, CSpinner } from '@coreui/react'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { usersApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { UserSummary } from '@/shared/api/types'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useDebounce } from '@/shared/hooks/useDebounce'

interface Props {
  value: UserSummary | null
  onChange: (user: UserSummary | null) => void
  invalid?: boolean
}

/** Chọn user qua GET /api/v1/users (chỉ SystemAdmin gọi được) — dùng khi tạo team để chỉ định TeamLead đầu tiên. */
export function UserPicker({ value, onChange, invalid }: Props) {
  const { t } = useTranslation()
  const [search, setSearch] = useState('')
  const debounced = useDebounce(search.trim(), 300)
  const query = useQuery({ queryKey: qk.users(debounced), queryFn: () => usersApi.search(debounced), enabled: !value })

  if (value) {
    return (
      <div className="d-flex align-items-center justify-content-between border rounded p-2">
        <div className="d-flex align-items-center gap-2">
          <UserAvatar name={value.fullName} src={value.avatarUrl} size="sm" />
          <div>
            <div className="fw-semibold">{value.fullName}</div>
            <div className="small text-body-secondary">{value.email}</div>
          </div>
        </div>
        <button type="button" className="btn btn-link btn-sm" onClick={() => onChange(null)}>
          {t('common.change')}
        </button>
      </div>
    )
  }

  return (
    <div>
      <CFormInput placeholder={t('teams.searchUserPlaceholder')} value={search} invalid={invalid} onChange={(e) => setSearch(e.target.value)} />
      <div className="mt-2" style={{ maxHeight: 220, overflowY: 'auto' }}>
        {query.isPending && (
          <div className="text-center py-2">
            <CSpinner size="sm" />
          </div>
        )}
        {query.data?.items.length === 0 && <div className="text-body-secondary small py-2">{t('common.noResults')}</div>}
        <CListGroup>
          {query.data?.items.map((u) => (
            <CListGroupItem key={u.id} as="button" type="button" className="d-flex align-items-center gap-2" onClick={() => onChange(u)}>
              <UserAvatar name={u.fullName} src={u.avatarUrl} size="sm" />
              <span>
                {u.fullName} <span className="text-body-secondary small">{u.email}</span>
              </span>
            </CListGroupItem>
          ))}
        </CListGroup>
      </div>
    </div>
  )
}

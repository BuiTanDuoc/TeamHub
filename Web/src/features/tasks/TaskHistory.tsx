import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { tasksApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { TaskHistoryEntry, WorkItemStatus } from '@/shared/api/types'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { formatDateTime } from '@/shared/utils/format'

interface Props {
  teamId: string
  projectId: string
  taskId: string
}

export function TaskHistory({ teamId, projectId, taskId }: Props) {
  const { t } = useTranslation()
  const query = useQuery({ queryKey: qk.taskHistory(teamId, projectId, taskId), queryFn: () => tasksApi.history(teamId, projectId, taskId) })

  // Backend lưu giá trị hiển thị: Status là tên enum (dịch được), Assignee là tên người, Estimate là số giờ
  const show = (entry: TaskHistoryEntry, value: string | null): string => {
    if (value === null || value === '') return t('history.none')
    if (entry.field === 'Status') return t(`enums.taskStatus.${value as WorkItemStatus}`, { defaultValue: value })
    if (entry.field === 'Estimate') return `${value}h`
    return value
  }

  return (
    <QueryBoundary query={query}>
      {(data) =>
        data.items.length === 0 ? (
          <p className="small text-body-secondary mb-0">{t('history.empty')}</p>
        ) : (
          <ul className="list-unstyled small mb-0">
            {data.items.map((h, i) => (
              <li key={`${h.changedAt}-${i}`} className="mb-2">
                <div>
                  <span className="fw-semibold">{h.changedByName}</span> · {t(`enums.historyField.${h.field}`)}:{' '}
                  <span className="text-body-secondary">{show(h, h.oldValue)}</span> → <span className="fw-semibold">{show(h, h.newValue)}</span>
                </div>
                <div className="text-body-secondary">{formatDateTime(h.changedAt)}</div>
              </li>
            ))}
          </ul>
        )
      }
    </QueryBoundary>
  )
}

import { useQuery } from '@tanstack/react-query'
import { useTranslation } from 'react-i18next'
import { bugsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import { BugStatusBadge } from '@/shared/components/Badges'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { formatDateTime } from '@/shared/utils/format'

export function BugHistory({ teamId, projectId, bugId }: { teamId: string; projectId: string; bugId: string }) {
  const { t } = useTranslation()
  const query = useQuery({ queryKey: qk.bugHistory(teamId, projectId, bugId), queryFn: () => bugsApi.history(teamId, projectId, bugId) })
  return (
    <QueryBoundary query={query}>
      {(data) =>
        data.items.length === 0 ? (
          <p className="small text-body-secondary mb-0">{t('history.empty')}</p>
        ) : (
          <ul className="list-unstyled small mb-0">
            {data.items.map((h, i) => (
              <li key={`${h.changedAt}-${i}`} className="mb-3">
                <div className="d-flex flex-wrap align-items-center gap-1">
                  <span className="fw-semibold">{h.changedByName}</span>
                  {h.fromStatus && <><BugStatusBadge status={h.fromStatus} /> →</>}
                  <BugStatusBadge status={h.toStatus} />
                </div>
                {h.note && <div className="task-description fst-italic">“{h.note}”</div>}
                <div className="text-body-secondary">{formatDateTime(h.changedAt)}</div>
              </li>
            ))}
          </ul>
        )
      }
    </QueryBoundary>
  )
}

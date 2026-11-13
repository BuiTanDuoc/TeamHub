import { cilPencil, cilTrash } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { CButton, CDropdown, CDropdownItem, CDropdownMenu, CDropdownToggle, CFormTextarea, CSpinner } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { commentsApi, teamsApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { TaskComment } from '@/shared/api/types'
import { ConfirmModal } from '@/shared/components/ConfirmModal'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { useApiError } from '@/shared/hooks/useApiError'
import { formatDateTime } from '@/shared/utils/format'

const MAX_LENGTH = 5000 // khớp độ dài cột Body (AddCommentValidator)
const MAX_MENTIONS = 20 // khớp AddCommentValidator.MaxMentions

interface Props {
  teamId: string
  projectId: string
  taskId: string
  meId: string | undefined
  /** Từ Tester trở lên được viết bình luận. */
  canContribute: boolean
  /** TeamLead/SystemAdmin được xóa bình luận của người khác. */
  canManage: boolean
}

export function CommentsSection({ teamId, projectId, taskId, meId, canContribute, canManage }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const key = qk.taskComments(teamId, projectId, taskId)

  const [draft, setDraft] = useState('')
  // @mention: Id → tên đã chọn; chỉ gửi những người mà "@Tên" còn nằm trong nội dung khi bấm gửi
  const [mentions, setMentions] = useState<Record<string, string>>({})
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editText, setEditText] = useState('')
  const [deleting, setDeleting] = useState<TaskComment | null>(null)

  const teamMembers = useQuery({ queryKey: qk.teamMembers(teamId), queryFn: () => teamsApi.members(teamId), enabled: canContribute })
  const query = useQuery({ queryKey: key, queryFn: () => commentsApi.list(teamId, projectId, taskId) })
  const refresh = () => queryClient.invalidateQueries({ queryKey: key })

  const add = useMutation({
    mutationFn: ({ body, mentionedUserIds }: { body: string; mentionedUserIds: string[] }) =>
      commentsApi.add(teamId, projectId, taskId, body, mentionedUserIds),
    onSuccess: async () => {
      setDraft('')
      setMentions({})
      await refresh()
    },
    onError: showError,
  })

  const edit = useMutation({
    mutationFn: ({ id, body }: { id: string; body: string }) => commentsApi.edit(teamId, projectId, taskId, id, body),
    onSuccess: async () => {
      setEditingId(null)
      await refresh()
    },
    onError: showError,
  })

  const remove = useMutation({
    mutationFn: (id: string) => commentsApi.remove(teamId, projectId, taskId, id),
    onSuccess: async () => {
      setDeleting(null)
      await refresh()
    },
    onError: (e) => {
      setDeleting(null)
      showError(e)
    },
  })

  const submit = () => {
    const body = draft.trim()
    if (!body) return
    const mentionedUserIds = Object.entries(mentions)
      .filter(([, name]) => body.includes(`@${name}`))
      .map(([id]) => id)
      .slice(0, MAX_MENTIONS)
    add.mutate({ body, mentionedUserIds })
  }

  const mention = (id: string, name: string) => {
    setMentions((m) => ({ ...m, [id]: name }))
    setDraft((d) => `${d}${d && !d.endsWith(' ') ? ' ' : ''}@${name} `)
  }

  return (
    <div>
      <h3 className="h6 mb-3">{t('comments.title')}</h3>

      <QueryBoundary query={query}>
        {(data) => (
          <>
            {data.items.length === 0 && <p className="text-body-secondary small">{t('comments.empty')}</p>}
            <div className="d-flex flex-column gap-3 mb-3">
              {data.items.map((c) => {
                const isAuthor = c.authorId === meId
                const isEditing = editingId === c.id
                return (
                  <div key={c.id} className="d-flex gap-2">
                    <UserAvatar name={c.authorName} src={c.authorAvatarUrl} size="sm" />
                    <div className="flex-grow-1">
                      <div className="d-flex flex-wrap align-items-center gap-2">
                        <span className="fw-semibold">{c.authorName}</span>
                        <span className="small text-body-secondary">{formatDateTime(c.createdAt)}</span>
                        {c.editedAt && <span className="small text-body-secondary">({t('comments.edited')})</span>}
                        <span className="ms-auto d-flex gap-1">
                          {isAuthor && canContribute && !isEditing && (
                            <CButton color="secondary" variant="ghost" size="sm" title={t('common.edit')} onClick={() => { setEditingId(c.id); setEditText(c.body) }}>
                              <CIcon icon={cilPencil} size="sm" />
                            </CButton>
                          )}
                          {(isAuthor || canManage) && (
                            <CButton color="danger" variant="ghost" size="sm" title={t('common.delete')} onClick={() => setDeleting(c)}>
                              <CIcon icon={cilTrash} size="sm" />
                            </CButton>
                          )}
                        </span>
                      </div>
                      {isEditing ? (
                        <div className="mt-1">
                          <CFormTextarea rows={3} value={editText} maxLength={MAX_LENGTH} onChange={(e) => setEditText(e.target.value)} />
                          <div className="d-flex gap-2 mt-2">
                            <CButton size="sm" color="primary" disabled={!editText.trim() || edit.isPending} onClick={() => edit.mutate({ id: c.id, body: editText.trim() })}>
                              {edit.isPending && <CSpinner size="sm" className="me-1" />}
                              {t('common.save')}
                            </CButton>
                            <CButton size="sm" color="secondary" variant="outline" onClick={() => setEditingId(null)}>
                              {t('common.cancel')}
                            </CButton>
                          </div>
                        </div>
                      ) : (
                        <div className="comment-body">{c.body}</div>
                      )}
                    </div>
                  </div>
                )
              })}
            </div>
          </>
        )}
      </QueryBoundary>

      {canContribute ? (
        <div>
          <CFormTextarea
            rows={3} value={draft} maxLength={MAX_LENGTH} placeholder={t('comments.placeholder')} aria-label={t('comments.placeholder')}
            onChange={(e) => setDraft(e.target.value)}
            onKeyDown={(e) => (e.ctrlKey || e.metaKey) && e.key === 'Enter' && submit()}
          />
          <div className="d-flex justify-content-between align-items-center mt-2">
            <span className="d-flex align-items-center gap-2">
              <CDropdown>
                <CDropdownToggle color="secondary" variant="outline" size="sm" caret={false} disabled={!teamMembers.data?.items.length}>
                  @ {t('comments.mention')}
                </CDropdownToggle>
                <CDropdownMenu style={{ maxHeight: 240, overflowY: 'auto' }}>
                  {teamMembers.data?.items.filter((m) => m.userId !== meId).map((m) => (
                    <CDropdownItem key={m.userId} as="button" type="button" onClick={() => mention(m.userId, m.fullName)}>
                      {m.fullName}
                    </CDropdownItem>
                  ))}
                </CDropdownMenu>
              </CDropdown>
              <span className="small text-body-secondary d-none d-md-inline">{t('comments.submitHint')}</span>
            </span>
            <CButton color="primary" size="sm" disabled={!draft.trim() || add.isPending} onClick={submit}>
              {add.isPending && <CSpinner size="sm" className="me-1" />}
              {t('comments.send')}
            </CButton>
          </div>
        </div>
      ) : (
        <p className="small text-body-secondary">{t('comments.readOnly')}</p>
      )}

      <ConfirmModal
        visible={!!deleting}
        title={t('comments.delete')}
        message={t('comments.deleteConfirm')}
        confirmLabel={t('common.delete')}
        busy={remove.isPending}
        onClose={() => setDeleting(null)}
        onConfirm={() => deleting && remove.mutate(deleting.id)}
      />
    </div>
  )
}

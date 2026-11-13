import { DndContext, DragOverlay, PointerSensor, TouchSensor, useDndContext, useDroppable, useSensor, useSensors, type DragEndEvent, type DragStartEvent } from '@dnd-kit/core'
import { CBadge, CFormSelect } from '@coreui/react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { projectsApi, tasksApi } from '@/shared/api/endpoints'
import { qk } from '@/shared/api/queryKeys'
import type { KanbanBoard, KanbanColumn, Task, WorkItemStatus } from '@/shared/api/types'
import { TASK_STATUS_COLOR } from '@/shared/components/Badges'
import { QueryBoundary } from '@/shared/components/QueryBoundary'
import { useApiError } from '@/shared/hooks/useApiError'
import { toast } from '@/shared/stores/uiStore'
import { TaskCard, TaskCardBody } from './TaskCard'
import { canTransition } from './workflow'

interface ColumnProps {
  column: KanbanColumn
  draggable: boolean
  onOpen: (taskId: string) => void
}

function BoardColumn({ column, draggable, onOpen }: ColumnProps) {
  const { t } = useTranslation()
  const { setNodeRef, isOver } = useDroppable({ id: column.status })
  const { active } = useDndContext()
  const dragged = (active?.data.current as { task?: Task } | undefined)?.task

  // Tô sáng theo workflow: cột hợp lệ (xanh), không hợp lệ (đỏ) khi thẻ đang bay trên cột đó
  const allowed = dragged ? canTransition(dragged.status, column.status) : false
  const classes = [
    'kanban-column',
    dragged && dragged.status !== column.status && allowed ? 'is-droppable' : '',
    isOver && dragged && dragged.status !== column.status ? (allowed ? 'is-over-allowed' : 'is-over-denied') : '',
  ].join(' ')

  return (
    <div ref={setNodeRef} className={classes}>
      <div className="d-flex justify-content-between align-items-center p-2 border-bottom">
        <span className="fw-semibold">{t(`enums.taskStatus.${column.status}`)}</span>
        <CBadge color={TASK_STATUS_COLOR[column.status]} shape="rounded-pill">
          {column.totalCount}
        </CBadge>
      </div>
      <div className="p-2 d-flex flex-column gap-2">
        {column.cards.map((task) => (
          <TaskCard key={task.id} task={task} draggable={draggable} onOpen={onOpen} />
        ))}
        {column.cards.length === 0 && <div className="text-center text-body-secondary small py-3">{t('board.emptyColumn')}</div>}
        {column.totalCount > column.cards.length && (
          <div className="text-center text-body-secondary small">{t('board.moreInList', { count: column.totalCount - column.cards.length })}</div>
        )}
      </div>
    </div>
  )
}

interface Props {
  teamId: string
  projectId: string
  canContribute: boolean
  onOpen: (taskId: string) => void
}

export function BoardView({ teamId, projectId, canContribute, onOpen }: Props) {
  const { t } = useTranslation()
  const queryClient = useQueryClient()
  const { showError } = useApiError()
  const [assigneeId, setAssigneeId] = useState('')
  const [activeTask, setActiveTask] = useState<Task | null>(null)

  const boardKey = qk.board(teamId, projectId, assigneeId || undefined)
  const board = useQuery({
    queryKey: boardKey,
    queryFn: () => tasksApi.board(teamId, projectId, assigneeId || undefined),
    refetchInterval: 30_000, // BoardHub chưa có sự kiện đẩy; thông báo realtime + polling nhẹ giữ board gần realtime
  })
  const members = useQuery({ queryKey: qk.projectMembers(teamId, projectId), queryFn: () => projectsApi.members(teamId, projectId) })

  // Kéo thả cần dịch chuyển tối thiểu để phân biệt với click mở chi tiết
  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 6 } }),
    useSensor(TouchSensor, { activationConstraint: { delay: 200, tolerance: 6 } }),
  )

  const move = useMutation({
    mutationFn: ({ task, to }: { task: Task; to: WorkItemStatus }) => tasksApi.changeStatus(teamId, projectId, task.id, to),
    // Cập nhật lạc quan: thẻ nhảy sang cột mới ngay, lỗi thì hoàn tác
    onMutate: async ({ task, to }) => {
      await queryClient.cancelQueries({ queryKey: boardKey })
      const previous = queryClient.getQueryData<KanbanBoard>(boardKey)
      if (previous) {
        queryClient.setQueryData<KanbanBoard>(boardKey, {
          ...previous,
          columns: previous.columns.map((col) => {
            if (col.status === task.status) {
              return { ...col, totalCount: Math.max(0, col.totalCount - 1), cards: col.cards.filter((c) => c.id !== task.id) }
            }
            if (col.status === to) {
              return { ...col, totalCount: col.totalCount + 1, cards: [{ ...task, status: to }, ...col.cards] }
            }
            return col
          }),
        })
      }
      return { previous }
    },
    onError: (error, _vars, context) => {
      if (context?.previous) queryClient.setQueryData(boardKey, context.previous)
      showError(error)
    },
    onSettled: () => void queryClient.invalidateQueries({ queryKey: qk.project(teamId, projectId) }),
  })

  const onDragStart = (e: DragStartEvent) => setActiveTask((e.active.data.current as { task?: Task } | undefined)?.task ?? null)

  const onDragEnd = (e: DragEndEvent) => {
    setActiveTask(null)
    const task = (e.active.data.current as { task?: Task } | undefined)?.task
    const to = e.over?.id as WorkItemStatus | undefined
    if (!task || !to || to === task.status) return
    if (!canTransition(task.status, to)) {
      toast.error(t('board.invalidTransition', { from: t(`enums.taskStatus.${task.status}`), to: t(`enums.taskStatus.${to}`) }))
      return
    }
    move.mutate({ task, to })
  }

  return (
    <>
      <div className="d-flex flex-wrap align-items-center gap-3 mb-3">
        <CFormSelect style={{ width: 240 }} value={assigneeId} aria-label={t('tasks.assignee')} onChange={(e) => setAssigneeId(e.target.value)}>
          <option value="">{t('board.allAssignees')}</option>
          {members.data?.items.map((m) => (
            <option key={m.userId} value={m.userId}>
              {m.fullName}
            </option>
          ))}
        </CFormSelect>
        {canContribute && <span className="small text-body-secondary">{t('board.dragHint')}</span>}
      </div>

      <QueryBoundary query={board}>
        {(data) => (
          <DndContext sensors={sensors} onDragStart={onDragStart} onDragEnd={onDragEnd} onDragCancel={() => setActiveTask(null)}>
            <div className="kanban">
              {data.columns.map((column) => (
                <BoardColumn key={column.status} column={column} draggable={canContribute} onOpen={onOpen} />
              ))}
            </div>
            <DragOverlay>
              {activeTask ? (
                <div className={`kanban-card is-overlay priority-${activeTask.priority}`}>
                  <TaskCardBody task={activeTask} />
                </div>
              ) : null}
            </DragOverlay>
          </DndContext>
        )}
      </QueryBoundary>
    </>
  )
}

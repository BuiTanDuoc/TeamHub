import { cilCalendar, cilClock } from '@coreui/icons'
import CIcon from '@coreui/icons-react'
import { useDraggable } from '@dnd-kit/core'
import { memo } from 'react'
import type { Task } from '@/shared/api/types'
import { PriorityBadge, TypeBadge } from '@/shared/components/Badges'
import { UserAvatar } from '@/shared/components/UserAvatar'
import { formatDate, todayDateOnly } from '@/shared/utils/format'

interface CardBodyProps {
  task: Task
}

/** Nội dung thẻ (dùng cả cho thẻ thật và DragOverlay). */
export function TaskCardBody({ task }: CardBodyProps) {
  const overdue = !!task.dueDate && task.status !== 'Done' && task.dueDate < todayDateOnly()
  return (
    <>
      <div className="d-flex justify-content-between align-items-start gap-2 mb-1">
        <span className="small text-body-secondary fw-semibold">{task.code}</span>
        <TypeBadge type={task.type} />
      </div>
      <div className="mb-2" style={{ wordBreak: 'break-word' }}>{task.title}</div>
      <div className="d-flex justify-content-between align-items-center">
        <div className="d-flex align-items-center gap-2 small text-body-secondary">
          <PriorityBadge priority={task.priority} />
          {task.dueDate && (
            <span className={overdue ? 'text-danger fw-semibold' : ''} title={formatDate(task.dueDate)}>
              <CIcon icon={cilCalendar} size="sm" className="me-1" />
              {formatDate(task.dueDate)}
            </span>
          )}
          {task.estimateHours != null && (
            <span>
              <CIcon icon={cilClock} size="sm" className="me-1" />
              {task.estimateHours}h
            </span>
          )}
        </div>
        {task.assigneeName && <UserAvatar name={task.assigneeName} size="sm" />}
      </div>
    </>
  )
}

interface Props {
  task: Task
  draggable: boolean
  onOpen: (taskId: string) => void
}

function TaskCardComponent({ task, draggable, onOpen }: Props) {
  const { attributes, listeners, setNodeRef, isDragging } = useDraggable({ id: task.id, data: { task }, disabled: !draggable })
  return (
    <div
      ref={setNodeRef}
      className={`kanban-card priority-${task.priority} ${isDragging ? 'is-dragging' : ''}`}
      style={draggable ? undefined : { cursor: 'pointer', touchAction: 'auto' }}
      onClick={() => onOpen(task.id)}
      onKeyDown={(e) => e.key === 'Enter' && onOpen(task.id)}
      {...attributes}
      {...listeners}
    >
      <TaskCardBody task={task} />
    </div>
  )
}

export const TaskCard = memo(TaskCardComponent)

// src/constants/permissions.js
import { RequestStatus } from './dataConst'

export const RequestPermissions = {
  canEdit: status =>
    [RequestStatus.DRAFT, RequestStatus.EXECUTIVE_REJECTED].includes(status),

  canExecutiveReview: status =>
    status === RequestStatus.SUBMITTED,

  canSupervisionReview: status =>
    [RequestStatus.EXECUTIVE_APPROVED, RequestStatus.OBJECTION_SUBMITTED].includes(status),

  canAdvertise: status =>
    status === RequestStatus.SUPERVISION_APPROVED,

  canSubmitObjection: status =>
    status === RequestStatus.SUPERVISION_REJECTED
}

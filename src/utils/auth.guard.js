import store from '../store'
import { isAuthGuardActive } from '../constants/config'

export default (to, from, next) => {
  if (!to.matched.some(r => r.meta.loginRequired)) {
    return next()
  }

  if (!isAuthGuardActive) {
    return next()
  }

  const user = store.getters.currentUser
  const requestStatus = store.getters.requestStatus

  if (!user) {
    return next('/unauthorized')
  }

  // 🔐 Role check
  const requiredRoles = to.matched
    .filter(r => r.meta.roles)
    .flatMap(r => r.meta.roles)

  if (
    requiredRoles.length &&
    !requiredRoles.some(r => user.roles.includes(r))
  ) {
    return next('/unauthorized')
  }
 const requiresProvinceSupervisor = to.matched.some(r => r.meta.provinceSupervisorOnly)
  if (
    requiresProvinceSupervisor &&
    !user.roles.includes('ADMIN') &&
    !(user.roles.includes('SUPERVISOR') && String(user.regionId || '').endsWith('00'))
  ) {
    return next('/unauthorized')
  }
  // 🔁 RequestStatus check
  const allowedStatuses = to.matched
    .filter(r => r.meta.allowedStatuses)
    .flatMap(r => r.meta.allowedStatuses)

  if (
    allowedStatuses.length &&
    !allowedStatuses.includes(requestStatus)
  ) {
    return next('/unauthorized')
  }

  next()
}

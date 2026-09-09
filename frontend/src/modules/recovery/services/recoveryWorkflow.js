export function apiErrorMessage(error, fallback = 'The Recovery request could not be completed. Please try again.') {
  const data = error?.response?.data
  for (const message of [data?.detail, data?.title]) {
    if (typeof message === 'string' && message.length <= 500 &&
        !/https?:|www\.|[\\/]|\bat\s|exception|stack|token|authorization|header|bearer|password|secret|connectionstring/i.test(message)) return message
  }
  return fallback
}

export function optionUnavailable(option, item) {
  if (!item?.id || !Number.isInteger(item.version) || item.version < 1) return 'Reload the case before selecting an option.'
  if (!option?.id || !Number.isInteger(option.version) || option.version < 1 || option.caseRevision !== item.revision) return 'This option is stale or incomplete. Replan the case.'
  if (!['Validated', 'Selected'].includes(option.status) || !option.estimate) return 'This option has not been validated.'
  if (typeof option.requiresPartner !== 'boolean' || typeof option.requiresPickup !== 'boolean') return 'Integration requirements are unavailable.'
  const match = option.integration?.match
  const pickup = option.integration?.pickup
  if (option.requiresPartner && (!match?.matchId || match.recoveryOptionId !== option.id || !(match.version > 0) || !match.freshnessToken || match.eligibility !== 'Eligible' || match.response !== 'Accepted')) return 'An eligible, accepted partner match is required.'
  if (option.requiresPickup && (!pickup?.pickupPlanId || !match?.matchId || pickup.matchId !== match.matchId || !(pickup.version > 0) || !pickup.freshnessToken || pickup.feasibility !== 'Feasible')) return 'A feasible pickup plan is required.'
  return ''
}

export function proposalPayload(item, option, expiresAt, explanation) {
  const match = option.integration?.match
  const pickup = option.integration?.pickup
  return {
    expectedVersion: item.version,
    optionId: option.id,
    optionVersion: option.version,
    match: option.requiresPartner ? { id: match.matchId, version: match.version, freshnessToken: match.freshnessToken } : null,
    pickup: option.requiresPickup ? { id: pickup.pickupPlanId, version: pickup.version, freshnessToken: pickup.freshnessToken } : null,
    expiresAt: new Date(expiresAt).toISOString(),
    explanation: explanation.trim(),
  }
}

export function validDate(value) {
  const date = value ? new Date(value) : null
  return date && Number.isFinite(date.getTime()) ? date : null
}

export function localDateInput(value) {
  const date = validDate(value)
  if (!date) return ''
  return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 16)
}

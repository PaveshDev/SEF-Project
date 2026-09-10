export const routes = ['Reuse', 'Donate', 'RepairThenReuse', 'Resell', 'Recycle']
export const isUuid = value => /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value ?? '') && !/^0{8}-0{4}-0{4}-0{4}-0{12}$/.test(value)
export const isMoney = value => /^(0|[1-9]\d{0,9})(\.\d{1,2})?$/.test(String(value)) && Number(value) <= 9999999999.99
export const currencyValid = value => /^[A-Z]{3}$/.test(value)
export function caseErrors(value, now = Date.now()) {
  const errors = {}
  if (!isUuid(value.itemId)) errors.itemId = 'Enter the nonempty item UUID supplied by Items.'
  if (!value.objective?.trim() || value.objective.trim().length > 1000) errors.objective = 'Enter an objective of 1–1,000 characters.'
  if (!value.preferredRoutes?.length || value.preferredRoutes.length > 5 || new Set(value.preferredRoutes).size !== value.preferredRoutes.length || value.preferredRoutes.some(x => !routes.includes(x))) errors.preferredRoutes = 'Choose between one and five distinct routes.'
  if (!currencyValid(value.currency)) errors.currency = 'Use three uppercase letters, for example LKR.'
  if (value.budget !== '' && value.budget != null && !isMoney(value.budget)) errors.budget = 'Enter a nonnegative amount up to 9,999,999,999.99 with at most two decimals.'
  if (value.deadline && !(Date.parse(value.deadline) > now)) errors.deadline = 'Choose a future date and time in your local timezone.'
  return errors
}
export function referenceErrors(value, now = Date.now()) {
  const errors = {}
  if (!isUuid(value.categoryId)) errors.categoryId = 'Enter a valid nonempty category UUID.'
  for (const key of ['valueLow', 'valueHigh']) if (!isMoney(value[key])) errors[key] = 'Enter a nonnegative amount with at most two decimals.'
  if (!errors.valueLow && !errors.valueHigh && Number(value.valueLow) > Number(value.valueHigh)) errors.valueHigh = 'High value must be at least the low value.'
  if (!currencyValid(value.currency)) errors.currency = 'Use three uppercase letters.'
  if (!value.sourceName?.trim() || value.sourceName.trim().length > 200) errors.sourceName = 'Enter a source name of 1–200 characters.'
  if ((value.sourceReference?.length ?? 0) > 1000) errors.sourceReference = 'Use at most 1,000 characters.'
  if (!Number.isFinite(Date.parse(value.observedAt)) || Date.parse(value.observedAt) > now) errors.observedAt = 'Choose an observation time that is not in the future.'
  return errors
}
export function fieldErrors(error) {
  const result = {}
  const aliases = { maximumPickupCost: 'budget' }
  for (const path of Object.keys(error?.response?.data?.errors ?? {})) {
    const name = path.split('.').at(-1)
    const key = name ? name[0].toLowerCase() + name.slice(1) : ''
    if (key) result[aliases[key] ?? key] = 'The server rejected this field. Check its format and allowed value.'
  }
  return result
}
export const caseActions = status => ({
  edit: ['Draft', 'RevisionRequested', 'Planning', 'AwaitingInputs', 'AwaitingApproval'].includes(status),
  plan: ['Draft', 'Planning', 'AwaitingApproval', 'RevisionRequested', 'AwaitingInputs', 'Failed'].includes(status),
  cancel: ['Draft', 'Planning', 'AwaitingInputs', 'AwaitingApproval', 'RevisionRequested'].includes(status),
  delete: ['Draft', 'Rejected', 'Failed', 'Cancelled'].includes(status),
})
export const humanize = value => (value ?? '').replace(/([a-z])([A-Z])/g, '$1 $2')

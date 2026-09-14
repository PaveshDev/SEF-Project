import test from 'node:test'
import assert from 'node:assert/strict'
import { caseActions, caseErrors, currencyValid, isMoney, isUuid, referenceErrors } from '../services/recoveryValidation.js'

test('identifiers and currency reject malformed or empty identities', () => {
  assert.equal(isUuid('00000000-0000-0000-0000-000000000000'), false)
  assert.equal(isUuid('not-an-item'), false)
  assert.equal(currencyValid('12!'), false)
  assert.equal(currencyValid('LKR'), true)
})
test('money distinguishes absent optional input from invalid numbers', () => {
  for (const value of ['NaN', 'Infinity', '-1', '1.001', '10000000000', '1e2', '']) assert.equal(isMoney(value), false)
  for (const value of ['0', '20.50', '9999999999.99']) assert.equal(isMoney(value), true)
})
test('objective-only edits retain all valid routes', () => {
  const value = { itemId: '10000000-0000-0000-0000-000000000001', objective: 'Changed objective', preferredRoutes: ['Reuse', 'Donate'], currency: 'LKR', budget: '', deadline: '' }
  assert.deepEqual(caseErrors(value), {})
  assert.deepEqual(value.preferredRoutes, ['Reuse', 'Donate'])
  assert.ok(caseErrors({ ...value, preferredRoutes: ['Reuse', 'Reuse'] }).preferredRoutes)
  assert.ok(caseErrors({ ...value, objective: '   ' }).objective)
  assert.ok(caseErrors({ ...value, deadline: '2020-01-01' }).deadline)
})
test('reference ranges and future observation are rejected', () => {
  const errors = referenceErrors({ categoryId: '10000000-0000-0000-0000-000000000001', valueLow: '20', valueHigh: '10', currency: 'LKR', sourceName: 'Source', observedAt: '2035-01-01T00:00:00Z' }, Date.parse('2026-09-09'))
  assert.ok(errors.valueHigh)
  assert.ok(errors.observedAt)
})
test('client actions match case transitions', () => {
  assert.deepEqual(caseActions('Approved'), { edit: false, plan: false, cancel: false, delete: false })
  assert.equal(caseActions('AwaitingApproval').edit, true)
  assert.equal(caseActions('AwaitingApproval').plan, true)
  assert.equal(caseActions('InventedStatus').plan, false)
})

/**
 * @typedef {Object} ItemResponse
 * @property {string} id
 * @property {string} ownerId
 * @property {string} title
 * @property {string} description
 * @property {string} category
 * @property {string} locationArea
 * @property {string} status
 * @property {number} version
 * @property {string} createdAt
 * @property {string} updatedAt
 * @property {PhotoResponse[]} photos
 * @property {ConditionAnswerResponse[]} conditionAnswers
 * @property {AssessmentResponse[]} assessments
 */

/**
 * @typedef {Object} PhotoResponse
 * @property {string} id
 * @property {string} itemId
 * @property {string} imageUrl
 * @property {number} photoOrder
 */

/**
 * @typedef {Object} ConditionAnswerResponse
 * @property {string} id
 * @property {string} questionCode
 * @property {string} questionText
 * @property {string} answer
 */

/**
 * @typedef {Object} AssessmentResponse
 * @property {string} id
 * @property {string} status
 * @property {number} version
 * @property {string} suggestedCategory
 * @property {string} conditionGrade
 * @property {string} conditionSummary
 * @property {string} visibleObservations
 * @property {string} ownerReportedFunctionality
 * @property {string} missingInformation
 * @property {number} confidence
 * @property {ClarificationResponse[]} clarifications
 */

/**
 * @typedef {Object} ClarificationResponse
 * @property {string} id
 * @property {string} questionText
 * @property {string} answer
 */

import { useState } from 'react';
import { itemsApi } from '../services/itemsApi';

export function useAssessment(itemId) {
  const [isSubmitting, setIsSubmitting] = useState(false);

  const answerClarification = async (clarificationId, answerStr, onSuccess) => {
    setIsSubmitting(true);
    try {
      await itemsApi.answerClarification(itemId, clarificationId, { answer: answerStr });
      if (onSuccess) onSuccess();
    } finally {
      setIsSubmitting(false);
    }
  };

  const confirmAssessment = async (assessmentId, onSuccess) => {
    setIsSubmitting(true);
    try {
      await itemsApi.confirmAssessment(itemId, assessmentId, { ownerConfirmation: true });
      if (onSuccess) onSuccess();
    } finally {
      setIsSubmitting(false);
    }
  };

  const requestReassessment = async (assessmentId, reason, onSuccess) => {
    setIsSubmitting(true);
    try {
      await itemsApi.requestReassessment(itemId, assessmentId, { reason });
      if (onSuccess) onSuccess();
    } finally {
      setIsSubmitting(false);
    }
  };

  return { isSubmitting, answerClarification, confirmAssessment, requestReassessment };
}

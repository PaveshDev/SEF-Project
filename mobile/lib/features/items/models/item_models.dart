class ItemSummary {
  final String id;
  final String title;
  final String category;
  final String locationArea;
  final String status;
  final DateTime createdAt;

  ItemSummary({
    required this.id,
    required this.title,
    required this.category,
    required this.locationArea,
    required this.status,
    required this.createdAt,
  });

  factory ItemSummary.fromJson(Map<String, dynamic> json) {
    return ItemSummary(
      id: json['id'] as String,
      title: json['title'] as String,
      category: json['category'] as String,
      locationArea: json['locationArea'] as String,
      status: json['status'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }
}

class Item {
  final String id;
  final String ownerId;
  final String title;
  final String description;
  final String category;
  final String locationArea;
  final String status;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final int version;
  final List<ItemPhoto> photos;
  final List<ItemConditionAnswer> conditionAnswers;
  final List<ItemAssessment> assessments;

  Item({
    required this.id,
    required this.ownerId,
    required this.title,
    required this.description,
    required this.category,
    required this.locationArea,
    required this.status,
    required this.createdAt,
    this.updatedAt,
    required this.version,
    required this.photos,
    required this.conditionAnswers,
    required this.assessments,
  });

  factory Item.fromJson(Map<String, dynamic> json) {
    return Item(
      id: json['id'] as String,
      ownerId: json['ownerId'] as String,
      title: json['title'] as String,
      description: json['description'] as String,
      category: json['category'] as String,
      locationArea: json['locationArea'] as String,
      status: json['status'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] != null ? DateTime.parse(json['updatedAt'] as String) : null,
      version: json['version'] as int,
      photos: (json['photos'] as List<dynamic>?)
              ?.map((e) => ItemPhoto.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      conditionAnswers: (json['conditionAnswers'] as List<dynamic>?)
              ?.map((e) => ItemConditionAnswer.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      assessments: (json['assessments'] as List<dynamic>?)
              ?.map((e) => ItemAssessment.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class ItemPhoto {
  final String id;
  final String itemId;
  final String imageUrl;
  final DateTime createdAt;

  ItemPhoto({
    required this.id,
    required this.itemId,
    required this.imageUrl,
    required this.createdAt,
  });

  factory ItemPhoto.fromJson(Map<String, dynamic> json) {
    return ItemPhoto(
      id: json['id'] as String,
      itemId: json['itemId'] as String,
      imageUrl: json['imageUrl'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }
}

class ItemConditionAnswer {
  final String id;
  final String itemId;
  final String questionCode;
  final String answer;
  final DateTime createdAt;

  ItemConditionAnswer({
    required this.id,
    required this.itemId,
    required this.questionCode,
    required this.answer,
    required this.createdAt,
  });

  factory ItemConditionAnswer.fromJson(Map<String, dynamic> json) {
    return ItemConditionAnswer(
      id: json['id'] as String,
      itemId: json['itemId'] as String,
      questionCode: json['questionCode'] as String,
      answer: json['answer'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }
}

class ItemAssessment {
  final String id;
  final String itemId;
  final int version;
  final String? suggestedCategory;
  final String? conditionGrade;
  final String? conditionSummary;
  final String? visibleObservations;
  final String? ownerReportedFunctionality;
  final String? missingInformation;
  final double confidence;
  final String status;
  final DateTime createdAt;
  final DateTime? updatedAt;
  final List<AssessmentEvidence> evidences;
  final List<AssessmentClarification> clarifications;

  ItemAssessment({
    required this.id,
    required this.itemId,
    required this.version,
    this.suggestedCategory,
    this.conditionGrade,
    this.conditionSummary,
    this.visibleObservations,
    this.ownerReportedFunctionality,
    this.missingInformation,
    required this.confidence,
    required this.status,
    required this.createdAt,
    this.updatedAt,
    required this.evidences,
    required this.clarifications,
  });

  factory ItemAssessment.fromJson(Map<String, dynamic> json) {
    return ItemAssessment(
      id: json['id'] as String,
      itemId: json['itemId'] as String,
      version: json['version'] as int,
      suggestedCategory: json['suggestedCategory'] as String?,
      conditionGrade: json['conditionGrade'] as String?,
      conditionSummary: json['conditionSummary'] as String?,
      visibleObservations: json['visibleObservations'] as String?,
      ownerReportedFunctionality: json['ownerReportedFunctionality'] as String?,
      missingInformation: json['missingInformation'] as String?,
      confidence: (json['confidence'] as num).toDouble(),
      status: json['status'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] != null ? DateTime.parse(json['updatedAt'] as String) : null,
      evidences: (json['evidences'] as List<dynamic>?)
              ?.map((e) => AssessmentEvidence.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      clarifications: (json['clarifications'] as List<dynamic>?)
              ?.map((e) => AssessmentClarification.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class AssessmentEvidence {
  final String id;
  final String assessmentId;
  final String evidenceType;
  final String description;
  final String? sourceUrl;
  final DateTime createdAt;

  AssessmentEvidence({
    required this.id,
    required this.assessmentId,
    required this.evidenceType,
    required this.description,
    this.sourceUrl,
    required this.createdAt,
  });

  factory AssessmentEvidence.fromJson(Map<String, dynamic> json) {
    return AssessmentEvidence(
      id: json['id'] as String,
      assessmentId: json['assessmentId'] as String,
      evidenceType: json['evidenceType'] as String,
      description: json['description'] as String,
      sourceUrl: json['sourceUrl'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }
}

class AssessmentClarification {
  final String id;
  final String assessmentId;
  final String question;
  final String reason;
  final String status;
  final String? answer;
  final DateTime createdAt;
  final DateTime? answeredAt;

  AssessmentClarification({
    required this.id,
    required this.assessmentId,
    required this.question,
    required this.reason,
    required this.status,
    this.answer,
    required this.createdAt,
    this.answeredAt,
  });

  factory AssessmentClarification.fromJson(Map<String, dynamic> json) {
    return AssessmentClarification(
      id: json['id'] as String,
      assessmentId: json['assessmentId'] as String,
      question: json['question'] as String,
      reason: json['reason'] as String,
      status: json['status'] as String,
      answer: json['answer'] as String?,
      createdAt: DateTime.parse(json['createdAt'] as String),
      answeredAt: json['answeredAt'] != null ? DateTime.parse(json['answeredAt'] as String) : null,
    );
  }
}

// Request Models

class CreateItemRequest {
  final String title;
  final String description;
  final String category;
  final String locationArea;

  CreateItemRequest({
    required this.title,
    required this.description,
    required this.category,
    required this.locationArea,
  });

  Map<String, dynamic> toJson() => {
        'title': title,
        'description': description,
        'category': category,
        'locationArea': locationArea,
      };
}

class UpdateItemRequest {
  final String id;
  final String title;
  final String description;
  final String category;
  final String locationArea;
  final int version;

  UpdateItemRequest({
    required this.id,
    required this.title,
    required this.description,
    required this.category,
    required this.locationArea,
    required this.version,
  });

  Map<String, dynamic> toJson() => {
        'id': id,
        'title': title,
        'description': description,
        'category': category,
        'locationArea': locationArea,
        'version': version,
      };
}

class AddPhotoRequest {
  final String imageUrl;

  AddPhotoRequest({required this.imageUrl});

  Map<String, dynamic> toJson() => {'imageUrl': imageUrl};
}

class SubmitConditionAnswersRequest {
  final List<ConditionAnswerDto> answers;

  SubmitConditionAnswersRequest({required this.answers});

  Map<String, dynamic> toJson() => {
        'answers': answers.map((e) => e.toJson()).toList(),
      };
}

class ConditionAnswerDto {
  final String questionCode;
  final String questionText;
  final String answer;

  ConditionAnswerDto({
    required this.questionCode,
    required this.questionText,
    required this.answer,
  });

  Map<String, dynamic> toJson() => {
        'questionCode': questionCode,
        'questionText': questionText,
        'answer': answer,
      };
}

class AnswerClarificationRequest {
  final String answer;

  AnswerClarificationRequest({required this.answer});

  Map<String, dynamic> toJson() => {'answer': answer};
}

class ConfirmAssessmentRequest {
  final bool accepted;

  ConfirmAssessmentRequest({required this.accepted});

  Map<String, dynamic> toJson() => {'accepted': accepted};
}

class RequestReassessmentRequest {
  final String reason;

  RequestReassessmentRequest({required this.reason});

  Map<String, dynamic> toJson() => {'reason': reason};
}

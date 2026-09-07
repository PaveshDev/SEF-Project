-- Waste-to-Value initial PostgreSQL schema reference
-- Run only on an empty database and only by the database integrator.
-- Do not also apply an equivalent EF initial migration to the same database.

BEGIN;

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE app_users (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    email varchar(320) NOT NULL,
    normalized_email varchar(320) NOT NULL,
    display_name varchar(120) NOT NULL,
    password_hash text NOT NULL,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT uq_app_users_normalized_email UNIQUE (normalized_email),
    CONSTRAINT ck_app_users_version CHECK (version > 0)
);

CREATE TABLE roles (
    id smallint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name varchar(50) NOT NULL UNIQUE,
    CONSTRAINT ck_roles_name CHECK (name IN ('ItemOwner','PartnerRepresentative','CollectionStaff','Administrator'))
);

CREATE TABLE user_roles (
    user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    role_id smallint NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    created_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE categories (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    code varchar(40) NOT NULL UNIQUE,
    name varchar(100) NOT NULL,
    handling_class varchar(30) NOT NULL DEFAULT 'STANDARD',
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE items (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    owner_id uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    category_id uuid REFERENCES categories(id) ON DELETE RESTRICT,
    title varchar(160) NOT NULL,
    description varchar(2000),
    location_area varchar(160) NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'DRAFT',
    revision integer NOT NULL DEFAULT 1,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_items_status CHECK (status IN ('DRAFT','SUBMITTED','CLARIFICATION_REQUIRED','ASSESSED','CONFIRMED','WITHDRAWN')),
    CONSTRAINT ck_items_revision CHECK (revision > 0),
    CONSTRAINT ck_items_version CHECK (version > 0)
);

CREATE TABLE item_photos (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id uuid NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    storage_key varchar(500) NOT NULL,
    content_type varchar(100) NOT NULL,
    display_order smallint NOT NULL DEFAULT 0,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_item_photos_storage_key UNIQUE (storage_key),
    CONSTRAINT ck_item_photos_order CHECK (display_order >= 0)
);

CREATE TABLE item_answers (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id uuid NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    item_revision integer NOT NULL,
    question_code varchar(80) NOT NULL,
    answer_text varchar(2000) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_item_answers_revision_question UNIQUE (item_id, item_revision, question_code),
    CONSTRAINT ck_item_answers_revision CHECK (item_revision > 0)
);

CREATE TABLE assessments (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id uuid NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    item_revision integer NOT NULL,
    category_suggestion varchar(100),
    condition_grade varchar(20) NOT NULL,
    functional_status varchar(30) NOT NULL,
    visible_observations jsonb NOT NULL DEFAULT '[]'::jsonb,
    owner_reported_facts jsonb NOT NULL DEFAULT '{}'::jsonb,
    unknown_fields jsonb NOT NULL DEFAULT '[]'::jsonb,
    evidence_references jsonb NOT NULL DEFAULT '[]'::jsonb,
    confidence numeric(5,4) NOT NULL,
    status varchar(25) NOT NULL DEFAULT 'DRAFT',
    is_current boolean NOT NULL DEFAULT true,
    confirmed_by uuid REFERENCES app_users(id) ON DELETE RESTRICT,
    confirmed_at timestamptz,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_assessments_condition CHECK (condition_grade IN ('EXCELLENT','GOOD','FAIR','POOR','UNSAFE','UNKNOWN')),
    CONSTRAINT ck_assessments_function CHECK (functional_status IN ('WORKING','PARTIALLY_WORKING','NOT_WORKING','UNKNOWN')),
    CONSTRAINT ck_assessments_status CHECK (status IN ('DRAFT','CLARIFICATION_REQUIRED','READY_FOR_CONFIRMATION','CONFIRMED','SUPERSEDED','FAILED')),
    CONSTRAINT ck_assessments_confidence CHECK (confidence >= 0 AND confidence <= 1),
    CONSTRAINT ck_assessments_revision CHECK (item_revision > 0),
    CONSTRAINT ck_assessments_confirmation CHECK ((status = 'CONFIRMED' AND confirmed_by IS NOT NULL AND confirmed_at IS NOT NULL) OR status <> 'CONFIRMED')
);

CREATE UNIQUE INDEX uq_assessments_current_revision ON assessments(item_id, item_revision) WHERE is_current;

CREATE TABLE assessment_clarifications (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    assessment_id uuid NOT NULL REFERENCES assessments(id) ON DELETE CASCADE,
    question_code varchar(80) NOT NULL,
    question_text varchar(1000) NOT NULL,
    answer_text varchar(2000),
    status varchar(20) NOT NULL DEFAULT 'OPEN',
    answered_by uuid REFERENCES app_users(id) ON DELETE RESTRICT,
    answered_at timestamptz,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_clarifications_status CHECK (status IN ('OPEN','ANSWERED','CANCELLED'))
);

CREATE TABLE recovery_cases (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id uuid NOT NULL REFERENCES items(id) ON DELETE RESTRICT,
    owner_id uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    assessment_id uuid NOT NULL REFERENCES assessments(id) ON DELETE RESTRICT,
    objective varchar(1000) NOT NULL,
    preferred_routes jsonb NOT NULL DEFAULT '[]'::jsonb,
    maximum_pickup_cost numeric(12,2),
    currency char(3) NOT NULL DEFAULT 'LKR',
    deadline timestamptz,
    status varchar(30) NOT NULL DEFAULT 'DRAFT',
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_recovery_cases_cost CHECK (maximum_pickup_cost IS NULL OR maximum_pickup_cost >= 0),
    CONSTRAINT ck_recovery_cases_currency CHECK (currency ~ '^[A-Z]{3}$'),
    CONSTRAINT ck_recovery_cases_status CHECK (status IN ('DRAFT','PLANNING','AWAITING_INPUTS','AWAITING_APPROVAL','APPROVED','REJECTED','REVISION_REQUESTED','COMPLETED','FAILED','CANCELLED'))
);

CREATE UNIQUE INDEX uq_recovery_cases_active_item ON recovery_cases(item_id)
WHERE status NOT IN ('REJECTED','COMPLETED','FAILED','CANCELLED');

CREATE TABLE value_references (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id uuid NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    condition_grade varchar(20) NOT NULL,
    route_type varchar(30) NOT NULL,
    value_low numeric(12,2) NOT NULL,
    value_high numeric(12,2) NOT NULL,
    currency char(3) NOT NULL DEFAULT 'LKR',
    source_name varchar(200) NOT NULL,
    source_reference varchar(1000),
    observed_at timestamptz NOT NULL,
    is_verified boolean NOT NULL DEFAULT false,
    created_by uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_value_references_values CHECK (value_low >= 0 AND value_high >= value_low),
    CONSTRAINT ck_value_references_currency CHECK (currency ~ '^[A-Z]{3}$'),
    CONSTRAINT ck_value_references_route CHECK (route_type IN ('REUSE','DONATE','REPAIR_THEN_REUSE','RESELL','RECYCLE'))
);

CREATE TABLE recovery_options (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recovery_case_id uuid NOT NULL REFERENCES recovery_cases(id) ON DELETE CASCADE,
    assessment_id uuid NOT NULL REFERENCES assessments(id) ON DELETE RESTRICT,
    route_type varchar(30) NOT NULL,
    estimated_value_low numeric(12,2) NOT NULL DEFAULT 0,
    estimated_value_high numeric(12,2) NOT NULL DEFAULT 0,
    estimated_repair_cost numeric(12,2) NOT NULL DEFAULT 0,
    estimated_pickup_cost numeric(12,2) NOT NULL DEFAULT 0,
    estimated_net_value numeric(12,2) NOT NULL DEFAULT 0,
    currency char(3) NOT NULL DEFAULT 'LKR',
    evidence jsonb NOT NULL DEFAULT '[]'::jsonb,
    non_financial_benefits jsonb NOT NULL DEFAULT '[]'::jsonb,
    status varchar(25) NOT NULL DEFAULT 'DRAFT',
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_recovery_options_values CHECK (estimated_value_low >= 0 AND estimated_value_high >= estimated_value_low AND estimated_repair_cost >= 0 AND estimated_pickup_cost >= 0),
    CONSTRAINT ck_recovery_options_currency CHECK (currency ~ '^[A-Z]{3}$'),
    CONSTRAINT ck_recovery_options_route CHECK (route_type IN ('REUSE','DONATE','REPAIR_THEN_REUSE','RESELL','RECYCLE')),
    CONSTRAINT ck_recovery_options_status CHECK (status IN ('DRAFT','VALIDATED','SELECTED','STALE','REJECTED'))
);

CREATE TABLE partners (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    name varchar(200) NOT NULL,
    partner_type varchar(30) NOT NULL,
    verification_status varchar(20) NOT NULL DEFAULT 'PENDING',
    is_active boolean NOT NULL DEFAULT true,
    service_area varchar(300) NOT NULL,
    contact_email varchar(320),
    contact_phone varchar(40),
    capacity integer NOT NULL DEFAULT 0,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_partners_type CHECK (partner_type IN ('DONATION_ORGANIZATION','SCHOOL','REPAIR_PARTNER','RESELLER','RECYCLER')),
    CONSTRAINT ck_partners_verification CHECK (verification_status IN ('PENDING','VERIFIED','REJECTED','SUSPENDED')),
    CONSTRAINT ck_partners_capacity CHECK (capacity >= 0)
);

CREATE TABLE partner_memberships (
    partner_id uuid NOT NULL REFERENCES partners(id) ON DELETE CASCADE,
    user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    membership_role varchar(30) NOT NULL DEFAULT 'REPRESENTATIVE',
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    PRIMARY KEY (partner_id, user_id)
);

CREATE TABLE acceptance_rules (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    partner_id uuid NOT NULL REFERENCES partners(id) ON DELETE CASCADE,
    category_id uuid NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    route_type varchar(30) NOT NULL,
    minimum_condition varchar(20) NOT NULL,
    restrictions jsonb NOT NULL DEFAULT '{}'::jsonb,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_acceptance_rule UNIQUE (partner_id, category_id, route_type),
    CONSTRAINT ck_acceptance_rules_route CHECK (route_type IN ('REUSE','DONATE','REPAIR_THEN_REUSE','RESELL','RECYCLE'))
);

CREATE TABLE recipient_needs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    partner_id uuid NOT NULL REFERENCES partners(id) ON DELETE CASCADE,
    category_id uuid NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    description varchar(2000) NOT NULL,
    quantity_required integer NOT NULL,
    quantity_fulfilled integer NOT NULL DEFAULT 0,
    deadline timestamptz,
    status varchar(20) NOT NULL DEFAULT 'OPEN',
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_recipient_needs_quantity CHECK (quantity_required > 0 AND quantity_fulfilled >= 0 AND quantity_fulfilled <= quantity_required),
    CONSTRAINT ck_recipient_needs_status CHECK (status IN ('OPEN','PAUSED','FULFILLED','CLOSED','EXPIRED'))
);

CREATE TABLE matches (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recovery_option_id uuid NOT NULL REFERENCES recovery_options(id) ON DELETE CASCADE,
    partner_id uuid NOT NULL REFERENCES partners(id) ON DELETE RESTRICT,
    recipient_need_id uuid REFERENCES recipient_needs(id) ON DELETE RESTRICT,
    eligibility_status varchar(20) NOT NULL,
    partner_response_status varchar(20) NOT NULL DEFAULT 'PENDING',
    rank_score numeric(7,4),
    evidence jsonb NOT NULL DEFAULT '[]'::jsonb,
    exclusion_reasons jsonb NOT NULL DEFAULT '[]'::jsonb,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT uq_matches_option_partner UNIQUE (recovery_option_id, partner_id),
    CONSTRAINT ck_matches_eligibility CHECK (eligibility_status IN ('ELIGIBLE','INELIGIBLE','REQUIRES_REVIEW')),
    CONSTRAINT ck_matches_response CHECK (partner_response_status IN ('PENDING','ACCEPTED','REJECTED','EXPIRED')),
    CONSTRAINT ck_matches_rank CHECK (rank_score IS NULL OR (rank_score >= 0 AND rank_score <= 100))
);

CREATE TABLE partner_responses (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    match_id uuid NOT NULL REFERENCES matches(id) ON DELETE CASCADE,
    partner_id uuid NOT NULL REFERENCES partners(id) ON DELETE RESTRICT,
    responded_by uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    decision varchar(20) NOT NULL,
    comment varchar(1000),
    responded_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_partner_response_match UNIQUE (match_id),
    CONSTRAINT ck_partner_responses_decision CHECK (decision IN ('ACCEPTED','REJECTED'))
);

CREATE TABLE collection_slots (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    collector_id uuid REFERENCES app_users(id) ON DELETE RESTRICT,
    starts_at timestamptz NOT NULL,
    ends_at timestamptz NOT NULL,
    service_area varchar(300) NOT NULL,
    capacity integer NOT NULL DEFAULT 1,
    reserved_count integer NOT NULL DEFAULT 0,
    vehicle_class varchar(40),
    status varchar(20) NOT NULL DEFAULT 'AVAILABLE',
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_collection_slots_time CHECK (ends_at > starts_at),
    CONSTRAINT ck_collection_slots_capacity CHECK (capacity > 0 AND reserved_count >= 0 AND reserved_count <= capacity),
    CONSTRAINT ck_collection_slots_status CHECK (status IN ('AVAILABLE','FULL','UNAVAILABLE','CANCELLED'))
);

CREATE TABLE pickup_plans (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    match_id uuid NOT NULL REFERENCES matches(id) ON DELETE RESTRICT,
    collection_slot_id uuid REFERENCES collection_slots(id) ON DELETE RESTRICT,
    proposed_start timestamptz NOT NULL,
    proposed_end timestamptz NOT NULL,
    estimated_cost numeric(12,2) NOT NULL DEFAULT 0,
    currency char(3) NOT NULL DEFAULT 'LKR',
    handling_requirements jsonb NOT NULL DEFAULT '[]'::jsonb,
    travel_estimate jsonb,
    feasibility_status varchar(25) NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_pickup_plans_time CHECK (proposed_end > proposed_start),
    CONSTRAINT ck_pickup_plans_cost CHECK (estimated_cost >= 0),
    CONSTRAINT ck_pickup_plans_feasibility CHECK (feasibility_status IN ('FEASIBLE','INFEASIBLE','MANUAL_REVIEW','STALE'))
);

CREATE TABLE recovery_proposals (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recovery_case_id uuid NOT NULL REFERENCES recovery_cases(id) ON DELETE RESTRICT,
    recovery_option_id uuid NOT NULL REFERENCES recovery_options(id) ON DELETE RESTRICT,
    match_id uuid NOT NULL REFERENCES matches(id) ON DELETE RESTRICT,
    pickup_plan_id uuid NOT NULL REFERENCES pickup_plans(id) ON DELETE RESTRICT,
    revision integer NOT NULL DEFAULT 1,
    explanation varchar(4000) NOT NULL,
    status varchar(25) NOT NULL DEFAULT 'AWAITING_APPROVAL',
    expires_at timestamptz NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT uq_recovery_proposal_revision UNIQUE (recovery_case_id, revision),
    CONSTRAINT ck_recovery_proposals_revision CHECK (revision > 0),
    CONSTRAINT ck_recovery_proposals_status CHECK (status IN ('DRAFT','AWAITING_APPROVAL','APPROVED','REJECTED','REVISION_REQUESTED','EXPIRED','EXECUTED','STALE'))
);

CREATE TABLE proposal_decisions (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recovery_proposal_id uuid NOT NULL REFERENCES recovery_proposals(id) ON DELETE RESTRICT,
    proposal_revision integer NOT NULL,
    decided_by uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    decision_type varchar(30) NOT NULL DEFAULT 'OWNER_DECISION',
    decision varchar(25) NOT NULL,
    comment varchar(2000),
    idempotency_key varchar(100) NOT NULL,
    decided_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT uq_proposal_decision_idempotency UNIQUE (idempotency_key),
    CONSTRAINT uq_proposal_decision_actor UNIQUE (recovery_proposal_id, proposal_revision, decided_by, decision_type),
    CONSTRAINT ck_proposal_decisions_revision CHECK (proposal_revision > 0),
    CONSTRAINT ck_proposal_decisions_decision CHECK (decision IN ('APPROVED','REJECTED','REVISION_REQUESTED'))
);

CREATE TABLE pickup_requests (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recovery_proposal_id uuid NOT NULL UNIQUE REFERENCES recovery_proposals(id) ON DELETE RESTRICT,
    collection_slot_id uuid NOT NULL REFERENCES collection_slots(id) ON DELETE RESTRICT,
    collector_id uuid REFERENCES app_users(id) ON DELETE RESTRICT,
    owner_id uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    pickup_address_encrypted text NOT NULL,
    scheduled_start timestamptz NOT NULL,
    scheduled_end timestamptz NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'CONFIRMED',
    idempotency_key varchar(100) NOT NULL UNIQUE,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_pickup_requests_time CHECK (scheduled_end > scheduled_start),
    CONSTRAINT ck_pickup_requests_status CHECK (status IN ('DRAFT','PROPOSED','AWAITING_APPROVAL','CONFIRMED','ASSIGNED','COLLECTED','DELIVERED','FAILED','RESCHEDULE_REQUIRED','CANCELLED'))
);

CREATE TABLE pickup_events (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    pickup_request_id uuid NOT NULL REFERENCES pickup_requests(id) ON DELETE RESTRICT,
    event_type varchar(30) NOT NULL,
    actor_id uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    event_at timestamptz NOT NULL DEFAULT now(),
    notes varchar(2000),
    idempotency_key varchar(100) NOT NULL UNIQUE,
    CONSTRAINT ck_pickup_events_type CHECK (event_type IN ('CONFIRMED','ASSIGNED','RESCHEDULED','COLLECTED','DELIVERED','FAILED','CANCELLED'))
);

CREATE TABLE handover_proofs (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    pickup_request_id uuid NOT NULL REFERENCES pickup_requests(id) ON DELETE RESTRICT,
    pickup_event_id uuid NOT NULL REFERENCES pickup_events(id) ON DELETE RESTRICT,
    proof_type varchar(30) NOT NULL,
    storage_key varchar(500),
    verification_hash varchar(255),
    verified_by uuid REFERENCES app_users(id) ON DELETE RESTRICT,
    verified_at timestamptz,
    created_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT ck_handover_proofs_type CHECK (proof_type IN ('QR_CODE','ONE_TIME_CODE','PHOTO','SIGNATURE')),
    CONSTRAINT ck_handover_proofs_value CHECK (storage_key IS NOT NULL OR verification_hash IS NOT NULL)
);

CREATE TABLE agent_workflows (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    recovery_case_id uuid REFERENCES recovery_cases(id) ON DELETE RESTRICT,
    objective varchar(2000) NOT NULL,
    plan jsonb NOT NULL DEFAULT '[]'::jsonb,
    current_stage varchar(80),
    status varchar(30) NOT NULL DEFAULT 'CREATED',
    approval_status varchar(30) NOT NULL DEFAULT 'NOT_REQUIRED',
    final_outcome jsonb,
    error_summary jsonb,
    started_at timestamptz,
    completed_at timestamptz,
    created_by uuid NOT NULL REFERENCES app_users(id) ON DELETE RESTRICT,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    version integer NOT NULL DEFAULT 1,
    CONSTRAINT ck_agent_workflows_status CHECK (status IN ('CREATED','RUNNING','PAUSED_FOR_APPROVAL','COMPLETED','FAILED','CANCELLED')),
    CONSTRAINT ck_agent_workflows_approval CHECK (approval_status IN ('NOT_REQUIRED','PENDING','APPROVED','REJECTED','REVISION_REQUESTED'))
);

CREATE TABLE agent_steps (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    workflow_id uuid NOT NULL REFERENCES agent_workflows(id) ON DELETE CASCADE,
    agent_name varchar(80) NOT NULL,
    step_name varchar(120) NOT NULL,
    sequence_number integer NOT NULL,
    status varchar(25) NOT NULL DEFAULT 'PENDING',
    input_data jsonb NOT NULL DEFAULT '{}'::jsonb,
    output_data jsonb,
    validation_result jsonb,
    attempt_count smallint NOT NULL DEFAULT 0,
    started_at timestamptz,
    completed_at timestamptz,
    error_code varchar(80),
    error_summary varchar(2000),
    CONSTRAINT uq_agent_steps_sequence UNIQUE (workflow_id, sequence_number),
    CONSTRAINT ck_agent_steps_sequence CHECK (sequence_number > 0),
    CONSTRAINT ck_agent_steps_attempts CHECK (attempt_count >= 0),
    CONSTRAINT ck_agent_steps_status CHECK (status IN ('PENDING','RUNNING','WAITING','COMPLETED','FAILED','SKIPPED'))
);

CREATE TABLE agent_tool_calls (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    agent_step_id uuid NOT NULL REFERENCES agent_steps(id) ON DELETE CASCADE,
    tool_name varchar(100) NOT NULL,
    sanitized_arguments jsonb NOT NULL DEFAULT '{}'::jsonb,
    result_summary jsonb,
    validation_status varchar(20) NOT NULL,
    duration_ms integer,
    attempted_at timestamptz NOT NULL DEFAULT now(),
    error_code varchar(80),
    CONSTRAINT ck_tool_calls_validation CHECK (validation_status IN ('VALID','INVALID','FAILED','TIMEOUT')),
    CONSTRAINT ck_tool_calls_duration CHECK (duration_ms IS NULL OR duration_ms >= 0)
);

CREATE TABLE approvals (
    id uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    workflow_id uuid NOT NULL REFERENCES agent_workflows(id) ON DELETE RESTRICT,
    recovery_proposal_id uuid REFERENCES recovery_proposals(id) ON DELETE RESTRICT,
    proposal_revision integer,
    approval_type varchar(40) NOT NULL,
    requested_from_role varchar(50) NOT NULL,
    decided_by uuid REFERENCES app_users(id) ON DELETE RESTRICT,
    decision varchar(25) NOT NULL DEFAULT 'PENDING',
    comment varchar(2000),
    requested_at timestamptz NOT NULL DEFAULT now(),
    decided_at timestamptz,
    CONSTRAINT ck_approvals_decision CHECK (decision IN ('PENDING','APPROVED','REJECTED','REVISION_REQUESTED')),
    CONSTRAINT ck_approvals_decided CHECK ((decision = 'PENDING' AND decided_by IS NULL AND decided_at IS NULL) OR (decision <> 'PENDING' AND decided_by IS NOT NULL AND decided_at IS NOT NULL))
);

CREATE TABLE audit_events (
    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    actor_id uuid REFERENCES app_users(id) ON DELETE RESTRICT,
    action varchar(120) NOT NULL,
    entity_type varchar(100) NOT NULL,
    entity_id uuid,
    change_summary jsonb NOT NULL DEFAULT '{}'::jsonb,
    correlation_id uuid,
    occurred_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_items_owner_status ON items(owner_id, status);
CREATE INDEX ix_items_category_status ON items(category_id, status);
CREATE INDEX ix_assessments_item ON assessments(item_id, item_revision);
CREATE INDEX ix_recovery_cases_owner_status ON recovery_cases(owner_id, status);
CREATE INDEX ix_recovery_options_case_status ON recovery_options(recovery_case_id, status);
CREATE INDEX ix_value_references_lookup ON value_references(category_id, condition_grade, route_type, currency, observed_at DESC);
CREATE INDEX ix_partners_type_verification ON partners(partner_type, verification_status, is_active);
CREATE INDEX ix_recipient_needs_lookup ON recipient_needs(category_id, status, deadline);
CREATE INDEX ix_matches_option_eligibility ON matches(recovery_option_id, eligibility_status, partner_response_status);
CREATE INDEX ix_collection_slots_availability ON collection_slots(status, starts_at, ends_at);
CREATE INDEX ix_pickup_requests_owner_status ON pickup_requests(owner_id, status);
CREATE INDEX ix_pickup_events_request_time ON pickup_events(pickup_request_id, event_at);
CREATE INDEX ix_agent_workflows_case_status ON agent_workflows(recovery_case_id, status);
CREATE INDEX ix_agent_steps_workflow_status ON agent_steps(workflow_id, status);
CREATE INDEX ix_audit_events_entity ON audit_events(entity_type, entity_id, occurred_at DESC);

INSERT INTO roles(name) VALUES
('ItemOwner'), ('PartnerRepresentative'), ('CollectionStaff'), ('Administrator')
ON CONFLICT (name) DO NOTHING;

INSERT INTO categories(code, name, handling_class) VALUES
('PHONE','Phone','ELECTRONICS'),
('LAPTOP','Laptop','ELECTRONICS'),
('CHAIR','Chair','BULKY'),
('CLOTHES','Clothes','STANDARD'),
('BOOKS','Books','STANDARD')
ON CONFLICT (code) DO NOTHING;

COMMIT;

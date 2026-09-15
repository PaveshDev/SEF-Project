using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WasteToValue.Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIntegratedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "collection_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    collector_id = table.Column<Guid>(type: "uuid", nullable: true),
                    starts_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    service_area = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    reserved_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    vehicle_class = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "AVAILABLE"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collection_slots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LocationArea = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    partner_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    verification_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    service_area = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    contact_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    contact_phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "recovery_cases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_revision = table.Column<int>(type: "integer", nullable: false),
                    assessment_version = table.Column<int>(type: "integer", nullable: false),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    objective = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    preferred_routes = table.Column<string>(type: "jsonb", nullable: false),
                    currency = table.Column<string>(type: "character(3)", nullable: false),
                    maximum_pickup_cost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery_cases", x => x.id);
                    table.CheckConstraint("ck_recovery_cases_revisions", "revision > 0 AND item_revision > 0 AND assessment_version > 0");
                    table.CheckConstraint("ck_recovery_cases_version", "version > 0");
                    table.CheckConstraint("ck_recoverycase_currency", "currency ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_recoverycase_maximum_pickup_cost", "maximum_pickup_cost IS NULL OR maximum_pickup_cost >= 0");
                    table.CheckConstraint("ck_recoverycase_status", "status IN ('DRAFT','PLANNING','AWAITING_INPUTS','AWAITING_APPROVAL','APPROVED','REJECTED','REVISION_REQUESTED','COMPLETED','FAILED','CANCELLED')");
                });

            migrationBuilder.CreateTable(
                name: "value_references",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condition_grade = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    route_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    value_low = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    value_high = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character(3)", nullable: false),
                    source_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_reference = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    observed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_value_references", x => x.id);
                    table.CheckConstraint("ck_value_references_bounds", "value_low <= value_high");
                    table.CheckConstraint("ck_value_references_version", "version > 0");
                    table.CheckConstraint("ck_valuereference_condition_grade", "condition_grade IN ('EXCELLENT','GOOD','FAIR','POOR','UNSAFE','UNKNOWN')");
                    table.CheckConstraint("ck_valuereference_currency", "currency ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_valuereference_route_type", "route_type IN ('REUSE','DONATE','REPAIR_THEN_REUSE','RESELL','RECYCLE')");
                    table.CheckConstraint("ck_valuereference_value_high", "value_high IS NULL OR value_high >= 0");
                    table.CheckConstraint("ck_valuereference_value_low", "value_low IS NULL OR value_low >= 0");
                });

            migrationBuilder.CreateTable(
                name: "pickup_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_slot_id = table.Column<Guid>(type: "uuid", nullable: true),
                    proposed_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    proposed_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    estimated_cost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "LKR"),
                    handling_requirements = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    travel_estimate = table.Column<string>(type: "jsonb", nullable: true),
                    feasibility_status = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pickup_plans", x => x.id);
                    table.ForeignKey(
                        name: "FK_pickup_plans_collection_slots_collection_slot_id",
                        column: x => x.collection_slot_id,
                        principalTable: "collection_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pickup_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recovery_proposal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collection_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    collector_id = table.Column<Guid>(type: "uuid", nullable: true),
                    owner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pickup_address_encrypted = table.Column<string>(type: "text", nullable: false),
                    scheduled_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    scheduled_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "CONFIRMED"),
                    verification_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pickup_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_pickup_requests_collection_slots_collection_slot_id",
                        column: x => x.collection_slot_id,
                        principalTable: "collection_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    SuggestedCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ConditionGrade = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConditionSummary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    VisibleObservations = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OwnerReportedFunctionality = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MissingInformation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Confidence = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemAssessments", x => x.Id);
                    table.CheckConstraint("CK_Assessment_Confidence", "\"Confidence\" >= 0 AND \"Confidence\" <= 1");
                    table.ForeignKey(
                        name: "FK_ItemAssessments_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemConditionAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QuestionText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Answer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemConditionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemConditionAnswers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PhotoOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemPhotos_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "acceptance_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    minimum_condition = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    restrictions = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acceptance_rules", x => x.id);
                    table.ForeignKey(
                        name: "FK_acceptance_rules_partners_partner_id",
                        column: x => x.partner_id,
                        principalTable: "partners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partner_memberships",
                columns: table => new
                {
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    membership_role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Representative"),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_memberships", x => new { x.partner_id, x.user_id });
                    table.ForeignKey(
                        name: "FK_partner_memberships_partners_partner_id",
                        column: x => x.partner_id,
                        principalTable: "partners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipient_needs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    partner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    quantity_required = table.Column<int>(type: "integer", nullable: false),
                    quantity_fulfilled = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    deadline = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Open"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipient_needs", x => x.id);
                    table.ForeignKey(
                        name: "FK_recipient_needs_partners_partner_id",
                        column: x => x.partner_id,
                        principalTable: "partners",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recovery_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    recovery_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_revision = table.Column<int>(type: "integer", nullable: false),
                    assessment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assessment_version = table.Column<int>(type: "integer", nullable: false),
                    route_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    requires_partner = table.Column<bool>(type: "boolean", nullable: false),
                    requires_pickup = table.Column<bool>(type: "boolean", nullable: false),
                    estimated_value_low = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    estimated_value_high = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    estimated_repair_cost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    estimated_pickup_cost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    estimated_net_value = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    estimated_shortfall = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character(3)", nullable: false),
                    evidence = table.Column<string>(type: "jsonb", nullable: false),
                    integration_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    non_financial_benefits = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery_options", x => x.id);
                    table.CheckConstraint("ck_recovery_options_balance", "estimated_net_value = GREATEST(estimated_value_low - estimated_repair_cost - estimated_pickup_cost, 0) AND estimated_shortfall = GREATEST(estimated_repair_cost + estimated_pickup_cost - estimated_value_low, 0)");
                    table.CheckConstraint("ck_recovery_options_bounds", "estimated_value_low <= estimated_value_high");
                    table.CheckConstraint("ck_recovery_options_complete", "status NOT IN ('VALIDATED','SELECTED') OR (estimated_value_low IS NOT NULL AND estimated_value_high IS NOT NULL AND estimated_repair_cost IS NOT NULL AND estimated_pickup_cost IS NOT NULL AND estimated_net_value IS NOT NULL AND estimated_shortfall IS NOT NULL)");
                    table.CheckConstraint("ck_recovery_options_pickup", "NOT requires_pickup OR requires_partner");
                    table.CheckConstraint("ck_recovery_options_revisions", "case_revision > 0 AND assessment_version > 0");
                    table.CheckConstraint("ck_recovery_options_version", "version > 0");
                    table.CheckConstraint("ck_recoveryoption_currency", "currency ~ '^[A-Z]{3}$'");
                    table.CheckConstraint("ck_recoveryoption_estimated_net_value", "estimated_net_value IS NULL OR estimated_net_value >= 0");
                    table.CheckConstraint("ck_recoveryoption_estimated_pickup_cost", "estimated_pickup_cost IS NULL OR estimated_pickup_cost >= 0");
                    table.CheckConstraint("ck_recoveryoption_estimated_repair_cost", "estimated_repair_cost IS NULL OR estimated_repair_cost >= 0");
                    table.CheckConstraint("ck_recoveryoption_estimated_shortfall", "estimated_shortfall IS NULL OR estimated_shortfall >= 0");
                    table.CheckConstraint("ck_recoveryoption_estimated_value_high", "estimated_value_high IS NULL OR estimated_value_high >= 0");
                    table.CheckConstraint("ck_recoveryoption_estimated_value_low", "estimated_value_low IS NULL OR estimated_value_low >= 0");
                    table.CheckConstraint("ck_recoveryoption_route_type", "route_type IN ('REUSE','DONATE','REPAIR_THEN_REUSE','RESELL','RECYCLE')");
                    table.CheckConstraint("ck_recoveryoption_status", "status IN ('DRAFT','VALIDATED','SELECTED','STALE','REJECTED')");
                    table.ForeignKey(
                        name: "FK_recovery_options_recovery_cases_recovery_case_id",
                        column: x => x.recovery_case_id,
                        principalTable: "recovery_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "pickup_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pickup_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    actor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pickup_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_pickup_events_pickup_requests_pickup_request_id",
                        column: x => x.pickup_request_id,
                        principalTable: "pickup_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentClarifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    QuestionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Answer = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentClarifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentClarifications_ItemAssessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "ItemAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssessmentClarifications_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhotoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Observation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    EvidenceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentEvidences_ItemAssessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "ItemAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentEvidences_ItemPhotos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "ItemPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "recovery_proposals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    recovery_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    case_revision = table.Column<int>(type: "integer", nullable: false),
                    recovery_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    option_version = table.Column<int>(type: "integer", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: true),
                    match_version = table.Column<int>(type: "integer", nullable: true),
                    match_freshness_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    pickup_plan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pickup_plan_version = table.Column<int>(type: "integer", nullable: true),
                    pickup_freshness_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    revision = table.Column<int>(type: "integer", nullable: false),
                    input_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    estimate_snapshot = table.Column<string>(type: "jsonb", nullable: false),
                    explanation = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    recommendation_origin = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    agent_run_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recovery_proposals", x => x.id);
                    table.UniqueConstraint("AK_recovery_proposals_id_revision", x => new { x.id, x.revision });
                    table.CheckConstraint("ck_recovery_proposals_expiry", "expires_at > created_at");
                    table.CheckConstraint("ck_recovery_proposals_match", "(match_id IS NULL AND match_version IS NULL AND match_freshness_token IS NULL) OR (match_id IS NOT NULL AND match_version > 0 AND match_freshness_token IS NOT NULL)");
                    table.CheckConstraint("ck_recovery_proposals_origin", "(recommendation_origin = 'HUMAN' AND agent_run_id IS NULL) OR (recommendation_origin = 'AGENT' AND agent_run_id IS NOT NULL)");
                    table.CheckConstraint("ck_recovery_proposals_pickup", "(pickup_plan_id IS NULL AND pickup_plan_version IS NULL AND pickup_freshness_token IS NULL) OR (pickup_plan_id IS NOT NULL AND pickup_plan_version > 0 AND pickup_freshness_token IS NOT NULL AND match_id IS NOT NULL)");
                    table.CheckConstraint("ck_recovery_proposals_revisions", "revision > 0 AND case_revision > 0 AND option_version > 0");
                    table.CheckConstraint("ck_recovery_proposals_version", "version > 0");
                    table.CheckConstraint("ck_recoveryproposal_recommendation_origin", "recommendation_origin IN ('HUMAN','AGENT')");
                    table.CheckConstraint("ck_recoveryproposal_status", "status IN ('DRAFT','AWAITING_APPROVAL','APPROVED','REJECTED','REVISION_REQUESTED','EXPIRED','EXECUTED','STALE')");
                    table.ForeignKey(
                        name: "FK_recovery_proposals_recovery_cases_recovery_case_id",
                        column: x => x.recovery_case_id,
                        principalTable: "recovery_cases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recovery_proposals_recovery_options_recovery_option_id",
                        column: x => x.recovery_option_id,
                        principalTable: "recovery_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "handover_proofs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pickup_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pickup_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proof_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    verification_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    verified_by = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_handover_proofs", x => x.id);
                    table.ForeignKey(
                        name: "FK_handover_proofs_pickup_events_pickup_event_id",
                        column: x => x.pickup_event_id,
                        principalTable: "pickup_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_handover_proofs_pickup_requests_pickup_request_id",
                        column: x => x.pickup_request_id,
                        principalTable: "pickup_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "proposal_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    recovery_proposal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposal_revision = table.Column<int>(type: "integer", nullable: false),
                    decided_by = table.Column<Guid>(type: "uuid", nullable: false),
                    decision_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    decision = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    comment = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proposal_decisions", x => x.id);
                    table.CheckConstraint("ck_proposal_decisions_type", "proposal_revision > 0 AND decision_type = 'OWNER_DECISION'");
                    table.CheckConstraint("ck_proposaldecision_decision", "decision IN ('APPROVED','REJECTED','REVISION_REQUESTED')");
                    table.ForeignKey(
                        name: "FK_proposal_decisions_recovery_proposals_recovery_proposal_id_~",
                        columns: x => new { x.recovery_proposal_id, x.proposal_revision },
                        principalTable: "recovery_proposals",
                        principalColumns: new[] { "id", "revision" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "uq_acceptance_rule",
                table: "acceptance_rules",
                columns: new[] { "partner_id", "category_id", "route_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentClarifications_AssessmentId",
                table: "AssessmentClarifications",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentClarifications_ItemId",
                table: "AssessmentClarifications",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvidences_AssessmentId",
                table: "AssessmentEvidences",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvidences_PhotoId",
                table: "AssessmentEvidences",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "ix_collection_slots_availability",
                table: "collection_slots",
                columns: new[] { "status", "starts_at", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "IX_handover_proofs_pickup_event_id",
                table: "handover_proofs",
                column: "pickup_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_handover_proofs_pickup_request_id",
                table: "handover_proofs",
                column: "pickup_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_ItemAssessments_ItemId_Version",
                table: "ItemAssessments",
                columns: new[] { "ItemId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemConditionAnswers_ItemId",
                table: "ItemConditionAnswers",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemConditionAnswers_ItemId_QuestionCode",
                table: "ItemConditionAnswers",
                columns: new[] { "ItemId", "QuestionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_ItemId",
                table: "ItemPhotos",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemPhotos_ItemId_PhotoOrder",
                table: "ItemPhotos",
                columns: new[] { "ItemId", "PhotoOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_CreatedAt",
                table: "Items",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OwnerId",
                table: "Items",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Status",
                table: "Items",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_pickup_events_idempotency_key",
                table: "pickup_events",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pickup_events_request_time",
                table: "pickup_events",
                columns: new[] { "pickup_request_id", "event_at" });

            migrationBuilder.CreateIndex(
                name: "IX_pickup_plans_collection_slot_id",
                table: "pickup_plans",
                column: "collection_slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_pickup_requests_collection_slot_id",
                table: "pickup_requests",
                column: "collection_slot_id");

            migrationBuilder.CreateIndex(
                name: "IX_pickup_requests_idempotency_key",
                table: "pickup_requests",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_pickup_requests_owner_status",
                table: "pickup_requests",
                columns: new[] { "owner_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_pickup_requests_recovery_proposal_id",
                table: "pickup_requests",
                column: "recovery_proposal_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proposal_decisions_decided_by_idempotency_key",
                table: "proposal_decisions",
                columns: new[] { "decided_by", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proposal_decisions_recovery_proposal_id_proposal_revision_d~",
                table: "proposal_decisions",
                columns: new[] { "recovery_proposal_id", "proposal_revision", "decided_by", "decision_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipient_needs_partner_id",
                table: "recipient_needs",
                column: "partner_id");

            migrationBuilder.CreateIndex(
                name: "IX_recovery_cases_owner_id_status",
                table: "recovery_cases",
                columns: new[] { "owner_id", "status" });

            migrationBuilder.CreateIndex(
                name: "uq_recovery_cases_active_item",
                table: "recovery_cases",
                column: "item_id",
                unique: true,
                filter: "status NOT IN ('REJECTED','COMPLETED','FAILED','CANCELLED')");

            migrationBuilder.CreateIndex(
                name: "IX_recovery_options_recovery_case_id_status",
                table: "recovery_options",
                columns: new[] { "recovery_case_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_recovery_proposals_recovery_case_id_revision",
                table: "recovery_proposals",
                columns: new[] { "recovery_case_id", "revision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recovery_proposals_recovery_option_id",
                table: "recovery_proposals",
                column: "recovery_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_value_references_category_id_condition_grade_route_type_cur~",
                table: "value_references",
                columns: new[] { "category_id", "condition_grade", "route_type", "currency", "observed_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acceptance_rules");

            migrationBuilder.DropTable(
                name: "AssessmentClarifications");

            migrationBuilder.DropTable(
                name: "AssessmentEvidences");

            migrationBuilder.DropTable(
                name: "handover_proofs");

            migrationBuilder.DropTable(
                name: "ItemConditionAnswers");

            migrationBuilder.DropTable(
                name: "partner_memberships");

            migrationBuilder.DropTable(
                name: "pickup_plans");

            migrationBuilder.DropTable(
                name: "proposal_decisions");

            migrationBuilder.DropTable(
                name: "recipient_needs");

            migrationBuilder.DropTable(
                name: "value_references");

            migrationBuilder.DropTable(
                name: "ItemAssessments");

            migrationBuilder.DropTable(
                name: "ItemPhotos");

            migrationBuilder.DropTable(
                name: "pickup_events");

            migrationBuilder.DropTable(
                name: "recovery_proposals");

            migrationBuilder.DropTable(
                name: "partners");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "pickup_requests");

            migrationBuilder.DropTable(
                name: "recovery_options");

            migrationBuilder.DropTable(
                name: "collection_slots");

            migrationBuilder.DropTable(
                name: "recovery_cases");
        }
    }
}

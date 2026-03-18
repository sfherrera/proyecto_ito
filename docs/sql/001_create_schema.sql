-- ============================================================
-- ITO Cloud — Script de creación de esquema de base de datos
-- PostgreSQL 18
-- Versión: 1.0 | Fecha: 2026-03-17
-- ============================================================

-- Extensiones necesarias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";    -- búsqueda de texto
CREATE EXTENSION IF NOT EXISTS "unaccent";   -- búsqueda sin acentos

-- ============================================================
-- TENANTS (Multi-tenancy)
-- ============================================================
CREATE TABLE tenants (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    name                VARCHAR(200)    NOT NULL,
    slug                VARCHAR(100)    NOT NULL UNIQUE,
    plan                VARCHAR(50)     NOT NULL DEFAULT 'basic',
    is_active           BOOLEAN         NOT NULL DEFAULT TRUE,
    logo_url            VARCHAR(500),
    primary_color       VARCHAR(20),
    max_users           INT             NOT NULL DEFAULT 10,
    max_projects        INT             NOT NULL DEFAULT 5,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    CONSTRAINT chk_tenants_plan CHECK (plan IN ('basic', 'professional', 'enterprise'))
);

-- ============================================================
-- USERS (ASP.NET Identity personalizado)
-- ============================================================
CREATE TABLE users (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID            NOT NULL REFERENCES tenants(id),
    -- Identity fields
    user_name               VARCHAR(256)    NOT NULL,
    normalized_user_name    VARCHAR(256)    NOT NULL,
    email                   VARCHAR(256)    NOT NULL,
    normalized_email        VARCHAR(256)    NOT NULL,
    email_confirmed         BOOLEAN         NOT NULL DEFAULT FALSE,
    password_hash           VARCHAR(500),
    security_stamp          VARCHAR(200),
    concurrency_stamp       VARCHAR(200),
    phone_number            VARCHAR(50),
    phone_number_confirmed  BOOLEAN         NOT NULL DEFAULT FALSE,
    two_factor_enabled      BOOLEAN         NOT NULL DEFAULT FALSE,
    lockout_end             TIMESTAMPTZ,
    lockout_enabled         BOOLEAN         NOT NULL DEFAULT TRUE,
    access_failed_count     INT             NOT NULL DEFAULT 0,
    -- Custom fields
    first_name              VARCHAR(100)    NOT NULL,
    last_name               VARCHAR(100)    NOT NULL,
    rut                     VARCHAR(20),
    position                VARCHAR(100),
    avatar_url              VARCHAR(500),
    is_active               BOOLEAN         NOT NULL DEFAULT TRUE,
    last_login_at           TIMESTAMPTZ,
    -- Auditoría
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ,
    created_by              UUID,
    updated_by              UUID,
    is_deleted              BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID
);

CREATE TABLE roles (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id           UUID            REFERENCES tenants(id),
    name                VARCHAR(256)    NOT NULL,
    normalized_name     VARCHAR(256)    NOT NULL,
    concurrency_stamp   VARCHAR(200),
    description         VARCHAR(500),
    is_system_role      BOOLEAN         NOT NULL DEFAULT FALSE
);

CREATE TABLE user_roles (
    user_id     UUID    NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id     UUID    NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE user_claims (
    id          SERIAL  PRIMARY KEY,
    user_id     UUID    NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    claim_type  VARCHAR(200),
    claim_value VARCHAR(500)
);

CREATE TABLE role_claims (
    id          SERIAL  PRIMARY KEY,
    role_id     UUID    NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    claim_type  VARCHAR(200),
    claim_value VARCHAR(500)
);

CREATE TABLE user_tokens (
    user_id         UUID            NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    login_provider  VARCHAR(200)    NOT NULL,
    name            VARCHAR(200)    NOT NULL,
    value           VARCHAR(1000),
    PRIMARY KEY (user_id, login_provider, name)
);

CREATE TABLE user_logins (
    login_provider          VARCHAR(200)    NOT NULL,
    provider_key            VARCHAR(200)    NOT NULL,
    provider_display_name   VARCHAR(200),
    user_id                 UUID            NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    PRIMARY KEY (login_provider, provider_key)
);

-- ============================================================
-- COMPANIES (Empresas clientes)
-- ============================================================
CREATE TABLE companies (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    name            VARCHAR(200)    NOT NULL,
    rut             VARCHAR(20),
    business_name   VARCHAR(300),
    company_type    VARCHAR(50)     NOT NULL DEFAULT 'constructora',
    address         VARCHAR(300),
    city            VARCHAR(100),
    region          VARCHAR(100),
    phone           VARCHAR(50),
    email           VARCHAR(256),
    website         VARCHAR(300),
    logo_url        VARCHAR(500),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    notes           TEXT,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    created_by      UUID            NOT NULL REFERENCES users(id),
    updated_by      UUID            REFERENCES users(id),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID            REFERENCES users(id),
    CONSTRAINT chk_companies_type CHECK (company_type IN ('constructora','inmobiliaria','mixta','consultora','otra'))
);

-- ============================================================
-- PROJECTS (Obras / Proyectos)
-- ============================================================
CREATE TABLE projects (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID            NOT NULL REFERENCES tenants(id),
    company_id              UUID            NOT NULL REFERENCES companies(id),
    code                    VARCHAR(50)     NOT NULL,
    name                    VARCHAR(300)    NOT NULL,
    description             TEXT,
    project_type            VARCHAR(50)     NOT NULL DEFAULT 'edificio',
    status                  VARCHAR(50)     NOT NULL DEFAULT 'activo',
    address                 VARCHAR(300),
    city                    VARCHAR(100),
    region                  VARCHAR(100),
    latitude                DECIMAL(10,7),
    longitude               DECIMAL(10,7),
    start_date              DATE,
    estimated_end_date      DATE,
    actual_end_date         DATE,
    total_units             INT,
    ito_manager_id          UUID            REFERENCES users(id),
    mandante_name           VARCHAR(200),
    mandante_contact        VARCHAR(200),
    mandante_email          VARCHAR(256),
    construction_permit     VARCHAR(100),
    is_active               BOOLEAN         NOT NULL DEFAULT TRUE,
    notes                   TEXT,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ,
    created_by              UUID            NOT NULL REFERENCES users(id),
    updated_by              UUID            REFERENCES users(id),
    is_deleted              BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID            REFERENCES users(id),
    CONSTRAINT chk_projects_status CHECK (status IN ('planificacion','activo','pausado','finalizado','cerrado')),
    CONSTRAINT chk_projects_type   CHECK (project_type IN ('edificio','conjunto_casas','infraestructura','comercial','industrial','mixto'))
);

-- Miembros del proyecto (ITO, supervisores, etc.)
CREATE TABLE project_members (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    project_id      UUID            NOT NULL REFERENCES projects(id),
    user_id         UUID            NOT NULL REFERENCES users(id),
    project_role    VARCHAR(100)    NOT NULL DEFAULT 'inspector',
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    assigned_at     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    assigned_by     UUID            REFERENCES users(id),
    UNIQUE(project_id, user_id),
    CONSTRAINT chk_project_role CHECK (project_role IN ('director','supervisor','inspector','contratista','visualizador'))
);

-- ============================================================
-- PROJECT STAGES (Etapas de obra)
-- ============================================================
CREATE TABLE project_stages (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    project_id      UUID            NOT NULL REFERENCES projects(id),
    name            VARCHAR(200)    NOT NULL,
    description     TEXT,
    order_index     INT             NOT NULL DEFAULT 0,
    status          VARCHAR(50)     NOT NULL DEFAULT 'pendiente',
    start_date      DATE,
    end_date        DATE,
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    created_by      UUID            NOT NULL REFERENCES users(id),
    updated_by      UUID            REFERENCES users(id),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID            REFERENCES users(id),
    CONSTRAINT chk_stages_status CHECK (status IN ('pendiente','en_progreso','completada','suspendida'))
);

-- ============================================================
-- PROJECT SECTORS (Sectores / Torres / Bloques / Pisos)
-- ============================================================
CREATE TABLE project_sectors (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id           UUID            NOT NULL REFERENCES tenants(id),
    project_id          UUID            NOT NULL REFERENCES projects(id),
    parent_sector_id    UUID            REFERENCES project_sectors(id),
    name                VARCHAR(200)    NOT NULL,
    sector_type         VARCHAR(50)     NOT NULL DEFAULT 'sector',
    order_index         INT             NOT NULL DEFAULT 0,
    is_active           BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    created_by          UUID            NOT NULL REFERENCES users(id),
    updated_by          UUID            REFERENCES users(id),
    is_deleted          BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID            REFERENCES users(id),
    CONSTRAINT chk_sector_type CHECK (sector_type IN ('torre','bloque','sector','piso','area_comun','subterraneo','fachada'))
);

-- ============================================================
-- PROJECT UNITS (Departamentos / Casas / Unidades)
-- ============================================================
CREATE TABLE project_units (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    project_id      UUID            NOT NULL REFERENCES projects(id),
    sector_id       UUID            REFERENCES project_sectors(id),
    unit_code       VARCHAR(50)     NOT NULL,
    unit_type       VARCHAR(50)     NOT NULL DEFAULT 'departamento',
    floor           INT,
    surface_m2      DECIMAL(10,2),
    status          VARCHAR(50)     NOT NULL DEFAULT 'construccion',
    owner_name      VARCHAR(200),
    owner_email     VARCHAR(256),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    notes           TEXT,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    created_by      UUID            NOT NULL REFERENCES users(id),
    updated_by      UUID            REFERENCES users(id),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID            REFERENCES users(id),
    CONSTRAINT chk_unit_type   CHECK (unit_type IN ('departamento','casa','local','bodega','estacionamiento','area_comun','oficina')),
    CONSTRAINT chk_unit_status CHECK (status IN ('construccion','recepcion_preliminar','recepcionada','entregada','con_observaciones'))
);

-- ============================================================
-- SPECIALTIES (Especialidades de obra)
-- ============================================================
CREATE TABLE specialties (
    id          UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id   UUID            NOT NULL REFERENCES tenants(id),
    name        VARCHAR(200)    NOT NULL,
    code        VARCHAR(50),
    description TEXT,
    color       VARCHAR(20),
    is_active   BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by  UUID            NOT NULL REFERENCES users(id),
    is_deleted  BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at  TIMESTAMPTZ
);

-- ============================================================
-- CONTRACTORS (Contratistas)
-- ============================================================
CREATE TABLE contractors (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    company_id      UUID            REFERENCES companies(id),
    name            VARCHAR(200)    NOT NULL,
    rut             VARCHAR(20),
    contact_name    VARCHAR(200),
    contact_email   VARCHAR(256),
    contact_phone   VARCHAR(50),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    notes           TEXT,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    created_by      UUID            NOT NULL REFERENCES users(id),
    updated_by      UUID            REFERENCES users(id),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID            REFERENCES users(id)
);

CREATE TABLE contractor_specialties (
    contractor_id   UUID    NOT NULL REFERENCES contractors(id) ON DELETE CASCADE,
    specialty_id    UUID    NOT NULL REFERENCES specialties(id) ON DELETE CASCADE,
    PRIMARY KEY (contractor_id, specialty_id)
);

-- Contratistas asignados a un proyecto
CREATE TABLE project_contractors (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    project_id      UUID            NOT NULL REFERENCES projects(id),
    contractor_id   UUID            NOT NULL REFERENCES contractors(id),
    specialty_id    UUID            REFERENCES specialties(id),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    assigned_at     TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    assigned_by     UUID            REFERENCES users(id),
    UNIQUE(project_id, contractor_id, specialty_id)
);

-- ============================================================
-- INSPECTION TEMPLATES (Plantillas de inspección)
-- ============================================================
CREATE TABLE inspection_templates (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID            NOT NULL REFERENCES tenants(id),
    name                    VARCHAR(300)    NOT NULL,
    description             TEXT,
    template_type           VARCHAR(100),
    specialty_id            UUID            REFERENCES specialties(id),
    status                  VARCHAR(50)     NOT NULL DEFAULT 'borrador',
    current_version         INT             NOT NULL DEFAULT 1,
    is_global               BOOLEAN         NOT NULL DEFAULT FALSE,
    allow_partial_save      BOOLEAN         NOT NULL DEFAULT TRUE,
    require_geolocation     BOOLEAN         NOT NULL DEFAULT FALSE,
    require_signature       BOOLEAN         NOT NULL DEFAULT FALSE,
    passing_score           DECIMAL(5,2),
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ,
    created_by              UUID            NOT NULL REFERENCES users(id),
    updated_by              UUID            REFERENCES users(id),
    is_deleted              BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID            REFERENCES users(id),
    CONSTRAINT chk_template_status CHECK (status IN ('borrador','activa','inactiva','archivada'))
);

-- Historial de versiones de plantillas
CREATE TABLE template_versions (
    id              UUID        PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID        NOT NULL REFERENCES tenants(id),
    template_id     UUID        NOT NULL REFERENCES inspection_templates(id),
    version_number  INT         NOT NULL,
    snapshot        JSONB       NOT NULL,
    change_notes    TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    created_by      UUID        NOT NULL REFERENCES users(id),
    UNIQUE(template_id, version_number)
);

-- Secciones de la plantilla
CREATE TABLE template_sections (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    template_id     UUID            NOT NULL REFERENCES inspection_templates(id),
    title           VARCHAR(300)    NOT NULL,
    description     TEXT,
    order_index     INT             NOT NULL DEFAULT 0,
    is_required     BOOLEAN         NOT NULL DEFAULT TRUE,
    weight          DECIMAL(5,2)    NOT NULL DEFAULT 1.0,
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    created_by      UUID            NOT NULL REFERENCES users(id),
    updated_by      UUID            REFERENCES users(id)
);

-- Preguntas de cada sección
CREATE TABLE template_questions (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id           UUID            NOT NULL REFERENCES tenants(id),
    section_id          UUID            NOT NULL REFERENCES template_sections(id),
    parent_question_id  UUID            REFERENCES template_questions(id),
    trigger_value       VARCHAR(200),
    question_text       TEXT            NOT NULL,
    description         TEXT,
    question_type       VARCHAR(50)     NOT NULL,
    order_index         INT             NOT NULL DEFAULT 0,
    is_required         BOOLEAN         NOT NULL DEFAULT TRUE,
    is_critical         BOOLEAN         NOT NULL DEFAULT FALSE,
    weight              DECIMAL(5,2)    NOT NULL DEFAULT 1.0,
    min_value           DECIMAL(15,4),
    max_value           DECIMAL(15,4),
    min_photos          INT             NOT NULL DEFAULT 0,
    max_photos          INT             NOT NULL DEFAULT 10,
    placeholder         TEXT,
    validation_regex    VARCHAR(500),
    validation_message  VARCHAR(300),
    is_active           BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    created_by          UUID            NOT NULL REFERENCES users(id),
    updated_by          UUID            REFERENCES users(id),
    CONSTRAINT chk_question_type CHECK (question_type IN (
        'yes_no','multiple_choice','text','numeric','date',
        'photo','video','audio','signature','geolocation','file'
    ))
);

-- Opciones para preguntas de tipo multiple_choice
CREATE TABLE template_question_options (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    question_id         UUID            NOT NULL REFERENCES template_questions(id) ON DELETE CASCADE,
    label               VARCHAR(300)    NOT NULL,
    value               VARCHAR(200)    NOT NULL,
    order_index         INT             NOT NULL DEFAULT 0,
    is_failure_option   BOOLEAN         NOT NULL DEFAULT FALSE,
    score               DECIMAL(5,2)    NOT NULL DEFAULT 1.0
);

-- ============================================================
-- INSPECTIONS (Inspecciones programadas / ejecutadas)
-- ============================================================
CREATE TABLE inspections (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID            NOT NULL REFERENCES tenants(id),
    project_id              UUID            NOT NULL REFERENCES projects(id),
    template_id             UUID            NOT NULL REFERENCES inspection_templates(id),
    template_version        INT             NOT NULL DEFAULT 1,
    stage_id                UUID            REFERENCES project_stages(id),
    sector_id               UUID            REFERENCES project_sectors(id),
    unit_id                 UUID            REFERENCES project_units(id),
    code                    VARCHAR(50)     NOT NULL,
    title                   VARCHAR(300)    NOT NULL,
    description             TEXT,
    inspection_type         VARCHAR(100)    NOT NULL DEFAULT 'ordinaria',
    status                  VARCHAR(50)     NOT NULL DEFAULT 'programada',
    priority                VARCHAR(20)     NOT NULL DEFAULT 'normal',
    scheduled_date          TIMESTAMPTZ     NOT NULL,
    scheduled_end_date      TIMESTAMPTZ,
    started_at              TIMESTAMPTZ,
    finished_at             TIMESTAMPTZ,
    assigned_to_id          UUID            REFERENCES users(id),
    assigned_by_id          UUID            REFERENCES users(id),
    supervisor_id           UUID            REFERENCES users(id),
    contractor_id           UUID            REFERENCES contractors(id),
    specialty_id            UUID            REFERENCES specialties(id),
    score                   DECIMAL(5,2),
    passing_score           DECIMAL(5,2),
    passed                  BOOLEAN,
    total_questions         INT             NOT NULL DEFAULT 0,
    answered_questions      INT             NOT NULL DEFAULT 0,
    conforming_count        INT             NOT NULL DEFAULT 0,
    non_conforming_count    INT             NOT NULL DEFAULT 0,
    na_count                INT             NOT NULL DEFAULT 0,
    latitude                DECIMAL(10,7),
    longitude               DECIMAL(10,7),
    weather_conditions      VARCHAR(100),
    temperature             DECIMAL(4,1),
    is_offline_created      BOOLEAN         NOT NULL DEFAULT FALSE,
    sync_id                 VARCHAR(100),
    notes                   TEXT,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ,
    created_by              UUID            NOT NULL REFERENCES users(id),
    updated_by              UUID            REFERENCES users(id),
    is_deleted              BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID            REFERENCES users(id),
    CONSTRAINT chk_inspection_status   CHECK (status IN ('programada','en_proceso','finalizada','observada','cerrada','cancelada')),
    CONSTRAINT chk_inspection_priority CHECK (priority IN ('baja','normal','alta','critica')),
    CONSTRAINT chk_inspection_type     CHECK (inspection_type IN ('ordinaria','extraordinaria','reinspeccion','recepcion'))
);

-- Respuestas de inspección
CREATE TABLE inspection_answers (
    id                  UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id           UUID            NOT NULL REFERENCES tenants(id),
    inspection_id       UUID            NOT NULL REFERENCES inspections(id),
    question_id         UUID            NOT NULL REFERENCES template_questions(id),
    answer_value        TEXT,
    selected_option_id  UUID            REFERENCES template_question_options(id),
    numeric_value       DECIMAL(15,4),
    date_value          DATE,
    is_conforming       BOOLEAN,
    is_na               BOOLEAN         NOT NULL DEFAULT FALSE,
    score               DECIMAL(5,2),
    notes               TEXT,
    latitude            DECIMAL(10,7),
    longitude           DECIMAL(10,7),
    answered_at         TIMESTAMPTZ,
    answered_by         UUID            REFERENCES users(id),
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    UNIQUE(inspection_id, question_id)
);

-- Evidencia adjunta (fotos, videos, documentos)
CREATE TABLE inspection_evidence (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID            NOT NULL REFERENCES tenants(id),
    inspection_id           UUID            NOT NULL REFERENCES inspections(id),
    answer_id               UUID            REFERENCES inspection_answers(id),
    observation_id          UUID,           -- FK se agrega después de crear observations
    file_type               VARCHAR(20)     NOT NULL DEFAULT 'photo',
    file_name               VARCHAR(300)    NOT NULL,
    file_path               VARCHAR(1000)   NOT NULL,
    file_size_bytes         BIGINT,
    mime_type               VARCHAR(100),
    thumbnail_path          VARCHAR(1000),
    caption                 TEXT,
    latitude                DECIMAL(10,7),
    longitude               DECIMAL(10,7),
    taken_at                TIMESTAMPTZ,
    is_offline_uploaded     BOOLEAN         NOT NULL DEFAULT FALSE,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by              UUID            NOT NULL REFERENCES users(id),
    CONSTRAINT chk_evidence_type CHECK (file_type IN ('photo','video','audio','document','signature'))
);

-- ============================================================
-- OBSERVATIONS / NO-CONFORMANCES (Observaciones / NC)
-- ============================================================
CREATE TABLE observations (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID            NOT NULL REFERENCES tenants(id),
    project_id              UUID            NOT NULL REFERENCES projects(id),
    inspection_id           UUID            REFERENCES inspections(id),
    answer_id               UUID            REFERENCES inspection_answers(id),
    code                    VARCHAR(50)     NOT NULL,
    title                   VARCHAR(300)    NOT NULL,
    description             TEXT            NOT NULL,
    specialty_id            UUID            REFERENCES specialties(id),
    category                VARCHAR(100),
    severity                VARCHAR(20)     NOT NULL DEFAULT 'media',
    status                  VARCHAR(50)     NOT NULL DEFAULT 'abierta',
    stage_id                UUID            REFERENCES project_stages(id),
    sector_id               UUID            REFERENCES project_sectors(id),
    unit_id                 UUID            REFERENCES project_units(id),
    location_description    VARCHAR(300),
    contractor_id           UUID            REFERENCES contractors(id),
    assigned_to_id          UUID            REFERENCES users(id),
    assigned_by_id          UUID            REFERENCES users(id),
    detected_at             TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    detected_by             UUID            NOT NULL REFERENCES users(id),
    due_date                DATE,
    extended_due_date       DATE,
    closed_at               TIMESTAMPTZ,
    closed_by               UUID            REFERENCES users(id),
    rejection_count         INT             NOT NULL DEFAULT 0,
    root_cause              TEXT,
    corrective_action       TEXT,
    latitude                DECIMAL(10,7),
    longitude               DECIMAL(10,7),
    is_recurring            BOOLEAN         NOT NULL DEFAULT FALSE,
    is_offline_created      BOOLEAN         NOT NULL DEFAULT FALSE,
    sync_id                 VARCHAR(100),
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ,
    created_by              UUID            NOT NULL REFERENCES users(id),
    updated_by              UUID            REFERENCES users(id),
    is_deleted              BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID            REFERENCES users(id),
    CONSTRAINT chk_obs_severity CHECK (severity IN ('baja','media','alta','critica')),
    CONSTRAINT chk_obs_status   CHECK (status IN ('abierta','asignada','en_correccion','corregida','rechazada','cerrada'))
);

-- FK diferida: evidencia → observación
ALTER TABLE inspection_evidence
    ADD CONSTRAINT fk_evidence_observation
    FOREIGN KEY (observation_id) REFERENCES observations(id);

-- Historial de cambios de observaciones
CREATE TABLE observation_history (
    id                      UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id               UUID            NOT NULL REFERENCES tenants(id),
    observation_id          UUID            NOT NULL REFERENCES observations(id),
    action                  VARCHAR(100)    NOT NULL,
    previous_status         VARCHAR(50),
    new_status              VARCHAR(50),
    previous_assigned_to    UUID            REFERENCES users(id),
    new_assigned_to         UUID            REFERENCES users(id),
    comment                 TEXT,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    created_by              UUID            NOT NULL REFERENCES users(id)
);

-- ============================================================
-- REINSPECTIONS (Reinspecciones)
-- ============================================================
CREATE TABLE reinspections (
    id                          UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id                   UUID            NOT NULL REFERENCES tenants(id),
    original_inspection_id      UUID            NOT NULL REFERENCES inspections(id),
    new_inspection_id           UUID            REFERENCES inspections(id),
    observation_id              UUID            REFERENCES observations(id),
    code                        VARCHAR(50)     NOT NULL,
    status                      VARCHAR(50)     NOT NULL DEFAULT 'pendiente',
    result                      VARCHAR(20),
    rejection_reason            TEXT,
    scheduled_date              TIMESTAMPTZ,
    executed_at                 TIMESTAMPTZ,
    executed_by                 UUID            REFERENCES users(id),
    created_at                  TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at                  TIMESTAMPTZ,
    created_by                  UUID            NOT NULL REFERENCES users(id),
    updated_by                  UUID            REFERENCES users(id),
    CONSTRAINT chk_reinsp_status  CHECK (status IN ('pendiente','programada','ejecutada')),
    CONSTRAINT chk_reinsp_result  CHECK (result IS NULL OR result IN ('aprobada','rechazada'))
);

-- ============================================================
-- PROJECT DOCUMENTS (Documentos adjuntos)
-- ============================================================
CREATE TABLE project_documents (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    project_id      UUID            NOT NULL REFERENCES projects(id),
    inspection_id   UUID            REFERENCES inspections(id),
    observation_id  UUID            REFERENCES observations(id),
    category        VARCHAR(100)    NOT NULL DEFAULT 'general',
    name            VARCHAR(300)    NOT NULL,
    description     TEXT,
    file_name       VARCHAR(300)    NOT NULL,
    file_path       VARCHAR(1000)   NOT NULL,
    file_size_bytes BIGINT,
    mime_type       VARCHAR(100),
    version         VARCHAR(50),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    created_by      UUID            NOT NULL REFERENCES users(id),
    updated_by      UUID            REFERENCES users(id),
    is_deleted      BOOLEAN         NOT NULL DEFAULT FALSE,
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID            REFERENCES users(id),
    CONSTRAINT chk_doc_category CHECK (category IN ('plano','especificacion','contrato','procedimiento','informe','foto','otro','general'))
);

-- ============================================================
-- NOTIFICATIONS (Notificaciones internas)
-- ============================================================
CREATE TABLE notifications (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    user_id         UUID            NOT NULL REFERENCES users(id),
    type            VARCHAR(100)    NOT NULL,
    title           VARCHAR(300)    NOT NULL,
    body            TEXT            NOT NULL,
    entity_type     VARCHAR(100),
    entity_id       UUID,
    is_read         BOOLEAN         NOT NULL DEFAULT FALSE,
    read_at         TIMESTAMPTZ,
    sent_email      BOOLEAN         NOT NULL DEFAULT FALSE,
    sent_push       BOOLEAN         NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- ============================================================
-- AUDIT LOGS (Auditoría de acciones del sistema)
-- ============================================================
CREATE TABLE audit_logs (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            REFERENCES tenants(id),
    user_id         UUID            REFERENCES users(id),
    action          VARCHAR(100)    NOT NULL,
    entity_type     VARCHAR(200),
    entity_id       VARCHAR(200),
    old_values      JSONB,
    new_values      JSONB,
    ip_address      VARCHAR(50),
    user_agent      VARCHAR(500),
    additional_data JSONB,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- ============================================================
-- SEQUENCE COUNTERS (Correlativos por tenant y año)
-- ============================================================
CREATE TABLE sequence_counters (
    id              UUID            PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id       UUID            NOT NULL REFERENCES tenants(id),
    entity_type     VARCHAR(100)    NOT NULL,
    prefix          VARCHAR(20)     NOT NULL,
    year            INT             NOT NULL,
    last_value      INT             NOT NULL DEFAULT 0,
    UNIQUE(tenant_id, entity_type, year)
);

-- ============================================================
-- INDEXES (Performance)
-- ============================================================

-- Users
CREATE UNIQUE INDEX idx_users_normalized_email      ON users(tenant_id, normalized_email) WHERE is_deleted = FALSE;
CREATE UNIQUE INDEX idx_users_normalized_username   ON users(tenant_id, normalized_user_name) WHERE is_deleted = FALSE;
CREATE INDEX idx_users_tenant                       ON users(tenant_id) WHERE is_deleted = FALSE;

-- Roles
CREATE UNIQUE INDEX idx_roles_normalized_name ON roles(tenant_id, normalized_name);

-- Companies
CREATE INDEX idx_companies_tenant   ON companies(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_companies_rut      ON companies(tenant_id, rut) WHERE rut IS NOT NULL;

-- Projects
CREATE INDEX idx_projects_tenant    ON projects(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_projects_company   ON projects(company_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_projects_status    ON projects(tenant_id, status) WHERE is_deleted = FALSE;
CREATE UNIQUE INDEX idx_projects_code ON projects(tenant_id, code) WHERE is_deleted = FALSE;

-- Project members
CREATE INDEX idx_project_members_project ON project_members(project_id);
CREATE INDEX idx_project_members_user    ON project_members(user_id);

-- Stages, Sectors, Units
CREATE INDEX idx_stages_project     ON project_stages(project_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_sectors_project    ON project_sectors(project_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_sectors_parent     ON project_sectors(parent_sector_id) WHERE parent_sector_id IS NOT NULL;
CREATE INDEX idx_units_project      ON project_units(project_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_units_sector       ON project_units(sector_id) WHERE sector_id IS NOT NULL;

-- Specialties & Contractors
CREATE INDEX idx_specialties_tenant     ON specialties(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_contractors_tenant     ON contractors(tenant_id) WHERE is_deleted = FALSE;

-- Templates
CREATE INDEX idx_templates_tenant       ON inspection_templates(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_templates_status       ON inspection_templates(tenant_id, status) WHERE is_deleted = FALSE;
CREATE INDEX idx_sections_template      ON template_sections(template_id);
CREATE INDEX idx_questions_section      ON template_questions(section_id);
CREATE INDEX idx_questions_parent       ON template_questions(parent_question_id) WHERE parent_question_id IS NOT NULL;
CREATE INDEX idx_options_question       ON template_question_options(question_id);

-- Inspections
CREATE INDEX idx_inspections_tenant         ON inspections(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_inspections_project        ON inspections(project_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_inspections_assigned_to    ON inspections(assigned_to_id) WHERE assigned_to_id IS NOT NULL;
CREATE INDEX idx_inspections_status         ON inspections(tenant_id, status) WHERE is_deleted = FALSE;
CREATE INDEX idx_inspections_scheduled_date ON inspections(tenant_id, scheduled_date) WHERE is_deleted = FALSE;
CREATE UNIQUE INDEX idx_inspections_code    ON inspections(tenant_id, code) WHERE is_deleted = FALSE;
CREATE INDEX idx_inspections_sync_id        ON inspections(sync_id) WHERE sync_id IS NOT NULL;

-- Answers
CREATE INDEX idx_answers_inspection ON inspection_answers(inspection_id);
CREATE INDEX idx_answers_question   ON inspection_answers(question_id);

-- Evidence
CREATE INDEX idx_evidence_inspection    ON inspection_evidence(inspection_id);
CREATE INDEX idx_evidence_answer        ON inspection_evidence(answer_id) WHERE answer_id IS NOT NULL;
CREATE INDEX idx_evidence_observation   ON inspection_evidence(observation_id) WHERE observation_id IS NOT NULL;

-- Observations
CREATE INDEX idx_observations_tenant        ON observations(tenant_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_observations_project       ON observations(project_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_observations_inspection    ON observations(inspection_id) WHERE inspection_id IS NOT NULL;
CREATE INDEX idx_observations_status        ON observations(tenant_id, status) WHERE is_deleted = FALSE;
CREATE INDEX idx_observations_severity      ON observations(tenant_id, severity) WHERE is_deleted = FALSE;
CREATE INDEX idx_observations_assigned_to   ON observations(assigned_to_id) WHERE assigned_to_id IS NOT NULL;
CREATE INDEX idx_observations_due_date      ON observations(tenant_id, due_date) WHERE due_date IS NOT NULL AND is_deleted = FALSE;
CREATE UNIQUE INDEX idx_observations_code   ON observations(tenant_id, code) WHERE is_deleted = FALSE;

-- Observation history
CREATE INDEX idx_obs_history_observation ON observation_history(observation_id);

-- Reinspections
CREATE INDEX idx_reinspections_original ON reinspections(original_inspection_id);
CREATE INDEX idx_reinspections_obs      ON reinspections(observation_id) WHERE observation_id IS NOT NULL;

-- Documents
CREATE INDEX idx_documents_project      ON project_documents(project_id) WHERE is_deleted = FALSE;
CREATE INDEX idx_documents_inspection   ON project_documents(inspection_id) WHERE inspection_id IS NOT NULL;
CREATE INDEX idx_documents_observation  ON project_documents(observation_id) WHERE observation_id IS NOT NULL;

-- Notifications
CREATE INDEX idx_notifications_user         ON notifications(user_id, is_read);
CREATE INDEX idx_notifications_tenant       ON notifications(tenant_id);

-- Audit logs
CREATE INDEX idx_audit_tenant       ON audit_logs(tenant_id);
CREATE INDEX idx_audit_user         ON audit_logs(user_id) WHERE user_id IS NOT NULL;
CREATE INDEX idx_audit_entity       ON audit_logs(entity_type, entity_id) WHERE entity_type IS NOT NULL;
CREATE INDEX idx_audit_created_at   ON audit_logs(created_at);

-- Sequence counters
CREATE INDEX idx_sequence_tenant ON sequence_counters(tenant_id);

-- ============================================================
-- DATOS INICIALES (Seed)
-- ============================================================

-- Tenant demo
INSERT INTO tenants (id, name, slug, plan, is_active, max_users, max_projects)
VALUES ('00000000-0000-0000-0000-000000000001', 'ITO Cloud Demo', 'demo', 'enterprise', TRUE, 999, 999);

-- Roles del sistema
INSERT INTO roles (id, tenant_id, name, normalized_name, description, is_system_role) VALUES
    ('10000000-0000-0000-0000-000000000001', NULL, 'SuperAdmin',      'SUPERADMIN',      'Administrador del sistema',           TRUE),
    ('10000000-0000-0000-0000-000000000002', NULL, 'TenantAdmin',     'TENANTADMIN',     'Administrador del tenant',            TRUE),
    ('10000000-0000-0000-0000-000000000003', NULL, 'ITO',             'ITO',             'Inspector Técnico de Obras',          TRUE),
    ('10000000-0000-0000-0000-000000000004', NULL, 'Supervisor',      'SUPERVISOR',      'Supervisor de obra',                  TRUE),
    ('10000000-0000-0000-0000-000000000005', NULL, 'Contratista',     'CONTRATISTA',     'Contratista (acceso limitado)',        TRUE),
    ('10000000-0000-0000-0000-000000000006', NULL, 'Visualizador',    'VISUALIZADOR',    'Solo lectura de proyectos asignados', TRUE);

-- Usuario admin inicial (contraseña: Admin@1234 — hash se actualiza desde la app)
INSERT INTO users (
    id, tenant_id, user_name, normalized_user_name,
    email, normalized_email, email_confirmed,
    first_name, last_name, is_active,
    password_hash, security_stamp, concurrency_stamp, created_by
) VALUES (
    '20000000-0000-0000-0000-000000000001',
    '00000000-0000-0000-0000-000000000001',
    'admin@itocloud.cl', 'ADMIN@ITOCLOUD.CL',
    'admin@itocloud.cl', 'ADMIN@ITOCLOUD.CL', TRUE,
    'Admin', 'Sistema', TRUE,
    'AQAAAAIAAYagAAAAEPlaceholderHashReemplazarDesdeApp==',
    'PLACEHOLDER_SECURITY_STAMP',
    'PLACEHOLDER_CONCURRENCY_STAMP',
    '20000000-0000-0000-0000-000000000001'
);

-- Asignar rol SuperAdmin al usuario inicial
INSERT INTO user_roles (user_id, role_id)
VALUES ('20000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001');

-- Especialidades base
INSERT INTO specialties (id, tenant_id, name, code, color, is_active, created_by) VALUES
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Estructura y Hormigón',        'EST', '#EF4444', TRUE, '20000000-0000-0000-0000-000000000001'),
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Terminaciones',                'TER', '#F97316', TRUE, '20000000-0000-0000-0000-000000000001'),
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Instalaciones Sanitarias',     'SAN', '#3B82F6', TRUE, '20000000-0000-0000-0000-000000000001'),
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Instalaciones Eléctricas',     'ELE', '#EAB308', TRUE, '20000000-0000-0000-0000-000000000001'),
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Climatización y Ventilación',  'CLI', '#6366F1', TRUE, '20000000-0000-0000-0000-000000000001'),
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Impermeabilización',           'IMP', '#8B5CF6', TRUE, '20000000-0000-0000-0000-000000000001'),
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Seguridad y Prevención',       'SEG', '#EC4899', TRUE, '20000000-0000-0000-0000-000000000001'),
    (uuid_generate_v4(), '00000000-0000-0000-0000-000000000001', 'Paisajismo y Áreas Comunes',   'PAI', '#10B981', TRUE, '20000000-0000-0000-0000-000000000001');

-- ============================================================
-- FUNCIÓN: Generar código correlativo por tenant
-- ============================================================
CREATE OR REPLACE FUNCTION generate_sequence_code(
    p_tenant_id UUID,
    p_entity_type VARCHAR,
    p_prefix VARCHAR
) RETURNS VARCHAR AS $$
DECLARE
    v_year INT := EXTRACT(YEAR FROM NOW());
    v_next INT;
BEGIN
    INSERT INTO sequence_counters (tenant_id, entity_type, prefix, year, last_value)
    VALUES (p_tenant_id, p_entity_type, p_prefix, v_year, 1)
    ON CONFLICT (tenant_id, entity_type, year)
    DO UPDATE SET last_value = sequence_counters.last_value + 1
    RETURNING last_value INTO v_next;

    RETURN p_prefix || '-' || v_year || '-' || LPAD(v_next::TEXT, 4, '0');
END;
$$ LANGUAGE plpgsql;

-- ============================================================
-- COMENTARIOS DE TABLAS (documentación interna)
-- ============================================================
COMMENT ON TABLE tenants              IS 'Empresas/organizaciones que usan la plataforma (multi-tenant)';
COMMENT ON TABLE users                IS 'Usuarios del sistema. Extiende ASP.NET Identity';
COMMENT ON TABLE roles                IS 'Roles del sistema. Extiende ASP.NET Identity';
COMMENT ON TABLE companies            IS 'Empresas clientes (constructoras, inmobiliarias)';
COMMENT ON TABLE projects             IS 'Obras o proyectos de construcción';
COMMENT ON TABLE project_members      IS 'Usuarios asignados a un proyecto con su rol';
COMMENT ON TABLE project_stages       IS 'Etapas de obra (fundaciones, estructura, terminaciones, etc.)';
COMMENT ON TABLE project_sectors      IS 'Sectores jerárquicos: Torre > Piso > etc.';
COMMENT ON TABLE project_units        IS 'Unidades inspeccionables (departamentos, casas, bodegas)';
COMMENT ON TABLE specialties          IS 'Especialidades técnicas (eléctrica, sanitaria, estructura, etc.)';
COMMENT ON TABLE contractors          IS 'Contratistas responsables de trabajos';
COMMENT ON TABLE inspection_templates IS 'Plantillas configurables de inspección';
COMMENT ON TABLE template_sections    IS 'Secciones dentro de una plantilla';
COMMENT ON TABLE template_questions   IS 'Preguntas de una sección, con tipo y configuración';
COMMENT ON TABLE inspections          IS 'Inspecciones programadas y ejecutadas';
COMMENT ON TABLE inspection_answers   IS 'Respuestas a cada pregunta en una inspección';
COMMENT ON TABLE inspection_evidence  IS 'Archivos multimedia adjuntos a respuestas';
COMMENT ON TABLE observations         IS 'Observaciones y no conformidades detectadas';
COMMENT ON TABLE observation_history  IS 'Historial de cambios de estado de observaciones';
COMMENT ON TABLE reinspections        IS 'Reinspecciones para validar correcciones';
COMMENT ON TABLE project_documents    IS 'Documentos adjuntos a proyectos/inspecciones';
COMMENT ON TABLE notifications        IS 'Notificaciones internas del sistema';
COMMENT ON TABLE audit_logs           IS 'Registro de auditoría de todas las acciones';
COMMENT ON TABLE sequence_counters    IS 'Generador de correlativos por tenant y año';

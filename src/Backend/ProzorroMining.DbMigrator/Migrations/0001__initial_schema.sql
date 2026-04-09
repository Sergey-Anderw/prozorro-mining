CREATE TABLE IF NOT EXISTS tenders (
    id BIGSERIAL PRIMARY KEY,
    prozorro_tender_id TEXT NOT NULL,
    status TEXT NOT NULL,
    cpv_code TEXT,
    expected_amount NUMERIC(18,2),
    procuring_entity_name TEXT,
    tender_date TIMESTAMPTZ,
    date_created TIMESTAMPTZ NOT NULL,
    date_modified TIMESTAMPTZ NOT NULL,
    raw_payload JSONB,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_tenders_prozorro_tender_id ON tenders (prozorro_tender_id);
CREATE INDEX IF NOT EXISTS ix_tenders_cpv_status_date ON tenders (cpv_code, status, tender_date);
CREATE INDEX IF NOT EXISTS ix_tenders_procuring_entity_name ON tenders (procuring_entity_name);
CREATE INDEX IF NOT EXISTS ix_tenders_date_modified ON tenders (date_modified);

CREATE TABLE IF NOT EXISTS suppliers (
    id BIGSERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_suppliers_name ON suppliers (name);

CREATE TABLE IF NOT EXISTS contracts (
    id BIGSERIAL PRIMARY KEY,
    tender_id BIGINT NOT NULL REFERENCES tenders (id) ON DELETE CASCADE,
    contract_amount NUMERIC(18,2) NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_contracts_tender_id ON contracts (tender_id);

CREATE TABLE IF NOT EXISTS tender_suppliers (
    tender_id BIGINT NOT NULL REFERENCES tenders (id) ON DELETE CASCADE,
    supplier_id BIGINT NOT NULL REFERENCES suppliers (id),
    PRIMARY KEY (tender_id, supplier_id)
);

CREATE INDEX IF NOT EXISTS ix_tender_suppliers_supplier_id ON tender_suppliers (supplier_id);

CREATE TABLE IF NOT EXISTS import_runs (
    id BIGSERIAL PRIMARY KEY,
    started_at TIMESTAMPTZ NOT NULL,
    finished_at TIMESTAMPTZ,
    status TEXT NOT NULL,
    processed_count INTEGER NOT NULL DEFAULT 0,
    inserted_count INTEGER NOT NULL DEFAULT 0,
    updated_count INTEGER NOT NULL DEFAULT 0,
    failed_count INTEGER NOT NULL DEFAULT 0,
    error_message TEXT
);

CREATE TABLE IF NOT EXISTS import_checkpoint (
    id BIGSERIAL PRIMARY KEY,
    source_name TEXT NOT NULL,
    last_seen_date TIMESTAMPTZ,
    last_run_at TIMESTAMPTZ,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_import_checkpoint_source_name ON import_checkpoint (source_name);

-- ============================================================
--  MedicAlex — Schema Baza de Date
--  PostgreSQL 18
--  Toate parolele sunt stocate ca hash BCrypt (cost 12)
--  generat de backend-ul C# cu BCrypt.Net-Next
-- ============================================================

-- Extensii utile
CREATE EXTENSION IF NOT EXISTS "pgcrypto";  -- pentru gen_random_uuid() dacă e nevoie

-- ============================================================
--  ENUM-URI
-- ============================================================

CREATE TYPE user_role AS ENUM (
    'admin',
    'doctor_medicina',
    'doctor_laborator'
);

CREATE TYPE analysis_status AS ENUM (
    'pending',    -- cerere trimisă, nicio acceptare
    'accepted',   -- acceptată de un doctor laborator
    'completed'   -- rezultate încărcate
);

-- ============================================================
--  TABELUL: users
--  Stochează toți utilizatorii: admin, doctori medicină,
--  doctori laborator.
--  password_hash = BCrypt hash (cost 12) generat în C#
-- ============================================================

CREATE TABLE IF NOT EXISTS users (
    id              SERIAL          PRIMARY KEY,
    email           VARCHAR(255)    NOT NULL UNIQUE,
    password_hash   VARCHAR(255)    NOT NULL,
    first_name      VARCHAR(100)    NOT NULL,
    last_name       VARCHAR(100)    NOT NULL,
    role            user_role       NOT NULL,
    specialization  VARCHAR(150),           -- null pentru admin
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

-- Index pentru login rapid
CREATE INDEX IF NOT EXISTS idx_users_email ON users (email);
CREATE INDEX IF NOT EXISTS idx_users_role  ON users (role);

-- ============================================================
--  TABELUL: patients
--  Profilurile pacienților.
--  doctor_id = doctorul de medicină responsabil (poate fi NULL
--  dacă doctorul a fost șters sau pacientul e neasignat)
-- ============================================================

CREATE TABLE IF NOT EXISTS patients (
    id                  SERIAL          PRIMARY KEY,
    first_name          VARCHAR(100)    NOT NULL,
    last_name           VARCHAR(100)    NOT NULL,
    age                 SMALLINT        NOT NULL
                            CHECK (age > 0 AND age < 150),
    gender              VARCHAR(30)     NOT NULL,
    medical_description TEXT,
    doctor_id           INTEGER
                            REFERENCES users (id)
                            ON DELETE SET NULL,     -- dacă doctorul e șters, pacientul rămâne
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_patients_doctor ON patients (doctor_id);
CREATE INDEX IF NOT EXISTS idx_patients_name   ON patients (last_name, first_name);

-- ============================================================
--  TABELUL: analysis_requests
--  Cereri de analize făcute de doctori de medicină.
--  Când un doctor laborator acceptă → status = 'accepted'
--                                    accepted_by = id-ul lui
--  Când se încarcă PDF            → status = 'completed'
-- ============================================================

CREATE TABLE IF NOT EXISTS analysis_requests (
    id                      SERIAL          PRIMARY KEY,
    patient_id              INTEGER         NOT NULL
                                REFERENCES patients (id)
                                ON DELETE CASCADE,
    requesting_doctor_id    INTEGER         NOT NULL
                                REFERENCES users (id)
                                ON DELETE CASCADE,
    analysis_type           VARCHAR(255)    NOT NULL,
    status                  analysis_status NOT NULL DEFAULT 'pending',
    accepted_by             INTEGER
                                REFERENCES users (id)
                                ON DELETE SET NULL,
    created_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at              TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_analysis_req_patient  ON analysis_requests (patient_id);
CREATE INDEX IF NOT EXISTS idx_analysis_req_status   ON analysis_requests (status);
CREATE INDEX IF NOT EXISTS idx_analysis_req_doctor   ON analysis_requests (requesting_doctor_id);

-- ============================================================
--  TABELUL: analysis_results
--  Rezultatele analizelor (fișiere PDF) încărcate de
--  doctorii de laborator.
--  Un pacient poate avea mai multe rezultate (istoricul se
--  păstrează — nu se suprascrie).
-- ============================================================

CREATE TABLE IF NOT EXISTS analysis_results (
    id                  SERIAL          PRIMARY KEY,
    patient_id          INTEGER         NOT NULL
                            REFERENCES patients (id)
                            ON DELETE CASCADE,
    analysis_request_id INTEGER
                            REFERENCES analysis_requests (id)
                            ON DELETE SET NULL,     -- poate fi fără cerere asociată
    lab_doctor_id       INTEGER         NOT NULL
                            REFERENCES users (id)
                            ON DELETE RESTRICT,    -- nu șterge doctorul dacă are rezultate
    pdf_filename        VARCHAR(500)    NOT NULL,   -- numele fișierului pe server
    pdf_original_name   VARCHAR(500)    NOT NULL,   -- numele original dat de doctor
    description         VARCHAR(500),
    file_size_bytes     INTEGER,
    uploaded_at         TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_results_patient    ON analysis_results (patient_id);
CREATE INDEX IF NOT EXISTS idx_results_lab_doctor ON analysis_results (lab_doctor_id);
CREATE INDEX IF NOT EXISTS idx_results_request    ON analysis_results (analysis_request_id);

-- ============================================================
--  TABELUL: notifications
--  Notificări pentru doctorii de laborator.
--  Când un doctor medicină face o cerere → se creează câte o
--  notificare pentru FIECARE doctor laborator.
--  Când cererea e acceptată → notificările celorlalți se șterg.
-- ============================================================

CREATE TABLE IF NOT EXISTS notifications (
    id                  SERIAL          PRIMARY KEY,
    user_id             INTEGER         NOT NULL
                            REFERENCES users (id)
                            ON DELETE CASCADE,
    message             TEXT            NOT NULL,
    is_read             BOOLEAN         NOT NULL DEFAULT FALSE,
    related_request_id  INTEGER
                            REFERENCES analysis_requests (id)
                            ON DELETE CASCADE,    -- ștergere în cascadă
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_notif_user   ON notifications (user_id, is_read);
CREATE INDEX IF NOT EXISTS idx_notif_req    ON notifications (related_request_id);

-- ============================================================
--  FUNCȚIE: updated_at auto-update trigger
--  Actualizează automat câmpul updated_at la orice modificare
-- ============================================================

CREATE OR REPLACE FUNCTION trigger_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Aplică trigger-ul pe tabelele cu updated_at
CREATE TRIGGER set_updated_at_users
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER set_updated_at_patients
    BEFORE UPDATE ON patients
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER set_updated_at_analysis_requests
    BEFORE UPDATE ON analysis_requests
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

-- ============================================================
--  VIEW: v_patients_with_doctor
--  View util pentru a vedea pacienții cu datele doctorului
-- ============================================================

CREATE OR REPLACE VIEW v_patients_with_doctor AS
SELECT
    p.id,
    p.first_name,
    p.last_name,
    p.age,
    p.gender,
    p.medical_description,
    p.doctor_id,
    p.created_at,
    p.updated_at,
    u.first_name    AS doctor_first_name,
    u.last_name     AS doctor_last_name,
    u.email         AS doctor_email,
    u.specialization AS doctor_specialization
FROM patients p
LEFT JOIN users u ON p.doctor_id = u.id;

-- ============================================================
--  VIEW: v_analysis_requests_full
--  View util pentru cereri cu toate datele asociate
-- ============================================================

CREATE OR REPLACE VIEW v_analysis_requests_full AS
SELECT
    ar.id,
    ar.analysis_type,
    ar.status,
    ar.created_at,
    ar.updated_at,
    -- Pacient
    p.id            AS patient_id,
    p.first_name    AS patient_first_name,
    p.last_name     AS patient_last_name,
    -- Doctor solicitant
    d.id            AS requesting_doctor_id,
    d.first_name    AS requesting_doctor_first_name,
    d.last_name     AS requesting_doctor_last_name,
    d.specialization AS requesting_doctor_spec,
    -- Doctor laborator (acceptant)
    l.id            AS accepted_by_id,
    l.first_name    AS accepted_by_first_name,
    l.last_name     AS accepted_by_last_name
FROM analysis_requests ar
JOIN  patients p ON ar.patient_id           = p.id
JOIN  users    d ON ar.requesting_doctor_id = d.id
LEFT JOIN users l ON ar.accepted_by         = l.id;

-- ============================================================
--  SEED: Admin inițial
--
--  IMPORTANT: password_hash-ul de mai jos este un PLACEHOLDER.
--  Backend-ul C# va detecta că hash-ul nu e valid BCrypt și va
--  reseta parola la Admin@1234 la primul start.
--  SAU: backend-ul inserează adminul direct cu hash corect.
--
--  Email:   admin@medicalex.ro
--  Parola:  Admin@1234  (schimbă-o după primul login!)
-- ============================================================

INSERT INTO users (email, password_hash, first_name, last_name, role)
VALUES (
    'admin@medicalex.ro',
    'NEEDS_BACKEND_HASH',   -- backend-ul C# va înlocui acesta la startup
    'Administrator',
    'MedicAlex',
    'admin'
)
ON CONFLICT (email) DO NOTHING;

-- ============================================================
--  COMENTARII pe coloane (documentație în DB)
-- ============================================================

COMMENT ON TABLE users              IS 'Utilizatorii sistemului MedicAlex: admin, doctori medicină, doctori laborator';
COMMENT ON COLUMN users.password_hash IS 'Hash BCrypt (cost 12) generat cu BCrypt.Net-Next în C#. NU se stochează parola în clar.';
COMMENT ON COLUMN users.is_active   IS 'Cont activ/dezactivat. Doctorii șterși sunt dezactivați, nu eliminați dacă au date asociate.';

COMMENT ON TABLE patients           IS 'Profilurile medicale ale pacienților';
COMMENT ON TABLE analysis_requests  IS 'Cereri de analize medicale de la doctorii de medicină către laborator';
COMMENT ON TABLE analysis_results   IS 'Fișiere PDF cu rezultatele analizelor încărcate de doctorii de laborator';
COMMENT ON TABLE notifications      IS 'Notificări pentru doctorii de laborator la cereri noi de analize';

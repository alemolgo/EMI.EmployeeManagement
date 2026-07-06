------------------------------------------------------------------------------
-- OBJECTIVE:                 Create role structure for authorization in EMI database.
-- OBJECTIVE TABLE:           Table
-- OBJECT NAME:               emi.role
-- IMPUT PARAMETER:
-- OUTPUT PARAETER:
-- TECNICAL LEADER:
-- DATE:                      05/07/2026
-- MADE BY:                   EMI (ING. ALEXANDER MOLINA)
-- UPDATE DATE:
-- UPDATE LEADER:
-- UPDATE OBJECTIVE:
------------------------------------------------------------------------------

DROP TABLE IF EXISTS "emi"."role";
DROP SEQUENCE IF EXISTS "emi"."role_code_seq";

CREATE SEQUENCE "emi"."role_code_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 99999
    CACHE 1;

ALTER SEQUENCE "emi"."role_code_seq" OWNER TO postgresqluser;

CREATE TABLE "emi"."role"
(
    "role_id"        INTEGER         NOT NULL DEFAULT nextval('"emi"."role_code_seq"'),
    "role_name"      VARCHAR(50)     NOT NULL,

    CONSTRAINT "role_pkey" PRIMARY KEY ("role_id"),
    CONSTRAINT "role_ukey" UNIQUE ("role_name")
);



--------------------------------------------------------------------------------
---------------------------Intial Data-------------------------------------------
INSERT INTO "emi"."role" ("role_name")
VALUES
('Admin'),
('User');
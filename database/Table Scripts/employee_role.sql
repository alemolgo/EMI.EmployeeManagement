------------------------------------------------------------------------------
-- OBJECTIVE:                  Map employees to roles for authorization in EMI database.
-- OBJECTIVE TABLE:            Table
-- OBJECT NAME:                emi.employee_role
-- IMPUT PARAMETER:
-- OUTPUT PARAETER:
-- TECNICAL LEADER:
-- DATE:                       05/07/2026
-- MADE BY:                    EMI (ING. ALEXANDER MOLINA)
-- UPDATE DATE:
-- UPDATE LEADER:
-- UPDATE OBJEBTIVE:
------------------------------------------------------------------------------

DROP TABLE IF EXISTS "emi"."employee_role";

CREATE TABLE "emi"."employee_role"
(
    "employee_role_id"     INTEGER NOT NULL DEFAULT nextval('emi.employee_code_seq'),
    "employee_id"          INTEGER NOT NULL,
    "role_id"              INTEGER NOT NULL,


    CONSTRAINT "employee_role_pkey" 			PRIMARY KEY ("employee_role_id"),
    CONSTRAINT "employee_role_employee_fkey"	FOREIGN KEY ("employee_id")	REFERENCES "emi"."employee"("employee_id")	ON DELETE CASCADE	ON UPDATE CASCADE,
    CONSTRAINT "employee_role_role_fkey"		FOREIGN KEY ("role_id")		REFERENCES "emi"."role"("role_id")			ON DELETE RESTRICT	ON UPDATE CASCADE
);

-- =========================================================
-- RULE: The same combination of employee_id + role_id cannot be repeated.
-- =========================================================
CREATE UNIQUE INDEX "uq_employee_role" ON "emi"."employee_role" ("employee_id", "role_id");

--------------------------------------------------------------------------------
---------------------------Initial Data-------------------------------------------
INSERT INTO "emi"."employee_role" ("employee_id", "role_id")
VALUES
(1, 1);

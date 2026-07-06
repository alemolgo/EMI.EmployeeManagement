------------------------------------------------------------------------------
-- OBJECTIVE: 					Create data structure for Position_History in EMI database.
-- OBJECTIVE TABLE:				Table
-- OBJECT NAME:					emi.Position_History
-- IMPUT PARAMETER: 		
-- OUTPUT PARAETER: 		 
-- TECNICAL LEADER: 				
-- DATE: 						03/07/2026
-- MADE BY:						emi (ING. ALEXANDER MOLINA)
-- UPDATE DATE: 	  
-- UPDATE LEADER: 	    
-- UPDATE OBJEBTIVE: 
------------------------------------------------------------------------------

DROP TABLE IF EXISTS "emi"."position_history";

DROP SEQUENCE IF EXISTS "emi"."position_history_code_seq";

CREATE SEQUENCE "emi"."position_history_code_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 99999
    CACHE 1;

ALTER SEQUENCE "emi"."position_history_code_seq" OWNER TO postgresqluser;


CREATE TABLE "emi"."position_history"
(	
	"position_history_id"					INTEGER			NOT NULL	DEFAULT nextval('"emi"."position_history_code_seq"'),
	"position_history_employee_id"			INTEGER			NOT NULL,
	"position_history_position_id"			INTEGER			NOT NULL,
	"position_history_is_active"			BOOLEAN 		NOT NULL	DEFAULT TRUE,
	"position_history_start_date"    		TIMESTAMPTZ 	NOT NULL,
	"position_history_end_date"     		TIMESTAMPTZ,
			
			
	CONSTRAINT "position_history_pkey" 					PRIMARY KEY ("position_history_id")	
   ,CONSTRAINT "position_history_employee_id_fkey"		FOREIGN KEY ("position_history_employee_id")	REFERENCES "emi"."employee"("employee_id")	ON DELETE RESTRICT	ON UPDATE CASCADE
   ,CONSTRAINT "position_history_position_id_fkey"		FOREIGN KEY ("position_history_position_id")	REFERENCES "emi"."position"("position_id")	ON DELETE RESTRICT	ON UPDATE CASCADE
 );


-- =========================================================
-- RULE: Only one active position per employee
-- =========================================================
CREATE UNIQUE INDEX "uq_position_history_employee_active" ON "emi"."position_history" ("position_history_employee_id") WHERE "position_history_is_active";
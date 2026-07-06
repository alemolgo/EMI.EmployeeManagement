------------------------------------------------------------------------------
-- OBJECTIVE: 					Create data structure for Employee in EMI database.
-- OBJECTIVE TABLE:				Table
-- OBJECT NAME:					emi.employee
-- IMPUT PARAMETER: 		
-- OUTPUT PARAETER: 		 
-- TECNICAL LEADER: 				
-- DATE: 						03/07/2026
-- MADE BY:						EMI (ING. ALEXANDER MOLINA)
-- UPDATE DATE: 	  
-- UPDATE LEADER: 	    
-- UPDATE OBJEBTIVE: 
------------------------------------------------------------------------------

DROP TABLE IF EXISTS "emi"."employee";

DROP SEQUENCE IF EXISTS "emi"."employee_code_seq";

CREATE SEQUENCE "emi"."employee_code_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 99999
    CACHE 1;

ALTER SEQUENCE "emi"."employee_code_seq" OWNER TO postgresqluser;


CREATE TABLE "emi"."employee"
(	
	"employee_id"					INTEGER			NOT NULL	DEFAULT nextval('"emi"."employee_code_seq"'),
	"employee_name"					VARCHAR(100)	NOT NULL,
	"employee_current_position_id"	INTEGER		 	NOT NULL,
	"employee_salary"				DECIMAL(18,2) 	NOT NULL,
	"employee_password_hash"       	VARCHAR(255)	 NOT NULL,	
	CONSTRAINT "employee_pkey" 					PRIMARY KEY ("employee_id")	
   ,CONSTRAINT "employee_current_position_fkey"	FOREIGN KEY ("employee_current_position_id")	REFERENCES "emi"."position"("position_id")	ON DELETE RESTRICT	ON UPDATE CASCADE
);


--------------------------------------------------------------------------------
---------------------------Initial Data-------------------------------------------
INSERT INTO "emi"."employee" ("employee_name", "employee_current_position_id", "employee_salary", "employee_password_hash")
VALUES
('admin', 1, 0.00, 'admin1234');


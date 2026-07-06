------------------------------------------------------------------------------
-- OBJECTIVE: 					Create data structure for Position in EMI database.
-- OBJECTIVE TABLE:				Table
-- OBJECT NAME:					emi.position
-- IMPUT PARAMETER: 		
-- OUTPUT PARAETER: 		 
-- TECNICAL LEADER: 				
-- DATE: 						03/07/2026
-- MADE BY:						EMI (ING. ALEXANDER MOLINA)
-- UPDATE DATE: 	  
-- UPDATE LEADER: 	    
-- UPDATE OBJEBTIVE: 
------------------------------------------------------------------------------

CREATE SCHEMA IF NOT EXISTS emi AUTHORIZATION postgresqluser;

DROP TABLE IF EXISTS "emi"."position";

DROP SEQUENCE IF EXISTS "emi"."position_code_seq";

CREATE SEQUENCE "emi"."position_code_seq"
    INCREMENT 1
    START 1
    MINVALUE 1
    MAXVALUE 99999
    CACHE 1;

ALTER SEQUENCE "emi"."position_code_seq" OWNER TO postgresqluser;


CREATE TABLE "emi"."position"
(	
	"position_id"					INTEGER			NOT NULL	DEFAULT nextval('"emi"."position_code_seq"'),
	"position_name"					VARCHAR(50)		NOT NULL,
	"position_is_manager"			BOOLEAN  		NOT NULL DEFAULT FALSE,
				
			
	CONSTRAINT "position_pkey"	PRIMARY KEY ("position_id")
   ,CONSTRAINT "position_ukey" 	UNIQUE ("position_name")
);


INSERT INTO "emi"."position" ("position_name", "position_is_manager")
VALUES
('Gerente General', TRUE),
('Gerente de Recursos Humanos', TRUE),
('Gerente de Sistemas', TRUE),
('Gerente Financiero', TRUE),
('Analista de Sistemas', FALSE),
('Desarrollador Backend', FALSE),
('Desarrollador Frontend', FALSE),
('QA Tester', FALSE),
('Soporte Técnico', FALSE),
('DBA (Administrador de Base de Datos)', FALSE),
('Arquitecto de Software', TRUE),
('Líder Técnico', TRUE);

--Scaffold-DbContext "Host=localhost;Port=5432;Database=emi;Username=postgresqluser;Password=postgressqlpass" Npgsql.EntityFrameworkCore.PostgreSQL -OutputDir Models -Context AppDbContext -Schemas emi -Force
-- ============================================================
-- ITO Cloud — Datos de prueba (seed)
-- Versión: 1.0 | Fecha: 2026-03-18
-- Password de todos los usuarios: Test1234!
-- ============================================================

BEGIN;

-- ============================================================
-- TENANTS
-- ============================================================
INSERT INTO tenants (id, name, slug, plan, is_active, primary_color, max_users, max_projects) VALUES
  ('11111111-0000-0000-0000-000000000001', 'ITO Pacífico SpA',          'ito-pacifico',  'professional', TRUE, '#1F3864', 20, 15),
  ('22222222-0000-0000-0000-000000000001', 'Consultores Sur Ltda',       'consultores-sur','basic',       TRUE, '#1A6B3C',  8,  5);

-- ============================================================
-- ROLES
-- ============================================================
INSERT INTO roles (id, tenant_id, name, normalized_name, description, is_system_role) VALUES
  ('aaaaaaaa-0000-0000-0000-000000000001', NULL, 'Administrador', 'ADMINISTRADOR', 'Acceso total al sistema del tenant', TRUE),
  ('aaaaaaaa-0000-0000-0000-000000000002',   NULL, 'Supervisor',    'SUPERVISOR',    'Supervisa inspecciones y reportes',  TRUE),
  ('aaaaaaaa-0000-0000-0000-000000000003',   NULL, 'Inspector',     'INSPECTOR',     'Ejecuta inspecciones en terreno',    TRUE),
  ('aaaaaaaa-0000-0000-0000-000000000004',  NULL, 'Contratista',   'CONTRATISTA',   'Visualiza observaciones asignadas',  TRUE);

-- ============================================================
-- USUARIOS — Tenant 1: ITO Pacífico SpA
-- ============================================================
INSERT INTO users (id, tenant_id, user_name, normalized_user_name, email, normalized_email,
  email_confirmed, password_hash, security_stamp, concurrency_stamp,
  first_name, last_name, rut, position, is_active, created_at, created_by) VALUES
  ('11111111-1111-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','admin.pacifico','ADMIN.PACIFICO','admin@itopacifico.cl','ADMIN@ITOPACIFICO.CL',
   TRUE,'AQAAAAIAACcQAAAAENHitJpuRAw9+o0wZBPOVAdPcGlD31ZcqN2dYBsFam8wGSRVMJU1bXRXjM0HcIxp2A==','f9ac3eb3-c90f-477d-8d04-e2442e673cb9','448665bb-b67e-4442-97c2-fa75e7289c12',
   'Rodrigo','Fuentes Vidal','12.345.678-9','Administrador ITO',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-1111-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','carlos.mendez','CARLOS.MENDEZ','carlos.mendez@itopacifico.cl','CARLOS.MENDEZ@ITOPACIFICO.CL',
   TRUE,'AQAAAAIAACcQAAAAENHitJpuRAw9+o0wZBPOVAdPcGlD31ZcqN2dYBsFam8wGSRVMJU1bXRXjM0HcIxp2A==','ac54fd85-6eef-42c3-a746-cc2ca56a3082','4a07078c-dbb5-41f6-9e99-fd6bb4c32064',
   'Carlos','Méndez Torres','13.456.789-0','Supervisor de Obra',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-1111-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','pablo.rojas','PABLO.ROJAS','pablo.rojas@itopacifico.cl','PABLO.ROJAS@ITOPACIFICO.CL',
   TRUE,'AQAAAAIAACcQAAAAENHitJpuRAw9+o0wZBPOVAdPcGlD31ZcqN2dYBsFam8wGSRVMJU1bXRXjM0HcIxp2A==','eb0d0276-4d09-42df-b9cd-9ae323e7dc06','8dc4299f-7385-4876-b164-c76cb9d90114',
   'Pablo','Rojas Soto','14.567.890-1','Inspector ITO Senior',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-1111-0000-0000-000000000004','11111111-0000-0000-0000-000000000001','ana.gonzalez','ANA.GONZALEZ','ana.gonzalez@itopacifico.cl','ANA.GONZALEZ@ITOPACIFICO.CL',
   TRUE,'AQAAAAIAACcQAAAAENHitJpuRAw9+o0wZBPOVAdPcGlD31ZcqN2dYBsFam8wGSRVMJU1bXRXjM0HcIxp2A==','8e69f8be-a466-44f9-9276-6b40237d1d45','9a939ce1-0094-4400-b15f-8e3df2f0baef',
   'Ana','González Muñoz','15.678.901-2','Inspector ITO',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-1111-0000-0000-000000000005','11111111-0000-0000-0000-000000000001','juan.herrera','JUAN.HERRERA','juan.herrera@estructurassur.cl','JUAN.HERRERA@ESTRUCTURASSUR.CL',
   TRUE,'AQAAAAIAACcQAAAAENHitJpuRAw9+o0wZBPOVAdPcGlD31ZcqN2dYBsFam8wGSRVMJU1bXRXjM0HcIxp2A==','5d2668e6-7c32-4d9e-bce5-c8635b987379','d6578fe4-ce6e-44d6-a5cd-ac04b2e97ce3',
   'Juan','Herrera Lagos','16.789.012-3','Jefe de Obra',TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- USUARIOS — Tenant 2: Consultores Sur Ltda
-- ============================================================
INSERT INTO users (id, tenant_id, user_name, normalized_user_name, email, normalized_email,
  email_confirmed, password_hash, security_stamp, concurrency_stamp,
  first_name, last_name, rut, position, is_active, created_at, created_by) VALUES
  ('22222222-1111-0000-0000-000000000001','22222222-0000-0000-0000-000000000001','admin.sur','ADMIN.SUR','admin@consultores-sur.cl','ADMIN@CONSULTORES-SUR.CL',
   TRUE,'AQAAAAIAACcQAAAAENHitJpuRAw9+o0wZBPOVAdPcGlD31ZcqN2dYBsFam8wGSRVMJU1bXRXjM0HcIxp2A==','1bd8199c-1d17-4b62-8a15-217e3264d2d4','8c1bf032-acac-42e1-9a3a-70b9a4015898',
   'Marcela','Ríos Pinto','17.890.123-4','Directora',TRUE,NOW(),'22222222-1111-0000-0000-000000000001'),

  ('22222222-1111-0000-0000-000000000002','22222222-0000-0000-0000-000000000001','diego.silva','DIEGO.SILVA','diego.silva@consultores-sur.cl','DIEGO.SILVA@CONSULTORES-SUR.CL',
   TRUE,'AQAAAAIAACcQAAAAENHitJpuRAw9+o0wZBPOVAdPcGlD31ZcqN2dYBsFam8wGSRVMJU1bXRXjM0HcIxp2A==','573cb027-c944-4583-b687-36e0e96e59fe','c72c92c7-57ee-406c-afec-d5e96dd76430',
   'Diego','Silva Araya','18.901.234-5','Inspector',TRUE,NOW(),'22222222-1111-0000-0000-000000000001');

-- ============================================================
-- USER ROLES
-- ============================================================
INSERT INTO user_roles (user_id, role_id) VALUES
  ('11111111-1111-0000-0000-000000000001', 'aaaaaaaa-0000-0000-0000-000000000001'),
  ('11111111-1111-0000-0000-000000000002', 'aaaaaaaa-0000-0000-0000-000000000002'),
  ('11111111-1111-0000-0000-000000000003', 'aaaaaaaa-0000-0000-0000-000000000003'),
  ('11111111-1111-0000-0000-000000000004', 'aaaaaaaa-0000-0000-0000-000000000003'),
  ('11111111-1111-0000-0000-000000000005','aaaaaaaa-0000-0000-0000-000000000004'),
  ('22222222-1111-0000-0000-000000000001', 'aaaaaaaa-0000-0000-0000-000000000001'),
  ('22222222-1111-0000-0000-000000000002', 'aaaaaaaa-0000-0000-0000-000000000003');

-- ============================================================
-- EMPRESAS
-- ============================================================
INSERT INTO companies (id, tenant_id, name, rut, business_name, company_type, address, city, region,
  phone, email, website, is_active, created_at, created_by) VALUES
  ('11111111-2222-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','Constructora Pacífico SpA','96.123.456-7','Constructora Pacífico Sociedad por Acciones',
   'constructora','Av. Apoquindo 4501, Of. 702','Santiago','Región Metropolitana',
   '+56 2 2345 6789','contacto@constructorapacifico.cl','www.constructorapacifico.cl',
   TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-2222-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','Inmobiliaria Las Palmas S.A.','97.234.567-8','Inmobiliaria Las Palmas Sociedad Anónima',
   'inmobiliaria','Av. Las Condes 11.000, Piso 3','Las Condes','Región Metropolitana',
   '+56 2 2456 7890','info@laspalmasinmobiliaria.cl','www.laspalmasinmobiliaria.cl',
   TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('22222222-2222-0000-0000-000000000001','22222222-0000-0000-0000-000000000001','Constructora Sur Ltda.','78.345.678-9','Constructora Sur Limitada',
   'constructora','Av. Independencia 1250','Concepción','Biobío',
   '+56 41 234 5678','admin@constructorasur.cl',NULL,
   TRUE,NOW(),'22222222-1111-0000-0000-000000000001');

-- ============================================================
-- PROYECTOS
-- ============================================================
INSERT INTO projects (id, tenant_id, company_id, code, name, description, project_type, status,
  address, city, region, latitude, longitude, start_date, estimated_end_date,
  total_units, ito_manager_id, mandante_name, mandante_contact, mandante_email,
  construction_permit, is_active, created_at, created_by) VALUES
  ('11111111-3333-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-2222-0000-0000-000000000002','LP-2024-001','Edificio Las Palmas — Torre Norte',
   'Edificio residencial de 18 pisos con 120 departamentos, 2 subterráneos y áreas comunes.',
   'edificio','activo','Av. Las Palmas 1250','Vitacura','Región Metropolitana',
   -33.3890,-70.5765,'2024-03-01','2026-09-30',120,'11111111-1111-0000-0000-000000000002',
   'Inmobiliaria Las Palmas S.A.','Pedro Contreras Lara','pcontreras@laspalmasinmobiliaria.cl',
   'DOM-VIT-2024-0123',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-3333-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-2222-0000-0000-000000000001','CRN-2024-002','Conjunto Residencial Norte — Fase I',
   'Conjunto de 60 casas pareadas de 2 pisos, urbanización completa.',
   'conjunto_casas','activo','Parcela 5, Camino El Valle Km 12','Colina','Región Metropolitana',
   -33.2012,-70.6703,'2024-06-01','2025-12-31',60,'11111111-1111-0000-0000-000000000002',
   'Constructora Pacífico SpA','Rodrigo Fuentes','rfuentes@constructorapacifico.cl',
   'DOM-COL-2024-0456',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('22222222-3333-0000-0000-000000000001','22222222-0000-0000-0000-000000000001','22222222-2222-0000-0000-000000000001','CV-2025-001','Edificio Costa Verde',
   'Edificio de 12 pisos, 80 departamentos en la ciudad de Concepción.',
   'edificio','activo','Av. O''Higgins 750','Concepción','Biobío',
   -36.8201,-73.0444,'2025-01-15','2026-06-30',80,'22222222-1111-0000-0000-000000000001',
   'Constructora Sur Ltda.','Marcela Ríos','mrios@constructorasur.cl',
   'MUN-CCP-2025-0012',TRUE,NOW(),'22222222-1111-0000-0000-000000000001');

-- ============================================================
-- ETAPAS DE PROYECTO (Proyecto 1: Edificio Las Palmas)
-- ============================================================
INSERT INTO project_stages (id, tenant_id, project_id, name, description, order_index, status,
  start_date, end_date, created_at, created_by) VALUES
  ('11111111-4444-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','Obra Gruesa','Estructura, hormigón y fierros',1,'completada','2024-03-01','2025-02-28',NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-4444-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','Terminaciones','Pisos, pintura, ventanas y revestimientos',2,'en_progreso','2025-03-01','2026-03-31',NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-4444-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','Instalaciones y Recepción','EESS, AACC, entrega final',3,'pendiente','2026-04-01','2026-09-30',NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- SECTORES
-- ============================================================
INSERT INTO project_sectors (id, tenant_id, project_id, name, sector_type, order_index, created_at, created_by) VALUES
  ('11111111-5555-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','Torre A','torre',1,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-5555-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','Torre B','torre',2,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-5555-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','Subterráneo -1','subterraneo',3,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- UNIDADES (muestra)
-- ============================================================
INSERT INTO project_units (id, tenant_id, project_id, sector_id, unit_code, unit_type, floor, surface_m2, status, owner_name, owner_email, is_active, created_at, created_by) VALUES
  ('11111111-ffff-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-5555-0000-0000-000000000001','301','departamento',3,67.50,'recepcion_preliminar',NULL,NULL,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-ffff-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-5555-0000-0000-000000000001','302','departamento',3,67.50,'recepcion_preliminar',NULL,NULL,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-ffff-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-5555-0000-0000-000000000001','303','departamento',3,67.50,'recepcion_preliminar',NULL,NULL,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-ffff-0000-0000-000000000004','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-5555-0000-0000-000000000002','401','departamento',4,67.50,'recepcion_preliminar',NULL,NULL,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-ffff-0000-0000-000000000005','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-5555-0000-0000-000000000002','402','departamento',4,67.50,'recepcion_preliminar',NULL,NULL,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-ffff-0000-0000-000000000006','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-5555-0000-0000-000000000002','403','departamento',4,67.50,'recepcion_preliminar',NULL,NULL,TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- ESPECIALIDADES
-- ============================================================
INSERT INTO specialties (id, tenant_id, name, code, description, color, is_active, created_at, created_by) VALUES
  ('11111111-6666-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','Estructural',        'EST','Hormigón, fierros, moldajes y obra gruesa',     '#1F3864',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-6666-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','Terminaciones',      'TER','Pisos, revestimientos, pintura y ventanas',     '#26A69A',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-6666-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','Instalaciones',      'INS','Instalaciones eléctricas, sanitarias y gas',    '#F4A261',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-6666-0000-0000-000000000004','11111111-0000-0000-0000-000000000001','Impermeabilización', 'IMP','Techumbres, terrazas y muros perimetrales',     '#E76F51',TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- CONTRATISTAS
-- ============================================================
INSERT INTO contractors (id, tenant_id, company_id, name, rut, contact_name, contact_email, contact_phone, is_active, created_at, created_by) VALUES
  ('11111111-7777-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-2222-0000-0000-000000000001','Estructuras Del Sur Ltda','79.111.222-3','Miguel Fuentes','mfuentes@estructurassur.cl','+56 9 8111 2233',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-7777-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-2222-0000-0000-000000000001','Terminaciones Norte SpA', '80.222.333-4','Claudia Vega',  'cvega@terminacionesnorte.cl','+56 9 8222 3344',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-7777-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-2222-0000-0000-000000000001','Instalaciones Rápidas SA', '81.333.444-5','Andrés Mora',  'amora@instalrapidas.cl',    '+56 9 8333 4455',TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

INSERT INTO contractor_specialties (contractor_id, specialty_id) VALUES
  ('11111111-7777-0000-0000-000000000001','11111111-6666-0000-0000-000000000001'),('11111111-7777-0000-0000-000000000002','11111111-6666-0000-0000-000000000002'),('11111111-7777-0000-0000-000000000003','11111111-6666-0000-0000-000000000003');

INSERT INTO project_contractors (id, tenant_id, project_id, contractor_id, specialty_id, is_active, assigned_at, assigned_by) VALUES
  ('2a8c9c75-1784-46e4-bdd9-c2979337fd37','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-7777-0000-0000-000000000001','11111111-6666-0000-0000-000000000001',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('84c69ff4-ae5f-4c44-9360-042419259057','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-7777-0000-0000-000000000002','11111111-6666-0000-0000-000000000002',TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('2c002e4b-b7e6-4202-9904-bf13b506e84c','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-7777-0000-0000-000000000003','11111111-6666-0000-0000-000000000003',TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- MIEMBROS DEL PROYECTO
-- ============================================================
INSERT INTO project_members (id, tenant_id, project_id, user_id, project_role, is_active, assigned_at, assigned_by) VALUES
  ('a90f93d9-e9ca-4ad6-b558-afe03ab06974','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-1111-0000-0000-000000000002', 'supervisor',  TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('ee7fe823-18b1-4910-8429-b90161c87c88','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-1111-0000-0000-000000000003', 'inspector',   TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('3dafc8ff-7599-43e5-94ad-0b3a65cb2049','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-1111-0000-0000-000000000004', 'inspector',   TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('be70aea8-718d-47d2-83a9-5522532246a1','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-1111-0000-0000-000000000005','contratista',  TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('0b9a99d8-3f44-41fd-ad5b-ed2c9014b237','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000002','11111111-1111-0000-0000-000000000002', 'supervisor',  TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('b56ae78f-3569-42f5-b873-fc583daea441','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000002','11111111-1111-0000-0000-000000000003', 'inspector',   TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- PLANTILLAS DE INSPECCIÓN
-- ============================================================
INSERT INTO inspection_templates (id, tenant_id, name, description, template_type, specialty_id,
  status, current_version, is_global, allow_partial_save, require_geolocation,
  passing_score, created_at, created_by) VALUES
  ('11111111-8888-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','Inspección Obra Gruesa v2',
   'Checklist completo para revisión de hormigón armado, enfierradura y moldajes.',
   'obra_gruesa','11111111-6666-0000-0000-000000000001','activa',2,FALSE,TRUE,TRUE,70.0,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-8888-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','Inspección Terminaciones',
   'Revisión de pisos, revestimientos, pintura, ventanas y puertas.',
   'terminaciones','11111111-6666-0000-0000-000000000002','activa',1,FALSE,TRUE,FALSE,80.0,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-8888-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','Recepción de Unidad',
   'Formulario de recepción de departamento o casa al propietario.',
   'recepcion',NULL,'activa',1,TRUE,TRUE,FALSE,90.0,NOW(),'11111111-1111-0000-0000-000000000001');

INSERT INTO template_sections (id, tenant_id, template_id, title, description, order_index,
  is_required, weight, is_active, created_at, created_by) VALUES
  ('11111111-9999-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-8888-0000-0000-000000000001','Hormigón','Revisión de calidad del vaciado y curado',1,TRUE,1.5,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-9999-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-8888-0000-0000-000000000001','Enfierradura','Control de enfierradura antes del vaciado',2,TRUE,2.0,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-9999-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-8888-0000-0000-000000000001','Moldajes','Revisión de moldajes y puntales',3,TRUE,1.0,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-9999-0000-0000-000000000010','11111111-0000-0000-0000-000000000001','11111111-8888-0000-0000-000000000002','Pisos','Control de instalación y terminación de pisos',1,TRUE,1.0,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-9999-0000-0000-000000000011','11111111-0000-0000-0000-000000000001','11111111-8888-0000-0000-000000000002','Muros y Pintura','Revisión de estucos, revestimientos y pintura',2,TRUE,1.0,TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

INSERT INTO template_questions (id, tenant_id, section_id, question_text, question_type,
  order_index, is_required, is_critical, weight, min_photos, max_photos, is_active, created_at, created_by) VALUES
  ('11111111-aaaa-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000001','¿El hormigón presenta resistencia certificada f''c ≥ 250 kg/cm²?','yes_no',1,TRUE,TRUE,2.0,0,5,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000001','¿El proceso de curado cumple el tiempo mínimo requerido (7 días)?','yes_no',2,TRUE,TRUE,1.5,1,3,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000001','Temperatura ambiente al momento del vaciado (°C)','numeric',3,TRUE,FALSE,1.0,0,2,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000004','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000002','¿El diámetro de barras corresponde al plano estructural?','yes_no',1,TRUE,TRUE,2.0,1,5,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000005','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000002','¿Los empalmes cumplen la longitud mínima de traslapo?','yes_no',2,TRUE,TRUE,2.0,1,3,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000006','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000002','¿Los recubrimientos son los especificados (mín. 2 cm)?','yes_no',3,TRUE,FALSE,1.5,1,3,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000007','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000003','Estado general del moldaje','multiple_choice',1,TRUE,FALSE,1.0,1,5,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000008','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000010','¿La nivelación del piso cumple tolerancia ±3mm/2m?','yes_no',1,TRUE,FALSE,1.0,1,3,TRUE,NOW(),'11111111-1111-0000-0000-000000000001'),
  ('11111111-aaaa-0000-0000-000000000009','11111111-0000-0000-0000-000000000001','11111111-9999-0000-0000-000000000011','¿La pintura cubre uniformemente sin escurrimientos ni burbujas?','yes_no',1,TRUE,FALSE,1.0,1,3,TRUE,NOW(),'11111111-1111-0000-0000-000000000001');

INSERT INTO template_question_options (id, question_id, label, value, order_index, is_failure_option, score) VALUES
  ('11111111-bbbb-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000007','Bueno',      'bueno',   1,FALSE,1.0),
  ('11111111-bbbb-0000-0000-000000000002','11111111-aaaa-0000-0000-000000000007','Regular',    'regular', 2,FALSE,0.5),
  ('11111111-bbbb-0000-0000-000000000003','11111111-aaaa-0000-0000-000000000007','Deficiente', 'deficiente',3,TRUE,0.0);

INSERT INTO template_versions (id, tenant_id, template_id, version_number, snapshot, change_notes, created_at, created_by) VALUES
  ('9a342a90-c468-4388-aa20-680a977ac21d','11111111-0000-0000-0000-000000000001','11111111-8888-0000-0000-000000000001',1,
   '{"version":1,"sections":3,"questions":7,"notes":"Versión inicial"}',
   'Versión inicial del formulario de obra gruesa.',NOW(),'11111111-1111-0000-0000-000000000001'),
  ('8ebfbe2c-de10-419a-99c9-6cf330b5b611','11111111-0000-0000-0000-000000000001','11111111-8888-0000-0000-000000000001',2,
   '{"version":2,"sections":3,"questions":7,"notes":"Se agregó control de temperatura"}',
   'Se agregó pregunta de temperatura durante vaciado.',NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- INSPECCIONES
-- ============================================================
INSERT INTO inspections (id, tenant_id, project_id, template_id, template_version,
  stage_id, sector_id, code, title, description, inspection_type, status, priority,
  scheduled_date, started_at, finished_at,
  assigned_to_id, assigned_by_id, supervisor_id, contractor_id, specialty_id,
  score, passing_score, passed, total_questions, answered_questions,
  conforming_count, non_conforming_count, na_count,
  latitude, longitude, weather_conditions, temperature,
  is_offline_created, created_at, created_by) VALUES

  ('11111111-cccc-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-8888-0000-0000-000000000001',2,'11111111-4444-0000-0000-000000000001','11111111-5555-0000-0000-000000000001','INS-2025-001',
   'Inspección Obra Gruesa — Torre A Piso 8',
   'Control de vaciado de losa nivel 8 Torre A.',
   'ordinaria','cerrada','normal',
   '2025-06-15 09:00:00+00','2025-06-15 09:30:00+00','2025-06-15 11:45:00+00',
   '11111111-1111-0000-0000-000000000003','11111111-1111-0000-0000-000000000002','11111111-1111-0000-0000-000000000002','11111111-7777-0000-0000-000000000001','11111111-6666-0000-0000-000000000001',
   85.5,70.0,TRUE,7,7,6,1,0,
   -33.3890,-70.5765,'Soleado',22.5,
   FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-cccc-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-8888-0000-0000-000000000002',1,'11111111-4444-0000-0000-000000000002','11111111-5555-0000-0000-000000000001','INS-2025-002',
   'Inspección Terminaciones — Torre A Pisos 5-6',
   'Revisión de pisos y muros terminados en pisos 5 y 6.',
   'ordinaria','en_proceso','alta',
   '2026-03-18 10:00:00+00','2026-03-18 10:15:00+00',NULL,
   '11111111-1111-0000-0000-000000000004','11111111-1111-0000-0000-000000000002','11111111-1111-0000-0000-000000000002','11111111-7777-0000-0000-000000000002','11111111-6666-0000-0000-000000000002',
   NULL,80.0,NULL,4,2,1,1,0,
   -33.3891,-70.5766,'Nublado',18.0,
   FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-cccc-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-8888-0000-0000-000000000001',2,'11111111-4444-0000-0000-000000000002','11111111-5555-0000-0000-000000000002','INS-2025-003',
   'Inspección Obra Gruesa — Torre B Piso 10',
   'Control de enfierradura y moldajes antes del vaciado.',
   'ordinaria','programada','normal',
   '2026-03-25 09:00:00+00',NULL,NULL,
   '11111111-1111-0000-0000-000000000003','11111111-1111-0000-0000-000000000002','11111111-1111-0000-0000-000000000002','11111111-7777-0000-0000-000000000001','11111111-6666-0000-0000-000000000001',
   NULL,70.0,NULL,7,0,0,0,0,
   NULL,NULL,NULL,NULL,
   FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-cccc-0000-0000-000000000004','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-8888-0000-0000-000000000002',1,'11111111-4444-0000-0000-000000000002','11111111-5555-0000-0000-000000000001','INS-2025-004',
   'Inspección Terminaciones — Torre A Pisos 3-4',
   'Detectadas observaciones en revestimientos.',
   'ordinaria','observada','alta',
   '2026-03-10 09:00:00+00','2026-03-10 09:20:00+00','2026-03-10 12:00:00+00',
   '11111111-1111-0000-0000-000000000004','11111111-1111-0000-0000-000000000002','11111111-1111-0000-0000-000000000002','11111111-7777-0000-0000-000000000002','11111111-6666-0000-0000-000000000002',
   58.0,80.0,FALSE,4,4,2,2,0,
   -33.3892,-70.5767,'Soleado',20.0,
   FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-cccc-0000-0000-000000000005','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000002','11111111-8888-0000-0000-000000000001',2,NULL,NULL,'INS-2025-005',
   'Inspección Obra Gruesa — Casas Sector Norte Bloque 1',
   'Control de losas de casas pareadas bloque 1-10.',
   'ordinaria','finalizada','normal',
   '2025-11-20 09:00:00+00','2025-11-20 09:10:00+00','2025-11-20 13:00:00+00',
   '11111111-1111-0000-0000-000000000003','11111111-1111-0000-0000-000000000002','11111111-1111-0000-0000-000000000002','11111111-7777-0000-0000-000000000001','11111111-6666-0000-0000-000000000001',
   92.0,70.0,TRUE,7,7,7,0,0,
   -33.2012,-70.6703,'Soleado',25.0,
   FALSE,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- RESPUESTAS DE INSPECCIÓN (INS-2025-001 completa)
-- ============================================================
INSERT INTO inspection_answers (id, tenant_id, inspection_id, question_id, answer_value,
  selected_option_id, numeric_value, date_value, is_conforming, is_na, score, notes,
  latitude, longitude, answered_at, answered_by, created_at, updated_at) VALUES
  ('2626c32e-54bd-41b0-8895-44bee1128e34','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000001','true',NULL,NULL,NULL,TRUE,FALSE,2.0,NULL,NULL,NULL,'2025-06-15 11:00:00+00','11111111-1111-0000-0000-000000000003',NOW(),NOW()),
  ('e3ad448d-c3da-4dec-8b4e-13ceee01490f','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000002','true',NULL,NULL,NULL,TRUE,FALSE,1.5,NULL,NULL,NULL,'2025-06-15 11:00:00+00','11111111-1111-0000-0000-000000000003',NOW(),NOW()),
  ('c9aa3043-ef99-44b8-8e0d-9a656f741b77','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000003','22.5',NULL,22.5,NULL,TRUE,FALSE,1.0,NULL,NULL,NULL,'2025-06-15 11:00:00+00','11111111-1111-0000-0000-000000000003',NOW(),NOW()),
  ('144a2664-8ae5-4c39-9b9f-c16ace5a469e','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000004','true',NULL,NULL,NULL,TRUE,FALSE,2.0,NULL,NULL,NULL,'2025-06-15 11:00:00+00','11111111-1111-0000-0000-000000000003',NOW(),NOW()),
  ('2804a40f-91dc-4ca7-8686-92d27fe7c5b5','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000005','true',NULL,NULL,NULL,TRUE,FALSE,2.0,NULL,NULL,NULL,'2025-06-15 11:00:00+00','11111111-1111-0000-0000-000000000003',NOW(),NOW()),
  ('acc7ab7e-365d-484e-a41c-9bf8d922229a','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000006','false',NULL,NULL,NULL,FALSE,FALSE,0.0,NULL,NULL,NULL,'2025-06-15 11:00:00+00','11111111-1111-0000-0000-000000000003',NOW(),NOW()),
  ('a2b8851c-ae08-45b8-9f24-2d2bbb3d7976','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','11111111-aaaa-0000-0000-000000000007','bueno','11111111-bbbb-0000-0000-000000000001',NULL,NULL,TRUE,FALSE,1.0,NULL,NULL,NULL,'2025-06-15 11:00:00+00','11111111-1111-0000-0000-000000000003',NOW(),NOW());

-- ============================================================
-- OBSERVACIONES
-- ============================================================
INSERT INTO observations (id, tenant_id, project_id, inspection_id, code, title, description,
  specialty_id, category, severity, status, stage_id, sector_id,
  location_description, contractor_id, assigned_to_id, assigned_by_id,
  detected_at, detected_by, due_date, root_cause, corrective_action,
  latitude, longitude, is_recurring, created_at, created_by) VALUES

  ('11111111-dddd-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','OBS-2025-001',
   'Recubrimiento insuficiente en enfierradura — Torre A P8',
   'Recubrimiento medido de 1.5 cm, norma exige mínimo 2.0 cm en losa de piso. '
   'Se evidencia en 3 puntos distintos del sector norponiente.',
   '11111111-6666-0000-0000-000000000001','hormigon_armado','alta','en_correccion','11111111-4444-0000-0000-000000000001','11111111-5555-0000-0000-000000000001',
   'Losa piso 8, sector norponiente entre ejes C-D / 3-4',
   '11111111-7777-0000-0000-000000000001','11111111-1111-0000-0000-000000000005','11111111-1111-0000-0000-000000000002',
   '2025-06-15 12:00:00+00','11111111-1111-0000-0000-000000000003','2025-06-30',
   'Error de instalación de separadores de enfierradura.',
   'Demolición y reconstrucción del sector afectado con separadores certificados.',
   -33.3890,-70.5765,FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-dddd-0000-0000-000000000002','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-cccc-0000-0000-000000000004','OBS-2026-001',
   'Nivelación de piso fuera de tolerancia — Torre A P3',
   'Medición de planimetría detecta desviación de 6mm en 2m lineal. '
   'Tolerancia máxima según especificación técnica es ±3mm.',
   '11111111-6666-0000-0000-000000000002','terminaciones','media','asignada','11111111-4444-0000-0000-000000000002','11111111-5555-0000-0000-000000000001',
   'Departamentos 301, 302 y 303 — sala comedor',
   '11111111-7777-0000-0000-000000000002','11111111-1111-0000-0000-000000000005','11111111-1111-0000-0000-000000000002',
   '2026-03-10 12:00:00+00','11111111-1111-0000-0000-000000000004','2026-04-15',
   'Deficiencia en control de nivelación durante instalación de contrapiso.',
   'Escarificado y nivelación con mortero autonivelante en zonas afectadas.',
   -33.3891,-70.5767,FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-dddd-0000-0000-000000000003','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-cccc-0000-0000-000000000004','OBS-2026-002',
   'Manchas de humedad en muro sur — Torre A P4',
   'Se observan manchas de humedad activa en muro perimetral sur, '
   'posible filtración por junta de expansión.',
   '11111111-6666-0000-0000-000000000004','impermeabilizacion','alta','abierta','11111111-4444-0000-0000-000000000002','11111111-5555-0000-0000-000000000001',
   'Muro sur departamento 401, altura 1.2m desde el piso',
   '11111111-7777-0000-0000-000000000002',NULL,'11111111-1111-0000-0000-000000000002',
   '2026-03-10 12:30:00+00','11111111-1111-0000-0000-000000000004','2026-04-10',
   NULL,NULL,
   -33.3892,-70.5766,FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-dddd-0000-0000-000000000004','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000001','11111111-cccc-0000-0000-000000000001','OBS-2025-002',
   'Fisura en viga secundaria — Torre A P8',
   'Fisura de 0.3mm en viga secundaria en zona de apoyo. '
   'Requiere evaluación de ingeniero calculista.',
   '11111111-6666-0000-0000-000000000001','estructura','critica','cerrada','11111111-4444-0000-0000-000000000001','11111111-5555-0000-0000-000000000001',
   'Viga VS-15 eje D entre ejes 3-4 piso 8',
   '11111111-7777-0000-0000-000000000001','11111111-1111-0000-0000-000000000005','11111111-1111-0000-0000-000000000002',
   '2025-06-15 13:00:00+00','11111111-1111-0000-0000-000000000003','2025-07-15',
   'Deformación diferencial durante curado por temperatura excesiva.',
   'Inyección de resina epóxica certificada + informe de ingeniero calculista aprobado.',
   -33.3890,-70.5765,FALSE,NOW(),'11111111-1111-0000-0000-000000000001'),

  ('11111111-dddd-0000-0000-000000000005','11111111-0000-0000-0000-000000000001','11111111-3333-0000-0000-000000000002','11111111-cccc-0000-0000-000000000005','OBS-2025-003',
   'Traslapo insuficiente en enfierradura horizontal — Casa 5',
   'Traslapo medido de 28 cm, especificación exige mínimo 35 cm.',
   '11111111-6666-0000-0000-000000000001','hormigon_armado','alta','cerrada',NULL,NULL,
   'Losa de techo casa 5 sector norte, eje perimetral',
   '11111111-7777-0000-0000-000000000001','11111111-1111-0000-0000-000000000005','11111111-1111-0000-0000-000000000002',
   '2025-11-20 13:30:00+00','11111111-1111-0000-0000-000000000003','2025-12-05',
   'Corte de barra en posición incorrecta por falta de control de planos.',
   'Adición de longitud de traslapo mediante soldadura certificada + control radiográfico.',
   -33.2012,-70.6703,FALSE,NOW(),'11111111-1111-0000-0000-000000000001');

-- ============================================================
-- HISTORIAL DE OBSERVACIONES
-- ============================================================
INSERT INTO observation_history (id, tenant_id, observation_id, action,
  previous_status, new_status, previous_assigned_to, new_assigned_to,
  comment, created_at, created_by) VALUES

  ('1cccfb39-42d9-4cf9-9cb1-53fb9d44e69c','11111111-0000-0000-0000-000000000001','11111111-dddd-0000-0000-000000000001','created',NULL,'abierta',NULL,NULL,
   'Observación registrada durante inspección INS-2025-001.','2025-06-15 12:00:00+00','11111111-1111-0000-0000-000000000003'),

  ('1e3eb6bd-4bc5-4b36-89a1-23507f6214da','11111111-0000-0000-0000-000000000001','11111111-dddd-0000-0000-000000000001','assigned','abierta','asignada',NULL,'11111111-1111-0000-0000-000000000005',
   'Asignada a contratista Estructuras Del Sur para corrección.','2025-06-16 08:00:00+00','11111111-1111-0000-0000-000000000002'),

  ('4d0bc718-03c7-4275-8a3c-0e3f50e606a4','11111111-0000-0000-0000-000000000001','11111111-dddd-0000-0000-000000000001','status_change','asignada','en_correccion','11111111-1111-0000-0000-000000000005','11111111-1111-0000-0000-000000000005',
   'Contratista confirmó inicio de trabajos de corrección.','2025-06-20 09:30:00+00','11111111-1111-0000-0000-000000000005'),

  ('a0d57175-2052-4858-8e64-15183073336a','11111111-0000-0000-0000-000000000001','11111111-dddd-0000-0000-000000000004','created',NULL,'abierta',NULL,NULL,
   'Fisura detectada durante inspección. Se solicitó evaluación de calculista.','2025-06-15 13:00:00+00','11111111-1111-0000-0000-000000000003'),

  ('9aa86afe-6adf-4931-a58d-d6229106c8bb','11111111-0000-0000-0000-000000000001','11111111-dddd-0000-0000-000000000004','closed','corregida','cerrada','11111111-1111-0000-0000-000000000005',NULL,
   'Informe de ingeniero aprobado. Reparación verificada y aceptada.','2025-07-20 10:00:00+00','11111111-1111-0000-0000-000000000002');

-- ============================================================
-- REINSPECCIONES
-- ============================================================
INSERT INTO reinspections (id, tenant_id, original_inspection_id, observation_id,
  code, status, result, scheduled_date, executed_at, executed_by,
  created_at, created_by) VALUES
  ('11111111-eeee-0000-0000-000000000001','11111111-0000-0000-0000-000000000001','11111111-cccc-0000-0000-000000000004','11111111-dddd-0000-0000-000000000002','REINSP-2026-001',
   'programada',NULL,'2026-04-20 09:00:00+00',NULL,NULL,
   NOW(),'11111111-1111-0000-0000-000000000002');

-- ============================================================
-- NOTIFICACIONES
-- ============================================================
INSERT INTO notifications (id, tenant_id, user_id, type, title, body,
  entity_type, entity_id, is_read, sent_email, sent_push, created_at) VALUES

  ('17b26b84-4552-46ce-8e0d-bd0dac382791','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000003','inspection_assigned',
   'Nueva inspección asignada',
   'Se te ha asignado la inspección INS-2025-003 programada para el 25 de marzo.',
   'inspection','11111111-cccc-0000-0000-000000000003',FALSE,TRUE,FALSE,NOW()),

  ('5b47e1a9-592c-460c-b61c-42289eab2425','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000005','observation_assigned',
   'Observación asignada para corrección',
   'Se te ha asignado la observación OBS-2026-001: Nivelación de piso fuera de tolerancia. Plazo: 15/04/2026.',
   'observation','11111111-dddd-0000-0000-000000000002',FALSE,TRUE,FALSE,NOW()),

  ('06947997-bd57-4bb5-9754-1790807af449','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000002','observation_due_soon',
   'Observación próxima a vencer',
   'La observación OBS-2026-002 vence el 10/04/2026 y aún está abierta.',
   'observation','11111111-dddd-0000-0000-000000000003',FALSE,TRUE,FALSE,NOW()),

  ('92e6c51c-0f88-4671-b155-e56b3940dbcc','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000001','inspection_completed',
   'Inspección completada con observaciones',
   'La inspección INS-2025-004 fue finalizada con 2 No Conformidades. Score: 58.0%%.',
   'inspection','11111111-cccc-0000-0000-000000000004',TRUE,FALSE,FALSE,NOW()),

  ('c3cce103-6cbf-445e-8554-0d77e13fdf8b','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000004','inspection_assigned',
   'Nueva inspección asignada',
   'Se te ha asignado la inspección INS-2025-002 en ejecución.',
   'inspection','11111111-cccc-0000-0000-000000000002',TRUE,TRUE,FALSE,NOW());

-- ============================================================
-- AUDIT LOGS
-- ============================================================
INSERT INTO audit_logs (id, tenant_id, user_id, action, entity_type, entity_id,
  new_values, ip_address, created_at) VALUES
  ('638aa1c1-91d9-4035-96e7-ff58084a25e8','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000001','Create','Project','11111111-3333-0000-0000-000000000001',
   '{"name":"Edificio Las Palmas — Torre Norte","status":"activo"}','192.168.1.10',NOW() - INTERVAL '30 days'),
  ('d323059e-5113-4dd4-a4e4-c13ad3113e28','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000001','Create','Project','11111111-3333-0000-0000-000000000002',
   '{"name":"Conjunto Residencial Norte — Fase I","status":"activo"}','192.168.1.10',NOW() - INTERVAL '28 days'),
  ('f0c19216-b012-479a-a24f-0f002131088c','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000002','Create','Inspection','11111111-cccc-0000-0000-000000000001',
   '{"code":"INS-2025-001","status":"programada"}','192.168.1.11',NOW() - INTERVAL '20 days'),
  ('1d78ed4f-267f-4d1a-9d75-a323c7f66dde','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000003','Update','Inspection','11111111-cccc-0000-0000-000000000001',
   '{"status":"cerrada","score":85.5}','10.0.0.5',NOW() - INTERVAL '10 days'),
  ('efa41bae-a85b-4855-b69c-67921858dc84','11111111-0000-0000-0000-000000000001','11111111-1111-0000-0000-000000000001','Login','User','11111111-1111-0000-0000-000000000001',
   '{"email":"admin@itopacifico.cl"}','192.168.1.10',NOW() - INTERVAL '1 hour');

-- ============================================================
-- CONTADORES DE SECUENCIA
-- ============================================================
INSERT INTO sequence_counters (id, tenant_id, entity_type, prefix, year, last_value) VALUES
  ('864f8ffb-c711-48c1-8eae-e375ea56447f','11111111-0000-0000-0000-000000000001','inspection',  'INS', 2025, 2),
  ('aadd4f12-ad90-4976-ad5f-85687b1019e2','11111111-0000-0000-0000-000000000001','inspection',  'INS', 2026, 3),
  ('91cee804-b47e-4cd8-8bcc-b00f6c85ff30','11111111-0000-0000-0000-000000000001','observation',  'OBS', 2025, 2),
  ('9a471ffc-7334-45cc-bdae-fe68a5f973bb','11111111-0000-0000-0000-000000000001','observation',  'OBS', 2026, 2),
  ('cd59bd43-b0e0-46d6-9f83-b32feb8449b8','11111111-0000-0000-0000-000000000001','reinspection','REINSP',2026,1),
  ('152e0935-3b87-4d74-9e12-070f298736a2','22222222-0000-0000-0000-000000000001','inspection',  'INS', 2025, 0);

COMMIT;

-- ============================================================
-- VERIFICACIÓN RÁPIDA
-- ============================================================
SELECT 'tenants'          AS tabla, COUNT(*) AS registros FROM tenants UNION ALL
SELECT 'users',            COUNT(*) FROM users UNION ALL
SELECT 'companies',        COUNT(*) FROM companies UNION ALL
SELECT 'projects',         COUNT(*) FROM projects UNION ALL
SELECT 'inspections',      COUNT(*) FROM inspections UNION ALL
SELECT 'observations',     COUNT(*) FROM observations UNION ALL
SELECT 'templates',        COUNT(*) FROM inspection_templates UNION ALL
SELECT 'notifications',    COUNT(*) FROM notifications UNION ALL
SELECT 'audit_logs',       COUNT(*) FROM audit_logs
ORDER BY tabla;
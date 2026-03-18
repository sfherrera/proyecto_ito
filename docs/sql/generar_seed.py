"""
Genera 002_seed_data.sql con datos de prueba realistas para ITO Cloud.
Incluye: 2 tenants, usuarios, empresas, proyectos, plantillas, inspecciones, observaciones, etc.
"""

import hashlib, os, base64, struct, uuid, datetime

# ─── Genera password hash compatible con ASP.NET Identity v3 ───────────────
def aspnet_identity_hash(password: str) -> str:
    """PBKDF2-SHA256, 10000 iter, 16 bytes salt, 32 bytes subkey — formato v3."""
    salt = os.urandom(16)
    prf       = 2          # HMACSHA256
    iterations = 10000
    key_length = 32
    dk = hashlib.pbkdf2_hmac('sha256', password.encode('utf-8'), salt, iterations, dklen=key_length)
    header = struct.pack('>BIII', 0x01, prf, iterations, len(salt))
    return base64.b64encode(header + salt + dk).decode('utf-8')

# ─── UUIDs fijos para referencias cruzadas ─────────────────────────────────
T1  = '11111111-0000-0000-0000-000000000001'   # tenant 1
T2  = '22222222-0000-0000-0000-000000000001'   # tenant 2

# Usuarios tenant 1
U_ADM1  = '11111111-1111-0000-0000-000000000001'   # admin
U_SUP1  = '11111111-1111-0000-0000-000000000002'   # supervisor
U_INS1  = '11111111-1111-0000-0000-000000000003'   # inspector 1
U_INS2  = '11111111-1111-0000-0000-000000000004'   # inspector 2
U_CONT1 = '11111111-1111-0000-0000-000000000005'   # contratista viewer

# Usuarios tenant 2
U_ADM2  = '22222222-1111-0000-0000-000000000001'
U_INS3  = '22222222-1111-0000-0000-000000000002'

# Roles
R_ADMIN = 'aaaaaaaa-0000-0000-0000-000000000001'
R_SUP   = 'aaaaaaaa-0000-0000-0000-000000000002'
R_INS   = 'aaaaaaaa-0000-0000-0000-000000000003'
R_CONT  = 'aaaaaaaa-0000-0000-0000-000000000004'

# Empresas
C1 = '11111111-2222-0000-0000-000000000001'   # Constructora Pacífico
C2 = '11111111-2222-0000-0000-000000000002'   # Inmobiliaria Las Palmas
C3 = '22222222-2222-0000-0000-000000000001'   # Constructora Sur

# Proyectos
P1 = '11111111-3333-0000-0000-000000000001'   # Edificio Las Palmas (T1)
P2 = '11111111-3333-0000-0000-000000000002'   # Conjunto Residencial Norte (T1)
P3 = '22222222-3333-0000-0000-000000000001'   # Edificio Costa Verde (T2)

# Etapas proyecto 1
ST1_1 = '11111111-4444-0000-0000-000000000001'
ST1_2 = '11111111-4444-0000-0000-000000000002'
ST1_3 = '11111111-4444-0000-0000-000000000003'

# Sectores proyecto 1
SEC1_1 = '11111111-5555-0000-0000-000000000001'  # Torre A
SEC1_2 = '11111111-5555-0000-0000-000000000002'  # Torre B
SEC1_3 = '11111111-5555-0000-0000-000000000003'  # Subterráneo

# Especialidades
SP1 = '11111111-6666-0000-0000-000000000001'  # Estructural
SP2 = '11111111-6666-0000-0000-000000000002'  # Terminaciones
SP3 = '11111111-6666-0000-0000-000000000003'  # Instalaciones
SP4 = '11111111-6666-0000-0000-000000000004'  # Impermeabilización

# Contratistas
CT1 = '11111111-7777-0000-0000-000000000001'  # Estructuras Del Sur
CT2 = '11111111-7777-0000-0000-000000000002'  # Terminaciones Norte
CT3 = '11111111-7777-0000-0000-000000000003'  # Instalaciones Rápidas

# Plantillas
TPL1 = '11111111-8888-0000-0000-000000000001'  # Inspección Obra Gruesa
TPL2 = '11111111-8888-0000-0000-000000000002'  # Inspección Terminaciones
TPL3 = '11111111-8888-0000-0000-000000000003'  # Recepción de Unidad

# Secciones plantilla 1 (Obra Gruesa)
SEC_TPL1_1 = '11111111-9999-0000-0000-000000000001'  # Hormigón
SEC_TPL1_2 = '11111111-9999-0000-0000-000000000002'  # Fierros
SEC_TPL1_3 = '11111111-9999-0000-0000-000000000003'  # Moldajes

# Secciones plantilla 2 (Terminaciones)
SEC_TPL2_1 = '11111111-9999-0000-0000-000000000010'  # Pisos
SEC_TPL2_2 = '11111111-9999-0000-0000-000000000011'  # Muros

# Preguntas plantilla 1
Q1 = '11111111-aaaa-0000-0000-000000000001'
Q2 = '11111111-aaaa-0000-0000-000000000002'
Q3 = '11111111-aaaa-0000-0000-000000000003'
Q4 = '11111111-aaaa-0000-0000-000000000004'
Q5 = '11111111-aaaa-0000-0000-000000000005'
Q6 = '11111111-aaaa-0000-0000-000000000006'
Q7 = '11111111-aaaa-0000-0000-000000000007'
Q8 = '11111111-aaaa-0000-0000-000000000008'
Q9 = '11111111-aaaa-0000-0000-000000000009'

# Opciones de pregunta multiple_choice
OPT1 = '11111111-bbbb-0000-0000-000000000001'
OPT2 = '11111111-bbbb-0000-0000-000000000002'
OPT3 = '11111111-bbbb-0000-0000-000000000003'

# Inspecciones
INS1 = '11111111-cccc-0000-0000-000000000001'  # finalizada
INS2 = '11111111-cccc-0000-0000-000000000002'  # en_proceso
INS3 = '11111111-cccc-0000-0000-000000000003'  # programada
INS4 = '11111111-cccc-0000-0000-000000000004'  # observada
INS5 = '11111111-cccc-0000-0000-000000000005'  # cerrada

# Observaciones
OBS1 = '11111111-dddd-0000-0000-000000000001'
OBS2 = '11111111-dddd-0000-0000-000000000002'
OBS3 = '11111111-dddd-0000-0000-000000000003'
OBS4 = '11111111-dddd-0000-0000-000000000004'
OBS5 = '11111111-dddd-0000-0000-000000000005'

# Reinspecciones
REINSP1 = '11111111-eeee-0000-0000-000000000001'

# Password hash para todos los usuarios de prueba (password: Test1234!)
PW = aspnet_identity_hash('Test1234!')

lines = []
a = lines.append

a("-- ============================================================")
a("-- ITO Cloud — Datos de prueba (seed)")
a("-- Versión: 1.0 | Fecha: 2026-03-18")
a("-- Password de todos los usuarios: Test1234!")
a("-- ============================================================")
a("")
a("BEGIN;")
a("")

# ── TENANTS ────────────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- TENANTS")
a("-- ============================================================")
a(f"""INSERT INTO tenants (id, name, slug, plan, is_active, primary_color, max_users, max_projects) VALUES
  ('{T1}', 'ITO Pacífico SpA',          'ito-pacifico',  'professional', TRUE, '#1F3864', 20, 15),
  ('{T2}', 'Consultores Sur Ltda',       'consultores-sur','basic',       TRUE, '#1A6B3C',  8,  5);
""")

# ── ROLES (sistema, sin tenant) ────────────────────────────────────────────
a("-- ============================================================")
a("-- ROLES")
a("-- ============================================================")
a(f"""INSERT INTO roles (id, tenant_id, name, normalized_name, description, is_system_role) VALUES
  ('{R_ADMIN}', NULL, 'Administrador', 'ADMINISTRADOR', 'Acceso total al sistema del tenant', TRUE),
  ('{R_SUP}',   NULL, 'Supervisor',    'SUPERVISOR',    'Supervisa inspecciones y reportes',  TRUE),
  ('{R_INS}',   NULL, 'Inspector',     'INSPECTOR',     'Ejecuta inspecciones en terreno',    TRUE),
  ('{R_CONT}',  NULL, 'Contratista',   'CONTRATISTA',   'Visualiza observaciones asignadas',  TRUE);
""")

# ── USUARIOS TENANT 1 ──────────────────────────────────────────────────────
a("-- ============================================================")
a("-- USUARIOS — Tenant 1: ITO Pacífico SpA")
a("-- ============================================================")
a(f"""INSERT INTO users (id, tenant_id, user_name, normalized_user_name, email, normalized_email,
  email_confirmed, password_hash, security_stamp, concurrency_stamp,
  first_name, last_name, rut, position, is_active, created_at, created_by) VALUES
  ('{U_ADM1}','{T1}','admin.pacifico','ADMIN.PACIFICO','admin@itopacifico.cl','ADMIN@ITOPACIFICO.CL',
   TRUE,'{PW}','{str(uuid.uuid4())}','{str(uuid.uuid4())}',
   'Rodrigo','Fuentes Vidal','12.345.678-9','Administrador ITO',TRUE,NOW(),'{U_ADM1}'),

  ('{U_SUP1}','{T1}','carlos.mendez','CARLOS.MENDEZ','carlos.mendez@itopacifico.cl','CARLOS.MENDEZ@ITOPACIFICO.CL',
   TRUE,'{PW}','{str(uuid.uuid4())}','{str(uuid.uuid4())}',
   'Carlos','Méndez Torres','13.456.789-0','Supervisor de Obra',TRUE,NOW(),'{U_ADM1}'),

  ('{U_INS1}','{T1}','pablo.rojas','PABLO.ROJAS','pablo.rojas@itopacifico.cl','PABLO.ROJAS@ITOPACIFICO.CL',
   TRUE,'{PW}','{str(uuid.uuid4())}','{str(uuid.uuid4())}',
   'Pablo','Rojas Soto','14.567.890-1','Inspector ITO Senior',TRUE,NOW(),'{U_ADM1}'),

  ('{U_INS2}','{T1}','ana.gonzalez','ANA.GONZALEZ','ana.gonzalez@itopacifico.cl','ANA.GONZALEZ@ITOPACIFICO.CL',
   TRUE,'{PW}','{str(uuid.uuid4())}','{str(uuid.uuid4())}',
   'Ana','González Muñoz','15.678.901-2','Inspector ITO',TRUE,NOW(),'{U_ADM1}'),

  ('{U_CONT1}','{T1}','juan.herrera','JUAN.HERRERA','juan.herrera@estructurassur.cl','JUAN.HERRERA@ESTRUCTURASSUR.CL',
   TRUE,'{PW}','{str(uuid.uuid4())}','{str(uuid.uuid4())}',
   'Juan','Herrera Lagos','16.789.012-3','Jefe de Obra',TRUE,NOW(),'{U_ADM1}');
""")

# ── USUARIOS TENANT 2 ──────────────────────────────────────────────────────
a("-- ============================================================")
a("-- USUARIOS — Tenant 2: Consultores Sur Ltda")
a("-- ============================================================")
a(f"""INSERT INTO users (id, tenant_id, user_name, normalized_user_name, email, normalized_email,
  email_confirmed, password_hash, security_stamp, concurrency_stamp,
  first_name, last_name, rut, position, is_active, created_at, created_by) VALUES
  ('{U_ADM2}','{T2}','admin.sur','ADMIN.SUR','admin@consultores-sur.cl','ADMIN@CONSULTORES-SUR.CL',
   TRUE,'{PW}','{str(uuid.uuid4())}','{str(uuid.uuid4())}',
   'Marcela','Ríos Pinto','17.890.123-4','Directora',TRUE,NOW(),'{U_ADM2}'),

  ('{U_INS3}','{T2}','diego.silva','DIEGO.SILVA','diego.silva@consultores-sur.cl','DIEGO.SILVA@CONSULTORES-SUR.CL',
   TRUE,'{PW}','{str(uuid.uuid4())}','{str(uuid.uuid4())}',
   'Diego','Silva Araya','18.901.234-5','Inspector',TRUE,NOW(),'{U_ADM2}');
""")

# ── USER_ROLES ─────────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- USER ROLES")
a("-- ============================================================")
a(f"""INSERT INTO user_roles (user_id, role_id) VALUES
  ('{U_ADM1}', '{R_ADMIN}'),
  ('{U_SUP1}', '{R_SUP}'),
  ('{U_INS1}', '{R_INS}'),
  ('{U_INS2}', '{R_INS}'),
  ('{U_CONT1}','{R_CONT}'),
  ('{U_ADM2}', '{R_ADMIN}'),
  ('{U_INS3}', '{R_INS}');
""")

# ── COMPANIES ──────────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- EMPRESAS")
a("-- ============================================================")
a(f"""INSERT INTO companies (id, tenant_id, name, rut, business_name, company_type, address, city, region,
  phone, email, website, is_active, created_at, created_by) VALUES
  ('{C1}','{T1}','Constructora Pacífico SpA','96.123.456-7','Constructora Pacífico Sociedad por Acciones',
   'constructora','Av. Apoquindo 4501, Of. 702','Santiago','Región Metropolitana',
   '+56 2 2345 6789','contacto@constructorapacifico.cl','www.constructorapacifico.cl',
   TRUE,NOW(),'{U_ADM1}'),

  ('{C2}','{T1}','Inmobiliaria Las Palmas S.A.','97.234.567-8','Inmobiliaria Las Palmas Sociedad Anónima',
   'inmobiliaria','Av. Las Condes 11.000, Piso 3','Las Condes','Región Metropolitana',
   '+56 2 2456 7890','info@laspalmasinmobiliaria.cl','www.laspalmasinmobiliaria.cl',
   TRUE,NOW(),'{U_ADM1}'),

  ('{C3}','{T2}','Constructora Sur Ltda.','78.345.678-9','Constructora Sur Limitada',
   'constructora','Av. Independencia 1250','Concepción','Biobío',
   '+56 41 234 5678','admin@constructorasur.cl',NULL,
   TRUE,NOW(),'{U_ADM2}');
""")

# ── PROJECTS ───────────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- PROYECTOS")
a("-- ============================================================")
a(f"""INSERT INTO projects (id, tenant_id, company_id, code, name, description, project_type, status,
  address, city, region, latitude, longitude, start_date, estimated_end_date,
  total_units, ito_manager_id, mandante_name, mandante_contact, mandante_email,
  construction_permit, is_active, created_at, created_by) VALUES
  ('{P1}','{T1}','{C2}','LP-2024-001','Edificio Las Palmas — Torre Norte',
   'Edificio residencial de 18 pisos con 120 departamentos, 2 subterráneos y áreas comunes.',
   'edificio','activo','Av. Las Palmas 1250','Vitacura','Región Metropolitana',
   -33.3890,-70.5765,'2024-03-01','2026-09-30',120,'{U_SUP1}',
   'Inmobiliaria Las Palmas S.A.','Pedro Contreras Lara','pcontreras@laspalmasinmobiliaria.cl',
   'DOM-VIT-2024-0123',TRUE,NOW(),'{U_ADM1}'),

  ('{P2}','{T1}','{C1}','CRN-2024-002','Conjunto Residencial Norte — Fase I',
   'Conjunto de 60 casas pareadas de 2 pisos, urbanización completa.',
   'conjunto_casas','activo','Parcela 5, Camino El Valle Km 12','Colina','Región Metropolitana',
   -33.2012,-70.6703,'2024-06-01','2025-12-31',60,'{U_SUP1}',
   'Constructora Pacífico SpA','Rodrigo Fuentes','rfuentes@constructorapacifico.cl',
   'DOM-COL-2024-0456',TRUE,NOW(),'{U_ADM1}'),

  ('{P3}','{T2}','{C3}','CV-2025-001','Edificio Costa Verde',
   'Edificio de 12 pisos, 80 departamentos en la ciudad de Concepción.',
   'edificio','activo','Av. O''Higgins 750','Concepción','Biobío',
   -36.8201,-73.0444,'2025-01-15','2026-06-30',80,'{U_ADM2}',
   'Constructora Sur Ltda.','Marcela Ríos','mrios@constructorasur.cl',
   'MUN-CCP-2025-0012',TRUE,NOW(),'{U_ADM2}');
""")

# ── PROJECT STAGES ─────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- ETAPAS DE PROYECTO (Proyecto 1: Edificio Las Palmas)")
a("-- ============================================================")
a(f"""INSERT INTO project_stages (id, tenant_id, project_id, name, description, order_index, status,
  start_date, end_date, created_at, created_by) VALUES
  ('{ST1_1}','{T1}','{P1}','Obra Gruesa','Estructura, hormigón y fierros',1,'completada','2024-03-01','2025-02-28',NOW(),'{U_ADM1}'),
  ('{ST1_2}','{T1}','{P1}','Terminaciones','Pisos, pintura, ventanas y revestimientos',2,'en_progreso','2025-03-01','2026-03-31',NOW(),'{U_ADM1}'),
  ('{ST1_3}','{T1}','{P1}','Instalaciones y Recepción','EESS, AACC, entrega final',3,'pendiente','2026-04-01','2026-09-30',NOW(),'{U_ADM1}');
""")

# ── PROJECT SECTORS ────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- SECTORES")
a("-- ============================================================")
a(f"""INSERT INTO project_sectors (id, tenant_id, project_id, name, sector_type, order_index, created_at, created_by) VALUES
  ('{SEC1_1}','{T1}','{P1}','Torre A','torre',1,NOW(),'{U_ADM1}'),
  ('{SEC1_2}','{T1}','{P1}','Torre B','torre',2,NOW(),'{U_ADM1}'),
  ('{SEC1_3}','{T1}','{P1}','Subterráneo -1','subterraneo',3,NOW(),'{U_ADM1}');
""")

# ── PROJECT UNITS (muestra: 6 unidades) ───────────────────────────────────
a("-- ============================================================")
a("-- UNIDADES (muestra)")
a("-- ============================================================")
units = []
for floor_n, unit_n, sec, unit_id_suffix in [
    (3,'301',SEC1_1,'000000000001'),(3,'302',SEC1_1,'000000000002'),(3,'303',SEC1_1,'000000000003'),
    (4,'401',SEC1_2,'000000000004'),(4,'402',SEC1_2,'000000000005'),(4,'403',SEC1_2,'000000000006'),
]:
    uid = f'11111111-ffff-0000-0000-{unit_id_suffix}'
    units.append(f"  ('{uid}','{T1}','{P1}','{sec}','{unit_n}','departamento',{floor_n},67.50,'recepcion_preliminar',NULL,NULL,TRUE,NOW(),'{U_ADM1}')")
a("INSERT INTO project_units (id, tenant_id, project_id, sector_id, unit_code, unit_type, floor, surface_m2, status, owner_name, owner_email, is_active, created_at, created_by) VALUES")
a(",\n".join(units) + ";\n")

# ── ESPECIALIDADES ─────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- ESPECIALIDADES")
a("-- ============================================================")
a(f"""INSERT INTO specialties (id, tenant_id, name, code, description, color, is_active, created_at, created_by) VALUES
  ('{SP1}','{T1}','Estructural',        'EST','Hormigón, fierros, moldajes y obra gruesa',     '#1F3864',TRUE,NOW(),'{U_ADM1}'),
  ('{SP2}','{T1}','Terminaciones',      'TER','Pisos, revestimientos, pintura y ventanas',     '#26A69A',TRUE,NOW(),'{U_ADM1}'),
  ('{SP3}','{T1}','Instalaciones',      'INS','Instalaciones eléctricas, sanitarias y gas',    '#F4A261',TRUE,NOW(),'{U_ADM1}'),
  ('{SP4}','{T1}','Impermeabilización', 'IMP','Techumbres, terrazas y muros perimetrales',     '#E76F51',TRUE,NOW(),'{U_ADM1}');
""")

# ── CONTRATISTAS ───────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- CONTRATISTAS")
a("-- ============================================================")
a(f"""INSERT INTO contractors (id, tenant_id, company_id, name, rut, contact_name, contact_email, contact_phone, is_active, created_at, created_by) VALUES
  ('{CT1}','{T1}','{C1}','Estructuras Del Sur Ltda','79.111.222-3','Miguel Fuentes','mfuentes@estructurassur.cl','+56 9 8111 2233',TRUE,NOW(),'{U_ADM1}'),
  ('{CT2}','{T1}','{C1}','Terminaciones Norte SpA', '80.222.333-4','Claudia Vega',  'cvega@terminacionesnorte.cl','+56 9 8222 3344',TRUE,NOW(),'{U_ADM1}'),
  ('{CT3}','{T1}','{C1}','Instalaciones Rápidas SA', '81.333.444-5','Andrés Mora',  'amora@instalrapidas.cl',    '+56 9 8333 4455',TRUE,NOW(),'{U_ADM1}');
""")

a(f"""INSERT INTO contractor_specialties (contractor_id, specialty_id) VALUES
  ('{CT1}','{SP1}'),('{CT2}','{SP2}'),('{CT3}','{SP3}');
""")

a(f"""INSERT INTO project_contractors (id, tenant_id, project_id, contractor_id, specialty_id, is_active, assigned_at, assigned_by) VALUES
  ('{str(uuid.uuid4())}','{T1}','{P1}','{CT1}','{SP1}',TRUE,NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{P1}','{CT2}','{SP2}',TRUE,NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{P1}','{CT3}','{SP3}',TRUE,NOW(),'{U_ADM1}');
""")

# ── PROJECT MEMBERS ────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- MIEMBROS DEL PROYECTO")
a("-- ============================================================")
a(f"""INSERT INTO project_members (id, tenant_id, project_id, user_id, project_role, is_active, assigned_at, assigned_by) VALUES
  ('{str(uuid.uuid4())}','{T1}','{P1}','{U_SUP1}', 'supervisor',  TRUE,NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{P1}','{U_INS1}', 'inspector',   TRUE,NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{P1}','{U_INS2}', 'inspector',   TRUE,NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{P1}','{U_CONT1}','contratista',  TRUE,NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{P2}','{U_SUP1}', 'supervisor',  TRUE,NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{P2}','{U_INS1}', 'inspector',   TRUE,NOW(),'{U_ADM1}');
""")

# ── PLANTILLA 1: OBRA GRUESA ───────────────────────────────────────────────
a("-- ============================================================")
a("-- PLANTILLAS DE INSPECCIÓN")
a("-- ============================================================")
a(f"""INSERT INTO inspection_templates (id, tenant_id, name, description, template_type, specialty_id,
  status, current_version, is_global, allow_partial_save, require_geolocation,
  passing_score, created_at, created_by) VALUES
  ('{TPL1}','{T1}','Inspección Obra Gruesa v2',
   'Checklist completo para revisión de hormigón armado, enfierradura y moldajes.',
   'obra_gruesa','{SP1}','activa',2,FALSE,TRUE,TRUE,70.0,NOW(),'{U_ADM1}'),

  ('{TPL2}','{T1}','Inspección Terminaciones',
   'Revisión de pisos, revestimientos, pintura, ventanas y puertas.',
   'terminaciones','{SP2}','activa',1,FALSE,TRUE,FALSE,80.0,NOW(),'{U_ADM1}'),

  ('{TPL3}','{T1}','Recepción de Unidad',
   'Formulario de recepción de departamento o casa al propietario.',
   'recepcion',NULL,'activa',1,TRUE,TRUE,FALSE,90.0,NOW(),'{U_ADM1}');
""")

# ── SECCIONES PLANTILLA 1 ─────────────────────────────────────────────────
a(f"""INSERT INTO template_sections (id, tenant_id, template_id, title, description, order_index,
  is_required, weight, is_active, created_at, created_by) VALUES
  ('{SEC_TPL1_1}','{T1}','{TPL1}','Hormigón','Revisión de calidad del vaciado y curado',1,TRUE,1.5,TRUE,NOW(),'{U_ADM1}'),
  ('{SEC_TPL1_2}','{T1}','{TPL1}','Enfierradura','Control de enfierradura antes del vaciado',2,TRUE,2.0,TRUE,NOW(),'{U_ADM1}'),
  ('{SEC_TPL1_3}','{T1}','{TPL1}','Moldajes','Revisión de moldajes y puntales',3,TRUE,1.0,TRUE,NOW(),'{U_ADM1}'),
  ('{SEC_TPL2_1}','{T1}','{TPL2}','Pisos','Control de instalación y terminación de pisos',1,TRUE,1.0,TRUE,NOW(),'{U_ADM1}'),
  ('{SEC_TPL2_2}','{T1}','{TPL2}','Muros y Pintura','Revisión de estucos, revestimientos y pintura',2,TRUE,1.0,TRUE,NOW(),'{U_ADM1}');
""")

# ── PREGUNTAS PLANTILLA 1 ─────────────────────────────────────────────────
a(f"""INSERT INTO template_questions (id, tenant_id, section_id, question_text, question_type,
  order_index, is_required, is_critical, weight, min_photos, max_photos, is_active, created_at, created_by) VALUES
  ('{Q1}','{T1}','{SEC_TPL1_1}','¿El hormigón presenta resistencia certificada f''c ≥ 250 kg/cm²?','yes_no',1,TRUE,TRUE,2.0,0,5,TRUE,NOW(),'{U_ADM1}'),
  ('{Q2}','{T1}','{SEC_TPL1_1}','¿El proceso de curado cumple el tiempo mínimo requerido (7 días)?','yes_no',2,TRUE,TRUE,1.5,1,3,TRUE,NOW(),'{U_ADM1}'),
  ('{Q3}','{T1}','{SEC_TPL1_1}','Temperatura ambiente al momento del vaciado (°C)','numeric',3,TRUE,FALSE,1.0,0,2,TRUE,NOW(),'{U_ADM1}'),
  ('{Q4}','{T1}','{SEC_TPL1_2}','¿El diámetro de barras corresponde al plano estructural?','yes_no',1,TRUE,TRUE,2.0,1,5,TRUE,NOW(),'{U_ADM1}'),
  ('{Q5}','{T1}','{SEC_TPL1_2}','¿Los empalmes cumplen la longitud mínima de traslapo?','yes_no',2,TRUE,TRUE,2.0,1,3,TRUE,NOW(),'{U_ADM1}'),
  ('{Q6}','{T1}','{SEC_TPL1_2}','¿Los recubrimientos son los especificados (mín. 2 cm)?','yes_no',3,TRUE,FALSE,1.5,1,3,TRUE,NOW(),'{U_ADM1}'),
  ('{Q7}','{T1}','{SEC_TPL1_3}','Estado general del moldaje','multiple_choice',1,TRUE,FALSE,1.0,1,5,TRUE,NOW(),'{U_ADM1}'),
  ('{Q8}','{T1}','{SEC_TPL2_1}','¿La nivelación del piso cumple tolerancia ±3mm/2m?','yes_no',1,TRUE,FALSE,1.0,1,3,TRUE,NOW(),'{U_ADM1}'),
  ('{Q9}','{T1}','{SEC_TPL2_2}','¿La pintura cubre uniformemente sin escurrimientos ni burbujas?','yes_no',1,TRUE,FALSE,1.0,1,3,TRUE,NOW(),'{U_ADM1}');
""")

# ── OPCIONES PREGUNTA MULTIPLE_CHOICE ─────────────────────────────────────
a(f"""INSERT INTO template_question_options (id, question_id, label, value, order_index, is_failure_option, score) VALUES
  ('{OPT1}','{Q7}','Bueno',      'bueno',   1,FALSE,1.0),
  ('{OPT2}','{Q7}','Regular',    'regular', 2,FALSE,0.5),
  ('{OPT3}','{Q7}','Deficiente', 'deficiente',3,TRUE,0.0);
""")

# ── VERSION SNAPSHOT PLANTILLA 1 ──────────────────────────────────────────
a(f"""INSERT INTO template_versions (id, tenant_id, template_id, version_number, snapshot, change_notes, created_at, created_by) VALUES
  ('{str(uuid.uuid4())}','{T1}','{TPL1}',1,
   '{{"version":1,"sections":3,"questions":7,"notes":"Versión inicial"}}',
   'Versión inicial del formulario de obra gruesa.',NOW(),'{U_ADM1}'),
  ('{str(uuid.uuid4())}','{T1}','{TPL1}',2,
   '{{"version":2,"sections":3,"questions":7,"notes":"Se agregó control de temperatura"}}',
   'Se agregó pregunta de temperatura durante vaciado.',NOW(),'{U_ADM1}');
""")

# ── INSPECCIONES ───────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- INSPECCIONES")
a("-- ============================================================")
a(f"""INSERT INTO inspections (id, tenant_id, project_id, template_id, template_version,
  stage_id, sector_id, code, title, description, inspection_type, status, priority,
  scheduled_date, started_at, finished_at,
  assigned_to_id, assigned_by_id, supervisor_id, contractor_id, specialty_id,
  score, passing_score, passed, total_questions, answered_questions,
  conforming_count, non_conforming_count, na_count,
  latitude, longitude, weather_conditions, temperature,
  is_offline_created, created_at, created_by) VALUES

  ('{INS1}','{T1}','{P1}','{TPL1}',2,'{ST1_1}','{SEC1_1}','INS-2025-001',
   'Inspección Obra Gruesa — Torre A Piso 8',
   'Control de vaciado de losa nivel 8 Torre A.',
   'ordinaria','cerrada','normal',
   '2025-06-15 09:00:00+00','2025-06-15 09:30:00+00','2025-06-15 11:45:00+00',
   '{U_INS1}','{U_SUP1}','{U_SUP1}','{CT1}','{SP1}',
   85.5,70.0,TRUE,7,7,6,1,0,
   -33.3890,-70.5765,'Soleado',22.5,
   FALSE,NOW(),'{U_ADM1}'),

  ('{INS2}','{T1}','{P1}','{TPL2}',1,'{ST1_2}','{SEC1_1}','INS-2025-002',
   'Inspección Terminaciones — Torre A Pisos 5-6',
   'Revisión de pisos y muros terminados en pisos 5 y 6.',
   'ordinaria','en_proceso','alta',
   '2026-03-18 10:00:00+00','2026-03-18 10:15:00+00',NULL,
   '{U_INS2}','{U_SUP1}','{U_SUP1}','{CT2}','{SP2}',
   NULL,80.0,NULL,4,2,1,1,0,
   -33.3891,-70.5766,'Nublado',18.0,
   FALSE,NOW(),'{U_ADM1}'),

  ('{INS3}','{T1}','{P1}','{TPL1}',2,'{ST1_2}','{SEC1_2}','INS-2025-003',
   'Inspección Obra Gruesa — Torre B Piso 10',
   'Control de enfierradura y moldajes antes del vaciado.',
   'ordinaria','programada','normal',
   '2026-03-25 09:00:00+00',NULL,NULL,
   '{U_INS1}','{U_SUP1}','{U_SUP1}','{CT1}','{SP1}',
   NULL,70.0,NULL,7,0,0,0,0,
   NULL,NULL,NULL,NULL,
   FALSE,NOW(),'{U_ADM1}'),

  ('{INS4}','{T1}','{P1}','{TPL2}',1,'{ST1_2}','{SEC1_1}','INS-2025-004',
   'Inspección Terminaciones — Torre A Pisos 3-4',
   'Detectadas observaciones en revestimientos.',
   'ordinaria','observada','alta',
   '2026-03-10 09:00:00+00','2026-03-10 09:20:00+00','2026-03-10 12:00:00+00',
   '{U_INS2}','{U_SUP1}','{U_SUP1}','{CT2}','{SP2}',
   58.0,80.0,FALSE,4,4,2,2,0,
   -33.3892,-70.5767,'Soleado',20.0,
   FALSE,NOW(),'{U_ADM1}'),

  ('{INS5}','{T1}','{P2}','{TPL1}',2,NULL,NULL,'INS-2025-005',
   'Inspección Obra Gruesa — Casas Sector Norte Bloque 1',
   'Control de losas de casas pareadas bloque 1-10.',
   'ordinaria','finalizada','normal',
   '2025-11-20 09:00:00+00','2025-11-20 09:10:00+00','2025-11-20 13:00:00+00',
   '{U_INS1}','{U_SUP1}','{U_SUP1}','{CT1}','{SP1}',
   92.0,70.0,TRUE,7,7,7,0,0,
   -33.2012,-70.6703,'Soleado',25.0,
   FALSE,NOW(),'{U_ADM1}');
""")

# ── INSPECTION ANSWERS (para INS1 completada) ─────────────────────────────
a("-- ============================================================")
a("-- RESPUESTAS DE INSPECCIÓN (INS-2025-001 completa)")
a("-- ============================================================")
answers = [
    (Q1,'true',  None, None, True,  False, 2.0),
    (Q2,'true',  None, None, True,  False, 1.5),
    (Q3,'22.5',  None, 22.5, True,  False, 1.0),
    (Q4,'true',  None, None, True,  False, 2.0),
    (Q5,'true',  None, None, True,  False, 2.0),
    (Q6,'false', None, None, False, False, 0.0),  # NC — recubrimiento insuficiente
    (Q7,'bueno', OPT1, None, True,  False, 1.0),
]
ans_rows = []
for q, val, opt, num, conf, na, score in answers:
    opt_str = f"'{opt}'" if opt else 'NULL'
    num_str = str(num) if num else 'NULL'
    conf_str = 'TRUE' if conf else 'FALSE'
    na_str   = 'TRUE' if na else 'FALSE'
    ans_id   = str(uuid.uuid4())
    ans_rows.append(
        f"  ('{ans_id}','{T1}','{INS1}','{q}','{val}',{opt_str},{num_str},NULL,"
        f"{conf_str},{na_str},{score},NULL,NULL,NULL,"
        f"'2025-06-15 11:00:00+00','{U_INS1}',NOW(),NOW())"
    )
a("INSERT INTO inspection_answers (id, tenant_id, inspection_id, question_id, answer_value,")
a("  selected_option_id, numeric_value, date_value, is_conforming, is_na, score, notes,")
a("  latitude, longitude, answered_at, answered_by, created_at, updated_at) VALUES")
a(",\n".join(ans_rows) + ";\n")

# ── OBSERVATIONS ───────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- OBSERVACIONES")
a("-- ============================================================")
a(f"""INSERT INTO observations (id, tenant_id, project_id, inspection_id, code, title, description,
  specialty_id, category, severity, status, stage_id, sector_id,
  location_description, contractor_id, assigned_to_id, assigned_by_id,
  detected_at, detected_by, due_date, root_cause, corrective_action,
  latitude, longitude, is_recurring, created_at, created_by) VALUES

  ('{OBS1}','{T1}','{P1}','{INS1}','OBS-2025-001',
   'Recubrimiento insuficiente en enfierradura — Torre A P8',
   'Recubrimiento medido de 1.5 cm, norma exige mínimo 2.0 cm en losa de piso. '
   'Se evidencia en 3 puntos distintos del sector norponiente.',
   '{SP1}','hormigon_armado','alta','en_correccion','{ST1_1}','{SEC1_1}',
   'Losa piso 8, sector norponiente entre ejes C-D / 3-4',
   '{CT1}','{U_CONT1}','{U_SUP1}',
   '2025-06-15 12:00:00+00','{U_INS1}','2025-06-30',
   'Error de instalación de separadores de enfierradura.',
   'Demolición y reconstrucción del sector afectado con separadores certificados.',
   -33.3890,-70.5765,FALSE,NOW(),'{U_ADM1}'),

  ('{OBS2}','{T1}','{P1}','{INS4}','OBS-2026-001',
   'Nivelación de piso fuera de tolerancia — Torre A P3',
   'Medición de planimetría detecta desviación de 6mm en 2m lineal. '
   'Tolerancia máxima según especificación técnica es ±3mm.',
   '{SP2}','terminaciones','media','asignada','{ST1_2}','{SEC1_1}',
   'Departamentos 301, 302 y 303 — sala comedor',
   '{CT2}','{U_CONT1}','{U_SUP1}',
   '2026-03-10 12:00:00+00','{U_INS2}','2026-04-15',
   'Deficiencia en control de nivelación durante instalación de contrapiso.',
   'Escarificado y nivelación con mortero autonivelante en zonas afectadas.',
   -33.3891,-70.5767,FALSE,NOW(),'{U_ADM1}'),

  ('{OBS3}','{T1}','{P1}','{INS4}','OBS-2026-002',
   'Manchas de humedad en muro sur — Torre A P4',
   'Se observan manchas de humedad activa en muro perimetral sur, '
   'posible filtración por junta de expansión.',
   '{SP4}','impermeabilizacion','alta','abierta','{ST1_2}','{SEC1_1}',
   'Muro sur departamento 401, altura 1.2m desde el piso',
   '{CT2}',NULL,'{U_SUP1}',
   '2026-03-10 12:30:00+00','{U_INS2}','2026-04-10',
   NULL,NULL,
   -33.3892,-70.5766,FALSE,NOW(),'{U_ADM1}'),

  ('{OBS4}','{T1}','{P1}','{INS1}','OBS-2025-002',
   'Fisura en viga secundaria — Torre A P8',
   'Fisura de 0.3mm en viga secundaria en zona de apoyo. '
   'Requiere evaluación de ingeniero calculista.',
   '{SP1}','estructura','critica','cerrada','{ST1_1}','{SEC1_1}',
   'Viga VS-15 eje D entre ejes 3-4 piso 8',
   '{CT1}','{U_CONT1}','{U_SUP1}',
   '2025-06-15 13:00:00+00','{U_INS1}','2025-07-15',
   'Deformación diferencial durante curado por temperatura excesiva.',
   'Inyección de resina epóxica certificada + informe de ingeniero calculista aprobado.',
   -33.3890,-70.5765,FALSE,NOW(),'{U_ADM1}'),

  ('{OBS5}','{T1}','{P2}','{INS5}','OBS-2025-003',
   'Traslapo insuficiente en enfierradura horizontal — Casa 5',
   'Traslapo medido de 28 cm, especificación exige mínimo 35 cm.',
   '{SP1}','hormigon_armado','alta','cerrada',NULL,NULL,
   'Losa de techo casa 5 sector norte, eje perimetral',
   '{CT1}','{U_CONT1}','{U_SUP1}',
   '2025-11-20 13:30:00+00','{U_INS1}','2025-12-05',
   'Corte de barra en posición incorrecta por falta de control de planos.',
   'Adición de longitud de traslapo mediante soldadura certificada + control radiográfico.',
   -33.2012,-70.6703,FALSE,NOW(),'{U_ADM1}');
""")

# ── OBSERVATION HISTORY ────────────────────────────────────────────────────
a("-- ============================================================")
a("-- HISTORIAL DE OBSERVACIONES")
a("-- ============================================================")
a(f"""INSERT INTO observation_history (id, tenant_id, observation_id, action,
  previous_status, new_status, previous_assigned_to, new_assigned_to,
  comment, created_at, created_by) VALUES

  ('{str(uuid.uuid4())}','{T1}','{OBS1}','created',NULL,'abierta',NULL,NULL,
   'Observación registrada durante inspección INS-2025-001.','2025-06-15 12:00:00+00','{U_INS1}'),

  ('{str(uuid.uuid4())}','{T1}','{OBS1}','assigned','abierta','asignada',NULL,'{U_CONT1}',
   'Asignada a contratista Estructuras Del Sur para corrección.','2025-06-16 08:00:00+00','{U_SUP1}'),

  ('{str(uuid.uuid4())}','{T1}','{OBS1}','status_change','asignada','en_correccion','{U_CONT1}','{U_CONT1}',
   'Contratista confirmó inicio de trabajos de corrección.','2025-06-20 09:30:00+00','{U_CONT1}'),

  ('{str(uuid.uuid4())}','{T1}','{OBS4}','created',NULL,'abierta',NULL,NULL,
   'Fisura detectada durante inspección. Se solicitó evaluación de calculista.','2025-06-15 13:00:00+00','{U_INS1}'),

  ('{str(uuid.uuid4())}','{T1}','{OBS4}','closed','corregida','cerrada','{U_CONT1}',NULL,
   'Informe de ingeniero aprobado. Reparación verificada y aceptada.','2025-07-20 10:00:00+00','{U_SUP1}');
""")

# ── REINSPECCIONES ─────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- REINSPECCIONES")
a("-- ============================================================")
a(f"""INSERT INTO reinspections (id, tenant_id, original_inspection_id, observation_id,
  code, status, result, scheduled_date, executed_at, executed_by,
  created_at, created_by) VALUES
  ('{REINSP1}','{T1}','{INS4}','{OBS2}','REINSP-2026-001',
   'programada',NULL,'2026-04-20 09:00:00+00',NULL,NULL,
   NOW(),'{U_SUP1}');
""")

# ── NOTIFICACIONES ─────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- NOTIFICACIONES")
a("-- ============================================================")
a(f"""INSERT INTO notifications (id, tenant_id, user_id, type, title, body,
  entity_type, entity_id, is_read, sent_email, sent_push, created_at) VALUES

  ('{str(uuid.uuid4())}','{T1}','{U_INS1}','inspection_assigned',
   'Nueva inspección asignada',
   'Se te ha asignado la inspección INS-2025-003 programada para el 25 de marzo.',
   'inspection','{INS3}',FALSE,TRUE,FALSE,NOW()),

  ('{str(uuid.uuid4())}','{T1}','{U_CONT1}','observation_assigned',
   'Observación asignada para corrección',
   'Se te ha asignado la observación OBS-2026-001: Nivelación de piso fuera de tolerancia. Plazo: 15/04/2026.',
   'observation','{OBS2}',FALSE,TRUE,FALSE,NOW()),

  ('{str(uuid.uuid4())}','{T1}','{U_SUP1}','observation_due_soon',
   'Observación próxima a vencer',
   'La observación OBS-2026-002 vence el 10/04/2026 y aún está abierta.',
   'observation','{OBS3}',FALSE,TRUE,FALSE,NOW()),

  ('{str(uuid.uuid4())}','{T1}','{U_ADM1}','inspection_completed',
   'Inspección completada con observaciones',
   'La inspección INS-2025-004 fue finalizada con 2 No Conformidades. Score: 58.0%%.',
   'inspection','{INS4}',TRUE,FALSE,FALSE,NOW()),

  ('{str(uuid.uuid4())}','{T1}','{U_INS2}','inspection_assigned',
   'Nueva inspección asignada',
   'Se te ha asignado la inspección INS-2025-002 en ejecución.',
   'inspection','{INS2}',TRUE,TRUE,FALSE,NOW());
""")

# ── AUDIT LOGS ─────────────────────────────────────────────────────────────
a("-- ============================================================")
a("-- AUDIT LOGS")
a("-- ============================================================")
a(f"""INSERT INTO audit_logs (id, tenant_id, user_id, action, entity_type, entity_id,
  new_values, ip_address, created_at) VALUES
  ('{str(uuid.uuid4())}','{T1}','{U_ADM1}','Create','Project','{P1}',
   '{{"name":"Edificio Las Palmas — Torre Norte","status":"activo"}}','192.168.1.10',NOW() - INTERVAL '30 days'),
  ('{str(uuid.uuid4())}','{T1}','{U_ADM1}','Create','Project','{P2}',
   '{{"name":"Conjunto Residencial Norte — Fase I","status":"activo"}}','192.168.1.10',NOW() - INTERVAL '28 days'),
  ('{str(uuid.uuid4())}','{T1}','{U_SUP1}','Create','Inspection','{INS1}',
   '{{"code":"INS-2025-001","status":"programada"}}','192.168.1.11',NOW() - INTERVAL '20 days'),
  ('{str(uuid.uuid4())}','{T1}','{U_INS1}','Update','Inspection','{INS1}',
   '{{"status":"cerrada","score":85.5}}','10.0.0.5',NOW() - INTERVAL '10 days'),
  ('{str(uuid.uuid4())}','{T1}','{U_ADM1}','Login','User','{U_ADM1}',
   '{{"email":"admin@itopacifico.cl"}}','192.168.1.10',NOW() - INTERVAL '1 hour');
""")

# ── SEQUENCE COUNTERS ──────────────────────────────────────────────────────
a("-- ============================================================")
a("-- CONTADORES DE SECUENCIA")
a("-- ============================================================")
a(f"""INSERT INTO sequence_counters (id, tenant_id, entity_type, prefix, year, last_value) VALUES
  ('{str(uuid.uuid4())}','{T1}','inspection',  'INS', 2025, 2),
  ('{str(uuid.uuid4())}','{T1}','inspection',  'INS', 2026, 3),
  ('{str(uuid.uuid4())}','{T1}','observation',  'OBS', 2025, 2),
  ('{str(uuid.uuid4())}','{T1}','observation',  'OBS', 2026, 2),
  ('{str(uuid.uuid4())}','{T1}','reinspection','REINSP',2026,1),
  ('{str(uuid.uuid4())}','{T2}','inspection',  'INS', 2025, 0);
""")

a("COMMIT;")
a("")
a("-- ============================================================")
a("-- VERIFICACIÓN RÁPIDA")
a("-- ============================================================")
a("SELECT 'tenants'          AS tabla, COUNT(*) AS registros FROM tenants UNION ALL")
a("SELECT 'users',            COUNT(*) FROM users UNION ALL")
a("SELECT 'companies',        COUNT(*) FROM companies UNION ALL")
a("SELECT 'projects',         COUNT(*) FROM projects UNION ALL")
a("SELECT 'inspections',      COUNT(*) FROM inspections UNION ALL")
a("SELECT 'observations',     COUNT(*) FROM observations UNION ALL")
a("SELECT 'templates',        COUNT(*) FROM inspection_templates UNION ALL")
a("SELECT 'notifications',    COUNT(*) FROM notifications UNION ALL")
a("SELECT 'audit_logs',       COUNT(*) FROM audit_logs")
a("ORDER BY tabla;")

output = "\n".join(lines)
path = r'F:\SISTEMA DE INSPECCION DE OBRA\docs\sql\002_seed_data.sql'
with open(path, 'w', encoding='utf-8') as f:
    f.write(output)

print(f"Generado: {path}")
print(f"Tamaño: {len(output):,} caracteres")

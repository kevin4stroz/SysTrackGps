-- public.error_log definition
CREATE TABLE public.error_log (
	id_error_log uuid NOT NULL,
	controller_name varchar NOT NULL,
	method_name varchar NOT NULL,
	stack_trace varchar NOT NULL,
	created_date timestamp NOT NULL,
	code varchar NOT NULL,
	CONSTRAINT error_log_pk PRIMARY KEY (id_error_log)
);

-- public.vehiculo definition
CREATE TABLE public.vehiculo (
	id_vehiculo uuid NOT NULL,
	placa varchar NOT NULL,
	color varchar NOT NULL,
	modelo int4 NOT NULL,
	cilindraje numeric NOT NULL,
	capacidad_pasajeros int4 NOT NULL,
	capacidad_carga int4 NOT NULL,
	created_date date NOT NULL,
	CONSTRAINT vehiculos_pk PRIMARY KEY (id_vehiculo)
);

-- public.vehiculo_status definition
CREATE TABLE public.vehiculo_status (
	id_vehiculo_status uuid NOT NULL,
	descripcion varchar NOT NULL,
	created_date timestamp NOT NULL,
	CONSTRAINT vehiculo_status_pk PRIMARY KEY (id_vehiculo_status)
);

-- default data public.vehiculo_status
INSERT INTO public.vehiculo_status (id_vehiculo_status,descripcion,created_date) VALUES
	 ('9c017058-bf55-4919-bade-357fa72b5612'::uuid,'DISPONIBLE','2025-09-21 11:56:48.564'),
	 ('46b4b21d-d386-469f-beb3-b3e3a2079114'::uuid,'NO DISPONIBLE','2025-09-21 11:56:48.564'),
	 ('b17de527-94fc-4891-897a-ac1d0095faec'::uuid,'EN CURSO','2025-09-21 11:56:48.564');


-- public.vehiculo_vehiculo_status definition
CREATE TABLE public.vehiculo_vehiculo_status (
	id_vehiculo_vehiculo_status uuid NOT NULL,
	id_vehiculo uuid NOT NULL,
	id_vehiculo_status uuid NOT NULL,
	flg_current_status bool NOT NULL,
	created_date date NOT NULL,
	CONSTRAINT vehiculos_vehiculos_status_pk PRIMARY KEY (id_vehiculo_vehiculo_status)
);


-- public.vehiculo_vehiculo_status foreign keys
ALTER TABLE public.vehiculo_vehiculo_status ADD CONSTRAINT vehiculo_vehiculo_status_vehiculo_fk FOREIGN KEY (id_vehiculo) REFERENCES public.vehiculo(id_vehiculo);
ALTER TABLE public.vehiculo_vehiculo_status ADD CONSTRAINT vehiculo_vehiculo_status_vehiculo_status_fk FOREIGN KEY (id_vehiculo_status) REFERENCES public.vehiculo_status(id_vehiculo_status);


-- public.vehiculo_viaje definition
CREATE TABLE public.vehiculo_viaje (
	id_vehiculo_viaje uuid NOT NULL,
	id_vehiculo_vehiculo_status uuid NOT NULL,
	origen varchar NOT NULL,
	destino varchar NOT NULL,
	created_date date NOT NULL,
	key_redis_ruta varchar NOT NULL,
	CONSTRAINT vehiculo_viaje_pk PRIMARY KEY (id_vehiculo_viaje)
);

-- public.vehiculo_viaje foreign keys
ALTER TABLE public.vehiculo_viaje ADD CONSTRAINT vehiculo_viaje_vehiculo_vehiculo_status_fk FOREIGN KEY (id_vehiculo_vehiculo_status) REFERENCES public.vehiculo_vehiculo_status(id_vehiculo_vehiculo_status);



-- public.posicion_actual_viaje definition
CREATE TABLE public.posicion_actual_viaje (
	id_posicion_actual_viaje uuid NOT NULL,
	latitud float8 NOT NULL,
	longitud float8 NOT NULL,
	id_vehiculo_viaje uuid NOT NULL,
	created_date timestamp NOT NULL,
	CONSTRAINT posicion_actual_viaje_pk PRIMARY KEY (id_posicion_actual_viaje)
);


-- public.posicion_actual_viaje foreign keys
ALTER TABLE public.posicion_actual_viaje ADD CONSTRAINT posicion_actual_viaje_vehiculo_viaje_fk FOREIGN KEY (id_vehiculo_viaje) REFERENCES public.vehiculo_viaje(id_vehiculo_viaje);


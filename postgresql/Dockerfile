# Usa la imagen oficial de PostgreSQL
FROM postgres:16.8

# Configura las variables de entorno para la autenticación
ENV POSTGRES_USER=admin_prueba_tecnica
ENV POSTGRES_PASSWORD=l4tQmS5pv89IggWQTfJ
ENV POSTGRES_DB=SysTrackGps

# monta backup de datos
COPY init_db.sql /docker-entrypoint-initdb.d/

# Expone el puerto predeterminado de PostgreSQL
EXPOSE 5432

# Comando de inicio
CMD ["postgres"]

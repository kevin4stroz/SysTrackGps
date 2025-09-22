# Contexto

![](./Grafo_Mock.png)

Para este ejercicio se planteo el siguente grafo mock, el cual tiene 11 puntos, de 11 localidades, estas localidades se asociaron por medio de las rutas visibles en la grafica. Este mock de modelo en redis usando RedisGraph, cada una de las aristas del grafo son bidirecionales y la distancia(peso para la heuristica de la implemantacion de A*) se calcula por medio de la latitud y longitud obtenida de los puntos. se uso la herramienta https://geojson.io/ para modelar esto y poder obtener algunos datos reales.

El ejercicio consta de un vehiculo (o N dependiendo de los que se parametricen en el sistema o se agreguen por medio del endpoint), podra definir un punto de origen y un punto de destino. la implementacion realizara el calculo de la ruta optima usando A* (tene en cuenta que no estamos usando calles si no la linea que une de los puntos), esta ruta optima sera cacheada en redis y almacenada de manera transaccional en postgresql. Una vez el vehiculo seleccione la ruta, se habilitara el endpoint que recibe coordenadas, cada una de estas coordenadas seran almacenadas tanto en postgresql como redis, este proceso de almacenamiento se realizo usando rabbitmq para asegurar que si alguna de las dos bases de datos no responde, el mensaje quedara guardado en rabbitmq hasta que el almacenamiento se de en ambas partes. 

Tambien manejamos 3 posibles estados para un vehiculo. cuando recien se crea el vehiculo queda en esta DISPONIBLE (puede iniciar viajes), cuando esta recorriendo el viaje esta en estado EN CURSO (en proceso de envio de coordenadas), cuando termina el viaje queda DISPONIBLE DE NUEVO (el terminar en viaje es que envie una posicion de alrededor de 50 metros a la posicion destino) y un estado NO DISPONIBLE (se parametrizo pero no se uso a lo largo de la implementacion).

Para el manejo de auditoria se almacenan y se controlan los errores dados dentro de la aplicacion.

# comandos de despliegue

El despliegue se realizo usando docker compose, cada uno de los servicios quedo configurado con scripts de inicializacion para asegurar que los datos necesarios para la primera ejecucion queden almacenados dentro de cada contendor y su respectivo almacenamiento en el host.

```
git clone [repo]
cd SysTrackGps
docker compose up -d
```

# Base de datos

## Postgresql

![](./Mer.png)

- `public.error_log` : tabla de auditoria de errores controlados
- `public.vehiculo` : tabla de almacemiento de metadata de vehiculo
- `public.vehiculo_status` : tabla de datos maestros de estados del vehiculo
- `public.vehiculo_vehiculo_status` : tabla de asociacion de vehiculos y estados
- `public.vehiculo_viaje` : tabla de almacenamiento de viaje o ruta, aqui se guarda el origen y destino y la key de consulta para redis
- `public.posicion_actual_viaje` : tabla de almacenamiento de coordenadas de posicion del vehiculo

## Redis grafo

```

/* ================================================
   Creación de Localidades con coordenadas
   ================================================ */

GRAPH.QUERY CityGraph "
CREATE
(:Localidad {name:'Usaquen', lat:4.716184571820477, lon:-74.04424425163468}),
(:Localidad {name:'Prado veraniego sur', lat:4.717335593755351, lon:-74.06434009036245}),
(:Localidad {name:'Suba', lat:4.743578375261492, lon:-74.08489790239479}),
(:Localidad {name:'Chapinero', lat:4.660703061232553, lon:-74.04909497132734}),
(:Localidad {name:'Barrios unidos', lat:4.670832794026751, lon:-74.07588942296492}),
(:Localidad {name:'Engativa', lat:4.712731494570704, lon:-74.11030643412036}),
(:Localidad {name:'Fontibon', lat:4.678429997618849, lon:-74.13386707262875}),
(:Localidad {name:'Santafe', lat:4.615347699066604, lon:-74.0705767299677}),
(:Localidad {name:'Puente Aranda', lat:4.620873381039658, lon:-74.10776558094774}),
(:Localidad {name:'Kennedy', lat:4.635608322257497, lon:-74.15026712492444}),
(:Localidad {name:'Bosa', lat:4.621564088258694, lon:-74.19461656211718})
"

/* ================================================
   Creación de aristas bidireccionales
   ================================================

'Suba' -> ('Prado veraniego sur', 'Engativa')
'Engativa' -> ('Fontibon', 'Suba')
'Fontibon' -> ('Engativa', 'Kennedy', 'Prado veraniego sur', 'Barrios unidos')
'Kennedy' -> ('Fontibon', 'Puente Aranda', 'Bosa')
'Bosa' -> ('Kennedy', 'Puente Aranda')
'Puente Aranda' -> ('Bosa', 'Kennedy', 'Prado veraniego sur', 'Santafe')
'Santafe' -> ('Puente Aranda', 'Barrios unidos', 'Chapinero')
'Chapinero' -> ('Santafe', 'Barrios unidos')
'Barrios unidos' -> ('Chapinero', 'Santafe', 'Fontibon', 'Usaquen')
'Usaquen' -> ( 'Barrios unidos', 'Prado veraniego sur')
'Prado veraniego sur' -> ('Suba', 'Fontibon', 'Puente Aranda', 'Usaquen')   

*/

GRAPH.QUERY CityGraph "
MATCH
  (suba:Localidad {name:'Suba'}),
  (prado:Localidad {name:'Prado veraniego sur'}),
  (engativa:Localidad {name:'Engativa'}),
  (fontibon:Localidad {name:'Fontibon'}),
  (kennedy:Localidad {name:'Kennedy'}),
  (bosa:Localidad {name:'Bosa'}),
  (puente:Localidad {name:'Puente Aranda'}),
  (santafe:Localidad {name:'Santafe'}),
  (chapinero:Localidad {name:'Chapinero'}),
  (barrios:Localidad {name:'Barrios unidos'}),
  (usaquen:Localidad {name:'Usaquen'})
CREATE
  (suba)-[:CONNECTED]->(prado), (prado)-[:CONNECTED]->(suba),
  (suba)-[:CONNECTED]->(engativa), (engativa)-[:CONNECTED]->(suba),
  (engativa)-[:CONNECTED]->(fontibon), (fontibon)-[:CONNECTED]->(engativa),
  (fontibon)-[:CONNECTED]->(kennedy), (kennedy)-[:CONNECTED]->(fontibon),
  (fontibon)-[:CONNECTED]->(prado), (prado)-[:CONNECTED]->(fontibon),
  (fontibon)-[:CONNECTED]->(barrios), (barrios)-[:CONNECTED]->(fontibon),
  (kennedy)-[:CONNECTED]->(bosa), (bosa)-[:CONNECTED]->(kennedy),
  (kennedy)-[:CONNECTED]->(puente), (puente)-[:CONNECTED]->(kennedy),
  (bosa)-[:CONNECTED]->(puente), (puente)-[:CONNECTED]->(bosa),
  (puente)-[:CONNECTED]->(santafe), (santafe)-[:CONNECTED]->(puente),
  (puente)-[:CONNECTED]->(prado), (prado)-[:CONNECTED]->(puente),
  (santafe)-[:CONNECTED]->(chapinero), (chapinero)-[:CONNECTED]->(santafe),
  (santafe)-[:CONNECTED]->(barrios), (barrios)-[:CONNECTED]->(santafe),
  (chapinero)-[:CONNECTED]->(barrios), (barrios)-[:CONNECTED]->(chapinero),
  (barrios)-[:CONNECTED]->(usaquen), (usaquen)-[:CONNECTED]->(barrios),
  (usaquen)-[:CONNECTED]->(prado), (prado)-[:CONNECTED]->(usaquen)
"
```

# Endpoints

### api/Vehiculos/GetVehiculoStatusList

- lista los estados maestros que puede tener el vehiculo

```bash
curl --location 'http://localhost:8080/api/Vehiculos/GetVehiculoStatusList'
```

### api/Vehiculos/CreateVehiculo

- Creacion del vehiculo

```bash
curl --location 'http://localhost:8080/api/Vehiculos/CreateVehiculo' \
--header 'Content-Type: application/json' \
--data '{
  "placa": "AAA-111",
  "color": "negro",
  "modelo": 1994,
  "cilindraje": 3.960,
  "capacidad_pasajeros": 5,
  "capacidad_carga": 2500
}'
```

### api/Rutas/GetAllLocalidades

- Lista las localidades almacenadas en el grafo de redis

```bash
curl --location 'http://localhost:8080/api/Rutas/GetAllLocalidades'
```

### api/Rutas/IniciarViaje

- Cambia de estado el vehiculo, calcula ruta optima de localidad origen a localidad destino, habilita el endpoint para recibir la informacion de las coordenadas actuales del vehiculo


```bash
curl --location 'http://localhost:8080/api/Rutas/IniciarViaje' \
--header 'Content-Type: application/json' \
--data '{
  "id_vehiculo": "70275af4-62c9-4e45-b543-2f2e5e6855c2",
  "localidad_origen": "Bosa",
  "localidad_destino": "Usaquen"
}'
```


### api/Rutas/RecvCoordsCurrentPosition

- Recibe el id del carro y las posiciones actuales, realiza persitencia en postgresql y redis por medio de docker


```bash
curl --location 'http://localhost:8080/api/Rutas/RecvCoordsCurrentPosition' \
--header 'Content-Type: application/json' \
--data '{
  "id_vehiculo": "70275af4-62c9-4e45-b543-2f2e5e6855c2",
  
  "latitud": 4.71618457182048,
  "longitud": -74.0442442516347
}'
```

# Dificultades

- Primera vez usando redis
- Me falto realizar los mocks pruebas de pruebas unitarias, pero se realizaron pruebas de debug y manuales por medio de los endpoints y los request en curl.

# Evidencia fotografica

![](Evidencia.png)


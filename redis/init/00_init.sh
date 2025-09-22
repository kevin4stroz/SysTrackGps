#!/bin/bash

LOG_FILE="/var/log/redis-init.log"

REDIS_CLI="redis-cli -a C3JbTQR3zB82zogUKN"

echo "Esperando a que Redis arranque..." >> $LOG_FILE
while ! $REDIS_CLI ping | grep -q PONG; do 
  sleep 1
done

echo "Redis listo, creando datos..." >> $LOG_FILE

echo "=== Creando Localidades ===" >> $LOG_FILE
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Usaquen', lat:4.716184571820477, lon:-74.04424425163468})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Prado veraniego sur', lat:4.717335593755351, lon:-74.06434009036245})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Suba', lat:4.743578375261492, lon:-74.08489790239479})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Chapinero', lat:4.660703061232553, lon:-74.04909497132734})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Barrios unidos', lat:4.670832794026751, lon:-74.07588942296492})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Engativa', lat:4.712731494570704, lon:-74.11030643412036})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Fontibon', lat:4.678429997618849, lon:-74.13386707262875})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Santafe', lat:4.615347699066604, lon:-74.0705767299677})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Puente Aranda', lat:4.620873381039658, lon:-74.10776558094774})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Kennedy', lat:4.635608322257497, lon:-74.15026712492444})"
$REDIS_CLI GRAPH.QUERY CityGraph "CREATE (:Localidad {name:'Bosa', lat:4.621564088258694, lon:-74.19461656211718})"

echo "=== Creando aristas bidireccionales ===" >> $LOG_FILE


# 'Suba' -> ('Prado veraniego sur', 'Engativa')
# 'Engativa' -> ('Fontibon', 'Suba')
# 'Fontibon' -> ('Engativa', 'Kennedy', 'Prado veraniego sur', 'Barrios unidos')
# 'Kennedy' -> ('Fontibon', 'Puente Aranda', 'Bosa')
# 'Bosa' -> ('Kennedy', 'Puente Aranda')
# 'Puente Aranda' -> ('Bosa', 'Kennedy', 'Prado veraniego sur', 'Santafe')
# 'Santafe' -> ('Puente Aranda', 'Barrios unidos', 'Chapinero')
# 'Chapinero' -> ('Santafe', 'Barrios unidos')
# 'Barrios unidos' -> ('Chapinero', 'Santafe', 'Fontibon', 'Usaquen')
# 'Usaquen' -> ( 'Barrios unidos', 'Prado veraniego sur')
# 'Prado veraniego sur' -> ('Suba', 'Fontibon', 'Puente Aranda', 'Usaquen') 


# Suba
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (s:Localidad {name:'Suba'}), (p:Localidad {name:'Prado veraniego sur'}) CREATE (s)-[:CONNECTED]->(p), (p)-[:CONNECTED]->(s)"
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (s:Localidad {name:'Suba'}), (e:Localidad {name:'Engativa'}) CREATE (s)-[:CONNECTED]->(e), (e)-[:CONNECTED]->(s)"

# Engativa
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (e:Localidad {name:'Engativa'}), (f:Localidad {name:'Fontibon'}) CREATE (e)-[:CONNECTED]->(f), (f)-[:CONNECTED]->(e)"

# Fontibon
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (f:Localidad {name:'Fontibon'}), (k:Localidad {name:'Kennedy'}) CREATE (f)-[:CONNECTED]->(k), (k)-[:CONNECTED]->(f)"
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (f:Localidad {name:'Fontibon'}), (p:Localidad {name:'Prado veraniego sur'}) CREATE (f)-[:CONNECTED]->(p), (p)-[:CONNECTED]->(f)"
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (f:Localidad {name:'Fontibon'}), (b:Localidad {name:'Barrios unidos'}) CREATE (f)-[:CONNECTED]->(b), (b)-[:CONNECTED]->(f)"

# Kennedy
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (k:Localidad {name:'Kennedy'}), (b:Localidad {name:'Bosa'}) CREATE (k)-[:CONNECTED]->(b), (b)-[:CONNECTED]->(k)"
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (k:Localidad {name:'Kennedy'}), (p:Localidad {name:'Puente Aranda'}) CREATE (k)-[:CONNECTED]->(p), (p)-[:CONNECTED]->(k)"

# Bosa
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (b:Localidad {name:'Bosa'}), (p:Localidad {name:'Puente Aranda'}) CREATE (b)-[:CONNECTED]->(p), (p)-[:CONNECTED]->(b)"

# Puente Aranda
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (p:Localidad {name:'Puente Aranda'}), (s:Localidad {name:'Santafe'}) CREATE (p)-[:CONNECTED]->(s), (s)-[:CONNECTED]->(p)"
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (p:Localidad {name:'Puente Aranda'}), (pr:Localidad {name:'Prado veraniego sur'}) CREATE (p)-[:CONNECTED]->(pr), (pr)-[:CONNECTED]->(p)"

# Santafe
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (s:Localidad {name:'Santafe'}), (c:Localidad {name:'Chapinero'}) CREATE (s)-[:CONNECTED]->(c), (c)-[:CONNECTED]->(s)"
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (s:Localidad {name:'Santafe'}), (b:Localidad {name:'Barrios unidos'}) CREATE (s)-[:CONNECTED]->(b), (b)-[:CONNECTED]->(s)"

# Chapinero
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (c:Localidad {name:'Chapinero'}), (b:Localidad {name:'Barrios unidos'}) CREATE (c)-[:CONNECTED]->(b), (b)-[:CONNECTED]->(c)"

# Barrios unidos
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (b:Localidad {name:'Barrios unidos'}), (u:Localidad {name:'Usaquen'}) CREATE (b)-[:CONNECTED]->(u), (u)-[:CONNECTED]->(b)"

# Usaquen
$REDIS_CLI GRAPH.QUERY CityGraph "MATCH (u:Localidad {name:'Usaquen'}), (pr:Localidad {name:'Prado veraniego sur'}) CREATE (u)-[:CONNECTED]->(pr), (pr)-[:CONNECTED]->(u)"

echo "=== Inicialización completada ===" >> $LOG_FILE

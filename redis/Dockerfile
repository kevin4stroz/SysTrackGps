FROM redislabs/redisgraph:2.10.10

COPY redis.conf /usr/local/etc/redis/redis.conf
COPY ./init/00_init.sh /docker-entrypoint-initdb.d/00_init.sh
COPY ./init/init.sh /entry/init.sh
RUN sed -i 's/\r$//' /entry/init.sh
RUN sed -i 's/\r$//' /docker-entrypoint-initdb.d/00_init.sh
CMD ["sh", "/entry/init.sh"]
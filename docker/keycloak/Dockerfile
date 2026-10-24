FROM quay.io/keycloak/keycloak:26.4 AS builder
ADD --chmod=644 https://repo1.maven.org/maven2/com/microsoft/sqlserver/mssql-jdbc/12.8.1.jre11/mssql-jdbc-12.8.1.jre11.jar /opt/keycloak/providers/
ENV KC_DB=mssql KC_HEALTH_ENABLED=true
RUN /opt/keycloak/bin/kc.sh build --db=mssql

FROM quay.io/keycloak/keycloak:26.4
COPY --from=builder /opt/keycloak/ /opt/keycloak/
ENTRYPOINT ["/opt/keycloak/bin/kc.sh"]

# FCG.Catalog.API

Descrição
- Microserviço da API do catálogo (jogos, compras, eventos).
- Projeto .NET 8.

Visão geral
- Expõe endpoints HTTP na porta `8080`.
- Usa SQL Server (connection string), JWT para autenticação e RabbitMQ para eventos.
- Dockerfile com estágios: `build`, `runtime` e `migrations` (estágio `migrations` executa `dotnet ef database update`).
- Manifests Kubernetes em `k8s/` (ConfigMap, Secret template, Deployment com initContainer para migrations e Service).

Pré-requisitos
- .NET 8 SDK (desenvolvimento)
- Docker
- docker-compose (opcional)
- kubectl (para deploy em cluster)
- Acesso a um registry de containers (se for rodar no cluster)

Variáveis de ambiente (usadas pela aplicação)
- `ConnectionStrings__DefaultConnection` — connection string do SQL Server.  
  Ex.: `Server=sqlserver-service,1433;Database=FCGCatalog;User Id=sa;Password=...;TrustServerCertificate=True;Encrypt=False;`
- `Jwt__SecretKey` — chave secreta para JWT.
- `RabbitMQ__Host` — host/service do RabbitMQ (ex.: `rabbitmq-service`).
- `RabbitMQ__Username` e `RabbitMQ__Password` — credenciais do RabbitMQ.
- `ASPNETCORE_ENVIRONMENT` — `Development` | `Production`.
- `ASPNETCORE_URLS` — ex.: `http://+:8080`.

Observação: não versionar segredos. Use `Secret` no Kubernetes ou um provider de secrets.

Build e execução com Docker
1. Build da imagem do runtime:
   docker build --target runtime -t catalogapi:latest -f FCG.Catalog.API/Dockerfile .
2. (Opcional) Build da imagem de migrations:
   docker build --target migrations -t catalogapi-migrations:latest -f FCG.Catalog.API/Dockerfile .
3. Executar container (exemplo):
   docker run --rm -e ConnectionStrings__DefaultConnection="SUA_CONN" \
     -e Jwt__SecretKey="SUA_CHAVE" \
     -e RabbitMQ__Host="rabbitmq-service" \
     -p 8080:8080 \
     catalogapi:latest

Executar migrations (local / container)
- Usando a imagem de migrations:
  docker run --rm \
    -e ConnectionStrings__DefaultConnection="SUA_CONN" \
    -e Jwt__SecretKey="SUA_CHAVE" \
    -e RabbitMQ__Host="rabbitmq-service" \
    catalogapi-migrations:latest
- Ou rodar localmente com `dotnet ef database update` no projeto `FCG.Catalog.Infrastructure` apontando o `--startup-project` para `FCG.Catalog.API`.

Exemplo mínimo (trecho) para `docker-compose.yml`
- Configure variáveis em `.env` ou diretamente:
  services:
    catalogapi:
      image: catalogapi:latest
      ports:
        - "8080:8080"
      environment:
        - ConnectionStrings__DefaultConnection=${ConnectionStrings__DefaultConnection}
        - Jwt__SecretKey=${Jwt__SecretKey}
        - RabbitMQ__Host=${RabbitMQ__Host}

Deploy no Kubernetes (manifests em `k8s/`)
1. Atualize as imagens em `k8s/deployment.yaml` para apontar para seu registry (se necessário).
2. Aplicar ConfigMap / Secret / Deployment / Service:
   kubectl apply -f k8s/configmap.yaml
   kubectl apply -f k8s/secret.yaml          # ou criar Secret via CLI (recomendado)
   kubectl apply -f k8s/deployment.yaml
   kubectl apply -f k8s/service.yaml
3. Exemplo para criar secret via CLI:
   kubectl create secret generic catalogapi-secret \
     --from-literal=ConnectionStrings__DefaultConnection="SUA_CONN" \
     --from-literal=Jwt__SecretKey="SUA_CHAVE" \
     --from-literal=RabbitMQ__Host="rabbitmq-service" \
     --from-literal=username="RABBIT_USER" \
     --from-literal=password="RABBIT_PASS"

Notas importantes
- `k8s/secret-template.yaml` é um template — substitua valores sensíveis antes de aplicar.
- O `Deployment` inclui um `initContainer` que executa as migrations — garanta que a imagem de migrations esteja disponível para o cluster.
- Não versionar segredos no repositório. Use um provider de secrets (Azure Key Vault, HashiCorp Vault, etc.) em produção.
- Portas: container escuta `8080`; o `Service` mapeia `80 -> 8080`.

Scripts e orquestrador
- Caso exista uma pasta/orquestrador com `deploy.ps1`, este script automatiza build/push/apply para Kubernetes — revise parâmetros antes de executar.
- Cada microserviço deve possuir seu próprio `README.md` com as variáveis e instruções específicas.

Ajuda adicional
- Posso gerar um `docker-compose.yml` completo para o ambiente local ou revisar/adaptar um `deploy.ps1` para seu fluxo CI/CD.
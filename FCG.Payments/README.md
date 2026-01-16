# FCG.Payments 

Visão geral
- Repositório que contém o microserviço de pagamentos (`FCG.Payments.EventProcessor`) e os artefatos de orquestração (Kubernetes + Docker Compose).
- O serviço é uma Worker Service em .NET 8 que consome eventos `order-placed` do RabbitMQ, processa pagamentos e publica `payment-processed` via MassTransit.

Estrutura principal
- `FCG.Payments.EventProcessor/` — projeto .NET 8 (Worker) e `Dockerfile`.
- `FCG.Payments.Application/`, `FCG.Payments.Infrastructure/`, `FCG.Payments.Domain/` — camadas do domínio/infra.
- `k8s/` — manifests Kubernetes:
  - `k8s/configmap.yaml`
  - `k8s/secret.yaml`
  - `k8s/secret-template.yaml`
  - `k8s/deployment.yaml`
- Orquestrador (fora deste repositório ou em pasta `orchestrator/`): contém `deploy.ps1` (deploy para K8s) e `docker-compose.yml` (deploy via Docker Compose).

Variáveis de ambiente (usadas pelo serviço)
- `RabbitMQ__Host` — host do RabbitMQ (ex.: `rabbitmq-service`)
- `RabbitMQ__Username` — usuário do RabbitMQ (ex.: `guest`)
- `RabbitMQ__Password` — senha do RabbitMQ (ex.: `guest`)
- `ASPNETCORE_ENVIRONMENT` — ambiente .NET (ex.: `Development`, `Production`, `Deployment`)

Observação sobre mapeamentos
- `appsettings.json` espera `RabbitMQ:Host`, `RabbitMQ:Username`, `RabbitMQ:Password`. O provider de ambiente do .NET lê `RabbitMQ__Host`, etc.
- Os `ConfigMap`/`Secret` em `k8s/` podem usar chaves diferentes; certifique-se de expor variáveis com os nomes esperados (`RabbitMQ__Host`, `ASPNETCORE_ENVIRONMENT`, etc.) no `Deployment`.

Como rodar localmente (.NET)
1. Requisitos: .NET 8 SDK.
2. No diretório do projeto:
   - `cd FCG.Payments.EventProcessor`
   - `dotnet run --configuration Release`
3. Use variáveis de ambiente para apontar para um RabbitMQ acessível:
   - Ex.: `RabbitMQ__Host`, `RabbitMQ__Username`, `RabbitMQ__Password`.

Build de imagem Docker
- Do root do repositório:
  - `docker build -f FCG.Payments.EventProcessor/Dockerfile -t fcgpayments-worker:latest .`
- Push para registry (opcional):
  - `docker tag fcgpayments-worker:latest <registry>/fcgpayments-worker:latest`
  - `docker push <registry>/fcgpayments-worker:latest`
- Se usar imagem privada, atualize o `image` em `k8s/deployment.yaml` e adicione `imagePullSecrets` se necessário.

Execução com Docker (local)
- Exemplo:
  - `docker run --rm -e RabbitMQ__Host=rabbitmq-host -e RabbitMQ__Username=guest -e RabbitMQ__Password=guest -e ASPNETCORE_ENVIRONMENT=Development fcgpayments-worker:latest`

Orquestrador — Docker Compose
- O orquestrador inclui um `docker-compose.yml` que orquestra os serviços (RabbitMQ + worker).
- Para subir com Docker Compose:
  - `docker-compose up -d`
- Observações:
  - Verifique os nomes das variáveis de ambiente no `docker-compose.yml` (devem mapear para `RabbitMQ__Host`, etc.).
  - Se o `docker-compose.yml` estiver em outra pasta (ex.: `orchestrator/`), execute o comando nessa pasta.

Orquestrador — Kubernetes (deploy.ps1)
- O orquestrador fornece um script PowerShell `deploy.ps1` para aplicar os manifests no cluster.
- Uso típico:
  - Abra PowerShell com permissão e selecione o contexto correto do `kubectl` (`kubectl config current-context`).
  - Execute: `.\deploy.ps1` (no diretório do orquestrador).
- O script deve:
  - Aplicar `k8s/configmap.yaml`
  - Criar/atualizar `k8s/secret.yaml` (ou usar `secret-template.yaml`)
  - Atualizar a imagem no `k8s/deployment.yaml` (se necessário)
  - Aplicar `k8s/deployment.yaml`
- Dicas:
  - Para clusters locais (`kind`, `minikube`) e imagens construídas localmente, carregue a imagem no cluster:
    - kind: `kind load docker-image fcgpayments-worker:latest --name <cluster-name>`
    - minikube: `minikube image load fcgpayments-worker:latest`

Manifests importantes
- `k8s/configmap.yaml` — contém `RABBITMQ_HOST`, `RABBITMQ_PORT`, `RABBITMQ_MANAGEMENT_PORT`, `ASPNETCORE_ENVIRONMENT`. Verifique se esses nomes são expostos como variáveis de ambiente esperadas pela aplicação.
- `k8s/secret.yaml` — contém `RabbitMQ__Username` e `RabbitMQ__Password` (exemplo).
- `k8s/deployment.yaml` — Deployment do worker; contém initContainer que espera RabbitMQ em `rabbitmq-service:5672`.

Segurança / Boas práticas
- Não commit secrets reais. Use `k8s/secret-template.yaml` como modelo.
- Em clusters de produção, gerencie segredos com ferramentas dedicadas (Azure Key Vault, SealedSecrets, etc.).
- Use `imagePullSecrets` para imagens privadas.

Diagnóstico rápido
- Ver logs:
  - `kubectl logs deploy/payments-worker`
  - `kubectl describe pod <pod>` para eventos
- Se o pod ficar em Init:
  - Verifique se o RabbitMQ está disponível no host/porta configurados.
- Se imagem não for puxada:
  - Confirme `image` no `deployment.yaml` e credenciais de pull.

Próximos passos sugeridos
- Validar e alinhar nomes de variáveis entre `k8s/configmap.yaml` e o `Deployment` (usar `RabbitMQ__Host` para consistência).
- Adicionar instruções no `deploy.ps1` para aceitar parâmetros de imagem/tag e namespace.
- Fornecer `docker-compose.override.yml` para ambiente de desenvolvimento.

Se quiser, eu gero:
- Exemplo de `docker-compose.yml` para desenvolvimento (RabbitMQ + worker).
- Exemplo de `deploy.ps1` com parâmetros para imagem/tag e namespace.
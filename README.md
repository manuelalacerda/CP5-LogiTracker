# 🚚 LogiTracker - CP5
 
Projeto desenvolvido para o Checkpoint 5 (CP5) da FIAP, evoluindo a API REST desenvolvida nos checkpoints anteriores com a inclusão de Versionamento de API, Paginação de Dados e Rate Limit.

## 👥 Integrantes

* **Manuela de Lacerda Soares** — RM 564887
* **Sofia Siqueira Fontes** — RM 563829

## 🏗️ Domínio e Banco de Dados

**Domínio:** Logística e Transportes (Delivery, Cargo, Driver, Vehicle, Carrier)

**SGBD:** Oracle (via Entity Framework Core + Repository Pattern e migrations para persistência dos dados)

---

## ▶️ Como executar

### ⚠️ Pré-requisitos

* .NET 9 SDK ou superior
* Oracle Database ou ambiente Oracle acessível
* Entity Framework Core CLI (dotnet-ef)

### 1. Restaurar dependências
```bash
dotnet restore
```

### 2. Compilar
```bash
dotnet build
```

### 3. Atualizar o banco
> Com a connection string configurada no ambiente local:
```bash
dotnet ef database update --project LogiTracker.Infrastructure --startup-project LogiTracker.API
```

### 4. Executar a API
```bash
dotnet run --project LogiTracker.API
```

---

## 🔗 URLs

| Recurso | URL |
|---|---|
| Swagger (Development) | `http://localhost:5138/swagger/index.html` |
| Health check | `http://localhost:5138/health` |
| Listagem **v1** (deprecada) | `http://localhost:5138/api/Delivery?api-version=1.0` |
| Listagem **v2** (atual) | `http://localhost:5138/api/Delivery` |
| Listagem v2 com paginação explícita | `http://localhost:5138/api/Delivery?page=1&pageSize=20` |
 
---

## 🔀 Versionamento da API (CP5 - Seção A)

O recurso escolhido é **Delivery** (entregas). Os dois contratos usam o **mesmo serviço de aplicação** (`IDeliveryService`); só o formato da resposta da listagem muda.

| Versão | Status | `GET /api/Delivery` devolve |
|---|---|---|
| **1.0** | **Deprecada** | Lista (array) completa, contrato antigo do CP3, **sem paginação** |
| **2.0** | Atual (padrão) | **Envelope paginado** com totais |

### Como informar a versão

```http
GET /api/Delivery?api-version=1.0
```
```http
GET /api/Delivery
X-Api-Version: 1.0
```
```http
GET /api/Delivery
```
* Query string `api-version` ou header `X-Api-Version` escolhem a versão.
* **Sem versão informada → cai na 2.0** (`DefaultApiVersion = 2.0`, `AssumeDefaultVersionWhenUnspecified = true`).
* Toda resposta traz os headers `api-supported-versions: 1.0, 2.0` e `api-deprecated-versions: 1.0` (`ReportApiVersions = true`).
### Demais endpoints

* `GET /api/Delivery/{id}`, `POST /api/Delivery` e `DELETE /api/Delivery/{id}` valem para **as duas versões** (1.0 e 2.0). O fluxo de escrita do CP3 continua funcionando **sem informar versão** (cai na 2.0) ou com `api-version=1.0`.
* Os controllers dos outros recursos (Cargo, Carrier, Driver, Vehicle) estão marcados com `[ApiVersionNeutral]`: continuam no ar, sem mudança, e aparecem nos dois documentos do Swagger.
### Swagger por versão

Em Development, o Swagger tem um documento por versão (`/swagger/v1.0/swagger.json` e `/swagger/v2.0/swagger.json`), selecionáveis em **Select a definition**. A descrição do documento da v1 informa que a versão está **deprecada**.
 
---

## 📑 Paginação na v2.0 (CP5 - Seção B)

A listagem v2 (GET /api/Delivery) executa o corte diretamente no banco de dados Oracle via Skip() e Take() com ordenação fixa (OrderBy).   

|  Parâmetro  | Padrão | Regra / Limites |
|---|---|---|
| `page` | 1 | Inteiro >= 1 |
| `pageSize` | 20 | Inteiro de 1 a 100 |

* `page < 1` ou `pageSize` fora de 1-100 → **400** (Problem Details, com a mensagem da regra que falhou).
* Página além do total → **200** com `items: []` (não é erro).

Formato do Envelope de Resposta (200 OK):
```json
{
  "page": 1,
  "pageSize": 20,
  "totalItems": 137,
  "totalPages": 7,
  "items": [],
  "hasPrevious": false,
  "hasNext": true
}
```

Exemplos de **400**:

```http
GET /api/Delivery?page=0
GET /api/Delivery?pageSize=9999
```

---

## 🛑 Rate Limit (CP5 - Seção C)

Para proteger a escrita na API, foi configurada uma política nativa de Fixed Window (Microsoft.AspNetCore.RateLimiting)

* Endpoint Limitado: POST /api/Delivery
* Limite: 10 requisições por minuto por endereço IP.
* Comportamento em Estouro (HTTP 429):
  * Retorna o cabeçalho Retry-After indicando o tempo de espera em segundos.
  * Retorna o corpo formatado em JSON.
* Isolamento do Health Check: O endpoint GET /health está isento da limitação (DisableRateLimiting), mantendo-se sempre operacional em 200 OK.   

Exemplo de resposta no estouro:

```json
{
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Limite de 10 requisições por minuto excedido. Tente novamente em 42s.",
  "instance": "/api/Delivery"
}
```

### Como criar uma entrega (corpo do POST)

```json
{
  "vehicleId": "<id de um veículo>",
  "driverId": "<id de um motorista>",
  "cargoId": "<id de uma carga>"
}
```

> Cada carga só pode ter **uma** entrega (índice único em `CargoId`). Crie uma carga nova (`POST /api/Cargo`) para cada entrega.

---

## ❤️ Health Check

O projeto possui um único endpoint de Health Check:

```http
GET /health
```

O endpoint retorna o relatório completo dos checks:

|  Check  | O que valida |
|---|---|
| `self` | Disponibilidade da aplicação |
| `oracle-db` | Disponibilidade do banco Oracle. |

### Exemplo de resposta:

```json
{
  "status": "Healthy",
  "totalDurationMs": 2848.8161,
  "checks": [
    {
      "name": "self",
      "status": "Healthy",
      "description": "O processo da API est\u00E1 no ar.",
      "durationMs": 2.1387,
      "error": null
    },
    {
      "name": "oracle-db",
      "status": "Healthy",
      "description": null,
      "durationMs": 2461.9508,
      "error": null
    }
  ]
}
```

### Status HTTP

| Status    | HTTP                    |
| --------- | ----------------------- |
| Healthy   | 200 OK                  |
| Degraded  | 200 OK                  |
| Unhealthy | 503 Service Unavailable |

---

## 📊 Logs e Observabilidade

* Utilização do ILogger com registos estruturados.
* Inclusão do identificador único da requisição (HttpContext.TraceIdentifier registrado como traceId).
* Registo de início e finalização em operações cruciais de escrita (POST /api/Delivery).
* Captura centralizada de exceções via GlobalExceptionHandler sem expor stack traces em produção.

---

## 🧪 Testes Unitários
Para executar (na raiz da solution):

```bash
dotnet test
```

A solution possui dois projetos de testes:

* `LogiTracker.Domain.Tests` - testes de regras de negócio puras (sem mocks)
* `LogiTracker.Application.Tests` - testes das regras de serviço e paginação com Moq para isolar os repositórios


### 1. Domain.Tests

`LogiTracker.Domain.Tests` - referencia só o Domain, sem mocks.

* Para cenários de sucesso são utilizados `[Fact]` + `[Theory]`/`[InlineData]`
* Para cenários de erro os testes seguem o padrão **Arrange, Act, Assert (AAA)**.


### 2. Application.Tests

`LogiTracker.Application.Tests` - testa `DeliveryService` utilizando Moq, as interfaces dos repositórios são substituídas por mocks.

* Testa serviços existentes da camada Application.
* Para cenários de erro, quanto há dependência inexistente, verifica `Times.Never` na persistência.
* Para cenários de sucesso, a operação esperada é verificada com `Times.Once`

As evidências (`/health` Healthy/Unhealthy, logs com `traceId`, saída do `dotnet test`) estão em `/docs`.

---

## 🛡️ Tratamento de Exceções

O projeto mantém o `GlobalExceptionHandler` desenvolvido nos CPs anteriores.

As exceções são convertidas para respostas HTTP utilizando `ProblemDetails`.

| Exceção                     | HTTP                      |
| --------------------------- | ------------------------- |
| `ResourceNotFoundException` | 404 Not Found             |
| `DomainException`           | 400 Bad Request           |
| `ArgumentException`         | 400 Bad Request           |
| `KeyNotFoundException`      | 404 Not Found             |
| `InvalidOperationException` | 409 Conflict              |
| Outras exceções             | 500 Internal Server Error |

---

## 📁 Evidências do Checkpoint 5 (`/docs/Evidences - CP5`)

| Arquivo | O que comprova                                         |
|---|--------------------------------------------------------------|
| `v1-list.json` / `v1-list.png` | `GET` v1 (`?api-version=1.0`): lista (array) sem paginação |
| `v2-paged.json` / `v2-paged.png` | `GET` sem versão (cai na 2.0): envelope paginado |
| `version-headers.png` | Headers `api-supported-versions` e `api-deprecated-versions` |
| `swagger-versions-dropdown.png` | Swagger com os dois grupos (v1 e v2) no seletor |
| `swagger-v1.png` | Documento v1 marcado como deprecada |
| `swagger-v2.png` | Documento v2 com `page` e `pageSize` |
| `v2-page1-page2.png` | Páginas 1 e 2 (`pageSize=2`) com itens distintos |
| `page-and-pagesize-invalid-400.png` | 400 para `page=0` e`pageSize=9999` |
| `error429-and-GET-health-200.png` | 429 com `Retry-After` e corpo JSON no `POST /api/Delivery` e `GET /health` 200 depois do estouro do rate limit |
| `dotnet-test.png` | Saída completa do `dotnet test` (testes do CP4 e de paginação) |

As evidências do CP4 (health Healthy/Unhealthy, logs com `traceId`) continuam nas subpastas de `/docs/`.

---




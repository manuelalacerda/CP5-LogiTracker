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

* .NET 8 SDK ou superior
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

## 🔗 URLs:
Swagger:

O Swagger permite visualizar e testar os endpoints da API.

```text
http://localhost:5138/swagger/index.html
```

Health check:

```text
http://localhost:5138/health
```

---

## 🔀 Versionamento da API (CP5 - Seção A)

A API suporta duas versões ativas do recurso Delivery sem interromper os consumidores legados:   

* v1.0 (Deprecada): Mantém o contrato antigo do CP3, devolvendo uma lista completa (IReadOnlyList) sem paginação.
* *v2.0 (Atual - Padrão): Introduz a paginação com envelope estruturado e filtros no banco de dados.   
---

## 📑 Paginação na v2.0 (CP5 - Seção B)

A listagem v2 (GET /api/Delivery/paged) executa o corte diretamente no banco de dados Oracle via Skip() e Take() com ordenação fixa (OrderBy).   

|  Parâmetro  | Padrão | Regra / Limites |
|---|---|---|
| `page` | 1 | Inteiro >= 1 |
| `pageSize` | 20 | Inteiro de 1 a 100 |

Formato do Envelope de Resposta (200 OK):
```JSON
{
  "page": 1,
  "pageSize": 20,
  "totalItems": 137,
  "totalPages": 7,
  "items": []
}
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



---

# ❤️ Health Check

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
  "totalDuration": "00:00:00.1234567",
  "entries": {
    "self": {
      "status": "Healthy",
      "duration": "00:00:00.0010000"
    },
    "oracle-db": {
      "status": "Healthy",
      "duration": "00:00:00.1200000"
    }
  }
}
```

### Status HTTP

| Status    | HTTP                    |
| --------- | ----------------------- |
| Healthy   | 200 OK                  |
| Degraded  | 200 OK                  |
| Unhealthy | 503 Service Unavailable |

---

# 📊 Logs e Observabilidade

* Utilização do ILogger com registos estruturados.
* Inclusão do identificador único da requisição (HttpContext.TraceIdentifier registrado como traceId).
* Registo de início e finalização em operações cruciais de escrita (POST /api/Delivery).
* Captura centralizada de exceções via GlobalExceptionHandler sem expor stack traces em produção.

---

# 🧪 Testes Unitários
Para executar:

```bash
dotnet test
```

A solução possui dois projetos de testes:

```text
LogiTracker.Domain.Tests: Testes de regras de negócio puras (sem mocks)
LogiTracker.Application.Tests: Testes das regras de serviço e paginação com Moq para isolar os repositórios.
```

## 1. Domain.Tests

`LogiTracker.Domain.Tests` — referencia só o Domain, sem mocks.

* Para cenários de sucesso são utilizados `[Fact]` + `[Theory]`/`[InlineData]`
* Para cenários de erro os testes seguem o padrão **Arrange, Act, Assert (AAA)**.


## 2. Application.Tests

`LogiTracker.Application.Tests` - testa `DeliveryService` utilizando Moq, as interfaces dos repositórios são substituídas por mocks.

* Testa serviços existentes da camada Application.
* Para cenários de erro, quanto há dependência inexistente, verifica `Times.Never` na persistência.
* Para cenários de sucesso, a operação esperada é verificada com `Times.Once`

As evidências (`/health` Healthy/Unhealthy, logs com `traceId`, saída do `dotnet test`) estão em `/docs`.
---

# 🛡️ Tratamento de Exceções

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

# 📁 Evidências do Checkpoint 5 (/docs/)

Na pasta /docs/ do repositório encontram-se as evidências de validação do CP5:   
* v1-list.json / screenshot: Resposta da v1 com array simples sem paginação.
* v2-paged.json / screenshot: Resposta da v2 paginada com envelope completo.
* version-headers.png: Evidência dos cabeçalhos api-supported-versions e api-deprecated-versions.
* swagger-versions.png: Tela do Swagger dividida entre os documentos v1 (deprecada) e v2.
* page-invalid-400.png: Resposta 400 Bad Request ao testar page=0 ou pageSize=9999.
* rate-limit-429.png: Resposta 429 Too Many Requests com o cabeçalho Retry-After.
* health-200-after-429.png: Acesso com sucesso (200 OK) ao /health após o bloqueio de escrita do Rate Limit.
* dotnet-test.png: Saída dos testes automatizados com sucesso.

---




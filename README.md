# 🚚 LogiTracker - CP5
 
Projeto desenvolvido para o **Checkpoint 5 (CP5)** da FIAP, evoluindo a API REST desenvolvida nos checkpoints anteriores.

## 👥 Integrantes

* **Manuela de Lacerda Soares** — RM 564887
* **Sofia Siqueira Fontes** — RM 563829

## 🏗️ Domínio e Banco de Dados

**Domínio:** Logística e Transportes

**SGBD:** Oracle (via Entity Framework Core + Repository Pattern e migrations para persistência dos dados)

---

## ▶️ Como executar

### ⚠️ Pré-requisitos

* .NET SDK compatível com o projeto
* Oracle Database ou ambiente Oracle acessível
* Entity Framework Core CLI

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

A aplicação utiliza `ILogger<T>` para geração de logs estruturados com propriedades nomeadas e utilizam `HttpContext.TraceIdentifier` (o identificador é apresentado como `traceId`):

Em um fluxo de escrita da aplicação são registrados:

* `DeliveryController.Create` (`POST /api/Delivery`) - loga início e sucesso da operação.
* `GlobalExceptionHandler` registra exceções não tratadas em nível `Error`, contendo a exceção, a mensagem e o `traceId`.
* Em ambiente de produção, a resposta HTTP não expõe stack trace.

---

# 🧪 Testes
Para executar:

```bash
dotnet test
```

A solução possui dois projetos de testes:

```text
LogiTracker.Domain.Tests
LogiTracker.Application.Tests
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

# 💰 Financial Ledger API

API financeira desenvolvida com foco em **segurança**, **boas práticas arquiteturais** e **qualidade de código**, seguindo padrões utilizados em sistemas corporativos reais.

---

## 🚀 Objetivo

Fornecer uma base sólida para controle financeiro, garantindo:

* Isolamento total de dados por usuário
* Segurança contra acessos indevidos
* Organização e escalabilidade do código
* Alta testabilidade

---

## 🧠 Arquitetura

O projeto foi construído utilizando:

* **Clean Architecture**
* **CQRS (Command Query Responsibility Segregation)**
* **MediatR**
* **Unit of Work**
* **Repository Pattern**

Estrutura em camadas:

```id="vuzg45"
📦 src
 ┣ 📂 Domain
 ┣ 📂 Application
 ┣ 📂 Infrastructure
 ┣ 📂 API
```

---

## 🧩 Camadas do Projeto

### 🟣 Domain (Regra de Negócio)

Responsável pelo coração da aplicação.

**Contém:**

* Entidades (Account, Transaction, Category)
* Regras de negócio puras
* Validações de domínio
* Interfaces de repositório

---

### 🔵 Application (Casos de Uso)

Camada responsável pela orquestração da aplicação.

**Contém:**

* Commands e Queries (CQRS)
* Handlers com MediatR
* DTOs
* Validações

**Pipeline Behaviors (Cross-cutting concerns):**

```csharp id="m6m4r2"
services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

---

### 🟢 Infrastructure (Infraestrutura)

Responsável pela comunicação com o mundo externo.

**Contém:**

* Implementação de repositórios
* Banco de dados
* Serviços auxiliares

```csharp id="8dy5wq"
services.AddScoped<IApplicationDbContext, AppDbContext>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

---

### 🟡 API (Camada de Entrada)

Ponto de entrada da aplicação.

```csharp id="s2lg4u"
services.AddMemoryCache();
services.AddScoped<AbuseProtectionService>();
```

---

## 🔄 Fluxo de Requisição

Aqui está como uma requisição percorre a aplicação:

```id="n8k1y2"
[HTTP Request]
      ↓
[Controller]
      ↓
[MediatR]
      ↓
[Pipeline Behaviors] (Validation / Logging)
      ↓
[Handler (Application)]
      ↓
[Domain Rules]
      ↓
[Repository]
      ↓
[Unit of Work]
      ↓
[Database]
      ↓
[Response]
```

💡 **Resumo:**

* Controller apenas delega
* MediatR desacopla
* Pipeline trata validação/log
* Handler executa caso de uso
* Domain garante regra
* Repository acessa dados
* UnitOfWork controla commit

---

## 🧱 Diagrama de Arquitetura

```id="j4x9qa"
           ┌───────────────┐
           │     API       │
           │ Controllers   │
           │ Middlewares   │
           └───────┬───────┘
                   │
                   ▼
           ┌───────────────┐
           │ Application   │
           │ CQRS +        │
           │ MediatR       │
           └───────┬───────┘
                   │
                   ▼
           ┌───────────────┐
           │    Domain     │
           │ Rules / Core  │
           └───────┬───────┘
                   │
                   ▼
           ┌───────────────┐
           │ Infrastructure│
           │ DB / External │
           └───────────────┘
```

---

## 🔐 Segurança

* Autenticação com **JWT**
* Isolamento de dados por usuário
* Proteção contra manipulação de IDs
* Rate Limiting (anti brute force)

---

## 📊 Observabilidade

* Logging estruturado
* Pipeline de logging
* Global Exception Handling

---

## 🧪 Testes

* Testes unitários com **xUnit**
* Uso de mocks
* Validação isolada das regras de negócio

---

## 🛠️ Tecnologias Utilizadas

* .NET / C#
* Entity Framework
* MediatR
* FluentValidation
* JWT Authentication
* xUnit
* Moq

---

## 📌 Diferenciais

* Arquitetura limpa e escalável
* Forte separação de responsabilidades
* Segurança aplicada na prática
* Uso de pipelines (cross-cutting bem definidos)
* Pronto para ambiente corporativo

---

## ▶️ Como executar

```bash id="q6c3fs"
git clone <repo-url>
cd financial-ledger-api
dotnet run
```

---

## 📈 Melhorias futuras

* Docker
* Mensageria (RabbitMQ / Kafka)
* Observabilidade (Prometheus + Grafana)
* 2FA

---

## 🤝 Contribuição

Sinta-se à vontade para contribuir 🚀

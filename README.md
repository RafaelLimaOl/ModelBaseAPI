# 📘 Projeto — Documentação Geral

Bem-vindo ao repositório! Este projeto foi desenvolvido utilizando práticas modernas de arquitetura, segurança, observabilidade e resiliência. A seguir, você encontrará uma visão completa dos principais recursos implementados.

---

## 🚀 Funcionalidades do Projeto

### 📄 **Documentação com Swagger**
- Documentação interativa da API utilizando **Swagger UI**.
- Permite testar endpoints diretamente pela interface.
- Configuração de segurança para autenticação via JWT.

---

### 🔐 **Autenticação e Autorização com JWT**
- Implementação de autenticação baseada em **JSON Web Tokens (JWT)**.
- Suporte a controle de acesso via **Claims** e **Roles**.
- Middleware configurado para validação automática dos tokens.

---

### 🚦 **Rate Limiting por IP**
- Utilização de **RateLimit** para evitar sobrecarga.
- Limitação baseada em **IP**, bloqueando requisições abusivas.
- Configurada para funcionamento global e por rota.

---

### 🛡️ **Resilience Pipeline**
- Aplicação de padrões de resiliência (Polly):
  - Retry
  - Circuit Breaker
  - Timeout
  - Fallback
- Configurado como Pipeline centralizado.

---

### 🗄️ **Banco de Dados SQL Server**
- Conexão com **SQL Server**.
- Utilização do micro Mapeador Objeto-Relacional  **DAPPER**
- Implementação com **Entity Framework Core**.

---

### 🌐 **Política de CORS Customizada**
- Restrição de origens, métodos e cabeçalhos.
- Suporte para ambientes especificos.

---

### 📊 **Telemetria com OpenTelemetry & Prometheus**
- Métricas, logs e traces coletados via **OpenTelemetry**.
- Exportação para **Prometheus** para consultas e gráficos através do **ASPIRE**.

---

### 🧩 **Exceções e Erros Customizados**
- Middleware para tratamento global de erros.
- Respostas padronizadas e rastreamento de exceções.

---

### 📝 **Serilog & Logs Estruturados**
- Configuração avançada de logs via **Serilog**.
- Correlation Id para rastreamento de requisições.
- Envio para sinks variados (Console, Seq, etc.).

---

### 🧱 **Middlewares Customizados**
- **Idempotency Middleware** para evitar duplicidade de requisições.
- Logs estruturados por requisição.
- Proteção contra ataques **XSS**.
- **Health Check** completo da aplicação.

---

### ✔️ **Validação com FluentValidation**
- Validação de Requests utilizando **FluentValidation**.
- Regras centralizadas e respostas padronizadas.

---

### 📦 **Estruturas de Dados**
- **Entity**: Modelos de banco de dados.
- **Request**: DTOs para entrada.
- **Response**: DTOs para saída.

---

### 🧩 **Arquitetura: Controller, Service, Repository**
- Controllers.
- Regras de negócio isoladas em Services.
- Persistência desacoplada via Repository Pattern.

---

### ⚡ **Cache com IMemoryCache**
- Cache in-memory para melhor desempenho.
- Suporte a expiração e invalidação.

---

### 📬 **Resposta de API Customizada**
- Estrutura consistente de retorno:
  - Sucesso
  - Mensagens
  - Erros
  - Dados

---

### 🐇 **Integração com RabbitMQ**
- Publicação e consumo de mensagens.
- Queues e Bindings configurados.

---

### 🧪 **Testes Unitários**
- Testes automatizados para serviços e componentes críticos.
- Utilização de xUnit.

---

### 🌐 **Integração com Aspire**
- Observabilidade unificada via plataforma Aspire.
- Dashboards e monitoramento.

---

## 🧷 Estrutura do Projeto (REST)
```
src/
  Controller/
  Interface/
    IServices/
    IRespository/
  Services/
  Repository/
  Models/
    Request/
    Response/
    Validator/
    Entity
tests/
Docker/
docs/
```

---

## 🏁 Como Executar o Projeto
```sh
dotnet restore
dotnet build
dotnet run
```

Com Docker:
```sh
docker-compose up -d
```

---

## 🤝 Contribuição
Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

---

## 📄 Licença
Uso FREE, projetos pessoais e uso comercial.


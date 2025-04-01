# FreteManager

Sistema de Gerenciamento de Pedidos para Logística

## Descrição do Projeto

O FreteManager é uma aplicação de gerenciamento de pedidos de transporte que permite a criação, atualização, visualização e exclusão de pedidos logísticos. O sistema inclui funcionalidades para gerenciar informações de clientes, controlar o status de pedidos e integrar com serviços de terceiros para cálculo de frete.

A aplicação foi desenvolvida em C# com .NET 8.0, utilizando Entity Framework Core para acesso a dados em SQL Server. A API é protegida por autenticação JWT e documentada com Swagger/OpenAPI.

### Funcionalidades Principais

- **Gestão de Clientes**: Cadastro completo com validações de dados
- **Gestão de Pedidos**: Ciclo de vida completo, desde a criação até a entrega
- **Cálculo de Frete**: Integração com API externa (Frenet) e mecanismo de contingência
- **Segurança**: Autenticação JWT com roles e proteção de endpoints
- **Logs**: Sistema de logging completo com NLog
- **Testes**: Testes unitários e de integração para garantia de qualidade

## Instruções de Configuração

### Pré-requisitos

- .NET 8.0 SDK
- SQL Server 2019 ou superior
- Visual Studio 2022 ou VS Code

### Configuração Inicial

1. Clone o repositório:
   ```bash
   git clone https://github.com/seu-usuario/FreteManager.git
   cd FreteManager
   ```

2. Configure a string de conexão com o banco de dados:
   
   Edite o arquivo `appsettings.json` na pasta `FreteManager` e ajuste a string de conexão `DefaultConnection` para apontar para o seu servidor SQL Server.
   
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=seu_servidor;Database=FreteManagerDb;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   }
   ```

3. Criação do banco de dados:

   Execute os scripts SQL na seguinte ordem:
   - `FreteManager/Scripts/CriacaoBd.sql` - Cria o banco de dados
   - `FreteManager/Scripts/CriacaoTabelas.sql` - Cria as tabelas e relacionamentos

4. Configure as chaves de API:

   Ainda no arquivo `appsettings.json`, configure as seções:
   
   ```json
   "Jwt": {
     "Secret": "SuaChaveSecretaComPeloMenos32Caracteres",
     "ExpirationInHours": 8
   },
   "Frenet": {
     "Token": "SeuTokenDaApiFrenet"
   }
   ```

   > **Importante**: Em ambiente de produção, utilize o User Secrets ou variáveis de ambiente para armazenar informações sensíveis.

## Como Executar a Aplicação

### Via Visual Studio

1. Abra a solução `FreteManager.sln` no Visual Studio
2. Defina o projeto `FreteManager` como projeto de inicialização
3. Pressione F5 ou use o botão "Iniciar" para executar o projeto
4. O navegador abrirá automaticamente com a interface do Swagger

### Via Linha de Comando

1. Navegue até a pasta do projeto principal:
   ```bash
   cd FreteManager
   ```

2. Execute a aplicação:
   ```bash
   dotnet run
   ```

3. Abra um navegador e acesse a URL:
   ```
   https://localhost:7007
   ```
   ou
   ```
   http://localhost:5016
   ```

4. A interface do Swagger estará disponível na raiz da aplicação

## Como Executar os Testes

### Via Visual Studio

1. Abra o Test Explorer (Gerenciador de Testes) pelo menu "Exibir" > "Gerenciador de Testes"
2. Clique em "Executar Todos os Testes" ou selecione categorias específicas

### Via Linha de Comando

1. Para executar todos os testes:
   ```bash
   dotnet test
   ```

2. Para executar testes de uma categoria específica:
   ```bash
   dotnet test --filter "Category=Integration" # Testes de integração
   dotnet test --filter "Category=Unit" # Testes unitários
   ```

3. Para executar testes com relatório de cobertura de código:
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
   ```

## Documentação da API

A API é documentada usando Swagger/OpenAPI, que fornece uma interface interativa para explorar e testar os endpoints. Após iniciar a aplicação, acesse a raiz da URL para abrir a documentação.

### Endpoints Principais

- **Auth**
  - POST `/v1/Auth/register` - Registra um novo usuário
  - POST `/v1/Auth/login` - Autentica um usuário e retorna um token JWT

- **Clientes**
  - GET `/v1/Clientes` - Lista todos os clientes
  - GET `/v1/Clientes/{id}` - Obtém um cliente específico
  - POST `/v1/Clientes` - Cria um novo cliente
  - PUT `/v1/Clientes/{id}` - Atualiza um cliente
  - DELETE `/v1/Clientes/{id}` - Remove um cliente

- **Pedidos**
  - GET `/v1/Pedidos` - Lista todos os pedidos
  - GET `/v1/Pedidos/{id}` - Obtém um pedido específico
  - GET `/v1/Pedidos/cliente/{clienteId}` - Lista pedidos de um cliente
  - POST `/v1/Pedidos` - Cria um novo pedido
  - PUT `/v1/Pedidos/{id}` - Atualiza um pedido
  - DELETE `/v1/Pedidos/{id}` - Remove um pedido
  - PATCH `/v1/Pedidos/{id}/status` - Atualiza o status de um pedido

- **Frete**
  - POST `/v1/Frete/calcular-frete` - Calcula o valor do frete

### Autenticação

Todos os endpoints (exceto login e registro) exigem autenticação via token JWT. Para autenticar:

1. Use o endpoint `/v1/Auth/login` para obter um token
2. Inclua o token no cabeçalho de suas requisições:
   ```
   Authorization: Bearer seu_token_aqui
   ```

## Respostas ao Questionário

### Seção 1: Decisões de Arquitetura

1. **Padrão de Design**: O projeto segue o padrão Repository com Service Layer, separando claramente a lógica de acesso a dados da lógica de negócios. Isso facilita a manutenção, testabilidade e possíveis alterações futuras, como a substituição do mecanismo de persistência.

2. **Tratamento de Erros**: Implementamos um sistema centralizado de tratamento de exceções usando middleware. Criamos exceções personalizadas como `EntityNotFoundException` e `BusinessRuleViolationException` para capturar cenários específicos de negócio. O middleware transforma essas exceções em respostas HTTP padronizadas no formato RFC 7807 (Problem Details).

3. **Segurança**: A segurança é implementada em várias camadas:
   - Autenticação via JWT com expiração de token
   - Senha armazenada como hash usando PBKDF2 com salt
   - Validação de modelo para prevenir ataques de injeção
   - Proteção de endpoints sensíveis com o atributo [Authorize]
   - Testes específicos para verificar vulnerabilidades de segurança

4. **Performance**: Otimizamos a performance com:
   - Cache em memória para resultados do cálculo de frete
   - Uso de consultas assíncronas para não bloquear threads
   - Carregamento seletivo de dados utilizando projeções do EF Core
   - Índices adequados no banco de dados

### Seção 2: Desafios e Soluções

1. **Integração com API Externa**: O maior desafio foi criar uma integração robusta com a API externa de cálculo de frete. Implementamos:
   - Tratamento abrangente de erros
   - Mecanismo de fallback para quando a API está indisponível
   - Sistema de cache para reduzir chamadas repetidas
   - Validação rigorosa de parâmetros antes de chamar a API

2. **Gestão do Ciclo de Vida de Pedidos**: Implementamos um sistema de transição de estados que impede transições inválidas (por exemplo, não permite que um pedido cancelado volte a ser processado). Isso garante a integridade dos dados e previne inconsistências.

3. **Tratamento de Relacionamentos Complexos**: Os relacionamentos entre pedidos e pacotes exigiram atenção especial. Criamos lógica específica para garantir que a atualização de pedidos lide adequadamente com adições, remoções e modificações de pacotes.

### Seção 3: Possíveis Melhorias

1. **Escalabilidade**: Para melhorar a escalabilidade, poderíamos:
   - Implementar cache distribuído usando Redis
   - Adotar mensageria para processamento assíncrono de operações demoradas
   - Configurar a aplicação para execução em contêineres com Kubernetes

2. **Monitoramento**: Adicionaríamos:
   - Integração com ferramentas como Application Insights ou Prometheus
   - Métricas personalizadas para monitorar tempos de resposta
   - Sistema de alertas para falhas críticas

3. **UI**: O próximo passo seria desenvolver:
   - Interface web usando Blazor ou React
   - Aplicativo móvel para rastreamento de pedidos
   - Dashboard para análise de dados e relatórios

4. **Integrações Adicionais**: Poderíamos expandir as integrações:
   - Serviços de mapas para visualização de rotas
   - Serviços de SMS/email para notificações
   - APIs de transportadoras adicionais

---

## Tecnologias Utilizadas

- **Backend**: .NET 8.0, C#, ASP.NET Core Web API
- **ORM**: Entity Framework Core 9.0.3
- **Banco de Dados**: Microsoft SQL Server
- **Documentação**: Swagger/OpenAPI
- **Autenticação**: JWT (JSON Web Tokens)
- **Logging**: NLog
- **Testes**: xUnit, Moq, Microsoft.EntityFrameworkCore.InMemory
- **Ferramentas de Qualidade**: Microsoft.NET.Test.Sdk, coverlet

## Licença

Este projeto está licenciado sob a licença MIT - veja o arquivo LICENSE para detalhes.

## Contato

Para questões, sugestões ou problemas, entre em contato através de:
- Email: contato@fretemanager.com
- GitHub: [https://github.com/seu-usuario/FreteManager](https://github.com/seu-usuario/FreteManager)

---

Desenvolvido como parte de um teste técnico para demonstração de habilidades em desenvolvimento .NET.

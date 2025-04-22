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
   git clone https://github.com/adrianorosadreis/FreteManager.git
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

### Seção 1: C# e Desenvolvimento de API RESTful
Qual é o propósito do comando using em C#?
Resposta: B) Importar um namespace

Qual é o tipo de dado mais apropriado para armazenar uma data e hora em C#?
Resposta: C) DateTime

Qual é o método mais comum para criar uma instância de uma classe em C#?
Resposta: A) new

Qual é o propósito do atributo [ApiController] em ASP.NET Core?
Resposta: A) Definir um controlador de API

Qual é o tipo de retorno mais comum para um método de API RESTful em C#?
Resposta: D) IActionResult

Qual é o propósito do método ConfigureServices no arquivo Startup.cs em ASP.NET Core?
Resposta: A) Configurar os serviços de dependência

Qual é o propósito do atributo [Route] em ASP.NET Core?
Resposta: A) Definir uma rota de API

Qual é o tipo de retorno mais comum para um método de API RESTful que retorna uma lista de objetos em C#?
Resposta: B) IEnumerable<T>

Qual é o propósito do método AddDbContext no arquivo Startup.cs em ASP.NET Core?
Resposta: A) Adicionar um contexto de banco de dados

Qual é o tipo de dado mais apropriado para armazenar uma chave primária em C#?
Resposta: C) Guid

Qual é o propósito do método AddSwaggerGen no arquivo Startup.cs em ASP.NET Core?
Resposta: D) Gerar documentação de API com Swagger

Qual é o tipo de exceção mais comum para lidar com erros de validação de dados em C#?
Resposta: B) ValidationException

### Seção 2: Banco de Dados Microsoft SQL Server
Qual é o comando SQL utilizado para criar uma tabela no Microsoft SQL Server?
Resposta: A) CREATE TABLE

Qual é o tipo de dado mais comum utilizado para armazenar datas e horas no Microsoft SQL Server?
Resposta: C) DATETIME

Qual é o comando SQL utilizado para atualizar dados em uma tabela no Microsoft SQL Server?
Resposta: A) UPDATE

Qual é o comando SQL utilizado para excluir dados de uma tabela no Microsoft SQL Server?
Resposta: A) DELETE

Qual é o conceito de transação no Microsoft SQL Server?
Resposta: A) Conjunto de operações que devem ser executadas como uma unidade

Qual é o comando SQL utilizado para criar um índice em uma tabela no Microsoft SQL Server?
Resposta: A) CREATE INDEX

Qual é o comando SQL utilizado para criar uma visão em uma tabela no Microsoft SQL Server?
Resposta: A) CREATE VIEW

Qual é o objetivo do uso de particionamento de tabelas no SQL Server?
Resposta: A) Melhorar a performance de consultas

Qual é o uso do comando WITH (NOLOCK) em uma consulta SQL Server?
Resposta: B) Para evitar bloqueios de tabela durante a consulta

Qual é o objetivo do uso de índices compostos no SQL Server?
Resposta: B) Melhorar a performance de consultas que usam várias colunas

Qual é o objetivo do uso de triggers no SQL Server?
Resposta: D) Automatizar tarefas de manutenção de dados

Qual é o uso do comando CHECKPOINT no SQL Server?
Resposta: D) Para forçar a gravação de dados no disco

### Seção 3: Padrão Swagger
Qual é o objetivo principal do Swagger?
Resposta: A) Documentar APIs RESTful

Qual é o padrão de especificação de API mais comumente utilizado em conjunto com o Swagger?
Resposta: A) OpenAPI

Qual é o benefício de utilizar o Swagger First em desenvolvimento de APIs?
Resposta: C) Melhorar a documentação da API

Qual é o recurso do Swagger que permite gerar código mínimo para APIs?
Resposta: A) Swagger Codegen

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


## Contato

Para questões, sugestões ou problemas, entre em contato através de:
- Email: adrianorosadreis@gmail.com
- GitHub: (https://github.com/adrianorosadreis/FreteManager)

---

Desenvolvido como parte de um teste técnico para demonstração de habilidades em desenvolvimento .NET.

# **Direcional  API – Teste Prático**

# **SOBRE O PROJETO**
Este projeto tem como objetivo implementar uma API para gerenciamento de vendas de apartamentos, incluindo controle de clientes, reservas e vendas, conforme cenário proposto no teste técnico.

A aplicação foi estruturada com foco em boas práticas de desenvolvimento, organização e escalabilidade. 

## **Estrutura adotada**
/direcional-api  
│── /backend  
│──│── /Direcional.Api  
│──│──│──/Direcional.Api  
│──│──│──│── /Controllers  
│──│──│──│──│── ApartamentosController.cs  
│──│──│──│──│── AuthController.cs  
│──│──│──│──│── ClientesController.cs  
│──│──│──│──│── ReservasController.cs  
│──│──│──│──│── VendasController.cs  
│──│──│──│── /Data  
│──│──│──│──│── AppDbContext.cs  
│──│──│──│── /DTOs  
│──│──│──│──│── /Apartamentos  
│──│──│──│──│──│── ApartamentoDto.cs  
│──│──│──│──│──│── CreateApartamentoDto.cs  
│──│──│──│──│──│── UpdateApartamentoDto.cs  
│──│──│──│──│── /Clientes  
│──│──│──│──│──│── ClienteDto.cs  
│──│──│──│──│──│── CreateClienteDto.cs  
│──│──│──│──│──│── UpdateClienteDto.cs  
│──│──│──│──│── /Reservas  
│──│──│──│──│──│── CreateReservaDto.cs  
│──│──│──│──│──│── ReservaDto.cs  
│──│──│──│──│── /Vendas  
│──│──│──│──│──│── CreateVendaDto.cs  
│──│──│──│──│──│── VendaDto.cs  
│──│──│──│── /Entities  
│──│──│──│──│── Apartamento.cs  
│──│──│──│──│── Cliente.cs  
│──│──│──│──│── Corretor.cs  
│──│──│──│──│── Reserva.cs  
│──│──│──│──│── Venda.cs  
│──│──│──│── appsettings.json  
│──│──│──│── Direcional.Api.http  
│──│──│──│── Direcional.Api.csproj  
│──│──│──│── Direcional.Api.csproj.user  
│──│──│──│── Dockerfile  
│──│──│──│── Program.cs  
│──│──│── Direcional.Api.slnx  
│── /database  
│── │── init.sql  
│── docker-compose.yml  
│── readme.md  

# **FLUXO PRINCIAL DO SISTEMA**
1. Corretor realiza login;
1. Cliente é cadastrado;
1. Apartamento disponível é consultado;
1. Reserva é criada (Apartamento → Reservado);
1. Venda é realizada (Apartamento → Vendido, Reserva → Fechada).

# **BANCO DE DADOS COM DOCKER**
O ambiente foi configurado para permitir execução local e via container, utilizando diferentes connection strings conforme o ambiente.

## **Subir o Container**
Execute o comando abaixo na pasta raiz do projeto /direcional-api.
```bash
docker compose up --build -d
```

Aparecerá o container.
```bash
sqlserver\_direcional
```

## **Conexão com o banco de dados**
Dados para conexão com o container via SSMS.
```bash
Servidor: localhost,1433 ou sqlserver,1433
Login: sa
Senha: Dev@Direcional#2026
```

Após subir os containers, o banco de dados deve ser inicializado executando o script `/database/init.sql` via SQL Server Management Studio.

## **Criação do Banco e Estrutura**
O banco é criado via script init.sql. Foram inseridos dados iniciais para facilitar os testes da API.

- **Banco de dados criado**: direcional\_db;

## *Modelagem*
- **clientes**: armazena informações do cliente.
  - id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID()
  - nome NVARCHAR(150) NOT NULL
  - email NVARCHAR(150) UNIQUE NOT NULL
  - telefone NVARCHAR(20)
  - criado_em DATETIME2 DEFAULT GETDATE()

- **apartamentos**: armazena informações dos apartamentos.
  - id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID()
  - numero NVARCHAR(10) NOT NULL
  - andar INT NOT NULL
  - valor DECIMAL(18,2) NOT NULL
  - status NVARCHAR(20) NOT NULL
  - criado_em DATETIME2 DEFAULT GETDATE()
- **Status disponíveis:** Disponível, Reservado e Vendido

- **reservas**: armazena informações sobre reservas de apartamentos para os clientes.
  - id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID()
  - cliente_id UNIQUEIDENTIFIER NOT NULL
  - apartamento_id UNIQUEIDENTIFIER NOT NULL
  - data_reserva DATETIME2 DEFAULT GETDATE()
  - status NVARCHAR(20) NOT NULL
- **Status disponíveis:** Ativa, Cancelada, Expirada e Fechada

- **vendas**: armazena informações sobre as vendas de apartamentos para os clientes.
  - id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID()
  - cliente_id UNIQUEIDENTIFIER NOT NULL
  - apartamento_id UNIQUEIDENTIFIER NOT NULL
  - reserva_id UNIQUEIDENTIFIER NULL
  - valor_final DECIMAL(18,2) NOT NULL
  - data_venda DATETIME2 DEFAULT GETDATE()

- **corretores:** armazena as informações dos corretores nos quais utilizam e realizam a autenticação no sistema.
  - id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID()
  - nome NVARCHAR(150) NOT NULL
  - email NVARCHAR(150) NOT NULL UNIQUE
  - senha_hash NVARCHAR(255) NOT NULL
  - ativo BIT NOT NULL DEFAULT 1
  - criado_em DATETIME2 NOT NULL DEFAULT GETDATE()

## *Relacionamentos*
- Um cliente pode ter várias reservas e várias vendas;
- Um apartamento pode ser reservado ou vendido;
- Uma venda pode estar associada a uma reserva;
- Um corretor pode criar várias reservas e realizar várias vendas.

# **AUTENTICAÇÃO COM JWT**
- Para gerar o token, utilize o endpoint de login;
```http
POST /api/auth/login
```

Exemplo da requisição
```json
{
  "email": "admin@direcional.com",
  "password": "123456"
}
```

Exemplo de resposta
```json
{
  "token": "SEU_TOKEN_JWT",
  "expiresInMinutes": 120,
  "corretor": {
    "id": "0418811d-76ac-4d64-89fb-09192e1f5552",
    "nome": "Corretor Admin",
    "email": "admin@direcional.com"
  }
}
```

- Após autenticação, copie o token e envie nas próximas requisições protegidas no header, pois sem o token não conseguirá acessar nenhum endpoint do fluxo do processo (Regra de Negócio);
```http
Authorization: Bearer SEU_TOKEN_JWT
```

Exemplo:
```http
Authorization: Bearer SEU_TOKEN_JWT
```

Authorize via Swagger
```bash
Bearer SEU_TOKEN_JWT
```

## **Comportamento esperado sem token**
Se um endpoint protegido for chamado sem autenticação, a API retornará a mensagem abaixo. Esse comportamento é esperado e garante que apenas Corretores possam acessar as operações sensíveis do sistema.
```http
401 Unauthorized
```

## **Claims incluídas no token**
O token gerado contém as informações do Corretor autenticado.

- nameid: identificador do corretor;
- name: nome do corretor;
- email: email do corretor;
- role: perfil “Corretor”.

Essas claims permitem identificar qual corretor executou as operações como reservas e vendas.

Para visualizar o claims completo do token, cole-o na página <https://jwt.io>.


# **REQUISIÇÕES DA API**

## **Endpoint Clientes**
Este endpoint é responsável pelo cadastro e gerenciamento dos clientes (compradores) atendidos pelos corretores.
Todos os endpoints desta seção são protegidos por autenticação JWT e antes de utilizá-los, é necessário obter um token e enviá-lo no header.
```http
Authorization: Bearer SEU_TOKEN_JWT
```

Authorize via Swagger
```bash
Bearer SEU_TOKEN_JWT
```

### *Criar Cliente*
Cria um novo cliente no sistema.
```http
POST /api/clientes
```

Exemplo de requisição.
```json
{
  "nome": "João da Silva",
  "email": "joao.silva@email.com",
  "telefone": "31999999999"
}
```

Exemplo de resposta
```json
{
  "id": "e7bb5fa2-1d3e-4b88-8f2e-13b69f8b76d1",
  "nome": "João da Silva",
  "email": "joao.silva@email.com"
}
```

- Regras aplicadas:
  - O e-mail do cliente deve ser único;
  - Caso já exista um cliente com o mesmo e-mail, a API retorna erro de validação.
- Possíveis respostas:
  - 201 Created → cliente criado com sucesso;
  - 400 Bad Request → e-mail já cadastrado ou dados inválidos;
  - 401 Unauthorized → token ausente ou inválido.

### *Listar Clientes*
Retorna uma lista de Clientes cadastrados.
```http
GET /api/clientes
```

Exemplo de resposta
```json
[
  {
    "id": "e7bb5fa2-1d3e-4b88-8f2e-13b69f8b76d1",
    "nome": "João da Silva",
    "email": "joao.silva@email.com",
    "telefone": "31999999999"
  },
  {
    "id": "4f289b8c-d86a-4d42-a6d8-4f0be80dce1a",
    "nome": "Maria Oliveira",
    "email": "maria.oliveira@email.com",
    "telefone": "31988888888"
  }
]
```

- Possíveis respostas:
  - 200 OK → consulta realizada com sucesso;
  - 401 Unauthorized → token ausente ou inválido.

### *Buscar Cliente por ID*
Retorna os dados de um cliente específico.

```http
GET /api/clientes/{id}
```

Exemplo:
```http
GET /api/clientes/e7bb5fa2-1d3e-4b88-8f2e-13b69f8b76d1
```

Exemplo de resposta
```bash
{
  "id": "e7bb5fa2-1d3e-4b88-8f2e-13b69f8b76d1",
  "nome": "João da Silva",
  "email": "joao.silva@email.com",
  "telefone": "31999999999"
}
```

- Possíveis respostas:
  - 200 OK → cliente encontrado com sucesso;
  - 404 Not Found → cliente não encontrado;
  - 401 Unauthorized → token ausente ou inválido.

### *Atualizar Cliente*
Atualiza os dados de um cliente cadastrado.
```http
PUT /api/clientes/{id}
```

Exemplo de requisição
```json
{
  "nome": "João da Silva Souza",
  "email": "joao.souza@email.com",
  "telefone": "31997777777"
}
```

- Possíveis respostas:
  - 204 No Content → atualização realizada com sucesso;
  - 404 Not Found → cliente não encontrado;
  - 400 Bad Request → dados inválidos;
  - 401 Unauthorized → token ausente ou inválido.

### *Excluir Cliente*
Remove um cliente do sistema.
```http
DELETE /api/clientes/{id}
```

Exemplo
```http
DELETE /api/clientes/e7bb5fa2-1d3e-4b88-8f2e-13b69f8b76d1
```

- Possíveis respostas:
  - 204 No Content → exclusão realizada com sucesso;
  - 404 Not Found → cliente não encontrado;
  - 401 Unauthorized → token ausente ou inválido.

## **Endpoint Reservas**
Os endpoints de reservas são responsáveis por gerenciar o bloqueio temporário de apartamentos para Clientes, garantindo que não sejam vendidos simultaneamente para mais de um comprador.
Todas as operações de reserva são realizadas por um Corretor autenticado e seguem regras de negócio críticas para o domínio. Todos os endpoints são protegidos por autenticação JWT.

### *Criar Reservas*
Cria uma nova reserva para um apartamento “disponível”.
```http
POST /api/reservas
```

Exemplo de requisição
```json
{
  "clienteId": " 422E6EB2-1EF6-4017-A138-5649D332A5A7",
  "apartamentoId": " A3E955B3-18B3-4370-8465-09219F0BB310"
}
```

Exemplo de resposta
```json
{
  "id": "b9c2e4b4-5b1a-4c2f-9e1c-9a6c8a7d9f12",
  "clienteId": "GUID_DO_CLIENTE",
  "apartamentoId": "GUID_DO_APARTAMENTO",
  "corretorId": "GUID_DO_CORRETOR",
  "status": "Ativa",
  "dataReserva": "2026-03-21T18:30:00Z"
}
```

- Regras aplicadas:
  - O cliente, apartamento (disponível) deve existir;
  - O corretor deve estar autenticado;
  - Ao criar a reserva:
    - O status da reserva será **“Ativa”**;
    - O status do apartamento será alterado para **“Reservado”**;
  - O corretor autenticado é automaticamente associado à reserva.
- Possíveis respostas:
  - 201 Created → reserva criada com sucesso;
  - 400 Bad Request → cliente/apartamento inválido ou indisponível;
  - 401 Unauthorized → token ausente ou inválido.

### *Listar Reservas*
Retorna todas as reservas cadastradas.
```http
GET /api/reservas
```

Exemplo da resposta
```json
[
  {
    "id": "b9c2e4b4-5b1a-4c2f-9e1c-9a6c8a7d9f12",
    "clienteId": "GUID_DO_CLIENTE",
    "clienteNome": "João da Silva",
    "apartamentoId": "GUID_DO_APARTAMENTO",
    "apartamentoNumero": "101",
    "corretorId": "GUID_DO_CORRETOR",
    "corretorNome": "Corretor Admin",
    "status": "Ativa",
    "dataReserva": "2026-03-21T18:30:00Z"
  }
]
```

- Possíveis respostas:
  - 200 OK → consulta com sucesso;
  - 401 Unauthorized → token ausente ou inválido.

### *Buscar Reservas por ID*
Retorna os dados de uma reserva específica.
```http
GET /api/reservas/{id}
```

Exemplo
```http
GET /api/reservas/b9c2e4b4-5b1a-4c2f-9e1c-9a6c8a7d9f12
```

- Possíveis respostas:
  - 200 OK → reserva encontrada;
  - 404 Not Found → reserva não encontrada;
  - 401 Unauthorized → token ausente ou inválido.

### *Cancelar Reservas*
Cancela uma reserva ativa e libera o apartamento.
```http
PUT /api/reservas/{id}/cancelar
```

- Regras aplicadas:
  - Apenas reservas com status Ativa podem ser canceladas;
  - Ao cancelar uma reserva:
    - O status da reserva será alterado para **“Cancelada”**;
    - O status do apartamento será alterado para **“Disponível”**;
- Possíveis respostas:
  - 204 No Content → reserva cancelada com sucesso;
  - 400 Bad Request → reserva não está “ativa”;
  - 404 Not Found → reserva não encontrada;
  - 401 Unauthorized → token ausente ou inválido.

## **Endpoint Vendas**
Os endpoints de vendas são responsáveis por concluir a compra de um apartamento previamente reservado por um cliente.
Todas as operações de vendas são realizadas por um Corretor autenticado e seguem regras de negócio críticas para o domínio. Todos os endpoints são protegidos por autenticação JWT.

### *Criar Venda*
Cria uma nova venda e conclui o processo de compra do apartamento.
```http
POST /api/vendas
```

Exemplo de requisição
```json
{
  "clienteId": "GUID_DO_CLIENTE",
  "apartamentoId": "GUID_DO_APARTAMENTO",
  "reservaId": "GUID_DA_RESERVA",
  "valorFinal": 285000.00
}
```

Exemplo de resposta
```json
{
  "id": "2dd9dcfe-5b91-4e35-a6b2-bfd7f9ee8e7f",
  "clienteId": "GUID_DO_CLIENTE",
  "apartamentoId": "GUID_DO_APARTAMENTO",
  "reservaId": "GUID_DA_RESERVA",
  "corretorId": "GUID_DO_CORRETOR",
  "valorFinal": 285000.00,
  "dataVenda": "2026-03-22T20:15:00Z"
}
```

- Regras aplicadas:
  - O cliente, apartamento (reservado) deve existir;
  - O corretor deve estar autenticado;
  - A venda deve estar associada a uma reserva ativa;
  - A reserva deve corresponder ao mesmo cliente e apartamentos informados;
  - Um apartamento não pode possuir mais de uma venda registrada;
  - Ao concluir a venda:
    - O status do apartamento é alterado para **“Vendido”**;
    - O status da reserva será alterado para **“Fechada”**;
  - O corretor autenticado é automaticamente associado à venda.
- Possíveis respostas:
  - 201 Created → venda criada com sucesso;
  - 400 Bad Request → cliente/apartamento/reserva inválidos ou indisponíveis;
  - 401 Unauthorized → token ausente ou inválido.

### *Listar Vendas*
Retorna todas as vendas registradas.
```http
GET /api/vendas
```

Exemplo de resposta
```json
[
  {
    "id": "2dd9dcfe-5b91-4e35-a6b2-bfd7f9ee8e7f",
    "clienteId": "GUID_DO_CLIENTE",
    "clienteNome": "João da Silva",
    "apartamentoId": "GUID_DO_APARTAMENTO",
    "apartamentoNumero": "101",
    "reservaId": "GUID_DA_RESERVA",
    "corretorId": "GUID_DO_CORRETOR",
    "corretorNome": "Corretor Admin",
    "valorFinal": 285000.00,
    "dataVenda": "2026-03-22T20:15:00Z"
  }
]
```

- Possíveis respostas:
  - 200 OK → venda localizada com sucesso;
  - 401 Unauthorized → token ausente ou inválido.

### *Buscar Venda por ID*
Retorna os dados de uma venda específica.
```http
GET /api/vendas 
```

Exemplo
```http
GET /api/vendas/{id}
```

- Possíveis respostas:
  - 200 OK → venda localizada com sucesso;
  - 404 Not Found → venda não encontrada;
  - 401 Unauthorized → token ausente ou inválido.

## **Endpoint Apartamentos**
Os endpoints de apartamentos são responsáveis pelo cadastro e gerenciamento das unidades disponíveis para reserva e venda no sistema.
Todas as operações de apartamentos são realizadas por um Corretor autenticado e seguem regras de negócio críticas para o domínio. Todos os endpoints são protegidos por autenticação JWT.

### *Criar Apartamentos*
Cria um novo apartamento no sistema.
```http
POST /api/apartamentos
```

Exemplo de requisição
```json
{
  "numero": "301",
  "andar": 3,
  "valor": 350000.00,
  "status": "Disponível"
}
```

Exemplo de resposta
```json
{
  "id": "5a2c5b3d-73b0-4ae6-b11a-18ac3fc1f42a",
  "numero": "301",
  "andar": 3,
  "valor": 350000.00,
  "status": "Disponível"
}
```

- Regras aplicadas:
  - Não é permitido cadastrar mais de um apartamento com o mesmo número e andar;
  - O status deve ser um dos valores permitidos:
    - Disponível;
    - Reservado;
    - Vendido.
- Possíveis respostas:
  - 201 Created → apartamento criado com sucesso;
  - 400 Bad Request → status inválido, dados inválidos ou com duplicidade;
  - 401 Unauthorized → token ausente ou inválido.

### *Listar Apartamentos*
Retorna uma lista de apartamentos criados.
```http
GET /api/apartamentos
```

Exemplo de resposta
```json
[
  {
    "id": "5a2c5b3d-73b0-4ae6-b11a-18ac3fc1f42a",
    "numero": "301",
    "andar": 3,
    "valor": 350000.00,
    "status": "Disponível"
  },
  {
    "id": "c8d4a4be-0f4f-42eb-93cf-bfa3b4c27e47",
    "numero": "302",
    "andar": 3,
    "valor": 360000.00,
    "status": "Reservado"
  }
]
```

- Possíveis respostas:
  - 200 OK → consulta realizada com sucesso;
  - 400 Bad Request → status inválido, dados inválidos ou com duplicidade;
  - 401 Unauthorized → token ausente ou inválido.

### *Buscar Apartamentos por ID*
Retorna os dados de um apartamento específico.
```http
GET /api/apartamentos/{id}
```

Exemplo
```http
GET /api/apartamentos/5a2c5b3d-73b0-4ae6-b11a-18ac3fc1f42a
```

Exemplo de resposta
```json
{
  "id": "5a2c5b3d-73b0-4ae6-b11a-18ac3fc1f42a",
  "numero": "301",
  "andar": 3,
  "valor": 350000.00,
  "status": "Disponível"
}
```

- Possíveis respostas:
  - 200 OK → apartamento encontrado com sucesso;
  - 404 Not Found → apartamento não encontrado;
  - 401 Unauthorized → token ausente ou inválido.

### *Atualizar Apartamentos*
Atualiza os dados de um apartamento já cadastrado.
```http
PUT /api/apartamentos/{id}
```

Exemplo da requisição
```json
{
  "numero": "301A",
  "andar": 3,
  "valor": 355000.00,
  "status": "Disponível"
}
```

- Regras aplicadas:
  - Não é permitido atualizar um apartamento para um número e andar que já existam;
  - O status deve ser um dos valores permitidos:
    - Disponível;
    - Reservado;
    - Vendido.
- Possíveis respostas:
  - 204 No Content → atualização realizada com sucesso;
  - 400 Bad Request → status inválido, dados inválidos ou com duplicidade;
  - 404 Not Found → apartamento não encontrado;
  - 401 Unauthorized → token ausente ou inválido.

### *Excluir Apartamentos*
Remove um apartamento do sistema.
```http
DELETE /api/apartamentos/{id}
```

Exemplo:
```http
DELETE /api/apartamentos/5a2c5b3d-73b0-4ae6-b11a-18ac3fc1f42a
```

- Regras aplicadas:
  - Não é permitido apartamentos com status **Vendido**;
- Possíveis respostas:
  - 204 No Content → exclusão realizada com sucesso;
  - 400 Bad Request → apartamento vendido não pode ser excluído;
  - 404 Not Found → apartamento não encontrado;
  - 401 Unauthorized → token ausente ou inválido.

# **REGRAS DE NEGÓCIO**
- Um apartamento não pode ser reservado se estiver: **Reservado** ou **Vendido**;
- Um apartamento só pode ser reservado se estiver: **Disponível**;
- Ao criar uma reserva: o status do apartamento muda para **Reservado**;
- Ao finalizar uma venda: o status do apartamento muda para **Vendido**;
- Uma reserva deve estar associada a um **Cliente** existente;
- Ao realizar uma reserva a mesma fica com status **Ativo**;
- Apenas reservas com status **Ativo** podem ser canceladas;
- Ao cancelar uma reserva, a mesma fica **Cancelada** e o apartamento retorna para o status **Disponível**;
- Ao reservar um apartamento, o mesmo fica com o status **Reservado**;
- Uma venda pode estar vinculada a uma reserva existente;
- A autenticação é feita via JWT (JSON Web Token);
- Apenas corretores autenticados podem acessar os endpoints (API);
- O token contém: Id do corretor, Nome, Email e Role (Corretor);
- O cliente é cadastrado por um corretor autenticado;
- O cliente não possui login no sistema;
- O email é tratado como identificador único do Cliente;
- Ao reservar é registrado o corretor responsável pela mesma;
- Os endpoints de Clientes só podem ser acessados com token JWT válido;
- Não é permitido cadastrar mais de um apartamento com o mesmo número e andar;
- O status do apartamento foi mantido como campo controlado por validação de negócio e por CONSTRAINT no banco;
- A exclusão de apartamentos vendidos foi bloqueada para preservar a integridade histórica das transações;
- O uso de DTOs evita expor diretamente a entidade da camada de persistência;

# **DECISÕES TÉCNICAS**
- Uso de UNIQUEIDENTIFIER para garantir IDs únicos e distribuídos;
- Uso de CHECK CONSTRAINT para garantir integridade dos status;
- Separação entre reservas e vendas para refletir o domínio real;
- Uso de Docker para garantir portabilidade e isolamento do ambiente;
- Script SQL idempotente para evitar erros em múltiplas execuções;
- Utilizado uma nova tabela para os usuários que devem ser autenticados no sistema, exceto clientes;
- A separação entre **Cliente** e **Corretor** foi mantida para refletir corretamente o cenário de negócio descrito no teste;
- Estrutura preparada para evolução (roles, permissões, etc.);
- No JWT foi realizada uma abordagem stateless, simples de escalar e adequada para APIs REST. O uso do identificador do corretor dentro do token permite rastrear quem realizou cada operação de negócio no sistema;
- Foi adotado uso de DTOs para entrada e saída de dados, evitando expor as entidades do domínio;
- As senhas dos corretores são armazenadas utilizando BCrypt, garantindo segurança no armazenamento e validação das credenciais.

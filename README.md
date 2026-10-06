# Biblioteca API

API RESTful em **C# .NET 10** com **Entity Framework Core** para gerenciamento de autores e livros de uma biblioteca.

> Projeto da disciplina **C# Software Development**, CP5 (API RESTful com EF Core .NET 10).

## Integrantes

| Nome | RM |
|------|----|
| _Nome 1_ | _RM_ |
| _Nome 2_ | _RM_ |
| _Nome 3_ | _RM_ |

## Contexto do projeto

**O que é:** uma API para cadastro e consulta de autores e dos livros que escreveram.

**Problema que resolve:** bibliotecas pequenas (escolares, comunitárias, de bairro) costumam controlar o acervo em planilhas, o que gera registros duplicados, livros sem autor definido e dificuldade para buscar. A API centraliza o acervo, garante integridade (um livro sempre pertence a um autor existente e o ISBN é único) e expõe os dados para qualquer front-end ou aplicativo consumir.

**Para quem é:** bibliotecários e desenvolvedores que precisam de um back-end simples e confiável para gerir o acervo.

## Tecnologias

- C# / .NET 10 (ASP.NET Core Web API com Controllers)
- Entity Framework Core 10 (Code First + Migrations)
- **Banco de dados: SQLite** (arquivo `biblioteca.db`, criado automaticamente)
- Asp.Versioning (versionamento por URL: `/api/v1/...`)
- OpenAPI + Scalar (documentação interativa)

## Como rodar localmente

**Pré-requisito:** [.NET 10 SDK](https://dotnet.microsoft.com/download) instalado.

```bash
# 1. Clonar o repositório
git clone <URL-DO-REPOSITORIO>
cd BibliotecaApi

# 2. Restaurar dependências
dotnet restore

# 3. Executar (aplica as migrations e cria o banco automaticamente)
dotnet run
```

A API sobe em **http://localhost:5000** e a documentação interativa (para testar os endpoints) fica em **http://localhost:5000/scalar/v1**.

Também é possível testar com o arquivo [`requests.http`](requests.http) (VS Code com a extensão REST Client, ou Visual Studio / Rider).

## Migrations

A migration inicial fica na pasta `Migrations/`. Para recriar ou adicionar novas:

```bash
dotnet tool install --global dotnet-ef      # apenas na primeira vez
dotnet ef migrations add <NomeDaMigration>  # cria uma nova migration
dotnet ef database update                   # aplica no banco (também é aplicada ao iniciar a API)
```

**Migration `InitialCreate`:** cria as tabelas `Autores` e `Livros`, a chave estrangeira `Livros.AutorId → Autores.Id` (sem exclusão em cascata) e o índice único em `Livros.Isbn`.

## Endpoints (versão 1)

### Autores

| Método | Rota | Descrição | Status |
|--------|------|-----------|--------|
| GET | `/api/v1/autores` | Lista todos os autores | 200 |
| GET | `/api/v1/autores/{id}` | Busca um autor pelo id | 200, 404 |
| GET | `/api/v1/autores/{id}/livros` | Lista os livros de um autor | 200, 404 |
| POST | `/api/v1/autores` | Cria um autor | 201, 400 |
| PUT | `/api/v1/autores/{id}` | Atualiza um autor | 200, 400, 404 |
| DELETE | `/api/v1/autores/{id}` | Remove um autor (se não tiver livros) | 204, 404, 409 |

### Livros

| Método | Rota | Descrição | Status |
|--------|------|-----------|--------|
| GET | `/api/v1/livros` | Lista livros (filtros opcionais: `?autorId=` e `?titulo=`, busca parcial sem diferenciar maiúsculas) | 200 |
| GET | `/api/v1/livros/{id}` | Busca um livro pelo id | 200, 404 |
| POST | `/api/v1/livros` | Cadastra um livro | 201, 400, 409 |
| PUT | `/api/v1/livros/{id}` | Atualiza um livro | 200, 400, 404, 409 |
| DELETE | `/api/v1/livros/{id}` | Remove um livro | 204, 404 |

### Exemplos de corpo

**Autor**
```json
{ "nome": "Machado de Assis", "nacionalidade": "Brasileira" }
```

**Livro**
```json
{ "titulo": "Dom Casmurro", "isbn": "9788535911664", "anoPublicacao": 1899, "autorId": 1 }
```

### Tratamento de erros

Os erros seguem o padrão `ProblemDetails` (RFC 9457): `400` para validação, `404` para recurso inexistente, `409` para conflito (ISBN duplicado ou autor com livros) e `500` para exceções não tratadas.

## Evidências de teste

Os prints dos testes de cada endpoint estão em [`docs/evidencias/`](docs/evidencias/).

## Estrutura do projeto

```
BibliotecaApi/
├── Controllers/   # Endpoints (AutoresController, LivrosController)
├── Data/          # AppDbContext (EF Core)
├── Dtos/          # Objetos de entrada/saída e validações
├── Migrations/    # Migrations do EF Core
├── Models/        # Entidades (Autor, Livro)
├── docs/evidencias/
├── Program.cs
└── requests.http
```

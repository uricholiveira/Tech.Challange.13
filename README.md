
# Tech Challenge 13

Projeto desenvolvido em ASP.NET Core (.NET 9.0) seguindo princípios de Clean Architecture e boas práticas de desenvolvimento.

## 📋 Sumário

- [Arquitetura](#-arquitetura)
- [Tecnologias](#-tecnologias)
- [Pré-requisitos](#-pré-requisitos)
- [Como Executar](#-como-executar)
- [Estrutura do Projeto](#-estrutura-do-projeto)

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture** com **Vertical Slicing**, separando as responsabilidades em camadas bem definidas:

### Camadas da Aplicação
- **API:**: Camada responsável por expor as APIs REST.
- **Infraestrutura**: Camada responsável por interagir com recursos externos, como bancos de dados, serviços externos, etc.
- **Aplicação**: Camada que contém a lógica de negócios e regras de negócio.
- **Domínio**: Camada que define os conceitos, entidades do sistema, e eventos de domínio.


### Princípios Aplicados
- **Separation of Concerns**: Cada camada possui uma responsabilidade específica
- **Dependency Inversion**: As dependências apontam para abstrações, não implementações
- **Single Responsibility**: Cada classe possui uma única razão para mudar
- **Clean Code**: Código limpo, legível e manutenível

### Fluxo de Dependências
API → Business → Domain ↓ ↓ Infrastructure

- **Domain**: Núcleo da aplicação, contém as regras de negócio e entidades. Não depende de nenhuma outra camada.
- **Business**: Contém os casos de uso e lógica de aplicação. Depende apenas do Domain.
- **Infrastructure**: Implementa interfaces definidas no Domain (repositórios, acesso a dados, serviços externos).
- **API**: Camada de apresentação que expõe endpoints REST. Coordena as requisições entre as outras camadas.

## 🚀 Tecnologias

- **.NET 9.0**: Framework principal
- **ASP.NET Core**: Para construção da API REST
- **C# 13.0**: Linguagem de programação
- **Docker & Docker Compose**: Containerização e orquestração
- **Razor**: Template engine

## 📦 Pré-requisitos

Antes de executar o projeto, certifique-se de ter instalado:

- [Docker](https://www.docker.com/get-started) (versão 20.10 ou superior)
- [Docker Compose](https://docs.docker.com/compose/install/) (versão 2.0 ou superior)

## 🔧 Como Executar

### Usando Docker Compose (Recomendado)

1. Clone o repositório:
````shell 
git clone https://github.com/uricholiveira/Tech.Challange.13.git ; cd Tech.Challange.13
````
2. Execute o Docker Compose:
````shell 
docker-compose up -d
````
3. Verifique se os containers estão rodando:
````shell
docker-compose ps 
````
4. Acesse a aplicação:
- API: `http://localhost:5000`
- SCALAR: `http://localhost:5000/scalar`


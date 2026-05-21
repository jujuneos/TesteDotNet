# TesteDotNet

API desenvolvida em .NET Core para gerenciar uma tabela de usuários.
Foi utilizado banco de dados SQL Server containerizado em docker.
Para subir o banco basta abrir o terminal e executar ´docker compose up -d´.

### Application

A camada de aplicação contém os commands (Criação, Edição e Remoção) e queries (Obter todos, Obter por ID) dos usuários, além de uma classe Behavior para validar dados de entrada do usuário.

### Domain

A camada de domínio contém a entidade Usuário e a interface de seu repositório.

### Infrastructure

Na camada de infrastrutra estão as migrations, o arquivo de contexto e a implementação do repositório.

### TesteDotNet

A camada de API contém o Controller e a classe Program.

### Tests

Por fim, a camada de testes contém os testes unitários para todos os UseCases criados. Os testes utilizam as bibliotecas xUnit e NSubstitute.

### Observação

A branch principal está apenas com esse arquivo readme. O código está na branch master.

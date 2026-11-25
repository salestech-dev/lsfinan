LSFINAN - Sistema de organização financeira (entrada e saida de gastos)

O sistema utiliza as seguintes tecnologias e versões
- C#
- DOTNET 9
- Identity 


O SISTEMA PERMITE QUE: 
USUÁRIO
 │
 ├── Dashboard
 │
 ├── Entradas
 │    ├── Cadastrar
 │    ├── Listar
 │    ├── Editar
 │    ├── Excluir
 │    └── Marcar como recebida
 │
 ├── Saídas
 │    ├── Cadastrar
 │    ├── Listar
 │    ├── Editar
 │    ├── Excluir
 │    └── Marcar como paga
 │
 ├── Categorias
 │    ├── Criar
 │    ├── Editar
 │    └── Excluir
 │
 ├── Contas
 │    ├── Criar
 │    ├── Editar (inclui ativar/inativar; contas não são excluídas)
 │    └── Consultar saldo
 │
 ├── Contas a pagar
 │    ├── Cadastrar (nome, valor, vencimento, categoria)
 │    ├── Listar (atrasadas, vencem hoje, próximos 7 dias, mais adiante, pagas)
 │    ├── Editar / Excluir (só enquanto não paga)
 │    ├── Pagar (com uma conta ou dividindo entre várias)
 │    └── Desfazer pagamento
 │
 ├── Minha conta (perfil)
 │    ├── Alterar nome
 │    └── Alterar senha
 │ │FUTURO
 └── Relatórios
      ├── Mensal
      ├── Por categoria
      └── Entradas x Saídas

=====================================================================
CONTAS A PAGAR, EDIÇÃO E REGRAS DE NEGÓCIO
=====================================================================

BANCO DE DADOS (rodar uma vez):
      dotnet ef database update

   A migration "AdicionarContasPagar" cria a tabela ContasPagar, adiciona Saidas.ContaPagarId
   e REATIVA todas as contas existentes (a migration anterior criou Contas.Ativa com padrão
   false, então contas antigas estavam como inativas).
   Se der conflito de snapshot, apague os 3 arquivos "20261124120000_AdicionarContasPagar*"
   e gere de novo:  dotnet ef migrations add AdicionarContasPagar

REGRAS DE NEGÓCIO
   - Saldo da conta = saldo inicial + entradas - saídas. Nenhuma conta fica negativa.
   - Saída sem saldo suficiente: o sistema abre um painel para escolher OUTRA conta ou DIVIDIR o
     valor entre duas ou mais contas (gera uma saída por conta). Só contas ativas entram.
   - Valores precisam ser maiores que zero (antes, valor negativo aumentava o saldo).
   - Conta ou categoria INATIVA não aceita novas movimentações (o histórico continua).
   - Conta/categoria de outro usuário nunca é aceita.
   - Excluir entrada, ou reduzir o valor dela, é bloqueado se a conta ficaria negativa.
   - Categoria com movimentações ou contas a pagar não pode ser excluída (inative-a).
   - Pagar uma conta a pagar gera as saídas e marca como paga na MESMA gravação.
   - "Desfazer pagamento" apaga as saídas geradas e a conta volta a ficar pendente.
   - Saídas geradas por conta a pagar só são desfeitas pela própria conta a pagar.

ALERTAS DE VENCIMENTO (RegrasFinanceiras.JanelaAlertaDias = 7)
   - Aviso (janela) logo depois do login: atrasadas, vencem hoje e próximos 7 dias.
   - Contador no menu "Contas a pagar" (vermelho se há atraso, laranja se só vencendo).
   - Faixa de aviso e card "Contas a pagar" no Dashboard.

ARQUITETURA
   - Services/RegrasFinanceiras.cs: regras puras (saldo disponível, divisão entre contas,
     status de vencimento). Fácil de testar.
   - Exceptions/: RegraDeNegocioException (mensagem amigável) e SaldoInsuficienteException.
   - Infrastructure/DecimalModelBinderProvider.cs: aceita "100.50" e "100,50".
   - Program.cs: cultura pt-BR fixa e LoginPath /Usuario/Login (antes ia para /Account/Login).

CORREÇÕES DE BUGS ENCONTRADOS
   - Filtro de mês e paginação de "Todas as saídas" não funcionavam (a view mandava mes/ano e o
     controller lia mesAno).
   - "Minha conta" (menu) apontava para uma página que não existia.
   - Entrada não gravava o nome; formulário de entrada perdia as listas ao dar erro.
   - Descrição da saída era obrigatória sem avisar (string não anulável).
   - Dashboard: "Últimas movimentações" nunca era preenchida.
   - Categoria repetida dava erro 500.

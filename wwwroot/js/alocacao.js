// Painel "de onde sai o dinheiro": escolher uma conta ou dividir entre várias.
(function () {
    var moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

    function centavos(v) { return Math.round((parseFloat(v) || 0) * 100); }

    function iniciar(raiz) {
        var total = centavos(raiz.dataset.valor);
        var linhas = Array.prototype.slice.call(raiz.querySelectorAll('.divisao-conta'));
        var botao = raiz.dataset.confirmar ? document.getElementById(raiz.dataset.confirmar) : null;
        var elDistribuido = raiz.querySelector('[data-role=distribuido]');
        var elFalta = raiz.querySelector('[data-role=falta]');

        function dados(linha) {
            var input = linha.querySelector('.divisao-input');
            return { linha: linha, input: input, saldo: centavos(linha.dataset.saldo), valor: centavos(input.value) };
        }

        function atualizar() {
            var soma = 0, estourou = false;
            linhas.forEach(function (l) {
                var d = dados(l);
                soma += d.valor;
                var passou = d.valor > d.saldo;
                l.classList.toggle('divisao-estourou', passou);
                if (passou) estourou = true;
            });
            var falta = total - soma;
            elDistribuido.textContent = moeda.format(soma / 100);
            elFalta.textContent = moeda.format(Math.max(0, falta) / 100) + (falta < 0 ? ' (passou ' + moeda.format(-falta / 100) + ')' : '');
            elFalta.className = falta === 0 ? 'divisao-ok' : 'divisao-falta';
            if (botao) botao.disabled = !(falta === 0 && !estourou && soma > 0);
        }

        function definir(d, centavosValor) {
            d.input.value = (centavosValor / 100).toFixed(2);
        }

        raiz.addEventListener('input', function (e) {
            if (e.target.classList.contains('divisao-input')) atualizar();
        });

        raiz.addEventListener('click', function (e) {
            var acao = e.target.getAttribute('data-role');
            if (!acao) return;

            if (acao === 'tudo') {
                var d = dados(e.target.closest('.divisao-conta'));
                var outros = 0;
                linhas.forEach(function (l) { if (l !== d.linha) outros += dados(l).valor; });
                var faltando = Math.max(0, total - outros);
                definir(d, Math.min(d.saldo, faltando));
            }

            if (acao === 'limpar') {
                linhas.forEach(function (l) { definir(dados(l), 0); });
            }

            if (acao === 'sugerir') {
                var lista = linhas.map(dados).filter(function (d) { return d.saldo > 0; });
                linhas.forEach(function (l) { definir(dados(l), 0); });
                var unica = lista.filter(function (d) { return d.saldo >= total; })
                                 .sort(function (a, b) { return b.saldo - a.saldo; })[0];
                if (unica) {
                    definir(unica, total);
                } else {
                    var resta = total;
                    lista.sort(function (a, b) { return b.saldo - a.saldo; }).forEach(function (d) {
                        if (resta <= 0) return;
                        var usar = Math.min(resta, d.saldo);
                        definir(d, usar);
                        resta -= usar;
                    });
                }
            }
            atualizar();
        });

        atualizar();
    }

    document.addEventListener('DOMContentLoaded', function () {
        Array.prototype.forEach.call(document.querySelectorAll('.divisao'), iniciar);
    });
})();

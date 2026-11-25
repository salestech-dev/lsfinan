using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace lsfinan.Infrastructure
{
    /// <summary>
    /// Aceita valores em dinheiro nos dois jeitos: "1234.56" (campo number do
    /// navegador) e "1.234,56" (digitado no padrão brasileiro). Sem isso, o
    /// resultado dependeria da cultura do servidor e poderia gravar 100,50 como 10050.
    /// </summary>
    public class DecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            var tipo = context.Metadata.ModelType;

            if (tipo == typeof(decimal) || tipo == typeof(decimal?))
            {
                return new DecimalModelBinder(tipo == typeof(decimal?));
            }

            return null;
        }
    }

    public class DecimalModelBinder : IModelBinder
    {
        private readonly bool _aceitaNulo;

        public DecimalModelBinder(bool aceitaNulo)
        {
            _aceitaNulo = aceitaNulo;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var resultado = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (resultado == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, resultado);

            var texto = resultado.FirstValue;

            if (string.IsNullOrWhiteSpace(texto))
            {
                if (_aceitaNulo)
                {
                    bindingContext.Result = ModelBindingResult.Success(null);
                }
                else
                {
                    bindingContext.ModelState.TryAddModelError(
                        bindingContext.ModelName, "Informe um valor válido.");
                }

                return Task.CompletedTask;
            }

            if (TentarConverter(texto, out var valor))
            {
                bindingContext.Result = ModelBindingResult.Success(valor);
            }
            else
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName, "Informe um valor válido.");
            }

            return Task.CompletedTask;
        }

        public static bool TentarConverter(string texto, out decimal valor)
        {
            var limpo = texto.Trim()
                .Replace("R$", string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace(" ", string.Empty);

            var ultimaVirgula = limpo.LastIndexOf(',');
            var ultimoPonto = limpo.LastIndexOf('.');

            if (ultimaVirgula >= 0 && ultimoPonto >= 0)
            {
                // o separador que vem por último é o decimal; o outro é de milhar
                limpo = ultimaVirgula > ultimoPonto
                    ? limpo.Replace(".", string.Empty).Replace(',', '.')
                    : limpo.Replace(",", string.Empty);
            }
            else if (ultimaVirgula >= 0)
            {
                limpo = limpo.Replace(',', '.');
            }

            return decimal.TryParse(
                limpo,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out valor);
        }
    }
}

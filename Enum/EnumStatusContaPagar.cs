namespace lsfinan.Enum
{
    public enum EnumStatusContaPagar
    {
        Pendente = 1,     // vence depois da janela de alerta
        VenceEmBreve = 2, // vence nos próximos dias
        VenceHoje = 3,
        Atrasada = 4,
        Paga = 5
    }
}

using System;

namespace SimuladorSO
{
    public static class Logger
    {
        // Evento que permite que qualquer parte do código "escute" os logs
        public static event Action<string> OnLog;

        private static int _relogio = 0;
        public static int Relogio => _relogio;

        // Avança o relógio (tick)
        public static void Tick() => _relogio++;

        // Log genérico com timestamp do simulador
        public static void Log(string mensagem)
        {
            string timeStamp = $"[t={_relogio}]";
            string logFormatado = $"{timeStamp} {mensagem}";

            if (OnLog != null)
            {
                OnLog.Invoke(logFormatado);
            }
            else
            {
                Console.WriteLine(logFormatado);
            }
        }

        public static void Info(string mensagem) => Log(mensagem);

        public static void Erro(string mensagem)
        {
            string logFormatado = $"[ERRO] {mensagem}";
            var previous = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(logFormatado);
            Console.ForegroundColor = previous;
        }
    }
}
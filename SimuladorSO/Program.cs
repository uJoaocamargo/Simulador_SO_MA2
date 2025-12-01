// Program.cs - Ponto de entrada do simulador de sistema operacional
// Inicializa parâmetros, escalonador, memória, dispositivos e executa a simulação.

using System;

namespace SimuladorSO
{
    public static class Program
    {
        // Método auxiliar para mostrar uso
        private static void ShowUsage()
        {
            Console.WriteLine("Uso: SimuladorSO [--escalonador=FCFS|RR] [--quantum=N] [--seed=N] [--processos=N] [--molduras=N]");
            Console.WriteLine("Opções:");
            Console.WriteLine("  --escalonador   Escolhe o escalonador: FCFS (padrão) ou RR");
            Console.WriteLine("  --quantum       Quantum para RR (padrão: 4)");
            Console.WriteLine("  --seed          Semente para geração aleatória (0 = usar semente atual do relógio)");
            Console.WriteLine("  --processos     Número de processos iniciais (padrão: 5)");
            Console.WriteLine("  --molduras      Número de molduras de memória disponíveis (padrão: 3)");
            Console.WriteLine("  -h, --help      Mostra esta ajuda");
        }

        // Método principal: inicializa e executa a simulação
        public static void Main(string[] args)
        {
            // parâmetros padrão
            string escalonador = "FCFS"; // ou "RR"
            int quantum = 4;
            int seed = 42;
            int processosIniciais = 5;
            int memoriaMolduras = 3;

            try
            {
                // parse simples de args com validações
                foreach (var a in args)
                {
                    if (string.IsNullOrWhiteSpace(a))
                        continue;

                    if (a == "-h" || a == "--help")
                    {
                        ShowUsage();
                        return;
                    }

                    var parts = a.Split('=', 2);
                    var key = parts[0].Trim();
                    var value = parts.Length > 1 ? parts[1].Trim() : string.Empty;

                    switch (key)
                    {
                        case var _ when key.StartsWith("--escalonador", StringComparison.OrdinalIgnoreCase):
                            if (!string.IsNullOrEmpty(value))
                                escalonador = value.ToUpper();
                            break;

                        case var _ when key.StartsWith("--quantum", StringComparison.OrdinalIgnoreCase):
                            if (!int.TryParse(value, out quantum) || quantum <= 0)
                            {
                                Console.WriteLine("Valor inválido para --quantum. Usando padrão 4.");
                                quantum = 4;
                            }
                            break;

                        case var _ when key.StartsWith("--seed", StringComparison.OrdinalIgnoreCase):
                            if (!int.TryParse(value, out seed))
                            {
                                Console.WriteLine("Valor inválido para --seed. Usando padrão 42.");
                                seed = 42;
                            }
                            break;

                        case var _ when key.StartsWith("--processos", StringComparison.OrdinalIgnoreCase):
                            if (!int.TryParse(value, out processosIniciais) || processosIniciais < 0)
                            {
                                Console.WriteLine("Valor inválido para --processos. Usando padrão 5.");
                                processosIniciais = 5;
                            }
                            break;

                        case var _ when key.StartsWith("--molduras", StringComparison.OrdinalIgnoreCase):
                            if (!int.TryParse(value, out memoriaMolduras) || memoriaMolduras <= 0)
                            {
                                Console.WriteLine("Valor inválido para --molduras. Usando padrão 3.");
                                memoriaMolduras = 3;
                            }
                            break;

                        default:
                            // argumento desconhecido
                            Console.WriteLine($"Argumento desconhecido: {a}");
                            break;
                    }
                }

                // Ajustes de valores
                if (seed == 0)
                {
                    seed = Environment.TickCount;
                    Console.WriteLine($"Semente não fornecida (0). Usando semente aleatória: {seed}");
                }

                Console.WriteLine("Simulador de Sistema Operacional");
                Console.WriteLine($"Escalonador: {escalonador} | Quantum: {quantum} | Seed: {seed} | Molduras: {memoriaMolduras}");
                Console.WriteLine();

                Random rnd = new Random(seed);
                IEscalonador esc;
                if (string.Equals(escalonador, "RR", StringComparison.OrdinalIgnoreCase))
                    esc = new EscalonadorRR(quantum);
                else
                    esc = new EscalonadorFCFS();

                var gerMem = new GerenciadorMemoria(memoriaMolduras);
                var disp = new DispositivoIO();
                var so = new SistemaOperacional(esc, gerMem, disp, rnd, quantum);

                // Cria processos iniciais e coloca em pronto
                for (int i = 0; i < processosIniciais; i++)
                {
                    int tempoCpu = rnd.Next(3, 12);
                    int pri = rnd.Next(0, 3);
                    var p = so.CriarProcesso("P" + (i + 1), tempoCpu, pri);
                    so.ColocarEmPronto(p.Id);
                }

                // Executa a simulação principal
                so.ExecutarSimulacao();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro durante a execução da simulação: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Environment.ExitCode = 1;
            }
        }
    }
}

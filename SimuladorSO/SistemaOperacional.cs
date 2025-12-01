// SistemaOperacional.cs - Simulador principal do sistema operacional
// Gerencia processos, escalonador, memória, dispositivos e executa a simulação

using System;
using System.Linq;

namespace SimuladorSO
{
    // Sistema Operacional (simulador)
    public class SistemaOperacional
    {
        private TabelaDeProcessos tabela = new TabelaDeProcessos(); // Tabela de processos
        private IEscalonador escalonador; // Escalonador de processos
        private GerenciadorMemoria memoria; // Gerenciador de memória
        private DispositivoIO dispositivo; // Dispositivo de I/O
        private Metricas metricas = new Metricas(); // Métricas da simulação
        private int proximoId = 1; // Próximo ID de processo
        private Random aleatorio; // Gerador de números aleatórios
        private int quantumRR = 4; // Quantum para RR
        private bool usarRR = false; // Indica se RR está em uso

        // Construtor: inicializa componentes do SO
        public SistemaOperacional(IEscalonador esc, GerenciadorMemoria mem, DispositivoIO disp, Random rnd, int quantum = 4)
        {
            escalonador = esc;
            memoria = mem;
            dispositivo = disp;
            aleatorio = rnd;
            quantumRR = quantum;
            usarRR = esc is EscalonadorRR;
        }

        // Cria processo e tenta alocar memória
        public Processo CriarProcesso(string nome, int tempoCpu, int prioridade = 0)
        {
            var p = new Processo(proximoId++, nome, tempoCpu, prioridade);
            tabela.Adicionar(p);
            Logger.Log($"Criado processo {p.Nome} (id {p.Id}) tempoCPU={p.TempoRestanteCpu} pri={p.Prioridade}");
            bool alocou = memoria.Alocar(p);
            if (!alocou)
                Logger.Log($"Memória insuficiente para processo {p.Id}, ficará sem alocação (simulação).");
            return p;
        }

        // Coloca processo em pronto (fila do escalonador)
        public void ColocarEmPronto(int id)
        {
            var p = tabela.BuscarPorId(id);
            if (p != null && p.Estado != EstadoDoProcesso.Finalizado)
            {
                p.Estado = EstadoDoProcesso.Pronto;
                escalonador.AdicionarPronto(p);
                Logger.Log($"Processo {p.Nome} (id {p.Id}) colocado em PRONTO.");
            }
        }

        // Bloquear por I/O (vai para fila do dispositivo)
        public void BloquearPorEntradaSaida(int id)
        {
            var p = tabela.BuscarPorId(id);
            if (p != null && p.Estado == EstadoDoProcesso.Executando)
            {
                p.Estado = EstadoDoProcesso.Esperando;
                dispositivo.SolicitarIO(p);
                Logger.Log($"Processo {p.Nome} (id {p.Id}) solicitado I/O e ficou ESPERANDO.");
            }
        }

        // Finaliza processo (libera memória)
        public void FinalizarProcesso(int id)
        {
            var p = tabela.BuscarPorId(id);
            if (p != null)
            {
                p.Estado = EstadoDoProcesso.Finalizado;
                p.FinalizacaoTick = Logger.Relogio;
                memoria.Liberar(p);
                Logger.Log($"Processo {p.Nome} (id {p.Id}) FINALIZADO.");
            }
        }

        // Simulação principal: roda até todos processos finalizarem
        public void ExecutarSimulacao()
        {
            while (tabela.Todos().Any(p => p.Estado != EstadoDoProcesso.Finalizado))
            {
                Logger.Tick();

                foreach (var p in tabela.Todos().Where(pp => pp.Estado == EstadoDoProcesso.Pronto))
                    p.TempoEspera++;

                var ioPronto = dispositivo.Tick();
                if (ioPronto != null)
                {
                    ioPronto.Estado = EstadoDoProcesso.Pronto;
                    escalonador.AdicionarPronto(ioPronto);
                    Logger.Log($"Processo {ioPronto.Nome} (id {ioPronto.Id}) retornou de I/O e foi para PRONTO.");
                }

                var processoCorrente = escalonador.ProximoProcesso();

                if (processoCorrente != null)
                {
                    metricas.RegistrarTroca();
                    processoCorrente.Estado = EstadoDoProcesso.Executando;
                    if (processoCorrente.InicioExecucaoTick == -1)
                        processoCorrente.InicioExecucaoTick = Logger.Relogio;
                    Logger.Log($"Escalado para EXECUTAR: {processoCorrente.Nome} (id {processoCorrente.Id})");

                    int ticksExecutados = 0;
                    int maxTicks = usarRR ? quantumRR : 1;

                    while (ticksExecutados < maxTicks && processoCorrente.TempoRestanteCpu > 0)
                    {
                        processoCorrente.ProgramCounter++;
                        processoCorrente.Registradores["R0"] += 1;
                        processoCorrente.TempoRestanteCpu--;
                        ticksExecutados++;
                        metricas.RegistrarCpuTick();
                        Logger.Tick();
                        Logger.Log($"Executando {processoCorrente.Nome}: remaining CPU {processoCorrente.TempoRestanteCpu}");

                        if (aleatorio.NextDouble() < 0.1)
                        {
                            processoCorrente.Estado = EstadoDoProcesso.Esperando;
                            dispositivo.SolicitarIO(processoCorrente);
                            Logger.Log($"Processo {processoCorrente.Nome} pediu I/O durante execução.");
                            break;
                        }
                    }

                    if (processoCorrente.TempoRestanteCpu <= 0)
                        FinalizarProcesso(processoCorrente.Id);
                    else if (processoCorrente.Estado == EstadoDoProcesso.Esperando)
                    {
                        // enfileirado no dispositivo
                    }
                    else
                    {
                        processoCorrente.Estado = EstadoDoProcesso.Pronto;
                        escalonador.AdicionarPronto(processoCorrente);
                        Logger.Log($"Processo {processoCorrente.Nome} (id {processoCorrente.Id}) voltou para PRONTO.");
                    }
                }
                else
                {
                    Logger.Log("CPU ociosa neste tick.");
                }
            }

            Logger.Log("Simulação completa.");
            Console.WriteLine();
            Console.WriteLine("=== Métricas Finais ===");
            Console.WriteLine(metricas.ToString());
            Console.WriteLine(memoria.ToString());
            Console.WriteLine();
            Console.WriteLine("Tempo de cada processo (espera / início / final):");
            foreach (var p in tabela.Todos())
            {
                Console.WriteLine($"Processo {p.Nome} (id {p.Id}) - Espera: {p.TempoEspera} ticks, InícioExec: {p.InicioExecucaoTick}, Final: {p.FinalizacaoTick}");
            }
            Console.WriteLine("=======================");
        }
    }
}

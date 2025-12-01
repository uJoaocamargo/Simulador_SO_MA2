using System.Collections.Generic;

namespace SimuladorSO
{
    public class EscalonadorRR : IEscalonador
    {
        private readonly Queue<Processo> _filaDeProntos = new Queue<Processo>();
        private readonly int _quantum;

        public EscalonadorRR(int quantum)
        {
            _quantum = quantum;
        }

        public string Nome => "RR";

        public void AdicionarPronto(Processo p)
        {
            if (p.Estado != EstadoDoProcesso.Finalizado && !_filaDeProntos.Contains(p))
            {
                p.Estado = EstadoDoProcesso.Pronto;
                _filaDeProntos.Enqueue(p);
            }
        }

        public Processo ProximoProcesso()
        {
            return _filaDeProntos.Count > 0 ? _filaDeProntos.Dequeue() : null;
        }

        public void Tick()
        {
            // Não há estado por tick neste RR básico. Se implementar aging, faça aqui.
        }

        // Expõe o quantum para o sistema (opcional)
        public int Quantum => _quantum;
    }
}
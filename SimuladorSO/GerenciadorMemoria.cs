using System;
using System.Collections.Generic;
using System.Linq;

namespace SimuladorSO
{
    public class GerenciadorMemoria
    {
        private int[] _quadros; // Representa a memória física (Frames)
        private int _tamanhoPagina;

        public GerenciadorMemoria(int totalQuadros, int tamanhoPagina = 1)
        {
            if (totalQuadros <= 0) throw new ArgumentException("Total de quadros deve ser maior que zero.");

            _quadros = new int[totalQuadros];
            _tamanhoPagina = tamanhoPagina;

            // Inicializa com -1 indicando que está livre
            Array.Fill(_quadros, -1);
        }

        // Nome antigo: AlocarMemoria -> adapta para Alocar
        public bool Alocar(Processo processo) => AlocarMemoria(processo);
        public void Liberar(Processo processo) => DesalocarMemoria(processo);

        public bool AlocarMemoria(Processo processo)
        {
            // Lógica simples de alocação contígua ou First-Fit
            // Procura um quadro livre (-1)
            for (int i = 0; i < _quadros.Length; i++)
            {
                if (_quadros[i] == -1) // Encontrou espaço livre
                {
                    _quadros[i] = processo.Id; // Marca com o ID do processo
                    Logger.Info($"Memória: Processo {processo.Id} alocado no quadro {i}.");
                    return true;
                }
            }

            Logger.Erro($"Memória cheia! Não foi possível alocar o Processo {processo.Id}.");
            return false;
        }

        public void DesalocarMemoria(Processo processo)
        {
            bool desalocou = false;
            for (int i = 0; i < _quadros.Length; i++)
            {
                if (_quadros[i] == processo.Id)
                {
                    _quadros[i] = -1; // Libera o quadro
                    desalocou = true;
                }
            }

            if (desalocou)
                Logger.Info($"Memória: Recursos do Processo {processo.Id} liberados.");
        }

        public int ObterQuadrosLivres() => _quadros.Count(q => q == -1);

        public override string ToString()
        {
            return $"Memória: {ObterQuadrosLivres()} quadros livres de {_quadros.Length}.";
        }
    }
}
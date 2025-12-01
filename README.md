```mermaid
graph TD
    subgraph "Kernel do SO"
        SO["Sistema Operacional<br/>(Loop Principal)"]
        MEM["Gerenciador de Memória<br/>(Paginação/Frames)"]
        SCHED["Escalonador<br/>(Interface IEscalonador)"]
        PCB["Tabela de Processos<br/>(PCBs)"]
    end

    subgraph "Hardware Simulado"
        CPU{"CPU"}
        IO["Dispositivo de I/O<br/>(Fila de Espera)"]
    end

    subgraph "Algoritmos"
        FCFS["Escalonador FCFS"]
        RR["Escalonador Round Robin"]
    end

    SO -->|Gerencia| PCB
    SO -->|Aloca| MEM
    SO -->|Consulta| SCHED
    SO -->|Executa| CPU
    
    SCHED -.->|Implementa| FCFS
    SCHED -.->|Implementa| RR
    
    CPU -->|Interrupção de Timer| SCHED
    CPU -->|Solicitação de I/O| IO
    IO -->|Conclusão de I/O| SCHED
```
*********************************************************************************************************************************************************************************************************************

stateDiagram-v2
    [*] --> Novo: Criação
    Novo --> Pronto: Admissão (Memória OK)
    Pronto --> Executando: Dispatcher
    Executando --> Pronto: Interrupção (Quantum)
    Executando --> Terminado: Fim da Execução
    Terminado --> [*]: Desalocação

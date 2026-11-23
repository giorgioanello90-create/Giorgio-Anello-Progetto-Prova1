using UnityEngine;

[RequireComponent(typeof(MovimentoNPC))]
public class CervelloNPC : MonoBehaviour
{
    [Header("Comportamento Iniziale")]
    [SerializeField] private ModalitaNPC modalita = ModalitaNPC.Fermo;
    [Header("Pattugliamento")]
    [SerializeField] private Transform[] puntiPattuglia;
    [Header("Movimento casuale")]
    [SerializeField] private float raggioMovimento = 3f;
    [SerializeField] private float raggioControlloOstacoli = 0.25f;
    [SerializeField] private LayerMask ostacoli;
    [Header("Pause")]
    [SerializeField] private float pausaMinima = 1f;
    [SerializeField] private float pausaMassima = 3f;
    [Header("Inseguimento")]
    [SerializeField] private float velocitaInseguimento = 3f;
    [SerializeField] private float distanzaArresto = 0.5f;
    [Header("Ricerca")]
    [SerializeField] private float durataRicerca = 20f;
    [SerializeField] private float raggioRicerca = 3f;
    [SerializeField] private float tempoUltimaPosizione = 4f;
    [Header("Ritorno")]
    [SerializeField] private float velocitaRitorno = 2.5f;
    [Header("Debug")]
    [SerializeField] private bool mostraTransizione = true;
    
    private MovimentoNPC movimento;
    private RilevamentoNPC rilevamento;
    private Vector2 centroMovimento;
    private Vector2 ultimaPosizioneNota;
    private IStatoNPC statoNormale;
    private IStatoNPC statoInseguimento;
    private IStatoNPC statoAttuale;
    private StatoUltimaPosizione statoUltimaPosizione;
    private StatoRicerca statoRicerca;
    private StatoRitorno statoRitorno;

    private void Awake()
    {
        movimento = GetComponent<MovimentoNPC>();
        rilevamento = GetComponent<RilevamentoNPC>();
        centroMovimento = transform.position;
    }

    private void Start()
    {
        switch (modalita)
        {
            case ModalitaNPC.Fermo:
                statoNormale = new StatoFermo(movimento);
                break;
            case ModalitaNPC.Pattuglia:
                statoNormale = new StatoPattuglia(movimento, puntiPattuglia, pausaMinima, pausaMassima);
                break;
            case ModalitaNPC.Casuale:
                statoNormale = new StatoCasuale(movimento, centroMovimento, raggioMovimento,raggioControlloOstacoli, ostacoli, pausaMinima, pausaMassima);
                break;
            default:
                statoNormale = new StatoFermo(movimento);
                break;
        }
        if(GetComponent<Nemico>() != null && rilevamento != null)
        {
            statoInseguimento = new StatoInseguimento(movimento, rilevamento, velocitaInseguimento, distanzaArresto);
            statoUltimaPosizione = new StatoUltimaPosizione(movimento, tempoUltimaPosizione, raggioControlloOstacoli, ostacoli);
            statoRicerca = new StatoRicerca(movimento, durataRicerca, raggioRicerca, raggioControlloOstacoli, ostacoli, pausaMinima, pausaMassima);
            StatoPattuglia pattuglia = statoNormale as StatoPattuglia;
            statoRitorno = new StatoRitorno(movimento, pattuglia, centroMovimento, velocitaRitorno, raggioControlloOstacoli, ostacoli);

        }
        CambiaStato(statoNormale);
    }

    private void Update()
    {
        if (statoAttuale == null) { return;  }

        if (statoInseguimento != null)
        {
            if (rilevamento == null || !rilevamento.isActiveAndEnabled)
            {
                CambiaStato(statoNormale);
            }
            else
            {
                bool inAllerta = statoAttuale != statoNormale;
                bool vede = rilevamento.VedeBersaglio(inAllerta);
                if (vede)
                {
                    // La memoria del bersaglio cambia solo quando è visibile.
                    ultimaPosizioneNota = rilevamento.PosizioneBersaglio;
                    CambiaStato(statoInseguimento);
                }
                else if(statoAttuale == statoInseguimento)
                {
                    statoUltimaPosizione.ImpostaDestinazione(ultimaPosizioneNota);
                    CambiaStato(statoUltimaPosizione);
                }
            }
        }
        statoAttuale.Aggiorna();
        if(statoAttuale == statoUltimaPosizione && statoUltimaPosizione.Terminato)
        {
            Vector2 centroRicerca = statoUltimaPosizione.Raggiunto ? ultimaPosizioneNota : movimento.Posizione;
            statoRicerca.ImpostaCentro(centroRicerca);
            CambiaStato(statoRicerca);
        }
        else if (statoAttuale == statoRicerca && statoRicerca.Terminato)
        {
            CambiaStato(statoRitorno);
        }
        else if(statoAttuale == statoRitorno && statoRitorno.Terminato)
        {
            CambiaStato(statoNormale);
        }
    }

    private void CambiaStato(IStatoNPC nuovoStato)
    {
        if(nuovoStato == null || statoAttuale == nuovoStato) { return;  }
        if(statoAttuale != null)
        {
            statoAttuale.Esci();
        }
        statoAttuale = nuovoStato;
        // Ogni stato gestisce autonomamente il proprio ingresso.
        statoAttuale.Entra();
        if (mostraTransizione)
        {
            Debug.Log($"{name}: {statoAttuale.GetType().Name}", this);
        }
    }

    private void OnEnable()
    {
        if(statoNormale != null && statoAttuale == null)
        {
            CambiaStato(statoNormale);
        }
    }

    private void OnDisable()
    {
        if(statoAttuale != null)
        {
            statoAttuale.Esci();
            statoAttuale = null;
        }

        if(movimento != null)
        {
            movimento.Ferma();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (modalita != ModalitaNPC.Casuale)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Vector3 centro = Application.isPlaying
            ? new Vector3(
                centroMovimento.x,
                centroMovimento.y,
                transform.position.z
              )
            : transform.position;
        Gizmos.DrawWireSphere(centro, raggioMovimento);
    }
}

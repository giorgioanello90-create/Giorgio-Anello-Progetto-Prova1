using UnityEngine;

public class StatoCasuale : IStatoNPC
{
    private MovimentoNPC movimento;
    private Vector2 centro;
    private Vector2 destinazione;
    private float raggioMovimento;
    private float raggioOstacoli;
    private LayerMask ostacoli;
    private float pausaMinima;
    private float pausaMassima;
    private float tempoPausa;
    private bool destinazioneValida;

    public StatoCasuale(MovimentoNPC movimento, Vector2 centro, float raggioMovimento, float raggioOstacoli, LayerMask ostacoli, float pausaMinima, float pausaMassima)
    {
        this.movimento = movimento;
        this.centro = centro;
        this.raggioMovimento = raggioMovimento;
        this.raggioOstacoli = raggioOstacoli;
        this.ostacoli = ostacoli;
        this.pausaMinima = pausaMinima;
        this.pausaMassima = pausaMassima;
    }

    public void Entra()
    {
        destinazioneValida = false;
        tempoPausa = 0f;
        movimento.Ferma();
    }

    public void Aggiorna()
    {
        if (tempoPausa > 0f)
        {
            tempoPausa -= Time.deltaTime;
            movimento.Ferma();
            return;
        }
        if (!destinazioneValida)
        {
            destinazioneValida = ScegliDestinazione();
            if (!destinazioneValida)
            {
                tempoPausa = 1f;
                movimento.Ferma();
                return;
            }
        }
        if (movimento.Arrivato(destinazione))
        {
            destinazioneValida = false;
            tempoPausa = Random.Range(pausaMinima, pausaMassima);
            movimento.Ferma();
        } 
        else
        {
            movimento.MuoviVerso(destinazione);
        }

    }

    public void Esci()
    {
        movimento.Ferma();
        destinazioneValida = false;
    }

    private bool ScegliDestinazione()
    {
        for (int tentativo = 0; tentativo < 10; tentativo++)
        {
            Vector2 punto = centro + Random.insideUnitCircle * raggioMovimento;
            if (Vector2.Distance(movimento.Posizione, punto) < 0.05f)
                continue;

            // Lo stato sceglie un punto; MovimentoNPC verifica il percorso.
            if (!movimento.PercorsoLibero(punto, raggioOstacoli, ostacoli))
                continue;
            destinazione = punto;
            return true;
        }
        return false;
    }
}

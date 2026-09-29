using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AI; // Necessário se estiver usando NavMesh


public class Perseguidor1 : MonoBehaviour, IPerseguidor1
{
    private NavMeshAgent agent;
    private bool estaTonto = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void FicarTonto(float duracao)
    {
        if (!estaTonto)
        {
            StartCoroutine(RotinaTontura(duracao));
        }
    }

    private IEnumerator RotinaTontura(float duracao)
    {
        estaTonto = true;

        // Para o movimento do perseguidor
        if (agent != null)
        {
            agent.isStopped = true;
        }

        // Aguarda os 2 minutos (120 segundos)
        yield return new WaitForSeconds(duracao);

        // Retoma o movimento normal
        if (agent != null)
        {
            agent.isStopped = false;
        }

        estaTonto = false;
    }
}

using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EmpurraoRefri : MonoBehaviour
{
    [Header("Configuração do Empurrão")]
    public float duracao = 1.5f;
    public float distanciaEmpurrao = 8f;

    private NavMeshAgent agent;
    private bool sendoEmpurrado = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Empurrar(Vector3 direcao, float forca)
    {
        if (sendoEmpurrado)
            return;

        if (direcao.sqrMagnitude < 0.001f)
            return;

        StartCoroutine(
            RotinaEmpurrao(direcao.normalized, forca)
        );
    }

    private IEnumerator RotinaEmpurrao(
        Vector3 direcao,
        float forca)
    {
        sendoEmpurrado = true;

        bool agenteAtivo =
            agent != null && agent.enabled;

        if (agenteAtivo)
        {
            if (!agent.isOnNavMesh)
            {
                sendoEmpurrado = false;
                yield break;
            }

            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        float tempo = 0f;
        float distanciaPercorrida = 0f;

        while (tempo < duracao &&
               distanciaPercorrida < distanciaEmpurrao)
        {
            float movimento =
                forca * Time.deltaTime;

            Vector3 deslocamento =
                direcao * movimento;

            transform.position += deslocamento;

            distanciaPercorrida += movimento;
            tempo += Time.deltaTime;

            yield return null;
        }

        if (agenteAtivo && agent != null)
        {
            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                transform.position,
                out hit,
                5f,
                NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }

            agent.enabled = true;

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }
        }

        sendoEmpurrado = false;
    }

    public bool EstaSendoEmpurrado()
    {
        return sendoEmpurrado;
    }
}

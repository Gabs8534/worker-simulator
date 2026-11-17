
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EmpurraoRefri : MonoBehaviour
{
    [Header("Configuração do Empurrão")]
    public float duracao = 1.5f;
    public float distanciaEmpurrao = 8f;

    [Header("Força")]
    public float multiplicadorForca = 1f;

    private NavMeshAgent agent;

    private bool sendoEmpurrado = false;
    private Coroutine rotinaAtual;

    public bool EstaSendoEmpurrado()
    {
        return sendoEmpurrado;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError(
                "EmpurraoRefri precisa estar no objeto com NavMeshAgent: " +
                gameObject.name
            );
        }
    }

    public void Empurrar(Vector3 direcao, float forca)
    {
        Debug.Log("TENTANDO EMPURRAR: " + gameObject.name);

        if (sendoEmpurrado)
            return;

        if (agent == null || !agent.enabled)
        {
            Debug.LogWarning("NavMeshAgent não está ativo!");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Inimigo está fora do NavMesh!");
            return;
        }

        direcao.y = 0f;

        if (direcao.sqrMagnitude < 0.001f || forca <= 0f)
            return;

        rotinaAtual = StartCoroutine(
            RotinaEmpurrao(direcao.normalized, forca)
        );
    }

    private IEnumerator RotinaEmpurrao(
        Vector3 direcao,
        float forca)
    {
        sendoEmpurrado = true;

        bool estadoAnterior = agent.isStopped;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        float tempo = 0f;
        float distanciaPercorrida = 0f;

        while (
            tempo < duracao &&
            distanciaPercorrida < distanciaEmpurrao)
        {
            if (agent == null ||
                !agent.enabled ||
                !agent.isOnNavMesh)
                break;

            float movimento = Mathf.Min(
                forca * multiplicadorForca * Time.deltaTime,
                distanciaEmpurrao - distanciaPercorrida
            );

            Vector3 antes = transform.position;

            agent.Move(direcao * movimento);

            float deslocamento = Vector3.Distance(
                antes,
                transform.position
            );

            distanciaPercorrida += deslocamento;
            tempo += Time.deltaTime;

            yield return null;
        }

        if (agent != null &&
            agent.enabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = estadoAnterior;
        }

        sendoEmpurrado = false;
        rotinaAtual = null;

        Debug.Log("EMPURRÃO FINALIZADO: " + gameObject.name);
    }

    private void OnDisable()
    {
        if (rotinaAtual != null)
        {
            StopCoroutine(rotinaAtual);
            rotinaAtual = null;
        }

        sendoEmpurrado = false;
    }
}

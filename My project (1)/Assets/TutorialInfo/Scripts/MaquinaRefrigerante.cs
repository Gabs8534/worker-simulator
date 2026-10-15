using UnityEngine;
using System.Collections;

public class MaquinaRefrigerante : MonoBehaviour
{
    [Header("Preço da máquina")] public int preco = 1;

    [Header("Refrigerante")] public GameObject prefabRefri; public Transform saidaRefri;

    [Header("Interação")] public float distanciaInteracao = 3f; public float tempoParaLiberar = 1f;

    private Camera cameraPrincipal; private PlayerMovemen player; private bool processando = false;

    private void Start() { cameraPrincipal = Camera.main; player = FindFirstObjectByType<PlayerMovemen>(); }

    private void Update()
    {
        if (processando) return;

        if (Input.GetMouseButtonDown(1)) { TentarComprar(); }
    }

    private void TentarComprar()
    {
        if (cameraPrincipal == null) cameraPrincipal = Camera.main;

        if (player == null) player = FindFirstObjectByType<PlayerMovemen>();

        if (cameraPrincipal == null || player == null) return;

        Ray raio = new Ray(cameraPrincipal.transform.position, cameraPrincipal.transform.forward);

        RaycastHit hit;

        if (!Physics.Raycast(raio, out hit, distanciaInteracao)) return;

        MaquinaRefrigerante maquinaEncontrada = hit.collider.GetComponentInParent<MaquinaRefrigerante>();

        if (maquinaEncontrada != this) return;

        GameObject objetoSegurado = player.itemAtualSegurado;

        if (objetoSegurado == null) { Debug.Log("Pegue uma moeda primeiro!"); return; }

        Moeda moeda = objetoSegurado.GetComponent<Moeda>();

        if (moeda == null || !moeda.EstaSendoSegurada()) { Debug.Log("Você precisa estar segurando uma moeda!"); return; }

        if (moeda.valor < preco) { Debug.Log("A moeda não tem valor suficiente!"); return; }

        if (prefabRefri == null) { Debug.LogError("Arraste o prefab do refrigerante para o Inspector!"); return; }

        StartCoroutine(EntregarRefri(objetoSegurado));
    }

    private IEnumerator EntregarRefri(GameObject moedaObjeto)
    {
        processando = true;

        if (player != null && player.itemAtualSegurado == moedaObjeto) { player.itemAtualSegurado = null; }

        Destroy(moedaObjeto);

        Debug.Log("Moeda inserida na máquina!");

        yield return new WaitForSeconds(tempoParaLiberar);

        Vector3 posicaoSaida; Quaternion rotacaoSaida;

        if (saidaRefri != null) { posicaoSaida = saidaRefri.position; rotacaoSaida = saidaRefri.rotation; } else { posicaoSaida = transform.position + transform.forward; rotacaoSaida = Quaternion.identity; }

        GameObject refriCriado = Instantiate(prefabRefri, posicaoSaida, rotacaoSaida);

        Rigidbody corpoRefri = refriCriado.GetComponent<Rigidbody>();

        if (corpoRefri != null) { corpoRefri.isKinematic = false; corpoRefri.useGravity = true; }

        Debug.Log("A máquina liberou o refrigerante!");

        processando = false;
    }
}